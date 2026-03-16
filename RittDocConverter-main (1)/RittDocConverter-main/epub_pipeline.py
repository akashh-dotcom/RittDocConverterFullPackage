#!/usr/bin/env python3
"""
ePub to RittDoc Pipeline

Main entry point for converting ePub files to RittDoc format.
This is a refactored version of integrated_pipeline.py that focuses
exclusively on EPUB processing.

Usage:
    python epub_pipeline.py input.epub [output_dir]
    python epub_pipeline.py input.epub --debug

Author: RittDocConverter Team
"""

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

import epub_to_structured_v2
from comprehensive_dtd_fixer import \
    process_zip_package as comprehensive_fix_dtd
from conversion_tracker import (ConversionStatus, ConversionTracker,
                                ConversionType, TemplateType)
from fix_chapters_simple import process_zip_package as fix_chapter_violations
from fix_misclassified_figures import \
    process_zip_package as fix_misclassified_figures
from content_diff import run_content_diff
from package import make_file_fetcher, package_docbook
from pipeline_controller import PipelineController
from fxl_preprocessor import detect_fixed_layout, preprocess_fxl_epub
from reference_mapper import get_mapper, reset_mapper
from validate_with_entity_tracking import EntityTrackingValidator

# Parallel processing support
try:
    from parallel_processor import (
        ParallelChapterProcessor, ParallelConfig,
        parallel_convert_chapters, estimate_optimal_workers
    )
    PARALLEL_PROCESSING_AVAILABLE = True
except ImportError:
    PARALLEL_PROCESSING_AVAILABLE = False

# Auto-detection constant for parallel workers
# -1 means auto-detect optimal worker count based on system resources
PARALLEL_AUTO_DETECT = -1

# Plugin architecture support
try:
    from plugin_architecture import (
        get_plugin_manager, reset_plugin_manager, register_builtin_plugins,
        PluginContext, PluginManager
    )
    PLUGIN_SYSTEM_AVAILABLE = True
except ImportError:
    PLUGIN_SYSTEM_AVAILABLE = False

from validate_rittdoc import (
    validate_component_rules,
    validate_entity_declarations,
    validate_invalid_element_nesting,
    validate_section_rules,
    validate_post_split_robustness,
    validate_package as validate_dtd_compliance,
    DTD_PATH,
)
from validation_report import ValidationReportGenerator, build_global_id_index
from xml_utils import count_resources, detect_template_type
from xslt_transformer import transform_to_rittdoc_compliance
from manual_postprocessor import process_zip_package as run_manual_postprocessing
from toc_linkend_validator import process_zip_package as validate_toc_linkends
from title_synchronizer import process_zip_package as sync_section_titles
from toc_nesting_fixer import process_zip_package as fix_toc_nesting

ROOT = Path(__file__).resolve().parent

# Reusable XML parsers (created once, reused throughout pipeline)
# This avoids overhead of creating parser objects multiple times
STRICT_XML_PARSER = etree.XMLParser(load_dtd=True, resolve_entities=True, no_network=True)
LENIENT_XML_PARSER = etree.XMLParser(dtd_validation=False, resolve_entities=False)


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


