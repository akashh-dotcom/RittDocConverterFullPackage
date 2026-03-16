#!/bin/bash
#
# Batch EPUB Processor
# Processes all .epub files in a folder sequentially
#
# Usage:
#   ./batch_epub.sh /path/to/epub/folder [output_folder]
#
# Examples:
#   ./batch_epub.sh ./my_epubs
#   ./batch_epub.sh ./my_epubs ./Output
#   ./batch_epub.sh /home/user/books /home/user/converted
#
# Output structure:
#   output_folder/
#   ├── Successful/          # Clean conversions (ISBN.zip)
#   ├── NeedsReview/         # Files with validation issues + reports
#   └── [raw output files]   # Original pipeline output
#

set -e

# Get script directory first (needed for venv path)
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# ============================================================
# OS Detection
# ============================================================
detect_os() {
    case "$(uname -s)" in
        Linux*)     OS_TYPE="Linux";;
        Darwin*)    OS_TYPE="macOS";;
        CYGWIN*)    OS_TYPE="Windows";;
        MINGW*)     OS_TYPE="Windows";;
        MSYS*)      OS_TYPE="Windows";;
        *)          OS_TYPE="Unknown";;
    esac

    # Also check for WSL (Windows Subsystem for Linux)
    if [ "$OS_TYPE" = "Linux" ] && grep -qi microsoft /proc/version 2>/dev/null; then
        OS_TYPE="WSL"
    fi
}

detect_os

# Colors for output (disable on Windows CMD if not supported)
if [ "$OS_TYPE" = "Windows" ] && [ -z "$TERM" ]; then
    RED=''
    GREEN=''
    YELLOW=''
    BLUE=''
    CYAN=''
    NC=''
else
    RED='\033[0;31m'
    GREEN='\033[0;32m'
    YELLOW='\033[1;33m'
    BLUE='\033[0;34m'
    CYAN='\033[0;36m'
    NC='\033[0m' # No Color
fi

# Detect Python command
detect_python() {
    case "$OS_TYPE" in
        Windows)
            # On Windows, 'python' is typically the command
            if command -v python &> /dev/null; then
                PYTHON_CMD="python"
            elif command -v python3 &> /dev/null; then
                PYTHON_CMD="python3"
            else
                echo -e "${RED}Error: Python not found. Please install Python 3.${NC}"
                exit 1
            fi
            ;;
        *)
            # On Linux/macOS/WSL, prefer python3
            if command -v python3 &> /dev/null; then
                PYTHON_CMD="python3"
            elif command -v python &> /dev/null; then
                PYTHON_CMD="python"
            else
                echo -e "${RED}Error: Python not found. Please install Python 3.${NC}"
                exit 1
            fi
            ;;
    esac

    # Verify Python version is 3.x
    PY_VERSION=$($PYTHON_CMD -c 'import sys; print(sys.version_info.major)' 2>/dev/null)
    if [ "$PY_VERSION" != "3" ]; then
        echo -e "${RED}Error: Python 3 is required. Found Python $PY_VERSION.${NC}"
        exit 1
    fi
}

detect_python

# ============================================================
# Virtual Environment Setup
# ============================================================
VENV_DIR="$SCRIPT_DIR/.venv"
REQUIREMENTS_FILE="$SCRIPT_DIR/requirements.txt"

