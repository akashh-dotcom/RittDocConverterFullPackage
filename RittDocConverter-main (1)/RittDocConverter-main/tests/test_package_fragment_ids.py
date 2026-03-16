from lxml import etree


def test_split_root_assigns_id_to_toc_fragment():
    from package import _split_root

    book = etree.Element("book")
    book.set("id", "b001")

    toc = etree.SubElement(book, "chapter")
    toc.set("role", "toc")
    title = etree.SubElement(toc, "title")
    title.text = "Table of Contents"

    _, fragments = _split_root(book)

    toc_frag = next(f for f in fragments if f.kind == "toc")
    assert (toc_frag.element.get("id") or "").strip() == toc_frag.entity


def test_split_root_keeps_part_inline_no_fragment_file():
    """
    Parts are emitted ONLY inline in Book.XML (with chapter entity references).
    NO separate part fragment files (pt0001.xml, etc.) are created.

    This is because the DocBook DTD requires <part> to have at least one child
    structural element (chapter, appendix, etc.). Since chapters are in separate
    files referenced via entities declared only in Book.XML, a standalone part
    file would be invalid according to the DTD.
    """
    from package import _split_root

    book = etree.Element("book")
    book.set("id", "b001")

    part = etree.SubElement(book, "part")
    part.set("id", "pt0001")
    title = etree.SubElement(part, "title")
    title.text = "Part I"

    # Add a chapter inside the part
    ch = etree.SubElement(part, "chapter")
    ch_title = etree.SubElement(ch, "title")
    ch_title.text = "Chapter 1"

    root_copy, fragments = _split_root(book)

    # Part should NOT be a fragment file (no pt0001.xml)
    part_frags = [f for f in fragments if f.section_type == "part"]
    assert len(part_frags) == 0, "Parts should NOT be emitted as fragment files"

    # The chapter inside the part should still be a fragment
    ch_frags = [f for f in fragments if f.section_type == "chapter"]
    assert len(ch_frags) == 1, "Chapter inside part should be a fragment"

    # Book.XML should have the part element inline
    part_in_book = root_copy.find(".//part")
    assert part_in_book is not None, "Part should be embedded in Book.XML"
    assert part_in_book.get("id") == "pt0001"

    # The inline part in Book.XML should have entity references to chapters
    has_chapter_entity_inline = False
    for child in part_in_book:
        if isinstance(child, etree._Entity):
            has_chapter_entity_inline = True
            break
    assert has_chapter_entity_inline, "Inline part in Book.XML should have entity refs to chapters"

    # The part should also have its title
    part_title = part_in_book.find("title")
    assert part_title is not None, "Part should have title"
    assert part_title.text == "Part I"


def test_split_root_assigns_id_to_chapter_fragment_when_missing():
    from package import _split_root

    book = etree.Element("book")
    book.set("id", "b001")

    ch = etree.SubElement(book, "chapter")
    etree.SubElement(ch, "title").text = "Chapter 1"

    _, fragments = _split_root(book)

    ch_frag = next(f for f in fragments if f.kind == "chapter")
    assert (ch_frag.element.get("id") or "").strip() == ch_frag.entity


def test_split_root_preserves_existing_chapter_ids():
    """
    When a chapter already has a valid ID from upstream conversion (e.g., ch0015),
    that ID should be preserved instead of being renumbered.

    This ensures consistency between structured.xml (created by epub_to_structured_v2)
    and the final package output. IDs assigned upstream reflect the actual chapter
    numbering and should not be overwritten.
    """
    from package import _split_root

    book = etree.Element("book")
    book.set("id", "b001")

    # First add a preface (will consume pr0001 per R2 convention)
    pf = etree.SubElement(book, "preface")
    pf.set("id", "pr0001")
    etree.SubElement(pf, "title").text = "Preface"
    sect1_pf = etree.SubElement(pf, "sect1")
    sect1_pf.set("id", "pr0001s0000")
    etree.SubElement(sect1_pf, "title").text = "Section"
    etree.SubElement(sect1_pf, "para").text = "Content"

    # Add a chapter with ID ch0015 - this ID should be PRESERVED, not renumbered
    # This simulates a chapter from upstream conversion (epub_to_structured_v2)
    ch = etree.SubElement(book, "chapter")
    ch.set("id", "ch0015")  # Upstream-assigned ID
    etree.SubElement(ch, "title").text = "Chapter 15"

    # Add internal elements with ch0015 prefix
    sect1 = etree.SubElement(ch, "sect1")
    sect1.set("id", "ch0015s0000")
    etree.SubElement(sect1, "title").text = "Introduction"

    anchor1 = etree.SubElement(sect1, "anchor")
    anchor1.set("id", "ch0015s0000a0001")

    anchor2 = etree.SubElement(sect1, "anchor")
    anchor2.set("id", "ch0015s0000a0002")

    para = etree.SubElement(sect1, "para")
    para.set("id", "ch0015s0000p0001")
    para.text = "Some text"

    # Add a link referencing an internal ID
    link = etree.SubElement(para, "link")
    link.set("linkend", "ch0015s0000a0001")
    link.text = "see above"

    _, fragments = _split_root(book)

    # Find the chapter fragment - should preserve ch0015
    ch_frag = next(f for f in fragments if f.section_type == "chapter")

    # The entity_id should be ch0015 (preserved from input)
    assert ch_frag.entity == "ch0015", f"Expected entity ch0015 (preserved), got {ch_frag.entity}"

    # The filename should match the preserved entity
    assert ch_frag.filename == "ch0015.xml", f"Expected filename ch0015.xml, got {ch_frag.filename}"

    # The chapter root ID should remain ch0015
    ch_elem = ch_frag.element
    assert ch_elem.get("id") == "ch0015", f"Chapter ID should be ch0015, got {ch_elem.get('id')}"

    # All internal IDs should remain unchanged (ch0015 prefix)
    sect1_elem = ch_elem.find(".//sect1")
    assert sect1_elem.get("id") == "ch0015s0000", f"Sect1 ID should be ch0015s0000, got {sect1_elem.get('id')}"

    anchors = ch_elem.findall(".//anchor")
    for anchor in anchors:
        anchor_id = anchor.get("id")
        assert anchor_id.startswith("ch0015"), f"Anchor ID should start with ch0015, got {anchor_id}"

    para_elem = ch_elem.find(".//para[@id]")
    assert para_elem.get("id") == "ch0015s0000p0001", f"Para ID should be ch0015s0000p0001, got {para_elem.get('id')}"

    # Link references should remain unchanged
    link_elem = ch_elem.find(".//link[@linkend]")
    assert link_elem.get("linkend") == "ch0015s0000a0001", f"Linkend should be ch0015s0000a0001, got {link_elem.get('linkend')}"

