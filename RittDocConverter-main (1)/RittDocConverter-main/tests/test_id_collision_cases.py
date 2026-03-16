#!/usr/bin/env python3
"""
Test cases for ID code collision scenarios.

These tests verify that the ID generation and parsing correctly handles cases where
the same code is used for both chapter prefixes AND element codes.

Collision Cases:
1. `pr` - Used for Preface (chapter) AND Procedure (element)
2. `gl` - Used for Glossary (chapter) AND Glossary Entry (element)
3. `bi`/`bib` - `bi` for Bibliography (chapter), `bib` for Bibliography Entry (element)

Based on R2_LINKEND_AND_TOC_RULESET.md Section 1.5
"""

import re
import sys
import unittest
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent.parent))

from id_authority import ELEMENT_CODES, MAX_ID_LENGTH, validate_id_simple as validate_id


class TestIDCollisionCases(unittest.TestCase):
    """Test ID collision scenarios between chapter prefixes and element codes."""

    def test_pr_collision_preface_with_procedure(self):
        """Test that procedure elements in preface sections have valid IDs."""
        # Preface section with procedure element
        preface_procedure_id = "pr0001s0000pr0001"

        # Validate ID format
        is_valid, error = validate_id(preface_procedure_id)
        self.assertTrue(is_valid, f"ID should be valid: {error}")

        # Check length constraint
        self.assertLessEqual(len(preface_procedure_id), MAX_ID_LENGTH)

        # Verify structure: pr0001 (preface) + s0000 (section) + pr0001 (procedure)
        self.assertTrue(preface_procedure_id.startswith("pr0001"))
        self.assertIn("s0000", preface_procedure_id)
        self.assertTrue(preface_procedure_id.endswith("pr0001"))

    def test_pr_collision_chapter_with_procedure(self):
        """Test that procedure elements in regular chapters have no collision."""
        # Chapter section with procedure element (no collision)
        chapter_procedure_id = "ch0001s0001pr0001"

        is_valid, error = validate_id(chapter_procedure_id)
        self.assertTrue(is_valid, f"ID should be valid: {error}")

        # Verify structure: ch0001 (chapter) + s0001 (section) + pr0001 (procedure)
        self.assertTrue(chapter_procedure_id.startswith("ch0001"))
        self.assertIn("s0001", chapter_procedure_id)
        self.assertTrue(chapter_procedure_id.endswith("pr0001"))

    def test_gl_collision_glossary_with_glossentry(self):
        """Test that glossary entries in glossary sections have valid IDs."""
        # Glossary section with glossary entry (expected collision)
        glossary_entry_id = "gl0001s0000gl0001"

        is_valid, error = validate_id(glossary_entry_id)
        self.assertTrue(is_valid, f"ID should be valid: {error}")

        # Verify structure: gl0001 (glossary) + s0000 (section) + gl0001 (glossentry)
        self.assertTrue(glossary_entry_id.startswith("gl0001"))
        self.assertIn("s0000", glossary_entry_id)
        self.assertTrue(glossary_entry_id.endswith("gl0001"))

    def test_gl_collision_chapter_with_glossentry(self):
        """Test that glossary entries in regular chapters have no collision."""
        # Chapter with glossary entry reference (no collision)
        chapter_glossentry_id = "ch0001s0001gl0001"

        is_valid, error = validate_id(chapter_glossentry_id)
        self.assertTrue(is_valid, f"ID should be valid: {error}")

        # Verify structure: ch0001 (chapter) + s0001 (section) + gl0001 (glossentry)
        self.assertTrue(chapter_glossentry_id.startswith("ch0001"))
        self.assertIn("s0001", chapter_glossentry_id)
        self.assertTrue(chapter_glossentry_id.endswith("gl0001"))

    def test_bi_bib_no_collision(self):
        """Test that bibliography entries use 'bib' (3-char) distinct from 'bi' (2-char)."""
        # Bibliography section with bibliography entry
        biblio_entry_id = "bi0001s0000bib0001"

        is_valid, error = validate_id(biblio_entry_id)
        self.assertTrue(is_valid, f"ID should be valid: {error}")

        # Verify 'bib' element code is distinct from 'bi' prefix
        self.assertTrue(biblio_entry_id.startswith("bi0001"))
        self.assertIn("bib0001", biblio_entry_id)

        # The 3-char 'bib' element code (18 chars total) is longer than
        # 2-char element codes (17 chars total), showing they're distinct
        self.assertEqual(len(biblio_entry_id), 18)  # bi0001 + s0000 + bib0001 = 6+5+7=18

        # XSLT finds 'bib' not 'bi' when searching for element code
        self.assertIn("bib", biblio_entry_id[11:])  # Element code in position 11+

    def test_element_codes_are_correct(self):
        """Verify that the element codes match what XSLT expects."""
        # Popup-linked elements (from link.ritt.xsl)
        expected_popup_codes = {
            'figure': 'fg',
            'table': 'ta',
            'equation': 'eq',
            'bibliomixed': 'bib',
            'glossentry': 'gl',
            'qandaset': 'qa',
            'procedure': 'pr',
            'video': 'vd',
            'audio': 'ad',
        }

        for element, expected_code in expected_popup_codes.items():
            actual_code = ELEMENT_CODES.get(element)
            self.assertEqual(
                actual_code, expected_code,
                f"Element '{element}' should have code '{expected_code}', got '{actual_code}'"
            )


