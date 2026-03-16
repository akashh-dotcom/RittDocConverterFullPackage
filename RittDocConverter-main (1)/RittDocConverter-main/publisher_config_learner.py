#!/usr/bin/env python3
"""
Publisher Configuration Learner
================================

Learns CSS class patterns from EPUBs during conversion and generates
suggested publisher configurations. This enables automatic discovery
of publisher-specific patterns and helps maintain configuration files.

Features:
- Tracks all CSS classes encountered during conversion
- Records mapping decisions (which DocBook element was used)
- Identifies unrecognized/unmapped classes
- Generates suggested YAML configurations
- Provides analysis reports for manual review

Usage:
    from publisher_config_learner import (
        get_learner, reset_learner,
        record_class_usage, record_epub_type_usage,
        generate_config_suggestion, get_unrecognized_report
    )

    # During conversion, record each class usage
    record_class_usage('ChapterTitle', 'div', 'title', 'chapter-title')

    # At end of conversion, get reports
    report = get_unrecognized_report()
    suggestion = generate_config_suggestion('Springer')
"""

import json
import logging
import re
from collections import defaultdict
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple, Any

logger = logging.getLogger(__name__)

# ============================================================================
# DATA STRUCTURES
# ============================================================================

@dataclass
class ClassUsage:
    """Records usage of a CSS class during conversion."""
    css_class: str
    html_tag: str  # Original HTML tag (div, span, p, etc.)
    docbook_element: Optional[str] = None  # What it was mapped to
    docbook_role: Optional[str] = None  # Role attribute if any
    count: int = 0  # How many times encountered
    contexts: Set[str] = field(default_factory=set)  # Parent contexts
    matched_by: str = "unmapped"  # "config", "docbook_builder", "hardcoded", "unmapped"
    sample_content: Optional[str] = None  # Sample of content for analysis


@dataclass
class EpubTypeUsage:
    """Records usage of epub:type values during conversion."""
    epub_type: str
    docbook_element: Optional[str] = None
    count: int = 0
    matched_by: str = "unmapped"


@dataclass
class LearnerStats:
    """Statistics from the learning process."""
    total_classes_seen: int = 0
    unique_classes: int = 0
    mapped_classes: int = 0
    unmapped_classes: int = 0
    total_epub_types_seen: int = 0
    unique_epub_types: int = 0
    mapped_epub_types: int = 0
    unmapped_epub_types: int = 0


# ============================================================================
# PATTERN ANALYSIS HELPERS
# ============================================================================

# Common patterns that suggest DocBook element types
CLASS_PATTERN_HINTS = {
    # Title patterns
    r'(?i)(chapter|section|sect|heading|head|title|h[1-6])': ('title', 'bridgehead'),
    r'(?i)(subtitle|sub-title|subhead)': ('subtitle',),

    # Paragraph patterns
    r'(?i)(para|paragraph|body|text|content|normal)': ('para',),
    r'(?i)(indent|noindent|first|continue)': ('para',),

    # List patterns
    r'(?i)(bullet|unordered|itemized)': ('itemizedlist',),
    r'(?i)(number|ordered|enum)': ('orderedlist',),
    r'(?i)(definition|glossary|variablelist)': ('variablelist',),

    # Figure patterns
    r'(?i)(figure|fig|image|illustration|photo)': ('figure',),
    r'(?i)(caption|figcaption|figurecaption)': ('title',),  # In figure context

    # Table patterns
    r'(?i)(table|tbl)': ('table',),
    r'(?i)(tablecaption|table-caption|tabletitle)': ('title',),  # In table context

    # Bibliography patterns
    r'(?i)(bibliography|biblio|reference|citation|works-cited)': ('bibliography', 'bibliomixed'),

    # Footnote patterns
    r'(?i)(footnote|fn|endnote|note)': ('footnote',),

    # Sidebar/Box patterns
    r'(?i)(sidebar|aside|box|callout|infobox)': ('sidebar',),
    r'(?i)(tip|warning|caution|important|note)': ('tip', 'warning', 'caution', 'important', 'note'),
    r'(?i)(example|exercise|activity)': ('example', 'sidebar'),

    # Quote patterns
    r'(?i)(quote|blockquote|pullquote|epigraph|extract)': ('blockquote', 'epigraph'),

    # Emphasis patterns
    r'(?i)(bold|strong|emphasis|italic|underline)': ('emphasis',),
    r'(?i)(smallcaps|small-caps|allcaps)': ('emphasis',),
    r'(?i)(superscript|sup|subscript|sub)': ('superscript', 'subscript'),

    # Code patterns
    r'(?i)(code|programlisting|literal|monospace|pre)': ('programlisting', 'literal'),

    # Front/Back matter
    r'(?i)(frontmatter|front-matter|preface|foreword|introduction)': ('preface',),
    r'(?i)(backmatter|back-matter|appendix|afterword)': ('appendix',),
    r'(?i)(dedication|acknowledgment|colophon)': ('dedication', 'acknowledgments', 'colophon'),
    r'(?i)(index|glossary|toc|contents)': ('index', 'glossary', 'toc'),

    # Author patterns
    r'(?i)(author|byline|contributor|attribution)': ('author',),

    # Skip patterns (elements that shouldn't produce output)
    r'(?i)(pagebreak|page-break|pagenum|page-number)': (None,),  # Skip
    r'(?i)(running-head|header|footer|nav)': (None,),  # Skip
}


