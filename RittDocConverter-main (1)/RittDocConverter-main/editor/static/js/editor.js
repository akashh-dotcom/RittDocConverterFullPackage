// RittDoc Editor - Main Application JavaScript

// Global state
const APP_STATE = {
    pdfDoc: null,
    currentPage: 1,
    totalPages: 0,
    scale: 1.5,
    xmlContent: '',
    htmlContent: '',
    monacoEditor: null,
    currentView: 'xml',
    screenshotMode: false,
    screenshotData: null,
    mediaFiles: [],
    isHtmlEdited: false,
    pageElements: [],
    pageToXmlMapping: {}, // Maps page numbers to XML element info
    xmlToPageMapping: {}, // Maps XML element indices to page numbers
    totalXmlPages: 0, // Total unique pages found in XML
    syncInProgress: false, // Flag to prevent circular scroll updates
    htmlScrollListener: null,
    htmlScrollTarget: null,
    xmlScrollDisposable: null,
    showBlockOverlay: false, // Toggle for showing block info overlay
    // EPUB-specific state
    epubBook: null,
    epubRendition: null,
    epubCurrentLocation: null,
    epubFontSize: 100,
    epubToc: [],
    fileType: null, // 'pdf' or 'epub'
    epubPath: null
};

const MATHJAX_PENDING_KEY = '__mathjaxPending';

function renderMathInContainer(container) {
    if (!container) return;

    if (window.MathJax && window.MathJax.typesetPromise) {
        window.MathJax.typesetPromise([container]).catch((error) => {
            console.warn('MathJax typeset failed:', error);
        });
        return;
    }

    if (!window[MATHJAX_PENDING_KEY]) {
        window[MATHJAX_PENDING_KEY] = [];
    }
    window[MATHJAX_PENDING_KEY].push(container);
}

function normalizeMediaSrc(src) {
    if (!src) return null;
    const trimmed = src.trim();
    if (!trimmed) return null;

    const lower = trimmed.toLowerCase();
    if (lower.startsWith('http://') || lower.startsWith('https://') || lower.startsWith('data:') ||
        lower.startsWith('blob:') || lower.startsWith('file:')) {
        return trimmed;
    }

    const normalizeRelativePath = (path) => {
        let cleaned = path.trim();
        if (!cleaned) return '';
        if (cleaned.startsWith('/')) {
            cleaned = cleaned.slice(1);
        }
        if (cleaned.toLowerCase().startsWith('multimedia/')) {
            cleaned = cleaned.slice('multimedia/'.length);
        }
        const parts = cleaned.split('/').filter(Boolean);
        const stack = [];
        for (const part of parts) {
            if (part === '.' || part === '') continue;
            if (part === '..') {
                if (stack.length) stack.pop();
                continue;
            }
            stack.push(part);
        }
        return stack.join('/');
    };

    if (lower.startsWith('/api/media/')) {
        const remainder = trimmed.slice('/api/media/'.length);
        const cleaned = normalizeRelativePath(remainder);
        return cleaned ? `/api/media/${cleaned}` : trimmed;
    }
    if (lower.startsWith('api/media/')) {
        const remainder = trimmed.slice('api/media/'.length);
        const cleaned = normalizeRelativePath(remainder);
        return cleaned ? `/api/media/${cleaned}` : `/${trimmed}`;
    }
    if (lower.startsWith('/api/')) {
        return trimmed;
    }
    if (lower.startsWith('api/')) {
        return `/${trimmed}`;
    }

    const cleaned = normalizeRelativePath(trimmed);

    if (!cleaned) return trimmed;
    return `/api/media/${cleaned}`;
}

function updatePreviewImages(container) {
    if (!container) return;
    const images = container.querySelectorAll('img');
    images.forEach((img) => {
        const src = img.getAttribute('src');
        const normalized = normalizeMediaSrc(src);
        if (normalized && normalized !== src) {
            img.setAttribute('src', normalized);
        }
        img.setAttribute('loading', 'lazy');
        img.addEventListener('error', () => {
            img.classList.add('image-error');
            img.setAttribute('src', '/api/placeholder-image');
        }, { once: true });
    });
}

// PDF.js worker
pdfjsLib.GlobalWorkerOptions.workerSrc = 'https://cdnjs.cloudflare.com/ajax/libs/pdf.js/3.11.174/pdf.worker.min.js';

// Initialize application
document.addEventListener('DOMContentLoaded', () => {
    initializeApp();
});

async function initializeApp() {
    console.log('Initializing RittDoc Editor...');

    // Load initial data
    await loadInitialData();

    // Initialize Monaco Editor
    initializeMonacoEditor();

    // Setup event listeners
    setupEventListeners();

    // Load PDF or EPUB based on file type
    if (APP_STATE.fileType === 'pdf' && APP_STATE.pdfPath) {
        await loadPDF();
    } else if (APP_STATE.fileType === 'epub' && APP_STATE.epubPath) {
        await loadEPUB();
    }

    // Load media files
    await loadMediaFiles();

    // Setup page mapping (only for PDF)
    if (APP_STATE.fileType === 'pdf') {
        await setupPageMapping();
    }

    console.log('RittDoc Editor initialized successfully');
}

// Load initial data from server
async function loadInitialData() {
    try {
        showLoading();
        const response = await fetch('/api/init');
        const data = await response.json();

        if (data.error) {
            showNotification(data.error, 'error');
            return;
        }

        APP_STATE.xmlContent = data.xml;
        APP_STATE.htmlContent = data.html;
        APP_STATE.pdfPath = data.pdf.path;
        APP_STATE.epubPath = data.epub?.path;
        APP_STATE.fileType = data.file_type;
        APP_STATE.multimediaFolder = data.multimedia_folder;

        // Check for package mode
        if (data.package_mode && data.package) {
            initializePackageMode(data.package);
        }

        hideLoading();
    } catch (error) {
        console.error('Error loading initial data:', error);
        showNotification('Failed to load initial data', 'error');
        hideLoading();
    }
}

// Initialize Monaco Editor
function initializeMonacoEditor() {
    require.config({ paths: { vs: 'https://cdn.jsdelivr.net/npm/monaco-editor@0.44.0/min/vs' } });

    require(['vs/editor/editor.main'], function () {
        APP_STATE.monacoEditor = monaco.editor.create(document.getElementById('xmlEditor'), {
            value: APP_STATE.xmlContent,
            language: 'xml',
            theme: 'vs',
            automaticLayout: true,
            fontSize: 14,
            minimap: { enabled: true },
            scrollBeyondLastLine: false,
            wordWrap: 'on',
            folding: true,
            lineNumbers: 'on'
        });

        setupXmlScrollSync();
        console.log('Monaco Editor initialized with scroll sync');
    });
}

// Initialize HTML editable content
function initializeHTMLEditor() {
    const editableContent = document.getElementById('htmlEditableContent');
    if (editableContent) {
        // Track changes
        editableContent.addEventListener('input', () => {
            APP_STATE.isHtmlEdited = true;
        });

        // Handle paste - preserve some formatting
        editableContent.addEventListener('paste', (e) => {
            e.preventDefault();
            const text = (e.originalEvent || e).clipboardData.getData('text/html') ||
                        (e.originalEvent || e).clipboardData.getData('text/plain');
            document.execCommand('insertHTML', false, text);
        });

        // Keyboard shortcuts
        editableContent.addEventListener('keydown', (e) => {
            if (e.ctrlKey && e.key === 'b') {
                e.preventDefault();
                document.execCommand('bold');
            } else if (e.ctrlKey && e.key === 'i') {
                e.preventDefault();
                document.execCommand('italic');
            } else if (e.ctrlKey && e.key === 'u') {
                e.preventDefault();
                document.execCommand('underline');
            } else if (e.ctrlKey && e.key === 'z') {
                e.preventDefault();
                document.execCommand('undo');
            } else if (e.ctrlKey && e.key === 'y') {
                e.preventDefault();
                document.execCommand('redo');
            }
        });

        console.log('HTML Editor initialized');
    }

    setupRichTextToolbar();
}

// Setup event listeners
function setupEventListeners() {
    // View mode buttons
    document.getElementById('xmlViewBtn').addEventListener('click', () => switchView('xml'));
    document.getElementById('htmlViewBtn').addEventListener('click', () => switchView('html'));
    document.getElementById('htmlEditBtn').addEventListener('click', () => switchView('htmledit'));

    // Save button - only Save & Process option
    document.getElementById('saveReprocessBtn').addEventListener('click', () => saveChanges(true));

    // PDF controls
    document.getElementById('prevPageBtn').addEventListener('click', () => changePage(-1));
    document.getElementById('nextPageBtn').addEventListener('click', () => changePage(1));
    document.getElementById('pageNumberInput').addEventListener('change', (e) => {
        const page = parseInt(e.target.value);
        if (page >= 1 && page <= APP_STATE.totalPages) {
            scrollToPage(page);
        }
    });

    // Zoom controls
    document.getElementById('zoomInBtn').addEventListener('click', () => changeZoom(0.25));
    document.getElementById('zoomOutBtn').addEventListener('click', () => changeZoom(-0.25));

    // Screenshot button
    document.getElementById('screenshotBtn').addEventListener('click', toggleScreenshotMode);

    // Editor controls
    document.getElementById('formatBtn').addEventListener('click', formatXML);
    document.getElementById('validateBtn').addEventListener('click', validateXML);
    document.getElementById('refreshHtmlBtn').addEventListener('click', refreshHTMLPreview);

    // Thumbnail toggle
    document.getElementById('toggleThumbnailsBtn').addEventListener('click', toggleThumbnails);

    // Block overlay toggle
    const blockOverlayBtn = document.getElementById('blockOverlayBtn');
    if (blockOverlayBtn) {
        blockOverlayBtn.addEventListener('click', toggleBlockOverlay);
    }

    // PDF scroll sync with throttling
    const pdfViewer = document.getElementById('pdfViewer');
    if (pdfViewer) {
        let scrollTimeout = null;
        pdfViewer.addEventListener('scroll', () => {
            updatePageNumberFromScroll();
            if (scrollTimeout) clearTimeout(scrollTimeout);
            scrollTimeout = setTimeout(() => handlePdfScroll(), 50);
        });
    }

    document.getElementById('zoomLevel').textContent = `${Math.round(APP_STATE.scale * 100)}%`;
    setupResizer();
}

// Switch view mode
function switchView(view) {
    // In Package Mode, only allow XML view
    if (APP_STATE.packageMode && (view === 'html' || view === 'htmledit')) {
        showNotification('HTML mode not available in Package Mode. Use XML mode to edit chapters.', 'warning');
        // Switch to XML view instead
        view = 'xml';
    }

    APP_STATE.currentView = view;

    document.querySelectorAll('.mode-btn').forEach(btn => btn.classList.remove('active'));

    document.getElementById('xmlEditor').style.display = 'none';
    document.getElementById('htmlPreview').style.display = 'none';
    document.getElementById('htmlEditor').style.display = 'none';
    document.getElementById('refreshHtmlBtn').style.display = 'none';
    document.getElementById('formatBtn').style.display = 'inline-flex';
    document.getElementById('validateBtn').style.display = 'inline-flex';

    // Ensure loading is hidden when switching views
    hideLoading();

    if (view === 'xml') {
        document.getElementById('xmlViewBtn').classList.add('active');
        document.getElementById('xmlEditor').style.display = 'block';
        setupHtmlScrollSync(null);
        document.getElementById('editorTitle').innerHTML = '<i class="fas fa-code"></i> XML Editor';
    } else if (view === 'html') {
        document.getElementById('htmlViewBtn').classList.add('active');
        const htmlPreview = document.getElementById('htmlPreview');
        htmlPreview.style.display = 'block';
        htmlPreview.innerHTML = enhanceHTMLContent(APP_STATE.htmlContent);
        renderMathInContainer(htmlPreview);
        document.getElementById('refreshHtmlBtn').style.display = 'inline-flex';
        document.getElementById('formatBtn').style.display = 'none';
        document.getElementById('validateBtn').style.display = 'none';
        document.getElementById('editorTitle').innerHTML = '<i class="fas fa-eye"></i> HTML Preview';
        setupHtmlScrollSync(htmlPreview);
        if (APP_STATE.showBlockOverlay) {
            setTimeout(() => applyBlockOverlays(), 100);
        }
    } else if (view === 'htmledit') {
        document.getElementById('htmlEditBtn').classList.add('active');
        document.getElementById('htmlEditor').style.display = 'flex';
        const editableContent = document.getElementById('htmlEditableContent');
        if (!APP_STATE.isHtmlEdited) {
            editableContent.innerHTML = enhanceHTMLContent(APP_STATE.htmlContent);
        }
        initializeHTMLEditor();
        setupHtmlScrollSync(editableContent);
        document.getElementById('formatBtn').style.display = 'none';
        document.getElementById('validateBtn').style.display = 'none';
        document.getElementById('editorTitle').innerHTML = '<i class="fas fa-edit"></i> HTML Editor';
        if (APP_STATE.showBlockOverlay) {
            setTimeout(() => applyBlockOverlays(), 100);
        }
    }
}

// Enhance HTML content with proper font styling and image paths
function enhanceHTMLContent(html) {
    const parser = new DOMParser();
    const doc = parser.parseFromString(html, 'text/html');

    const applyImageSrc = (img) => {
        const src = img.getAttribute('src');
        const normalized = normalizeMediaSrc(src);
        if (normalized && normalized !== src) {
            img.setAttribute('src', normalized);
        }
    };

    // Extract font specifications
    const fontSpecs = {};
    const fontSpecElements = doc.querySelectorAll('[class*="font-spec"], [id*="font-spec"]');
    fontSpecElements.forEach(spec => {
        const id = spec.getAttribute('id') || spec.getAttribute('class');
        if (id) {
            const fontFamily = spec.getAttribute('data-font-family') || spec.style.fontFamily;
            const fontSize = spec.getAttribute('data-font-size') || spec.style.fontSize;
            const fontColor = spec.getAttribute('data-font-color') || spec.style.color;
            fontSpecs[id] = { fontFamily, fontSize, fontColor };
        }
    });

    // Apply font specs
    const elementsWithFontRef = doc.querySelectorAll('[data-font-spec], [class*="font-"], .phrase, span[style]');
    elementsWithFontRef.forEach(el => {
        const fontRef = el.getAttribute('data-font-spec') || el.className.match(/font-\d+/)?.[0];
        if (fontRef && fontSpecs[fontRef]) {
            const spec = fontSpecs[fontRef];
            if (spec.fontFamily) el.style.fontFamily = spec.fontFamily;
            if (spec.fontSize) el.style.fontSize = spec.fontSize;
            if (spec.fontColor) el.style.color = spec.fontColor;
        }
    });

    // Fix image paths
    const images = doc.querySelectorAll('img');
    images.forEach(img => {
        applyImageSrc(img);
        if (!img.closest('figure') && !img.closest('.figure') && !img.closest('.docbook-figure') && !img.closest('.media-figure')) {
            const wrapper = doc.createElement('div');
            wrapper.className = 'image-loading-wrapper';
            img.parentNode.insertBefore(wrapper, img);
            wrapper.appendChild(img);
        }
        img.classList.add('image-loading');
        img.setAttribute('loading', 'lazy');
        img.setAttribute('onload', 'this.classList.remove("image-loading"); this.classList.add("image-loaded"); if(this.parentNode.classList.contains("image-loading-wrapper")) this.parentNode.classList.add("loaded");');
        img.setAttribute('onerror', 'this.classList.remove("image-loading"); this.classList.add("image-error"); this.src="/api/placeholder-image";');
    });

    // Handle figure elements
    const figures = doc.querySelectorAll('figure, .figure, .docbook-figure, .media-figure');
    figures.forEach(fig => {
        fig.classList.add('figure-loading-container');
        const img = fig.querySelector('img');
        if (img) {
            applyImageSrc(img);
            img.classList.add('image-loading');
            img.setAttribute('loading', 'lazy');
            img.setAttribute('onload', 'this.classList.remove("image-loading"); this.classList.add("image-loaded"); this.closest(".figure-loading-container")?.classList.add("loaded");');
            img.setAttribute('onerror', 'this.classList.remove("image-loading"); this.classList.add("image-error"); this.src="/api/placeholder-image";');
        }
    });

    // Add data-section-id
    const sections = doc.querySelectorAll('section, div[class*="chapter"], div[class*="section"], article');
    sections.forEach((section, index) => {
        section.setAttribute('data-section-id', `section-${index}`);
    });

    return doc.body.innerHTML;
}

