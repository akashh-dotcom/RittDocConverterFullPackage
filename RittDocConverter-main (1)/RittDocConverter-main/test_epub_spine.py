#!/usr/bin/env python3
"""
Quick test to verify ebooklib reads all spine items from an EPUB.
Run with: python test_epub_spine.py /path/to/epub
"""

import sys
from pathlib import Path

try:
    import ebooklib
    from ebooklib import epub
except ImportError:
    print("Error: ebooklib not installed")
    sys.exit(1)


def test_spine(epub_path: Path):
    print(f"Testing: {epub_path.name}")
    print("=" * 60)

    book = epub.read_epub(str(epub_path))

    # Count spine items the same way the converter does
    spine_items = []
    for item_id, _ in book.spine:
        item = book.get_item_with_id(item_id)
        if item and item.get_type() == ebooklib.ITEM_DOCUMENT:
            spine_items.append((item_id, item.get_name()))

    print(f"Spine items found by ebooklib: {len(spine_items)}")
    print()

    # Show last 15 items to see where it stops
    print("Last 15 spine items:")
    for idx, (item_id, name) in enumerate(spine_items[-15:], len(spine_items)-14):
        print(f"  {idx:3d}. {item_id} -> {name}")

    print()
    print("=" * 60)

    # Check for expected items
    expected_last_items = [
        "C_067_part12",
        "C_068_c47",
        "C_069_c48",
        "C_070_c49",
        "C_071_c50",
        "Z_072_index"
    ]

    found_ids = {item_id for item_id, _ in spine_items}
    missing = [item for item in expected_last_items if item not in found_ids]

    if missing:
        print(f"WARNING: Missing expected items: {missing}")
        print("ebooklib is NOT reading the complete spine!")
    else:
        print("All expected items found in spine.")
        if len(spine_items) < 74:
            print(f"But only {len(spine_items)} items (expected 74)")
        else:
            print(f"Total: {len(spine_items)} items (expected 74)")

    return len(spine_items)


if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python test_epub_spine.py <epub_file>")
        sys.exit(1)

    epub_path = Path(sys.argv[1])
    if not epub_path.exists():
        print(f"Error: File not found: {epub_path}")
        sys.exit(1)

    test_spine(epub_path)
