@echo off
REM RittDoc Editor Launch Script for Windows

echo ===================================
echo     RittDoc Editor Launcher
echo ===================================
echo.

REM Check if Python is installed
python --version >nul 2>&1
if errorlevel 1 (
    echo Error: Python is not installed or not in PATH
    pause
    exit /b 1
)

REM Check if in correct directory
if not exist "server.py" (
    echo Error: server.py not found. Please run this script from the editor directory.
    pause
    exit /b 1
)

REM Check if requirements are installed
python -c "import flask" >nul 2>&1
if errorlevel 1 (
    echo Flask not found. Installing dependencies...
    pip install -r requirements.txt
)

echo Starting RittDoc Editor...
echo.

REM Run the server
python server.py %*
