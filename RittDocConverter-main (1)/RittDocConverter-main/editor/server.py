#!/usr/bin/env python3
"""
RittDoc Editor - Flask Backend Server
A web-based editor for PDF, XML, and HTML content with synchronized viewing.
"""

import base64
import glob
import logging
import os
import re
import shutil
import sys
import tempfile
import zipfile
from pathlib import Path

import requests
from flask import (Flask, jsonify, render_template, request, send_file,
                   send_from_directory)
from lxml import etree

# Add parent directory to path for imports
sys.path.insert(0, str(Path(__file__).resolve().parent.parent))

# Import reprocessing module
try:
    from reprocess import reprocess_package
    REPROCESS_AVAILABLE = True
except ImportError:
    REPROCESS_AVAILABLE = False
    print("Warning: reprocess module not available. Save will not trigger reprocessing.")

logger = logging.getLogger(__name__)

# UI Backend webhook configuration
UI_BACKEND_URL = os.environ.get('UI_BACKEND_URL', 'http://demo-ui-backend:3001')
WEBHOOK_ENDPOINT = '/api/files/webhook/complete'

# API server URL for download URLs (used in webhook payloads)
# In Docker: http://epub-converter-api:5001
# Local: http://localhost:5001
API_SERVER_URL = os.environ.get('EPUB_API_URL', 'http://epub-converter-api:5001')


def extract_isbn_from_package(zip_path: Path, extracted_dir: Path = None) -> str:
    """Extract ISBN from package filename or metadata."""
    # Try to extract from filename first (common pattern: ISBN_all_fixes.zip)
    filename = zip_path.stem

    # Remove common suffixes
    for suffix in ['_all_fixes', '_fixed', '_final', '_v2', '_v1']:
        if filename.endswith(suffix):
            filename = filename[:-len(suffix)]

    # Check if filename looks like an ISBN (10 or 13 digits, possibly with hyphens)
    isbn_pattern = r'^[\d\-]{10,17}$'
    if re.match(isbn_pattern, filename.replace('-', '')):
        return filename

    # Try to extract from Book.XML metadata if extracted_dir is available
    if extracted_dir:
        book_xml = extracted_dir / 'Book.XML'
        if not book_xml.exists():
            # Try case-insensitive
            for f in extracted_dir.iterdir():
                if f.name.lower() == 'book.xml':
                    book_xml = f
                    break

        if book_xml.exists():
            try:
                with open(book_xml, 'r', encoding='utf-8') as f:
                    content = f.read()

                # Look for ISBN in various formats
                isbn_patterns = [
                    r'<isbn[^>]*>([^<]+)</isbn>',
                    r'<biblioid[^>]*class=["\']isbn["\'][^>]*>([^<]+)</biblioid>',
                    r'isbn["\s:=]+([0-9\-]{10,17})',
                ]

                for pattern in isbn_patterns:
                    match = re.search(pattern, content, re.IGNORECASE)
                    if match:
                        return match.group(1).strip()
            except Exception as e:
                logger.warning(f"Error extracting ISBN from Book.XML: {e}")

    # Fallback to filename
    return filename


def send_completion_webhook(job_id: str, status: str, file_type: str, metadata: dict = None, isbn: str = None):
    """
    Send webhook notification to UI backend when file processing is complete.

    Includes full download URLs for all available output files so the UI can
    download whatever it needs.

    Args:
        job_id: Job identifier (usually ISBN)
        status: 'completed' or 'failed'
        file_type: 'epub' or 'pdf'
        metadata: Additional metadata dict
        isbn: ISBN for constructing download URLs (uses job_id if not provided)
    """
    try:
        webhook_url = f"{UI_BACKEND_URL}{WEBHOOK_ENDPOINT}"

        # Use isbn or job_id for download URLs
        file_id = isbn or job_id

        # Construct download URLs
        download_urls = {
            'package': f"{API_SERVER_URL}/api/v1/files/{file_id}/package",
            'report': f"{API_SERVER_URL}/api/v1/files/{file_id}/report",
            'info': f"{API_SERVER_URL}/api/v1/files/{file_id}"
        }

        payload = {
            'jobId': job_id,
            'status': status,
            'fileType': file_type,
            'metadata': metadata or {},
            'downloadUrls': download_urls
        }

        logger.info(f"Sending completion webhook to {webhook_url}: {payload}")

        response = requests.post(
            webhook_url,
            json=payload,
            timeout=10,
            headers={'Content-Type': 'application/json'}
        )

        if response.status_code == 200:
            logger.info(f"Webhook sent successfully for job {job_id}")
            return True
        else:
            logger.warning(f"Webhook returned status {response.status_code}: {response.text}")
            return False

    except requests.exceptions.ConnectionError:
        logger.warning(f"Could not connect to UI backend at {webhook_url} - webhook skipped")
        return False
    except requests.exceptions.Timeout:
        logger.warning(f"Webhook request timed out for job {job_id}")
        return False
    except Exception as e:
        logger.warning(f"Failed to send webhook: {e}")
        return False


app = Flask(__name__)
app.config['MAX_CONTENT_LENGTH'] = 50 * 1024 * 1024  # 50MB max file size

# Global state for current working files
CURRENT_STATE = {
    'working_dir': None,
    'xml_file': None,
    'html_file': None,
    'pdf_file': None,
    'epub_file': None,
    'file_type': None,  # 'pdf' or 'epub'
    'multimedia_folder': None,
    # Package mode
    'package_mode': False,
    'zip_file': None,
    'extracted_dir': None,
    'book_xml': None,
    'chapters': [],  # List of chapter files in order
    'combined_xml': None,
}