// Setup HTML scroll sync
function setupHtmlScrollSync(element) {
    if (APP_STATE.htmlScrollListener && APP_STATE.htmlScrollTarget) {
        APP_STATE.htmlScrollTarget.removeEventListener('scroll', APP_STATE.htmlScrollListener);
    }

    if (!element) {
        APP_STATE.htmlScrollTarget = null;
        return;
    }

    APP_STATE.htmlScrollTarget = element;

    APP_STATE.htmlScrollListener = () => {
        if (APP_STATE.syncInProgress) return;

        APP_STATE.syncInProgress = true;

        const elementsWithPages = element.querySelectorAll('[data-page]');
        let currentPageNum = 1;

        if (elementsWithPages.length > 0) {
            const containerRect = element.getBoundingClientRect();
            const viewportMiddle = containerRect.top + containerRect.height / 2;
            let minDistance = Infinity;

            elementsWithPages.forEach(el => {
                const rect = el.getBoundingClientRect();
                const elementMiddle = rect.top + rect.height / 2;
                const distance = Math.abs(elementMiddle - viewportMiddle);

                if (distance < minDistance) {
                    minDistance = distance;
                    const pageAttr = el.getAttribute('data-page');
                    if (pageAttr) currentPageNum = parseInt(pageAttr);
                }
            });

            if (currentPageNum > 0 && currentPageNum <= APP_STATE.totalPages) {
                syncPdfToPage(currentPageNum);
                syncXmlToPage(currentPageNum);
            }
        } else {
            const scrollPercentage = element.scrollTop / (element.scrollHeight - element.clientHeight);
            syncPdfToPercentage(scrollPercentage);
            syncXmlToPercentage(scrollPercentage);
        }

        setTimeout(() => {
            APP_STATE.syncInProgress = false;
        }, 100);
    };

    element.addEventListener('scroll', APP_STATE.htmlScrollListener);
}

// Setup XML scroll sync
function setupXmlScrollSync() {
    if (!APP_STATE.monacoEditor) return;

    if (APP_STATE.xmlScrollDisposable) {
        APP_STATE.xmlScrollDisposable.dispose();
    }

    APP_STATE.xmlScrollDisposable = APP_STATE.monacoEditor.onDidScrollChange(() => {
        if (APP_STATE.syncInProgress) return;

        APP_STATE.syncInProgress = true;

        const editor = APP_STATE.monacoEditor;
        const scrollTop = editor.getScrollTop();
        const maxScroll = Math.max(editor.getScrollHeight() - editor.getLayoutInfo().height, 1);
        const percentage = Math.max(0, Math.min(1, scrollTop / maxScroll));

        syncPdfToPercentage(percentage);
        syncHtmlToPercentage(percentage);

        setTimeout(() => {
            APP_STATE.syncInProgress = false;
        }, 100);
    });
}

// Sync functions
function syncPdfToPage(pageNum) {
    const pageWrapper = document.querySelector(`.pdf-page-wrapper[data-page="${pageNum}"]`);
    if (pageWrapper) {
        const pdfViewer = document.getElementById('pdfViewer');
        const containerRect = pdfViewer.getBoundingClientRect();
        const pageRect = pageWrapper.getBoundingClientRect();
        const relativeTop = pageRect.top - containerRect.top + pdfViewer.scrollTop;
        pdfViewer.scrollTo({ top: relativeTop, behavior: 'smooth' });
    }
}

function syncPdfToPercentage(percentage) {
    const pdfViewer = document.getElementById('pdfViewer');
    if (pdfViewer) {
        const targetScroll = percentage * (pdfViewer.scrollHeight - pdfViewer.clientHeight);
        pdfViewer.scrollTop = targetScroll;
    }
}

function syncXmlToPercentage(percentage) {
    if (APP_STATE.monacoEditor) {
        const editor = APP_STATE.monacoEditor;
        const lineCount = editor.getModel().getLineCount();
        const targetLine = Math.floor(percentage * lineCount);
        editor.revealLineInCenter(Math.max(1, targetLine));
    }
}

function syncHtmlToPercentage(percentage) {
    const htmlContainer = APP_STATE.currentView === 'html'
        ? document.getElementById('htmlPreview')
        : document.getElementById('htmlEditableContent');

    if (htmlContainer && htmlContainer.style.display !== 'none') {
        const targetScroll = percentage * (htmlContainer.scrollHeight - htmlContainer.clientHeight);
        htmlContainer.scrollTop = targetScroll;
    }
}

function updatePageNumberFromScroll() {
    const pdfViewer = document.getElementById('pdfViewer');
    const pages = document.querySelectorAll('.pdf-page-wrapper');
    if (pages.length === 0) return;

    const viewerRect = pdfViewer.getBoundingClientRect();
    const viewerMidpoint = viewerRect.top + viewerRect.height / 2;
    let currentPageNum = 1;
    let minDistance = Infinity;

    pages.forEach(pageWrapper => {
        const pageNum = parseInt(pageWrapper.getAttribute('data-page'));
        const rect = pageWrapper.getBoundingClientRect();
        const pageMidpoint = rect.top + rect.height / 2;
        const distance = Math.abs(pageMidpoint - viewerMidpoint);

        if (distance < minDistance) {
            minDistance = distance;
            currentPageNum = pageNum;
        }
    });

    if (currentPageNum !== APP_STATE.currentPage) {
        APP_STATE.currentPage = currentPageNum;
        document.getElementById('pageNumberInput').value = currentPageNum;
        updateThumbnailSelection(currentPageNum);
    }
}

function handlePdfScroll() {
    if (APP_STATE.syncInProgress) return;
    APP_STATE.syncInProgress = true;

    const pages = document.querySelectorAll('.pdf-page-wrapper');
    if (pages.length === 0) {
        APP_STATE.syncInProgress = false;
        return;
    }

    const currentPageNum = APP_STATE.currentPage;
    syncHtmlToPage(currentPageNum);
    syncXmlToPage(currentPageNum);

    setTimeout(() => {
        APP_STATE.syncInProgress = false;
    }, 100);
}

function syncHtmlToPage(pageNum) {
    const htmlContainer = APP_STATE.currentView === 'html'
        ? document.getElementById('htmlPreview')
        : document.getElementById('htmlEditableContent');

    if (!htmlContainer || htmlContainer.style.display === 'none') return;

    const pageElement = getFirstElementForPage(htmlContainer, pageNum);

    if (pageElement) {
        const containerRect = htmlContainer.getBoundingClientRect();
        const elementRect = pageElement.getBoundingClientRect();
        const relativeTop = elementRect.top - containerRect.top + htmlContainer.scrollTop;
        htmlContainer.scrollTo({ top: relativeTop - 50, behavior: 'smooth' });
        highlightSyncedElement(pageElement);
    } else {
        const scrollPercentage = (pageNum - 1) / Math.max(APP_STATE.totalPages - 1, 1);
        const targetScroll = scrollPercentage * (htmlContainer.scrollHeight - htmlContainer.clientHeight);
        htmlContainer.scrollTop = targetScroll;
    }
}

function highlightSyncedElement(element) {
    element.classList.add('sync-highlight');
    setTimeout(() => element.classList.remove('sync-highlight'), 1000);
}

function syncXmlToPage(pageNum) {
    if (!APP_STATE.monacoEditor) return;

    const editor = APP_STATE.monacoEditor;
    const model = editor.getModel();
    const content = model.getValue();
    const pagePattern = new RegExp(`page=["']${pageNum}["']`, 'i');
    const lines = content.split('\n');

    for (let i = 0; i < lines.length; i++) {
        if (pagePattern.test(lines[i])) {
            editor.revealLineInCenter(i + 1);
            return;
        }
    }

    const lineCount = model.getLineCount();
    const scrollPercentage = (pageNum - 1) / Math.max(APP_STATE.totalPages - 1, 1);
    const targetLine = Math.floor(scrollPercentage * lineCount);
    editor.revealLineInCenter(Math.max(1, targetLine));
}

function scrollToPage(pageNum) {
    APP_STATE.syncInProgress = true;
    const pageWrapper = document.querySelector(`.pdf-page-wrapper[data-page="${pageNum}"]`);
    if (pageWrapper) {
        pageWrapper.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }
    setTimeout(() => {
        APP_STATE.syncInProgress = false;
    }, 500);
}

async function setupPageMapping() {
    try {
        const response = await fetch('/api/page-mapping');
        const data = await response.json();
        if (data.mapping) {
            APP_STATE.pageToXmlMapping = data.mapping;
            APP_STATE.xmlToPageMapping = data.element_to_page || {};
            APP_STATE.totalXmlPages = data.total_xml_pages || 0;
            console.log(`Page mapping loaded: ${data.page_count} pages with content`);
        }
    } catch (error) {
        console.log('Page mapping not available, using estimated mapping');
        for (let i = 1; i <= APP_STATE.totalPages; i++) {
            APP_STATE.pageToXmlMapping[i] = [];
        }
    }
}

function getFirstElementForPage(container, pageNum) {
    let element = container.querySelector(`[data-page="${pageNum}"]`);
    if (element) return element;

    const allElements = container.querySelectorAll('[data-page]');
    let closest = null;
    let closestDiff = Infinity;

    allElements.forEach(el => {
        const elPage = parseInt(el.getAttribute('data-page'));
        if (!isNaN(elPage)) {
            const diff = Math.abs(elPage - pageNum);
            if (diff < closestDiff) {
                closestDiff = diff;
                closest = el;
            }
        }
    });

    return closest;
}

// Save changes
async function saveChanges(reprocess) {
    try {
        showLoading();

        let content = '';
        let contentType = 'xml';

        if (APP_STATE.currentView === 'xml') {
            content = APP_STATE.monacoEditor.getValue();
            contentType = 'xml';
        } else if (APP_STATE.currentView === 'htmledit') {
            const editableContent = document.getElementById('htmlEditableContent');
            content = editableContent.innerHTML;
            contentType = 'html';
        }

        const response = await fetch('/api/save', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ type: contentType, content, reprocess })
        });

        const result = await response.json();
        hideLoading();

        if (result.error) {
            showNotification(result.error, 'error');
        } else if (result.success) {
            showNotification(result.message || 'Saved successfully!', 'success');
            APP_STATE.isHtmlEdited = false;
            if (result.package) {
                showNotification(`Package created: ${result.package}`, 'success');
            }
            if (contentType === 'html' && result.html) {
                APP_STATE.htmlContent = result.html;
            }
        }
    } catch (error) {
        console.error('Error saving:', error);
        showNotification('Failed to save changes', 'error');
        hideLoading();
    }
}

// Load PDF
async function loadPDF() {
    try {
        const loadingTask = pdfjsLib.getDocument('/api/pdf');
        APP_STATE.pdfDoc = await loadingTask.promise;
        APP_STATE.totalPages = APP_STATE.pdfDoc.numPages;

        document.getElementById('totalPages').textContent = APP_STATE.totalPages;
        await renderAllPages();
        await generateThumbnails();
        console.log(`PDF loaded: ${APP_STATE.totalPages} pages`);
    } catch (error) {
        console.error('Error loading PDF:', error);
        showNotification('Failed to load PDF', 'error');
    }
}

// Render all pages
async function renderAllPages() {
    const container = document.getElementById('pdfPagesContainer');
    container.innerHTML = '';
    APP_STATE.pageElements = [];

    for (let i = 1; i <= APP_STATE.totalPages; i++) {
        const pageWrapper = document.createElement('div');
        pageWrapper.className = 'pdf-page-wrapper';
        pageWrapper.setAttribute('data-page', i);

        const canvasContainer = document.createElement('div');
        canvasContainer.className = 'pdf-canvas-container';

        const canvas = document.createElement('canvas');
        canvas.id = `pdfCanvas-${i}`;

        const textLayer = document.createElement('div');
        textLayer.className = 'textLayer';
        textLayer.id = `textLayer-${i}`;

        canvasContainer.appendChild(canvas);
        canvasContainer.appendChild(textLayer);

        const pageNumberLabel = document.createElement('div');
        pageNumberLabel.className = 'pdf-page-number';
        pageNumberLabel.textContent = `Page ${i}`;

        const overlay = document.createElement('div');
        overlay.className = 'screenshot-overlay';
        overlay.id = `screenshotOverlay-${i}`;

        pageWrapper.appendChild(canvasContainer);
        pageWrapper.appendChild(pageNumberLabel);
        pageWrapper.appendChild(overlay);
        container.appendChild(pageWrapper);

        await renderPage(i, canvas, textLayer);
        APP_STATE.pageElements.push(pageWrapper);
    }
}

// Render PDF page
async function renderPage(pageNum, canvas, textLayerDiv) {
    try {
        const page = await APP_STATE.pdfDoc.getPage(pageNum);
        const context = canvas.getContext('2d');
        const viewport = page.getViewport({ scale: APP_STATE.scale });

        canvas.height = viewport.height;
        canvas.width = viewport.width;

        await page.render({ canvasContext: context, viewport }).promise;

        if (textLayerDiv) {
            textLayerDiv.innerHTML = '';
            textLayerDiv.style.width = `${viewport.width}px`;
            textLayerDiv.style.height = `${viewport.height}px`;
            const textContent = await page.getTextContent();
            pdfjsLib.renderTextLayer({
                textContentSource: textContent,
                container: textLayerDiv,
                viewport,
                textDivs: []
            });
        }
    } catch (error) {
        console.error(`Error rendering page ${pageNum}:`, error);
    }
}

