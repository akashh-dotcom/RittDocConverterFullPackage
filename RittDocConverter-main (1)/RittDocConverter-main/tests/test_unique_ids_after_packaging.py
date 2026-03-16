from lxml import etree


def test_no_duplicate_ids_across_book_and_fragments_after_split():
    """
    Fast safety net: IDs in chapter fragments must not collide with each other
    or with IDs in Book.XML.

    Note: Part/subpart elements are only emitted inline in Book.XML (not as
    separate fragment files) because the DocBook DTD requires parts to have
    child structural elements, which are referenced via entities only declared
    in Book.XML.
    """
    from package import (
        _auto_group_chapters_into_volumes_and_sections,
        _split_root,
    )

    book = etree.Element("book")
    book.set("id", "b001")

    # Volume marker (becomes <part role="volume">)
    v = etree.SubElement(book, "chapter")
    v.set("id", "ch0006")
    etree.SubElement(v, "title").text = "VOLUME 1"

    # Section marker inside volume (becomes <subpart role="section">)
    s = etree.SubElement(book, "chapter")
    s.set("id", "ch0012")
    etree.SubElement(s, "title").text = "SECTION 1 Example Section"

    # Regular chapter content
    c = etree.SubElement(book, "chapter")
    c.set("id", "ch0013")
    etree.SubElement(c, "title").text = "Chapter 1"

    assert _auto_group_chapters_into_volumes_and_sections(book) is True

    book_root, fragments = _split_root(book)

    ids: list[str] = []
    # IDs in Book.XML tree
    for elem in book_root.iter():
        if not isinstance(elem.tag, str):
            continue
        val = (elem.get("id") or "").strip()
        if val:
            ids.append(val)

    # IDs in fragment roots (chapter files)
    for frag in fragments:
        val = (frag.element.get("id") or "").strip()
        if val:
            ids.append(val)

    # All IDs should be unique (no duplicates expected)
    seen = set()
    dupes = sorted({x for x in ids if (x in seen) or (seen.add(x) or False)})
    assert dupes == [], f"Duplicate IDs found: {dupes}"