def find_latest_output():
    """Find the most recent output files"""
    output_dir = Path(__file__).parent.parent / 'Output'

    if not output_dir.exists():
        return None

    # First check for ZIP packages (preferred)
    zip_files = list(output_dir.glob('*_all_fixes.zip'))
    if zip_files:
        # Use the most recently modified ZIP file
        zip_file = max(zip_files, key=lambda f: f.stat().st_mtime)
        return load_zip_package(zip_file)

    # Fall back to XML files
    xml_files = list(output_dir.glob('*.xml'))
    xml_files = [f for f in xml_files if '_all_fixes' not in f.name]

    if not xml_files:
        return None

    # Use the most recently modified XML file
    xml_file = max(xml_files, key=lambda f: f.stat().st_mtime)
    base_name = xml_file.stem

    return {
        'working_dir': output_dir,
        'xml_file': xml_file,
        'html_file': output_dir / f'{base_name}.html',
        'pdf_file': output_dir / f'{base_name}.pdf',
        'multimedia_folder': output_dir / 'multimedia'
    }

def load_zip_package(zip_path):
    """Load and extract a RittDoc package ZIP file"""
    try:
        # Create a temporary directory for extraction
        temp_dir = tempfile.mkdtemp(prefix='rittdoc_package_')
        extracted_dir = Path(temp_dir)

        # Extract ZIP
        with zipfile.ZipFile(zip_path, 'r') as zf:
            zf.extractall(extracted_dir)

        # Find Book.XML
        book_xml = extracted_dir / 'Book.XML'
        if not book_xml.exists():
            # Try case-insensitive search
            for file in extracted_dir.iterdir():
                if file.name.lower() == 'book.xml':
                    book_xml = file
                    break

        if not book_xml.exists():
            print(f"Warning: Book.XML not found in {zip_path}")
            return None

        # Parse Book.XML to get chapter structure
        chapters = parse_book_structure(book_xml)

        # Find multimedia folder
        multimedia_folder = extracted_dir / 'multimedia'
        if not multimedia_folder.exists():
            # Try to find it case-insensitively
            for item in extracted_dir.iterdir():
                if item.is_dir() and item.name.lower() == 'multimedia':
                    multimedia_folder = item
                    break

        # Combine all chapters
        combined_xml = combine_chapters(book_xml, chapters, extracted_dir)

        return {
            'package_mode': True,
            'zip_file': zip_path,
            'extracted_dir': extracted_dir,
            'working_dir': zip_path.parent,
            'book_xml': book_xml,
            'chapters': chapters,
            'combined_xml': combined_xml,
            'xml_file': book_xml,  # For compatibility
            'multimedia_folder': multimedia_folder if multimedia_folder.exists() else None,
            'html_file': None,  # Will generate from combined XML
            'pdf_file': None,
            'epub_file': None
        }
    except Exception as e:
        print(f"Error loading ZIP package: {e}")
        import traceback
        traceback.print_exc()
        return None

def parse_book_structure(book_xml_path):
    """Parse Book.XML to get the chapter structure"""
    try:
        # Read the raw XML file to check for entity declarations
        with open(book_xml_path, 'r', encoding='utf-8') as f:
            raw_content = f.read()

        chapters = []

        # Method 1: Look for DOCTYPE entity declarations
        # Pattern: <!ENTITY ch0001 SYSTEM "ch001.xml">
        import re
        entity_pattern = r'<!ENTITY\s+(\w+)\s+SYSTEM\s+"([^"]+)">'
        entity_matches = re.findall(entity_pattern, raw_content)

        if entity_matches:
            # Sort by entity name to maintain order
            entity_matches.sort(key=lambda x: x[0])
            for entity_name, filename in entity_matches:
                chapters.append({
                    'file': filename,
                    'entity': entity_name
                })
            print(f"Found {len(chapters)} chapters via DOCTYPE entities")
            return chapters

        # Method 2: Look for xi:include elements
        tree = etree.parse(str(book_xml_path))
        root = tree.getroot()

        ns = {'xi': 'http://www.w3.org/2001/XInclude'}
        includes = root.xpath('//xi:include', namespaces=ns)

        for include in includes:
            href = include.get('href')
            if href:
                chapters.append({
                    'file': href,
                    'element': include
                })

        if chapters:
            print(f"Found {len(chapters)} chapters via xi:include")
            return chapters

        # Method 3: Look for chapter files in the same directory
        book_dir = book_xml_path.parent
        chapter_files = sorted(book_dir.glob('ch*.xml'))
        for ch_file in chapter_files:
            chapters.append({
                'file': ch_file.name,
                'path': ch_file
            })

        if chapters:
            print(f"Found {len(chapters)} chapters via file scan")

        return chapters

    except Exception as e:
        print(f"Error parsing book structure: {e}")
        import traceback
        traceback.print_exc()
        return []

