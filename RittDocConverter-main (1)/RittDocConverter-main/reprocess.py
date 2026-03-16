#!/usr/bin/env python3
"""
Reprocessing Module

Handles reprocessing of edited RittDoc XML files:
- Applies XSLT transformation
- Repackages into ZIP
- Runs DTD fixing (multi-pass)
- Validates against DTD
- Generates updated reports

This module is called after edits are saved in the editor to produce
updated conversion output without re-running the full EPUB conversion.
"""

import logging
import shutil
import tempfile
import zipfile
from datetime import datetime
from pathlib import Path
from typing import Any, Dict, List, Optional, Set

from lxml import etree

logger = logging.getLogger(__name__)

from comprehensive_dtd_fixer import \
    process_zip_package as comprehensive_fix_dtd
from validate_with_entity_tracking import EntityTrackingValidator
from validation_report import ValidationReportGenerator
# Import processing modules
from xslt_transformer import transform_to_rittdoc_compliance

ROOT = Path(__file__).resolve().parent


def reprocess_package(
    extracted_dir: Path,
    original_zip_path: Path,
    output_dir: Optional[Path] = None,
    apply_xslt: bool = True,
    apply_dtd_fixes: bool = True,
    validate: bool = True,
    max_fix_passes: int = 3
) -> Dict[str, Any]:
    """
    Reprocess an edited RittDoc package.

    This function takes an extracted package directory (with edited XML files)
    and produces an updated, DTD-compliant package.

    Args:
        extracted_dir: Path to extracted package directory containing edited files
        original_zip_path: Path to the original ZIP file (for naming)
        output_dir: Output directory (default: same as original ZIP)
        apply_xslt: Whether to apply XSLT transformation
        apply_dtd_fixes: Whether to run DTD fixing passes
        validate: Whether to run final validation
        max_fix_passes: Maximum number of DTD fix passes

    Returns:
        Dict with:
            - success: bool
            - output_zip: Path to new package
            - validation_passed: bool
            - errors_fixed: int
            - remaining_errors: int
            - report_path: Path to validation report
            - message: str
    """
    result = {
        'success': False,
        'output_zip': None,
        'validation_passed': False,
        'errors_before': 0,
        'errors_fixed': 0,
        'remaining_errors': 0,
        'report_path': None,
        'message': ''
    }

    try:
        extracted_dir = Path(extracted_dir)
        original_zip_path = Path(original_zip_path)
        output_dir = Path(output_dir) if output_dir else original_zip_path.parent

        # Determine output naming
        base_name = original_zip_path.stem
        if base_name.endswith('_all_fixes'):
            base_name = base_name[:-10]  # Remove _all_fixes suffix
        if base_name.endswith('_NEEDS_REVIEW'):
            base_name = base_name[:-13]  # Remove _NEEDS_REVIEW suffix

        timestamp = datetime.now().strftime('%Y%m%d_%H%M%S')

        logger.info(f"Starting reprocessing of {base_name}")

        with tempfile.TemporaryDirectory(prefix="reprocess_") as tmp_dir:
            work_dir = Path(tmp_dir)

            # Step 1: Apply XSLT transformation to each chapter (if enabled)
            if apply_xslt:
                logger.info("Step 1: Applying XSLT transformations...")
                xslt_applied = 0

                for xml_file in extracted_dir.glob('*.xml'):
                    if xml_file.name.lower() == 'book.xml':
                        continue  # Skip Book.XML, it's the master file

                    try:
                        output_file = work_dir / xml_file.name
                        transform_to_rittdoc_compliance(xml_file, output_file)

                        # Copy transformed file back
                        shutil.copy2(output_file, xml_file)
                        xslt_applied += 1
                    except Exception as e:
                        logger.warning(f"XSLT transform warning for {xml_file.name}: {e}")

                logger.info(f"  Applied XSLT to {xslt_applied} files")

            # Step 2: Create intermediate ZIP package
            logger.info("Step 2: Creating intermediate package...")
            intermediate_zip = work_dir / f"{base_name}_intermediate.zip"

            with zipfile.ZipFile(intermediate_zip, 'w', zipfile.ZIP_DEFLATED) as zf:
                for file_path in extracted_dir.rglob('*'):
                    if file_path.is_file():
                        arcname = file_path.relative_to(extracted_dir)
                        zf.write(file_path, arcname)

            # Step 3: Run pre-fix validation to get baseline
            dtd_path = ROOT / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"

            if validate:
                logger.info("Step 3: Pre-fix validation...")
                pre_validator = EntityTrackingValidator(dtd_path)
                pre_report = pre_validator.validate_zip_package(
                    zip_path=intermediate_zip,
                    output_report_path=None
                )
                result['errors_before'] = pre_report.get_error_count()
                logger.info(f"  Found {result['errors_before']} validation errors")

            # Step 4: Apply DTD fixes (multi-pass)
            current_zip = intermediate_zip
            total_fixes = 0

            if apply_dtd_fixes and result['errors_before'] > 0:
                logger.info("Step 4: Applying DTD fixes...")

                for pass_num in range(1, max_fix_passes + 1):
                    pass_output = work_dir / f"{base_name}_pass{pass_num}.zip"

                    fix_stats = comprehensive_fix_dtd(
                        zip_path=current_zip,
                        output_path=pass_output,
                        dtd_path=dtd_path,
                        generate_reports=False
                    )

                    fixes_this_pass = fix_stats.get('total_fixes', 0)

                    if fixes_this_pass > 0:
                        logger.info(f"  Pass {pass_num}: Applied {fixes_this_pass} fixes")
                        total_fixes += fixes_this_pass
                        current_zip = pass_output
                    else:
                        logger.info(f"  Pass {pass_num}: No additional fixes needed")
                        if pass_output.exists():
                            pass_output.unlink()
                        break

                result['errors_fixed'] = total_fixes

            # Step 5: Final validation
            if validate:
                logger.info("Step 5: Final validation...")
                post_validator = EntityTrackingValidator(dtd_path)
                post_report = post_validator.validate_zip_package(
                    zip_path=current_zip,
                    output_report_path=None
                )
                result['remaining_errors'] = post_report.get_error_count()
                result['validation_passed'] = result['remaining_errors'] == 0

                logger.info(f"  Remaining errors: {result['remaining_errors']}")

            # Step 6: Create final output
            logger.info("Step 6: Creating final package...")

            if result['validation_passed']:
                final_name = f"{base_name}_all_fixes.zip"
            else:
                final_name = f"{base_name}_NEEDS_REVIEW.zip"

            final_zip = output_dir / final_name

            # Backup existing file if present
            if final_zip.exists():
                backup_path = output_dir / f"{base_name}_backup_{timestamp}.zip"
                shutil.move(str(final_zip), str(backup_path))
                logger.info(f"  Backed up existing file to {backup_path.name}")

            # Copy final result
            shutil.copy2(current_zip, final_zip)
            result['output_zip'] = str(final_zip)

            # Step 7: Generate validation report
            if validate:
                logger.info("Step 7: Generating validation report...")
                report_path = output_dir / f"{base_name}_validation_report.xlsx"

                unified_report = ValidationReportGenerator()

                for error in post_report.errors:
                    unified_report.add_error(error)

                for item in post_report.verification_items:
                    unified_report.add_verification_item(item)

                # Add summary
                improvement_pct = (result['errors_fixed'] / result['errors_before'] * 100) if result['errors_before'] > 0 else 0
                unified_report.add_general_error(
                    xml_filename="SUMMARY",
                    error_type="Reprocessing Summary",
                    description=f"Pre-edit errors: {result['errors_before']} | Post-edit errors: {result['remaining_errors']} | Fixed: {result['errors_fixed']} | Improvement: {improvement_pct:.1f}%",
                    severity="Info"
                )

                try:
                    unified_report.generate_excel_report(report_path, base_name)
                    result['report_path'] = str(report_path)
                except Exception as e:
                    logger.warning(f"Could not save report: {e}")

            # Success
            result['success'] = True
            if result['validation_passed']:
                result['message'] = f"Reprocessing complete. Package is DTD-compliant. Fixed {result['errors_fixed']} errors."
            else:
                result['message'] = f"Reprocessing complete. {result['remaining_errors']} errors remain. Fixed {result['errors_fixed']} errors."

            logger.info(f"Reprocessing complete: {result['message']}")

    except Exception as e:
        logger.exception("Reprocessing failed")
        result['success'] = False
        result['message'] = f"Reprocessing failed: {str(e)}"

    return result


