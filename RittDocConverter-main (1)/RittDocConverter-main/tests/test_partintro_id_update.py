"""Tests for partintro ID update when converting chapters to parts/subparts."""

from lxml import etree
from package import (
    _convert_chapter_to_part,
    _convert_chapter_to_subpart,
    _update_partintro_ids,
    _apply_id_mappings_globally,
)


def test_update_partintro_ids_updates_sect1_ids():
    """Test that sect1 IDs inside partintro are updated from ch#### to pt#### format."""
    partintro = etree.Element("partintro")
    sect1 = etree.SubElement(partintro, "sect1")
    sect1.set("id", "ch0012s0001")
    title = etree.SubElement(sect1, "title")
    title.text = "Section Title"
    para = etree.SubElement(sect1, "para")
    para.set("id", "ch0012s0001p0001")
    para.text = "Some content"

    id_mappings = _update_partintro_ids(partintro, "ch0012", "pt0012")

    assert len(id_mappings) == 2
    assert id_mappings["ch0012s0001"] == "pt0012s0001"
    assert id_mappings["ch0012s0001p0001"] == "pt0012s0001p0001"
    assert sect1.get("id") == "pt0012s0001"
    assert para.get("id") == "pt0012s0001p0001"


def test_update_partintro_ids_updates_linkend():
    """Test that linkend attributes are updated within partintro."""
    partintro = etree.Element("partintro")
    para = etree.SubElement(partintro, "para")
    para.set("id", "ch0012s0001p0001")  # Has an ID to track
    link = etree.SubElement(para, "link")
    link.set("linkend", "ch0012s0001fg0001")
    link.text = "See figure"

    id_mappings = _update_partintro_ids(partintro, "ch0012", "pt0012")

    # Only IDs are tracked in mappings, linkends within partintro are updated but not tracked
    assert "ch0012s0001p0001" in id_mappings
    assert link.get("linkend") == "pt0012s0001fg0001"


def test_update_partintro_ids_updates_url():
    """Test that url attributes are updated within partintro."""
    partintro = etree.Element("partintro")
    para = etree.SubElement(partintro, "para")
    para.set("id", "ch0012s0001p0001")  # Has an ID to track
    ulink = etree.SubElement(para, "ulink")
    ulink.set("url", "ch0012s0001")
    ulink.text = "Link text"

    id_mappings = _update_partintro_ids(partintro, "ch0012", "pt0012")

    assert "ch0012s0001p0001" in id_mappings
    assert ulink.get("url") == "pt0012s0001"


def test_update_partintro_ids_same_prefix_no_updates():
    """Test that no updates occur when old and new prefix are the same."""
    partintro = etree.Element("partintro")
    sect1 = etree.SubElement(partintro, "sect1")
    sect1.set("id", "pt0012s0001")

    id_mappings = _update_partintro_ids(partintro, "pt0012", "pt0012")

    assert id_mappings == {}
    assert sect1.get("id") == "pt0012s0001"


def test_convert_chapter_to_part_updates_partintro_ids():
    """Test that _convert_chapter_to_part updates IDs inside partintro."""
    chapter = etree.Element("chapter")
    chapter.set("id", "ch0012")
    title = etree.SubElement(chapter, "title")
    title.text = "Part I: Introduction"
    sect1 = etree.SubElement(chapter, "sect1")
    sect1.set("id", "ch0012s0001")
    sect1_title = etree.SubElement(sect1, "title")
    sect1_title.text = "Overview"
    para = etree.SubElement(sect1, "para")
    para.set("id", "ch0012s0001p0001")
    para.text = "Introduction content"

    part, id_mappings = _convert_chapter_to_part(chapter)

    assert part.tag == "part"
    assert part.get("id") == "pt0012"

    # Check ID mappings returned for global cross-reference updates
    assert "ch0012s0001" in id_mappings
    assert id_mappings["ch0012s0001"] == "pt0012s0001"
    assert "ch0012s0001p0001" in id_mappings
    assert id_mappings["ch0012s0001p0001"] == "pt0012s0001p0001"

    partintro = part.find("partintro")
    assert partintro is not None

    sect1_in_partintro = partintro.find("sect1")
    assert sect1_in_partintro is not None
    assert sect1_in_partintro.get("id") == "pt0012s0001"

    para_in_partintro = sect1_in_partintro.find("para")
    assert para_in_partintro is not None
    assert para_in_partintro.get("id") == "pt0012s0001p0001"