setup_venv() {
    echo -e "${CYAN}============================================================${NC}"
    echo -e "${CYAN}Setting up Python environment...${NC}"
    echo -e "${CYAN}============================================================${NC}"
    echo -e "${CYAN}Detected OS: $OS_TYPE${NC}"

    # Check if venv exists
    if [ ! -d "$VENV_DIR" ]; then
        echo -e "${YELLOW}Creating virtual environment at $VENV_DIR...${NC}"
        $PYTHON_CMD -m venv "$VENV_DIR"
        if [ $? -ne 0 ]; then
            echo -e "${RED}Error: Failed to create virtual environment${NC}"
            case "$OS_TYPE" in
                Linux|WSL)
                    echo -e "${YELLOW}Try: sudo apt install python3-venv${NC}"
                    ;;
                macOS)
                    echo -e "${YELLOW}Try: brew install python3${NC}"
                    ;;
                Windows)
                    echo -e "${YELLOW}Ensure Python was installed with 'Add to PATH' option${NC}"
                    ;;
            esac
            exit 1
        fi
        echo -e "${GREEN}Virtual environment created successfully${NC}"
    else
        echo -e "${GREEN}Virtual environment found at $VENV_DIR${NC}"
    fi

    # Activate venv (OS-specific paths)
    case "$OS_TYPE" in
        Windows)
            if [ -f "$VENV_DIR/Scripts/activate" ]; then
                source "$VENV_DIR/Scripts/activate"
            else
                echo -e "${RED}Error: Could not find Windows venv activation script${NC}"
                exit 1
            fi
            ;;
        *)
            if [ -f "$VENV_DIR/bin/activate" ]; then
                source "$VENV_DIR/bin/activate"
            else
                echo -e "${RED}Error: Could not find venv activation script${NC}"
                exit 1
            fi
            ;;
    esac

    echo -e "${GREEN}Virtual environment activated${NC}"

    # Verify we're using the venv python (not system python)
    VENV_PYTHON=$(which python 2>/dev/null || which python3 2>/dev/null)
    echo -e "${CYAN}Using Python: $VENV_PYTHON${NC}"
    if [[ "$VENV_PYTHON" != *".venv"* ]]; then
        echo -e "${YELLOW}Warning: Python path doesn't appear to be from venv${NC}"
        echo -e "${YELLOW}Expected: $VENV_DIR/bin/python or $VENV_DIR/Scripts/python${NC}"
        echo -e "${YELLOW}Got:      $VENV_PYTHON${NC}"
    fi

    # Update pip first (suppress output unless error)
    echo -e "${YELLOW}Updating pip...${NC}"
    pip install --upgrade pip -q 2>/dev/null || pip install --upgrade pip

    # Check and install requirements
    if [ -f "$REQUIREMENTS_FILE" ]; then
        echo -e "${YELLOW}Checking/installing dependencies from requirements.txt...${NC}"

        # Always run pip install -r to ensure all packages are present
        # This handles cases where venv exists but packages are missing
        pip install -r "$REQUIREMENTS_FILE" 2>&1 | \
            grep -v "already satisfied" | \
            grep -v "^$" || true

        # Verify critical imports work
        echo -e "${YELLOW}Verifying critical dependencies...${NC}"
        MISSING_DEPS=""
        python -c "import ebooklib" 2>/dev/null || MISSING_DEPS="$MISSING_DEPS ebooklib(EbookLib)"
        python -c "import lxml" 2>/dev/null || MISSING_DEPS="$MISSING_DEPS lxml"
        python -c "import bs4" 2>/dev/null || MISSING_DEPS="$MISSING_DEPS beautifulsoup4"
        python -c "import PIL" 2>/dev/null || MISSING_DEPS="$MISSING_DEPS Pillow"

        if [ -n "$MISSING_DEPS" ]; then
            echo -e "${RED}Error: The following critical packages failed to import:${NC}"
            echo -e "${RED}  $MISSING_DEPS${NC}"
            echo ""
            echo -e "${YELLOW}Try installing manually:${NC}"
            echo -e "${YELLOW}  source $VENV_DIR/bin/activate${NC}"
            echo -e "${YELLOW}  pip install -r $REQUIREMENTS_FILE${NC}"

            # OS-specific troubleshooting hints
            case "$OS_TYPE" in
                Linux|WSL)
                    echo -e "${YELLOW}You may need: sudo apt install build-essential python3-dev${NC}"
                    ;;
                macOS)
                    echo -e "${YELLOW}You may need: xcode-select --install${NC}"
                    ;;
                Windows)
                    echo -e "${YELLOW}You may need Visual C++ Build Tools${NC}"
                    ;;
            esac
            exit 1
        fi
        echo -e "${GREEN}All dependencies verified${NC}"
    else
        echo -e "${YELLOW}Warning: requirements.txt not found at $REQUIREMENTS_FILE${NC}"
    fi

    echo ""
}

# Run venv setup
setup_venv

# Update PYTHON_CMD to use venv python
PYTHON_CMD="python"

