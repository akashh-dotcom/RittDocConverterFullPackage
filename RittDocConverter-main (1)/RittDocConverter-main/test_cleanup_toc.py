#!/usr/bin/env python3
"""
Unit tests for cleanup_toc.py
"""

import sys
import unittest
from pathlib import Path

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).parent))

from cleanup_toc import (
    normalize_title,
    classify_title,
    clean_number_prefix,
    is_generic_title,
    fix_spacing,
)


class TestNormalizeTitle(unittest.TestCase):
    """Test title normalization."""

    def test_casefold_and_strip(self):
        self.assertEqual(normalize_title("  PREFACE  "), "preface")

    def test_remove_chapter_number_prefix(self):
        self.assertEqual(normalize_title("1: Introduction"), "introduction")
        self.assertEqual(normalize_title("20: Methods"), "methods")

    def test_remove_chapter_prefix(self):
        self.assertEqual(normalize_title("Chapter 1 Introduction"), "introduction")
        self.assertEqual(normalize_title("CHAPTER 5 Methods"), "methods")

    def test_remove_punctuation(self):
        self.assertEqual(normalize_title("About the Author's Notes"), "about the authors notes")

    def test_normalize_whitespace(self):
        self.assertEqual(normalize_title("About   the   Author"), "about the author")


class TestClassifyTitle(unittest.TestCase):
    """Test title classification."""

    def test_front_matter(self):
        self.assertEqual(classify_title("Preface"), "FRONT")
        self.assertEqual(classify_title("PREFACE"), "FRONT")
        self.assertEqual(classify_title("Contributors"), "FRONT")
        self.assertEqual(classify_title("About the Author"), "FRONT")
        self.assertEqual(classify_title("Table of Contents"), "FRONT")
        self.assertEqual(classify_title("Acknowledgements"), "FRONT")
        self.assertEqual(classify_title("Foreword"), "FRONT")
        self.assertEqual(classify_title("Introduction"), "FRONT")
        self.assertEqual(classify_title("Dedication"), "FRONT")
        self.assertEqual(classify_title("List of Figures"), "FRONT")
        self.assertEqual(classify_title("List of Tables"), "FRONT")
        self.assertEqual(classify_title("About the Editors"), "FRONT")
        self.assertEqual(classify_title("Biographies"), "FRONT")

    def test_back_matter(self):
        self.assertEqual(classify_title("Index"), "BACK")
        self.assertEqual(classify_title("INDEX"), "BACK")
        self.assertEqual(classify_title("Glossary"), "BACK")
        self.assertEqual(classify_title("Bibliography"), "BACK")
        self.assertEqual(classify_title("References"), "BACK")
        self.assertEqual(classify_title("Appendix"), "BACK")
        self.assertEqual(classify_title("Appendix A"), "BACK")
        self.assertEqual(classify_title("EULA"), "BACK")
        self.assertEqual(classify_title("Wiley End User License Agreement"), "BACK")
        self.assertEqual(classify_title("Further Reading"), "BACK")
        self.assertEqual(classify_title("Answers to Exercises"), "BACK")
        self.assertEqual(classify_title("Subject Index"), "BACK")
        self.assertEqual(classify_title("Author Index"), "BACK")

    def test_main_content(self):
        self.assertEqual(classify_title("Chapter 1"), "MAIN")
        self.assertEqual(classify_title("Chapter 15 Introduction"), "MAIN")
        self.assertEqual(classify_title("Chapter 1 Preface"), "MAIN")  # "Chapter X" overrides "preface"
        self.assertEqual(classify_title("Part I"), "MAIN")
        self.assertEqual(classify_title("Part IV Background"), "MAIN")
        self.assertEqual(classify_title("1: First Steps"), "MAIN")  # No front/back pattern match -> MAIN
        self.assertEqual(classify_title("5: Advanced Topics"), "MAIN")
        # Note: "1: Introduction" is FRONT because front matter patterns take precedence
        # over the generic "N:" pattern (see test_with_number_prefix)

    def test_unknown(self):
        self.assertEqual(classify_title("The History of Medicine"), "UNKNOWN")
        self.assertEqual(classify_title("Cardiovascular System"), "UNKNOWN")

    def test_with_number_prefix(self):
        # After normalization, "1:" prefix is removed before pattern matching
        # So "1: Index" -> normalizes to "index" -> matches back matter
        self.assertEqual(classify_title("1: Index"), "BACK")
        self.assertEqual(classify_title("2: Glossary"), "BACK")
        self.assertEqual(classify_title("1: Introduction"), "FRONT")
        self.assertEqual(classify_title("3: Preface"), "FRONT")
        # But content that doesn't match front/back patterns uses fallback "N:" -> MAIN
        self.assertEqual(classify_title("1: The Beginning"), "MAIN")


