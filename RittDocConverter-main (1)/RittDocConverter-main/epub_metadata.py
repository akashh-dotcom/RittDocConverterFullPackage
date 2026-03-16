"""
EPUB Metadata Extraction Module

Extracts and processes metadata from EPUB files, supporting both EPUB 2 and EPUB 3 formats.
This module handles:
- ISBN extraction (from filename and OPF)
- Dublin Core metadata (title, author, publisher, etc.)
- EPUB 3 meta properties
- RittDoc bookinfo element generation

Usage:
    from epub_metadata import extract_metadata, get_metadata_value

    bookinfo, metadata_dict = extract_metadata(book, epub_path)
"""

import logging
import re
import zipfile
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple

from lxml import etree

try:
    import ebooklib
    from ebooklib import epub
    HAS_EBOOKLIB = True
except ImportError:
    HAS_EBOOKLIB = False

logger = logging.getLogger(__name__)


# =============================================================================
# ISBN Extraction
# =============================================================================

def extract_isbn_from_filename(epub_path: Optional[Path]) -> Optional[str]:
    """
    Extract an ISBN from the EPUB filename.

    We prefer a 13-digit ISBN if present; otherwise fall back to 10-digit.
    Returns only the digit/X sequence (no hyphens).

    Args:
        epub_path: Path to the EPUB file

    Returns:
        ISBN string or None if not found
    """
    if not epub_path:
        return None

    stem = epub_path.stem.upper()
    if not stem:
        return None

    # Remove separators and non-ISBN characters
    cleaned = re.sub(r'[^0-9X]', '', stem)
    if not cleaned:
        return None

    # Prefer 13-digit ISBNs
    match = re.search(r'\d{13}', cleaned)
    if match:
        return match.group(0)

    # Fallback: ISBN-10 (allows X as check digit)
    match = re.search(r'\d{9}[0-9X]', cleaned)
    if match:
        return match.group(0)

    return None


def extract_isbn_from_opf(opf_root: etree._Element) -> Optional[str]:
    """
    Extract ISBN from OPF XML document.

    Args:
        opf_root: Parsed OPF XML root element

    Returns:
        ISBN string or None if not found
    """
    namespaces = {
        'opf': 'http://www.idpf.org/2007/opf',
        'dc': 'http://purl.org/dc/elements/1.1/',
    }

    # Look for <dc:identifier opf:scheme="ISBN">
    isbn_elements = opf_root.xpath(
        '//dc:identifier[@opf:scheme="ISBN"]',
        namespaces=namespaces
    )

    if not isbn_elements:
        # Try without namespace prefix
        isbn_elements = opf_root.xpath('//identifier[@opf:scheme="ISBN"]')

    if isbn_elements and isbn_elements[0].text:
        isbn_value = isbn_elements[0].text.strip()
        logger.info(f"Found ISBN via OPF XML parsing: {isbn_value}")
        return isbn_value

    return None


# =============================================================================
# OPF XML Parsing
# =============================================================================

def get_opf_xml_from_epub(epub_path: Path) -> Optional[etree._Element]:
    """
    Extract and parse the OPF XML file from an EPUB.

    Args:
        epub_path: Path to the EPUB file

    Returns:
        Parsed OPF XML root element or None
    """
    try:
        with zipfile.ZipFile(epub_path, 'r') as zf:
            # Read container.xml to find OPF location
            container_xml = zf.read('META-INF/container.xml')
            container_root = etree.fromstring(container_xml)

            # Find OPF path in container
            namespaces = {'container': 'urn:oasis:names:tc:opendocument:xmlns:container'}
            rootfiles = container_root.xpath(
                '//container:rootfile[@media-type="application/oebps-package+xml"]',
                namespaces=namespaces
            )

            if not rootfiles:
                # Try without namespace
                rootfiles = container_root.xpath('//rootfile[@media-type="application/oebps-package+xml"]')

            if rootfiles:
                opf_path = rootfiles[0].get('full-path')
                if opf_path:
                    # Read OPF file
                    opf_content = zf.read(opf_path)
                    opf_root = etree.fromstring(opf_content)
                    logger.debug(f"Successfully parsed OPF XML from {opf_path}")
                    return opf_root
    except Exception as e:
        logger.debug(f"Could not extract OPF XML from EPUB: {e}")

    return None


