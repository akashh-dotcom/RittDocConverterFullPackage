#!/bin/bash
# RittDoc Editor Launch Script

echo "==================================="
echo "    RittDoc Editor Launcher"
echo "==================================="
echo ""

# Check if Python is installed
if ! command -v python3 &> /dev/null; then
    echo "Error: Python 3 is not installed or not in PATH"
    exit 1
fi

# Check if in correct directory
if [ ! -f "server.py" ]; then
    echo "Error: server.py not found. Please run this script from the editor directory."
    exit 1
fi

# Check if requirements are installed
python3 -c "import flask" 2>/dev/null
if [ $? -ne 0 ]; then
    echo "Flask not found. Installing dependencies..."
    pip install -r requirements.txt
fi

echo "Starting RittDoc Editor..."
echo ""

# Run the server
python3 server.py "$@"
