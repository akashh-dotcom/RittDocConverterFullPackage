"""Tests for extracting partintro structural content into a synthetic chapter.

When a part's partintro contains structural content (sect1, sect2, section, simplesect),
that content is extracted into a separate chapter file. The synthetic chapter:
1. Uses the SAME title as its parent Part
2. Has role="partintro-content" to identify it as synthetic
3. Gets proper ID assignment and ID mappings are updated globally
4. Is placed as the FIRST chapter entity in the part (before other chapters)
"""
from lxml import etree
from collections import Counter

from package import _split_root


def test_partintro_structural_content_extracted_to_chapter():
    """Test that structural content (sect1) in partintro is extracted into a chapter."""
    book = etree.Element("book")
    book.set("id", "b001")

    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    part_title = etree.SubElement(part, "title")
    part_title.text = "Part I: Introduction"

    partintro = etree.SubElement(part, "partintro")
    sect1 = etree.SubElement(partintro, "sect1")
    sect1.set("id", "pt0001s0001")
    sect1_title = etree.SubElement(sect1, "title")
    sect1_title.text = "Overview Section"
    para = etree.SubElement(sect1, "para")
    para.text = "This is structural content that will be extracted."

    # Add a regular chapter
    chapter = etree.SubElement(part, "chapter")
    chapter.set("id", "ch0001")
    ch_title = etree.SubElement(chapter, "title")
    ch_title.text = "Chapter 1"

    book_root, fragments = _split_root(book)

    # Should have 2 chapter fragments: synthetic chapter + regular chapter
    ch_fragments = [f for f in fragments if f.entity.startswith("ch")]
    assert len(ch_fragments) == 2, f"Expected 2 chapter fragments, got {len(ch_fragments)}"

    # Find the synthetic chapter (role="partintro-content")
    synthetic_chapters = [
        f for f in ch_fragments
        if f.element.get("role") == "partintro-content"
    ]
    assert len(synthetic_chapters) == 1, "Expected exactly 1 synthetic chapter"

    synthetic = synthetic_chapters[0]
    # Verify the synthetic chapter has the Part's title
    synthetic_title = synthetic.element.find("title")
    assert synthetic_title is not None
    assert synthetic_title.text == "Part I: Introduction"

    # Verify the fragment title also matches
    assert synthetic.title == "Part I: Introduction"

    # Verify the structural content was moved to the synthetic chapter
    sect1_in_chapter = synthetic.element.find("sect1")
    assert sect1_in_chapter is not None
    sect1_title_in_chapter = sect1_in_chapter.find("title")
    assert sect1_title_in_chapter is not None
    assert sect1_title_in_chapter.text == "Overview Section"


def test_synthetic_chapter_placed_before_other_chapters():
    """Test that the synthetic chapter entity is placed FIRST in the part."""
    book = etree.Element("book")
    book.set("id", "b001")

    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    part_title = etree.SubElement(part, "title")
    part_title.text = "Part I"

    partintro = etree.SubElement(part, "partintro")
    sect1 = etree.SubElement(partintro, "sect1")
    etree.SubElement(sect1, "title").text = "Intro Section"

    # Add two regular chapters
    for i in range(1, 3):
        chapter = etree.SubElement(part, "chapter")
        chapter.set("id", f"ch{i:04d}")
        etree.SubElement(chapter, "title").text = f"Chapter {i}"

    book_root, fragments = _split_root(book)

    # Find the part element in book_root
    part_elem = book_root.find("part")
    assert part_elem is not None

    # Get all entity references in the part (after pre-content tags)
    entity_refs = []
    for child in part_elem:
        if isinstance(child, etree._Entity):
            entity_refs.append(child.name)

    # The synthetic chapter should be FIRST
    assert len(entity_refs) >= 1, "Expected at least 1 entity reference"

    # Find the synthetic chapter's entity ID
    synthetic = [f for f in fragments if f.element.get("role") == "partintro-content"]
    assert len(synthetic) == 1
    synthetic_entity = synthetic[0].entity

    # Verify synthetic chapter comes first
    assert entity_refs[0] == synthetic_entity, (
        f"Synthetic chapter {synthetic_entity} should be first, "
        f"but entity order is: {entity_refs}"
    )


