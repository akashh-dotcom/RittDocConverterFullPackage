# Quick Start Guide - RittDocConverter

**5-Minute Setup Guide for Group Members**

## 🚀 Quick Setup (Windows)

```powershell
# 1. Clone the repository
git clone https://github.com/akashh-dotcom/RittDocConverter.git
cd RittDocConverter

# 2. Create virtual environment
python -m venv venv312

# 3. Activate it
.\venv312\Scripts\Activate.ps1

# 4. Install dependencies
pip install -r requirements.txt
```

## 🚀 Quick Setup (Mac/Linux)

```bash
# 1. Clone the repository
git clone https://github.com/akashh-dotcom/RittDocConverter.git
cd RittDocConverter

# 2. Create virtual environment
python3 -m venv venv312

# 3. Activate it
source venv312/bin/activate

# 4. Install dependencies
pip install -r requirements.txt
```

## 📖 Convert a File

```bash
# EPUB to DocBook
python integrated_pipeline.py "path/to/book.epub"

# PDF to DocBook
python integrated_pipeline.py "path/to/book.pdf"
```

## 📁 Find Your Output

Check the `Output/` folder:
- `{ISBN}_all_fixes.zip` ← **Use this file!**
- `{ISBN}_validation_report.xlsx` ← Open in Excel

## ⚠️ Common Issues

**"python not found"?**
```bash
# Install Python from python.org first
```

**"ModuleNotFoundError"?**
```bash
# Make sure venv is activated (you should see (venv312))
.\venv312\Scripts\Activate.ps1
pip install -r requirements.txt
```

**"Execution Policy" error (Windows)?**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## 📚 Need More Help?

See **SETUP_GUIDE.md** for detailed instructions.

---

**That's it! You're ready to convert documents.** 🎉