class TestXSLTSubstringBehavior(unittest.TestCase):
    """
    Test cases that simulate XSLT substring-after() behavior for collision scenarios.

    The XSLT uses substring-after(@linkend, 'code') which returns everything
    after the FIRST occurrence of 'code' in the string.
    """

    def _substring_after(self, string: str, delimiter: str) -> str:
        """Simulate XSLT substring-after() function."""
        idx = string.find(delimiter)
        if idx == -1:
            return ""
        return string[idx + len(delimiter):]

    def test_xslt_pr_collision_behavior(self):
        """Test how XSLT would parse pr0001s0000pr0001."""
        linkend = "pr0001s0000pr0001"

        # XSLT: substring-after(@linkend, 'pr')
        result = self._substring_after(linkend, 'pr')

        # Should return "0001s0000pr0001" (after FIRST 'pr')
        self.assertEqual(result, "0001s0000pr0001")

        # This means XSLT sees the preface prefix, not the procedure element
        # The procedure element code won't be detected for popup behavior

    def test_xslt_gl_collision_behavior(self):
        """Test how XSLT would parse gl0001s0000gl0001."""
        linkend = "gl0001s0000gl0001"

        # XSLT: substring-after(@linkend, 'gl')
        result = self._substring_after(linkend, 'gl')

        # Should return "0001s0000gl0001" (after FIRST 'gl')
        self.assertEqual(result, "0001s0000gl0001")

        # This is expected - glossary entries in glossary sections
        # The second 'gl' is still present for popup detection

    def test_xslt_bib_no_collision(self):
        """Test how XSLT would parse bi0001s0000bib0001."""
        linkend = "bi0001s0000bib0001"

        # XSLT: substring-after(@linkend, 'bib')
        result = self._substring_after(linkend, 'bib')

        # Should return "0001" (after 'bib')
        self.assertEqual(result, "0001")

        # The 3-char 'bib' is found correctly, not confused with 2-char 'bi'

    def test_xslt_chapter_section_extraction(self):
        """Test XSLT chapter and section link extraction."""
        test_cases = [
            # (linkend, expected_chapter, expected_section)
            ("ch0001s0001fg0001", "ch0001", "s0001"),
            ("pr0001s0000ta0001", "pr0001", "s0000"),
            ("ap0001s0001eq0001", "ap0001", "s0001"),
            ("gl0001s0000gl0001", "gl0001", "s0000"),
            ("bi0001s0000bib0001", "bi0001", "s0000"),
        ]

        for linkend, expected_chapter, expected_section in test_cases:
            # Extract chapter (first 6 chars for standard prefixes)
            chapter = linkend[:6]
            self.assertEqual(
                chapter, expected_chapter,
                f"Chapter extraction failed for {linkend}"
            )

            # Extract section (s followed by 4 digits)
            section_match = re.search(r's\d{4}', linkend)
            self.assertIsNotNone(section_match, f"Section not found in {linkend}")
            self.assertEqual(
                section_match.group(), expected_section,
                f"Section extraction failed for {linkend}"
            )


class TestFileResolution(unittest.TestCase):
    """Test that file resolution uses the correct first 11 characters."""

    def test_file_resolution_standard_chapter(self):
        """Test file resolution for standard chapter elements."""
        linkend = "ch0001s0001fg0001"

        # File resolution uses first 11 characters
        file_id = linkend[:11]
        self.assertEqual(file_id, "ch0001s0001")

    def test_file_resolution_preface(self):
        """Test file resolution for preface elements."""
        linkend = "pr0001s0000fg0001"

        # File resolution uses first 11 characters
        file_id = linkend[:11]
        self.assertEqual(file_id, "pr0001s0000")

    def test_file_resolution_collision_case(self):
        """Test file resolution for collision cases."""
        # Preface with procedure
        linkend = "pr0001s0000pr0001"
        file_id = linkend[:11]
        self.assertEqual(file_id, "pr0001s0000")

        # Glossary with glossentry
        linkend = "gl0001s0000gl0001"
        file_id = linkend[:11]
        self.assertEqual(file_id, "gl0001s0000")

        # Bibliography with biblio entry
        linkend = "bi0001s0000bib0001"
        file_id = linkend[:11]
        self.assertEqual(file_id, "bi0001s0000")


class TestIDLengthConstraints(unittest.TestCase):
    """Test that collision cases stay within the 25-character limit."""

    def test_collision_ids_within_limit(self):
        """All collision case IDs should be within 25 characters."""
        collision_ids = [
            "pr0001s0000pr0001",      # 17 chars - preface + procedure
            "pr0001s0000pr9999",      # 17 chars - max procedure number
            "gl0001s0000gl0001",      # 17 chars - glossary + glossentry
            "gl0001s0000gl9999",      # 17 chars - max glossentry number
            "bi0001s0000bib0001",     # 18 chars - bibliography + biblio entry
            "bi0001s0000bib9999",     # 18 chars - max biblio entry number
        ]

        for id_value in collision_ids:
            self.assertLessEqual(
                len(id_value), MAX_ID_LENGTH,
                f"ID '{id_value}' ({len(id_value)} chars) exceeds max length {MAX_ID_LENGTH}"
            )

            is_valid, error = validate_id(id_value)
            self.assertTrue(is_valid, f"ID '{id_value}' should be valid: {error}")


if __name__ == "__main__":
    print("=" * 70)
    print("ID COLLISION CASE TESTS")
    print("=" * 70)
    print("Testing collision scenarios between chapter prefixes and element codes")
    print("Based on R2_LINKEND_AND_TOC_RULESET.md Section 1.5")
    print("=" * 70)
    print()

    unittest.main(verbosity=2)
