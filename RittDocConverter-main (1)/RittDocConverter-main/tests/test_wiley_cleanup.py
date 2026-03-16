from lxml import etree


def _make_chapter(book: etree._Element, title: str, role: str = "") -> etree._Element:
    chapter = etree.SubElement(book, "chapter")
    if role:
        chapter.set("role", role)
    etree.SubElement(chapter, "title").text = title
    etree.SubElement(chapter, "para").text = "Content"
    return chapter


def test_wiley_cleanup_removes_eula_and_index_fragments() -> None:
    from package import _filter_wiley_fragments, _split_root

    book = etree.Element("book")
    book.set("id", "b001")

    _make_chapter(book, "End User License Agreement")
    _make_chapter(book, "Index of Subjects")
    _make_chapter(book, "Chapter 1")

    book_root, fragments = _split_root(book)

    filtered, removed, removed_refs = _filter_wiley_fragments(book_root, fragments)
    removed_entities = {frag.entity for frag in removed}

    assert len(removed) == 2
    assert len(filtered) == 1
    assert removed_refs == len(removed_entities)

    for node in book_root.iter():
        if isinstance(node, etree._Entity):
            assert node.name not in removed_entities