def _analyze_class_name(class_name: str) -> Tuple[Optional[str], Optional[str], float]:
    """
    Analyze a class name and suggest a DocBook element.

    Returns:
        Tuple of (suggested_element, suggested_role, confidence)
        Confidence is 0.0-1.0
    """
    class_lower = class_name.lower()

    for pattern, elements in CLASS_PATTERN_HINTS.items():
        if re.search(pattern, class_name):
            # Return first element as suggestion
            element = elements[0] if elements else None

            # Determine role based on more specific patterns
            role = None
            if element == 'para' and 'indent' in class_lower:
                role = 'indent'
            elif element == 'para' and 'noindent' in class_lower:
                role = 'noindent'
            elif element == 'emphasis':
                if 'bold' in class_lower or 'strong' in class_lower:
                    role = 'bold'
                elif 'italic' in class_lower:
                    role = 'italic'
                elif 'smallcaps' in class_lower or 'small-caps' in class_lower:
                    role = 'smallcaps'

            # Confidence based on pattern specificity
            confidence = 0.7 if len(class_name) > 5 else 0.5

            return element, role, confidence

    return None, None, 0.0


# ============================================================================
# LEARNER CLASS
# ============================================================================

class PublisherConfigLearner:
    """
    Learns CSS class and epub:type patterns during EPUB conversion.
    """

    def __init__(self):
        self.reset()

    def reset(self):
        """Reset all learned data."""
        self.class_usages: Dict[str, ClassUsage] = {}
        self.epub_type_usages: Dict[str, EpubTypeUsage] = {}
        self.publisher_name: Optional[str] = None
        self.epub_files_processed: int = 0
        self.conversion_start_time: Optional[datetime] = None
        self._unrecognized_classes: Set[str] = set()
        self._unrecognized_epub_types: Set[str] = set()

    def start_conversion(self, publisher_name: str = "Unknown"):
        """Called at the start of a conversion."""
        self.publisher_name = publisher_name
        self.conversion_start_time = datetime.now()
        logger.debug(f"Learner started for publisher: {publisher_name}")

    def record_class(self, css_class: str, html_tag: str,
                     docbook_element: Optional[str] = None,
                     docbook_role: Optional[str] = None,
                     matched_by: str = "unmapped",
                     context: str = "",
                     sample_content: str = "") -> None:
        """
        Record usage of a CSS class.

        Args:
            css_class: The CSS class name
            html_tag: Original HTML tag
            docbook_element: DocBook element it was mapped to (or None)
            docbook_role: Role attribute if any
            matched_by: How it was matched ("config", "docbook_builder", "hardcoded", "unmapped")
            context: Parent element context
            sample_content: Sample of the element's content
        """
        if not css_class:
            return

        # Normalize class name (preserve case for analysis)
        key = css_class.strip()

        if key not in self.class_usages:
            self.class_usages[key] = ClassUsage(
                css_class=key,
                html_tag=html_tag,
                docbook_element=docbook_element,
                docbook_role=docbook_role,
                matched_by=matched_by
            )

        usage = self.class_usages[key]
        usage.count += 1
        if context:
            usage.contexts.add(context)
        if sample_content and not usage.sample_content:
            # Keep first sample (truncated)
            usage.sample_content = sample_content[:200]

        # Update mapping info if we now have it
        if docbook_element and not usage.docbook_element:
            usage.docbook_element = docbook_element
            usage.docbook_role = docbook_role
            usage.matched_by = matched_by

        # Track unrecognized
        if matched_by == "unmapped":
            self._unrecognized_classes.add(key)
        elif key in self._unrecognized_classes:
            self._unrecognized_classes.discard(key)

    def record_epub_type(self, epub_type: str,
                         docbook_element: Optional[str] = None,
                         matched_by: str = "unmapped") -> None:
        """Record usage of an epub:type value."""
        if not epub_type:
            return

        key = epub_type.strip().lower()

        if key not in self.epub_type_usages:
            self.epub_type_usages[key] = EpubTypeUsage(
                epub_type=key,
                docbook_element=docbook_element,
                matched_by=matched_by
            )

        usage = self.epub_type_usages[key]
        usage.count += 1

        if docbook_element and not usage.docbook_element:
            usage.docbook_element = docbook_element
            usage.matched_by = matched_by

        if matched_by == "unmapped":
            self._unrecognized_epub_types.add(key)
        elif key in self._unrecognized_epub_types:
            self._unrecognized_epub_types.discard(key)

    def get_stats(self) -> LearnerStats:
        """Get learning statistics."""
        mapped_classes = sum(1 for u in self.class_usages.values() if u.matched_by != "unmapped")
        mapped_epub = sum(1 for u in self.epub_type_usages.values() if u.matched_by != "unmapped")

        return LearnerStats(
            total_classes_seen=sum(u.count for u in self.class_usages.values()),
            unique_classes=len(self.class_usages),
            mapped_classes=mapped_classes,
            unmapped_classes=len(self.class_usages) - mapped_classes,
            total_epub_types_seen=sum(u.count for u in self.epub_type_usages.values()),
            unique_epub_types=len(self.epub_type_usages),
            mapped_epub_types=mapped_epub,
            unmapped_epub_types=len(self.epub_type_usages) - mapped_epub
        )

    def get_unrecognized_classes(self) -> List[ClassUsage]:
        """Get list of unrecognized CSS classes sorted by frequency."""
        unrecognized = [
            u for u in self.class_usages.values()
            if u.matched_by == "unmapped"
        ]
        return sorted(unrecognized, key=lambda x: x.count, reverse=True)

    def get_unrecognized_epub_types(self) -> List[EpubTypeUsage]:
        """Get list of unrecognized epub:type values sorted by frequency."""
        unrecognized = [
            u for u in self.epub_type_usages.values()
            if u.matched_by == "unmapped"
        ]
        return sorted(unrecognized, key=lambda x: x.count, reverse=True)

    def generate_config_suggestion(self) -> Dict[str, Any]:
        """
        Generate a suggested publisher configuration based on learned patterns.

        Returns:
            Dictionary in publisher config format
        """
        config = {
            'publisher_name': self.publisher_name or 'Unknown',
            'publisher_patterns': [self.publisher_name] if self.publisher_name else [],
            'css_class_mappings': {},
            'epub_type_mappings': {},
            '_metadata': {
                'generated_at': datetime.now().isoformat(),
                'total_classes_analyzed': len(self.class_usages),
                'total_epub_types_analyzed': len(self.epub_type_usages),
            }
        }

        # Generate CSS class mappings
        for class_name, usage in sorted(self.class_usages.items()):
            if usage.matched_by == "unmapped":
                # Try to suggest a mapping
                element, role, confidence = _analyze_class_name(class_name)
                if element and confidence >= 0.5:
                    mapping = {'element': element}
                    if role:
                        mapping['role'] = role
                    mapping['_suggested'] = True
                    mapping['_confidence'] = confidence
                    mapping['_count'] = usage.count
                    config['css_class_mappings'][class_name] = mapping
            else:
                # Record what it was actually mapped to
                if usage.docbook_element:
                    mapping = {'element': usage.docbook_element}
                    if usage.docbook_role:
                        mapping['role'] = usage.docbook_role
                    mapping['_learned'] = True
                    mapping['_count'] = usage.count
                    mapping['_matched_by'] = usage.matched_by
                    config['css_class_mappings'][class_name] = mapping

        # Generate epub:type mappings
        for epub_type, usage in sorted(self.epub_type_usages.items()):
            if usage.docbook_element:
                config['epub_type_mappings'][epub_type] = usage.docbook_element

        return config

    def generate_yaml_config(self) -> str:
        """Generate YAML formatted configuration."""
        config = self.generate_config_suggestion()

        # Remove internal metadata from output
        css_mappings = {}
        for k, v in config['css_class_mappings'].items():
            clean_v = {kk: vv for kk, vv in v.items() if not kk.startswith('_')}
            if clean_v:
                css_mappings[k] = clean_v

        lines = [
            f"# Publisher Configuration for {config['publisher_name']}",
            f"# Auto-generated on {config['_metadata']['generated_at']}",
            f"# Classes analyzed: {config['_metadata']['total_classes_analyzed']}",
            "",
            f"publisher_name: \"{config['publisher_name']}\"",
            "publisher_patterns:",
        ]

        for pattern in config['publisher_patterns']:
            lines.append(f"  - \"{pattern}\"")

        lines.extend(["", "css_class_mappings:"])

        for class_name, mapping in sorted(css_mappings.items()):
            element = mapping.get('element', '')
            role = mapping.get('role')
            if role:
                lines.append(f"  {class_name}:")
                lines.append(f"    element: {element}")
                lines.append(f"    role: {role}")
            else:
                lines.append(f"  {class_name}:")
                lines.append(f"    element: {element}")

        if config['epub_type_mappings']:
            lines.extend(["", "epub_type_mappings:"])
            for epub_type, element in sorted(config['epub_type_mappings'].items()):
                lines.append(f"  {epub_type}: {element}")

        return '\n'.join(lines)

    def get_unrecognized_report(self) -> str:
        """Generate a report of unrecognized classes for manual review."""
        lines = [
            "=" * 70,
            "UNRECOGNIZED CSS CLASSES AND EPUB:TYPES",
            f"Publisher: {self.publisher_name or 'Unknown'}",
            "=" * 70,
            "",
        ]

        unrecognized_classes = self.get_unrecognized_classes()
        if unrecognized_classes:
            lines.append("UNRECOGNIZED CSS CLASSES (sorted by frequency):")
            lines.append("-" * 50)
            for usage in unrecognized_classes[:50]:  # Top 50
                element, role, confidence = _analyze_class_name(usage.css_class)
                suggestion = ""
                if element:
                    suggestion = f" -> SUGGEST: {element}"
                    if role:
                        suggestion += f" (role={role})"
                    suggestion += f" [{confidence:.0%} confidence]"

                lines.append(f"  {usage.css_class:40} (count: {usage.count:4}, tag: {usage.html_tag}){suggestion}")

            if len(unrecognized_classes) > 50:
                lines.append(f"  ... and {len(unrecognized_classes) - 50} more")
        else:
            lines.append("No unrecognized CSS classes!")

        lines.append("")

        unrecognized_epub = self.get_unrecognized_epub_types()
        if unrecognized_epub:
            lines.append("UNRECOGNIZED EPUB:TYPES (sorted by frequency):")
            lines.append("-" * 50)
            for usage in unrecognized_epub[:30]:
                lines.append(f"  {usage.epub_type:40} (count: {usage.count})")

            if len(unrecognized_epub) > 30:
                lines.append(f"  ... and {len(unrecognized_epub) - 30} more")
        else:
            lines.append("No unrecognized epub:types!")

        lines.extend(["", "=" * 70])

        return '\n'.join(lines)

    def save_learned_config(self, output_path: Path) -> None:
        """Save the suggested configuration to a YAML file."""
        yaml_content = self.generate_yaml_config()
        output_path.write_text(yaml_content, encoding='utf-8')
        logger.info(f"Saved suggested config to {output_path}")

    def save_analysis_report(self, output_path: Path) -> None:
        """Save detailed analysis to a JSON file."""
        report = {
            'publisher_name': self.publisher_name,
            'generated_at': datetime.now().isoformat(),
            'stats': {
                'unique_classes': len(self.class_usages),
                'unmapped_classes': len(self._unrecognized_classes),
                'unique_epub_types': len(self.epub_type_usages),
                'unmapped_epub_types': len(self._unrecognized_epub_types),
            },
            'css_classes': {
                k: {
                    'count': v.count,
                    'html_tag': v.html_tag,
                    'docbook_element': v.docbook_element,
                    'docbook_role': v.docbook_role,
                    'matched_by': v.matched_by,
                    'contexts': list(v.contexts)[:5],
                    'sample': v.sample_content,
                }
                for k, v in sorted(self.class_usages.items(), key=lambda x: x[1].count, reverse=True)
            },
            'epub_types': {
                k: {
                    'count': v.count,
                    'docbook_element': v.docbook_element,
                    'matched_by': v.matched_by,
                }
                for k, v in sorted(self.epub_type_usages.items(), key=lambda x: x[1].count, reverse=True)
            }
        }

        output_path.write_text(json.dumps(report, indent=2), encoding='utf-8')
        logger.info(f"Saved analysis report to {output_path}")