def combine_chapters(book_xml_path, chapters, base_dir):
    """Combine all chapter files into a single XML for viewing"""
    try:
        # Read the raw Book.XML content
        with open(book_xml_path, 'r', encoding='utf-8') as f:
            raw_content = f.read()

        if not chapters:
            return raw_content

        # Check if this uses entity references
        uses_entities = any('entity' in ch for ch in chapters)

        if uses_entities:
            # Method 1: Replace entity references with actual chapter content
            import re

            # Build combined XML by replacing entity references
            combined_content = raw_content

            for chapter in chapters:
                entity_name = chapter.get('entity')
                filename = chapter['file']
                ch_file = base_dir / filename

                if ch_file.exists() and entity_name:
                    try:
                        # Read chapter content
                        with open(ch_file, 'r', encoding='utf-8') as cf:
                            ch_content = cf.read()

                        # Remove XML declaration from chapter
                        ch_content = re.sub(r'<\?xml[^?]*\?>\s*', '', ch_content)
                        # Remove DOCTYPE from chapter
                        ch_content = re.sub(r'<!DOCTYPE[^>]*>\s*', '', ch_content)

                        # Replace &entityname; with chapter content
                        entity_ref = f'&{entity_name};'
                        replacement = f'\n<!-- Chapter: {filename} -->\n{ch_content.strip()}\n'
                        combined_content = combined_content.replace(entity_ref, replacement)

                        print(f"Replaced entity &{entity_name}; with content from {filename}")

                    except Exception as e:
                        print(f"Error reading chapter {filename}: {e}")

            # Remove DOCTYPE declaration from combined content for cleaner view
            # But keep it in a comment for reference
            doctype_match = re.search(r'(<!DOCTYPE[^>]*(?:\[[^\]]*\])?>)', combined_content, re.DOTALL)
            if doctype_match:
                doctype = doctype_match.group(1)
                combined_content = combined_content.replace(doctype, f'<!-- Original DOCTYPE removed for viewing:\n{doctype}\n-->')

            return combined_content

        else:
            # Method 2: Handle xi:include style references
            tree = etree.parse(str(book_xml_path))
            root = tree.getroot()

            ns = {'xi': 'http://www.w3.org/2001/XInclude'}
            includes = root.xpath('//xi:include', namespaces=ns)

            for include in includes:
                href = include.get('href')
                if href:
                    ch_file = base_dir / href
                    if ch_file.exists():
                        try:
                            ch_tree = etree.parse(str(ch_file))
                            ch_root = ch_tree.getroot()

                            parent = include.getparent()
                            if parent is not None:
                                index = list(parent).index(include)
                                comment = etree.Comment(f' Chapter: {href} ')
                                parent.insert(index, comment)
                                parent.insert(index + 1, ch_root)
                                parent.remove(include)
                        except Exception as e:
                            print(f"Error processing chapter {href}: {e}")

            combined_xml = etree.tostring(
                root,
                encoding='unicode',
                pretty_print=True,
                xml_declaration=True
            )
            return combined_xml

    except Exception as e:
        print(f"Error combining chapters: {e}")
        import traceback
        traceback.print_exc()
        try:
            with open(book_xml_path, 'r', encoding='utf-8') as f:
                return f.read()
        except (IOError, OSError, FileNotFoundError) as read_error:
            logger.error(f"Failed to read book.xml fallback: {read_error}")
            return '<?xml version="1.0" encoding="UTF-8"?><book>Error loading book</book>'

@app.route('/')
def index():
    """
    Serve the main editor page with optional package auto-load.

    Query Parameters:
        package: Path to ZIP package to auto-load (e.g., /app/Output/isbn_all_fixes.zip)
        jobId: Job ID for webhook callbacks (usually ISBN)

    Example:
        GET /?package=/app/Output/9781234567890_all_fixes.zip&jobId=9781234567890
    """
    # Check for package query parameter to auto-load
    package_path = request.args.get('package')
    job_id = request.args.get('jobId')

    if package_path:
        # Auto-load the package
        zip_path = Path(package_path)
        if zip_path.exists():
            package_data = load_zip_package(zip_path)
            if package_data:
                CURRENT_STATE.update(package_data)
                # Store job_id for webhook
                if job_id:
                    CURRENT_STATE['job_id'] = job_id
                logger.info(f"Auto-loaded package from query param: {package_path}")
        else:
            logger.warning(f"Package not found: {package_path}")

    return render_template('index.html')

@app.route('/api/init', methods=['GET', 'POST'])
def init():
    """Initialize editor with file paths"""
    # Note: CURRENT_STATE is a global dict - we mutate it, not reassign it

    try:
        if request.method == 'POST':
            data = request.json
            CURRENT_STATE['working_dir'] = Path(data.get('workingDir', ''))
            CURRENT_STATE['xml_file'] = Path(data.get('xmlPath', ''))
            CURRENT_STATE['html_file'] = Path(data.get('htmlPath', ''))
            CURRENT_STATE['pdf_file'] = Path(data.get('pdfPath', ''))
        else:
            # Auto-detect latest output
            latest = find_latest_output()
            if latest:
                CURRENT_STATE.update(latest)

        # Read files
        xml_content = ''
        html_content = ''

        # Check if we're in package mode
        if CURRENT_STATE.get('package_mode'):
            # Use combined XML from all chapters
            xml_content = CURRENT_STATE.get('combined_xml', '')
        elif CURRENT_STATE['xml_file'] and CURRENT_STATE['xml_file'].exists():
            xml_content = CURRENT_STATE['xml_file'].read_text(encoding='utf-8')

        if CURRENT_STATE['html_file'] and CURRENT_STATE['html_file'].exists():
            html_content = CURRENT_STATE['html_file'].read_text(encoding='utf-8')
        else:
            html_content = '<div><p>HTML preview not available. Use "Refresh HTML" to generate from XML.</p></div>'

        pdf_exists = CURRENT_STATE['pdf_file'] and CURRENT_STATE['pdf_file'].exists()
        epub_exists = CURRENT_STATE['epub_file'] and CURRENT_STATE['epub_file'].exists()

        # Set multimedia folder
        if CURRENT_STATE['working_dir']:
            CURRENT_STATE['multimedia_folder'] = CURRENT_STATE.get('multimedia_folder') or CURRENT_STATE['working_dir'] / 'multimedia'

        # Determine file type
        file_type = None
        if pdf_exists:
            file_type = 'pdf'
        elif epub_exists:
            file_type = 'epub'

        # Build response
        response_data = {
            'success': True,
            'xml': xml_content,
            'html': html_content,
            'pdf': {
                'path': str(CURRENT_STATE['pdf_file']) if CURRENT_STATE['pdf_file'] else None,
                'exists': pdf_exists
            },
            'epub': {
                'path': str(CURRENT_STATE['epub_file']) if CURRENT_STATE['epub_file'] else None,
                'exists': epub_exists
            },
            'file_type': file_type,
            'multimedia_folder': str(CURRENT_STATE['multimedia_folder']) if CURRENT_STATE['multimedia_folder'] else None,
            'package_mode': CURRENT_STATE.get('package_mode', False),
        }

        # Add package-specific info
        if CURRENT_STATE.get('package_mode'):
            response_data['package'] = {
                'zip_file': str(CURRENT_STATE['zip_file']),
                'num_chapters': len(CURRENT_STATE.get('chapters', [])),
                'chapters': [{'file': ch['file']} for ch in CURRENT_STATE.get('chapters', [])]
            }

        return jsonify(response_data)

    except Exception as e:
        return jsonify({
            'error': str(e),
            'xml': '',
            'html': '',
            'pdf': {'path': None, 'exists': False},
            'multimedia_folder': None
        }), 500