class TestCleanNumberPrefix(unittest.TestCase):
    """Test number prefix cleanup."""

    def test_colon_separator(self):
        text, modified = clean_number_prefix("1: Author Biography")
        self.assertEqual(text, "Author Biography")
        self.assertTrue(modified)

    def test_space_separator(self):
        text, modified = clean_number_prefix("1 Index")
        self.assertEqual(text, "Index")
        self.assertTrue(modified)

    def test_with_chapter(self):
        text, modified = clean_number_prefix("4: CHAPTER 1 Title")
        self.assertEqual(text, "CHAPTER 1 Title")
        self.assertTrue(modified)

    def test_whitespace_handling(self):
        text, modified = clean_number_prefix("1\n\n         History")
        self.assertEqual(text, "History")
        self.assertTrue(modified)

    def test_leading_spaces(self):
        text, modified = clean_number_prefix("  2:  Some Text")
        self.assertEqual(text, "Some Text")
        self.assertTrue(modified)

    def test_no_prefix(self):
        text, modified = clean_number_prefix("Regular Title")
        self.assertEqual(text, "Regular Title")
        self.assertFalse(modified)

    def test_empty_string(self):
        text, modified = clean_number_prefix("")
        self.assertEqual(text, "")
        self.assertFalse(modified)


class TestIsGenericTitle(unittest.TestCase):
    """Test generic title detection."""

    def test_sect_patterns(self):
        self.assertTrue(is_generic_title("Sect1"))
        self.assertTrue(is_generic_title("sect2"))
        self.assertTrue(is_generic_title("SECT"))

    def test_generic_words(self):
        self.assertTrue(is_generic_title("Section"))
        self.assertTrue(is_generic_title("Content"))
        self.assertTrue(is_generic_title("Chapter"))
        self.assertTrue(is_generic_title("Part"))
        self.assertTrue(is_generic_title("Title"))
        self.assertTrue(is_generic_title("Untitled"))

    def test_id_patterns(self):
        self.assertTrue(is_generic_title("ch0001"))
        self.assertTrue(is_generic_title("pr0002"))
        self.assertTrue(is_generic_title("dd0001"))
        self.assertTrue(is_generic_title("ch0005s0000"))

    def test_key_points(self):
        self.assertTrue(is_generic_title("Key Points"))
        self.assertTrue(is_generic_title("key point"))

    def test_short_titles(self):
        self.assertTrue(is_generic_title("A"))
        self.assertTrue(is_generic_title(""))
        self.assertTrue(is_generic_title("AB"))

    def test_real_titles(self):
        self.assertFalse(is_generic_title("Chapter 1 Introduction"))
        self.assertFalse(is_generic_title("The History of Medicine"))
        self.assertFalse(is_generic_title("Cardiovascular System"))
        self.assertFalse(is_generic_title("Introduction to Programming"))


class TestFixSpacing(unittest.TestCase):
    """Test spacing fix."""

    def test_chapter_regulation(self):
        text, modified = fix_spacing("CHAPTER 1Regulation")
        self.assertEqual(text, "CHAPTER 1 Regulation")
        self.assertTrue(modified)

    def test_part_methods(self):
        text, modified = fix_spacing("Part 2Methods")
        self.assertEqual(text, "Part 2 Methods")
        self.assertTrue(modified)

    def test_section_overview(self):
        text, modified = fix_spacing("Section 3Overview")
        self.assertEqual(text, "Section 3 Overview")
        self.assertTrue(modified)

    def test_already_spaced(self):
        text, modified = fix_spacing("Chapter 1 Introduction")
        self.assertEqual(text, "Chapter 1 Introduction")
        self.assertFalse(modified)

    def test_all_caps_after_number(self):
        # Should NOT trigger for all caps after number
        text, modified = fix_spacing("ABC123DEF")
        self.assertEqual(text, "ABC123DEF")
        self.assertFalse(modified)

    def test_multiple_occurrences(self):
        text, modified = fix_spacing("Test1Test and Test2Value")
        self.assertEqual(text, "Test1 Test and Test2 Value")
        self.assertTrue(modified)


class TestIntegration(unittest.TestCase):
    """Integration tests for combined rules."""

    def test_full_cleanup_pipeline(self):
        """Test that rules work together correctly."""
        # Simulate processing order: spacing -> number prefix -> generic detection

        # Input: "1: CHAPTER 1Regulation"
        text = "1: CHAPTER 1Regulation"

        # Rule 7: Spacing fix first
        text, _ = fix_spacing(text)
        self.assertEqual(text, "1: CHAPTER 1 Regulation")

        # Rule 5: Number prefix cleanup
        text, _ = clean_number_prefix(text)
        self.assertEqual(text, "CHAPTER 1 Regulation")

        # Rule 6: Generic title check (should be false for real title)
        self.assertFalse(is_generic_title(text))

        # Rule 1: Classification
        classification = classify_title(text)
        self.assertEqual(classification, "MAIN")  # "Chapter X" pattern

    def test_back_matter_reclassification(self):
        """Test that appendix gets reclassified to back matter."""
        # Input in tocchap: "1: Appendix A"
        text = "1: Appendix A"

        text, _ = fix_spacing(text)  # No change
        text, _ = clean_number_prefix(text)
        self.assertEqual(text, "Appendix A")

        classification = classify_title(text)
        self.assertEqual(classification, "BACK")

    def test_front_matter_reclassification(self):
        """Test that preface gets reclassified to front matter."""
        text = "1: Preface"

        text, _ = clean_number_prefix(text)
        self.assertEqual(text, "Preface")

        classification = classify_title(text)
        self.assertEqual(classification, "FRONT")


if __name__ == '__main__':
    unittest.main()
