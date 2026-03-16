#!/usr/bin/env python3
"""
Lean XML Transformation Pipeline Tester

Tests RittDocConverter output through the LoadBook XSL transformation chain
WITHOUT requiring database connections, drug linking, or other slow external processes.

This tool helps catch XSL transformation errors early before running the full LoadBook.bat.

Usage:
    python pipeline_tester.py <input_zip_or_xml>
    python pipeline_tester.py --help

Transformation Pipeline:
    1. Pre-flight validation (author, ISBN, structure)
    2. AddRISInfo.xsl - Adds RIS metadata to sections
    3. Chunking - Splits book into sect1 files (Python implementation)
    4. TOC Transform - Generates TOC structure
    5. (Optional) RittBook.xsl - HTML rendering test
"""

import argparse
import json
import os
import re
import shutil
import sys
import tempfile
import time
import zipfile
from dataclasses import dataclass, field
from datetime import datetime
from pathlib import Path
from typing import Dict, List, Optional, Tuple

from lxml import etree

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent))
from xsl_runner import XSLRunner, TransformResult


@dataclass
class ValidationIssue:
    """Represents a validation issue found during pre-flight checks"""
    level: str  # 'error', 'warning', 'info'
    code: str
    message: str
    location: Optional[str] = None
    suggestion: Optional[str] = None


@dataclass
class PipelineResult:
    """Complete result of the pipeline test"""
    success: bool
    input_file: str
    timestamp: str
    duration_ms: float = 0.0
    stages: Dict[str, dict] = field(default_factory=dict)
    preflight_issues: List[ValidationIssue] = field(default_factory=list)
    summary: str = ""

    def to_dict(self) -> dict:
        return {
            'success': self.success,
            'input_file': self.input_file,
            'timestamp': self.timestamp,
            'duration_ms': self.duration_ms,
            'stages': self.stages,
            'preflight_issues': [
                {
                    'level': i.level,
                    'code': i.code,
                    'message': i.message,
                    'location': i.location,
                    'suggestion': i.suggestion
                }
                for i in self.preflight_issues
            ],
            'summary': self.summary
        }


