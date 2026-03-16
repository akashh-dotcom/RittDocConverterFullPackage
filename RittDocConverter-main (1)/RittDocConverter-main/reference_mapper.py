"""
Reference Mapping System for RittDocConverter

Tracks all resource renaming (images, files) throughout the conversion pipeline
to ensure proper reference resolution and validation.

This module provides:
- Persistent mapping of original → intermediate → final resource names
- Reference validation across chapter XMLs
- Export/import of mapping data for debugging and validation
"""

import json
import logging
import posixpath
import re
from collections import defaultdict
from dataclasses import asdict, dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional, Set, Tuple
from urllib.parse import unquote, urlparse

logger = logging.getLogger(__name__)


def normalize_href(href: str, base_file: Optional[str] = None) -> str:
    """
    Normalize an href to a canonical form for consistent lookup.

    This function handles:
    - URL decoding (%20 -> space, etc.)
    - Relative path resolution (../chapter02.xhtml -> chapter02.xhtml)
    - Case normalization for file extensions
    - Removal of leading ./ and redundant path components

    The canonical form is: {basename}#{fragment} or just {basename}

    Args:
        href: The href to normalize (e.g., "../chapter02.xhtml#fig01")
        base_file: The file containing the href, for relative resolution

    Returns:
        Normalized href in canonical form (e.g., "chapter02.xhtml#fig01")

    Examples:
        >>> normalize_href("chapter02.xhtml#fig01")
        'chapter02.xhtml#fig01'
        >>> normalize_href("../text/chapter02.xhtml#fig01")
        'chapter02.xhtml#fig01'
        >>> normalize_href("./chapter02.xhtml#fig01")
        'chapter02.xhtml#fig01'
        >>> normalize_href("Chapter02.XHTML#fig01")
        'chapter02.xhtml#fig01'
        >>> normalize_href("chapter%2002.xhtml#fig01")
        'chapter 02.xhtml#fig01'
    """
    if not href:
        return href

    # URL decode
    href = unquote(href)

    # Split into path and fragment
    if '#' in href:
        path_part, fragment = href.rsplit('#', 1)
    else:
        path_part = href
        fragment = None

    # Skip external URLs
    if path_part.startswith(('http://', 'https://', 'mailto:', 'ftp://')):
        return href

    # Handle relative paths
    if base_file and (path_part.startswith('../') or path_part.startswith('./')):
        # Resolve relative to base file's directory
        base_dir = posixpath.dirname(base_file)
        path_part = posixpath.normpath(posixpath.join(base_dir, path_part))

    # Extract just the filename (canonical form uses basename only)
    # This handles paths like "text/chapter02.xhtml" -> "chapter02.xhtml"
    basename = posixpath.basename(path_part)

    # Normalize extension case (common variations: .XHTML, .Xhtml, .HTML)
    if '.' in basename:
        name, ext = basename.rsplit('.', 1)
        basename = f"{name}.{ext.lower()}"

    # Reconstruct with fragment if present
    if fragment:
        return f"{basename}#{fragment}"
    return basename


def normalize_anchor_id(anchor_id: str) -> str:
    """
    Normalize an anchor ID for consistent lookup.

    This handles:
    - URL decoding
    - Whitespace trimming

    Args:
        anchor_id: The anchor ID to normalize

    Returns:
        Normalized anchor ID
    """
    if not anchor_id:
        return anchor_id

    # URL decode
    anchor_id = unquote(anchor_id)

    # Trim whitespace
    anchor_id = anchor_id.strip()

    return anchor_id


def extract_file_and_anchor(href: str) -> Tuple[Optional[str], Optional[str]]:
    """
    Extract the file basename and anchor from an href.

    Args:
        href: The href to parse (already normalized preferred)

    Returns:
        Tuple of (file_basename, anchor_id), either can be None
    """
    if not href:
        return None, None

    if href.startswith('#'):
        # Same-file anchor reference
        return None, href[1:] if len(href) > 1 else None

    if '#' in href:
        file_part, anchor = href.rsplit('#', 1)
        basename = posixpath.basename(file_part) if file_part else None
        return basename, anchor if anchor else None

    # No anchor, just file reference
    basename = posixpath.basename(href)
    return basename, None