// Generate thumbnails
async function generateThumbnails() {
    const thumbnailList = document.getElementById('thumbnailList');
    thumbnailList.innerHTML = '';

    for (let i = 1; i <= APP_STATE.totalPages; i++) {
        const thumbnailItem = document.createElement('div');
        thumbnailItem.className = 'thumbnail-item';
        thumbnailItem.dataset.page = i;

        const canvas = document.createElement('canvas');
        const page = await APP_STATE.pdfDoc.getPage(i);
        const viewport = page.getViewport({ scale: 0.2 });

        canvas.height = viewport.height;
        canvas.width = viewport.width;

        const context = canvas.getContext('2d');
        await page.render({ canvasContext: context, viewport }).promise;

        thumbnailItem.appendChild(canvas);

        const pageNum = document.createElement('div');
        pageNum.className = 'thumbnail-page-num';
        pageNum.textContent = `${i}`;
        thumbnailItem.appendChild(pageNum);

        thumbnailItem.addEventListener('click', () => scrollToPage(i));
        thumbnailList.appendChild(thumbnailItem);
    }

    updateThumbnailSelection(1);
}

function updateThumbnailSelection(pageNum) {
    document.querySelectorAll('.thumbnail-item').forEach(item => {
        item.classList.remove('active');
        if (parseInt(item.dataset.page) === pageNum) {
            item.classList.add('active');
            item.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
        }
    });
}

function changePage(delta) {
    const newPage = APP_STATE.currentPage + delta;
    if (newPage >= 1 && newPage <= APP_STATE.totalPages) {
        scrollToPage(newPage);
    }
}

async function changeZoom(delta) {
    APP_STATE.scale = Math.max(0.5, Math.min(3.0, APP_STATE.scale + delta));
    document.getElementById('zoomLevel').textContent = `${Math.round(APP_STATE.scale * 100)}%`;
    await renderAllPages();
    setTimeout(() => scrollToPage(APP_STATE.currentPage), 100);
}

// Screenshot functions
function toggleScreenshotMode() {
    APP_STATE.screenshotMode = !APP_STATE.screenshotMode;
    const btn = document.getElementById('screenshotBtn');

    if (APP_STATE.screenshotMode) {
        btn.style.background = '#ef4444';
        btn.style.color = 'white';
        document.querySelectorAll('.pdf-page-wrapper').forEach(wrapper => {
            wrapper.classList.add('screenshot-mode');
            const overlay = wrapper.querySelector('.screenshot-overlay');
            setupScreenshotCapture(overlay, wrapper);
        });
    } else {
        btn.style.background = '';
        btn.style.color = '';
        document.querySelectorAll('.pdf-page-wrapper').forEach(wrapper => {
            wrapper.classList.remove('screenshot-mode');
        });
    }
}

function setupScreenshotCapture(overlay, pageWrapper) {
    const canvas = pageWrapper.querySelector('canvas');
    const pageNum = parseInt(pageWrapper.getAttribute('data-page'));

    let isDrawing = false;
    let startX, startY;
    let selectionDiv = null;

    overlay.addEventListener('mousedown', (e) => {
        isDrawing = true;
        const rect = canvas.getBoundingClientRect();
        startX = e.clientX - rect.left;
        startY = e.clientY - rect.top;

        selectionDiv = document.createElement('div');
        selectionDiv.className = 'screenshot-selection';
        selectionDiv.style.left = startX + 'px';
        selectionDiv.style.top = startY + 'px';
        overlay.appendChild(selectionDiv);
    });

    overlay.addEventListener('mousemove', (e) => {
        if (!isDrawing || !selectionDiv) return;

        const rect = canvas.getBoundingClientRect();
        const currentX = e.clientX - rect.left;
        const currentY = e.clientY - rect.top;
        const width = currentX - startX;
        const height = currentY - startY;

        selectionDiv.style.width = Math.abs(width) + 'px';
        selectionDiv.style.height = Math.abs(height) + 'px';
        selectionDiv.style.left = (width < 0 ? currentX : startX) + 'px';
        selectionDiv.style.top = (height < 0 ? currentY : startY) + 'px';
    });

    overlay.addEventListener('mouseup', (e) => {
        if (!isDrawing || !selectionDiv) return;

        isDrawing = false;
        const rect = canvas.getBoundingClientRect();
        const endX = e.clientX - rect.left;
        const endY = e.clientY - rect.top;

        captureScreenshot(
            canvas,
            Math.min(startX, endX),
            Math.min(startY, endY),
            Math.abs(endX - startX),
            Math.abs(endY - startY),
            pageNum
        );

        overlay.removeChild(selectionDiv);
        selectionDiv = null;
    });
}

function captureScreenshot(sourceCanvas, x, y, width, height, pageNum) {
    const screenshotCanvas = document.createElement('canvas');
    screenshotCanvas.width = width;
    screenshotCanvas.height = height;

    const ctx = screenshotCanvas.getContext('2d');
    ctx.drawImage(sourceCanvas, x, y, width, height, 0, 0, width, height);

    const imageData = screenshotCanvas.toDataURL('image/png');
    APP_STATE.screenshotData = imageData;
    APP_STATE.screenshotPage = pageNum;

    showScreenshotDialog(imageData, pageNum);
}

function showScreenshotDialog(imageData, pageNum) {
    const dialog = document.getElementById('screenshotDialog');
    const preview = document.getElementById('screenshotPreview');
    const select = document.getElementById('replaceImageSelect');

    preview.src = imageData;

    select.innerHTML = '<option value="">-- New image --</option>';
    APP_STATE.mediaFiles.forEach(file => {
        const option = document.createElement('option');
        option.value = file.path;
        option.textContent = file.name;
        select.appendChild(option);
    });

    const timestamp = Date.now();
    document.getElementById('screenshotFilename').value = `screenshot_page${pageNum}_${timestamp}.png`;
    dialog.style.display = 'flex';
}

function closeScreenshotDialog() {
    document.getElementById('screenshotDialog').style.display = 'none';
    if (APP_STATE.screenshotMode) {
        toggleScreenshotMode();
    }
}

async function saveScreenshot() {
    const filename = document.getElementById('screenshotFilename').value;
    const replaceImage = document.getElementById('replaceImageSelect').value;
    const targetFilename = replaceImage || filename;

    try {
        showLoading();

        const response = await fetch('/api/screenshot', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                imageData: APP_STATE.screenshotData,
                targetFilename,
                pageNumber: APP_STATE.screenshotPage
            })
        });

        const result = await response.json();
        hideLoading();
        closeScreenshotDialog();

        if (result.error) {
            showNotification(result.error, 'error');
        } else {
            showNotification(result.message, 'success');
            await loadMediaFiles();
        }
    } catch (error) {
        console.error('Error saving screenshot:', error);
        showNotification('Failed to save screenshot', 'error');
        hideLoading();
    }
}

async function loadMediaFiles() {
    try {
        const response = await fetch('/api/media-list');
        const data = await response.json();
        if (data.files) {
            APP_STATE.mediaFiles = data.files;
        }
    } catch (error) {
        console.error('Error loading media files:', error);
    }
}

// XML operations
function formatXML() {
    try {
        const xml = APP_STATE.monacoEditor.getValue();
        const formatted = formatXMLString(xml);
        APP_STATE.monacoEditor.setValue(formatted);
        showNotification('XML formatted successfully', 'success');
    } catch (error) {
        showNotification('Error formatting XML: ' + error.message, 'error');
    }
}