def get_opf_path_and_dir(epub_path: Path) -> Tuple[Optional[str], Optional[str]]:
    """
    Get the OPF file path and its directory from an EPUB.

    Args:
        epub_path: Path to the EPUB file

    Returns:
        Tuple of (opf_path, opf_directory) or (None, None)
    """
    try:
        with zipfile.ZipFile(epub_path, 'r') as zf:
            container_xml = zf.read('META-INF/container.xml')
            container_root = etree.fromstring(container_xml)

            namespaces = {'container': 'urn:oasis:names:tc:opendocument:xmlns:container'}
            rootfiles = container_root.xpath(
                '//container:rootfile[@media-type="application/oebps-package+xml"]',
                namespaces=namespaces
            )

            if not rootfiles:
                rootfiles = container_root.xpath('//rootfile[@media-type="application/oebps-package+xml"]')

            if rootfiles:
                opf_path = rootfiles[0].get('full-path')
                if opf_path:
                    opf_dir = str(Path(opf_path).parent)
                    if opf_dir == '.':
                        opf_dir = ''
                    return opf_path, opf_dir
    except Exception as e:
        logger.debug(f"Could not get OPF path: {e}")

    return None, None


# =============================================================================
# Metadata Extraction
# =============================================================================

def get_metadata_value(
    book: 'epub.EpubBook',
    metadata_name: str,
    epub_path: Optional[Path] = None
) -> Optional[str]:
    """
    Robustly extract metadata from EPUB, supporting both EPUB 2 and EPUB 3 formats.

    EPUB 2 uses: <dc:publisher>Publisher Name</dc:publisher>
    EPUB 3 uses: <meta property="dcterms:publisher">Publisher Name</meta>

    This function tries multiple methods:
    1. Standard Dublin Core metadata (book.get_metadata('DC', ...))
    2. OPF custom metadata (book.get_metadata('OPF', ...))
    3. Direct OPF XML parsing for <meta property="..."> elements (EPUB 3)

    Args:
        book: EpubBook instance
        metadata_name: Name of metadata field (e.g., 'publisher', 'subtitle')
        epub_path: Optional path to EPUB file for direct XML parsing

    Returns:
        Metadata value or None if not found
    """
    if not HAS_EBOOKLIB:
        logger.warning("ebooklib not available for metadata extraction")
        return None

    # Method 1: Try standard Dublin Core
    try:
        values = book.get_metadata('DC', metadata_name)
        if values and len(values) > 0:
            value = values[0][0]
            if value and value.strip():
                logger.debug(f"Found {metadata_name} via DC: {value}")
                return value.strip()
    except (KeyError, IndexError, AttributeError, TypeError) as e:
        logger.debug(f"DC metadata extraction for '{metadata_name}' failed: {e}")

    # Method 2: Try OPF custom metadata
    try:
        values = book.get_metadata('OPF', metadata_name)
        if values and len(values) > 0:
            value = values[0][0] if values[0][0] else None
            if value and value.strip():
                logger.debug(f"Found {metadata_name} via OPF: {value}")
                return value.strip()
    except (KeyError, IndexError, AttributeError, TypeError) as e:
        logger.debug(f"OPF metadata extraction for '{metadata_name}' failed: {e}")

    # Method 3: Parse OPF XML directly for EPUB 3 meta properties
    if epub_path:
        try:
            opf_root = get_opf_xml_from_epub(epub_path)
            if opf_root is not None:
                namespaces = {
                    'opf': 'http://www.idpf.org/2007/opf',
                    'dc': 'http://purl.org/dc/elements/1.1/',
                }

                # Try different property name variations for EPUB 3
                property_names = [
                    f'dcterms:{metadata_name}',
                    f'schema:{metadata_name}',
                    metadata_name,
                ]

                for prop_name in property_names:
                    meta_elements = opf_root.xpath(
                        f'//opf:meta[@property="{prop_name}"]',
                        namespaces=namespaces
                    )

                    if not meta_elements:
                        meta_elements = opf_root.xpath(f'//meta[@property="{prop_name}"]')

                    if meta_elements and meta_elements[0].text:
                        value = meta_elements[0].text.strip()
                        if value:
                            logger.info(
                                f"Found {metadata_name} via EPUB 3 OPF XML "
                                f"property '{prop_name}': {value}"
                            )
                            return value
        except Exception as e:
            logger.debug(f"Could not parse OPF XML for {metadata_name}: {e}")

    logger.debug(f"Could not find metadata: {metadata_name}")
    return None