# ============================================================================
# MODULE-LEVEL SINGLETON
# ============================================================================

_learner: Optional[PublisherConfigLearner] = None


def get_learner() -> PublisherConfigLearner:
    """Get the global learner instance."""
    global _learner
    if _learner is None:
        _learner = PublisherConfigLearner()
    return _learner


def reset_learner() -> None:
    """Reset the global learner."""
    global _learner
    if _learner is not None:
        _learner.reset()
    else:
        _learner = PublisherConfigLearner()


def start_learning(publisher_name: str = "Unknown") -> None:
    """Start learning for a new conversion."""
    learner = get_learner()
    learner.reset()
    learner.start_conversion(publisher_name)


def record_class_usage(css_class: str, html_tag: str,
                       docbook_element: Optional[str] = None,
                       docbook_role: Optional[str] = None,
                       matched_by: str = "unmapped",
                       context: str = "",
                       sample_content: str = "") -> None:
    """Record a CSS class usage during conversion."""
    get_learner().record_class(
        css_class, html_tag, docbook_element, docbook_role,
        matched_by, context, sample_content
    )


def record_epub_type_usage(epub_type: str,
                           docbook_element: Optional[str] = None,
                           matched_by: str = "unmapped") -> None:
    """Record an epub:type usage during conversion."""
    get_learner().record_epub_type(epub_type, docbook_element, matched_by)