# Check arguments
if [ -z "$1" ]; then
    echo -e "${RED}Error: Please provide a folder path containing EPUB files${NC}"
    echo ""
    echo "Usage: $0 /path/to/epub/folder [output_folder]"
    echo ""
    echo "Examples:"
    echo "  $0 ./my_epubs"
    echo "  $0 ./my_epubs ./Output"
    exit 1
fi

INPUT_FOLDER="$1"
OUTPUT_FOLDER="${2:-./Output}"

# Verify input folder exists
if [ ! -d "$INPUT_FOLDER" ]; then
    echo -e "${RED}Error: Folder '$INPUT_FOLDER' does not exist${NC}"
    exit 1
fi

# Create output folders
mkdir -p "$OUTPUT_FOLDER"
mkdir -p "$OUTPUT_FOLDER/Successful"
mkdir -p "$OUTPUT_FOLDER/NeedsReview"

# Count EPUB files
EPUB_COUNT=$(find "$INPUT_FOLDER" -maxdepth 1 -name "*.epub" -type f | wc -l)

if [ "$EPUB_COUNT" -eq 0 ]; then
    echo -e "${YELLOW}No .epub files found in '$INPUT_FOLDER'${NC}"
    exit 0
fi

echo "============================================================"
echo "Batch EPUB Processor"
echo "============================================================"
echo "Input folder:  $INPUT_FOLDER"
echo "Output folder: $OUTPUT_FOLDER"
echo "  ├── Successful/   (clean conversions)"
echo "  └── NeedsReview/  (files with validation issues)"
echo "Files to process: $EPUB_COUNT"
echo "============================================================"
echo ""

# Counters
SUCCESS=0
NEEDS_REVIEW=0
FAILED=0
SKIPPED=0
CURRENT=0

# Arrays to track results
declare -a SUCCESS_FILES
declare -a REVIEW_FILES
declare -a FAILED_FILES
declare -a SKIPPED_FILES