class PipelineTester:
    """
    Lean XML transformation pipeline tester.

    Tests XML through the LoadBook XSL transformation chain without
    database operations or external dependencies.
    """

    def __init__(self, verbose: bool = False):
        self.verbose = verbose
        self.script_dir = Path(__file__).parent
        self.xsl_dir = self.script_dir / "xsl"
        self.output_dir = self.script_dir / "output"
        self.runner = XSLRunner(self.xsl_dir)

        # Ensure output directory exists
        self.output_dir.mkdir(parents=True, exist_ok=True)

    def log(self, message: str, level: str = "info"):
        """Log a message if verbose mode is enabled"""
        if self.verbose:
            prefix = {
                'info': '  ',
                'success': '✓ ',
                'warning': '⚠ ',
                'error': '✗ '
            }.get(level, '  ')
            print(f"{prefix}{message}")

    def extract_zip(self, zip_path: Path, extract_dir: Path) -> Path:
        """Extract ZIP file and return path to Book.XML"""
        self.log(f"Extracting {zip_path.name}...")

        with zipfile.ZipFile(zip_path, 'r') as zf:
            zf.extractall(extract_dir)

        # Find Book.XML (case-insensitive)
        for item in extract_dir.rglob("*"):
            if item.name.lower() == "book.xml":
                return item

        # Check for any XML file
        xml_files = list(extract_dir.glob("*.xml"))
        if xml_files:
            return xml_files[0]

        raise FileNotFoundError("No Book.XML or XML file found in ZIP")

    def preflight_checks(self, xml_path: Path) -> Tuple[etree._Element, List[ValidationIssue]]:
        """
        Run pre-flight validation checks on the input XML.

        Checks for:
        - Valid XML parsing
        - Required author/editor (fatal in AddRISInfo.xsl)
        - Valid ISBN
        - Required bookinfo elements
        - Chapter/section IDs
        """
        issues = []

        # Parse XML
        try:
            parser = etree.XMLParser(
                remove_blank_text=False,
                resolve_entities=False,
                load_dtd=False,
                no_network=True
            )
            tree = etree.parse(str(xml_path), parser)
            root = tree.getroot()
        except etree.XMLSyntaxError as e:
            issues.append(ValidationIssue(
                level='error',
                code='XML_PARSE_ERROR',
                message=f"Failed to parse XML: {e}",
                suggestion="Fix XML syntax errors before proceeding"
            ))
            return None, issues

        # Check for book element
        if root.tag != 'book':
            issues.append(ValidationIssue(
                level='error',
                code='INVALID_ROOT',
                message=f"Root element is '{root.tag}', expected 'book'",
                suggestion="Ensure the XML has a <book> root element"
            ))
            return root, issues

        # Check bookinfo
        bookinfo = root.find('.//bookinfo')
        if bookinfo is None:
            issues.append(ValidationIssue(
                level='error',
                code='MISSING_BOOKINFO',
                message="No <bookinfo> element found",
                suggestion="Add <bookinfo> with required metadata"
            ))
            return root, issues

        # Check author (CRITICAL - AddRISInfo.xsl terminates without this!)
        author_paths = [
            './/bookinfo/authorgroup/author',
            './/bookinfo/author',
            './/bookinfo/authorgroup/editor',
            './/bookinfo/editor',
            './/bookinfo/authorgroup/collab',
            './/bookinfo/collab'
        ]

        author_found = False
        for path in author_paths:
            if root.find(path) is not None:
                author_found = True
                break

        if not author_found:
            issues.append(ValidationIssue(
                level='error',
                code='MISSING_AUTHOR',
                message="No author, editor, or collab found in bookinfo",
                location="/book/bookinfo",
                suggestion="AddRISInfo.xsl will TERMINATE with 'No Author Found' - add <author> or <editor>"
            ))

        # Check ISBN
        isbn = bookinfo.find('isbn')
        if isbn is None or not isbn.text:
            issues.append(ValidationIssue(
                level='error',
                code='MISSING_ISBN',
                message="No ISBN found in bookinfo",
                location="/book/bookinfo/isbn",
                suggestion="Add <isbn> element with valid ISBN"
            ))
        else:
            # Validate ISBN format
            isbn_text = re.sub(r'[-\s]', '', isbn.text or '')
            if not re.match(r'^\d{10}(\d{3})?$', isbn_text):
                issues.append(ValidationIssue(
                    level='warning',
                    code='INVALID_ISBN',
                    message=f"ISBN '{isbn.text}' may be invalid",
                    location="/book/bookinfo/isbn",
                    suggestion="ISBN should be 10 or 13 digits"
                ))

        # Check title
        title = bookinfo.find('title')
        if title is None or not (title.text or len(title)):
            issues.append(ValidationIssue(
                level='error',
                code='MISSING_TITLE',
                message="No title found in bookinfo",
                location="/book/bookinfo/title",
                suggestion="Add <title> element"
            ))

        # Check for chapters/sections with IDs
        chapters = root.findall('.//chapter')
        if not chapters:
            # Check for parts containing chapters
            parts = root.findall('.//part')
            if parts:
                for part in parts:
                    chapters.extend(part.findall('.//chapter'))

        if not chapters:
            issues.append(ValidationIssue(
                level='warning',
                code='NO_CHAPTERS',
                message="No <chapter> elements found",
                suggestion="Book should have chapter structure for proper chunking"
            ))

        # Check chapter IDs
        chapters_without_ids = []
        for chapter in chapters:
            if not chapter.get('id'):
                title_elem = chapter.find('title')
                title_text = title_elem.text if title_elem is not None else "untitled"
                chapters_without_ids.append(title_text[:50])

        if chapters_without_ids:
            issues.append(ValidationIssue(
                level='warning',
                code='CHAPTERS_NO_ID',
                message=f"{len(chapters_without_ids)} chapter(s) missing 'id' attribute",
                suggestion="Add id attributes for proper linking: " + ", ".join(chapters_without_ids[:3])
            ))

        # Check sect1 elements
        sect1s = root.findall('.//sect1')
        sect1s_without_ids = [s for s in sect1s if not s.get('id')]
        if sect1s_without_ids:
            issues.append(ValidationIssue(
                level='warning',
                code='SECT1_NO_ID',
                message=f"{len(sect1s_without_ids)} sect1 element(s) missing 'id' attribute",
                suggestion="Chunking requires sect1 elements to have unique IDs"
            ))

        # Check for duplicate IDs
        all_ids = [elem.get('id') for elem in root.iter() if elem.get('id')]
        seen_ids = set()
        duplicate_ids = []
        for id_val in all_ids:
            if id_val in seen_ids:
                duplicate_ids.append(id_val)
            seen_ids.add(id_val)

        if duplicate_ids:
            issues.append(ValidationIssue(
                level='error',
                code='DUPLICATE_IDS',
                message=f"Duplicate IDs found: {', '.join(duplicate_ids[:5])}",
                suggestion="All id attributes must be unique within the document"
            ))

        return root, issues

    def validate_internal_links(self, root: etree._Element) -> Tuple[List[ValidationIssue], dict]:
        """
        Validate all internal links in the document.

        Checks that all @linkend references point to valid @id attributes.
        Also validates xref elements.

        Returns:
            Tuple of (issues list, stats dict)
        """
        issues = []
        stats = {
            'total_ids': 0,
            'total_links': 0,
            'broken_links': 0,
            'valid_links': 0,
            'broken_details': []
        }

        # Collect all IDs in the document
        all_ids = set()
        for elem in root.iter():
            elem_id = elem.get('id')
            if elem_id:
                all_ids.add(elem_id)

        stats['total_ids'] = len(all_ids)

        # Collect all linkend references
        linkend_refs = []

        # Check <link linkend="..."> elements
        for link in root.iter('link'):
            linkend = link.get('linkend')
            if linkend:
                linkend_refs.append(('link', linkend, link))

        # Check <xref linkend="..."> elements
        for xref in root.iter('xref'):
            linkend = xref.get('linkend')
            if linkend:
                linkend_refs.append(('xref', linkend, xref))

        # Check <footnoteref linkref="..."> elements
        for fnref in root.iter('footnoteref'):
            linkref = fnref.get('linkend')
            if linkref:
                linkend_refs.append(('footnoteref', linkref, fnref))

        # Check <biblioref linkend="..."> elements
        for bibref in root.iter('biblioref'):
            linkend = bibref.get('linkend')
            if linkend:
                linkend_refs.append(('biblioref', linkend, bibref))

        stats['total_links'] = len(linkend_refs)

        # Validate each link
        broken_links = []
        for link_type, linkend, elem in linkend_refs:
            if linkend not in all_ids:
                broken_links.append({
                    'type': link_type,
                    'linkend': linkend,
                    'context': self._get_element_context(elem)
                })

        stats['broken_links'] = len(broken_links)
        stats['valid_links'] = stats['total_links'] - stats['broken_links']
        stats['broken_details'] = broken_links[:20]  # Limit to first 20

        # Create issues for broken links
        if broken_links:
            # Group by linkend to avoid duplicate messages
            broken_targets = {}
            for bl in broken_links:
                target = bl['linkend']
                if target not in broken_targets:
                    broken_targets[target] = []
                broken_targets[target].append(bl['type'])

            for target, types in list(broken_targets.items())[:10]:
                issues.append(ValidationIssue(
                    level='error',
                    code='BROKEN_LINK',
                    message=f"Broken link: '{target}' (used by {', '.join(set(types))})",
                    suggestion=f"Ensure element with id='{target}' exists in the document"
                ))

            if len(broken_targets) > 10:
                issues.append(ValidationIssue(
                    level='error',
                    code='BROKEN_LINKS_SUMMARY',
                    message=f"... and {len(broken_targets) - 10} more broken link targets",
                    suggestion="Run with --keep-temp to see full link validation report"
                ))

        return issues, stats

    def _get_element_context(self, elem: etree._Element) -> str:
        """Get context string for an element (parent info, text snippet)"""
        parent = elem.getparent()
        parent_info = f"in <{parent.tag}>" if parent is not None else ""

        # Get text content snippet
        text = ''.join(elem.itertext())[:50]
        if text:
            return f"{parent_info} text: '{text}...'"
        return parent_info

    def run_add_ris_info(self, xml_path: Path, output_path: Path) -> TransformResult:
        """
        Run AddRISInfo.xsl transformation.

        This adds risinfo elements to sect1info, including:
        - Primary author
        - Chapter info reference
        - Book metadata
        """
        self.log("Running AddRISInfo.xsl...")

        result = self.runner.transform(
            xml_input=xml_path,
            xsl_file="AddRISInfo.xsl",
            output_file=output_path
        )

        if result.success:
            self.log("AddRISInfo transformation completed", "success")
        else:
            self.log("AddRISInfo transformation FAILED", "error")
            for error in result.errors:
                self.log(f"  {error}", "error")

        return result

    def run_chunking(self, xml_path: Path, output_dir: Path) -> TransformResult:
        """
        Chunk the book into separate sect1 XML files.

        This is a Python implementation of RISChunker.xsl functionality
        since the original uses Saxon-specific xsl:document extension.
        """
        self.log("Running chunking (Python implementation)...")

        result = TransformResult(success=False)
        start_time = time.time()

        try:
            parser = etree.XMLParser(
                remove_blank_text=False,
                resolve_entities=False,
                load_dtd=False,
                no_network=True
            )
            tree = etree.parse(str(xml_path), parser)
            root = tree.getroot()

            # Get ISBN for naming
            isbn_elem = root.find('.//bookinfo/isbn')
            isbn = isbn_elem.text.replace('-', '').replace(' ', '') if isbn_elem is not None and isbn_elem.text else 'unknown'

            output_dir.mkdir(parents=True, exist_ok=True)

            # Find all sect1 elements to chunk
            sect1_elements = root.findall('.//sect1[@id]')
            chunked_count = 0

            for sect1 in sect1_elements:
                sect1_id = sect1.get('id')
                if not sect1_id:
                    continue

                # Create chunk filename
                chunk_filename = f"sect1.{isbn}.{sect1_id}.xml"
                chunk_path = output_dir / chunk_filename

                # Create a standalone sect1 document
                chunk_root = etree.Element('sect1')
                for attr, value in sect1.attrib.items():
                    chunk_root.set(attr, value)

                # Copy all children
                for child in sect1:
                    chunk_root.append(etree.fromstring(etree.tostring(child)))

                # Write chunk
                chunk_tree = etree.ElementTree(chunk_root)
                chunk_tree.write(
                    str(chunk_path),
                    encoding='UTF-8',
                    xml_declaration=True,
                    pretty_print=True
                )
                chunked_count += 1
                result.messages.append(f"Created chunk: {chunk_filename}")

            # Copy main book.xml (would normally have entity refs)
            main_output = output_dir / "Book.xml"
            shutil.copy(xml_path, main_output)

            result.success = True
            result.messages.append(f"Chunked {chunked_count} sect1 elements")
            self.log(f"Created {chunked_count} chunk files", "success")

        except Exception as e:
            result.errors.append(f"Chunking error: {e}")
            self.log(f"Chunking failed: {e}", "error")

        result.duration_ms = (time.time() - start_time) * 1000
        return result

    def run_toc_transform(self, xml_path: Path, output_path: Path) -> TransformResult:
        """
        Run toctransform.xsl to generate TOC structure.

        This creates the table of contents XML with tocchap, toclevel1, etc.
        """
        self.log("Running toctransform.xsl...")

        result = self.runner.transform(
            xml_input=xml_path,
            xsl_file="toctransform.xsl",
            output_file=output_path
        )

        if result.success:
            self.log("TOC transformation completed", "success")
            # Count TOC entries
            if result.output_tree is not None:
                # Count elements with names starting with "toc"
                toc_entries = sum(1 for elem in result.output_tree.iter()
                                  if elem.tag.startswith('toc') or elem.tag.endswith('toc'))
                result.messages.append(f"Generated {toc_entries} TOC entries")
        else:
            self.log("TOC transformation FAILED", "error")
            for error in result.errors:
                self.log(f"  {error}", "error")

        return result

    def run_rittnav_test(self, toc_xml_path: Path, output_path: Path, objectid: str = "") -> TransformResult:
        """
        Test rittnav.xsl navigation generation.

        This generates navigation XML for a given section, used by R2Utilities
        to build the navigation panel in the web interface.
        """
        self.log("Running rittnav.xsl (navigation test)...")

        result = self.runner.transform(
            xml_input=toc_xml_path,
            xsl_file="rittnav.xsl",
            output_file=output_path,
            params={"objectid": objectid} if objectid else None
        )

        if result.success:
            self.log("Navigation generation completed", "success")
        else:
            self.log("Navigation generation FAILED", "error")
            for error in result.errors:
                self.log(f"  {error}", "error")

        return result

    def run_ritttoc_test(self, toc_xml_path: Path, output_path: Path) -> TransformResult:
        """
        Test ritttoc.xsl TOC HTML rendering.

        This generates the HTML table of contents used in the web interface.
        """
        self.log("Running ritttoc.xsl (TOC HTML test)...")

        result = self.runner.transform(
            xml_input=toc_xml_path,
            xsl_file="ritttoc.xsl",
            output_file=output_path
        )

        if result.success:
            self.log("TOC HTML generation completed", "success")
        else:
            self.log("TOC HTML generation FAILED", "error")
            for error in result.errors:
                self.log(f"  {error}", "error")

        return result

    def run_ritt_book_test(self, xml_path: Path, output_path: Path) -> TransformResult:
        """
        Optionally test RittBook.xsl HTML rendering.

        This is a more thorough test that validates HTML output generation.
        """
        self.log("Running RittBook.xsl (HTML rendering test)...")

        result = self.runner.transform(
            xml_input=xml_path,
            xsl_file="RittBook.xsl",
            output_file=output_path
        )

        if result.success:
            self.log("RittBook HTML rendering completed", "success")
        else:
            # RittBook often has warnings for unhandled elements - not always fatal
            if result.warnings and not result.errors:
                self.log("RittBook completed with warnings", "warning")
            else:
                self.log("RittBook transformation FAILED", "error")
                for error in result.errors:
                    self.log(f"  {error}", "error")

        return result

    def run_pipeline(
        self,
        input_path: Path,
        skip_html: bool = False,
        keep_temp: bool = False
    ) -> PipelineResult:
        """
        Run the complete lean transformation pipeline.

        Args:
            input_path: Path to input ZIP or XML file
            skip_html: Skip the RittBook HTML rendering test
            keep_temp: Keep temporary files after completion

        Returns:
            PipelineResult with complete test results
        """
        start_time = time.time()
        result = PipelineResult(
            success=False,
            input_file=str(input_path),
            timestamp=datetime.now().isoformat()
        )

        # Create temp directory for processing
        temp_dir = Path(tempfile.mkdtemp(prefix="pipeline_test_"))
        work_dir = temp_dir / "work"
        work_dir.mkdir()

        try:
            print(f"\n{'='*60}")
            print(f"LEAN PIPELINE TESTER")
            print(f"{'='*60}")
            print(f"Input: {input_path.name}")
            print(f"Time: {result.timestamp}")
            print(f"{'='*60}\n")

            # Step 1: Extract/Copy input
            print("STAGE 1: Input Preparation")
            print("-" * 40)

            if input_path.suffix.lower() == '.zip':
                extract_dir = work_dir / "extracted"
                extract_dir.mkdir()
                xml_path = self.extract_zip(input_path, extract_dir)
                self.log(f"Found XML: {xml_path.name}", "success")
            else:
                xml_path = input_path

            result.stages['input'] = {
                'success': True,
                'file': str(xml_path)
            }

            # Step 2: Pre-flight checks
            print("\nSTAGE 2: Pre-flight Validation")
            print("-" * 40)

            root, issues = self.preflight_checks(xml_path)
            result.preflight_issues = issues

            errors = [i for i in issues if i.level == 'error']
            warnings = [i for i in issues if i.level == 'warning']

            if errors:
                print(f"  ✗ {len(errors)} error(s) found:")
                for issue in errors:
                    print(f"    [{issue.code}] {issue.message}")
                    if issue.suggestion:
                        print(f"      → {issue.suggestion}")

            if warnings:
                print(f"  ⚠ {len(warnings)} warning(s) found:")
                for issue in warnings:
                    print(f"    [{issue.code}] {issue.message}")

            if not errors:
                print("  ✓ Pre-flight checks passed")

            result.stages['preflight'] = {
                'success': len(errors) == 0,
                'errors': len(errors),
                'warnings': len(warnings)
            }

            # Stop if critical errors
            if root is None or errors:
                result.summary = f"Pre-flight failed: {len(errors)} error(s)"
                return result

            # Step 3: Internal Link Validation
            print("\nSTAGE 3: Internal Link Validation")
            print("-" * 40)

            link_issues, link_stats = self.validate_internal_links(root)

            print(f"  Found {link_stats['total_ids']} IDs, {link_stats['total_links']} links")

            if link_stats['broken_links'] > 0:
                print(f"  ✗ {link_stats['broken_links']} broken link(s) found:")
                for issue in link_issues[:5]:
                    print(f"    [{issue.code}] {issue.message}")
                if len(link_issues) > 5:
                    print(f"    ... and {len(link_issues) - 5} more")
            else:
                print(f"  ✓ All {link_stats['valid_links']} links valid")

            result.stages['link_validation'] = {
                'success': link_stats['broken_links'] == 0,
                'total_ids': link_stats['total_ids'],
                'total_links': link_stats['total_links'],
                'broken_links': link_stats['broken_links'],
                'valid_links': link_stats['valid_links'],
                'broken_details': link_stats['broken_details']
            }

            # Add link issues to preflight issues for reporting
            result.preflight_issues.extend(link_issues)

            # Step 4: AddRISInfo transformation
            print("\nSTAGE 4: AddRISInfo.xsl")
            print("-" * 40)

            ris_output = work_dir / "book_with_ris.xml"
            ris_result = self.run_add_ris_info(xml_path, ris_output)

            result.stages['add_ris_info'] = {
                'success': ris_result.success,
                'duration_ms': ris_result.duration_ms,
                'messages': ris_result.messages,
                'warnings': ris_result.warnings,
                'errors': ris_result.errors
            }

            if not ris_result.success:
                result.summary = "Failed at AddRISInfo.xsl transformation"
                return result

            print(f"  Duration: {ris_result.duration_ms:.0f}ms")

            # Step 5: Chunking
            print("\nSTAGE 5: Chunking (sect1 splitting)")
            print("-" * 40)

            chunk_dir = work_dir / "chunks"
            chunk_result = self.run_chunking(ris_output, chunk_dir)

            result.stages['chunking'] = {
                'success': chunk_result.success,
                'duration_ms': chunk_result.duration_ms,
                'messages': chunk_result.messages,
                'errors': chunk_result.errors
            }

            if not chunk_result.success:
                result.summary = "Failed at chunking stage"
                return result

            print(f"  Duration: {chunk_result.duration_ms:.0f}ms")
            for msg in chunk_result.messages:
                print(f"  {msg}")

            # Step 6: TOC Transform
            print("\nSTAGE 6: TOC Transform")
            print("-" * 40)

            toc_output = work_dir / "toc.xml"
            toc_result = self.run_toc_transform(ris_output, toc_output)

            result.stages['toc_transform'] = {
                'success': toc_result.success,
                'duration_ms': toc_result.duration_ms,
                'messages': toc_result.messages,
                'warnings': toc_result.warnings,
                'errors': toc_result.errors
            }

            if not toc_result.success:
                result.summary = "Failed at TOC transformation"
                return result

            print(f"  Duration: {toc_result.duration_ms:.0f}ms")
            for msg in toc_result.messages:
                print(f"  {msg}")

            # Step 7: R2Utilities - rittnav.xsl (Navigation)
            print("\nSTAGE 7: rittnav.xsl (R2Utilities Navigation)")
            print("-" * 40)

            nav_output = work_dir / "nav.xml"
            nav_result = self.run_rittnav_test(toc_output, nav_output)

            result.stages['rittnav'] = {
                'success': nav_result.success,
                'duration_ms': nav_result.duration_ms,
                'messages': nav_result.messages,
                'warnings': nav_result.warnings,
                'errors': nav_result.errors
            }

            print(f"  Duration: {nav_result.duration_ms:.0f}ms")
            if not nav_result.success:
                print("  ⚠ Navigation transform failed (non-fatal)")

            # Step 8: R2Utilities - ritttoc.xsl (TOC HTML)
            print("\nSTAGE 8: ritttoc.xsl (R2Utilities TOC HTML)")
            print("-" * 40)

            toc_html_output = work_dir / "toc.html"
            toc_html_result = self.run_ritttoc_test(toc_output, toc_html_output)

            result.stages['ritttoc'] = {
                'success': toc_html_result.success,
                'duration_ms': toc_html_result.duration_ms,
                'messages': toc_html_result.messages,
                'warnings': toc_html_result.warnings,
                'errors': toc_html_result.errors
            }

            print(f"  Duration: {toc_html_result.duration_ms:.0f}ms")
            if not toc_html_result.success:
                print("  ⚠ TOC HTML transform failed (non-fatal)")

            # Step 9: Optional HTML rendering test
            if not skip_html:
                print("\nSTAGE 9: RittBook.xsl (HTML Rendering)")
                print("-" * 40)

                html_output = work_dir / "output.html"
                html_result = self.run_ritt_book_test(ris_output, html_output)

                result.stages['ritt_book'] = {
                    'success': html_result.success,
                    'duration_ms': html_result.duration_ms,
                    'messages': html_result.messages,
                    'warnings': html_result.warnings,
                    'errors': html_result.errors
                }

                print(f"  Duration: {html_result.duration_ms:.0f}ms")
                if html_result.warnings:
                    print(f"  ⚠ {len(html_result.warnings)} warning(s)")

            # All stages passed
            result.success = True
            result.summary = "All transformation stages passed"

            # Copy outputs if keeping
            if keep_temp:
                final_output = self.output_dir / f"test_{datetime.now().strftime('%Y%m%d_%H%M%S')}"
                shutil.copytree(work_dir, final_output)
                print(f"\n  Output saved to: {final_output}")

        except Exception as e:
            result.success = False
            result.summary = f"Pipeline error: {e}"
            print(f"\n✗ Pipeline error: {e}")
            import traceback
            traceback.print_exc()

        finally:
            # Cleanup temp directory
            if not keep_temp and temp_dir.exists():
                shutil.rmtree(temp_dir)

        result.duration_ms = (time.time() - start_time) * 1000

        # Print summary
        print(f"\n{'='*60}")
        print("PIPELINE TEST SUMMARY")
        print(f"{'='*60}")
        print(f"Status: {'✓ PASSED' if result.success else '✗ FAILED'}")
        print(f"Duration: {result.duration_ms:.0f}ms ({result.duration_ms/1000:.1f}s)")
        print(f"Summary: {result.summary}")
        print(f"{'='*60}\n")

        return result


