#!/usr/bin/env python3
"""
Diagnostic script to check EPUB spine contents.
Helps identify why some XHTML files might not be converted.
"""

import sys
from pathlib import Path

try:
    import ebooklib
    from ebooklib import epub
except ImportError:
    print("Error: ebooklib not installed. Run: pip install EbookLib")
    sys.exit(1)


def diagnose_epub(epub_path: Path):
    """Analyze EPUB spine and content files."""
    print(f"\n{'='*70}")
    print(f"EPUB Spine Diagnostic: {epub_path.name}")
    print(f"{'='*70}\n")

    try:
        book = epub.read_epub(str(epub_path))
    except Exception as e:
        print(f"ERROR: Cannot read EPUB: {e}")
        return

    # Get all items
    all_items = list(book.get_items())
    documents = [item for item in all_items if item.get_type() == ebooklib.ITEM_DOCUMENT]

    print(f"Total items in EPUB: {len(all_items)}")
    print(f"Document items (XHTML): {len(documents)}")

    # Get spine items
    spine_items = []
    spine_ids = []
    for item_id, _ in book.spine:
        item = book.get_item_with_id(item_id)
        if item:
            spine_items.append(item)
            spine_ids.append(item_id)

    print(f"Items in spine: {len(spine_items)}")
    print(f"\n{'='*70}")
    print("SPINE ORDER (reading order):")
    print(f"{'='*70}")

    for idx, item in enumerate(spine_items, 1):
        name = item.get_name()
        item_id = spine_ids[idx-1] if idx-1 < len(spine_ids) else "?"
        print(f"  {idx:3d}. [{item_id:20s}] {name}")

    # Find documents NOT in spine
    spine_names = {item.get_name() for item in spine_items}
    not_in_spine = [doc for doc in documents if doc.get_name() not in spine_names]

    if not_in_spine:
        print(f"\n{'='*70}")
        print(f"WARNING: {len(not_in_spine)} document(s) NOT in spine (won't be converted):")
        print(f"{'='*70}")
        for doc in not_in_spine:
            print(f"  - {doc.get_name()}")

    # Summary
    print(f"\n{'='*70}")
    print("SUMMARY:")
    print(f"{'='*70}")
    print(f"  Documents in EPUB:     {len(documents)}")
    print(f"  Documents in spine:    {len(spine_items)}")
    print(f"  Documents NOT in spine: {len(not_in_spine)}")

    if len(spine_items) < len(documents):
        print(f"\n  [!] Some documents are excluded from conversion because")
        print(f"      they are not in the EPUB spine (reading order).")
        print(f"      This is normal for navigation files, but check if")
        print(f"      any content chapters are missing from the spine.")

    return len(spine_items), len(documents)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python diagnose_epub_spine.py <epub_file>")
        sys.exit(1)

    epub_path = Path(sys.argv[1])
    if not epub_path.exists():
        print(f"Error: File not found: {epub_path}")
        sys.exit(1)

    diagnose_epub(epub_path)