@app.route('/api/pdf')
def get_pdf():
    """Serve the PDF file"""
    try:
        pdf_file = CURRENT_STATE['pdf_file']
        if not pdf_file or not pdf_file.exists():
            return jsonify({'error': 'PDF file not found'}), 404

        return send_file(pdf_file, mimetype='application/pdf')

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/epub')
def get_epub():
    """Serve the EPUB file"""
    try:
        epub_file = CURRENT_STATE['epub_file']
        if not epub_file or not epub_file.exists():
            return jsonify({'error': 'EPUB file not found'}), 404

        return send_file(epub_file, mimetype='application/epub+zip')

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/media/<path:filename>')
def get_media(filename):
    """Serve media files from multimedia folder or extracted package"""
    try:
        multimedia_folder = CURRENT_STATE.get('multimedia_folder')

        if not multimedia_folder or not Path(multimedia_folder).exists():
            return jsonify({'error': 'Multimedia folder not found'}), 404

        file_path = Path(multimedia_folder) / filename
        if not file_path.exists():
            return jsonify({'error': f'Media file not found: {filename}'}), 404

        return send_file(file_path)

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/media-list')
def list_media():
    """List all media files"""
    try:
        multimedia_folder = CURRENT_STATE['multimedia_folder']
        if not multimedia_folder or not multimedia_folder.exists():
            return jsonify({'files': []})

        image_extensions = {'.png', '.jpg', '.jpeg', '.gif', '.svg', '.webp'}
        files = []

        for file_path in multimedia_folder.iterdir():
            if file_path.is_file() and file_path.suffix.lower() in image_extensions:
                files.append({
                    'name': file_path.name,
                    'path': f'/api/media/{file_path.name}'
                })

        return jsonify({'files': files})

    except Exception as e:
        return jsonify({'error': str(e), 'files': []}), 500

