# RittDocConverter - Setup Guide for VS Code

This guide will help you set up and use the RittDocConverter repository in Visual Studio Code.

## Prerequisites

Before you begin, make sure you have the following installed:

1. **Python 3.12 or 3.13**
   - Download from: https://www.python.org/downloads/
   - During installation, check "Add Python to PATH"
   - Verify installation: `python --version`

2. **Visual Studio Code**
   - Download from: https://code.visualstudio.com/

3. **Git**
   - Download from: https://git-scm.com/downloads
   - Verify installation: `git --version`

4. **VS Code Extensions (Recommended)**
   - Python (by Microsoft)
   - Pylance (by Microsoft)

---

## Step 1: Clone the Repository

### Option A: Using VS Code

1. Open VS Code
2. Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac)
3. Type "Git: Clone" and select it
4. Enter the repository URL: `https://github.com/akashh-dotcom/RittDocConverter.git`
5. Choose a folder where you want to save the project
6. Click "Open" when prompted

### Option B: Using Command Line

```bash
# Navigate to your desired folder
cd C:\Users\YourName\Documents

# Clone the repository
git clone https://github.com/akashh-dotcom/RittDocConverter.git

# Open in VS Code
cd RittDocConverter
code .
```

---

## Step 2: Create a Virtual Environment

### Windows (PowerShell)

```powershell
# Create virtual environment
python -m venv venv312

# Activate it
.\venv312\Scripts\Activate.ps1

# If you get an execution policy error, run this first:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Mac/Linux (Terminal)

```bash
# Create virtual environment
python3 -m venv venv312

# Activate it
source venv312/bin/activate
```

**You'll know it's activated when you see `(venv312)` before your command prompt.**

---

## Step 3: Install Dependencies

With your virtual environment activated:

```bash
# Upgrade pip first
pip install --upgrade pip

# Install all required packages
pip install -r requirements.txt
```

This will install all necessary libraries including:
- EbookLib (for EPUB processing)
- lxml (for XML handling)
- PyMuPDF (for PDF processing)
- Pillow (for image processing)
- And many more...

**Installation may take 5-10 minutes.**

---

## Step 4: Verify Installation

```bash
# Check if all imports work
python -c "import ebooklib, lxml, fitz; print('All imports successful!')"
```

If you see "All imports successful!" - you're ready to go! ✓

---

## Step 5: Running Conversions

### Convert an EPUB File

```bash
# Basic usage
python integrated_pipeline.py "path/to/your/file.epub"

# Example (Windows)
python integrated_pipeline.py "C:\Users\DELL\Documents\book.epub"

# Example (Mac/Linux)
python integrated_pipeline.py "/Users/username/Documents/book.epub"
```

### Convert a PDF File

```bash
python integrated_pipeline.py "path/to/your/file.pdf"
```

---

## Step 6: Understanding the Output

After conversion, check the `Output` folder:

```
Output/
├── 9781234567890.zip                    # Final package (use this!)
├── 9781234567890_all_fixes.zip          # Package after DTD fixes
├── pre_fixes_9781234567890.zip          # Package before fixes
├── 9781234567890_validation_report.xlsx # Validation results
├── conversion_dashboard.xlsx            # Tracking dashboard
└── 9781234567890_intermediate/          # Debug files
    └── structured_compliant.xml
```

**Main output files:**
- `{ISBN}_all_fixes.zip` - **Use this file** - fully validated and DTD-compliant
- `{ISBN}_validation_report.xlsx` - Open in Excel to see validation results

### Inside the ZIP File

```
Book.XML              # Main book file with entity references
ch0001.xml            # Chapter 1
ch0002.xml            # Chapter 2
...
MultiMedia/           # All images
  Ch0001f01.jpg
  Ch0002f01.jpg
  ...
```

---

## Step 7: Working in VS Code

### Recommended VS Code Settings

1. **Select Python Interpreter**
   - Press `Ctrl+Shift+P`
   - Type "Python: Select Interpreter"
   - Choose the one from `.\venv312\Scripts\python.exe`

2. **Open Integrated Terminal**
   - Press `` Ctrl+` `` (backtick)
   - Make sure `(venv312)` appears in the terminal
   - If not, activate manually: `.\venv312\Scripts\Activate.ps1`

