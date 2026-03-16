#!/usr/bin/env python3
import argparse
import logging
import re
import shutil
import subprocess
import sys
import tempfile
import time
import webbrowser
import zipfile
from pathlib import Path
from subprocess import run
from typing import Optional

from lxml import etree
from pdf_mapper_wrapper import (count_resources, detect_template_type,
                                integrate_pdf_with_mapper)

import epub_to_structured_v2
from comprehensive_dtd_fixer import \
    process_zip_package as comprehensive_fix_dtd
from conversion_tracker import (ConversionStatus, ConversionTracker,
                                ConversionType, TemplateType)
from fix_chapters_simple import process_zip_package as fix_chapter_violations
from fix_misclassified_figures import \
    process_zip_package as fix_misclassified_figures
from package import make_file_fetcher, package_docbook
from pipeline_controller import PipelineController
from reference_mapper import get_mapper, reset_mapper
from validate_with_entity_tracking import EntityTrackingValidator
from validation_report import ValidationReportGenerator
from xslt_transformer import transform_to_rittdoc_compliance

ROOT = Path(__file__).resolve().parent


def sh(cmd, *, cwd: Path = ROOT) -> None:
    """Run a subprocess while echoing the command."""
    print("+", " ".join(cmd))
    run(cmd, check=True, cwd=cwd)


def _sanitize_basename(name: str) -> str:
    cleaned = re.sub(r"[^0-9A-Za-z_-]", "", name)
    return cleaned or "book"


def prompt_yes_no(question: str, default: str = "n") -> bool:
    """
    Prompt user for yes/no input.
    Args:
        question: The question to ask
        default: Default value if user just presses Enter ('y' or 'n')
    Returns:
        True if user chose yes, False otherwise
    """
    default = default.lower()
    prompt = f"{question} [y/n] (default: {default}): "

    while True:
        try:
            response = input(prompt).strip().lower()
            if not response:
                response = default

            if response in ('y', 'yes'):
                return True
            elif response in ('n', 'no'):
                return False
            else:
                print("Please enter 'y' or 'n'")
        except (KeyboardInterrupt, EOFError):
            print()
            return False


def launch_editor(xml_path: Path, pdf_path: Path, working_dir: Path) -> None:
    """
    Launch the RittDoc Editor for viewing/editing the converted files.
    Args:
        xml_path: Path to the XML file to edit
        pdf_path: Path to the reference file to view (PDF or EPUB)
        working_dir: Working directory containing the files
    """
    editor_dir = ROOT / "editor"
    editor_script = editor_dir / "server.py"

    if not editor_script.exists():
        print(f"\n[!] Editor not found at {editor_script}")
        print("    Please ensure the editor is installed in the 'editor' directory")
        return

    # Determine file type for display
    file_type = "Not available"
    if pdf_path.exists():
        ext = pdf_path.suffix.lower()
        if ext == ".pdf":
            file_type = f"{pdf_path.name} (PDF)"
        elif ext in [".epub", ".epub3"]:
            file_type = f"{pdf_path.name} (EPUB - preview not yet supported)"
        else:
            file_type = pdf_path.name

    print("\n" + "="*60)
    print("  Launching RittDoc Editor...")
    print("="*60)
    print(f"  XML:        {xml_path.name}")
    print(f"  Reference:  {file_type}")
    print(f"  Server:     http://127.0.0.1:5000")
    print("="*60)

    # Build command to launch editor
    cmd = [
        sys.executable,  # Use the same Python interpreter
        str(editor_script),
        "--xml", str(xml_path),
    ]

    # Only add PDF argument if PDF exists
    if pdf_path.exists():
        cmd.extend(["--pdf", str(pdf_path)])

    try:
        # Launch the editor server
        print("\nStarting editor server...")
        process = subprocess.Popen(
            cmd,
            cwd=str(editor_dir),
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True
        )

        # Wait a moment for server to start
        time.sleep(2)

        # Check if server started successfully
        if process.poll() is not None:
            # Server exited immediately - there was an error
            stdout, stderr = process.communicate()
            print(f"\n[!] Editor failed to start:")
            if stderr:
                print(stderr)
            if stdout:
                print(stdout)
            return

        # Open browser
        editor_url = "http://127.0.0.1:5000"
        print(f"Opening browser at {editor_url}...")
        webbrowser.open(editor_url)

        print("\n" + "="*60)
        print("  Editor is running!")
        print("="*60)
        print("  Press Ctrl+C to stop the editor and return to terminal")
        print("="*60 + "\n")

        # Wait for user to stop the server
        try:
            process.wait()
        except KeyboardInterrupt:
            print("\n\nStopping editor server...")
            process.terminate()
            try:
                process.wait(timeout=5)
            except subprocess.TimeoutExpired:
                process.kill()
            print("Editor stopped.")

    except Exception as e:
        print(f"\n[!] Error launching editor: {e}")
        import traceback
        traceback.print_exc()


