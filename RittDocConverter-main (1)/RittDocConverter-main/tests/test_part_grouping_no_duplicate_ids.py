from lxml import etree


def test_part_grouping_preserves_part_id_and_creates_fragment():
    """
    If a chapter with id chNNNN is treated as a Part marker, packaging converts it to <part>
    with a corresponding ptNNNN id (ch0012 -> pt0012). The part exists inline in Book.XML
    (no separate part fragment file is created).

    Parts are only inline in Book.XML because the DocBook DTD requires parts to have
    child structural elements (chapters), which are referenced via entities only declared
    in Book.XML.
    """
    from package import _auto_group_chapters_into_parts, _split_root

    book = etree.Element("book")
    book.set("id", "b001")

    part_marker = etree.SubElement(book, "chapter")
    part_marker.set("id", "ch0012")
    etree.SubElement(part_marker, "title").text = "Part I"

    ch_next = etree.SubElement(book, "chapter")
    ch_next.set("id", "ch0013")
    etree.SubElement(ch_next, "title").text = "Chapter 1"

    assert _auto_group_chapters_into_parts(book) is True

    # After grouping, the part wrapper gets id="pt0012" (ch0012 -> pt0012)
    part = next(child for child in book if isinstance(child.tag, str) and child.tag == "part")
    assert (part.get("id") or "").strip() == "pt0012"

    # Split - parts are NOT created as fragment files
    book_root, fragments = _split_root(book)
    frag_ids = {(f.element.get("id") or "").strip() for f in fragments}

    # No part fragments should exist (parts are only inline in Book.XML)
    pt_frags = [f for f in fragments if f.section_type == "part"]
    assert len(pt_frags) == 0, f"Parts should NOT be fragment files, got {pt_frags}"

    # Chapter fragment should have id="ch0013" (preserved from input)
    assert "ch0013" in frag_ids, f"Expected ch0013 in fragment IDs, got {frag_ids}"

    # Verify only chapter fragments exist
    ch_ids = [fid for fid in frag_ids if fid.startswith("ch")]
    assert len(ch_ids) == 1, f"Expected exactly 1 chapter fragment, got {ch_ids}"

    # Part should exist inline in Book.XML with correct ID
    part_in_book = book_root.find(".//part")
    assert part_in_book is not None, "Part should be inline in Book.XML"
    assert part_in_book.get("id") == "pt0012", f"Part ID should be pt0012, got {part_in_book.get('id')}"