def extract_metadata(
    book: 'epub.EpubBook',
    epub_path: Optional[Path] = None,
    validated_subelement: Optional[callable] = None
) -> Tuple[etree._Element, Dict[str, Any]]:
    """
    Extract EPUB metadata and convert to RittDoc <bookinfo> element.
    Supports both EPUB 2 and EPUB 3 metadata formats.

    Args:
        book: EpubBook instance
        epub_path: Optional path to EPUB file for robust EPUB 3 metadata extraction
        validated_subelement: Optional function to create validated subelements

    Returns:
        Tuple of (bookinfo Element, metadata dict for tracking)
    """
    if not HAS_EBOOKLIB:
        bookinfo = etree.Element('bookinfo')
        return bookinfo, {'error': 'ebooklib not available'}

    # Use simple subelement if no validator provided
    if validated_subelement is None:
        validated_subelement = etree.SubElement

    bookinfo = etree.Element('bookinfo')
    metadata_dict: Dict[str, Any] = {}

    # ISBN - prefer filename ISBN, fall back to metadata identifiers
    isbn_found = False
    isbn_value = extract_isbn_from_filename(epub_path)
    fallback_identifier = None

    if isbn_value:
        logger.info(f"Using ISBN from filename: {isbn_value}")
    else:
        identifiers = book.get_metadata('DC', 'identifier')

        # Pass 1: Look for identifiers explicitly marked as ISBN
        for identifier_tuple in identifiers:
            identifier_value = identifier_tuple[0]
            identifier_attrs = identifier_tuple[1] if len(identifier_tuple) > 1 else {}

            # Check if this identifier has opf:scheme="ISBN"
            scheme = identifier_attrs.get('opf:scheme', '').upper()
            if scheme == 'ISBN':
                isbn_clean = re.sub(r'^(urn:)?isbn:', '', identifier_value, flags=re.IGNORECASE)
                isbn_value = isbn_clean.strip()
                logger.info(f"Found ISBN via opf:scheme attribute: {isbn_value}")
                break

            # Check if identifier starts with "urn:isbn:" or "isbn:" prefix
            if re.match(r'^(urn:)?isbn:', identifier_value, flags=re.IGNORECASE):
                isbn_clean = re.sub(r'^(urn:)?isbn:', '', identifier_value, flags=re.IGNORECASE)
                if re.search(r'\d', isbn_clean):
                    isbn_value = isbn_clean.strip()
                    logger.info(f"Found ISBN via urn:isbn or isbn prefix: {isbn_value}")
                    break

            # Keep track of first numeric identifier as fallback
            if fallback_identifier is None and re.search(r'\d', identifier_value):
                fallback_identifier = identifier_value

        # Pass 2: Try direct OPF XML parsing for EPUB 3 format
        if isbn_value is None and epub_path:
            opf_root = get_opf_xml_from_epub(epub_path)
            if opf_root is not None:
                isbn_value = extract_isbn_from_opf(opf_root)

    # Use found ISBN or fall back to first numeric identifier
    if isbn_value:
        isbn_elem = validated_subelement(bookinfo, 'isbn')
        isbn_elem.text = isbn_value
        metadata_dict['isbn'] = isbn_value
        isbn_found = True
    elif fallback_identifier:
        isbn_clean = re.sub(r'^(urn:)?isbn:', '', fallback_identifier, flags=re.IGNORECASE)
        isbn_elem = validated_subelement(bookinfo, 'isbn')
        isbn_elem.text = isbn_clean.strip()
        metadata_dict['isbn'] = isbn_clean.strip()
        isbn_found = True
        logger.warning(f"Using fallback identifier as ISBN: {isbn_clean.strip()}")

    if not isbn_found:
        isbn_elem = validated_subelement(bookinfo, 'isbn')
        isbn_elem.text = 'UNKNOWN'
        metadata_dict['isbn'] = 'UNKNOWN'

    # Title
    titles = book.get_metadata('DC', 'title')
    if titles:
        title_elem = validated_subelement(bookinfo, 'title')
        title_elem.text = titles[0][0]
        metadata_dict['title'] = titles[0][0]

    # Subtitle
    subtitle = get_metadata_value(book, 'subtitle', epub_path)
    if subtitle:
        subtitle_elem = validated_subelement(bookinfo, 'subtitle')
        subtitle_elem.text = subtitle
        metadata_dict['subtitle'] = subtitle

    # Author(s)
    creators = book.get_metadata('DC', 'creator')
    authors = []
    if creators:
        authorgroup = validated_subelement(bookinfo, 'authorgroup')
        for creator_tuple in creators:
            author_elem = validated_subelement(authorgroup, 'author')
            personname = validated_subelement(author_elem, 'personname')

            # Parse name: "FirstName LastName" or "LastName, FirstName"
            name = creator_tuple[0].strip()
            authors.append(name)

            if ', ' in name:
                last, first = name.split(', ', 1)
            else:
                parts = name.rsplit(' ', 1)
                first = parts[0] if len(parts) > 1 else ''
                last = parts[-1] if len(parts) > 0 else name

            if first:
                firstname = validated_subelement(personname, 'firstname')
                firstname.text = first.strip()
            if last:
                surname = validated_subelement(personname, 'surname')
                surname.text = last.strip()

    metadata_dict['authors'] = authors

    # Publisher
    publisher_name = get_metadata_value(book, 'publisher', epub_path)
    if publisher_name:
        publisher = validated_subelement(bookinfo, 'publisher')
        publishername = validated_subelement(publisher, 'publishername')
        publishername.text = publisher_name
        metadata_dict['publisher'] = publisher_name
        logger.info(f"Found publisher: {publisher_name}")
    else:
        logger.warning("Publisher metadata not found in EPUB")

    # Publication date
    dates = book.get_metadata('DC', 'date')
    if dates:
        pubdate = validated_subelement(bookinfo, 'pubdate')
        date_str = dates[0][0]
        year_match = re.search(r'\d{4}', date_str)
        if year_match:
            pubdate.text = year_match.group(0)
        else:
            pubdate.text = date_str

    # Language
    languages = book.get_metadata('DC', 'language')
    if languages:
        language_elem = validated_subelement(bookinfo, 'language')
        language_elem.text = languages[0][0]
        metadata_dict['language'] = languages[0][0]

    # Copyright/Rights
    rights = book.get_metadata('DC', 'rights')
    if rights:
        copyright_elem = validated_subelement(bookinfo, 'copyright')
        rights_text = rights[0][0]
        year_match = re.search(r'\d{4}', rights_text)
        if year_match:
            year_elem = validated_subelement(copyright_elem, 'year')
            year_elem.text = year_match.group(0)
        holder_elem = validated_subelement(copyright_elem, 'holder')
        holder_elem.text = rights_text

    return bookinfo, metadata_dict