def reprocess_from_xml(
    xml_content: str,
    chapters: List[Dict[str, str]],
    output_dir: Path,
    base_name: str,
    multimedia_dir: Optional[Path] = None
) -> Dict[str, Any]:
    """
    Reprocess from XML content (for non-package mode editing).

    Args:
        xml_content: The edited XML content (combined or single file)
        chapters: List of chapter info dicts with 'file' and 'content' keys
        output_dir: Output directory
        base_name: Base name for output files
        multimedia_dir: Path to multimedia files

    Returns:
        Dict with reprocessing results
    """
    result = {
        'success': False,
        'output_zip': None,
        'message': ''
    }

    try:
        with tempfile.TemporaryDirectory(prefix="reprocess_xml_") as tmp_dir:
            work_dir = Path(tmp_dir)
            package_dir = work_dir / "package"
            package_dir.mkdir()

            # Write chapters
            for chapter in chapters:
                chapter_file = package_dir / chapter['file']
                chapter_file.write_text(chapter['content'], encoding='utf-8')

            # Write Book.XML if provided separately
            book_xml = package_dir / "Book.XML"
            if not book_xml.exists():
                # Generate Book.XML from chapters
                book_content = generate_book_xml(chapters, base_name)
                book_xml.write_text(book_content, encoding='utf-8')

            # Copy multimedia
            if multimedia_dir and multimedia_dir.exists():
                dest_multimedia = package_dir / "multimedia"
                shutil.copytree(multimedia_dir, dest_multimedia)

            # Create temporary ZIP for reprocessing
            temp_zip = work_dir / f"{base_name}.zip"
            with zipfile.ZipFile(temp_zip, 'w', zipfile.ZIP_DEFLATED) as zf:
                for file_path in package_dir.rglob('*'):
                    if file_path.is_file():
                        arcname = file_path.relative_to(package_dir)
                        zf.write(file_path, arcname)

            # Run reprocessing
            result = reprocess_package(
                extracted_dir=package_dir,
                original_zip_path=temp_zip,
                output_dir=output_dir
            )

    except Exception as e:
        logger.exception("Reprocessing from XML failed")
        result['success'] = False
        result['message'] = f"Reprocessing failed: {str(e)}"

    return result


