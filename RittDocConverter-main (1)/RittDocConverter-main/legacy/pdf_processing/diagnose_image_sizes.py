#!/usr/bin/env python3
"""
Diagnostic script to compare image dimensions in EPUB vs extracted.
"""
import io
import sys
from pathlib import Path

import ebooklib
from ebooklib import epub
from PIL import Image


def diagnose_epub_images(epub_path: str):
    """Compare images inside EPUB with extracted versions."""
    print(f"Analyzing: {epub_path}\n")

    book = epub.read_epub(epub_path)

    print("=" * 80)
    print("IMAGE DIMENSIONS INSIDE EPUB (before extraction)")
    print("=" * 80)

    for item in book.get_items():
        if item.get_type() == ebooklib.ITEM_IMAGE:
            original_path = item.get_name()
            content = item.get_content()

            # Skip zero-byte images
            if len(content) == 0:
                continue

            try:
                # Read dimensions from bytes
                img = Image.open(io.BytesIO(content))
                width, height = img.size
                file_size_kb = len(content) / 1024

                print(f"\n{original_path}")
                print(f"  Dimensions: {width} x {height} pixels")
                print(f"  File size: {file_size_kb:.1f} KB")
                print(f"  Format: {img.format}")

            except Exception as e:
                print(f"\n{original_path}")
                print(f"  ERROR: Could not read image - {e}")

    print("\n" + "=" * 80)
    print("INSTRUCTIONS:")
    print("=" * 80)
    print("1. Run the EPUB conversion")
    print("2. Compare these dimensions with extracted images in MultiMedia/")
    print("3. If dimensions match, the images in EPUB are already large")
    print("   (EPUB readers scale them down via CSS for display)")
    print("=" * 80)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python diagnose_image_sizes.py <epub_file>")
        sys.exit(1)

    epub_path = sys.argv[1]
    if not Path(epub_path).exists():
        print(f"Error: File not found: {epub_path}")
        sys.exit(1)

    diagnose_epub_images(epub_path)