def get_learning_stats() -> LearnerStats:
    """Get current learning statistics."""
    return get_learner().get_stats()


def get_unrecognized_report() -> str:
    """Get report of unrecognized patterns."""
    return get_learner().get_unrecognized_report()


def generate_config_suggestion() -> Dict[str, Any]:
    """Generate suggested configuration dictionary."""
    return get_learner().generate_config_suggestion()


def generate_yaml_suggestion() -> str:
    """Generate suggested configuration as YAML string."""
    return get_learner().generate_yaml_config()


def save_suggested_config(output_dir: Path, publisher_name: str = "") -> Tuple[Path, Path]:
    """
    Save suggested config and analysis report.

    Args:
        output_dir: Directory to save files
        publisher_name: Publisher name for filename

    Returns:
        Tuple of (config_path, report_path)
    """
    learner = get_learner()

    # Sanitize publisher name for filename
    safe_name = re.sub(r'[^\w\-]', '_', publisher_name or learner.publisher_name or 'unknown').lower()

    config_path = output_dir / f"{safe_name}_suggested.yaml"
    report_path = output_dir / f"{safe_name}_analysis.json"

    learner.save_learned_config(config_path)
    learner.save_analysis_report(report_path)

    return config_path, report_path


def log_learning_summary() -> None:
    """Log a summary of what was learned."""
    learner = get_learner()
    stats = learner.get_stats()

    logger.info("=" * 60)
    logger.info("CSS CLASS LEARNING SUMMARY")
    logger.info("=" * 60)
    logger.info(f"Publisher: {learner.publisher_name}")
    logger.info(f"Unique CSS classes seen: {stats.unique_classes}")
    logger.info(f"  - Mapped: {stats.mapped_classes}")
    logger.info(f"  - Unmapped: {stats.unmapped_classes}")
    logger.info(f"Unique epub:types seen: {stats.unique_epub_types}")
    logger.info(f"  - Mapped: {stats.mapped_epub_types}")
    logger.info(f"  - Unmapped: {stats.unmapped_epub_types}")

    # Log top unrecognized classes
    unrecognized = learner.get_unrecognized_classes()[:10]
    if unrecognized:
        logger.info("")
        logger.info("Top unrecognized CSS classes:")
        for usage in unrecognized:
            element, role, conf = _analyze_class_name(usage.css_class)
            suggestion = f" -> suggest {element}" if element else ""
            logger.info(f"  {usage.css_class}: {usage.count} occurrences{suggestion}")

    logger.info("=" * 60)