def validate_packaged_xml(zip_path: Path) -> tuple[bool, str, Optional[object]]:
    """
    Validate Book.XML inside the generated package against the RittDoc DTD.
    Returns (is_valid, report_text, error_log). Falls back to the local workspace DTD copy if the
    package does not include one.
    """
    if not zip_path.exists():
        return False, f"Package not found: {zip_path}", None

    with tempfile.TemporaryDirectory(prefix="ritt_validate_") as tmp:
        extract_dir = Path(tmp)
        with zipfile.ZipFile(zip_path, "r") as zf:
            zf.extractall(extract_dir)

        book_xml = extract_dir / "Book.XML"
        if not book_xml.exists():
            return False, "Book.XML not found in package.", None

        dtd_path = extract_dir / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"
        if not dtd_path.exists():
            fallback_dtd = ROOT / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"
            if fallback_dtd.exists():
                dtd_path = fallback_dtd
            else:
                return False, "RittDocBook.dtd not available for validation.", None

        parser = etree.XMLParser(load_dtd=True, resolve_entities=True, no_network=True)
        try:
            tree = etree.parse(str(book_xml), parser)
        except etree.XMLSyntaxError as exc:
            return False, f"XML parsing failed: {exc}", None

        dtd = etree.DTD(str(dtd_path))
        is_valid = dtd.validate(tree)
        if is_valid:
            return True, "DTD validation passed.", None

        error_lines = "\n".join(str(err) for err in dtd.error_log)
        return False, f"DTD validation failed:\n{error_lines.strip() or 'Unknown error'}", dtd.error_log


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Run the end-to-end PDF/ePub → RittDoc packaging pipeline."
    )
    parser.add_argument("input", help="Input file path (PDF or ePub)")
    parser.add_argument(
        "out_dir",
        nargs="?",
        help="Directory where the final ZIP will be written (default: ./Output)",
    )
    parser.add_argument(
        "--font-only-structure",
        action="store_true",
        help="Skip heuristic block labeling during structuring and rely on font tiers only.",
    )
    parser.add_argument(
        "--format",
        choices=["pdf", "epub", "auto"],
        default="auto",
        help="Input format (default: auto-detect from extension)",
    )
    parser.add_argument(
        "--debug",
        action="store_true",
        help="Enable debug logging for detailed diagnostics",
    )
    args = parser.parse_args()

    # Configure logging based on --debug flag
    log_level = logging.DEBUG if args.debug else logging.INFO
    logging.basicConfig(
        level=log_level,
        format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
        datefmt="%H:%M:%S"
    )

    input_path = Path(args.input).resolve()
    if not input_path.exists():
        parser.error(f"Input file not found: {input_path}")

    # Detect format
    if args.format == "auto":
        ext = input_path.suffix.lower()
        if ext == ".pdf":
            input_format = "pdf"
        elif ext in [".epub", ".epub3"]:
            input_format = "epub"
        else:
            parser.error(f"Cannot auto-detect format from extension '{ext}'. Use --format to specify.")
    else:
        input_format = args.format

    out_dir = Path(args.out_dir).resolve() if args.out_dir else (ROOT / "Output")
    out_dir.mkdir(parents=True, exist_ok=True)
    extracted_dir = out_dir / "extracted"
    if extracted_dir.exists():
        print(f"! Removing stale extracted directory at {extracted_dir}")
        shutil.rmtree(extracted_dir)

    isbn = _sanitize_basename(input_path.stem)
    final_zip_path = out_dir / f"{isbn}.zip"

    print(f"\n=== Input Format: {input_format.upper()} ===")

    # Initialize conversion tracking
    conversion_type = ConversionType.EPUB if input_format == "epub" else ConversionType.PDF
    tracker = ConversionTracker(out_dir)
    tracker.start_conversion(
        filename=input_path.name,
        conversion_type=conversion_type,
        isbn=isbn
    )
    tracker.update_progress(5, ConversionStatus.IN_PROGRESS)

    # Initialize interactive pipeline controller
    controller = PipelineController()

    # Reset reference mapper for this conversion
    reset_mapper()
    mapper = get_mapper()

    validation_passed = None
    try:
        with tempfile.TemporaryDirectory(prefix="pipeline_") as tmp:
            work_dir = Path(tmp)
            reading_xml = work_dir / "reading_order.xml"
            enriched_reading_xml = work_dir / "reading_order_enriched.xml"
            font_roles_json = work_dir / "font_roles.json"
            media_xml = work_dir / "media.xml"
            structured_xml = work_dir / "structured.xml"
            epub_temp_dir = None  # Initialize for later use in media_fetcher

            # Route to appropriate pipeline based on format
            if input_format == "epub":
                # ePub Pipeline: Use new v2 processor with reference mapping
                print("\n=== ePub Conversion Pipeline (v2) ===")
                print("=== Step 1: Converting ePub to structured.xml ===")
                epub_temp_dir = work_dir / "epub_temp"

                # Use the new ePub processor directly
                epub_to_structured_v2.convert_epub_to_structured_v2(
                    epub_path=input_path,
                    output_xml=structured_xml,
                    temp_dir=epub_temp_dir,
                    tracker=tracker
                )
                tracker.update_progress(50)
                controller.prompt_continue("ePub to Structured XML Conversion")

            elif input_format == "pdf":
                # PDF Pipeline: Original multi-step process with reference mapping
                print("\n=== PDF Conversion Pipeline ===")
                tracker.update_progress(10)

                print("\n=== Step 1: Reading Order ===")
                sh(["python3", "grid_reading_order.py", str(input_path), str(reading_xml)])
                tracker.update_progress(20)
                controller.prompt_continue("Reading Order Extraction")

                print("\n=== Step 1.5: Enrich Reading Order with Font Sizes ===")
                sh(["python3", "enrich_reading_order.py", str(input_path), str(reading_xml), "--out", str(enriched_reading_xml)])
                reading_xml = enriched_reading_xml
                tracker.update_progress(25)
                controller.prompt_continue("Enrich Reading Order")

                print("\n=== Step 2: Auto Font Roles ===")
                sh(
                    [
                        "python3",
                        "font_roles_auto.py",
                        str(reading_xml),
                        "--out",
                        str(font_roles_json),
                        "--max-roles",
                        "5",
                        "--size-decimals",
                        "2",
                        "--ignore-small",
                        "6.0",
                    ]
                )
                tracker.update_progress(30)
                controller.prompt_continue("Font Role Detection")

                print("\n=== Step 3: Media Extraction ===")
                sh(
                    [
                        "python3",
                        "media_extractor_IgnoreVectorTabels.py",
                        str(input_path),
                        "--out",
                        str(media_xml),
                        "--text-boundary",
                        "15",
                        "--no-input",
                    ]
                )
                tracker.update_progress(40)
                controller.prompt_continue("Media Extraction")

                print("\n=== Step 4: Flow Building ===")
                flow_cmd = [
                    "python3",
                    "flow_builder.py",
                    "--font-roles",           # ← TOP-LEVEL FLAGS FIRST
                    str(font_roles_json),
                    "build+bind+structure",   # ← THEN SUBCOMMAND
                    "--reading",              # ← THEN SUBCOMMAND FLAGS
                    str(reading_xml),
                    "--media",
                    str(media_xml),
                    "--labels",
                    str(font_roles_json),
                    "--out",
                    str(structured_xml),
                ]
                if args.font_only_structure:
                    flow_cmd.append("--font-only")
                sh(flow_cmd)
                tracker.update_progress(50)
                controller.prompt_continue("Flow Building")

                # Integrate PDF media with reference mapper
                print("\n=== Integrating PDF reference mapping ===")
                integrate_pdf_with_mapper(media_xml, structured_xml, mapper)
                tracker.update_progress(55)
                controller.prompt_continue("PDF Reference Mapping")

            else:
                raise ValueError(f"Unsupported input format: {input_format}")

            # Collect statistics for tracking
            print("\n=== Collecting conversion statistics ===")
            resource_counts = count_resources(structured_xml)
            template_type_str = detect_template_type(structured_xml)

            tracker.current_metadata.num_chapters = resource_counts.get('num_chapters', 0)
            tracker.current_metadata.num_vector_images = mapper.stats['vector_images']
            tracker.current_metadata.num_raster_images = mapper.stats['raster_images']
            tracker.current_metadata.num_tables = resource_counts.get('num_tables', 0)
            tracker.current_metadata.num_equations = resource_counts.get('num_equations', 0)
            tracker.current_metadata.template_type = TemplateType(template_type_str)
            tracker.update_progress(60)
            controller.prompt_continue("Statistics Collection")

            print("\n=== Step 4.5: XSLT Transformation for DTD Compliance ===")
            # Apply XSLT transformation to ensure RittDoc DTD compliance
            structured_xml_compliant = work_dir / "structured_compliant.xml"
            try:
                transform_to_rittdoc_compliance(structured_xml, structured_xml_compliant)
                print("[OK] XSLT transformation completed - XML is now DTD compliant")
                # Use the compliant version for packaging
                structured_xml = structured_xml_compliant
            except Exception as e:
                print(f"[!] XSLT transformation warning: {e}")
                print("  Continuing with original structured.xml")
            tracker.update_progress(65)
            controller.prompt_continue("XSLT Transformation")

            print("\n=== Step 5: Packaging ===")
            structured_root = etree.parse(str(structured_xml)).getroot()
            root_name = (
                structured_root.tag.split("}", 1)[-1]
                if structured_root.tag.startswith("{")
                else structured_root.tag
            )

            assets: list[tuple[str, Path]] = []
            # Include epub_temp_dir in search paths for ePub conversions
            search_paths = [work_dir]
            if epub_temp_dir is not None and epub_temp_dir.exists():
                search_paths.append(epub_temp_dir)

            # Get reference mapper to enable MediaFetcher to resolve final names → intermediate names
            reference_mapper = get_mapper()
            media_fetcher = make_file_fetcher(search_paths, reference_mapper=reference_mapper)

            # Check for TOC structure JSON (generated during ePub conversion)
            toc_json_path = work_dir / "toc_structure.json"
            if not toc_json_path.exists():
                toc_json_path = None

            final_zip_path = package_docbook(
                root=structured_root,
                root_name=root_name,
                dtd_system="RITTDOCdtd/v1.1/RittDocBook.dtd",
                zip_path=str(final_zip_path),
                processing_instructions=(),
                assets=assets,
                media_fetcher=media_fetcher,
                source_format=input_format,
                toc_json_path=toc_json_path,
            )
            tracker.update_progress(85)
            controller.prompt_continue("Packaging")

            print("\n=== Step 5.5: Pre-Fix Validation ===")
            # Run validation BEFORE fixes to establish baseline
            dtd_path = ROOT / "RITTDOCdtd" / "v1.1" / "RittDocBook.dtd"
            pre_validator = EntityTrackingValidator(dtd_path)
            pre_validation_report = pre_validator.validate_zip_package(
                zip_path=final_zip_path,
                output_report_path=None
            )
            errors_before_fixes = pre_validation_report.get_error_count()

            if errors_before_fixes > 0:
                print(f"[!] Found {errors_before_fixes} validation errors before fixes")
                # Show error breakdown by type
                error_types = {}
                for error in pre_validation_report.errors:
                    error_type = error.error_type
                    error_types[error_type] = error_types.get(error_type, 0) + 1

                print("  Error breakdown:")
                for error_type, count in sorted(error_types.items(), key=lambda x: -x[1])[:5]:
                    print(f"    {error_type}: {count}")
            else:
                print("[OK] No validation errors found - package is already DTD-compliant")

            tracker.update_progress(68)
            controller.prompt_continue("Pre-Fix Validation")

            print("\n=== Step 6: Comprehensive Automated DTD Fixes ===")
            # Apply comprehensive fixes to handle ALL common DTD violations
            # Run multiple passes until no more fixes are applied (max 3 passes)
            all_fixed_zip_path = out_dir / f"{isbn}_all_fixes.zip"

            # Create unified validation report
            unified_report = ValidationReportGenerator()

            # Multi-pass fixing: run until no more fixes or max passes reached
            MAX_FIX_PASSES = 3
            current_input = final_zip_path
            total_fixes_all_passes = 0
            total_files_fixed = 0

            for pass_num in range(1, MAX_FIX_PASSES + 1):
                print(f"  → Fix pass {pass_num}/{MAX_FIX_PASSES}...")

                # Output path for this pass
                pass_output = out_dir / f"{isbn}_fixes_pass{pass_num}.zip"

                comprehensive_stats = comprehensive_fix_dtd(
                    zip_path=current_input,
                    output_path=pass_output,
                    dtd_path=dtd_path,
                    generate_reports=False
                )

                fixes_this_pass = comprehensive_stats['total_fixes']
                files_fixed_this_pass = comprehensive_stats['files_fixed']

                if fixes_this_pass > 0:
                    print(f"    [OK] Pass {pass_num}: Fixed {files_fixed_this_pass} chapters, {fixes_this_pass} total fixes")
                    total_fixes_all_passes += fixes_this_pass
                    total_files_fixed += files_fixed_this_pass
                    current_input = pass_output
                else:
                    print(f"    [OK] Pass {pass_num}: No additional fixes needed")
                    # Clean up unused output file
                    if pass_output.exists() and pass_output != current_input:
                        pass_output.unlink()
                    break

                # Collect verification items
                for item in comprehensive_stats.get('verification_items', []):
                    unified_report.add_verification_item(item)

            # Rename final output
            if current_input != final_zip_path and current_input.exists():
                import shutil
                shutil.move(str(current_input), str(all_fixed_zip_path))
                # Clean up intermediate pass files
                for i in range(1, MAX_FIX_PASSES + 1):
                    intermediate = out_dir / f"{isbn}_fixes_pass{i}.zip"
                    if intermediate.exists() and intermediate != all_fixed_zip_path:
                        intermediate.unlink()
            else:
                # No fixes needed, just copy
                import shutil
                shutil.copy2(str(final_zip_path), str(all_fixed_zip_path))

            print(f"    [OK] Total: Fixed {total_files_fixed} chapters with {total_fixes_all_passes} fixes across {pass_num} pass(es)")

            # Use the fully fixed version as final
            final_zip_path = all_fixed_zip_path
            tracker.update_progress(75)
            controller.prompt_continue("Comprehensive DTD Fixes")

            print("\n=== Step 7: Post-Fix DTD Validation ===")
            # Validate the fixed package to measure improvement
            post_validator = EntityTrackingValidator(dtd_path)
            post_validation_report = post_validator.validate_zip_package(
                zip_path=final_zip_path,
                output_report_path=None  # Don't generate report yet
            )

            errors_after_fixes = post_validation_report.get_error_count()
            errors_fixed = errors_before_fixes - errors_after_fixes
            improvement_pct = (errors_fixed / errors_before_fixes * 100) if errors_before_fixes > 0 else 0

            # Show validation results with comparison
            print(f"\n[STATS] Validation Results Comparison:")
            print(f"  Errors before fixes:  {errors_before_fixes}")
            print(f"  Errors after fixes:   {errors_after_fixes}")
            print(f"  Errors fixed:         {errors_fixed}")
            print(f"  Improvement:          {improvement_pct:.1f}%")

            # Merge post-fix validation errors into unified report
            for error in post_validation_report.errors:
                unified_report.add_error(error)

            # Merge verification items
            for item in post_validation_report.verification_items:
                unified_report.add_verification_item(item)

            validation_passed = not post_validation_report.has_errors()
            if validation_passed:
                print("\n[OK] DTD validation PASSED - Package is fully compliant!")
            else:
                print(f"\n[!] {errors_after_fixes} validation errors remain")
                # Show remaining error breakdown by type
                error_types = {}
                for error in post_validation_report.errors:
                    error_type = error.error_type
                    error_types[error_type] = error_types.get(error_type, 0) + 1

                print("\n  Remaining error types:")
                for error_type, count in sorted(error_types.items(), key=lambda x: -x[1])[:5]:
                    print(f"    {error_type}: {count}")

                # Show sample errors
                print("\n  Sample remaining errors:")
                for i, error in enumerate(post_validation_report.errors[:3]):
                    desc = error.error_description[:100] + "..." if len(error.error_description) > 100 else error.error_description
                    print(f"    {i+1}. {error.xml_file}:{error.line_number} - {desc}")

                # Show files with most errors
                errors_by_file = {}
                for error in post_validation_report.errors:
                    if error.xml_file not in errors_by_file:
                        errors_by_file[error.xml_file] = 0
                    errors_by_file[error.xml_file] += 1

                print("\n  Files with most errors:")
                for filename, count in sorted(errors_by_file.items(), key=lambda x: -x[1])[:5]:
                    print(f"    {filename}: {count} error(s)")

                # Rename output to indicate it needs review
                needs_review_path = out_dir / f"{isbn}_NEEDS_REVIEW.zip"
                if final_zip_path.exists():
                    shutil.move(str(final_zip_path), str(needs_review_path))
                    final_zip_path = needs_review_path
                    print(f"\n  [!] Output renamed to indicate validation issues: {needs_review_path.name}")

            tracker.update_progress(85)
            controller.prompt_continue("Post-Fix Validation")

            # Generate unified validation report (Excel format)
            print("\n=== Step 8: Generating Unified Validation Report ===")
            # Get reference validation errors
            _, reference_errors = mapper.validate(out_dir)

            # Add reference errors to unified report
            for ref_error in reference_errors:
                unified_report.add_general_error(
                    xml_filename="Package",
                    error_type="Reference Error",
                    description=ref_error,
                    severity="Warning"
                )

            # Add improvement summary to report
            unified_report.add_general_error(
                xml_filename="SUMMARY",
                error_type="Validation Summary",
                description=f"Pre-fix errors: {errors_before_fixes} | Post-fix errors: {errors_after_fixes} | Fixed: {errors_fixed} | Improvement: {improvement_pct:.1f}%",
                severity="Info"
            )

            # Generate Excel validation report with error handling
            report_path = out_dir / f"{isbn}_validation_report.xlsx"
            book_title = tracker.current_metadata.title or isbn

            report_saved = False
            try:
                unified_report.generate_excel_report(report_path, book_title)
                report_saved = True
                print(f"  - Post-fix validation errors: {len(unified_report.errors) - 1}")  # -1 for summary
                print(f"  - Verification items: {len(unified_report.verification_items)}")
                print(f"  - Errors fixed by automation: {errors_fixed}")
                if unified_report.verification_items:
                    print(f"  - Review 'Manual Verification' sheet for items requiring content check")
                if errors_after_fixes > 0:
                    print(f"  - [!] {errors_after_fixes} errors require manual review (see Excel report)")
            except PermissionError as e:
                # Report couldn't be saved, but conversion is still successful
                print(f"\n[!] Warning: Could not save validation report")
                print(f"  Reason: {str(e)}")
                print(f"\n  Validation Summary (Console Output):")
                print(f"    - Post-fix validation errors: {len(unified_report.errors) - 1}")
                print(f"    - Verification items: {len(unified_report.verification_items)}")
                print(f"    - Errors fixed by automation: {errors_fixed}")
                if errors_after_fixes > 0:
                    print(f"    - [!] {errors_after_fixes} validation errors remain")
                    # Print first few errors to console
                    print(f"\n  Sample remaining errors:")
                    for i, error in enumerate(unified_report.errors[:5]):
                        if error.xml_file != "SUMMARY":  # Skip summary row
                            print(f"    {i+1}. {error.xml_file}:{error.line_number} - {error.error_type}")
                print(f"\n  [TIP] Close Excel and retry the conversion to generate the report")

            tracker.update_progress(90)
            controller.prompt_continue("Validation Report Generation")

            print("\n=== Step 9: Saving Intermediate Artifacts ===")
            intermediate_dir = out_dir / f"{isbn}_intermediate"
            intermediate_dir.mkdir(parents=True, exist_ok=True)

            # For PDF, save all intermediate files
            if input_format == "pdf":
                artifacts = [
                    (reading_xml, "reading_order.xml"),
                    (font_roles_json, "font_roles.json"),
                    (media_xml, "media.xml"),
                    (structured_xml, "structured_compliant.xml"),
                ]
            # For ePub, only save structured.xml (no PDF-specific intermediates)
            else:
                artifacts = [
                    (structured_xml, "structured_compliant.xml"),
                ]

            for source, filename in artifacts:
                if source.exists():
                    target = intermediate_dir / filename
                    shutil.copy2(source, target)
                    print(f"  → Saved {(target.relative_to(out_dir)).as_posix()}")

            tracker.update_progress(95)
            controller.prompt_continue("Saving Intermediate Artifacts")

        # Calculate output size
        if final_zip_path.exists():
            output_size_mb = final_zip_path.stat().st_size / (1024 * 1024)
            tracker.current_metadata.output_size_mb = output_size_mb
            tracker.current_metadata.output_path = str(final_zip_path)

        # Complete tracking with success
        tracker.complete_conversion(
            status=ConversionStatus.SUCCESS,
            num_chapters=tracker.current_metadata.num_chapters,
            num_vector_images=tracker.current_metadata.num_vector_images,
            num_raster_images=tracker.current_metadata.num_raster_images,
            num_tables=tracker.current_metadata.num_tables,
        )

        print("\n=== DONE ===")
        if validation_passed is True:
            status_icon = "[OK]"
            validation_msg = f"DTD validation PASSED (fixed {errors_fixed} errors, {improvement_pct:.1f}% improvement)"
        elif validation_passed is False:
            status_icon = "[!]"
            validation_msg = f"DTD validation: {errors_after_fixes} errors remain (fixed {errors_fixed}/{errors_before_fixes})"
        else:
            status_icon = "[!]"
            validation_msg = "DTD validation skipped"
        print(f"{status_icon} RittDoc package: {final_zip_path}")
        print(f"  {validation_msg}")
        if report_saved:
            print(f"  Validation report: {report_path}")
        else:
            print(f"  Validation report: Not saved (file was locked - close Excel and retry)")

        # Prompt to open editor
        print("\n" + "="*60)
        if prompt_yes_no("Would you like to open this conversion in the editor?", default="y"):
            # Extract XML from package for editing
            with tempfile.TemporaryDirectory(prefix="editor_extract_") as tmp:
                extract_dir = Path(tmp)
                with zipfile.ZipFile(final_zip_path, "r") as zf:
                    zf.extractall(extract_dir)

                # Find Book.XML
                book_xml = extract_dir / "Book.XML"
                if not book_xml.exists():
                    print("[!] Book.XML not found in package - cannot launch editor")
                else:
                    # Copy XML to output directory for editing
                    editable_xml = out_dir / f"{isbn}_editable.xml"
                    shutil.copy2(book_xml, editable_xml)

                    # Generate HTML preview if it doesn't exist
                    html_preview = out_dir / f"{isbn}_preview.html"
                    if not html_preview.exists():
                        # Create basic HTML preview
                        html_content = f"""<!DOCTYPE html>
<html>
<head>
    <title>{isbn} Preview</title>
    <style>
        body {{ font-family: Arial, sans-serif; padding: 2rem; max-width: 900px; margin: 0 auto; }}
        h1, h2, h3 {{ color: #2563eb; }}
        p {{ line-height: 1.6; }}
    </style>
</head>
<body>
    <h1>Document Preview</h1>
    <p>Use "Refresh HTML" in the editor to generate preview from XML.</p>
</body>
</html>"""
                        html_preview.write_text(html_content, encoding='utf-8')

                    # Determine file to display alongside XML
                    if input_format == "pdf":
                        reference_file = input_path  # Use the original PDF
                    elif input_format == "epub":
                        reference_file = input_path  # Use the original EPUB
                    else:
                        reference_file = out_dir / f"{isbn}.pdf"  # Fallback

                    # Launch editor
                    launch_editor(
                        xml_path=editable_xml,
                        pdf_path=reference_file,
                        working_dir=out_dir
                    )
        else:
            print("Editor launch skipped. You can manually run the editor later with:")
            print(f"  cd editor && python server.py --xml {out_dir / f'{isbn}_editable.xml'}")

        # Stop the controller
        controller.stop()

    except Exception as e:
        # Handle conversion failure
        import traceback
        error_msg = f"{type(e).__name__}: {str(e)}"
        print(f"\n[ERROR] Conversion FAILED: {error_msg}", file=sys.stderr)
        traceback.print_exc()

        tracker.complete_conversion(
            status=ConversionStatus.FAILURE,
            error_message=error_msg
        )

        # Stop the controller
        controller.stop()
        raise


if __name__ == "__main__":
    main()