def test_synthetic_chapter_ids_updated_correctly():
    """Test that IDs inside extracted partintro content are updated to new prefix."""
    book = etree.Element("book")
    book.set("id", "b001")

    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    etree.SubElement(part, "title").text = "Part I"

    partintro = etree.SubElement(part, "partintro")
    # Use pt#### prefix IDs (as they would be after chapter-to-part conversion)
    sect1 = etree.SubElement(partintro, "sect1")
    sect1.set("id", "pt0001s0001")
    etree.SubElement(sect1, "title").text = "Section"
    para = etree.SubElement(sect1, "para")
    para.set("id", "pt0001s0001p0001")
    para.text = "Content"

    book_root, fragments = _split_root(book)

    # Find the synthetic chapter
    synthetic = [f for f in fragments if f.element.get("role") == "partintro-content"]
    assert len(synthetic) == 1

    synthetic_chapter = synthetic[0].element
    synthetic_id = synthetic[0].entity  # e.g., ch0001

    # The sect1 ID should be updated to use the new chapter prefix
    sect1_in_chapter = synthetic_chapter.find("sect1")
    assert sect1_in_chapter is not None

    sect1_id = sect1_in_chapter.get("id")
    # ID should start with the synthetic chapter's prefix, not pt0001
    expected_prefix = synthetic_id  # e.g., ch0001
    assert sect1_id.startswith(expected_prefix), (
        f"sect1 ID {sect1_id} should start with {expected_prefix}"
    )


def test_no_synthetic_chapter_when_partintro_has_no_structural_content():
    """Test that no synthetic chapter is created when partintro only has simple content."""
    book = etree.Element("book")
    book.set("id", "b001")

    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    etree.SubElement(part, "title").text = "Part I"

    # partintro with only para elements (no sect1, sect2, etc.)
    partintro = etree.SubElement(part, "partintro")
    para = etree.SubElement(partintro, "para")
    para.text = "Just some introductory text, no structural content."

    chapter = etree.SubElement(part, "chapter")
    chapter.set("id", "ch0001")
    etree.SubElement(chapter, "title").text = "Chapter 1"

    book_root, fragments = _split_root(book)

    # Should only have 1 chapter fragment (the regular chapter)
    ch_fragments = [f for f in fragments if f.entity.startswith("ch")]
    assert len(ch_fragments) == 1

    # Verify no synthetic chapters were created
    synthetic = [f for f in fragments if f.element.get("role") == "partintro-content"]
    assert len(synthetic) == 0

    # Verify partintro is preserved in the part element
    part_elem = book_root.find("part")
    assert part_elem is not None
    partintro_elem = part_elem.find("partintro")
    assert partintro_elem is not None
    para_elem = partintro_elem.find("para")
    assert para_elem is not None
    assert para_elem.text == "Just some introductory text, no structural content."


def test_multiple_parts_with_partintro_no_id_collision():
    """Test that multiple parts with partintro content don't cause ID collisions."""
    book = etree.Element("book")
    book.set("id", "b001")

    # First part with partintro structural content
    part1 = etree.SubElement(book, "part")
    part1.set("id", "pt0001")
    etree.SubElement(part1, "title").text = "Part 1: First"

    partintro1 = etree.SubElement(part1, "partintro")
    sect1 = etree.SubElement(partintro1, "sect1")
    etree.SubElement(sect1, "title").text = "Part 1 Intro"

    # Chapters in part 1
    for i in range(1, 4):
        ch = etree.SubElement(part1, "chapter")
        ch.set("id", f"ch{i:04d}")
        etree.SubElement(ch, "title").text = f"Chapter {i}"

    # Second part with partintro structural content
    part2 = etree.SubElement(book, "part")
    part2.set("id", "pt0002")
    etree.SubElement(part2, "title").text = "Part 2: Second"

    partintro2 = etree.SubElement(part2, "partintro")
    sect2 = etree.SubElement(partintro2, "sect1")
    etree.SubElement(sect2, "title").text = "Part 2 Intro"

    # Chapters in part 2
    for i in range(4, 7):
        ch = etree.SubElement(part2, "chapter")
        ch.set("id", f"ch{i:04d}")
        etree.SubElement(ch, "title").text = f"Chapter {i}"

    book_root, fragments = _split_root(book)

    # Collect chapter entity IDs
    ch_ids = [frag.entity for frag in fragments if frag.entity.startswith("ch")]

    # Count occurrences
    id_counts = Counter(ch_ids)
    duplicates = {eid: cnt for eid, cnt in id_counts.items() if cnt > 1}

    assert duplicates == {}, f"Found duplicate chapter entity IDs: {duplicates}"

    # All IDs should be unique
    assert len(ch_ids) == len(set(ch_ids)), f"Duplicate IDs found in {ch_ids}"

    # Verify we have 2 synthetic chapters (one per part)
    synthetic = [f for f in fragments if f.element.get("role") == "partintro-content"]
    assert len(synthetic) == 2

    # Verify each synthetic chapter has the correct part title
    synthetic_titles = {f.title for f in synthetic}
    assert "Part 1: First" in synthetic_titles
    assert "Part 2: Second" in synthetic_titles


