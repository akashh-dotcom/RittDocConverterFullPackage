#!/bin/bash
#
# Cleanup Script for Successful ZIP files
# Runs TOC cleanup rules on all ZIPs in Successful/ folder
#
# Usage:
#   ./cleanup_successful.sh /path/to/Output           # Process Output/Successful/
#   ./cleanup_successful.sh /path/to/Output --dry-run # Preview changes only
#   ./cleanup_successful.sh /path/to/Successful/      # Direct path to Successful folder
#
# This script should be run after batch_epub.sh completes
#

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Get script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Check arguments
if [ -z "$1" ]; then
    echo -e "${RED}Error: Please provide a path${NC}"
    echo ""
    echo "Usage: $0 /path/to/Output [--dry-run] [--verbose]"
    echo ""
    echo "Options:"
    echo "  --dry-run, -n    Preview changes without modifying files"
    echo "  --verbose, -v    Show detailed output"
    echo "  --quiet, -q      Only show errors and summary"
    exit 1
fi

INPUT_PATH="$1"
shift  # Remove first argument, keep rest for passing to Python

# Determine Successful folder path
if [ -d "$INPUT_PATH/Successful" ]; then
    SUCCESSFUL_PATH="$INPUT_PATH/Successful"
elif [ -d "$INPUT_PATH" ] && [[ "$INPUT_PATH" == *"Successful"* ]]; then
    SUCCESSFUL_PATH="$INPUT_PATH"
else
    echo -e "${RED}Error: Cannot find Successful/ folder${NC}"
    echo "  Tried: $INPUT_PATH/Successful"
    echo "  Tried: $INPUT_PATH"
    exit 1
fi

# Count ZIP files
ZIP_COUNT=$(find "$SUCCESSFUL_PATH" -maxdepth 1 -name "*.zip" -type f | wc -l)

if [ "$ZIP_COUNT" -eq 0 ]; then
    echo -e "${YELLOW}No ZIP files found in '$SUCCESSFUL_PATH'${NC}"
    exit 0
fi

echo "============================================================"
echo "TOC Cleanup for Successful Files"
echo "============================================================"
echo "Successful folder: $SUCCESSFUL_PATH"
echo "ZIP files to process: $ZIP_COUNT"
echo "============================================================"
echo ""

# Run the Python cleanup script
python3 "$SCRIPT_DIR/cleanup_toc.py" "$SUCCESSFUL_PATH" "$@"

EXIT_CODE=$?

echo ""
if [ $EXIT_CODE -eq 0 ]; then
    echo -e "${GREEN}Cleanup completed successfully${NC}"
else
    echo -e "${RED}Cleanup completed with errors${NC}"
fi

exit $EXIT_CODE