function formatXMLString(xml) {
    const PADDING = '  ';
    const reg = /(>)(<)(\/*)/g;
    let pad = 0;

    xml = xml.replace(reg, '$1\n$2$3');

    return xml.split('\n').map((node) => {
        let indent = 0;
        if (node.match(/.+<\/\w[^>]*>$/)) {
            indent = 0;
        } else if (node.match(/^<\/\w/) && pad > 0) {
            pad -= 1;
        } else if (node.match(/^<\w[^>]*[^\/]>.*$/)) {
            indent = 1;
        } else {
            indent = 0;
        }

        const padding = PADDING.repeat(pad);
        pad += indent;

        return padding + node;
    }).join('\n');
}

async function validateXML() {
    try {
        const xml = APP_STATE.monacoEditor.getValue();
        const parser = new DOMParser();
        const xmlDoc = parser.parseFromString(xml, 'text/xml');
        const parserError = xmlDoc.querySelector('parsererror');

        if (parserError) {
            showNotification('XML validation failed: Invalid syntax', 'error');
        } else {
            showNotification('XML is well-formed', 'success');
        }
    } catch (error) {
        showNotification('Error validating XML: ' + error.message, 'error');
    }
}

async function refreshHTMLPreview() {
    try {
        showLoading();
        const xml = APP_STATE.monacoEditor.getValue();

        const response = await fetch('/api/render-html', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ xml })
        });

        const result = await response.json();
        hideLoading();

        if (result.error) {
            showNotification(result.error, 'error');
        } else {
            APP_STATE.htmlContent = result.html;
            const enhanced = enhanceHTMLContent(result.html);
            const htmlPreview = document.getElementById('htmlPreview');
            htmlPreview.innerHTML = enhanced;
            renderMathInContainer(htmlPreview);
            showNotification('HTML preview refreshed', 'success');
        }
    } catch (error) {
        console.error('Error refreshing HTML:', error);
        showNotification('Failed to refresh HTML preview', 'error');
        hideLoading();
    }
}

function toggleThumbnails() {
    const thumbnailBar = document.getElementById('thumbnailBar');
    const btn = document.getElementById('toggleThumbnailsBtn');

    thumbnailBar.classList.toggle('collapsed');

    if (thumbnailBar.classList.contains('collapsed')) {
        btn.innerHTML = '<i class="fas fa-chevron-up"></i> Pages';
    } else {
        btn.innerHTML = '<i class="fas fa-chevron-down"></i> Pages';
    }
}

function setupResizer() {
    const resizer = document.getElementById('resizer');
    const leftPanel = document.querySelector('.left-panel');
    const rightPanel = document.querySelector('.right-panel');

    let isResizing = false;

    resizer.addEventListener('mousedown', (e) => {
        isResizing = true;
        document.body.style.cursor = 'col-resize';
    });

    document.addEventListener('mousemove', (e) => {
        if (!isResizing) return;

        const containerWidth = document.querySelector('.main-container').offsetWidth;
        const leftWidth = (e.clientX / containerWidth) * 100;

        if (leftWidth > 20 && leftWidth < 80) {
            leftPanel.style.width = leftWidth + '%';
            rightPanel.style.width = (100 - leftWidth) + '%';
        }
    });

    document.addEventListener('mouseup', () => {
        isResizing = false;
        document.body.style.cursor = '';
    });
}

function showLoading() {
    document.getElementById('loadingSpinner').style.display = 'flex';
}

function hideLoading() {
    document.getElementById('loadingSpinner').style.display = 'none';
}

function showNotification(message, type = 'success') {
    const toast = document.getElementById('notificationToast');
    toast.textContent = message;
    toast.className = `notification-toast ${type} show`;
    setTimeout(() => toast.classList.remove('show'), 3000);
}

// Rich text toolbar
function setupRichTextToolbar() {
    document.querySelectorAll('.toolbar-btn[data-command]').forEach(btn => {
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            const command = btn.getAttribute('data-command');
            document.execCommand(command, false, null);
            document.getElementById('htmlEditableContent').focus();
        });
    });

    const fontSizeSelect = document.getElementById('fontSizeSelect');
    if (fontSizeSelect) {
        fontSizeSelect.addEventListener('change', (e) => {
            const size = e.target.value;
            if (size) {
                document.execCommand('fontSize', false, size);
                document.getElementById('htmlEditableContent').focus();
            }
            e.target.value = '';
        });
    }

    const fontNameSelect = document.getElementById('fontNameSelect');
    if (fontNameSelect) {
        fontNameSelect.addEventListener('change', (e) => {
            const font = e.target.value;
            if (font) {
                document.execCommand('fontName', false, font);
                document.getElementById('htmlEditableContent').focus();
            }
            e.target.value = '';
        });
    }

    const textColorPicker = document.getElementById('textColorPicker');
    if (textColorPicker) {
        textColorPicker.addEventListener('change', (e) => {
            document.execCommand('foreColor', false, e.target.value);
            document.getElementById('htmlEditableContent').focus();
        });
    }

    const bgColorPicker = document.getElementById('bgColorPicker');
    if (bgColorPicker) {
        bgColorPicker.addEventListener('change', (e) => {
            document.execCommand('backColor', false, e.target.value);
            document.getElementById('htmlEditableContent').focus();
        });
    }
}

function insertLink() {
    const url = prompt('Enter URL:', 'https://');
    if (url && url.trim() !== '' && url !== 'https://') {
        document.execCommand('createLink', false, url);
        document.getElementById('htmlEditableContent').focus();
    }
}

function insertTable() {
    const rows = prompt('Number of rows:', '3');
    const cols = prompt('Number of columns:', '3');

    if (rows && cols) {
        const numRows = parseInt(rows);
        const numCols = parseInt(cols);

        if (numRows > 0 && numCols > 0) {
            let tableHTML = '<table border="1" style="border-collapse: collapse; width: 100%; margin: 1rem 0;"><tbody>';
            for (let i = 0; i < numRows; i++) {
                tableHTML += '<tr>';
                for (let j = 0; j < numCols; j++) {
                    tableHTML += '<td style="border: 1px solid #e2e8f0; padding: 0.5rem;">&nbsp;</td>';
                }
                tableHTML += '</tr>';
            }
            tableHTML += '</tbody></table>';
            document.execCommand('insertHTML', false, tableHTML);
            document.getElementById('htmlEditableContent').focus();
        }
    }
}

function showMathSymbolPicker() {
    document.getElementById('mathSymbolDialog').style.display = 'flex';
}

function closeMathSymbolDialog() {
    document.getElementById('mathSymbolDialog').style.display = 'none';
}

function insertSymbol(symbol) {
    const editableContent = document.getElementById('htmlEditableContent');
    editableContent.focus();
    document.execCommand('insertText', false, symbol);
    closeMathSymbolDialog();
}

// Block overlay functions
function toggleBlockOverlay() {
    APP_STATE.showBlockOverlay = !APP_STATE.showBlockOverlay;
    const btn = document.getElementById('blockOverlayBtn');

    if (APP_STATE.showBlockOverlay) {
        btn.classList.add('active');
        btn.style.background = '#3b82f6';
        btn.style.color = 'white';
        document.body.classList.add('show-block-overlay');
        applyBlockOverlays();
    } else {
        btn.classList.remove('active');
        btn.style.background = '';
        btn.style.color = '';
        document.body.classList.remove('show-block-overlay');
        removeBlockOverlays();
    }
}

function applyBlockOverlays() {
    const htmlContainer = APP_STATE.currentView === 'html'
        ? document.getElementById('htmlPreview')
        : document.getElementById('htmlEditableContent');

    if (!htmlContainer) return;

    const isEditMode = APP_STATE.currentView === 'htmledit';
    const blocks = htmlContainer.querySelectorAll('[data-page], [data-reading-block], [data-reading-order], [data-col-id]');

    blocks.forEach((block, index) => {
        const existingOverlay = block.querySelector('.block-info-overlay');
        if (existingOverlay) existingOverlay.remove();

        const page = block.getAttribute('data-page') || '-';
        const readingBlock = block.getAttribute('data-reading-block') || '-';
        const readingOrder = block.getAttribute('data-reading-order') || '-';
        const colId = block.getAttribute('data-col-id') || '-';

        if (page !== '-' || readingBlock !== '-' || readingOrder !== '-' || colId !== '-') {
            const overlay = document.createElement('div');
            overlay.className = 'block-info-overlay';

            if (isEditMode) {
                overlay.innerHTML = `
                    <span class="block-info-item page" title="Page">P:${page}</span>
                    <span class="block-info-item block editable" title="Click to edit Reading Block" data-field="reading-block" data-element-id="${index}">B:<span class="editable-value">${readingBlock}</span></span>
                    <span class="block-info-item order" title="Reading Order">O:${readingOrder}</span>
                    <span class="block-info-item col editable" title="Click to edit Column ID" data-field="col-id" data-element-id="${index}">C:<span class="editable-value">${colId}</span></span>
                `;
                block.setAttribute('data-block-index', index);
            } else {
                overlay.innerHTML = `
                    <span class="block-info-item page" title="Page">P:${page}</span>
                    <span class="block-info-item block" title="Reading Block">B:${readingBlock}</span>
                    <span class="block-info-item order" title="Reading Order">O:${readingOrder}</span>
                    <span class="block-info-item col" title="Column ID">C:${colId}</span>
                `;
            }

            const computedStyle = window.getComputedStyle(block);
            if (computedStyle.position === 'static') {
                block.style.position = 'relative';
            }

            block.insertBefore(overlay, block.firstChild);

            if (isEditMode) {
                const editableItems = overlay.querySelectorAll('.block-info-item.editable');
                editableItems.forEach(item => {
                    item.addEventListener('click', (e) => {
                        e.stopPropagation();
                        startInlineEdit(item, block);
                    });
                });
            }

            if (readingBlock !== '-') {
                const blockNum = parseInt(readingBlock);
                const colors = ['#3b82f6', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#ec4899', '#06b6d4'];
                const color = colors[blockNum % colors.length];
                block.style.borderLeft = `4px solid ${color}`;
            }
        }
    });
}

function startInlineEdit(item, blockElement) {
    if (item.querySelector('input')) return;

    const field = item.getAttribute('data-field');
    const valueSpan = item.querySelector('.editable-value');
    const currentValue = valueSpan.textContent;

    const input = document.createElement('input');
    input.type = 'text';
    input.className = 'inline-edit-input';
    input.value = currentValue === '-' ? '' : currentValue;
    input.setAttribute('data-original-value', currentValue);
    input.size = 3;

    valueSpan.style.display = 'none';
    item.appendChild(input);
    input.focus();
    input.select();

    input.addEventListener('blur', () => finishInlineEdit(input, item, blockElement, field));
    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            input.blur();
        } else if (e.key === 'Escape') {
            e.preventDefault();
            input.value = input.getAttribute('data-original-value');
            input.blur();
        }
    });
}

function finishInlineEdit(input, item, blockElement, field) {
    const newValue = input.value.trim() || '-';
    const originalValue = input.getAttribute('data-original-value');
    const valueSpan = item.querySelector('.editable-value');

    input.remove();
    valueSpan.style.display = '';
    valueSpan.textContent = newValue;

    if (newValue !== originalValue) {
        const attrName = `data-${field}`;

        if (field === 'reading-block' && newValue !== '-' && newValue !== originalValue) {
            handleBlockNumberChange(blockElement, originalValue, newValue);
        } else {
            blockElement.setAttribute(attrName, newValue);
            APP_STATE.isHtmlEdited = true;
            showNotification(`Updated ${field} to ${newValue}`, 'success');
        }
    }
}

function handleBlockNumberChange(sourceElement, oldBlockNum, newBlockNum) {
    const htmlContainer = document.getElementById('htmlEditableContent');
    if (!htmlContainer) return;

    const page = sourceElement.getAttribute('data-page');
    const targetBlocks = htmlContainer.querySelectorAll(
        `[data-page="${page}"][data-reading-block="${newBlockNum}"]`
    );

    if (targetBlocks.length > 0) {
        const lastTargetBlock = targetBlocks[targetBlocks.length - 1];
        sourceElement.setAttribute('data-reading-block', newBlockNum);
        lastTargetBlock.parentNode.insertBefore(sourceElement, lastTargetBlock.nextSibling);
        showNotification(`Merged block ${oldBlockNum} content into block ${newBlockNum}`, 'success');
    } else {
        sourceElement.setAttribute('data-reading-block', newBlockNum);
        showNotification(`Changed block number to ${newBlockNum}`, 'success');
    }

    recalculateReadingOrders(htmlContainer, page);
    setTimeout(() => applyBlockOverlays(), 100);
    APP_STATE.isHtmlEdited = true;
}

function recalculateReadingOrders(container, page) {
    const blocks = container.querySelectorAll(`[data-page="${page}"][data-reading-block]`);
    const blockGroups = {};

    blocks.forEach(block => {
        const blockNum = block.getAttribute('data-reading-block');
        if (!blockGroups[blockNum]) {
            blockGroups[blockNum] = [];
        }
        blockGroups[blockNum].push(block);
    });

    const sortedBlockNums = Object.keys(blockGroups).sort((a, b) => parseInt(a) - parseInt(b));
    let readingOrder = 1;

    sortedBlockNums.forEach(blockNum => {
        blockGroups[blockNum].forEach(block => {
            block.setAttribute('data-reading-order', readingOrder);
            readingOrder++;
        });
    });
}

function removeBlockOverlays() {
    const htmlContainer = APP_STATE.currentView === 'html'
        ? document.getElementById('htmlPreview')
        : document.getElementById('htmlEditableContent');

    if (!htmlContainer) return;

    const overlays = htmlContainer.querySelectorAll('.block-info-overlay');
    overlays.forEach(overlay => overlay.remove());

    const blocks = htmlContainer.querySelectorAll('[data-reading-block]');
    blocks.forEach(block => {
        block.style.borderLeft = '';
        if (block.style.position === 'relative' && !block.getAttribute('data-original-position')) {
            block.style.position = '';
        }
    });
}

// Make functions globally available
window.closeScreenshotDialog = closeScreenshotDialog;
window.saveScreenshot = saveScreenshot;
window.insertLink = insertLink;
window.insertTable = insertTable;
window.showMathSymbolPicker = showMathSymbolPicker;
window.closeMathSymbolDialog = closeMathSymbolDialog;
window.insertSymbol = insertSymbol;
window.toggleBlockOverlay = toggleBlockOverlay;


// ===== EPUB Viewer Functions =====

async function loadEPUB() {
    try {
        console.log('Loading EPUB...');

        // Update UI to show EPUB controls
        document.getElementById('viewerTitle').innerHTML = '<i class="fas fa-book"></i> EPUB Document';
        document.getElementById('pdfControls').style.display = 'none';
        document.getElementById('epubControls').style.display = 'flex';
        document.getElementById('pdfContentWrapper').style.display = 'none';
        document.getElementById('epubViewerWrapper').style.display = 'block';
        document.getElementById('thumbnailBar').style.display = 'none';

        // Initialize EPUB.js
        APP_STATE.epubBook = ePub('/api/epub');

        // Render the EPUB
        APP_STATE.epubRendition = APP_STATE.epubBook.renderTo('epubViewer', {
            width: '100%',
            height: '100%',
            spread: 'none',
            flow: 'scrolled-doc'
        });

        // Display the book
        await APP_STATE.epubRendition.display();

        // Load Table of Contents
        const navigation = await APP_STATE.epubBook.loaded.navigation;
        APP_STATE.epubToc = navigation.toc;
        renderEpubToc();

        // Track location changes
        APP_STATE.epubRendition.on('relocated', (location) => {
            APP_STATE.epubCurrentLocation = location;
            updateEpubChapterInfo();
        });

        console.log('EPUB loaded successfully');
        showNotification('EPUB loaded successfully', 'success');
    } catch (error) {
        console.error('Error loading EPUB:', error);
        showNotification('Failed to load EPUB: ' + error.message, 'error');
    }
}

function renderEpubToc() {
    const tocList = document.getElementById('epubTocList');
    if (!tocList) return;

    tocList.innerHTML = '';

    function renderTocItem(item, level = 1) {
        const tocItem = document.createElement('div');
        tocItem.className = `toc-item level-${Math.min(level, 3)}`;
        tocItem.textContent = item.label;
        tocItem.onclick = () => navigateToEpubSection(item.href);
        tocList.appendChild(tocItem);

        if (item.subitems && item.subitems.length > 0) {
            item.subitems.forEach(subitem => renderTocItem(subitem, level + 1));
        }
    }

    APP_STATE.epubToc.forEach(item => renderTocItem(item));
}

function navigateToEpubSection(href) {
    if (APP_STATE.epubRendition) {
        APP_STATE.epubRendition.display(href);
        closeEpubToc();
    }
}

function updateEpubChapterInfo() {
    if (!APP_STATE.epubCurrentLocation) return;

    const location = APP_STATE.epubCurrentLocation;
    const chapterInfo = document.getElementById('chapterInfo');

    if (chapterInfo) {
        // Try to find current chapter from TOC
        let currentChapter = 'Reading...';

        if (APP_STATE.epubToc && APP_STATE.epubToc.length > 0) {
            // Simple approach: show first TOC item
            // In production, you'd match the current location to the TOC
            currentChapter = `Chapter ${location.start.displayed.page || 1}`;
        }

        chapterInfo.textContent = currentChapter;
    }
}

function toggleEpubToc() {
    const tocPanel = document.getElementById('epubTocPanel');
    if (tocPanel.style.display === 'none' || !tocPanel.style.display) {
        tocPanel.style.display = 'block';
    } else {
        tocPanel.style.display = 'none';
    }
}

function closeEpubToc() {
    const tocPanel = document.getElementById('epubTocPanel');
    if (tocPanel) {
        tocPanel.style.display = 'none';
    }
}

function epubPrevChapter() {
    if (APP_STATE.epubRendition) {
        APP_STATE.epubRendition.prev();
    }
}

function epubNextChapter() {
    if (APP_STATE.epubRendition) {
        APP_STATE.epubRendition.next();
    }
}

function epubChangeFontSize(delta) {
    APP_STATE.epubFontSize += delta;
    APP_STATE.epubFontSize = Math.max(50, Math.min(200, APP_STATE.epubFontSize));

    document.getElementById('epubFontSize').textContent = `${APP_STATE.epubFontSize}%`;

    if (APP_STATE.epubRendition) {
        APP_STATE.epubRendition.themes.fontSize(`${APP_STATE.epubFontSize}%`);
    }
}

// Setup EPUB control event listeners
function setupEpubControls() {
    const epubPrevBtn = document.getElementById('epubPrevBtn');
    const epubNextBtn = document.getElementById('epubNextBtn');
    const epubTocBtn = document.getElementById('epubTocBtn');
    const epubZoomInBtn = document.getElementById('epubZoomInBtn');
    const epubZoomOutBtn = document.getElementById('epubZoomOutBtn');

    if (epubPrevBtn) epubPrevBtn.addEventListener('click', epubPrevChapter);
    if (epubNextBtn) epubNextBtn.addEventListener('click', epubNextChapter);
    if (epubTocBtn) epubTocBtn.addEventListener('click', toggleEpubToc);
    if (epubZoomInBtn) epubZoomInBtn.addEventListener('click', () => epubChangeFontSize(10));
    if (epubZoomOutBtn) epubZoomOutBtn.addEventListener('click', () => epubChangeFontSize(-10));
}

// Call setup for EPUB controls
document.addEventListener('DOMContentLoaded', () => {
    setupEpubControls();
});

// Make EPUB functions globally available
window.closeEpubToc = closeEpubToc;
window.navigateToEpubSection = navigateToEpubSection;


// ===== Package Mode Functions =====

function initializePackageMode(packageData) {
    console.log('Initializing package mode...');

    // Show package indicator and buttons
    document.getElementById('packageIndicator').style.display = 'inline-flex';
    document.getElementById('chaptersBtn').style.display = 'inline-flex';
    document.getElementById('refreshPreviewBtn').style.display = 'inline-flex';
    document.getElementById('editPreviewBtn').style.display = 'inline-flex';

    // Store package data
    APP_STATE.packageMode = true;
    APP_STATE.packageData = packageData;
    APP_STATE.chapters = packageData.chapters || [];

    // Update package info
    const packageInfo = document.getElementById('packageInfo');
    if (packageInfo) {
        packageInfo.textContent = `${packageData.num_chapters} chapters loaded from ${packageData.zip_file.split('/').pop()}`;
    }

    // Load chapter list
    loadChapterList();

    // Setup book preview on the left side
    setupBookPreview();

    console.log(`Package mode initialized with ${packageData.num_chapters} chapters`);
}

function loadChapterList() {
    const chapterList = document.getElementById('chapterList');
    if (!chapterList) return;

    chapterList.innerHTML = '';

    APP_STATE.chapters.forEach((chapter, index) => {
        const chapterItem = document.createElement('div');
        chapterItem.className = 'chapter-item';
        chapterItem.dataset.filename = chapter.file;
        chapterItem.dataset.index = index;

        const chapterNumber = document.createElement('span');
        chapterNumber.className = 'chapter-number';
        chapterNumber.textContent = `Ch ${index + 1}`;

        const chapterName = document.createElement('span');
        chapterName.textContent = chapter.file;

        const chapterIcon = document.createElement('i');
        chapterIcon.className = 'fas fa-file-alt chapter-icon';

        chapterItem.appendChild(chapterNumber);
        chapterItem.appendChild(chapterName);
        chapterItem.appendChild(chapterIcon);

        chapterItem.addEventListener('click', () => selectChapter(chapter.file, index));

        chapterList.appendChild(chapterItem);
    });
}

async function selectChapter(filename, index) {
    try {
        showLoading();

        // Fetch chapter content
        const response = await fetch(`/api/chapter/${filename}`);
        const data = await response.json();

        if (data.error) {
            showNotification(data.error, 'error');
            hideLoading(); // FIX: Always hide loading on error
            return;
        }

        // Update editor with chapter content
        if (APP_STATE.monacoEditor) {
            APP_STATE.monacoEditor.setValue(data.content);
        }

        // Update active chapter in list
        document.querySelectorAll('.chapter-item').forEach(item => {
            item.classList.remove('active');
        });

        const selectedItem = document.querySelector(`.chapter-item[data-index="${index}"]`);
        if (selectedItem) {
            selectedItem.classList.add('active');
        }

        APP_STATE.currentChapter = filename;
        APP_STATE.currentChapterIndex = index;

        // Render chapter preview on the left side
        await renderChapterPreview(data.content);

        // Setup live preview updates when editing
        setupLivePreview();

        showNotification(`Loaded chapter: ${filename}`, 'success');
        hideLoading();

    } catch (error) {
        console.error('Error loading chapter:', error);
        showNotification('Failed to load chapter', 'error');
        hideLoading();
    }
}

// Add data-xml-index attributes to HTML elements for better sync tracking
function addXmlIndexAttributes(container) {
    // Add indices to paragraphs
    const paragraphs = container.querySelectorAll('p');
    paragraphs.forEach((p, index) => {
        p.setAttribute('data-xml-index', index);
        p.setAttribute('data-xml-type', 'para');
    });

    // Add indices to headings/titles
    const headings = container.querySelectorAll('h1, h2, h3, h4, h5, h6');
    headings.forEach((h, index) => {
        h.setAttribute('data-xml-index', index);
        h.setAttribute('data-xml-type', 'title');
    });

    // Add indices to list items
    const listItems = container.querySelectorAll('li');
    listItems.forEach((li, index) => {
        li.setAttribute('data-xml-index', index);
        li.setAttribute('data-xml-type', 'listitem');
    });

    // Add indices to table cells
    const tableCells = container.querySelectorAll('td, th');
    tableCells.forEach((cell, index) => {
        cell.setAttribute('data-xml-index', index);
        cell.setAttribute('data-xml-type', cell.tagName.toLowerCase() === 'th' ? 'header-entry' : 'entry');
    });

    console.log(`Added XML index attributes: ${paragraphs.length} paras, ${headings.length} titles`);
}

async function renderChapterPreview(chapterXml) {
    try {
        // Ensure we have content to render
        if (!chapterXml || chapterXml.trim() === '') {
            console.warn('Empty chapter XML provided');
            const pdfViewer = document.getElementById('pdfViewer');
            if (pdfViewer) {
                pdfViewer.innerHTML = '<div style="padding: 20px; color: white;">No content to display</div>';
            }
            return;
        }

        // Call the API to render chapter HTML
        const response = await fetch('/api/render-book-html', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ xml: chapterXml })
        });

        const result = await response.json();

        if (result.error) {
            console.error('Error rendering chapter preview:', result.error);
            const pdfViewer = document.getElementById('pdfViewer');
            if (pdfViewer) {
                pdfViewer.innerHTML = `<div style="padding: 20px; color: #ff6b6b;">Error: ${result.error}</div>`;
            }
            return;
        }

        // Display the HTML in the viewer (left side)
        const pdfViewer = document.getElementById('pdfViewer');
        if (pdfViewer) {
            pdfViewer.innerHTML = result.html;
            pdfViewer.style.display = 'block';
            pdfViewer.style.background = 'white';
            updatePreviewImages(pdfViewer);
            renderMathInContainer(pdfViewer);

            // Add data-xml-index attributes for better sync tracking
            addXmlIndexAttributes(pdfViewer);

            // Make all links functional - scroll to sections or open external links
            setupLinkHandlers(pdfViewer);

            // Setup scroll synchronization after content is loaded
            setupScrollSync();
        }

    } catch (error) {
        console.error('Error rendering chapter preview:', error);
        const pdfViewer = document.getElementById('pdfViewer');
        if (pdfViewer) {
            pdfViewer.innerHTML = `<div style="padding: 20px; color: #ff6b6b;">Error rendering preview: ${error.message}</div>`;
        }
    }
}

// Setup link handlers - make links functional in preview
function setupLinkHandlers(container) {
    const links = container.querySelectorAll('a[href]');

    links.forEach(link => {
        const href = link.getAttribute('href');

        // Handle cross-chapter links (e.g., ch0026#anchor or http://host/ch0026#anchor)
        const chapterLinkMatch = href && href.match(/(?:^|\/)?(ch\d+)(?:#(.+))?$/i);

        if (chapterLinkMatch && APP_STATE.packageMode) {
            const [, chapterId, anchorId] = chapterLinkMatch;

            link.addEventListener('click', async (e) => {
                e.preventDefault();

                // Find the chapter in the list
                const chapterIndex = APP_STATE.chapters.findIndex(ch =>
                    ch.file.toLowerCase().includes(chapterId.toLowerCase())
                );

                if (chapterIndex >= 0) {
                    const chapter = APP_STATE.chapters[chapterIndex];
                    showNotification(`Loading chapter: ${chapter.file}`, 'info');

                    // Load the chapter
                    await selectChapter(chapter.file, chapterIndex);

                    // If there's an anchor, scroll to it after a short delay
                    if (anchorId) {
                        setTimeout(() => {
                            const pdfViewer = document.getElementById('pdfViewer');
                            const targetElement = pdfViewer && pdfViewer.querySelector(`[id="${anchorId}"]`);

                            if (targetElement) {
                                targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                                targetElement.style.outline = '3px solid #7c3aed';
                                targetElement.style.outlineOffset = '4px';
                                setTimeout(() => {
                                    targetElement.style.outline = '';
                                    targetElement.style.outlineOffset = '';
                                }, 2000);
                                showNotification(`Jumped to: ${anchorId}`, 'success');
                            } else {
                                showNotification(`Element not found: ${anchorId}`, 'warning');
                            }
                        }, 600); // Wait for chapter to render
                    }
                } else {
                    showNotification(`Chapter not found: ${chapterId}`, 'error');
                }
            });

            // Style cross-chapter links
            link.style.color = '#7c3aed'; // Purple for cross-chapter links
            link.style.cursor = 'pointer';
            link.title = `Go to ${chapterId}${anchorId ? '#' + anchorId : ''}`;
        }
        // Handle internal anchors (starting with #) - figures, tables, sections, equations
        else if (href && href.startsWith('#')) {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const targetId = href.substring(1);
                // Search in the full viewer, not just the container
                const pdfViewer = document.getElementById('pdfViewer');
                const targetElement = (pdfViewer && pdfViewer.querySelector(`[id="${targetId}"]`)) ||
                                      container.querySelector(`[id="${targetId}"]`) ||
                                      document.querySelector(`[id="${targetId}"]`);

                if (targetElement) {
                    // Smooth scroll to the target element
                    targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                    // Highlight the target briefly for visual feedback
                    targetElement.style.outline = '3px solid #3b82f6';
                    targetElement.style.outlineOffset = '4px';
                    targetElement.style.transition = 'outline-color 0.3s ease';
                    setTimeout(() => {
                        targetElement.style.outline = '';
                        targetElement.style.outlineOffset = '';
                    }, 2000);
                    showNotification(`Jumped to: ${targetId}`, 'success');
                } else {
                    showNotification(`Element not found: ${targetId}`, 'warning');
                }
            });

            // Make internal links visually distinct
            link.style.color = '#2563eb'; // Blue for same-page links
            link.style.cursor = 'pointer';
            link.style.textDecoration = 'underline';
            link.title = `Go to ${href.substring(1)}`;
        }
        // Handle external links
        else if (href && (href.startsWith('http://') || href.startsWith('https://'))) {
            // Check if it's actually a localhost link to a chapter
            const localhostMatch = href.match(/^https?:\/\/(?:localhost|127\.0\.0\.1)[^/]*\/(ch\d+)(?:#(.+))?$/i);

            if (localhostMatch && APP_STATE.packageMode) {
                // Treat as cross-chapter link
                const [, chapterId, anchorId] = localhostMatch;

                link.addEventListener('click', async (e) => {
                    e.preventDefault();

                    const chapterIndex = APP_STATE.chapters.findIndex(ch =>
                        ch.file.toLowerCase().includes(chapterId.toLowerCase())
                    );

                    if (chapterIndex >= 0) {
                        const chapter = APP_STATE.chapters[chapterIndex];
                        await selectChapter(chapter.file, chapterIndex);

                        if (anchorId) {
                            setTimeout(() => {
                                const pdfViewer = document.getElementById('pdfViewer');
                                const targetElement = pdfViewer.querySelector(`[id="${anchorId}"]`);

                                if (targetElement) {
                                    targetElement.scrollIntoView({ behavior: 'smooth', block: 'start' });
                                    showNotification(`Jumped to: ${anchorId}`, 'success');
                                }
                            }, 600);
                        }
                    }
                });

                link.style.color = '#7c3aed';
                link.style.cursor = 'pointer';
            } else {
                // Real external link
                link.style.color = '#059669'; // Green for external links
                link.style.cursor = 'pointer';
            }
        }
    });
}

// Scroll synchronization between preview and editor
function setupScrollSync() {
    const pdfViewer = document.getElementById('pdfViewer');
    const monacoContainer = document.querySelector('.monaco-editor .monaco-scrollable-element');

    if (!pdfViewer || !monacoContainer) return;

    let isPreviewScrolling = false;
    let isEditorScrolling = false;
    let previewScrollTimeout;
    let editorScrollTimeout;

    // Preview scroll handler
    pdfViewer.addEventListener('scroll', () => {
        if (isEditorScrolling) return;

        clearTimeout(previewScrollTimeout);
        isPreviewScrolling = true;

        // Calculate scroll percentage
        const scrollPercentage = pdfViewer.scrollTop / (pdfViewer.scrollHeight - pdfViewer.clientHeight);

        // Apply to editor
        if (APP_STATE.monacoEditor) {
            const editorScrollHeight = APP_STATE.monacoEditor.getScrollHeight();
            const editorViewportHeight = APP_STATE.monacoEditor.getLayoutInfo().height;
            const targetScroll = scrollPercentage * (editorScrollHeight - editorViewportHeight);
            APP_STATE.monacoEditor.setScrollTop(targetScroll);
        }

        previewScrollTimeout = setTimeout(() => {
            isPreviewScrolling = false;
        }, 100);
    });

    // Editor scroll handler
    if (APP_STATE.monacoEditor) {
        APP_STATE.monacoEditor.onDidScrollChange((e) => {
            if (isPreviewScrolling) return;

            clearTimeout(editorScrollTimeout);
            isEditorScrolling = true;

            // Calculate scroll percentage
            const scrollPercentage = e.scrollTop / (e.scrollHeight - APP_STATE.monacoEditor.getLayoutInfo().height);

            // Apply to preview
            const targetScroll = scrollPercentage * (pdfViewer.scrollHeight - pdfViewer.clientHeight);
            pdfViewer.scrollTop = targetScroll;

            editorScrollTimeout = setTimeout(() => {
                isEditorScrolling = false;
            }, 100);
        });
    }
}

// Live preview - update preview when editing XML
let livePreviewTimeout;
let livePreviewDisposer;

function setupLivePreview() {
    // Remove existing listener if any
    if (livePreviewDisposer) {
        livePreviewDisposer.dispose();
        livePreviewDisposer = null;
    }

    if (!APP_STATE.monacoEditor) {
        console.warn('Monaco editor not ready for live preview');
        return;
    }

    // Listen to content changes in Monaco editor
    livePreviewDisposer = APP_STATE.monacoEditor.onDidChangeModelContent(() => {
        // Clear existing timeout
        clearTimeout(livePreviewTimeout);

        // Debounce - wait 500ms after user stops typing (reduced from 800ms)
        livePreviewTimeout = setTimeout(async () => {
            try {
                const currentXml = APP_STATE.monacoEditor.getValue();

                // Only update if we have content
                if (currentXml && currentXml.trim() !== '') {
                    console.log('Live preview: updating preview...');
                    await renderChapterPreview(currentXml);
                    console.log('Live preview: updated successfully');
                }
            } catch (error) {
                console.error('Live preview error:', error);
                showNotification('Live preview update failed', 'error');
            }
        }, 500);
    });

    console.log('Live preview enabled - changes will update after 500ms');
    showNotification('Live preview enabled', 'success');
}

function toggleChapterNavigator() {
    const nav = document.getElementById('chapterNavigator');
    if (nav.style.display === 'none') {
        nav.style.display = 'flex';
    } else {
        nav.style.display = 'none';
    }
}

function closeChapterNavigator() {
    document.getElementById('chapterNavigator').style.display = 'none';
}

async function savePackage() {
    // Show confirmation dialog before proceeding
    const confirmed = await showSaveConfirmation();
    if (!confirmed) {
        return;
    }

    try {
        showLoading('Saving and reprocessing package...');

        const response = await fetch('/api/save-package', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        });

        const result = await response.json();
        hideLoading();

        if (result.error) {
            showNotification(result.error, 'error');
        } else if (result.reprocessed) {
            // Show detailed reprocessing results
            showReprocessingResults(result);
        } else {
            showNotification(result.message + ' (Backup created)', 'success');
        }

    } catch (error) {
        console.error('Error saving package:', error);
        showNotification('Failed to save package', 'error');
        hideLoading();
    }
}

/**
 * Show confirmation dialog before saving and reprocessing
 * @returns {Promise<boolean>} True if user confirmed, false otherwise
 */
function showSaveConfirmation() {
    return new Promise((resolve) => {
        // Create modal overlay
        const overlay = document.createElement('div');
        overlay.className = 'confirmation-overlay';
        overlay.innerHTML = `
            <div class="confirmation-modal">
                <div class="confirmation-header">
                    <span class="confirmation-icon">⚠️</span>
                    <h3>Confirm Save & Reprocess</h3>
                </div>
                <div class="confirmation-body">
                    <p>Saving will trigger the following operations:</p>
                    <ul>
                        <li><strong>XSLT Transformation</strong> - Apply DTD compliance transformations</li>
                        <li><strong>Repackaging</strong> - Rebuild the output ZIP package</li>
                        <li><strong>DTD Validation & Fixing</strong> - Multi-pass automated fixes</li>
                        <li><strong>Validation Report</strong> - Generate updated validation report</li>
                    </ul>
                    <p class="confirmation-note">
                        <strong>Note:</strong> A backup of your current package will be created before processing.
                        This may take a few moments depending on the document size.
                    </p>
                </div>
                <div class="confirmation-footer">
                    <button class="btn-cancel" id="confirmCancel">Cancel</button>
                    <button class="btn-confirm" id="confirmProceed">Save & Reprocess</button>
                </div>
            </div>
        `;

        document.body.appendChild(overlay);

        // Add event listeners
        document.getElementById('confirmCancel').addEventListener('click', () => {
            document.body.removeChild(overlay);
            resolve(false);
        });

        document.getElementById('confirmProceed').addEventListener('click', () => {
            document.body.removeChild(overlay);
            resolve(true);
        });

        // Close on overlay click (outside modal)
        overlay.addEventListener('click', (e) => {
            if (e.target === overlay) {
                document.body.removeChild(overlay);
                resolve(false);
            }
        });

        // Close on Escape key
        const escHandler = (e) => {
            if (e.key === 'Escape') {
                document.body.removeChild(overlay);
                document.removeEventListener('keydown', escHandler);
                resolve(false);
            }
        };
        document.addEventListener('keydown', escHandler);
    });
}

/**
 * Show reprocessing results in a notification/modal
 * @param {Object} result - The result from save-package API
 */
function showReprocessingResults(result) {
    const statusIcon = result.validation_passed ? '✅' : '⚠️';
    const statusClass = result.validation_passed ? 'success' : 'warning';

    let message = `${statusIcon} ${result.message}`;

    if (result.errors_fixed > 0) {
        message += `\n• Fixed ${result.errors_fixed} validation errors`;
    }

    if (result.remaining_errors > 0) {
        message += `\n• ${result.remaining_errors} errors require manual review`;
    }

    if (result.validation_passed) {
        message += '\n• Package is DTD-compliant';
    }

    showNotification(message, statusClass, 8000); // Show for 8 seconds

    // If there's a report path, offer to show it
    if (result.report_path && result.remaining_errors > 0) {
        console.log('Validation report available at:', result.report_path);
    }
}

async function saveCurrentChapter() {
    if (!APP_STATE.currentChapter) {
        showNotification('No chapter selected', 'error');
        return;
    }

    try {
        showLoading();

        const content = APP_STATE.monacoEditor.getValue();

        const response = await fetch('/api/save-chapter', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                filename: APP_STATE.currentChapter,
                content: content
            })
        });

        const result = await response.json();
        hideLoading();

        if (result.error) {
            showNotification(result.error, 'error');
        } else {
            showNotification(result.message, 'success');

            // Reload combined XML
            await reloadCombinedXML();
        }

    } catch (error) {
        console.error('Error saving chapter:', error);
        showNotification('Failed to save chapter', 'error');
        hideLoading();
    }
}

