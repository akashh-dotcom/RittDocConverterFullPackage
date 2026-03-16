#!/usr/bin/env python3
"""
Test that cross-file linkend references are converted to <ulink url="...">.

When chapter files are validated standalone against the DTD, linkend (IDREF)
values must reference an id in the SAME document.  Cross-file linkends are
therefore converted to <ulink url="target.xml#id"> during packaging.
"""

from lxml import etree

from package import ChapterFragment, _convert_cross_file_linkends_to_ulink


def _make_fragment(entity: str, filename: str, xml_str: str) -> ChapterFragment:
    """Helper to create a ChapterFragment from an XML string."""
    elem = etree.fromstring(xml_str)
    return ChapterFragment(entity=entity, filename=filename, element=elem)


def test_cross_file_link_converted_to_ulink():
    """A <link linkend="ch0002s0001"> in ch0001 should become a <ulink>."""
    frag1 = _make_fragment(
        "ch0001", "ch0001.xml",
        '<chapter id="ch0001">'
        '  <title>Chapter 1</title>'
        '  <sect1 id="ch0001s0001">'
        '    <title>Section</title>'
        '    <para>See <link linkend="ch0002s0001">other chapter</link>.</para>'
        '  </sect1>'
        '</chapter>',
    )
    frag2 = _make_fragment(
        "ch0002", "ch0002.xml",
        '<chapter id="ch0002">'
        '  <title>Chapter 2</title>'
        '  <sect1 id="ch0002s0001">'
        '    <title>Target</title>'
        '    <para>Content here.</para>'
        '  </sect1>'
        '</chapter>',
    )

    _convert_cross_file_linkends_to_ulink([frag1, frag2])

    # The <link> in ch0001 should now be <ulink url="ch0002.xml#ch0002s0001">
    ulinks = frag1.element.findall(".//ulink")
    assert len(ulinks) == 1, f"Expected 1 ulink, got {len(ulinks)}"
    assert ulinks[0].get("url") == "ch0002.xml#ch0002s0001"
    assert ulinks[0].text == "other chapter"

    # ch0002 should be untouched (no cross-file refs)
    assert frag2.element.findall(".//ulink") == []


def test_same_file_linkend_not_converted():
    """A linkend referencing an id in the same file should remain a <link>."""
    frag = _make_fragment(
        "ch0001", "ch0001.xml",
        '<chapter id="ch0001">'
        '  <title>Chapter 1</title>'
        '  <sect1 id="ch0001s0001">'
        '    <title>Section</title>'
        '    <para>See <link linkend="ch0001s0001">above</link>.</para>'
        '  </sect1>'
        '</chapter>',
    )

    _convert_cross_file_linkends_to_ulink([frag])

    # Should still be a <link>, not converted
    links = frag.element.findall(".//link")
    assert len(links) == 1
    assert links[0].get("linkend") == "ch0001s0001"
    assert frag.element.findall(".//ulink") == []


def test_xref_converted_with_text():
    """An <xref linkend="..."/> cross-file should become <ulink> with text."""
    frag1 = _make_fragment(
        "pr0002", "pr0002.xml",
        '<preface id="pr0002">'
        '  <title>Preface</title>'
        '  <sect1 id="pr0002s0001">'
        '    <title>Intro</title>'
        '    <para>See <xref linkend="ch0031s0001fg01"/>.</para>'
        '  </sect1>'
        '</preface>',
    )
    frag2 = _make_fragment(
        "ch0031", "ch0031.xml",
        '<chapter id="ch0031">'
        '  <title>Chapter 31</title>'
        '  <sect1 id="ch0031s0001">'
        '    <title>Section</title>'
        '    <figure id="ch0031s0001fg01"><title>Fig</title></figure>'
        '  </sect1>'
        '</chapter>',
    )

    _convert_cross_file_linkends_to_ulink([frag1, frag2])

    ulinks = frag1.element.findall(".//ulink")
    assert len(ulinks) == 1
    assert ulinks[0].get("url") == "ch0031.xml#ch0031s0001fg01"
    # xref has no text; function uses linkend as text placeholder
    assert ulinks[0].text == "ch0031s0001fg01"


def test_unknown_target_inferred_from_prefix():
    """When the target id isn't found in any fragment, infer filename from prefix."""
    frag = _make_fragment(
        "pr0002", "pr0002.xml",
        '<preface id="pr0002">'
        '  <title>Preface</title>'
        '  <sect1 id="pr0002s0001">'
        '    <title>Intro</title>'
        '    <para>See <link linkend="ch0099s0001">chapter 99</link>.</para>'
        '  </sect1>'
        '</preface>',
    )

    _convert_cross_file_linkends_to_ulink([frag])

    ulinks = frag.element.findall(".//ulink")
    assert len(ulinks) == 1
    assert ulinks[0].get("url") == "ch0099.xml#ch0099s0001"
    assert ulinks[0].text == "chapter 99"


def test_preserves_tail_text():
    """The tail text after the original element should be preserved on the ulink."""
    frag1 = _make_fragment(
        "ch0001", "ch0001.xml",
        '<chapter id="ch0001">'
        '  <title>Chapter 1</title>'
        '  <sect1 id="ch0001s0001">'
        '    <title>Section</title>'
        '    <para>See <link linkend="ch0002s0001">ref</link> for details.</para>'
        '  </sect1>'
        '</chapter>',
    )
    frag2 = _make_fragment(
        "ch0002", "ch0002.xml",
        '<chapter id="ch0002">'
        '  <sect1 id="ch0002s0001"><title>T</title><para>x</para></sect1>'
        '</chapter>',
    )

    _convert_cross_file_linkends_to_ulink([frag1, frag2])

    ulinks = frag1.element.findall(".//ulink")
    assert len(ulinks) == 1
    assert ulinks[0].tail == " for details."
