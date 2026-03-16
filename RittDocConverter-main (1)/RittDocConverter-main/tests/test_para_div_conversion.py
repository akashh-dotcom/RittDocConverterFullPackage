import re

from lxml import etree

from epub_to_structured_v2 import convert_xhtml_to_chapter
from reference_mapper import get_mapper, reset_mapper


def _text_content(elem: etree.Element) -> str:
    if elem is None:
        return ""
    text = "".join(elem.itertext())
    return re.sub(r"\s+", " ", text).strip()


def test_para_div_preserves_text_around_blocks() -> None:
    reset_mapper()
    mapper = get_mapper()

    xhtml = b"""<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
  <head><title>Dummy</title></head>
  <body>
    <h1 id="c1">Chapter Title</h1>
    <section>
      <h2 id="sec1">Section Title</h2>
      <div class="Para" id="Par13">
        Protease inhibitors (see Fig. <span class="InternalRef"><a href="#Fig2">6.2</a></span>)
        prevent degradation of proteins.
        <figure id="Fig2">
          <img src="../images/fig2.png" alt="Alt text" />
          <figcaption><p>Fig. 6.2 caption</p></figcaption>
        </figure>
        Additional note after figure.
      </div>
    </section>
  </body>
</html>
"""

    chapter = convert_xhtml_to_chapter(
        xhtml_content=xhtml,
        doc_path="OEBPS/Text/ch0001.xhtml",
        chapter_id="ch0001",
        mapper=mapper,
        toc_depth_map=None,
        element_type="chapter",
        matter_category=None,
    )

    paras = [_text_content(p) for p in chapter.xpath(".//para")]

    assert any("Protease inhibitors (see Fig. 6.2) prevent degradation of proteins." in p for p in paras)
    assert any("Additional note after figure." in p for p in paras)
