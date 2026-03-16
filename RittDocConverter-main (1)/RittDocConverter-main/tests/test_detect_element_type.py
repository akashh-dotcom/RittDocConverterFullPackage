"""
Tests for _detect_element_type() CSS class override logic.

When Springer uses class="Chapter" on a section element but epub:type="introduction",
the converter should classify it as 'chapter' (not 'preface').
"""

import sys
from pathlib import Path
from unittest.mock import MagicMock

import pytest

sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

from epub_to_structured_v2 import _detect_element_type


def _make_epub_item(xhtml_content: str, file_name: str = "test.xhtml"):
    """Create a mock EPUB item with the given XHTML content."""
    item = MagicMock()
    item.get_name.return_value = file_name
    item.file_name = file_name
    item.get_content.return_value = xhtml_content.encode('utf-8')
    item.properties = []
    return item


def _make_epub_book():
    """Create a minimal mock EpubBook."""
    book = MagicMock()
    book.get_items.return_value = []
    return book


def _make_xhtml(section_class: str, epub_type: str, role: str = "") -> str:
    """Generate minimal XHTML with the given section attributes."""
    role_attr = f' role="{role}"' if role else ''
    return f'''<?xml version="1.0" encoding="utf-8"?>
<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
<head><title>Test</title></head>
<body>
<section class="{section_class}" epub:type="{epub_type}"{role_attr}>
<h1>Test Title</h1>
<p>Content</p>
</section>
</body>
</html>'''


class TestClassChapterOverride:
    """Test that class='Chapter' overrides epub:type='introduction' -> preface."""

    def test_introduction_with_class_chapter_returns_chapter(self):
        """epub:type='introduction' + class='Chapter' should be 'chapter'."""
        xhtml = _make_xhtml("Chapter", "introduction", "doc-introduction")
        item = _make_epub_item(xhtml, "667463_1_En_248_Chapter.xhtml")
        book = _make_epub_book()

        result = _detect_element_type(item, "html/667463_1_En_248_Chapter.xhtml", book)
        assert result == 'chapter', f"Expected 'chapter' but got '{result}'"

    def test_introduction_with_class_preface_returns_preface(self):
        """epub:type='introduction' + class='Preface' should remain 'preface'."""
        xhtml = _make_xhtml("Preface", "introduction", "doc-introduction")
        item = _make_epub_item(xhtml, "test_intro.xhtml")
        book = _make_epub_book()

        result = _detect_element_type(item, "html/test_intro.xhtml", book)
        assert result == 'preface', f"Expected 'preface' but got '{result}'"

    def test_chapter_epub_type_with_class_chapter(self):
        """epub:type='chapter' + class='Chapter' should be 'chapter' (baseline)."""
        xhtml = _make_xhtml("Chapter", "chapter", "doc-chapter")
        item = _make_epub_item(xhtml, "chapter01.xhtml")
        book = _make_epub_book()

        result = _detect_element_type(item, "html/chapter01.xhtml", book)
        assert result == 'chapter', f"Expected 'chapter' but got '{result}'"

    def test_preface_epub_type_with_class_chapter_returns_chapter(self):
        """epub:type='preface' + class='Chapter' should override to 'chapter'."""
        xhtml = _make_xhtml("Chapter", "preface", "doc-preface")
        item = _make_epub_item(xhtml, "preface01.xhtml")
        book = _make_epub_book()

        # Note: filename "preface01" matches Priority 2 preface patterns,
        # so this will return 'preface' from filename detection before reaching Priority 3.
        # Use a generic filename instead.
        item2 = _make_epub_item(
            _make_xhtml("Chapter", "preface", "doc-preface"),
            "f01.xhtml"
        )
        result = _detect_element_type(item2, "html/f01.xhtml", book)
        assert result == 'chapter', f"Expected 'chapter' but got '{result}'"

    def test_foreword_with_class_chapter_returns_chapter(self):
        """epub:type='foreword' + class='Chapter' should override to 'chapter'."""
        xhtml = _make_xhtml("Chapter", "foreword", "doc-foreword")
        item = _make_epub_item(xhtml, "f02.xhtml")
        book = _make_epub_book()

        result = _detect_element_type(item, "html/f02.xhtml", book)
        assert result == 'chapter', f"Expected 'chapter' but got '{result}'"

    def test_dedication_not_overridden_by_class_chapter(self):
        """epub:type='dedication' + class='Chapter' should stay 'dedication' (not preface)."""
        xhtml = _make_xhtml("Chapter", "dedication", "doc-dedication")
        item = _make_epub_item(xhtml, "f03.xhtml")
        book = _make_epub_book()

        result = _detect_element_type(item, "html/f03.xhtml", book)
        assert result == 'dedication', f"Expected 'dedication' but got '{result}'"

    def test_no_section_element_falls_through(self):
        """When there's no section element, epub:type on body still works."""
        xhtml = '''<?xml version="1.0" encoding="utf-8"?>
<html xmlns="http://www.w3.org/1999/xhtml" xmlns:epub="http://www.idpf.org/2007/ops">
<head><title>Test</title></head>
<body epub:type="introduction">
<h1>Intro</h1>
<p>Content</p>
</body>
</html>'''
        item = _make_epub_item(xhtml, "f04.xhtml")
        book = _make_epub_book()

        result = _detect_element_type(item, "html/f04.xhtml", book)
        assert result == 'preface', f"Expected 'preface' but got '{result}'"