def launch_editor(xml_path: Path, epub_path: Path, working_dir: Path) -> None:
    """
    Launch the RittDoc Editor for viewing/editing the converted files.
    Args:
        xml_path: Path to the XML file to edit
        epub_path: Path to the reference EPUB file
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
    if epub_path.exists():
        file_type = f"{epub_path.name} (EPUB)"

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

    # Only add EPUB argument if it exists
    if epub_path.exists():
        cmd.extend(["--pdf", str(epub_path)])  # Uses --pdf arg but supports EPUB

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

        try:
            tree = etree.parse(str(book_xml), STRICT_XML_PARSER)
        except etree.XMLSyntaxError as exc:
            return False, f"XML parsing failed: {exc}", None

        dtd = etree.DTD(str(dtd_path))
        is_valid = dtd.validate(tree)
        if is_valid:
            return True, "DTD validation passed.", None

        error_lines = "\n".join(str(err) for err in dtd.error_log)
        return False, f"DTD validation failed:\n{error_lines.strip() or 'Unknown error'}", dtd.error_log


def convert_epub(
    input_path: Path,
    out_dir: Optional[Path] = None,
    debug: bool = False,
    interactive: bool = True,
    skip_content_diff: bool = False,
    max_chapters: Optional[int] = None,
    chapters: Optional[str] = None,
    parallel_workers: int = PARALLEL_AUTO_DETECT,
) -> Path:
    """
    Convert an EPUB file to RittDoc format.

    Args:
        input_path: Path to the input EPUB file
        out_dir: Output directory (default: ./Output)
        debug: Enable debug logging
        interactive: Enable interactive prompts (default: True)
        skip_content_diff: Skip content comparison (speeds up processing)
        max_chapters: Only process the first N chapters
        chapters: Specific chapter numbers to process (e.g., "1-3,7")
        parallel_workers: Number of parallel workers. Values:
            - PARALLEL_AUTO_DETECT (-1): Auto-detect optimal workers (default)
            - 0: Disable parallel processing
            - N > 0: Use exactly N workers

    Returns:
        Path to the generated ZIP package
    """
    # Resolve parallel workers: auto-detect if -1
    if parallel_workers == PARALLEL_AUTO_DETECT:
        if PARALLEL_PROCESSING_AVAILABLE:
            # Auto-detect based on system resources
            parallel_workers = estimate_optimal_workers(
                chapter_count=100,  # Estimate - will be refined after reading EPUB
                avg_chapter_size_kb=100,
                available_memory_mb=4096
            )
            logging.info(f"Auto-detected optimal parallel workers: {parallel_workers}")
        else:
            logging.info("Parallel processing not available - using sequential processing")
            parallel_workers = 0

    # Log parallel processing status
    if parallel_workers > 0:
        if PARALLEL_PROCESSING_AVAILABLE:
            logging.info(f"Parallel processing enabled with {parallel_workers} workers")
        else:
            logging.warning("Parallel processing requested but module not available - using sequential processing")
    # Configure logging based on debug flag
    log_level = logging.DEBUG if debug else logging.INFO
    logging.basicConfig(
        level=log_level,
        format="%(asctime)s - %(name)s - %(levelname)s - %(message)s",
        datefmt="%H:%M:%S"
    )

    input_path = Path(input_path).resolve()
    if not input_path.exists():
        raise FileNotFoundError(f"Input file not found: {input_path}")

    # Validate it's an EPUB file
    ext = input_path.suffix.lower()
    if ext not in [".epub", ".epub3"]:
        raise ValueError(f"Invalid file extension '{ext}'. Expected .epub or .epub3")

    out_dir = Path(out_dir).resolve() if out_dir else (ROOT / "Output")
    out_dir.mkdir(parents=True, exist_ok=True)
    extracted_dir = out_dir / "extracted"
    if extracted_dir.exists():
        print(f"! Removing stale extracted directory at {extracted_dir}")
        shutil.rmtree(extracted_dir)

    isbn = _sanitize_basename(input_path.stem)
    final_zip_path = out_dir / f"{isbn}.zip"

    print(f"\n=== Input Format: EPUB ===")

    # Initialize conversion tracking
    tracker = ConversionTracker(out_dir)
    tracker.start_conversion(
        filename=input_path.name,
        conversion_type=ConversionType.EPUB,
        isbn=isbn
    )
    tracker.update_progress(5, ConversionStatus.IN_PROGRESS)

    # Initialize interactive pipeline controller
    controller = PipelineController()
    if not interactive:
        controller.enabled = False

    # Reset reference mapper for this conversion
    reset_mapper()
    mapper = get_mapper()

    # Initialize plugin system
    plugin_context = None
    plugin_manager = None
    if PLUGIN_SYSTEM_AVAILABLE:
        plugin_manager = get_plugin_manager()
        # Add plugins directory if it exists
        plugins_dir = ROOT / "plugins"
        if plugins_dir.exists():
            plugin_manager.add_plugin_directory(plugins_dir)
            discovered = plugin_manager.discover_plugins()
            if discovered > 0:
                logging.info(f"Discovered {discovered} plugins from {plugins_dir}")
        # Register built-in plugins
        register_builtin_plugins(plugin_manager)
        logging.info(f"Plugin system initialized: {plugin_manager.total_plugins} plugins available")

        # Create plugin context for this conversion
        plugin_context = PluginContext(
            file_path=input_path,
            isbn=isbn,
            output_dir=out_dir,
            config={'debug': debug, 'parallel_workers': parallel_workers}
        )
        # Notify plugins that conversion is starting
        plugin_manager.notify_conversion_start(plugin_context)

    validation_passed = None
    try:
        with tempfile.TemporaryDirectory(prefix="pipeline_") as tmp:
            work_dir = Path(tmp)
            structured_xml = work_dir / "structured.xml"
            epub_temp_dir = work_dir / "epub_temp"

            # ePub Pipeline: Use v2 processor with reference mapping
            print("\n=== ePub Conversion Pipeline (v2) ===")

            # Step 0: Detect and preprocess fixed-layout EPUBs
            fxl_result = detect_fixed_layout(input_path)
            if fxl_result.is_fixed_layout:
                print(f"\n=== Step 0: Fixed-Layout EPUB Detected ===")
                print(f"  Confidence: {fxl_result.confidence:.0%}, "
                      f"Source: {fxl_result.source_tool}, "
                      f"Pages: {fxl_result.page_count}, "
                      f"Viewport: {fxl_result.viewport_width}x{fxl_result.viewport_height}")
                print(f"  Signals: {', '.join(fxl_result.signals)}")
                print("  Converting fixed-layout to reflowable...")
                input_path = preprocess_fxl_epub(
                    epub_path=input_path,
                    work_dir=work_dir,
                    fxl_info=fxl_result,
                    tracker=tracker,
                )
                print(f"  [OK] Reflowable EPUB created at: {input_path}")
                controller.prompt_continue("FXL Preprocessing")

            print("=== Step 1: Converting ePub to structured.xml ===")

            # Use the ePub processor directly
            epub_to_structured_v2.convert_epub_to_structured_v2(
                epub_path=input_path,
                output_xml=structured_xml,
                temp_dir=epub_temp_dir,
                tracker=tracker,
                max_chapters=max_chapters,
                chapters=chapters,
                parallel_workers=parallel_workers,
            )
            tracker.update_progress(50)
            controller.prompt_continue("ePub to Structured XML Conversion")

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

            print("\n=== Step 2: XSLT Transformation for DTD Compliance ===")
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

            print("\n=== Step 3: Packaging ===")
            structured_root = etree.parse(str(structured_xml), STRICT_XML_PARSER).getroot()
            root_name = (
                structured_root.tag.split("}", 1)[-1]
                if structured_root.tag.startswith("{")
                else structured_root.tag
            )

            assets: list[tuple[str, Path]] = []
            # Include epub_temp_dir in search paths for ePub conversions
            search_paths = [work_dir]
            if epub_temp_dir.exists():
                search_paths.append(epub_temp_dir)

            # Get reference mapper to enable MediaFetcher to resolve final names -> intermediate names
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
                source_format="epub",
                toc_json_path=toc_json_path,
            )
            tracker.update_progress(85)
            controller.prompt_continue("Packaging")

            print("\n=== Step 4: Pre-Fix Validation ===")
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

            print("\n=== Step 5: Comprehensive Automated DTD Fixes ===")
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
                print(f"  -> Fix pass {pass_num}/{MAX_FIX_PASSES}...")

                # Output path for this pass
                pass_output = out_dir / f"{isbn}_fixes_pass{pass_num}.zip"

                comprehensive_stats = comprehensive_fix_dtd(
                    zip_path=current_input,
                    output_path=pass_output,
                    dtd_path=dtd_path,
                    generate_reports=False,
                    id_registry_path=work_dir / "id_registry.json"
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
                shutil.move(str(current_input), str(all_fixed_zip_path))
                # Clean up intermediate pass files
                for i in range(1, MAX_FIX_PASSES + 1):
                    intermediate = out_dir / f"{isbn}_fixes_pass{i}.zip"
                    if intermediate.exists() and intermediate != all_fixed_zip_path:
                        intermediate.unlink()
            else:
                # No fixes needed, just copy
                shutil.copy2(str(final_zip_path), str(all_fixed_zip_path))

            print(f"    [OK] Total: Fixed {total_files_fixed} chapters with {total_fixes_all_passes} fixes across {pass_num} pass(es)")

            # Use the fully fixed version as final
            final_zip_path = all_fixed_zip_path
            tracker.update_progress(75)
            controller.prompt_continue("Comprehensive DTD Fixes")

            print("\n=== Step 6: Post-Fix DTD Validation ===")
            # Validate the fixed package to measure improvement
            post_validator = EntityTrackingValidator(dtd_path)
            post_validation_report = post_validator.validate_zip_package(
                zip_path=final_zip_path,
                output_report_path=None  # Don't generate report yet
            )

            # Build global ID index to filter cross-file IDREF errors
            # Cross-file references (e.g., <link linkend="ch0005"> in ap0001.xml) are valid
            # but appear as errors when validating each file in isolation
            global_ids = build_global_id_index(final_zip_path)

            # Filter cross-file IDREF errors and merge into unified report (single-pass)
            idref_pattern = re.compile(r'IDREF attribute linkend references an unknown ID "([^"]+)"')
            unknown_id_pattern = re.compile(r"references unknown ID ['\"]?([^'\"]+)['\"]?")

            true_errors = []
            cross_file_refs_count = 0

            # Single-pass: filter errors AND merge into unified report
            for error in post_validation_report.errors:
                is_cross_file_ref = False

                # Check if this is an IDREF error that's actually a valid cross-file reference
                if 'IDREF' in error.error_description or 'unknown ID' in error.error_description.lower():
                    match = idref_pattern.search(error.error_description)
                    if not match:
                        match = unknown_id_pattern.search(error.error_description)

                    if match:
                        referenced_id = match.group(1)
                        if referenced_id in global_ids:
                            # This is a valid cross-file reference, not a true error
                            is_cross_file_ref = True
                            cross_file_refs_count += 1

                if not is_cross_file_ref:
                    true_errors.append(error)
                    # Merge into unified report in same pass
                    unified_report.add_error(error)

            # Replace errors list with filtered list
            post_validation_report.errors = true_errors

            errors_after_fixes = len(true_errors)
            errors_fixed = errors_before_fixes - errors_after_fixes
            improvement_pct = (errors_fixed / errors_before_fixes * 100) if errors_before_fixes > 0 else 0

            # Show validation results with comparison
            print(f"\n[STATS] Validation Results Comparison:")
            print(f"  Errors before fixes:  {errors_before_fixes}")
            print(f"  Errors after fixes:   {errors_after_fixes}")
            if cross_file_refs_count > 0:
                print(f"  Cross-file refs (OK): {cross_file_refs_count}")
            print(f"  Errors fixed:         {errors_fixed}")
            print(f"  Improvement:          {improvement_pct:.1f}%")

            # Merge verification items
            for item in post_validation_report.verification_items:
                unified_report.add_verification_item(item)

            # === Run Three Rule Sets for LoadBook.bat Compatibility ===
            print("\n  Running downstream compatibility checks (LoadBook.bat rule sets)...")
            rule_set_errors = []

            with tempfile.TemporaryDirectory(prefix="ritt_rules_") as rules_tmp:
                rules_tmp_path = Path(rules_tmp)
                with zipfile.ZipFile(final_zip_path, 'r') as zf:
                    zf.extractall(rules_tmp_path)

                # Validate Book.XML for duplicate entity declarations (critical check)
                book_xml_path = rules_tmp_path / "Book.XML"
                if book_xml_path.exists():
                    entity_result = validate_entity_declarations(book_xml_path)
                    for err in entity_result.errors:
                        rule_set_errors.append(f"[Entity] Book.XML: {err}")

                # Find all chapter/appendix/preface XML files
                xml_files = list(rules_tmp_path.glob("*.xml"))
                xml_files = [f for f in xml_files if f.name != "Book.XML" and not f.name.startswith("._")]

                for xml_file in xml_files:
                    try:
                        tree = etree.parse(str(xml_file), LENIENT_XML_PARSER)

                        # Rule Set 1: Component rules (preface/appendix ordering)
                        comp_result = validate_component_rules(tree)
                        for err in comp_result.errors:
                            rule_set_errors.append(f"[Component] {xml_file.name}: {err}")

                        # Rule Set 2: Section rules (sect1/sect2 ordering)
                        sect_result = validate_section_rules(tree)
                        for err in sect_result.errors:
                            rule_set_errors.append(f"[Section] {xml_file.name}: {err}")

                        # Rule Set 3: Post-split robustness (linker safety)
                        split_result = validate_post_split_robustness(tree)
                        for err in split_result.errors:
                            rule_set_errors.append(f"[PostSplit] {xml_file.name}: {err}")

                        # Rule Set 4: Invalid element nesting (block in inline)
                        nesting_result = validate_invalid_element_nesting(tree)
                        for err in nesting_result.errors:
                            rule_set_errors.append(f"[Nesting] {xml_file.name}: {err}")

                    except etree.XMLSyntaxError as e:
                        rule_set_errors.append(f"[ParseError] {xml_file.name}: {e}")

            if rule_set_errors:
                print(f"  [!] Found {len(rule_set_errors)} downstream compatibility issues:")
                for err in rule_set_errors[:5]:
                    print(f"      {err[:100]}...")
                if len(rule_set_errors) > 5:
                    print(f"      ... and {len(rule_set_errors) - 5} more")

                # Add rule set errors to unified report
                for err in rule_set_errors:
                    unified_report.add_general_error(
                        xml_filename=err.split(":")[0].split("] ")[1] if "] " in err else "Unknown",
                        error_type=err.split("]")[0][1:] if err.startswith("[") else "Rule",
                        description=err.split(": ", 1)[1] if ": " in err else err,
                        severity="Error"
                    )
            else:
                print("  [OK] All downstream compatibility checks passed!")

            validation_passed = not post_validation_report.has_errors() and len(rule_set_errors) == 0
            if validation_passed:
                print("\n[OK] DTD validation PASSED - Package is fully compliant!")
            else:
                print(f"\n[!] {errors_after_fixes} validation errors remain")

                # Single-pass collection of error statistics (performance optimization)
                error_types = {}
                errors_by_file = {}
                sample_errors = []

                for error in post_validation_report.errors:
                    # Count by type
                    error_type = error.error_type
                    error_types[error_type] = error_types.get(error_type, 0) + 1

                    # Count by file
                    errors_by_file[error.xml_file] = errors_by_file.get(error.xml_file, 0) + 1

                    # Collect first 3 for samples
                    if len(sample_errors) < 3:
                        sample_errors.append(error)

                # Show remaining error breakdown by type
                print("\n  Remaining error types:")
                for error_type, count in sorted(error_types.items(), key=lambda x: -x[1])[:5]:
                    print(f"    {error_type}: {count}")

                # Show sample errors
                print("\n  Sample remaining errors:")
                for i, error in enumerate(sample_errors):
                    desc = error.error_description[:100] + "..." if len(error.error_description) > 100 else error.error_description
                    print(f"    {i+1}. {error.xml_file}:{error.line_number} - {desc}")

                # Show files with most errors
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
            print("\n=== Step 7: Generating Unified Validation Report ===")
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

            # Step 8: Intermediate artifacts are no longer saved (cleaned up automatically)
            # Only final outputs are kept: *_all_fixes.zip or *_NEEDS_REVIEW.zip and validation report
            tracker.update_progress(92)

            # Step 9: Manual Post-Processing (Spacing and Number Fixes)
            print("\n=== Step 9: Manual Post-Processing ===")
            print("  Applying spacing fixes and leading number stripping...")
            try:
                postprocess_report = run_manual_postprocessing(
                    zip_path=final_zip_path,
                    output_path=None,  # Modify in place
                    dry_run=False
                )

                if postprocess_report.has_changes():
                    print(f"  [OK] Applied {postprocess_report.total_changes()} post-processing fixes:")
                    print(f"       - Spacing fixes: {len(postprocess_report.spacing_fixes)}")
                    print(f"       - Number strips: {len(postprocess_report.number_strip_fixes)}")

                    # Add to unified report
                    for change in postprocess_report.spacing_fixes[:5]:
                        unified_report.add_general_error(
                            xml_filename=change.file,
                            error_type="Post-Processing",
                            description=f"Spacing fix: {change.rule}",
                            severity="Info"
                        )
                    for change in postprocess_report.number_strip_fixes[:5]:
                        unified_report.add_general_error(
                            xml_filename=change.file,
                            error_type="Post-Processing",
                            description=f"Number strip: {change.rule}",
                            severity="Info"
                        )
                else:
                    print("  [OK] No post-processing fixes needed")

                if postprocess_report.errors:
                    print(f"  [!] Post-processing warnings: {len(postprocess_report.errors)}")
                    for err in postprocess_report.errors[:3]:
                        print(f"      {err}")

            except Exception as e:
                print(f"  [!] Post-processing warning: {e}")
                logging.warning(f"Post-processing failed: {e}", exc_info=True)

            # Validate DTD compliance after post-processing
            print("\n  Validating DTD compliance after post-processing...")
            try:
                # Build global ID index for cross-file reference validation
                global_ids = build_global_id_index(final_zip_path)

                is_valid, validation_msg = validate_dtd_compliance(final_zip_path, DTD_PATH)
                if is_valid:
                    print("  [OK] DTD validation passed after post-processing")
                else:
                    # Filter validation messages to identify cross-file IDREF references
                    # which are valid but reported as errors by per-file DTD validation
                    idref_pattern = re.compile(r'IDREF attribute linkend references an unknown ID "([^"]+)"')

                    true_errors = []
                    cross_file_refs = []

                    for line in validation_msg.split('\n'):
                        if not line.strip():
                            continue
                        match = idref_pattern.search(line)
                        if match:
                            referenced_id = match.group(1)
                            if referenced_id in global_ids:
                                cross_file_refs.append(referenced_id)
                            else:
                                true_errors.append(line.strip())
                        else:
                            true_errors.append(line.strip())

                    if true_errors:
                        print(f"  [!] DTD validation issues after post-processing:")
                        # Show first few lines of true errors
                        for line in true_errors[:5]:
                            print(f"      {line}")
                        unified_report.add_general_error(
                            xml_filename="Book.XML",
                            error_type="Post-Process DTD",
                            description="DTD validation failed after post-processing - review changes",
                            severity="Warning"
                        )
                    else:
                        print("  [OK] DTD validation passed (IDREF errors are valid cross-file refs)")

                    if cross_file_refs:
                        print(f"  [i] {len(cross_file_refs)} cross-file reference(s) validated OK")
            except Exception as e:
                print(f"  [!] DTD validation check failed: {e}")
                logging.warning(f"Post-processing DTD validation failed: {e}", exc_info=True)

            # Run TOC Linkend Validation
            print("\n  Validating TOC linkend references...")
            try:
                linkend_report = validate_toc_linkends(final_zip_path)

                if linkend_report.invalid_count > 0:
                    print(f"  [!] Found {linkend_report.invalid_count} linkend issues:")
                    # Show first few issues
                    for issue in linkend_report.issues[:5]:
                        print(f"      {issue.file}: {issue.linkend} - {issue.description[:60]}...")

                    # Add to unified report
                    for issue in linkend_report.issues[:10]:
                        unified_report.add_general_error(
                            xml_filename=issue.file,
                            error_type="TOC Linkend",
                            description=f"linkend='{issue.linkend}': {issue.description}",
                            severity="Warning" if issue.severity == 'warning' else "Error"
                        )
                else:
                    print(f"  [OK] All {linkend_report.valid_count} linkend references are valid")

            except Exception as e:
                print(f"  [!] TOC linkend validation warning: {e}")
                logging.warning(f"TOC linkend validation failed: {e}", exc_info=True)

            # Run Title Synchronization (fix synthetic sect1 titles using EPUB TOC)
            print("\n  Synchronizing section titles with EPUB TOC...")
            try:
                title_sync_report = sync_section_titles(final_zip_path)

                if title_sync_report.has_changes():
                    print(f"  [OK] Synchronized {title_sync_report.total_fixes()} section titles:")
                    print(f"       - Section title fixes: {len(title_sync_report.section_fixes)}")
                    print(f"       - TOC entry fixes:     {len(title_sync_report.toc_fixes)}")

                    # Add to unified report
                    for fix in title_sync_report.section_fixes[:5]:
                        unified_report.add_general_error(
                            xml_filename=fix.file,
                            error_type="Title Sync",
                            description=f"Fixed sect1 title: '{fix.old_title[:30]}...' -> '{fix.new_title[:30]}...'",
                            severity="Info"
                        )
                else:
                    print("  [OK] All section titles are correct")

                if title_sync_report.errors:
                    print(f"  [!] Title sync warnings: {len(title_sync_report.errors)}")

            except Exception as e:
                print(f"  [!] Title synchronization warning: {e}")
                logging.warning(f"Title synchronization failed: {e}", exc_info=True)

            # Run TOC Nesting Fixer (ensure DTD-compliant TOC structure)
            print("\n  Validating TOC nesting structure...")
            try:
                toc_nesting_report = fix_toc_nesting(final_zip_path)

                if toc_nesting_report.fixes_applied > 0:
                    print(f"  [OK] Fixed {toc_nesting_report.fixes_applied} TOC nesting issues")

                    # Add to unified report
                    for issue in toc_nesting_report.issues[:5]:
                        if issue.fixed:
                            unified_report.add_general_error(
                                xml_filename="Book.XML",
                                error_type="TOC Nesting",
                                description=f"Fixed: {issue.description}",
                                severity="Info"
                            )
                else:
                    print(f"  [OK] TOC nesting structure is compliant")

                if toc_nesting_report.unfixed_errors() > 0:
                    print(f"  [!] {toc_nesting_report.unfixed_errors()} TOC nesting errors remain")
                    for issue in [i for i in toc_nesting_report.issues if not i.fixed][:3]:
                        print(f"      {issue.element_tag}: {issue.description}")

            except Exception as e:
                print(f"  [!] TOC nesting validation warning: {e}")
                logging.warning(f"TOC nesting validation failed: {e}", exc_info=True)

            tracker.update_progress(94)
            controller.prompt_continue("Manual Post-Processing")

            # Step 10: Content Diff - Compare EPUB vs XML content
            if not skip_content_diff:
                print("\n=== Step 10: Content Comparison (EPUB vs XML) ===")
                try:
                    contentdiff_result = run_content_diff(
                        epub_path=input_path,
                        rittdoc_path=final_zip_path,
                        output_dir=out_dir,
                        validation_report_path=report_path if report_saved else None
                    )

                    if contentdiff_result.content_loss_detected:
                        print(f"  [!] Content loss detected: {contentdiff_result.content_loss_percentage:.2f}%")
                        print(f"      EPUB characters: {contentdiff_result.total_epub_chars:,}")
                        print(f"      XML characters:  {contentdiff_result.total_xml_chars:,}")
                        print(f"      Difference:      {contentdiff_result.total_epub_chars - contentdiff_result.total_xml_chars:,} chars")
                        print(f"      Differences:     {len(contentdiff_result.differences)} items")
                        if contentdiff_result.missing_chapters:
                            print(f"      Missing chapters: {', '.join(contentdiff_result.missing_chapters)}")
                        print(f"  -> See contentdiff.xlsx for details")

                        # Add to unified report if we have one
                        if report_saved:
                            unified_report.add_general_error(
                                xml_filename="CONTENT_DIFF",
                                error_type="Content Loss",
                                description=f"Content loss: {contentdiff_result.content_loss_percentage:.2f}% ({contentdiff_result.total_epub_chars - contentdiff_result.total_xml_chars:,} chars)",
                                severity="Warning" if contentdiff_result.content_loss_percentage < 5 else "Error"
                            )
                    else:
                        print(f"  [OK] No significant content loss detected")
                        print(f"       Similarity: {(1 - contentdiff_result.content_loss_percentage/100)*100:.1f}%")

                    contentdiff_path = out_dir / "contentdiff.xlsx"
                    if contentdiff_path.exists():
                        print(f"  -> Content diff report: {contentdiff_path.name}")

                except Exception as e:
                    print(f"  [!] Content comparison failed: {e}")
                    logging.error(f"Content diff failed: {e}", exc_info=True)

                tracker.update_progress(95)
                controller.prompt_continue("Content Comparison")
            else:
                print("\n=== Step 10: Content Comparison ===")
                print("  [SKIPPED] Content comparison disabled for faster processing")
                print("  -> Use without --skip-content-diff to enable comparison")
                tracker.update_progress(95)

        # Calculate output size
        if final_zip_path.exists():
            output_size_mb = final_zip_path.stat().st_size / (1024 * 1024)
            tracker.current_metadata.output_size_mb = output_size_mb
            tracker.current_metadata.output_path = str(final_zip_path)

        # === Clean up intermediate output files ===
        # Only keep: final zip (*_all_fixes.zip or *_NEEDS_REVIEW.zip) and validation report
        print("\n=== Cleaning up intermediate files ===")
        cleanup_count = 0

        # 1. Delete the initial {isbn}.zip if it still exists and is not the final output
        initial_zip = out_dir / f"{isbn}.zip"
        if initial_zip.exists() and initial_zip != final_zip_path:
            try:
                initial_zip.unlink()
                print(f"  -> Removed intermediate: {initial_zip.name}")
                cleanup_count += 1
            except Exception as e:
                logging.warning(f"Could not delete {initial_zip}: {e}")

        # 2. Delete the intermediate artifacts folder
        intermediate_dir = out_dir / f"{isbn}_intermediate"
        if intermediate_dir.exists():
            try:
                shutil.rmtree(intermediate_dir)
                print(f"  -> Removed intermediate: {intermediate_dir.name}/")
                cleanup_count += 1
            except Exception as e:
                logging.warning(f"Could not delete {intermediate_dir}: {e}")

        # 3. Delete contentdiff.xlsx (intermediate artifact)
        contentdiff_file = out_dir / "contentdiff.xlsx"
        if contentdiff_file.exists():
            try:
                contentdiff_file.unlink()
                print(f"  -> Removed intermediate: {contentdiff_file.name}")
                cleanup_count += 1
            except Exception as e:
                logging.warning(f"Could not delete {contentdiff_file}: {e}")

        if cleanup_count == 0:
            print("  [OK] No intermediate files to clean up")
        else:
            print(f"  [OK] Cleaned up {cleanup_count} intermediate file(s)")

        # Copy id_registry.json from temp directory to output folder
        # This file is needed by comprehensive_dtd_fixer.py for dropped ID lookups
        # and must persist after the temp directory is deleted
        id_registry_src = work_dir / "id_registry.json"
        id_registry_dst = out_dir / "id_registry.json"
        if id_registry_src.exists():
            try:
                shutil.copy2(id_registry_src, id_registry_dst)
                print(f"  -> Preserved: {id_registry_dst.name}")
            except Exception as e:
                logging.warning(f"Could not copy id_registry.json: {e}")
        else:
            logging.warning(f"id_registry.json not found in temp directory")

        # Run plugin post-processors if available
        if PLUGIN_SYSTEM_AVAILABLE and plugin_manager and plugin_context:
            if plugin_manager.post_processor_count > 0:
                print("\n=== Running Plugin Post-Processors ===")
                plugin_context.output_dir = out_dir
                post_results = plugin_manager.run_post_processors(final_zip_path, plugin_context)
                for result in post_results:
                    if result.changes:
                        print(f"  {result.plugin_name}: {len(result.changes)} changes")
                    if result.metadata.get('error'):
                        print(f"  [!] {result.plugin_name} error: {result.metadata['error']}")

            # Notify plugins that conversion is ending
            plugin_manager.notify_conversion_end(plugin_context)
            logging.info("Plugin post-processors complete")

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

        # Stop the controller
        controller.stop()

        return final_zip_path

    except Exception as e:
        # Handle conversion failure
        import traceback
        error_msg = f"{type(e).__name__}: {str(e)}"
        print(f"\n[ERROR] Conversion FAILED: {error_msg}", file=sys.stderr)
        traceback.print_exc()

        # Notify plugins of conversion failure
        if PLUGIN_SYSTEM_AVAILABLE and plugin_manager and plugin_context:
            plugin_context.shared_data['error'] = error_msg
            plugin_manager.notify_conversion_end(plugin_context)

        tracker.complete_conversion(
            status=ConversionStatus.FAILURE,
            error_message=error_msg
        )

        # Stop the controller
        controller.stop()
        raise


def main() -> None:
    def _parse_chapter_spec(spec: str) -> str:
        """
        Validate a chapter selection string.

        Accepted forms:
        - "1,2,5"
        - "1-3,7,10-12"
        """
        if spec is None:
            return spec
        s = spec.strip()
        if not s:
            raise argparse.ArgumentTypeError("chapters cannot be empty")
        if not re.match(r"^[0-9,\-\s]+$", s):
            raise argparse.ArgumentTypeError("chapters must be digits/commas/ranges like: 1-3,7,10-12")
        return s

    parser = argparse.ArgumentParser(
        description="Convert ePub files to RittDoc format."
    )
    parser.add_argument("input", help="Input ePub file path")
    parser.add_argument(
        "out_dir",
        nargs="?",
        help="Directory where the final ZIP will be written (default: ./Output)",
    )
    parser.add_argument(
        "--debug",
        action="store_true",
        help="Enable debug logging for detailed diagnostics",
    )
    parser.add_argument(
        "--no-interactive",
        action="store_true",
        help="Disable interactive prompts (for automated processing)",
    )
    parser.add_argument(
        "--skip-content-diff",
        action="store_true",
        help="Skip content comparison step (speeds up processing significantly)",
    )
    parser.add_argument(
        "--max-chapters",
        type=int,
        default=None,
        help="Only process the first N chapters from spine order (default: all)",
    )
    parser.add_argument(
        "--chapters",
        type=_parse_chapter_spec,
        default=None,
        help='Only process specific chapter numbers (1-based), e.g. "1-3,7,10-12" (default: all)',
    )
    parser.add_argument(
        "--no-editor",
        action="store_true",
        help="Don't prompt to open editor after conversion",
    )
    parser.add_argument(
        "--parallel",
        type=int,
        default=PARALLEL_AUTO_DETECT,
        metavar="N",
        help="Parallel chapter processing workers: -1=auto-detect (default), 0=disabled, N=exact workers",
    )
    parser.add_argument(
        "--no-parallel",
        action="store_true",
        help="Disable parallel processing (equivalent to --parallel 0)",
    )
    parser.add_argument(
        "--list-plugins",
        action="store_true",
        help="List available plugins and exit",
    )
    args = parser.parse_args()

    # Handle --list-plugins
    if args.list_plugins:
        if PLUGIN_SYSTEM_AVAILABLE:
            pm = get_plugin_manager()
            plugins_dir = ROOT / "plugins"
            if plugins_dir.exists():
                pm.add_plugin_directory(plugins_dir)
                pm.discover_plugins()
            register_builtin_plugins(pm)

            plugins = pm.list_plugins()
            print("\n=== Available Plugins ===")
            print(f"\nValidators ({len(plugins['validators'])}):")
            for p in plugins['validators']:
                print(f"  - {p['name']} v{p['version']}: {p['description']}")
            print(f"\nFixers ({len(plugins['fixers'])}):")
            for p in plugins['fixers']:
                print(f"  - {p['name']} v{p['version']}: {p['description']}")
            print(f"\nPost-Processors ({len(plugins['post_processors'])}):")
            for p in plugins['post_processors']:
                print(f"  - {p['name']} v{p['version']}: {p['description']}")
            print(f"\nTotal: {pm.total_plugins} plugins")
        else:
            print("Plugin system not available")
        sys.exit(0)

    input_path = Path(args.input).resolve()
    out_dir = Path(args.out_dir).resolve() if args.out_dir else None

    # Handle --no-parallel flag (overrides --parallel)
    parallel_workers = 0 if args.no_parallel else args.parallel

    final_zip_path = convert_epub(
        input_path=input_path,
        out_dir=out_dir,
        debug=args.debug,
        interactive=not args.no_interactive,
        skip_content_diff=args.skip_content_diff,
        max_chapters=args.max_chapters,
        chapters=args.chapters,
        parallel_workers=parallel_workers,
    )

    # Prompt to open editor (unless disabled)
    if not args.no_editor:
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
                    out_dir_path = final_zip_path.parent
                    isbn = _sanitize_basename(input_path.stem)
                    editable_xml = out_dir_path / f"{isbn}_editable.xml"
                    shutil.copy2(book_xml, editable_xml)

                    # Generate HTML preview if it doesn't exist
                    html_preview = out_dir_path / f"{isbn}_preview.html"
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

                    # Launch editor with original EPUB as reference
                    launch_editor(
                        xml_path=editable_xml,
                        epub_path=input_path,
                        working_dir=out_dir_path
                    )
        else:
            print("Editor launch skipped. You can manually run the editor later with:")
            isbn = _sanitize_basename(input_path.stem)
            out_dir_path = final_zip_path.parent
            print(f"  cd editor && python server.py --xml {out_dir_path / f'{isbn}_editable.xml'}")


if __name__ == "__main__":
    main()