def test_sanitized_partintro_preserved_when_has_other_content():
    """Test that non-structural content is preserved in sanitized partintro."""
    book = etree.Element("book")
    book.set("id", "b001")

    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    etree.SubElement(part, "title").text = "Part I"

    partintro = etree.SubElement(part, "partintro")
    # Add non-structural content that should stay in partintro
    intro_para = etree.SubElement(partintro, "para")
    intro_para.text = "Brief introduction to this part."

    # Add structural content that should be extracted
    sect1 = etree.SubElement(partintro, "sect1")
    etree.SubElement(sect1, "title").text = "Detailed Section"
    etree.SubElement(sect1, "para").text = "Section content"

    book_root, fragments = _split_root(book)

    # Find the part in book_root
    part_elem = book_root.find("part")
    assert part_elem is not None

    # Verify sanitized partintro exists and has the para
    partintro_elem = part_elem.find("partintro")
    assert partintro_elem is not None

    para_elem = partintro_elem.find("para")
    assert para_elem is not None
    assert para_elem.text == "Brief introduction to this part."

    # Verify sect1 was NOT left in partintro
    assert partintro_elem.find("sect1") is None

    # Verify synthetic chapter was created with the sect1
    synthetic = [f for f in fragments if f.element.get("role") == "partintro-content"]
    assert len(synthetic) == 1
    sect1_in_chapter = synthetic[0].element.find("sect1")
    assert sect1_in_chapter is not None


def test_cross_references_to_partintro_content_updated():
    """Test that cross-references to IDs in partintro content are updated globally."""
    book = etree.Element("book")
    book.set("id", "b001")

    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    etree.SubElement(part, "title").text = "Part I"

    partintro = etree.SubElement(part, "partintro")
    sect1 = etree.SubElement(partintro, "sect1")
    sect1.set("id", "pt0001s0001")  # ID using part prefix
    etree.SubElement(sect1, "title").text = "Target Section"

    # Chapter with cross-reference to the partintro content
    chapter = etree.SubElement(part, "chapter")
    chapter.set("id", "ch0001")
    etree.SubElement(chapter, "title").text = "Chapter 1"
    para = etree.SubElement(chapter, "para")
    link = etree.SubElement(para, "link")
    link.set("linkend", "pt0001s0001")  # References the partintro sect1
    link.text = "See overview section"

    book_root, fragments = _split_root(book)

    # Find the synthetic chapter (it now contains the sect1)
    synthetic = [f for f in fragments if f.element.get("role") == "partintro-content"]
    assert len(synthetic) == 1

    synthetic_id = synthetic[0].entity

    # The sect1 ID in the synthetic chapter should be updated
    sect1_in_chapter = synthetic[0].element.find("sect1")
    new_sect1_id = sect1_in_chapter.get("id")
    assert new_sect1_id.startswith(synthetic_id), f"Expected {new_sect1_id} to start with {synthetic_id}"

    # Find the regular chapter fragment
    regular_chapters = [f for f in fragments if f.entity == "ch0001"]
    assert len(regular_chapters) == 1

    regular_chapter = regular_chapters[0].element

    # The link in the regular chapter should be updated to point to the new ID.
    # Cross-file references are converted to <ulink url="file.xml#id"> for DTD compliance.
    # So we need to check for either a link with updated linkend OR a ulink with updated url.
    ch_link = regular_chapter.find(".//link")
    ch_ulink = regular_chapter.find(".//ulink")

    # Expected new ID after prefix update: ch####s0001 (where ch#### is the synthetic chapter ID)
    expected_new_id = f"{synthetic_id}s0001"

    if ch_link is not None:
        # Link with linkend attribute
        linkend = ch_link.get("linkend")
        assert linkend == expected_new_id, (
            f"Cross-reference linkend={linkend} should be {expected_new_id}"
        )
    elif ch_ulink is not None:
        # Converted to ulink with url pointing to the file and ID
        url = ch_ulink.get("url")
        # URL format: <synthetic_id>.xml#<new_sect1_id>
        assert expected_new_id in url or new_sect1_id in url, (
            f"Cross-reference ulink url={url} should contain {expected_new_id}"
        )
    else:
        # The reference might be in the para's attributes or text
        # Let's just verify the prefix_remap was applied by checking if any
        # element has a reference to the new ID
        found_ref = False
        for elem in regular_chapter.iter():
            if not isinstance(elem.tag, str):
                continue
            linkend = elem.get("linkend") or ""
            url = elem.get("url") or ""
            if expected_new_id in linkend or expected_new_id in url or new_sect1_id in url:
                found_ref = True
                break
        assert found_ref, (
            f"Expected cross-reference to {expected_new_id} somewhere in regular chapter"
        )