# =============================================================================
# Publisher Detection
# =============================================================================

# Known publisher patterns for detection
PUBLISHER_PATTERNS = {
    'wiley': [
        'wiley', 'john wiley', 'wiley-blackwell', 'wiley-vch', 'jossey-bass'
    ],
    'springer': [
        'springer', 'springer nature', 'springer-verlag'
    ],
    'elsevier': [
        'elsevier', 'academic press', 'butterworth-heinemann', 'morgan kaufmann'
    ],
    'pearson': [
        'pearson', 'prentice hall', 'addison-wesley', 'informit'
    ],
    'mcgraw-hill': [
        'mcgraw-hill', 'mcgraw hill', 'mgh'
    ],
    'oxford': [
        'oxford university press', 'oxford', 'oup'
    ],
    'cambridge': [
        'cambridge university press', 'cambridge'
    ],
    'routledge': [
        'routledge', 'taylor & francis', 'taylor and francis', 'crc press'
    ],
}


def detect_publisher(
    book: Optional['epub.EpubBook'] = None,
    epub_path: Optional[Path] = None,
    metadata_dict: Optional[Dict[str, Any]] = None
) -> Optional[str]:
    """
    Detect the publisher from EPUB metadata.

    Args:
        book: EpubBook instance
        epub_path: Path to EPUB file
        metadata_dict: Pre-extracted metadata dictionary

    Returns:
        Publisher key (e.g., 'wiley', 'springer') or None
    """
    publisher_name = None

    # Try metadata dict first
    if metadata_dict and 'publisher' in metadata_dict:
        publisher_name = metadata_dict['publisher']

    # Try extracting from book
    if not publisher_name and book:
        publisher_name = get_metadata_value(book, 'publisher', epub_path)

    if not publisher_name:
        return None

    # Normalize for matching
    publisher_lower = publisher_name.lower()

    # Check against known patterns
    for publisher_key, patterns in PUBLISHER_PATTERNS.items():
        for pattern in patterns:
            if pattern in publisher_lower:
                logger.info(f"Detected publisher: {publisher_key} (from '{publisher_name}')")
                return publisher_key

    logger.debug(f"Unknown publisher: {publisher_name}")
    return None


# =============================================================================
# Utility Functions
# =============================================================================

def get_spine_items(book: 'epub.EpubBook') -> List[Any]:
    """
    Get ordered list of spine items from EPUB.

    Args:
        book: EpubBook instance

    Returns:
        List of spine item IDs in reading order
    """
    if not HAS_EBOOKLIB:
        return []

    spine_items = []
    for item_id, linear in book.spine:
        item = book.get_item_with_id(item_id)
        if item:
            spine_items.append(item)

    return spine_items


def get_document_items(book: 'epub.EpubBook') -> List[Any]:
    """
    Get all document (XHTML) items from EPUB.

    Args:
        book: EpubBook instance

    Returns:
        List of document items
    """
    if not HAS_EBOOKLIB:
        return []

    return list(book.get_items_of_type(ebooklib.ITEM_DOCUMENT))


def get_image_items(book: 'epub.EpubBook') -> List[Any]:
    """
    Get all image items from EPUB.

    Args:
        book: EpubBook instance

    Returns:
        List of image items
    """
    if not HAS_EBOOKLIB:
        return []

    return list(book.get_items_of_type(ebooklib.ITEM_IMAGE))
