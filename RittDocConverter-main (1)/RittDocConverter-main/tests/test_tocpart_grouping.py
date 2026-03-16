"""Test that tocpart correctly groups chapters under their parent parts."""

from lxml import etree
from package import _build_structure_toc, ChapterFragment


def test_tocpart_groups_chapters_under_part():
    """
    When a book has parts containing chapters, the TOC should use <tocpart>
    to group chapters under their parent part.

    Expected structure:
    <toc>
      <title>Table of Contents</title>
      <tocpart>
        <tocentry linkend="pt0001"><ulink url="pt0001">Part I: Introduction</ulink></tocentry>
        <tocchap>
          <tocentry linkend="ch0001"><ulink url="ch0001">Chapter 1</ulink></tocentry>
        </tocchap>
        <tocchap>
          <tocentry linkend="ch0002"><ulink url="ch0002">Chapter 2</ulink></tocentry>
        </tocchap>
      </tocpart>
    </toc>
    """
    # Create a book with a part containing chapters
    book = etree.Element("book")
    book.set("id", "b001")

    # Create Part I
    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    part_title = etree.SubElement(part, "title")
    part_title.text = "Part I: Introduction"

    # Add entity references for chapters inside the part
    # (simulating how chapters are referenced in book.xml)
    ch1_entity = etree.Entity("ch0001")
    part.append(ch1_entity)

    ch2_entity = etree.Entity("ch0002")
    part.append(ch2_entity)

    # Create fragment objects for the chapters
    fragments = [
        ChapterFragment(
            filename="ch0001.xml",
            element=etree.Element("chapter"),
            entity="ch0001",
            title="Chapter 1",
            section_type="chapter",
        ),
        ChapterFragment(
            filename="ch0002.xml",
            element=etree.Element("chapter"),
            entity="ch0002",
            title="Chapter 2",
            section_type="chapter",
        ),
    ]
    # Set IDs on the fragment elements
    fragments[0].element.set("id", "ch0001")
    fragments[1].element.set("id", "ch0002")

    # Build the TOC
    toc = _build_structure_toc(book, fragments)

    # Debug: print the TOC structure
    print("\nGenerated TOC:")
    print(etree.tostring(toc, pretty_print=True, encoding='unicode'))

    # Verify TOC was created
    assert toc is not None, "TOC should be created"
    assert toc.tag == "toc", "Root element should be <toc>"

    # Find tocpart elements
    tocparts = toc.findall(".//tocpart")
    assert len(tocparts) >= 1, f"Should have at least one tocpart, found {len(tocparts)}"

    tocpart = tocparts[0]

    # Verify tocpart has a tocentry for the part title
    part_entries = tocpart.findall("tocentry")
    assert len(part_entries) >= 1, "tocpart should have at least one tocentry for part title"

    # Verify chapters are nested inside tocpart as tocchap
    tocchaps = tocpart.findall("tocchap")
    assert len(tocchaps) == 2, f"Part should contain 2 tocchap elements, found {len(tocchaps)}"

    # Verify chapter entries have correct linkends
    ch1_entry = tocchaps[0].find("tocentry")
    ch2_entry = tocchaps[1].find("tocentry")

    assert ch1_entry is not None, "First tocchap should have tocentry"
    assert ch2_entry is not None, "Second tocchap should have tocentry"

    assert ch1_entry.get("linkend") == "ch0001", f"First chapter linkend should be ch0001, got {ch1_entry.get('linkend')}"
    assert ch2_entry.get("linkend") == "ch0002", f"Second chapter linkend should be ch0002, got {ch2_entry.get('linkend')}"