# Process each EPUB
for epub in "$INPUT_FOLDER"/*.epub; do
    [ -f "$epub" ] || continue

    CURRENT=$((CURRENT + 1))
    FILENAME=$(basename "$epub")

    # Extract ISBN from filename (remove .epub extension)
    ISBN="${FILENAME%.epub}"

    echo ""
    echo -e "${YELLOW}[$CURRENT/$EPUB_COUNT] Processing: $FILENAME${NC}"
    echo "------------------------------------------------------------"

    # Check if file already exists in Successful folder (skip only successful files)
    if [ -f "$OUTPUT_FOLDER/Successful/${ISBN}.zip" ]; then
        echo -e "${GREEN}[SKIPPED] $FILENAME - Already in Successful/${NC}"
        SKIPPED=$((SKIPPED + 1))
        SKIPPED_FILES+=("$ISBN")
        continue
    fi

    # Run the pipeline with no-interactive and no-editor flags
    if $PYTHON_CMD "$SCRIPT_DIR/epub_pipeline.py" "$epub" "$OUTPUT_FOLDER" --no-interactive --no-editor --skip-content-diff; then

        # Check for output files and organize them
        ALL_FIXES_ZIP="$OUTPUT_FOLDER/${ISBN}_all_fixes.zip"
        NEEDS_REVIEW_ZIP="$OUTPUT_FOLDER/${ISBN}_NEEDS_REVIEW.zip"
        VALIDATION_REPORT="$OUTPUT_FOLDER/${ISBN}_validation_report.xlsx"

        if [ -f "$NEEDS_REVIEW_ZIP" ]; then
            # File has validation issues - move to NeedsReview
            echo -e "${BLUE}[NEEDS REVIEW] $FILENAME${NC}"
            NEEDS_REVIEW=$((NEEDS_REVIEW + 1))
            REVIEW_FILES+=("$ISBN")

            # Copy NEEDS_REVIEW zip to NeedsReview folder (rename to just ISBN.zip)
            cp "$NEEDS_REVIEW_ZIP" "$OUTPUT_FOLDER/NeedsReview/${ISBN}.zip"

            # Copy validation report if it exists
            if [ -f "$VALIDATION_REPORT" ]; then
                cp "$VALIDATION_REPORT" "$OUTPUT_FOLDER/NeedsReview/${ISBN}_validation_report.xlsx"
            fi

        elif [ -f "$ALL_FIXES_ZIP" ]; then
            # Successful conversion - copy to Successful folder
            echo -e "${GREEN}[SUCCESS] $FILENAME${NC}"
            SUCCESS=$((SUCCESS + 1))
            SUCCESS_FILES+=("$ISBN")

            # Copy to Successful folder (rename to just ISBN.zip)
            cp "$ALL_FIXES_ZIP" "$OUTPUT_FOLDER/Successful/${ISBN}.zip"

        else
            # Check for basic ISBN.zip
            BASIC_ZIP="$OUTPUT_FOLDER/${ISBN}.zip"
            if [ -f "$BASIC_ZIP" ]; then
                echo -e "${GREEN}[SUCCESS] $FILENAME${NC}"
                SUCCESS=$((SUCCESS + 1))
                SUCCESS_FILES+=("$ISBN")
                cp "$BASIC_ZIP" "$OUTPUT_FOLDER/Successful/${ISBN}.zip"
            else
                echo -e "${RED}[FAILED] $FILENAME - No output file found${NC}"
                FAILED=$((FAILED + 1))
                FAILED_FILES+=("$FILENAME")
            fi
        fi
    else
        echo -e "${RED}[FAILED] $FILENAME${NC}"
        FAILED=$((FAILED + 1))
        FAILED_FILES+=("$FILENAME")
    fi
done

# Run TOC cleanup on successful files
if [ $SUCCESS -gt 0 ]; then
    echo ""
    echo "============================================================"
    echo "Running TOC Cleanup on Successful Files"
    echo "============================================================"

    if [ -f "$SCRIPT_DIR/cleanup_toc.py" ]; then
        # Run cleanup and capture output (no --quiet so we see the summary)
        $PYTHON_CMD "$SCRIPT_DIR/cleanup_toc.py" "$OUTPUT_FOLDER/Successful/"
        CLEANUP_EXIT=$?
        if [ $CLEANUP_EXIT -eq 0 ]; then
            echo -e "${GREEN}TOC cleanup completed successfully${NC}"
        else
            echo -e "${YELLOW}TOC cleanup completed with warnings${NC}"
        fi
    else
        echo -e "${YELLOW}Warning: cleanup_toc.py not found, skipping TOC cleanup${NC}"
    fi
fi

# Run Manual Post-Processing on successful files (spacing fixes, number stripping)
if [ $SUCCESS -gt 0 ]; then
    echo ""
    echo "============================================================"
    echo "Running Manual Post-Processing on Successful Files"
    echo "============================================================"
    echo "  - Fixing spacing issues (missing/extra spaces)"
    echo "  - Stripping leading numbers from ordered lists"
    echo "  - Stripping leading numbers from bibliography entries"
    echo ""

    if [ -f "$SCRIPT_DIR/manual_postprocessor.py" ]; then
        # Run post-processor on all successful files
        $PYTHON_CMD "$SCRIPT_DIR/manual_postprocessor.py" "$OUTPUT_FOLDER/Successful/"
        POSTPROC_EXIT=$?
        if [ $POSTPROC_EXIT -eq 0 ]; then
            echo -e "${GREEN}Manual post-processing completed successfully${NC}"
        else
            echo -e "${YELLOW}Manual post-processing completed with warnings${NC}"
        fi
    else
        echo -e "${YELLOW}Warning: manual_postprocessor.py not found, skipping post-processing${NC}"
    fi
fi

# Run Manual Post-Processing on NeedsReview files as well
if [ $NEEDS_REVIEW -gt 0 ]; then
    echo ""
    echo "============================================================"
    echo "Running Manual Post-Processing on NeedsReview Files"
    echo "============================================================"

    if [ -f "$SCRIPT_DIR/manual_postprocessor.py" ]; then
        $PYTHON_CMD "$SCRIPT_DIR/manual_postprocessor.py" "$OUTPUT_FOLDER/NeedsReview/" --quiet
        POSTPROC_EXIT=$?
        if [ $POSTPROC_EXIT -eq 0 ]; then
            echo -e "${GREEN}Manual post-processing completed for NeedsReview files${NC}"
        else
            echo -e "${YELLOW}Manual post-processing completed with warnings${NC}"
        fi
    fi
fi

# Run Title Synchronization (fix synthetic sect1 titles using EPUB TOC)
if [ $((SUCCESS + NEEDS_REVIEW)) -gt 0 ]; then
    echo ""
    echo "============================================================"
    echo "Running Title Synchronization"
    echo "============================================================"
    echo "  - Fixing synthetic sect1 titles using EPUB TOC"
    echo "  - Synchronizing section titles between chapters and TOC"
    echo ""

    if [ -f "$SCRIPT_DIR/title_synchronizer.py" ]; then
        # Run on Successful files
        if [ $SUCCESS -gt 0 ]; then
            echo "Synchronizing Successful files..."
            $PYTHON_CMD "$SCRIPT_DIR/title_synchronizer.py" "$OUTPUT_FOLDER/Successful/" --quiet
        fi

        # Run on NeedsReview files
        if [ $NEEDS_REVIEW -gt 0 ]; then
            echo "Synchronizing NeedsReview files..."
            $PYTHON_CMD "$SCRIPT_DIR/title_synchronizer.py" "$OUTPUT_FOLDER/NeedsReview/" --quiet
        fi

        echo -e "${GREEN}Title synchronization completed${NC}"
    else
        echo -e "${YELLOW}Warning: title_synchronizer.py not found, skipping title sync${NC}"
    fi
fi

# Run TOC Nesting Fixer (ensure DTD-compliant TOC structure)
if [ $((SUCCESS + NEEDS_REVIEW)) -gt 0 ]; then
    echo ""
    echo "============================================================"
    echo "Running TOC Nesting Fixer"
    echo "============================================================"
    echo "  - Validating TOC element hierarchy"
    echo "  - Fixing incorrect TOC nesting"
    echo ""

    if [ -f "$SCRIPT_DIR/toc_nesting_fixer.py" ]; then
        # Run on Successful files
        if [ $SUCCESS -gt 0 ]; then
            echo "Fixing Successful files..."
            $PYTHON_CMD "$SCRIPT_DIR/toc_nesting_fixer.py" "$OUTPUT_FOLDER/Successful/" --quiet
        fi

        # Run on NeedsReview files
        if [ $NEEDS_REVIEW -gt 0 ]; then
            echo "Fixing NeedsReview files..."
            $PYTHON_CMD "$SCRIPT_DIR/toc_nesting_fixer.py" "$OUTPUT_FOLDER/NeedsReview/" --quiet
        fi

        echo -e "${GREEN}TOC nesting fixes completed${NC}"
    else
        echo -e "${YELLOW}Warning: toc_nesting_fixer.py not found, skipping TOC nesting fixes${NC}"
    fi
fi

# Run TOC Linkend Validation on all processed files
if [ $((SUCCESS + NEEDS_REVIEW)) -gt 0 ]; then
    echo ""
    echo "============================================================"
    echo "Running TOC Linkend Validation"
    echo "============================================================"
    echo "  - Verifying linkend IDs correspond to existing files"
    echo "  - Verifying linkend IDs match section IDs in files"
    echo ""

    if [ -f "$SCRIPT_DIR/toc_linkend_validator.py" ]; then
        LINKEND_ERRORS=0

        # Validate Successful files
        if [ $SUCCESS -gt 0 ]; then
            echo "Validating Successful files..."
            for zip_file in "$OUTPUT_FOLDER/Successful"/*.zip; do
                [ -f "$zip_file" ] || continue
                ZIPNAME=$(basename "$zip_file")
                $PYTHON_CMD "$SCRIPT_DIR/toc_linkend_validator.py" "$zip_file" 2>/dev/null | tail -5
                if [ ${PIPESTATUS[0]} -ne 0 ]; then
                    LINKEND_ERRORS=$((LINKEND_ERRORS + 1))
                fi
            done
        fi

        # Validate NeedsReview files
        if [ $NEEDS_REVIEW -gt 0 ]; then
            echo "Validating NeedsReview files..."
            for zip_file in "$OUTPUT_FOLDER/NeedsReview"/*.zip; do
                [ -f "$zip_file" ] || continue
                ZIPNAME=$(basename "$zip_file")
                $PYTHON_CMD "$SCRIPT_DIR/toc_linkend_validator.py" "$zip_file" 2>/dev/null | tail -5
                if [ ${PIPESTATUS[0]} -ne 0 ]; then
                    LINKEND_ERRORS=$((LINKEND_ERRORS + 1))
                fi
            done
        fi

        if [ $LINKEND_ERRORS -eq 0 ]; then
            echo -e "${GREEN}TOC linkend validation passed for all files${NC}"
        else
            echo -e "${YELLOW}TOC linkend validation: $LINKEND_ERRORS file(s) have linkend issues${NC}"
        fi
    else
        echo -e "${YELLOW}Warning: toc_linkend_validator.py not found, skipping linkend validation${NC}"
    fi
fi

# Run Smart Link Target Creator on all processed files
if [ $((SUCCESS + NEEDS_REVIEW)) -gt 0 ]; then
    echo ""
    echo "============================================================"
    echo "Running Smart Link Target Creator"
    echo "============================================================"
    echo "  - Finding missing link targets (broken cross-references)"
    echo "  - Creating bibliography entries for citations"
    echo "  - Creating glossary entries for terms"
    echo "  - Creating anchor points for internal references"
    echo "  - Making ALL links work properly!"
    echo ""

    if [ -f "$SCRIPT_DIR/link_target_creator.py" ]; then
        TARGETS_CREATED=0

        # Fix Successful files
        if [ $SUCCESS -gt 0 ]; then
            echo "Creating targets in Successful files..."
            for zip_file in "$OUTPUT_FOLDER/Successful"/*.zip; do
                [ -f "$zip_file" ] || continue
                ZIPNAME=$(basename "$zip_file")
                OUTPUT=$($PYTHON_CMD "$SCRIPT_DIR/link_target_creator.py" "$zip_file" 2>&1)
                if echo "$OUTPUT" | grep -q "CREATED.*MISSING TARGET"; then
                    CREATED_COUNT=$(echo "$OUTPUT" | grep "CREATED" | grep -oP '\d+' | head -1)
                    echo "  ✓ $ZIPNAME: Created $CREATED_COUNT missing target(s)"
                    TARGETS_CREATED=$((TARGETS_CREATED + CREATED_COUNT))
                fi
            done
        fi

        # Fix NeedsReview files
        if [ $NEEDS_REVIEW -gt 0 ]; then
            echo "Creating targets in NeedsReview files..."
            for zip_file in "$OUTPUT_FOLDER/NeedsReview"/*.zip; do
                [ -f "$zip_file" ] || continue
                ZIPNAME=$(basename "$zip_file")
                OUTPUT=$($PYTHON_CMD "$SCRIPT_DIR/link_target_creator.py" "$zip_file" 2>&1)
                if echo "$OUTPUT" | grep -q "CREATED.*MISSING TARGET"; then
                    CREATED_COUNT=$(echo "$OUTPUT" | grep "CREATED" | grep -oP '\d+' | head -1)
                    echo "  ✓ $ZIPNAME: Created $CREATED_COUNT missing target(s)"
                    TARGETS_CREATED=$((TARGETS_CREATED + CREATED_COUNT))
                fi
            done
        fi

        if [ $TARGETS_CREATED -gt 0 ]; then
            echo -e "${GREEN}Link target creation completed: $TARGETS_CREATED target(s) created${NC}"
            echo -e "${GREEN}All links now have valid targets and will work properly!${NC}"
        else
            echo -e "${GREEN}No missing targets found - all links already have valid targets${NC}"
        fi
    else
        echo -e "${YELLOW}Warning: link_target_creator.py not found, skipping link target creation${NC}"
    fi
fi

# Clean up raw output files (keep only Successful/ and NeedsReview/ folders)
echo ""
echo "============================================================"
echo "Cleaning Up Raw Output Files"
echo "============================================================"
CLEANUP_COUNT=0

# Clean up files from successful conversions
for isbn in "${SUCCESS_FILES[@]}"; do
    # Remove _all_fixes.zip from root (now in Successful/)
    if [ -f "$OUTPUT_FOLDER/${isbn}_all_fixes.zip" ]; then
        rm -f "$OUTPUT_FOLDER/${isbn}_all_fixes.zip"
        CLEANUP_COUNT=$((CLEANUP_COUNT + 1))
    fi
    # Remove any validation report from root
    if [ -f "$OUTPUT_FOLDER/${isbn}_validation_report.xlsx" ]; then
        rm -f "$OUTPUT_FOLDER/${isbn}_validation_report.xlsx"
        CLEANUP_COUNT=$((CLEANUP_COUNT + 1))
    fi
    # Remove basic zip if it exists
    if [ -f "$OUTPUT_FOLDER/${isbn}.zip" ]; then
        rm -f "$OUTPUT_FOLDER/${isbn}.zip"
        CLEANUP_COUNT=$((CLEANUP_COUNT + 1))
    fi
done

# Clean up files from needs review conversions
for isbn in "${REVIEW_FILES[@]}"; do
    # Remove _NEEDS_REVIEW.zip from root (now in NeedsReview/)
    if [ -f "$OUTPUT_FOLDER/${isbn}_NEEDS_REVIEW.zip" ]; then
        rm -f "$OUTPUT_FOLDER/${isbn}_NEEDS_REVIEW.zip"
        CLEANUP_COUNT=$((CLEANUP_COUNT + 1))
    fi
    # Remove _all_fixes.zip if it exists
    if [ -f "$OUTPUT_FOLDER/${isbn}_all_fixes.zip" ]; then
        rm -f "$OUTPUT_FOLDER/${isbn}_all_fixes.zip"
        CLEANUP_COUNT=$((CLEANUP_COUNT + 1))
    fi
    # Remove validation report from root (now in NeedsReview/)
    if [ -f "$OUTPUT_FOLDER/${isbn}_validation_report.xlsx" ]; then
        rm -f "$OUTPUT_FOLDER/${isbn}_validation_report.xlsx"
        CLEANUP_COUNT=$((CLEANUP_COUNT + 1))
    fi
    # Remove basic zip if it exists
    if [ -f "$OUTPUT_FOLDER/${isbn}.zip" ]; then
        rm -f "$OUTPUT_FOLDER/${isbn}.zip"
        CLEANUP_COUNT=$((CLEANUP_COUNT + 1))
    fi
done

if [ $CLEANUP_COUNT -gt 0 ]; then
    echo -e "${GREEN}Cleaned up $CLEANUP_COUNT intermediate file(s)${NC}"
else
    echo "No intermediate files to clean up"
fi

# Print summary
echo ""
echo "============================================================"
echo "BATCH PROCESSING COMPLETE"
echo "============================================================"
echo -e "Total files:     $EPUB_COUNT"
echo -e "Successful:      ${GREEN}$SUCCESS${NC}"
echo -e "Needs Review:    ${BLUE}$NEEDS_REVIEW${NC}"
echo -e "Skipped:         ${GREEN}$SKIPPED${NC} (already in Successful/)"
echo -e "Failed:          ${RED}$FAILED${NC}"
echo ""
echo "Output locations:"
echo -e "  ${GREEN}Successful:${NC}   $OUTPUT_FOLDER/Successful/"
echo -e "  ${BLUE}Needs Review:${NC} $OUTPUT_FOLDER/NeedsReview/"

if [ $SKIPPED -gt 0 ]; then
    echo ""
    echo -e "${GREEN}Skipped files (already processed):${NC}"
    for f in "${SKIPPED_FILES[@]}"; do
        echo "  ⊘ $f.zip"
    done
fi

if [ $SUCCESS -gt 0 ]; then
    echo ""
    echo -e "${GREEN}Successful files (in Successful/):${NC}"
    for f in "${SUCCESS_FILES[@]}"; do
        echo "  ✓ $f.zip"
    done
fi

if [ $NEEDS_REVIEW -gt 0 ]; then
    echo ""
    echo -e "${BLUE}Files needing review (in NeedsReview/):${NC}"
    for f in "${REVIEW_FILES[@]}"; do
        echo "  ! $f.zip + ${f}_validation_report.xlsx"
    done
fi

if [ $FAILED -gt 0 ]; then
    echo ""
    echo -e "${RED}Failed files:${NC}"
    for f in "${FAILED_FILES[@]}"; do
        echo "  ✗ $f"
    done
fi

echo "============================================================"

# Exit with error code if any failed
if [ $FAILED -gt 0 ]; then
    exit 1
fi

exit 0