def test_convert_chapter_to_subpart_updates_partintro_ids():
    """Test that _convert_chapter_to_subpart updates IDs inside partintro."""
    chapter = etree.Element("chapter")
    chapter.set("id", "ch0015")
    title = etree.SubElement(chapter, "title")
    title.text = "Section 1: Basics"
    sect1 = etree.SubElement(chapter, "sect1")
    sect1.set("id", "ch0015s0001")
    sect1_title = etree.SubElement(sect1, "title")
    sect1_title.text = "Getting Started"
    figure = etree.SubElement(sect1, "figure")
    figure.set("id", "ch0015s0001fg0001")

    subpart, id_mappings = _convert_chapter_to_subpart(chapter)

    assert subpart.tag == "subpart"
    assert subpart.get("id") == "sp0015"

    # Check ID mappings returned
    assert "ch0015s0001" in id_mappings
    assert "ch0015s0001fg0001" in id_mappings

    partintro = subpart.find("partintro")
    assert partintro is not None

    sect1_in_partintro = partintro.find("sect1")
    assert sect1_in_partintro is not None
    assert sect1_in_partintro.get("id") == "sp0015s0001"

    figure_in_partintro = sect1_in_partintro.find("figure")
    assert figure_in_partintro is not None
    assert figure_in_partintro.get("id") == "sp0015s0001fg0001"


def test_convert_chapter_to_part_no_partintro_when_empty():
    """Test that no partintro is created when chapter has only title."""
    chapter = etree.Element("chapter")
    chapter.set("id", "ch0010")
    title = etree.SubElement(chapter, "title")
    title.text = "Part II: Advanced Topics"

    part, id_mappings = _convert_chapter_to_part(chapter)

    assert part.tag == "part"
    assert part.get("id") == "pt0010"
    assert part.find("partintro") is None
    assert id_mappings == {}


def test_convert_chapter_to_part_preserves_non_ch_ids():
    """Test that IDs not matching ch#### pattern are preserved."""
    chapter = etree.Element("chapter")
    chapter.set("id", "custom-id")  # Not ch#### format
    title = etree.SubElement(chapter, "title")
    title.text = "Part III"
    para = etree.SubElement(chapter, "para")
    para.set("id", "my-custom-para")
    para.text = "Content"

    part, id_mappings = _convert_chapter_to_part(chapter)

    # ID should be preserved as-is since it doesn't match ch#### pattern
    assert part.get("id") == "custom-id"
    assert id_mappings == {}  # No mappings since prefix didn't match

    partintro = part.find("partintro")
    assert partintro is not None
    para_in_partintro = partintro.find("para")
    assert para_in_partintro.get("id") == "my-custom-para"  # Unchanged


def test_apply_id_mappings_globally():
    """Test that _apply_id_mappings_globally updates cross-references in other chapters."""
    root = etree.Element("book")

    # Part with updated IDs
    part = etree.SubElement(root, "part")
    part.set("id", "pt0012")
    partintro = etree.SubElement(part, "partintro")
    sect1 = etree.SubElement(partintro, "sect1")
    sect1.set("id", "pt0012s0001")

    # Chapter with cross-references using OLD IDs
    chapter = etree.SubElement(part, "chapter")
    chapter.set("id", "ch0013")
    para = etree.SubElement(chapter, "para")
    link = etree.SubElement(para, "link")
    link.set("linkend", "ch0012s0001")  # OLD ID
    link.text = "See introduction"

    ulink = etree.SubElement(para, "ulink")
    ulink.set("url", "ch0012s0001")  # OLD ID
    ulink.text = "Link"

    # Apply mappings
    id_mappings = {"ch0012s0001": "pt0012s0001"}
    updates = _apply_id_mappings_globally(root, id_mappings)

    assert updates == 2
    assert link.get("linkend") == "pt0012s0001"  # Updated
    assert ulink.get("url") == "pt0012s0001"  # Updated


def test_apply_id_mappings_globally_no_mappings():
    """Test that _apply_id_mappings_globally returns 0 when no mappings provided."""
    root = etree.Element("book")
    chapter = etree.SubElement(root, "chapter")
    link = etree.SubElement(chapter, "link")
    link.set("linkend", "ch0012s0001")

    updates = _apply_id_mappings_globally(root, {})

    assert updates == 0
    assert link.get("linkend") == "ch0012s0001"  # Unchanged