@dataclass
class ResourceReference:
    """Tracks a single resource through all renaming stages"""
    original_path: str          # Original path in source (e.g., "OEBPS/images/fig1.png")
    original_filename: str      # Original filename only (e.g., "fig1.png")
    intermediate_name: str      # Temporary name during extraction (e.g., "img_0001.png")
    final_name: Optional[str] = None  # Final name after packaging (e.g., "Ch0001f01.jpg")

    # Context information
    resource_type: str = "image"  # "image", "link", "xhtml", etc.
    first_seen_in: Optional[str] = None  # Chapter/file where first referenced
    referenced_in: List[str] = field(default_factory=list)  # All chapters referencing this

    # Image-specific metadata
    is_vector: bool = False
    is_raster: bool = False
    width: Optional[int] = None
    height: Optional[int] = None
    file_size: Optional[int] = None

    # Validation
    exists_in_source: bool = True
    exists_in_output: bool = False
    all_references_updated: bool = False

    def to_dict(self) -> dict:
        """Convert to dictionary for JSON serialization"""
        return asdict(self)

    @classmethod
    def from_dict(cls, data: dict) -> 'ResourceReference':
        """Create from dictionary"""
        return cls(**data)


@dataclass
class LinkReference:
    """Tracks internal document links"""
    original_href: str          # Original link (e.g., "chapter02.xhtml#section1")
    source_chapter: str         # Chapter containing the link (e.g., "ch0001")
    target_chapter: Optional[str] = None  # Target chapter (e.g., "ch0002")
    target_anchor: Optional[str] = None   # Target anchor (e.g., "section1")
    resolved: bool = False

    def to_dict(self) -> dict:
        return asdict(self)

    @classmethod
    def from_dict(cls, data: dict) -> 'LinkReference':
        return cls(**data)