async function reloadCombinedXML() {
    // Reload the init data to get updated combined XML
    const response = await fetch('/api/init');
    const data = await response.json();

    if (!data.error) {
        APP_STATE.xmlContent = data.xml;
    }
}

// Setup package mode event listeners
function setupPackageModeControls() {
    const chaptersBtn = document.getElementById('chaptersBtn');
    const refreshPreviewBtn = document.getElementById('refreshPreviewBtn');
    const editPreviewBtn = document.getElementById('editPreviewBtn');

    if (chaptersBtn) {
        chaptersBtn.addEventListener('click', toggleChapterNavigator);
    }

    if (refreshPreviewBtn) {
        refreshPreviewBtn.addEventListener('click', refreshBookPreview);
    }

    if (editPreviewBtn) {
        editPreviewBtn.addEventListener('click', togglePreviewEdit);
    }
}

// Override save function for package mode
const originalSaveChanges = window.saveChanges || saveChanges;

function saveChangesOverride(reprocess) {
    if (APP_STATE.packageMode && APP_STATE.currentChapter) {
        // In package mode, save the current chapter
        saveCurrentChapter();
    } else {
        // Original save logic
        if (typeof originalSaveChanges === 'function') {
            originalSaveChanges(reprocess);
        }
    }
}

// Toggle preview edit mode
let previewEditMode = false;
let previewChangeTimeout;