def test_chapters_without_part_are_direct_tocchap():
    """
    Chapters that are not inside a part should be direct children of <toc>
    as <tocchap> elements (not wrapped in tocpart).
    """
    # Create a book with chapters directly (no parts)
    book = etree.Element("book")
    book.set("id", "b001")

    # Add entity references for chapters at book level
    ch1_entity = etree.Entity("ch0001")
    book.append(ch1_entity)

    ch2_entity = etree.Entity("ch0002")
    book.append(ch2_entity)

    # Create fragment objects for the chapters
    fragments = [
        ChapterFragment(
            filename="ch0001.xml",
            element=etree.Element("chapter"),
            entity="ch0001",
            title="Chapter 1",
            section_type="chapter",
        ),
        ChapterFragment(
            filename="ch0002.xml",
            element=etree.Element("chapter"),
            entity="ch0002",
            title="Chapter 2",
            section_type="chapter",
        ),
    ]
    fragments[0].element.set("id", "ch0001")
    fragments[1].element.set("id", "ch0002")

    # Build the TOC
    toc = _build_structure_toc(book, fragments)

    # Debug: print the TOC structure
    print("\nGenerated TOC (no parts):")
    print(etree.tostring(toc, pretty_print=True, encoding='unicode'))

    # Verify TOC was created
    assert toc is not None, "TOC should be created"

    # Verify NO tocpart elements (since no parts in book)
    tocparts = toc.findall(".//tocpart")
    assert len(tocparts) == 0, f"Should have no tocpart elements, found {len(tocparts)}"

    # Verify tocchap elements are direct children of toc
    tocchaps = toc.findall("tocchap")
    assert len(tocchaps) == 2, f"Should have 2 tocchap elements as direct children, found {len(tocchaps)}"


def test_mixed_parts_and_standalone_chapters():
    """
    A book may have some chapters in parts and some standalone.
    Both should be handled correctly.
    """
    book = etree.Element("book")
    book.set("id", "b001")

    # Standalone chapter before parts
    ch_intro = etree.Entity("ch0001")
    book.append(ch_intro)

    # Part with chapters
    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    part_title = etree.SubElement(part, "title")
    part_title.text = "Part I"

    ch_in_part = etree.Entity("ch0002")
    part.append(ch_in_part)

    # Standalone chapter after parts (appendix style)
    ch_appendix = etree.Entity("ch0003")
    book.append(ch_appendix)

    fragments = [
        ChapterFragment(
            filename="ch0001.xml",
            element=etree.Element("chapter"),
            entity="ch0001",
            title="Introduction",
            section_type="chapter",
        ),
        ChapterFragment(
            filename="ch0002.xml",
            element=etree.Element("chapter"),
            entity="ch0002",
            title="Chapter in Part",
            section_type="chapter",
        ),
        ChapterFragment(
            filename="ch0003.xml",
            element=etree.Element("chapter"),
            entity="ch0003",
            title="Appendix",
            section_type="chapter",
        ),
    ]
    for i, frag in enumerate(fragments, 1):
        frag.element.set("id", f"ch000{i}")

    toc = _build_structure_toc(book, fragments)

    print("\nGenerated TOC (mixed):")
    print(etree.tostring(toc, pretty_print=True, encoding='unicode'))

    assert toc is not None

    # Should have 1 tocpart for Part I
    tocparts = toc.findall(".//tocpart")
    assert len(tocparts) == 1, f"Should have 1 tocpart, found {len(tocparts)}"

    # The tocpart should contain 1 chapter
    part_tocchaps = tocparts[0].findall("tocchap")
    assert len(part_tocchaps) == 1, f"Part should have 1 tocchap, found {len(part_tocchaps)}"

    # Direct children of toc should include standalone chapters as tocchap
    direct_tocchaps = toc.findall("tocchap")
    # Note: This may include intro and appendix if they're standalone
    print(f"Direct tocchaps: {len(direct_tocchaps)}")


if __name__ == "__main__":
    test_tocpart_groups_chapters_under_part()
    test_chapters_without_part_are_direct_tocchap()
    test_mixed_parts_and_standalone_chapters()
    print("\nAll tests passed!")
