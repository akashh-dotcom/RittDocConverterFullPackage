#!/usr/bin/env python3
"""
Regression test for TOC depth normalization.

Some NCX hierarchies include wrappers above chapters (e.g., Book → Part → Chapter → Section).
When we use TOC depth to choose DocBook section levels, we must normalize relative to the
chapter's own depth; otherwise, the first in-chapter heading can incorrectly become <sect3>,
creating invalid nesting like <sect1><sect3>... which violates the DTD.
"""

from lxml import etree

from epub_to_structured_v2 import build_toc_depth_map, convert_xhtml_to_chapter
from reference_mapper import get_mapper, reset_mapper


def _title_text(elem: etree.Element) -> str:
    if elem is None:
        return ""
    return "".join(elem.itertext()).strip()


def test_toc_depth_normalization_wrapped_ncx() -> None:
    """
    Simulate an NCX hierarchy:
      depth 1: Book
      depth 2: Part
      depth 3: Chapter (file = ch0068.xml)
      depth 4: Section (h2 id=sec2-head-1134) → should become <sect1>
      depth 5: Subsection (h3 id=sec2-head-1138) → should become <sect2>
    """
    reset_mapper()
    mapper = get_mapper()

    toc_entries = [
        {
            "id": "book",
            "title": "Book",
            "href": "book.xml",
            "type": None,
            "children": [
                {
                    "id": "part1",
                    "title": "Part 1",
                    "href": "p0001.xml",
                    "type": None,
                    "children": [
                        {
                            "id": "ch0068",
                            "title": "Chapter 51",
                            "href": "ch0068.xml",
                            "type": "chapter",
                            "children": [
                                {
                                    "id": "sec2-head-1134",
                                    "title": "TAXONOMY, AND DESCRIPTION OF THE GENUS",
                                    "href": "ch0068.xml#sec2-head-1134",
                                    "type": None,
                                    "children": [
                                        {
                                            "id": "sec2-head-1138",
                                            "title": "Cat Scratch Disease",
                                            "href": "ch0068.xml#sec2-head-1138",
                                            "type": None,
                                            "children": [],
                                        }
                                    ],
                                }
                            ],
                        }
                    ],
                }
            ],
        }
    ]

    toc_depth_map = build_toc_depth_map(toc_entries)

    xhtml = b"""<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
  <head><title>Dummy</title></head>
  <body>
    <h1 id="c51">51 Bartonella</h1>
    <section>
      <h2 id="sec2-head-1134">TAXONOMY, AND DESCRIPTION OF THE GENUS</h2>
      <p>Intro para.</p>
      <section>
        <h3 id="sec2-head-1138">Cat Scratch Disease</h3>
        <p>Nested para.</p>
      </section>
    </section>
  </body>
</html>
"""

    chapter = convert_xhtml_to_chapter(
        xhtml_content=xhtml,
        doc_path="OEBPS/Text/ch0068.xhtml",
        chapter_id="ch0068",
        mapper=mapper,
        toc_depth_map=toc_depth_map,
        element_type="chapter",
        matter_category=None,
    )

    # The h2 should become sect1, not sect3.
    sect1_titles = chapter.xpath(".//sect1/title")
    assert any(
        _title_text(t) == "TAXONOMY, AND DESCRIPTION OF THE GENUS" for t in sect1_titles
    ), "Expected h2 to produce <sect1><title>..."

    # The nested h3 should become sect2 under that sect1.
    sect2_titles = chapter.xpath(".//sect1//sect2/title")
    assert any(_title_text(t) == "Cat Scratch Disease" for t in sect2_titles), (
        "Expected h3 to produce <sect2> nested under <sect1> when NCX is wrapped"
    )

    # DTD guard: sect3 is not allowed directly under sect1; ensure we didn't create that structure.
    assert len(chapter.xpath(".//sect1/sect3")) == 0