function togglePreviewEdit() {
    const pdfViewer = document.getElementById('pdfViewer');
    const editPreviewBtn = document.getElementById('editPreviewBtn');

    previewEditMode = !previewEditMode;

    if (previewEditMode) {
        // Enable editing on specific content elements only
        const editableElements = pdfViewer.querySelectorAll('p, h1, h2, h3, h4, h5, h6, li, td, th, abbr, em, strong, span:not([class]), a');

        editableElements.forEach(el => {
            el.contentEditable = 'true';
            el.style.outline = '1px dashed #93c5fd';
            el.addEventListener('focus', (e) => {
                e.target.style.outline = '2px solid #3b82f6';
            });
            el.addEventListener('blur', (e) => {
                e.target.style.outline = '1px dashed #93c5fd';
            });
        });

        pdfViewer.style.border = '3px solid #3b82f6';
        editPreviewBtn.classList.add('active');
        editPreviewBtn.innerHTML = '<i class="fas fa-check"></i> Done Editing';

        // Show MS Word-like editing toolbar
        const previewEditToolbar = document.getElementById('previewEditToolbar');
        if (previewEditToolbar) {
            previewEditToolbar.style.display = 'flex';
        }

        // Hide PDF controls when in edit mode
        const pdfControls = document.getElementById('pdfControls');
        if (pdfControls) {
            pdfControls.style.display = 'none';
        }

        console.log('Preview editing enabled on', editableElements.length, 'elements');
        console.log('Monaco editor:', APP_STATE.monacoEditor ? 'Ready' : 'Not ready');
        console.log('Current chapter:', APP_STATE.currentChapter || 'None');

        showNotification(`Edit mode ON - ${editableElements.length} elements editable. Use toolbar for formatting.`, 'info');

        // Listen for changes on pdfViewer
        pdfViewer.addEventListener('input', onPreviewChange, true);
        pdfViewer.addEventListener('DOMSubtreeModified', onPreviewChange, true);
    } else {
        // Disable editing on all editable elements
        const editableElements = pdfViewer.querySelectorAll('[contenteditable="true"]');

        editableElements.forEach(el => {
            el.contentEditable = 'false';
            el.style.outline = 'none';
        });

        pdfViewer.style.border = 'none';
        editPreviewBtn.classList.remove('active');
        editPreviewBtn.innerHTML = '<i class="fas fa-edit"></i> Edit Preview';

        // Hide MS Word-like editing toolbar
        const previewEditToolbar = document.getElementById('previewEditToolbar');
        if (previewEditToolbar) {
            previewEditToolbar.style.display = 'none';
        }

        // Show PDF controls when not in edit mode
        const pdfControls = document.getElementById('pdfControls');
        if (pdfControls) {
            pdfControls.style.display = 'flex';
        }

        // Remove listeners
        pdfViewer.removeEventListener('input', onPreviewChange, true);
        pdfViewer.removeEventListener('DOMSubtreeModified', onPreviewChange, true);

        console.log('Performing final sync...');
        showNotification('Edit mode OFF - syncing to XML...', 'info');

        // Final sync
        syncPreviewToXml();
    }
}

function onPreviewChange(e) {
    console.log('Preview changed - event:', e.type);

    // Debounce changes
    clearTimeout(previewChangeTimeout);
    previewChangeTimeout = setTimeout(() => {
        console.log('Debounce timer fired - syncing...');
        syncPreviewToXml();
    }, 1000); // Wait 1 second after typing stops
}

// Helper function to convert HTML elements to DocBook XML elements
function convertHtmlToDocBook(htmlElement, xmlDoc) {
    let result = '';

    for (const node of htmlElement.childNodes) {
        if (node.nodeType === Node.TEXT_NODE) {
            result += node.textContent;
        } else if (node.nodeType === Node.ELEMENT_NODE) {
            const tagName = node.tagName.toLowerCase();

            // Map HTML tags to DocBook tags
            if (tagName === 'strong' || tagName === 'b') {
                result += `<emphasis role="bold">${node.textContent}</emphasis>`;
            } else if (tagName === 'em' || tagName === 'i') {
                result += `<emphasis>${node.textContent}</emphasis>`;
            } else if (tagName === 'u') {
                result += `<emphasis role="underline">${node.textContent}</emphasis>`;
            } else if (tagName === 'a') {
                const href = node.getAttribute('href') || '';
                result += `<ulink url="${href}">${node.textContent}</ulink>`;
            } else if (tagName === 'code') {
                result += `<code>${node.textContent}</code>`;
            } else if (tagName === 'abbr') {
                result += `<abbrev>${node.textContent}</abbrev>`;
            } else {
                // For other tags, just get text content
                result += node.textContent;
            }
        }
    }

    return result;
}

// Helper function to append converted content to XML element
function appendConvertedContent(xmlElement, htmlElement, xmlDoc) {
    for (const node of htmlElement.childNodes) {
        if (node.nodeType === Node.TEXT_NODE) {
            xmlElement.appendChild(xmlDoc.createTextNode(node.textContent));
        } else if (node.nodeType === Node.ELEMENT_NODE) {
            const tagName = node.tagName.toLowerCase();

            // Map HTML tags to DocBook tags
            if (tagName === 'strong' || tagName === 'b') {
                const emphasis = xmlDoc.createElementNS(null, 'emphasis');
                emphasis.setAttribute('role', 'bold');
                emphasis.textContent = node.textContent;
                xmlElement.appendChild(emphasis);
            } else if (tagName === 'em' || tagName === 'i') {
                const emphasis = xmlDoc.createElementNS(null, 'emphasis');
                emphasis.textContent = node.textContent;
                xmlElement.appendChild(emphasis);
            } else if (tagName === 'u') {
                const emphasis = xmlDoc.createElementNS(null, 'emphasis');
                emphasis.setAttribute('role', 'underline');
                emphasis.textContent = node.textContent;
                xmlElement.appendChild(emphasis);
            } else if (tagName === 'a') {
                const ulink = xmlDoc.createElementNS(null, 'ulink');
                const href = node.getAttribute('href') || '';
                ulink.setAttribute('url', href);
                ulink.textContent = node.textContent;
                xmlElement.appendChild(ulink);
            } else if (tagName === 'code') {
                const code = xmlDoc.createElementNS(null, 'code');
                code.textContent = node.textContent;
                xmlElement.appendChild(code);
            } else if (tagName === 'abbr') {
                const abbrev = xmlDoc.createElementNS(null, 'abbrev');
                abbrev.textContent = node.textContent;
                xmlElement.appendChild(abbrev);
            } else if (tagName === 'sub') {
                const subscript = xmlDoc.createElementNS(null, 'subscript');
                subscript.textContent = node.textContent;
                xmlElement.appendChild(subscript);
            } else if (tagName === 'sup') {
                const superscript = xmlDoc.createElementNS(null, 'superscript');
                superscript.textContent = node.textContent;
                xmlElement.appendChild(superscript);
            } else {
                // For other tags, just append text content
                xmlElement.appendChild(xmlDoc.createTextNode(node.textContent));
            }
        }
    }
}

function syncPreviewToXml() {
    console.log('syncPreviewToXml called');
    console.log('Monaco editor exists:', !!APP_STATE.monacoEditor);
    console.log('Current chapter:', APP_STATE.currentChapter);
    console.log('Package mode:', APP_STATE.packageMode);

    if (!APP_STATE.monacoEditor) {
        console.error('Monaco editor not available');
        showNotification('Editor not ready', 'error');
        return;
    }

    // Allow sync in both single chapter mode and package mode
    if (!APP_STATE.currentChapter && !APP_STATE.packageMode) {
        console.error('No chapter loaded');
        showNotification('No chapter loaded', 'error');
        return;
    }

    try {
        // Get current XML from editor
        const currentXml = APP_STATE.monacoEditor.getValue();

        // Parse XML to DOM
        const parser = new DOMParser();
        const xmlDoc = parser.parseFromString(currentXml, 'text/xml');

        // Check for parse errors
        const parseError = xmlDoc.querySelector('parsererror');
        if (parseError) {
            throw new Error('XML parsing error');
        }

        // Get preview content
        const pdfViewer = document.getElementById('pdfViewer');

        // Extract text from editable elements and update XML
        let updated = false;

        // Update paragraphs - preserve inline formatting
        const paragraphs = pdfViewer.querySelectorAll('p[contenteditable="true"]');
        paragraphs.forEach((p, index) => {
            const xmlParas = xmlDoc.querySelectorAll('para');

            if (xmlParas[index]) {
                const newXmlContent = convertHtmlToDocBook(p, xmlDoc);
                const oldContent = xmlParas[index].innerHTML || '';

                if (newXmlContent !== oldContent) {
                    // Clear old content
                    while (xmlParas[index].firstChild) {
                        xmlParas[index].removeChild(xmlParas[index].firstChild);
                    }

                    // Add new content preserving inline elements
                    appendConvertedContent(xmlParas[index], p, xmlDoc);
                    updated = true;
                    console.log(`Updated para ${index} with formatting preserved`);
                }
            }
        });

        // Update headings - preserve inline formatting
        const headings = pdfViewer.querySelectorAll('h1[contenteditable="true"], h2[contenteditable="true"], h3[contenteditable="true"]');
        headings.forEach((h, index) => {
            const xmlTitles = xmlDoc.querySelectorAll('title');

            if (xmlTitles[index]) {
                // Clear old content
                while (xmlTitles[index].firstChild) {
                    xmlTitles[index].removeChild(xmlTitles[index].firstChild);
                }

                // Add new content preserving inline elements
                appendConvertedContent(xmlTitles[index], h, xmlDoc);
                updated = true;
                console.log(`Updated title ${index} with formatting preserved`);
            }
        });

        // Handle inserted images
        const insertedImages = pdfViewer.querySelectorAll('img.inserted-image[data-xml-type="mediaobject"]');
        insertedImages.forEach((img) => {
            const mediaObject = xmlDoc.createElementNS(null, 'mediaobject');
            const imageObject = xmlDoc.createElementNS(null, 'imageobject');
            const imageData = xmlDoc.createElementNS(null, 'imagedata');

            imageData.setAttribute('fileref', img.src);
            if (img.style.width) imageData.setAttribute('width', img.style.width);
            if (img.style.height) imageData.setAttribute('depth', img.style.height);

            imageObject.appendChild(imageData);
            mediaObject.appendChild(imageObject);

            // Find appropriate location in XML (append to current section or chapter)
            const currentSection = xmlDoc.querySelector('sect1, chapter');
            if (currentSection && !img.hasAttribute('data-synced')) {
                currentSection.appendChild(mediaObject);
                img.setAttribute('data-synced', 'true');
                updated = true;
                console.log('Added mediaobject to XML');
            }
        });

        // Handle inserted tables
        const insertedTables = pdfViewer.querySelectorAll('table.inserted-table[data-xml-type="table"]');
        insertedTables.forEach((table) => {
            const xmlTable = xmlDoc.createElementNS(null, 'table');

            // Add caption if exists
            const caption = table.querySelector('caption');
            if (caption) {
                const xmlTitle = xmlDoc.createElementNS(null, 'title');
                xmlTitle.textContent = caption.textContent;
                xmlTable.appendChild(xmlTitle);
            }

            // Create tgroup
            const tgroup = xmlDoc.createElementNS(null, 'tgroup');
            const cols = table.querySelectorAll('tr')[0]?.querySelectorAll('th, td').length || 0;
            tgroup.setAttribute('cols', cols);

            // Add thead if table has header row
            const hasHeader = table.querySelector('th');
            if (hasHeader) {
                const thead = xmlDoc.createElementNS(null, 'thead');
                const headerRow = table.querySelector('tr');
                const xmlRow = xmlDoc.createElementNS(null, 'row');

                headerRow.querySelectorAll('th').forEach(th => {
                    const entry = xmlDoc.createElementNS(null, 'entry');
                    entry.textContent = th.textContent;
                    xmlRow.appendChild(entry);
                });

                thead.appendChild(xmlRow);
                tgroup.appendChild(thead);
            }

            // Add tbody
            const tbody = xmlDoc.createElementNS(null, 'tbody');
            const rows = Array.from(table.querySelectorAll('tr'));
            const startIndex = hasHeader ? 1 : 0;

            rows.slice(startIndex).forEach(tr => {
                const xmlRow = xmlDoc.createElementNS(null, 'row');
                tr.querySelectorAll('td').forEach(td => {
                    const entry = xmlDoc.createElementNS(null, 'entry');
                    entry.textContent = td.textContent;
                    xmlRow.appendChild(entry);
                });
                tbody.appendChild(xmlRow);
            });

            tgroup.appendChild(tbody);
            xmlTable.appendChild(tgroup);

            // Add to XML
            const currentSection = xmlDoc.querySelector('sect1, chapter');
            if (currentSection && !table.hasAttribute('data-synced')) {
                currentSection.appendChild(xmlTable);
                table.setAttribute('data-synced', 'true');
                updated = true;
                console.log('Added table to XML');
            }
        });

        // Handle inserted shapes (SVG)
        const insertedShapes = pdfViewer.querySelectorAll('svg.inserted-shape[data-xml-type="informalfigure"]');
        insertedShapes.forEach((svg) => {
            const informalFigure = xmlDoc.createElementNS(null, 'informalfigure');
            const mediaObject = xmlDoc.createElementNS(null, 'mediaobject');
            const imageObject = xmlDoc.createElementNS(null, 'imageobject');
            const imageData = xmlDoc.createElementNS(null, 'imagedata');

            // Serialize SVG to string and encode as data URL
            const svgString = new XMLSerializer().serializeToString(svg);
            const svgDataUrl = 'data:image/svg+xml;base64,' + btoa(svgString);

            imageData.setAttribute('fileref', svgDataUrl);
            imageData.setAttribute('format', 'SVG');

            imageObject.appendChild(imageData);
            mediaObject.appendChild(imageObject);
            informalFigure.appendChild(mediaObject);

            // Add to XML
            const currentSection = xmlDoc.querySelector('sect1, chapter');
            if (currentSection && !svg.hasAttribute('data-synced')) {
                currentSection.appendChild(informalFigure);
                svg.setAttribute('data-synced', 'true');
                updated = true;
                console.log('Added informalfigure (shape) to XML');
            }
        });

        // Handle inserted lists
        const insertedLists = pdfViewer.querySelectorAll('ol[data-xml-type="orderedlist"], ul[data-xml-type="itemizedlist"]');
        insertedLists.forEach((list) => {
            const isOrdered = list.getAttribute('data-xml-type') === 'orderedlist';
            const xmlList = xmlDoc.createElementNS(null, isOrdered ? 'orderedlist' : 'itemizedlist');

            list.querySelectorAll('li').forEach(li => {
                const listItem = xmlDoc.createElementNS(null, 'listitem');
                const para = xmlDoc.createElementNS(null, 'para');
                para.textContent = li.textContent;
                listItem.appendChild(para);
                xmlList.appendChild(listItem);
            });

            // Add to XML
            const currentSection = xmlDoc.querySelector('sect1, chapter');
            if (currentSection && !list.hasAttribute('data-synced')) {
                currentSection.appendChild(xmlList);
                list.setAttribute('data-synced', 'true');
                updated = true;
                console.log('Added list to XML');
            }
        });

        // Handle inserted links
        const insertedLinks = pdfViewer.querySelectorAll('a[data-xml-type="ulink"]');
        insertedLinks.forEach((link) => {
            // Links are inline elements, so we need to find their parent paragraph
            const parentPara = link.closest('p[contenteditable="true"]');
            if (parentPara && !link.hasAttribute('data-synced')) {
                // The paragraph update above should handle this
                link.setAttribute('data-synced', 'true');
                updated = true;
                console.log('Marked link as synced (handled by paragraph update)');
            }
        });

        // Handle text formatting (bold, italic, underline)
        const formattedTexts = pdfViewer.querySelectorAll('strong[data-xml-type="emphasis"], em[data-xml-type="emphasis"], u[data-xml-type="emphasis"]');
        formattedTexts.forEach((formatted) => {
            // Formatted text is inline, handled by paragraph updates
            if (!formatted.hasAttribute('data-synced')) {
                formatted.setAttribute('data-synced', 'true');
                updated = true;
                console.log('Marked formatted text as synced');
            }
        });

        if (!updated) {
            console.log('No changes detected');
            showNotification('No changes to sync', 'info');
            return;
        }

        // Serialize XML back to string
        const serializer = new XMLSerializer();
        let newXml = serializer.serializeToString(xmlDoc);

        // Clean up XML formatting
        newXml = formatXml(newXml);

        // Disable live preview temporarily
        if (livePreviewDisposer) {
            livePreviewDisposer.dispose();
            livePreviewDisposer = null;
        }

        // Update Monaco editor
        APP_STATE.monacoEditor.setValue(newXml);

        // Re-enable live preview
        setTimeout(() => {
            setupLivePreview();
        }, 100);

        console.log('Preview changes synced to XML successfully');
        showNotification('✓ Changes synced to XML!', 'success');

        // Validate synced XML against DTD
        validateSyncedXml(newXml);

    } catch (error) {
        console.error('Error syncing preview to XML:', error);
        showNotification('Failed to sync: ' + error.message, 'error');
    }
}

