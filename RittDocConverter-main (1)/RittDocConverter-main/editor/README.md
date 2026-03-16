# RittDoc Editor

A professional web-based editor for viewing and editing PDF, XML, and HTML documents with synchronized scrolling between all three views.

## Features

### Core Functionality
- **PDF Viewer**: Continuous scrolling PDF viewer with zoom, navigation, and thumbnail preview
- **XML Editor**: Full-featured Monaco Editor with syntax highlighting, validation, and formatting
- **HTML Preview/Editor**: View and edit HTML with rich text toolbar and WYSIWYG capabilities
- **Synchronized Scrolling**: All three panels sync automatically as you navigate

### Advanced Features
- **Screenshot Tool**: Capture regions from PDF pages and save as images
- **Block Overlay**: Visualize page blocks, reading order, and column information
- **Media Management**: View and manage multimedia files
- **Rich Text Editing**: Full toolbar with formatting, tables, math symbols, and more
- **Selectable PDF Text**: PDF.js text layer allows selecting and copying text from PDFs

## Installation

### Prerequisites
- Python 3.8 or higher
- A modern web browser (Chrome, Firefox, Edge, Safari)

### Setup

1. Navigate to the editor directory:
```bash
cd editor
```

2. Install Python dependencies:
```bash
pip install -r requirements.txt
```

## Usage

### Quick Start

Run the editor server:
```bash
python server.py
```

The editor will start at `http://127.0.0.1:5000`

### Command Line Options

```bash
# Run on custom port
python server.py --port 8080

# Run on all interfaces (accessible from network)
python server.py --host 0.0.0.0

# Enable debug mode
python server.py --debug

# Specify files to edit
python server.py --xml path/to/file.xml --pdf path/to/file.pdf
```

### Auto-Detection

If you don't specify files, the editor will automatically load the most recent files from the `../Output` directory.

## File Structure

```
editor/
├── server.py              # Flask backend server
├── requirements.txt       # Python dependencies
├── README.md             # This file
├── templates/
│   └── index.html        # Main HTML template
└── static/
    ├── css/
    │   └── editor.css    # Comprehensive styles
    └── js/
        └── editor.js     # Main application JavaScript
```

## Editor Interface

### Header
- **View Mode Toggle**: Switch between XML, HTML Preview, and HTML Edit modes
- **Save Button**: Save current changes
- **Save & Process**: Save and trigger reprocessing pipeline (if implemented)

### Left Panel (PDF Viewer)
- **Navigation**: Previous/Next page buttons, direct page input
- **Zoom**: Zoom in/out controls with percentage display
- **Screenshot**: Enable screenshot mode to capture PDF regions
- **Thumbnails**: Bottom bar with page thumbnails for quick navigation

### Right Panel (Editor/Preview)
- **XML Mode**: Monaco editor with syntax highlighting, formatting, validation
- **HTML Preview**: Read-only HTML view with image loading and styling
- **HTML Edit**: WYSIWYG editor with rich text toolbar

### Toolbar Features (HTML Edit Mode)
- Text formatting (bold, italic, underline, strikethrough)
- Font size and family selection
- Text and background colors
- Text alignment
- Lists (ordered/unordered)
- Links and tables
- Math symbols (Greek letters, operators, set theory, arrows, etc.)
- Undo/redo
- Format clearing

## Keyboard Shortcuts

### XML Editor
- `Ctrl+F`: Find
- `Ctrl+H`: Find and replace
- `Ctrl+Z`: Undo
- `Ctrl+Y`: Redo
- `Ctrl+/`: Toggle comment

### HTML Editor
- `Ctrl+B`: Bold
- `Ctrl+I`: Italic
- `Ctrl+U`: Underline
- `Ctrl+Z`: Undo
- `Ctrl+Y`: Redo

## API Endpoints

The editor exposes the following REST API endpoints:

- `GET /api/init` - Initialize editor with auto-detected files
- `POST /api/init` - Initialize with specific files
- `GET /api/pdf` - Serve PDF file
- `GET /api/media/<filename>` - Serve media files
- `GET /api/media-list` - List available media files
- `POST /api/save` - Save XML or HTML changes
- `POST /api/screenshot` - Save screenshot image
- `POST /api/render-html` - Convert XML to HTML preview
- `GET /api/page-mapping` - Get page to XML element mapping

## Development

### Running in Development Mode

```bash
python server.py --debug
```

This enables:
- Auto-reload on code changes
- Detailed error messages
- Stack traces in browser

### Customization

#### Adding Custom Styles
Edit `static/css/editor.css` to customize the appearance.

#### Extending Functionality
Modify `static/js/editor.js` to add new features or modify behavior.

#### Backend Changes
Update `server.py` to add new API endpoints or modify existing functionality.

## Troubleshooting

### PDF Not Loading
- Ensure the PDF file exists and is accessible
- Check browser console for errors
- Verify PDF.js is loading correctly

### Monaco Editor Not Appearing
- Check that the Monaco CDN is accessible
- Verify no JavaScript errors in console
- Try clearing browser cache

### Media Files Not Displaying
- Verify `multimedia` folder exists in working directory
- Check file permissions
- Ensure image paths are correct in HTML/XML

### Scroll Sync Not Working
- Check that `data-page` attributes are present in HTML
- Verify no JavaScript errors
- Try refreshing the page

## Browser Compatibility

- Chrome/Edge: Full support
- Firefox: Full support
- Safari: Full support
- Internet Explorer: Not supported

## Performance Tips

1. **Large PDFs**: Consider reducing page count or using pagination
2. **Memory**: Close other browser tabs when editing large files
3. **Network**: Use local files for faster loading
4. **Rendering**: Reduce zoom level for faster PDF rendering

## Known Limitations

- Screenshot mode requires browser canvas support
- Very large XML files (>10MB) may be slow in Monaco editor
- PDF text layer quality depends on PDF source
- HTML editing doesn't preserve all XML structure details

## Contributing

This editor is part of the RittDocConverter project. For improvements or bug fixes:
1. Make changes in the `editor/` directory
2. Test thoroughly with sample files
3. Update documentation as needed

## License

Part of the RittDocConverter project.

## Support

For issues or questions:
1. Check this README
2. Review console logs for errors
3. Verify file paths and permissions
4. Ensure all dependencies are installed

## Version History

### v1.0 (Current)
- Initial release
- PDF viewer with continuous scrolling
- XML editor with Monaco
- HTML preview and edit modes
- Synchronized scrolling
- Screenshot tool
- Rich text editing
- Block overlay system