@app.route('/api/save', methods=['POST'])
def save():
    """Save XML or HTML content"""
    try:
        data = request.json
        content_type = data.get('type')
        content = data.get('content')
        reprocess = data.get('reprocess', False)

        if content_type == 'xml' and CURRENT_STATE['xml_file']:
            CURRENT_STATE['xml_file'].write_text(content, encoding='utf-8')

            if reprocess:
                # TODO: Trigger reprocessing pipeline
                return jsonify({
                    'success': True,
                    'message': 'XML saved. Reprocessing not yet implemented.',
                    'package': None
                })

            return jsonify({
                'success': True,
                'message': 'XML saved successfully'
            })

        elif content_type == 'html' and CURRENT_STATE['html_file']:
            CURRENT_STATE['html_file'].write_text(content, encoding='utf-8')

            return jsonify({
                'success': True,
                'message': 'HTML saved successfully',
                'html': content
            })

        return jsonify({'error': 'Invalid save type or file path not set'}), 400

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/screenshot', methods=['POST'])
def save_screenshot():
    """Save screenshot image"""
    try:
        data = request.json
        image_data = data.get('imageData')
        target_filename = data.get('targetFilename')
        page_number = data.get('pageNumber')

        multimedia_folder = CURRENT_STATE['multimedia_folder']
        if not multimedia_folder:
            return jsonify({'error': 'Multimedia folder not set'}), 400

        multimedia_folder.mkdir(parents=True, exist_ok=True)
        file_path = multimedia_folder / target_filename

        # Decode base64 image
        if ',' in image_data:
            image_data = image_data.split(',')[1]

        image_bytes = base64.b64decode(image_data)
        file_path.write_bytes(image_bytes)

        return jsonify({
            'success': True,
            'message': f'Screenshot saved as {target_filename}',
            'path': f'/api/media/{target_filename}'
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/render-html', methods=['POST'])
def render_html():
    """Convert XML to HTML preview"""
    try:
        data = request.json
        xml = data.get('xml', '')

        # Basic XML to HTML conversion
        # In production, use XSLT or proper XML parser
        import re

        html = xml
        # Remove XML declaration
        html = re.sub(r'<\?xml[^?]*\?>', '', html)
        # Basic tag replacements
        replacements = [
            (r'<book[^>]*>', '<div class="book">'),
            (r'</book>', '</div>'),
            (r'<chapter[^>]*>', '<div class="chapter">'),
            (r'</chapter>', '</div>'),
            (r'<section[^>]*>', '<section>'),
            (r'<title>', '<h2>'),
            (r'</title>', '</h2>'),
            (r'<para>', '<p>'),
            (r'</para>', '</p>'),
        ]

        for pattern, replacement in replacements:
            html = re.sub(pattern, replacement, html)

        return jsonify({
            'success': True,
            'html': html
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/page-mapping')
def get_page_mapping():
    """Get page to XML element mapping"""
    try:
        # TODO: Implement actual page mapping from XML
        # For now, return empty mapping
        return jsonify({
            'mapping': {},
            'element_to_page': {},
            'total_xml_pages': 0,
            'page_count': 0
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/placeholder-image')
def placeholder_image():
    """Return a placeholder image"""
    # 1x1 transparent PNG
    img_data = base64.b64decode(
        'iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg=='
    )
    from io import BytesIO
    return send_file(BytesIO(img_data), mimetype='image/png')

@app.route('/static/<path:filename>')
def serve_static(filename):
    """Serve static files"""
    return send_from_directory('static', filename)

@app.route('/api/load-package', methods=['POST'])
def load_package():
    """Load a specific ZIP package file"""
    try:
        data = request.json
        zip_path = Path(data.get('zipPath'))

        if not zip_path.exists():
            return jsonify({'error': f'ZIP file not found: {zip_path}'}), 404

        # Load the package
        package_data = load_zip_package(zip_path)
        if not package_data:
            return jsonify({'error': 'Failed to load package'}), 500

        # Update current state
        CURRENT_STATE.update(package_data)

        return jsonify({
            'success': True,
            'message': 'Package loaded successfully',
            'num_chapters': len(CURRENT_STATE.get('chapters', [])),
            'chapters': [{'file': ch['file']} for ch in CURRENT_STATE.get('chapters', [])]
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/chapters')
def get_chapters():
    """Get list of chapters in the current package"""
    try:
        if not CURRENT_STATE.get('package_mode'):
            return jsonify({'error': 'Not in package mode'}), 400

        chapters = CURRENT_STATE.get('chapters', [])
        return jsonify({
            'success': True,
            'chapters': [{'file': ch['file']} for ch in chapters]
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/chapter/<path:filename>')
def get_chapter(filename):
    """Get a specific chapter's content"""
    try:
        if not CURRENT_STATE.get('package_mode'):
            return jsonify({'error': 'Not in package mode'}), 400

        extracted_dir = CURRENT_STATE.get('extracted_dir')
        if not extracted_dir:
            return jsonify({'error': 'No extracted directory'}), 500

        chapter_file = Path(extracted_dir) / filename
        if not chapter_file.exists():
            return jsonify({'error': f'Chapter not found: {filename}'}), 404

        content = chapter_file.read_text(encoding='utf-8')
        return jsonify({
            'success': True,
            'filename': filename,
            'content': content
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/save-chapter', methods=['POST'])
def save_chapter():
    """Save changes to a specific chapter"""
    try:
        if not CURRENT_STATE.get('package_mode'):
            return jsonify({'error': 'Not in package mode'}), 400

        data = request.json
        filename = data.get('filename')
        content = data.get('content')

        extracted_dir = CURRENT_STATE.get('extracted_dir')
        if not extracted_dir:
            return jsonify({'error': 'No extracted directory'}), 500

        chapter_file = Path(extracted_dir) / filename
        chapter_file.write_text(content, encoding='utf-8')

        # Regenerate combined XML
        book_xml = CURRENT_STATE.get('book_xml')
        chapters = CURRENT_STATE.get('chapters', [])
        CURRENT_STATE['combined_xml'] = combine_chapters(book_xml, chapters, Path(extracted_dir))

        return jsonify({
            'success': True,
            'message': f'Chapter {filename} saved successfully'
        })

    except Exception as e:
        return jsonify({'error': str(e)}), 500

@app.route('/api/save-package', methods=['POST'])
def save_package():
    """Save all changes back to the ZIP file and automatically reprocess"""
    try:
        if not CURRENT_STATE.get('package_mode'):
            return jsonify({'error': 'Not in package mode'}), 400

        # Get job_id from request, stored state, or fall back to ISBN
        data = request.json or {}
        job_id = data.get('jobId') or CURRENT_STATE.get('job_id')

        zip_file = CURRENT_STATE.get('zip_file')
        extracted_dir = CURRENT_STATE.get('extracted_dir')

        if not zip_file or not extracted_dir:
            return jsonify({'error': 'Package information missing'}), 500

        zip_file = Path(zip_file)
        extracted_dir = Path(extracted_dir)

        # Extract ISBN for webhook metadata
        isbn = extract_isbn_from_package(zip_file, extracted_dir)

        # Use ISBN as job_id if still not provided
        if not job_id:
            job_id = isbn

        # Create a backup of the original
        backup_path = Path(str(zip_file) + '.backup')
        if zip_file.exists():
            shutil.copy2(zip_file, backup_path)

        # Automatically run reprocessing (XSLT, packaging, DTD fixes, validation)
        if REPROCESS_AVAILABLE:
            logger.info(f"Starting automatic reprocessing for {zip_file.name}")

            result = reprocess_package(
                extracted_dir=extracted_dir,
                original_zip_path=zip_file,
                output_dir=zip_file.parent,
                apply_xslt=True,
                apply_dtd_fixes=True,
                validate=True
            )

            if result['success']:
                # Reload the updated package
                new_zip_path = Path(result['output_zip'])
                if new_zip_path.exists():
                    # Update current state with new package
                    new_package_data = load_zip_package(new_zip_path)
                    if new_package_data:
                        CURRENT_STATE.update(new_package_data)

                # Send webhook notification to UI backend with download URLs
                webhook_sent = send_completion_webhook(
                    job_id=job_id,
                    status='completed',
                    file_type='epub',
                    metadata={
                        'isbn': isbn,
                        'output_zip': result['output_zip'],
                        'validation_passed': result['validation_passed'],
                        'errors_fixed': result['errors_fixed'],
                        'remaining_errors': result['remaining_errors'],
                        'report_path': result.get('report_path')
                    },
                    isbn=isbn
                )

                return jsonify({
                    'success': True,
                    'message': result['message'],
                    'backup': str(backup_path),
                    'reprocessed': True,
                    'output_zip': result['output_zip'],
                    'validation_passed': result['validation_passed'],
                    'errors_fixed': result['errors_fixed'],
                    'remaining_errors': result['remaining_errors'],
                    'report_path': result.get('report_path'),
                    'webhook_sent': webhook_sent
                })
            else:
                # Reprocessing failed - send failure webhook
                send_completion_webhook(
                    job_id=job_id,
                    status='failed',
                    file_type='epub',
                    metadata={
                        'isbn': isbn,
                        'error': result['message']
                    },
                    isbn=isbn
                )

                return jsonify({
                    'success': True,
                    'message': f"Package saved but reprocessing failed: {result['message']}",
                    'backup': str(backup_path),
                    'reprocessed': False,
                    'error': result['message']
                })
        else:
            # Reprocessing not available, just save the ZIP
            with zipfile.ZipFile(zip_file, 'w', zipfile.ZIP_DEFLATED) as zf:
                for file_path in extracted_dir.rglob('*'):
                    if file_path.is_file():
                        arcname = file_path.relative_to(extracted_dir)
                        zf.write(file_path, arcname)

            # Send webhook notification (without reprocessing details)
            webhook_sent = send_completion_webhook(
                job_id=job_id,
                status='completed',
                file_type='epub',
                metadata={
                    'isbn': isbn,
                    'output_zip': str(zip_file),
                    'reprocessed': False
                },
                isbn=isbn
            )

            return jsonify({
                'success': True,
                'message': 'Package saved successfully (reprocessing not available)',
                'backup': str(backup_path),
                'reprocessed': False,
                'webhook_sent': webhook_sent
            })

    except Exception as e:
        logger.exception("Error saving package")
        # Send failure webhook on exception
        try:
            job_id = (request.json or {}).get('jobId', 'unknown')
            send_completion_webhook(
                job_id=job_id,
                status='failed',
                file_type='epub',
                metadata={'error': str(e)}
            )
        except Exception:
            pass
        return jsonify({'error': str(e)}), 500

@app.route('/api/validate-dtd', methods=['POST'])
def validate_dtd():
    """Validate XML against RittDoc DTD"""
    try:
        data = request.json
        xml_content = data.get('xml', '')

        if not xml_content or not xml_content.strip():
            return jsonify({'error': 'No XML content provided'}), 400

        # Path to RittDoc DTD
        dtd_path = Path(__file__).parent.parent / 'RITTDOCdtd' / 'v1.1' / 'RittDocBook.dtd'

        if not dtd_path.exists():
            return jsonify({
                'error': 'DTD file not found',
                'valid': False,
                'errors': []
            }), 200

        # Parse XML
        try:
            xml_doc = etree.fromstring(xml_content.encode('utf-8'))
        except etree.XMLSyntaxError as e:
            return jsonify({
                'valid': False,
                'errors': [{'message': f'XML syntax error: {str(e)}', 'line': e.lineno if hasattr(e, 'lineno') else 0}],
                'can_auto_fix': False
            }), 200

        # Load DTD
        try:
            dtd = etree.DTD(str(dtd_path))
        except Exception as e:
            return jsonify({
                'error': f'Failed to load DTD: {str(e)}',
                'valid': False,
                'errors': []
            }), 200

        # Validate against DTD
        is_valid = dtd.validate(xml_doc)

        if is_valid:
            return jsonify({
                'valid': True,
                'errors': [],
                'message': 'XML is DTD-compliant'
            })
        else:
            # Collect validation errors
            errors = []
            for error in dtd.error_log.filter_from_errors():
                errors.append({
                    'message': error.message,
                    'line': error.line,
                    'column': error.column,
                    'type': error.type_name
                })

            return jsonify({
                'valid': False,
                'errors': errors,
                'can_auto_fix': True,  # comprehensive_dtd_fixer can fix many issues
                'error_count': len(errors)
            })

    except Exception as e:
        return jsonify({
            'error': f'Validation error: {str(e)}',
            'valid': False,
            'errors': []
        }), 500

@app.route('/api/render-book-html', methods=['POST'])
def render_book_html():
    """Generate HTML preview from combined XML"""
    try:
        data = request.json
        xml_content = data.get('xml', '')

        # Basic XML to HTML transformation
        import re

        html = xml_content

        # Remove XML declaration and DOCTYPE
        html = re.sub(r'<\?xml[^?]*\?>', '', html)
        html = re.sub(r'<!DOCTYPE[^>]*>', '', html)

        # Helper: extract id attribute from an XML tag string
        _id_re = re.compile(r'\bid="([^"]+)"')
        def _keep_id(tag_str):
            """Return ' id="..."' if the tag has an id attribute, else ''."""
            m = _id_re.search(tag_str)
            return f' id="{m.group(1)}"' if m else ''

        # Phase 1: Transform elements that need id preservation using lambdas.
        # Each re.sub runs once per element type so there's no clobbering.
        id_transforms = [
            (r'<chapter\b[^>]*>',       lambda m: f'<div class="chapter"{_keep_id(m.group())}>'),
            (r'<preface\b[^>]*>',       lambda m: f'<div class="preface"{_keep_id(m.group())}>'),
            (r'<sect1\b[^>]*>',         lambda m: f'<section class="section sect1"{_keep_id(m.group())}>'),
            (r'<sect2\b[^>]*>',         lambda m: f'<section class="section sect2"{_keep_id(m.group())}>'),
            (r'<sect3\b[^>]*>',         lambda m: f'<section class="section sect3"{_keep_id(m.group())}>'),
            (r'<section\b[^>]*>',       lambda m: f'<section class="section"{_keep_id(m.group())}>'),
            (r'<para\b[^>]*>',          lambda m: f'<p{_keep_id(m.group())}>'),
            (r'<figure\b[^>]*>',        lambda m: f'<figure class="book-figure"{_keep_id(m.group())}>'),
            (r'<informaltable\b[^>]*>', lambda m: f'<table class="book-table"{_keep_id(m.group())}>'),
            (r'<table\b[^>]*>',         lambda m: f'<table class="book-table"{_keep_id(m.group())}>'),
            (r'<inlineequation\b[^>]*>',   lambda m: f'<span class="inline-equation"{_keep_id(m.group())}>'),
            (r'<equation\b[^>]*>',         lambda m: f'<div class="equation"{_keep_id(m.group())}>'),
            (r'<informalequation\b[^>]*>', lambda m: f'<div class="equation"{_keep_id(m.group())}>'),
        ]
        for pattern, repl_fn in id_transforms:
            html = re.sub(pattern, repl_fn, html, flags=re.IGNORECASE)

        # Phase 2: Simple string-to-string transformations (closing tags, etc.)
        transformations = [
            # Book structure
            (r'<book[^>]*>', '<div class="book-content">'),
            (r'</book>', '</div>'),

            # Closing tags for elements handled in Phase 1
            (r'</chapter>', '</div>'),
            (r'</preface>', '</div>'),
            (r'</sect1>', '</section>'),
            (r'</sect2>', '</section>'),
            (r'</sect3>', '</section>'),
            (r'</section>', '</section>'),
            (r'</para>', '</p>'),
            (r'</figure>', '</figure>'),
            (r'</informaltable>', '</table>'),
            (r'</table>', '</table>'),
            (r'</inlineequation>', '</span>'),
            (r'</equation>', '</div>'),
            (r'</informalequation>', '</div>'),

            # Titles (handle empty titles too)
            (r'<title/>', ''),
            (r'<title>\s*</title>', ''),
            (r'<title>', '<h2 class="title">'),
            (r'</title>', '</h2>'),

            # Inline elements
            (r'<phrase[^>]*>', '<span>'),
            (r'</phrase>', '</span>'),
            (r'<abbrev[^>]*>', '<abbr>'),
            (r'</abbrev>', '</abbr>'),
            (r'<anchor[^>]*/?>', ''),  # Remove anchors, they're just markers
            (r'<textobject[^>]*>', '<span class="text-object">'),
            (r'</textobject>', '</span>'),
            (r'<alt[^>]*>', '<span class="alt-text">'),
            (r'</alt>', '</span>'),

            # Links - internal <link linkend="..."> for same-chapter navigation
            (r'<link\s+linkend="([^"]+)"[^>]*>', r'<a href="#\1" class="internal-ref">'),
            (r'</link>', '</a>'),
            # Links - <ulink> for cross-chapter and external navigation
            (r'<ulink\s+url="([^"]+)"[^>]*>', r'<a href="\1">'),
            (r'</ulink>', '</a>'),

            # Lists
            (r'<itemizedlist[^>]*>', '<ul>'),
            (r'</itemizedlist>', '</ul>'),
            (r'<orderedlist[^>]*>', '<ol>'),
            (r'</orderedlist>', '</ol>'),
            (r'<listitem>', '<li>'),
            (r'</listitem>', '</li>'),

            # Emphasis and strong (handle strong emphasis first)
            (r'<emphasis\s+role=["\']strong["\'][^>]*>(.*?)</emphasis>', r'<strong>\1</strong>'),
            (r'<emphasis[^>]*>', '<em>'),
            (r'</emphasis>', '</em>'),

            # Media objects
            (r'<mediaobject[^>]*>', '<div class="media-object">'),
            (r'</mediaobject>', '</div>'),
            (r'<imageobject[^>]*>', '<div class="image-object">'),
            (r'</imageobject>', '</div>'),
            (r'<inlinemediaobject[^>]*>', '<span class="inline-media">'),
            (r'</inlinemediaobject>', '</span>'),

            # Image tags - support multiple formats
            (r'<imagedata\s+fileref="([^"]+)"[^>]*/?>', r'<img src="/api/media/\1" class="book-image" loading="lazy" />'),
            (r'<graphic\s+fileref="([^"]+)"[^>]*/?>', r'<img src="/api/media/\1" class="book-image" loading="lazy" />'),
            (r'<image\s+src="([^"]+)"[^>]*/?>', r'<img src="/api/media/\1" class="book-image" loading="lazy" />'),
            (r'<image\s+href="([^"]+)"[^>]*/?>', r'<img src="/api/media/\1" class="book-image" loading="lazy" />'),
            (r'<img\s+src="multimedia/([^"]+)"[^>]*/?>', r'<img src="/api/media/\1" class="book-image" loading="lazy" />'),

            # Table sub-elements
            (r'<tgroup[^>]*>', ''),
            (r'</tgroup>', ''),
            (r'<colspec[^>]*/>', ''),
            (r'<tbody[^>]*>', '<tbody>'),
            (r'</tbody>', '</tbody>'),
            (r'<thead[^>]*>', '<thead>'),
            (r'</thead>', '</thead>'),
            (r'<row[^>]*>', '<tr>'),
            (r'</row>', '</tr>'),
            (r'<entry[^>]*>', '<td>'),
            (r'</entry>', '</td>'),
        ]

        for pattern, replacement in transformations:
            html = re.sub(pattern, replacement, html, flags=re.IGNORECASE)

        # Normalize MathML namespace prefixes for HTML rendering
        html = re.sub(r'<(/?)mml:', r'<\1', html)

        # Additional image processing - ensure all images are properly formatted
        # Handle any remaining image references that might be in different formats
        html = re.sub(r'<imagedata\s+([^>]*)fileref\s*=\s*["\']([^"\']+)["\']([^>]*)>',
                     r'<img src="/api/media/\2" class="book-image" loading="lazy" />', html, flags=re.IGNORECASE)

        # Clean up nested image containers that might be empty
        html = re.sub(r'<div class="image-object">\s*</div>', '', html)
        html = re.sub(r'<div class="media-object">\s*</div>', '', html)

        # Remove any remaining XML comments
        html = re.sub(r'<!--.*?-->', '', html, flags=re.DOTALL)

        # Clean up empty tags and whitespace
        html = re.sub(r'<(div|section)[^>]*>\s*</\1>', '', html)

        # Minimal wrapper without heavy styling - just basic layout
        styled_html = f'''<div style="padding: 20px; background: white; color: black;">{html}</div>'''

        return jsonify({
            'success': True,
            'html': styled_html
        })

    except Exception as e:
        import traceback
        traceback.print_exc()
        return jsonify({'error': str(e)}), 500

def main():
    """Main entry point"""
    import argparse

    parser = argparse.ArgumentParser(description='RittDoc Editor Server')
    parser.add_argument('--port', type=int, default=5000, help='Port to run server on')
    parser.add_argument('--host', default='127.0.0.1', help='Host to bind to')
    parser.add_argument('--debug', action='store_true', help='Run in debug mode')
    parser.add_argument('--xml', help='Path to XML file to edit')
    parser.add_argument('--pdf', help='Path to PDF file to view')

    args = parser.parse_args()

    # Set initial files if provided
    if args.xml:
        xml_path = Path(args.xml).resolve()
        if xml_path.exists():
            CURRENT_STATE['xml_file'] = xml_path
            CURRENT_STATE['working_dir'] = xml_path.parent

            # Determine base name for related files
            base_name = xml_path.stem
            # Remove _editable suffix if present
            if base_name.endswith('_editable'):
                base_name = base_name[:-9]

            CURRENT_STATE['html_file'] = xml_path.parent / f'{base_name}_preview.html'
            CURRENT_STATE['multimedia_folder'] = xml_path.parent / 'multimedia'

            if args.pdf:
                ref_file_path = Path(args.pdf).resolve()
                if ref_file_path.exists():
                    # Check if it's an EPUB or PDF
                    ext = ref_file_path.suffix.lower()
                    if ext in ['.epub', '.epub3']:
                        CURRENT_STATE['epub_file'] = ref_file_path
                        CURRENT_STATE['pdf_file'] = None
                        CURRENT_STATE['file_type'] = 'epub'
                    else:
                        CURRENT_STATE['pdf_file'] = ref_file_path
                        CURRENT_STATE['epub_file'] = None
                        CURRENT_STATE['file_type'] = 'pdf'
                else:
                    print(f"Warning: Reference file not found: {ref_file_path}")
                    CURRENT_STATE['pdf_file'] = None
                    CURRENT_STATE['epub_file'] = None
            else:
                # Try to find PDF or EPUB with same base name
                potential_pdf = xml_path.parent / f'{base_name}.pdf'
                potential_epub = xml_path.parent / f'{base_name}.epub'

                if potential_pdf.exists():
                    CURRENT_STATE['pdf_file'] = potential_pdf
                    CURRENT_STATE['file_type'] = 'pdf'
                elif potential_epub.exists():
                    CURRENT_STATE['epub_file'] = potential_epub
                    CURRENT_STATE['file_type'] = 'epub'
                else:
                    print(f"Info: No PDF or EPUB file found")
                    CURRENT_STATE['pdf_file'] = None
                    CURRENT_STATE['epub_file'] = None

            print(f"\n{'='*60}")
            print("RittDoc Editor Server")
            print(f"{'='*60}")
            print(f"Files loaded:")
            print(f"  XML: {xml_path.name}")

            if CURRENT_STATE['pdf_file'] and CURRENT_STATE['pdf_file'].exists():
                print(f"  PDF: {CURRENT_STATE['pdf_file'].name}")
            elif CURRENT_STATE['epub_file'] and CURRENT_STATE['epub_file'].exists():
                print(f"  EPUB: {CURRENT_STATE['epub_file'].name}")
            else:
                print(f"  Reference Document: Not available")

            if CURRENT_STATE['html_file'].exists():
                print(f"  HTML: {CURRENT_STATE['html_file'].name}")
            print(f"  Working Dir: {CURRENT_STATE['working_dir']}")
            print(f"{'='*60}")
        else:
            print(f"Error: XML file not found: {xml_path}")
            return

    print(f"\nServer running at: http://{args.host}:{args.port}")
    print(f"Press Ctrl+C to stop")
    print(f"{'='*60}\n")

    try:
        app.run(host=args.host, port=args.port, debug=args.debug, use_reloader=False)
    except KeyboardInterrupt:
        print("\n\nShutting down server...")
        print("Editor stopped.")

if __name__ == '__main__':
    main()