// Validate synced XML against DTD
async function validateSyncedXml(xml) {
    try {
        console.log('Validating synced XML against DTD...');

        // First check if XML is well-formed
        const parser = new DOMParser();
        const xmlDoc = parser.parseFromString(xml, 'text/xml');
        const parseError = xmlDoc.querySelector('parsererror');

        if (parseError) {
            console.error('XML is not well-formed:', parseError.textContent);
            showNotification('⚠ Synced XML has syntax errors', 'warning');
            return;
        }

        // Call backend for DTD validation
        const response = await fetch('/api/validate-dtd', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ xml: xml })
        });

        const result = await response.json();

        if (result.valid) {
            console.log('✓ XML is DTD-compliant');
            // Silent success - don't show notification to avoid notification spam
        } else if (result.errors && result.errors.length > 0) {
            console.warn('DTD validation errors:', result.errors);
            const errorCount = result.errors.length;
            showNotification(`⚠ ${errorCount} DTD validation issue(s) found. Click Validate for details.`, 'warning');

            // Optionally auto-fix if backend supports it
            if (result.can_auto_fix) {
                console.log('Auto-fix available for DTD errors');
            }
        }

    } catch (error) {
        console.error('Error validating synced XML:', error);
        // Don't show error notification to avoid disrupting user workflow
        console.warn('DTD validation skipped due to error');
    }
}