def generate_book_xml(chapters: List[Dict[str, str]], base_name: str) -> str:
    """Generate a Book.XML file from chapter list."""
    entities = []
    entity_refs = []
    seen_entities: Set[str] = set()

    for chapter in chapters:
        filename = chapter['file']
        entity_name = Path(filename).stem
        if entity_name in seen_entities:
            logger.warning(
                "Skipping duplicate chapter entity %s for %s while generating Book.XML",
                entity_name,
                filename,
            )
            continue
        seen_entities.add(entity_name)
        entities.append(f'<!ENTITY {entity_name} SYSTEM "{filename}">')
        entity_refs.append(f'&{entity_name};')

    entity_decls = '\n  '.join(entities)
    entity_content = '\n'.join(entity_refs)

    return f'''<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE book PUBLIC "-//RITT//DTD DocBook V5.0//EN" "RITTDOCdtd/v1.1/RittDocBook.dtd" [
  {entity_decls}
]>
<book>
{entity_content}
</book>
'''


if __name__ == "__main__":
    import argparse

    parser = argparse.ArgumentParser(description="Reprocess edited RittDoc package")
    parser.add_argument("package", help="Path to ZIP package or extracted directory")
    parser.add_argument("--output", "-o", help="Output directory")
    parser.add_argument("--no-xslt", action="store_true", help="Skip XSLT transformation")
    parser.add_argument("--no-fix", action="store_true", help="Skip DTD fixing")
    parser.add_argument("--no-validate", action="store_true", help="Skip validation")
    args = parser.parse_args()

    logging.basicConfig(
        level=logging.INFO,
        format="%(asctime)s - %(levelname)s - %(message)s"
    )

    package_path = Path(args.package)

    if package_path.is_file() and package_path.suffix == '.zip':
        # Extract first
        with tempfile.TemporaryDirectory() as tmp:
            extracted = Path(tmp) / "extracted"
            with zipfile.ZipFile(package_path, 'r') as zf:
                zf.extractall(extracted)

            result = reprocess_package(
                extracted_dir=extracted,
                original_zip_path=package_path,
                output_dir=Path(args.output) if args.output else None,
                apply_xslt=not args.no_xslt,
                apply_dtd_fixes=not args.no_fix,
                validate=not args.no_validate
            )
    else:
        # Already extracted
        result = reprocess_package(
            extracted_dir=package_path,
            original_zip_path=package_path.parent / f"{package_path.name}.zip",
            output_dir=Path(args.output) if args.output else None,
            apply_xslt=not args.no_xslt,
            apply_dtd_fixes=not args.no_fix,
            validate=not args.no_validate
        )

    print(f"\nResult: {result['message']}")
    if result['output_zip']:
        print(f"Output: {result['output_zip']}")