class ReferenceMapper:
    """
    Central reference mapping system for tracking all resource transformations
    throughout the conversion pipeline.
    """

    def __init__(self):
        self.resources: Dict[str, ResourceReference] = {}  # Key: original_path
        self.links: List[LinkReference] = []
        self.chapter_map: Dict[str, str] = {}  # original_file → chapter_id

        # Statistics
        self.stats = {
            'total_images': 0,
            'vector_images': 0,
            'raster_images': 0,
            'total_links': 0,
            'broken_links': 0,
            'unreferenced_resources': 0,
            'skipped_svg_no_converter': 0,  # SVGs skipped due to missing cairosvg
        }

        # Track skipped/lost resources for reporting
        self.skipped_resources: List[Dict[str, str]] = []

    def add_resource(self,
                     original_path: str,
                     intermediate_name: str,
                     resource_type: str = "image",
                     first_seen_in: Optional[str] = None,
                     **kwargs) -> ResourceReference:
        """
        Register a resource in the mapping system.

        Args:
            original_path: Original path in source document
            intermediate_name: Temporary name during extraction
            resource_type: Type of resource (image, xhtml, etc.)
            first_seen_in: Chapter/file where first encountered
            **kwargs: Additional metadata (is_vector, width, height, etc.)

        Returns:
            ResourceReference object
        """
        original_filename = Path(original_path).name

        ref = ResourceReference(
            original_path=original_path,
            original_filename=original_filename,
            intermediate_name=intermediate_name,
            resource_type=resource_type,
            first_seen_in=first_seen_in,
            referenced_in=[first_seen_in] if first_seen_in else [],
            **kwargs
        )

        self.resources[original_path] = ref

        if resource_type == "image":
            self.stats['total_images'] += 1
            if kwargs.get('is_vector'):
                self.stats['vector_images'] += 1
            if kwargs.get('is_raster'):
                self.stats['raster_images'] += 1

        logger.debug(f"Registered resource: {original_path} → {intermediate_name}")
        return ref

    def update_final_name(self, original_path: str, final_name: str) -> None:
        """Update the final name for a resource after packaging"""
        if original_path in self.resources:
            self.resources[original_path].final_name = final_name
            logger.debug(f"Updated final name: {original_path} → {final_name}")
        else:
            logger.warning(f"Attempted to update final name for unknown resource: {original_path}")

    def add_reference(self, original_path: str, referenced_in: str) -> None:
        """Record that a resource is referenced in a specific chapter"""
        if original_path in self.resources:
            if referenced_in not in self.resources[original_path].referenced_in:
                self.resources[original_path].referenced_in.append(referenced_in)
        else:
            logger.warning(f"Reference to unknown resource: {original_path} in {referenced_in}")

    def add_skipped_resource(self,
                             original_path: str,
                             reason: str,
                             resource_type: str = "image") -> None:
        """
        Record a resource that was skipped/lost during conversion.

        Args:
            original_path: Original path of the skipped resource
            reason: Why the resource was skipped
            resource_type: Type of resource (image, svg, etc.)
        """
        self.skipped_resources.append({
            'original_path': original_path,
            'reason': reason,
            'resource_type': resource_type
        })

        # Update specific stats based on reason
        if 'cairosvg' in reason.lower() or 'svg' in reason.lower():
            self.stats['skipped_svg_no_converter'] += 1

        logger.error(f"Resource LOST: {original_path} - {reason}")

    def add_link(self,
                 original_href: str,
                 source_chapter: str,
                 target_chapter: Optional[str] = None,
                 target_anchor: Optional[str] = None) -> LinkReference:
        """
        Register an internal link.

        The original_href is stored as-is for debugging, but lookups
        use normalized form for consistency.

        Args:
            original_href: Original href from HTML (stored as-is)
            source_chapter: Chapter containing the link
            target_chapter: Target chapter if known
            target_anchor: Target anchor if known

        Returns:
            LinkReference object
        """
        # Normalize the anchor if provided
        if target_anchor:
            target_anchor = normalize_anchor_id(target_anchor)

        link = LinkReference(
            original_href=original_href,
            source_chapter=source_chapter,
            target_chapter=target_chapter,
            target_anchor=target_anchor,
            resolved=target_chapter is not None
        )
        self.links.append(link)
        self.stats['total_links'] += 1

        if not link.resolved:
            self.stats['broken_links'] += 1

        return link

    def resolve_href(self, href: str, source_file: Optional[str] = None) -> Tuple[Optional[str], Optional[str]]:
        """
        Resolve an href to (chapter_id, anchor_id).

        This is the main entry point for link resolution during conversion.
        Uses normalized forms for consistent matching.

        Args:
            href: The href to resolve (e.g., "chapter02.xhtml#fig01" or "#fig01")
            source_file: The file containing the href, for relative path resolution

        Returns:
            Tuple of (chapter_id, anchor_id), either can be None
        """
        if not href:
            return None, None

        # Normalize the href
        normalized = normalize_href(href, source_file)

        # Extract file and anchor parts
        file_part, anchor_part = extract_file_and_anchor(normalized)

        # Look up chapter ID
        chapter_id = None
        if file_part:
            chapter_id = self.get_chapter_id(file_part)

        # Normalize anchor if present
        if anchor_part:
            anchor_part = normalize_anchor_id(anchor_part)

        return chapter_id, anchor_part

    def register_chapter(self, original_file: str, chapter_id: str) -> None:
        """
        Map original XHTML file to chapter ID.

        Both the original file path and normalized basename are registered
        to support lookups from different reference styles.

        Args:
            original_file: Original XHTML file path (e.g., "OEBPS/text/chapter02.xhtml")
            chapter_id: Chapter ID (e.g., "ch0002")
        """
        # Store with original path
        self.chapter_map[original_file] = chapter_id

        # Also store with normalized basename for flexible lookups
        normalized = normalize_href(original_file)
        if normalized != original_file:
            self.chapter_map[normalized] = chapter_id

        logger.debug(f"Registered chapter mapping: {original_file} → {chapter_id} (normalized: {normalized})")

    def get_final_name(self, original_path: str) -> Optional[str]:
        """Get the final name for a resource"""
        if original_path in self.resources:
            return self.resources[original_path].final_name
        return None

    def get_intermediate_name(self, original_path: str) -> Optional[str]:
        """Get the intermediate name for a resource"""
        if original_path in self.resources:
            return self.resources[original_path].intermediate_name
        return None

    def get_chapter_id(self, original_file: str) -> Optional[str]:
        """
        Get chapter ID for an original XHTML file.

        Tries multiple lookup strategies:
        1. Exact match on original_file
        2. Normalized basename lookup
        3. Case-insensitive basename match (fallback)

        Args:
            original_file: File reference (can be path or basename)

        Returns:
            Chapter ID or None if not found
        """
        # Try exact match first
        result = self.chapter_map.get(original_file)
        if result:
            return result

        # Try normalized basename
        normalized = normalize_href(original_file)
        result = self.chapter_map.get(normalized)
        if result:
            return result

        # Fallback: case-insensitive basename match
        normalized_lower = normalized.lower()
        for key, chapter_id in self.chapter_map.items():
            if normalize_href(key).lower() == normalized_lower:
                return chapter_id

        return None

    def mark_link_resolved(self, original_href: str, source_chapter: str, target_chapter: str) -> bool:
        """
        Mark a link as resolved after post-processing.

        Tries both exact match and normalized match on original_href.

        Args:
            original_href: The original href that was tracked
            source_chapter: The source chapter where the link exists
            target_chapter: The resolved target chapter

        Returns:
            True if link was found and updated, False otherwise
        """
        normalized_href = normalize_href(original_href)

        for link in self.links:
            # Try exact match or normalized match
            href_matches = (
                link.original_href == original_href or
                normalize_href(link.original_href) == normalized_href
            )
            if href_matches and link.source_chapter == source_chapter:
                if not link.resolved:
                    link.resolved = True
                    link.target_chapter = target_chapter
                    self.stats['broken_links'] = max(0, self.stats['broken_links'] - 1)
                    return True
                return True  # Already resolved
        return False

    def recalculate_link_stats(self) -> None:
        """Recalculate link statistics based on current state"""
        broken = sum(1 for link in self.links if not link.resolved)
        self.stats['broken_links'] = broken
        logger.debug(f"Recalculated link stats: {broken} broken links out of {len(self.links)} total")

    def validate(self, output_dir: Path) -> Tuple[bool, List[str]]:
        """
        Validate that all resources exist and all references are resolvable.

        Returns:
            (is_valid, list of error messages)
        """
        errors = []

        # Check that all resources have final names
        for path, ref in self.resources.items():
            if ref.final_name is None:
                errors.append(f"Resource has no final name: {path}")
            else:
                # Check if final file exists
                final_path = output_dir / "MultiMedia" / ref.final_name
                if final_path.exists():
                    ref.exists_in_output = True
                else:
                    ref.exists_in_output = False
                    errors.append(f"Final resource not found: {final_path}")

        # Check for unreferenced resources
        for path, ref in self.resources.items():
            if not ref.referenced_in:
                self.stats['unreferenced_resources'] += 1
                logger.warning(f"Unreferenced resource: {path}")

        # Check links
        for link in self.links:
            if not link.resolved:
                errors.append(f"Unresolved link: {link.original_href} in {link.source_chapter}")

        is_valid = len(errors) == 0
        return is_valid, errors

    def export_to_json(self, output_path: Path) -> None:
        """Export complete mapping to JSON for debugging and validation"""
        data = {
            'metadata': {
                'created': datetime.now().isoformat(),
                'total_resources': len(self.resources),
                'total_links': len(self.links),
                'skipped_resources': len(self.skipped_resources),
            },
            'resources': {path: ref.to_dict() for path, ref in self.resources.items()},
            'links': [link.to_dict() for link in self.links],
            'chapter_map': self.chapter_map,
            'statistics': self.stats,
            'skipped_resources': self.skipped_resources,
        }

        with open(output_path, 'w', encoding='utf-8') as f:
            json.dump(data, f, indent=2, ensure_ascii=False)

        logger.info(f"Exported reference mapping to {output_path}")

    def import_from_json(self, input_path: Path) -> None:
        """Import mapping from JSON"""
        with open(input_path, 'r', encoding='utf-8') as f:
            data = json.load(f)

        self.resources = {
            path: ResourceReference.from_dict(ref_data)
            for path, ref_data in data['resources'].items()
        }
        self.links = [LinkReference.from_dict(link_data) for link_data in data['links']]
        self.chapter_map = data['chapter_map']
        self.stats = data['statistics']
        # Backwards compatibility: older exports may not have skipped_resources
        self.skipped_resources = data.get('skipped_resources', [])

        logger.info(f"Imported reference mapping from {input_path}")

    def get_statistics(self) -> Dict:
        """Get current statistics"""
        return self.stats.copy()

    def generate_report(self) -> str:
        """Generate a human-readable report of the mapping state"""
        lines = [
            "=" * 80,
            "REFERENCE MAPPING REPORT",
            "=" * 80,
            f"Total Resources: {len(self.resources)}",
            f"  - Images: {self.stats['total_images']}",
            f"    - Vector: {self.stats['vector_images']}",
            f"    - Raster: {self.stats['raster_images']}",
            f"Total Links: {self.stats['total_links']}",
            f"  - Broken: {self.stats['broken_links']}",
            f"Unreferenced Resources: {self.stats['unreferenced_resources']}",
            f"Skipped SVGs (no converter): {self.stats.get('skipped_svg_no_converter', 0)}",
            "",
            "Chapter Mappings:",
        ]

        for original, chapter_id in sorted(self.chapter_map.items()):
            lines.append(f"  {original} → {chapter_id}")

        lines.extend([
            "",
            "Resource Mappings (first 10):",
        ])

        for i, (path, ref) in enumerate(list(self.resources.items())[:10]):
            lines.append(f"  {path}")
            lines.append(f"    → intermediate: {ref.intermediate_name}")
            lines.append(f"    → final: {ref.final_name or 'NOT SET'}")
            lines.append(f"    → referenced in: {', '.join(ref.referenced_in) or 'NONE'}")

        if len(self.resources) > 10:
            lines.append(f"  ... and {len(self.resources) - 10} more")

        # Report skipped/lost resources if any
        if self.skipped_resources:
            lines.extend([
                "",
                f"SKIPPED/LOST RESOURCES ({len(self.skipped_resources)}):",
            ])
            for skipped in self.skipped_resources:
                lines.append(f"  ✗ {skipped['original_path']}")
                lines.append(f"    Reason: {skipped['reason']}")
                lines.append(f"    Type: {skipped['resource_type']}")

        lines.append("=" * 80)
        return "\n".join(lines)


# Global mapper instance for use across pipeline
_global_mapper: Optional[ReferenceMapper] = None


def get_mapper() -> ReferenceMapper:
    """Get or create the global reference mapper instance"""
    global _global_mapper
    if _global_mapper is None:
        _global_mapper = ReferenceMapper()
    return _global_mapper


def reset_mapper() -> None:
    """Reset the global mapper (useful for testing or new conversions)"""
    global _global_mapper
    _global_mapper = ReferenceMapper()