def main():
    parser = argparse.ArgumentParser(
        description="Lean XML Transformation Pipeline Tester",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Examples:
    python pipeline_tester.py 9781234567890.zip
    python pipeline_tester.py Book.xml --skip-html
    python pipeline_tester.py output.zip --keep-temp --verbose
    python pipeline_tester.py input.xml --json-output results.json
        """
    )

    parser.add_argument(
        "input",
        type=Path,
        help="Input ZIP or XML file to test"
    )
    parser.add_argument(
        "--skip-html",
        action="store_true",
        help="Skip the RittBook.xsl HTML rendering test"
    )
    parser.add_argument(
        "--keep-temp",
        action="store_true",
        help="Keep temporary/output files after completion"
    )
    parser.add_argument(
        "--verbose", "-v",
        action="store_true",
        help="Enable verbose output"
    )
    parser.add_argument(
        "--json-output",
        type=Path,
        help="Write results to JSON file"
    )

    args = parser.parse_args()

    if not args.input.exists():
        print(f"Error: Input file not found: {args.input}")
        sys.exit(1)

    tester = PipelineTester(verbose=args.verbose)
    result = tester.run_pipeline(
        input_path=args.input,
        skip_html=args.skip_html,
        keep_temp=args.keep_temp
    )

    # Write JSON output if requested
    if args.json_output:
        with open(args.json_output, 'w') as f:
            json.dump(result.to_dict(), f, indent=2)
        print(f"Results written to: {args.json_output}")

    sys.exit(0 if result.success else 1)


if __name__ == "__main__":
    main()