3. **Useful VS Code Shortcuts**
   - `Ctrl+Shift+P` - Command palette
   - `` Ctrl+` `` - Toggle terminal
   - `Ctrl+B` - Toggle sidebar
   - `Ctrl+F` - Find in file
   - `Ctrl+Shift+F` - Find in all files

---

## Common Issues & Solutions

### Issue 1: "python: command not found"
**Solution:** Python not in PATH. Reinstall Python and check "Add to PATH" option.

### Issue 2: "ModuleNotFoundError: No module named 'ebooklib'"
**Solution:** Virtual environment not activated or dependencies not installed.
```bash
.\venv312\Scripts\Activate.ps1
pip install -r requirements.txt
```

### Issue 3: "Execution policy error" (Windows)
**Solution:** Run PowerShell as Administrator and execute:
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Issue 4: Unicode encoding errors
**Solution:** Already fixed in the latest code. Pull the latest changes:
```bash
git pull origin main
```

### Issue 5: Empty MultiMedia folder
**Solution:** Use the latest code from the feature branch:
```bash
git checkout claude/fix-empty-multimedia-file-01Md2ZP1f3gJKGj9XTxGp2dR
git pull
```

---

## Project Structure

```
RittDocConverter/
├── integrated_pipeline.py          # Main entry point
├── epub_to_structured_v2.py        # EPUB converter
├── package.py                      # ZIP packaging
├── validate_with_entity_tracking.py # DTD validation
├── fix_chapters_simple.py          # Auto-fix DTD errors
├── reference_mapper.py             # Image tracking
├── requirements.txt                # Python dependencies
├── xslt/                           # XSLT transformations
│   └── rittdoc_compliance.xslt
├── dtd/                            # DTD files
│   └── v1.1/
│       └── RittDocBook.dtd
└── Output/                         # Conversion results
```

---

## Development Workflow

### Making Changes

```bash
# Create a new branch for your work
git checkout -b feature/your-feature-name

# Make your changes in VS Code

# Check what changed
git status

# Add your changes
git add .

# Commit with a message
git commit -m "Description of your changes"

# Push to GitHub
git push origin feature/your-feature-name
```

### Pulling Latest Changes

```bash
# Make sure you're on the main branch
git checkout main

# Pull latest changes
git pull origin main

# If you have a virtual environment active, reinstall dependencies
pip install -r requirements.txt
```

---

## Testing Your Setup

Run this test to verify everything works:

```bash
# If you have a sample EPUB file
python integrated_pipeline.py "path/to/test.epub"

# Check the Output folder for results
ls Output/
```

---

## Getting Help

1. **Check the logs** - The console output shows detailed progress and errors
2. **Validation Report** - Open `{ISBN}_validation_report.xlsx` for validation issues
3. **Debug Files** - Check `Output/{ISBN}_intermediate/` for intermediate XML files
4. **GitHub Issues** - Report bugs at https://github.com/akashh-dotcom/RittDocConverter/issues

---

## Quick Reference Commands

```bash
# Activate virtual environment (Windows)
.\venv312\Scripts\Activate.ps1

# Activate virtual environment (Mac/Linux)
source venv312/bin/activate

# Deactivate virtual environment
deactivate

# Install dependencies
pip install -r requirements.txt

# Run conversion
python integrated_pipeline.py "file.epub"

# Check Python version
python --version

# List installed packages
pip list

# Update a package
pip install --upgrade package-name
```

---

## Tips for Efficient Use

1. **Use Tab Completion** - Press Tab to auto-complete file paths
2. **Drag & Drop** - Drag files into terminal to get their full path
3. **Check Output Early** - Monitor the Output folder during conversion
4. **Keep Terminal Open** - Don't close VS Code while conversion is running
5. **Use Latest Code** - Pull updates regularly for bug fixes

---

## VS Code Tips

### Split View
- Drag a file to the side to view code and terminal simultaneously

### Search Across Files
- `Ctrl+Shift+F` to find text across all project files

### Quick File Open
- `Ctrl+P` then type filename to quickly open files

### Format Code
- `Shift+Alt+F` to auto-format Python code

### View Problems
- `Ctrl+Shift+M` to see Python linting errors

---

**Last Updated:** December 2024

For questions or issues, contact the repository maintainer or open an issue on GitHub.

Happy converting! 🚀