// Format XML with proper indentation
function formatXml(xml) {
    try {
        const PADDING = '  '; // 2 spaces for indentation
        const reg = /(>)(<)(\/*)/g;
        let formatted = xml.replace(reg, '$1\n$2$3');

        let pad = 0;
        formatted = formatted.split('\n').map((line) => {
            let indent = 0;
            if (line.match(/.+<\/\w[^>]*>$/)) {
                indent = 0;
            } else if (line.match(/^<\/\w/)) {
                if (pad > 0) pad -= 1;
            } else if (line.match(/^<\w([^>]*[^\/])?>.*$/)) {
                indent = 1;
            }

            const padding = PADDING.repeat(pad);
            pad += indent;

            return padding + line;
        }).join('\n');

        return formatted;
    } catch (e) {
        console.error('Error formatting XML:', e);
        return xml;
    }
}

function convertHtmlToXml(html) {
    let xml = html;

    // Reverse the HTML transformations back to XML
    const reverseTransformations = [
        // Structure
        (/<div class="book-content">/gi, '<book>'),
        (/<\/div><!-- \.book-content -->/gi, '</book>'),
        (/<div class="chapter">/gi, '<chapter>'),
        (/<\/div><!-- \.chapter -->/gi, '</chapter>'),

        // Sections
        (/<section class="section sect1">/gi, '<sect1>'),
        (/<\/section><!-- \.sect1 -->/gi, '</sect1>'),
        (/<section class="section sect2">/gi, '<sect2>'),
        (/<\/section><!-- \.sect2 -->/gi, '</sect2>'),
        (/<section class="section">/gi, '<section>'),
        (/<\/section>/gi, '</section>'),

        // Titles
        (/<h2 class="title">(.*?)<\/h2>/gi, '<title>$1</title>'),
        (/<h2>(.*?)<\/h2>/gi, '<title>$1</title>'),

        // Paragraphs
        (/<p>(.*?)<\/p>/gis, '<para>$1</para>'),

        // Inline elements
        (/<span>(.*?)<\/span>/gi, '<phrase>$1</phrase>'),
        (/<abbr>(.*?)<\/abbr>/gi, '<abbrev>$1</abbrev>'),

        // Links
        (/<a href="([^"]+)"[^>]*>(.*?)<\/a>/gi, '<ulink url="$1">$2</ulink>'),

        // Emphasis
        (/<strong>(.*?)<\/strong>/gi, '<emphasis role="strong">$1</emphasis>'),
        (/<em>(.*?)<\/em>/gi, '<emphasis>$1</emphasis>'),

        // Lists
        (/<ul>/gi, '<itemizedlist>'),
        (/<\/ul>/gi, '</itemizedlist>'),
        (/<ol>/gi, '<orderedlist>'),
        (/<\/ol>/gi, '</orderedlist>'),
        (/<li>(.*?)<\/li>/gi, '<listitem>$1</listitem>'),

        // Images
        (/<img src="\/api\/media\/([^"]+)"[^>]*\/?>/gi, '<imagedata fileref="$1"/>'),

        // Clean up wrapper divs
        (/<div style="padding: 20px; background: white; color: black;">/gi, ''),
        (/<div style="padding: 20px; background: white;">/gi, ''),
    ];

    // Apply reverse transformations
    reverseTransformations.forEach(([pattern, replacement]) => {
        xml = xml.replace(pattern, replacement);
    });

    // Clean up any remaining HTML tags that weren't converted
    xml = xml.replace(/<div[^>]*>/gi, '');
    xml = xml.replace(/<\/div>/gi, '');

    // Add XML declaration if missing
    if (!xml.trim().startsWith('<?xml')) {
        xml = `<?xml version='1.0' encoding='UTF-8'?>\n${xml}`;
    }

    return xml;
}

// Call setup on load
document.addEventListener('DOMContentLoaded', () => {
    setupPackageModeControls();
});

async function setupBookPreview() {
    try {
        // Hide PDF/EPUB viewers
        document.getElementById('pdfContentWrapper').style.display = 'none';
        document.getElementById('epubViewerWrapper').style.display = 'none';
        document.getElementById('pdfControls').style.display = 'none';
        document.getElementById('epubControls').style.display = 'none';
        document.getElementById('thumbnailBar').style.display = 'none';

        // Update title
        document.getElementById('viewerTitle').innerHTML = '<i class="fas fa-book"></i> Book Preview';

        // Show loading message
        const pdfViewer = document.getElementById('pdfViewer');
        pdfViewer.innerHTML = '<div style="padding: 2rem; text-align: center;"><p>Loading book preview...</p></div>';
        document.getElementById('pdfContentWrapper').style.display = 'block';

        // Generate book HTML
        await renderBookPreview();

    } catch (error) {
        console.error('Error setting up book preview:', error);
        showNotification('Failed to setup book preview', 'error');
    }
}

async function renderBookPreview() {
    try {
        showLoading();

        // Check if we're viewing a specific chapter or the full book
        let xml;
        let previewType;

        if (APP_STATE.currentChapter && APP_STATE.monacoEditor) {
            // Render the current chapter from the editor
            xml = APP_STATE.monacoEditor.getValue();
            previewType = 'chapter';
        } else {
            // Render the combined book XML
            xml = APP_STATE.xmlContent;
            previewType = 'book';
        }

        // Call the API to render HTML
        const response = await fetch('/api/render-book-html', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ xml: xml })
        });

        const result = await response.json();
        hideLoading();

        if (result.error) {
            showNotification(result.error, 'error');
            return;
        }

        // Display the HTML in the viewer
        const pdfViewer = document.getElementById('pdfViewer');
        pdfViewer.innerHTML = result.html;
        pdfViewer.style.overflow = 'auto';
        pdfViewer.style.background = 'white';
        updatePreviewImages(pdfViewer);
        renderMathInContainer(pdfViewer);

        // Make all links functional
        setupLinkHandlers(pdfViewer);

        // Setup scroll synchronization
        setupScrollSync();

        const message = previewType === 'chapter' ? 'Chapter preview updated' : 'Book preview rendered successfully';
        showNotification(message, 'success');

    } catch (error) {
        console.error('Error rendering book preview:', error);
        showNotification('Failed to render book preview', 'error');
        hideLoading();
    }
}

async function refreshBookPreview() {
    if (APP_STATE.packageMode) {
        // Update xmlContent with current editor value
        if (APP_STATE.monacoEditor) {
            APP_STATE.xmlContent = APP_STATE.monacoEditor.getValue();
        }
        await renderBookPreview();
    }
}

// ============================================
// Rich Text Editor Features - Context Menu & Insertions
// ============================================

let contextMenuCursorPosition = null;
let selectedShape = 'rectangle';

// Setup context menu for preview editing
function setupPreviewContextMenu() {
    const pdfViewer = document.getElementById('pdfViewer');
    const contextMenu = document.getElementById('previewContextMenu');

    // Show context menu on right-click
    pdfViewer.addEventListener('contextmenu', (e) => {
        // Always show our context menu in Package Mode or when editing
        if (!APP_STATE.packageMode && !previewEditMode) {
            return; // Allow default context menu if not in package mode or edit mode
        }

        e.preventDefault();

        // Save cursor position
        contextMenuCursorPosition = e.target;

        // Position the context menu
        contextMenu.style.display = 'block';
        contextMenu.style.left = e.pageX + 'px';
        contextMenu.style.top = e.pageY + 'px';
    });

    // Hide context menu on click outside
    document.addEventListener('click', (e) => {
        if (!contextMenu.contains(e.target)) {
            contextMenu.style.display = 'none';
        }
    });
}

// Initialize context menu on page load
document.addEventListener('DOMContentLoaded', () => {
    setupPreviewContextMenu();
});

// ============================================
// Image Insertion
// ============================================

function showInsertImageDialog() {
    document.getElementById('previewContextMenu').style.display = 'none';
    document.getElementById('insertImageDialog').style.display = 'flex';
}

function closeInsertImageDialog() {
    document.getElementById('insertImageDialog').style.display = 'none';
    document.getElementById('imageUrl').value = '';
    document.getElementById('imageAlt').value = '';
    document.getElementById('imageWidth').value = '';
    document.getElementById('imageHeight').value = '';
    document.getElementById('imagePreviewBox').innerHTML = '<span>Image preview will appear here</span>';
}

function toggleImageSource() {
    const source = document.querySelector('input[name="imageSource"]:checked').value;
    const urlGroup = document.getElementById('imageUrlGroup');
    const uploadGroup = document.getElementById('imageUploadGroup');

    if (source === 'url') {
        urlGroup.style.display = 'block';
        uploadGroup.style.display = 'none';
    } else {
        urlGroup.style.display = 'none';
        uploadGroup.style.display = 'block';
    }
}

function previewImageUpload() {
    const fileInput = document.getElementById('imageUpload');
    const previewBox = document.getElementById('imagePreviewBox');

    if (fileInput.files && fileInput.files[0]) {
        const reader = new FileReader();
        reader.onload = (e) => {
            previewBox.innerHTML = `<img src="${e.target.result}" alt="Preview">`;
        };
        reader.readAsDataURL(fileInput.files[0]);
    }
}

// Preview image from URL
document.addEventListener('DOMContentLoaded', () => {
    const imageUrlInput = document.getElementById('imageUrl');
    if (imageUrlInput) {
        imageUrlInput.addEventListener('input', (e) => {
            const url = e.target.value;
            const previewBox = document.getElementById('imagePreviewBox');
            if (url) {
                previewBox.innerHTML = `<img src="${url}" alt="Preview" onerror="this.parentElement.innerHTML='<span>Invalid image URL</span>'">`;
            } else {
                previewBox.innerHTML = '<span>Image preview will appear here</span>';
            }
        });
    }
});

function insertImageToPreview() {
    const source = document.querySelector('input[name="imageSource"]:checked').value;
    let imageSrc = '';

    if (source === 'url') {
        imageSrc = document.getElementById('imageUrl').value;
        if (!imageSrc) {
            showNotification('Please enter an image URL', 'error');
            return;
        }
    } else {
        const fileInput = document.getElementById('imageUpload');
        if (!fileInput.files || !fileInput.files[0]) {
            showNotification('Please select an image file', 'error');
            return;
        }
        // For now, use a data URL - in production, upload to server
        const reader = new FileReader();
        reader.onload = (e) => {
            insertImageElement(e.target.result);
        };
        reader.readAsDataURL(fileInput.files[0]);
        return;
    }

    insertImageElement(imageSrc);
}

function insertImageElement(src) {
    const alt = document.getElementById('imageAlt').value || 'Image';
    const width = document.getElementById('imageWidth').value;
    const height = document.getElementById('imageHeight').value;

    // Create image element
    const img = document.createElement('img');
    img.src = src;
    img.alt = alt;
    img.className = 'inserted-image';
    img.contentEditable = 'false';
    img.setAttribute('data-xml-type', 'mediaobject');

    if (width) img.style.width = width;
    if (height) img.style.height = height;

    // Insert at cursor position
    if (contextMenuCursorPosition) {
        contextMenuCursorPosition.appendChild(img);
    } else {
        const pdfViewer = document.getElementById('pdfViewer');
        pdfViewer.appendChild(img);
    }

    closeInsertImageDialog();
    showNotification('Image inserted successfully', 'success');

    // Trigger sync to XML automatically
    setTimeout(() => {
        syncPreviewToXml();
    }, 300);
}

// ============================================
// Table Insertion
// ============================================

function showInsertTableDialog() {
    document.getElementById('previewContextMenu').style.display = 'none';
    document.getElementById('insertTableDialog').style.display = 'flex';
    updateTablePreview();
}

function closeInsertTableDialog() {
    document.getElementById('insertTableDialog').style.display = 'none';
}

function updateTablePreview() {
    const rows = parseInt(document.getElementById('tableRows').value) || 3;
    const cols = parseInt(document.getElementById('tableCols').value) || 3;
    const hasHeader = document.getElementById('tableHeader').checked;

    let html = '<table>';

    for (let r = 0; r < rows; r++) {
        html += '<tr>';
        const isHeaderRow = hasHeader && r === 0;
        const tag = isHeaderRow ? 'th' : 'td';

        for (let c = 0; c < cols; c++) {
            const content = isHeaderRow ? `Header ${c + 1}` : `Cell ${r},${c + 1}`;
            html += `<${tag}>${content}</${tag}>`;
        }
        html += '</tr>';
    }

    html += '</table>';
    document.getElementById('tablePreview').innerHTML = html;
}

// Update preview when inputs change
document.addEventListener('DOMContentLoaded', () => {
    const tableInputs = ['tableRows', 'tableCols', 'tableHeader'];
    tableInputs.forEach(id => {
        const el = document.getElementById(id);
        if (el) {
            el.addEventListener('change', updateTablePreview);
        }
    });
});

function insertTableToPreview() {
    const rows = parseInt(document.getElementById('tableRows').value) || 3;
    const cols = parseInt(document.getElementById('tableCols').value) || 3;
    const hasHeader = document.getElementById('tableHeader').checked;
    const caption = document.getElementById('tableCaption').value;

    // Create table element
    const table = document.createElement('table');
    table.className = 'inserted-table';
    table.contentEditable = 'false';
    table.setAttribute('data-xml-type', 'table');

    // Add caption if provided
    if (caption) {
        const captionEl = document.createElement('caption');
        captionEl.textContent = caption;
        table.appendChild(captionEl);
    }

    // Create table body
    const tbody = document.createElement('tbody');

    for (let r = 0; r < rows; r++) {
        const tr = document.createElement('tr');
        const isHeaderRow = hasHeader && r === 0;

        for (let c = 0; c < cols; c++) {
            const cell = document.createElement(isHeaderRow ? 'th' : 'td');
            cell.contentEditable = 'true';
            cell.textContent = isHeaderRow ? `Header ${c + 1}` : `Cell ${r},${c + 1}`;
            tr.appendChild(cell);
        }

        tbody.appendChild(tr);
    }

    table.appendChild(tbody);

    // Insert at cursor position
    const container = document.createElement('div');
    container.appendChild(table);

    if (contextMenuCursorPosition) {
        contextMenuCursorPosition.appendChild(container);
    } else {
        const pdfViewer = document.getElementById('pdfViewer');
        pdfViewer.appendChild(container);
    }

    closeInsertTableDialog();
    showNotification('Table inserted successfully', 'success');

    // Trigger sync to XML automatically
    setTimeout(() => {
        syncPreviewToXml();
    }, 300);
}

// ============================================
// Shape Insertion
// ============================================

function showInsertShapeDialog() {
    document.getElementById('previewContextMenu').style.display = 'none';
    document.getElementById('insertShapeDialog').style.display = 'flex';
}

function closeInsertShapeDialog() {
    document.getElementById('insertShapeDialog').style.display = 'none';
}

function selectShape(shape) {
    selectedShape = shape;

    // Update UI to show selected shape
    document.querySelectorAll('.shape-option').forEach(opt => {
        opt.classList.remove('selected');
    });
    document.querySelector(`.shape-option[data-shape="${shape}"]`).classList.add('selected');
}

function insertShapeToPreview() {
    const width = parseInt(document.getElementById('shapeWidth').value) || 200;
    const height = parseInt(document.getElementById('shapeHeight').value) || 150;
    const fill = document.getElementById('shapeFill').value;
    const stroke = document.getElementById('shapeStroke').value;
    const text = document.getElementById('shapeText').value;

    // Create SVG element
    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    svg.setAttribute('width', width);
    svg.setAttribute('height', height);
    svg.setAttribute('class', 'inserted-shape');
    svg.setAttribute('data-xml-type', 'informalfigure');
    svg.style.display = 'block';
    svg.style.margin = '1rem 0';

    let shapeElement;

    switch (selectedShape) {
        case 'rectangle':
            shapeElement = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
            shapeElement.setAttribute('width', width);
            shapeElement.setAttribute('height', height);
            shapeElement.setAttribute('rx', '8');
            break;

        case 'circle':
            shapeElement = document.createElementNS('http://www.w3.org/2000/svg', 'circle');
            const radius = Math.min(width, height) / 2;
            shapeElement.setAttribute('cx', width / 2);
            shapeElement.setAttribute('cy', height / 2);
            shapeElement.setAttribute('r', radius);
            break;

        case 'triangle':
            shapeElement = document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
            const points = `${width / 2},0 ${width},${height} 0,${height}`;
            shapeElement.setAttribute('points', points);
            break;

        case 'arrow':
            shapeElement = document.createElementNS('http://www.w3.org/2000/svg', 'path');
            const arrowPath = `M 0 ${height / 2} L ${width * 0.7} ${height / 2} L ${width * 0.7} 0 L ${width} ${height / 2} L ${width * 0.7} ${height} L ${width * 0.7} ${height / 2} Z`;
            shapeElement.setAttribute('d', arrowPath);
            break;

        case 'star':
            shapeElement = document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
            const cx = width / 2, cy = height / 2;
            const outerRadius = Math.min(width, height) / 2;
            const innerRadius = outerRadius * 0.4;
            let starPoints = '';
            for (let i = 0; i < 10; i++) {
                const radius = i % 2 === 0 ? outerRadius : innerRadius;
                const angle = (i * Math.PI / 5) - Math.PI / 2;
                const x = cx + radius * Math.cos(angle);
                const y = cy + radius * Math.sin(angle);
                starPoints += `${x},${y} `;
            }
            shapeElement.setAttribute('points', starPoints.trim());
            break;

        case 'hexagon':
            shapeElement = document.createElementNS('http://www.w3.org/2000/svg', 'polygon');
            const hcx = width / 2, hcy = height / 2;
            const hradius = Math.min(width, height) / 2;
            let hexPoints = '';
            for (let i = 0; i < 6; i++) {
                const angle = (i * Math.PI / 3) - Math.PI / 2;
                const x = hcx + hradius * Math.cos(angle);
                const y = hcy + hradius * Math.sin(angle);
                hexPoints += `${x},${y} `;
            }
            shapeElement.setAttribute('points', hexPoints.trim());
            break;
    }

    shapeElement.setAttribute('fill', fill);
    shapeElement.setAttribute('stroke', stroke);
    shapeElement.setAttribute('stroke-width', '2');
    svg.appendChild(shapeElement);

    // Add text if provided
    if (text) {
        const textElement = document.createElementNS('http://www.w3.org/2000/svg', 'text');
        textElement.setAttribute('x', width / 2);
        textElement.setAttribute('y', height / 2);
        textElement.setAttribute('text-anchor', 'middle');
        textElement.setAttribute('dominant-baseline', 'middle');
        textElement.setAttribute('fill', 'white');
        textElement.setAttribute('font-size', '16');
        textElement.setAttribute('font-weight', 'bold');
        textElement.textContent = text;
        svg.appendChild(textElement);
    }

    // Insert at cursor position
    if (contextMenuCursorPosition) {
        contextMenuCursorPosition.appendChild(svg);
    } else {
        const pdfViewer = document.getElementById('pdfViewer');
        pdfViewer.appendChild(svg);
    }

    closeInsertShapeDialog();
    showNotification('Shape inserted successfully', 'success');

    // Trigger sync to XML automatically
    setTimeout(() => {
        syncPreviewToXml();
    }, 300);
}

// ============================================
// Link Insertion
// ============================================

function showInsertLinkDialog() {
    document.getElementById('previewContextMenu').style.display = 'none';
    const selection = window.getSelection();
    const selectedText = selection.toString();

    if (selectedText) {
        document.getElementById('linkText').value = selectedText;
    }

    document.getElementById('insertLinkDialog').style.display = 'flex';
}

function closeInsertLinkDialog() {
    document.getElementById('insertLinkDialog').style.display = 'none';
    document.getElementById('linkText').value = '';
    document.getElementById('linkUrl').value = '';
}

function insertLinkToPreview() {
    const linkText = document.getElementById('linkText').value;
    const linkUrl = document.getElementById('linkUrl').value;
    const newTab = document.getElementById('linkNewTab').checked;

    if (!linkText || !linkUrl) {
        showNotification('Please enter both link text and URL', 'error');
        return;
    }

    const link = document.createElement('a');
    link.href = linkUrl;
    link.textContent = linkText;
    link.setAttribute('data-xml-type', 'ulink');
    if (newTab) {
        link.target = '_blank';
        link.rel = 'noopener noreferrer';
    }

    // Insert link
    const selection = window.getSelection();
    if (selection.rangeCount > 0) {
        const range = selection.getRangeAt(0);
        range.deleteContents();
        range.insertNode(link);
    } else if (contextMenuCursorPosition) {
        contextMenuCursorPosition.appendChild(link);
    }

    closeInsertLinkDialog();
    showNotification('Link inserted successfully', 'success');

    // Trigger sync to XML automatically
    setTimeout(() => {
        syncPreviewToXml();
    }, 300);
}

// ============================================
// Text Formatting
// ============================================

function formatSelectedText(format) {
    document.getElementById('previewContextMenu').style.display = 'none';

    const selection = window.getSelection();
    if (!selection.rangeCount) return;

    const range = selection.getRangeAt(0);
    const selectedText = selection.toString();

    if (!selectedText) {
        showNotification('Please select text to format', 'error');
        return;
    }

    let element;
    switch (format) {
        case 'bold':
            element = document.createElement('strong');
            element.setAttribute('data-xml-type', 'emphasis');
            element.setAttribute('data-emphasis-role', 'bold');
            break;
        case 'italic':
            element = document.createElement('em');
            element.setAttribute('data-xml-type', 'emphasis');
            break;
        case 'underline':
            element = document.createElement('u');
            element.setAttribute('data-xml-type', 'emphasis');
            element.setAttribute('data-emphasis-role', 'underline');
            break;
    }

    element.textContent = selectedText;
    range.deleteContents();
    range.insertNode(element);

    showNotification(`Text formatted as ${format}`, 'success');

    // Trigger sync to XML automatically
    setTimeout(() => {
        syncPreviewToXml();
    }, 300);
}

// ============================================
// List Insertion
// ============================================

function insertListItem(type) {
    document.getElementById('previewContextMenu').style.display = 'none';

    const listElement = document.createElement(type === 'ordered' ? 'ol' : 'ul');
    listElement.setAttribute('data-xml-type', type === 'ordered' ? 'orderedlist' : 'itemizedlist');
    listElement.style.margin = '1rem 0';
    listElement.style.paddingLeft = '2rem';

    // Create 3 default list items
    for (let i = 0; i < 3; i++) {
        const li = document.createElement('li');
        li.contentEditable = 'true';
        li.textContent = `List item ${i + 1}`;
        li.setAttribute('data-xml-type', 'listitem');
        listElement.appendChild(li);
    }

    // Insert at cursor position
    if (contextMenuCursorPosition) {
        contextMenuCursorPosition.appendChild(listElement);
    } else {
        const pdfViewer = document.getElementById('pdfViewer');
        pdfViewer.appendChild(listElement);
    }

    showNotification(`${type === 'ordered' ? 'Numbered' : 'Bullet'} list inserted`, 'success');

    // Trigger sync to XML automatically
    setTimeout(() => {
        syncPreviewToXml();
    }, 300);
}

// Make functions globally available
window.closeChapterNavigator = closeChapterNavigator;
window.savePackage = savePackage;
window.refreshBookPreview = refreshBookPreview;
window.showInsertImageDialog = showInsertImageDialog;
window.closeInsertImageDialog = closeInsertImageDialog;
window.toggleImageSource = toggleImageSource;
window.previewImageUpload = previewImageUpload;
window.insertImageToPreview = insertImageToPreview;
window.showInsertTableDialog = showInsertTableDialog;
window.closeInsertTableDialog = closeInsertTableDialog;
window.insertTableToPreview = insertTableToPreview;
window.showInsertShapeDialog = showInsertShapeDialog;
window.closeInsertShapeDialog = closeInsertShapeDialog;
window.selectShape = selectShape;
window.insertShapeToPreview = insertShapeToPreview;
window.showInsertLinkDialog = showInsertLinkDialog;
window.closeInsertLinkDialog = closeInsertLinkDialog;
window.insertLinkToPreview = insertLinkToPreview;
window.formatSelectedText = formatSelectedText;
window.insertListItem = insertListItem;
