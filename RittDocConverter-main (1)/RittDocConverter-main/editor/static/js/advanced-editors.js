// Advanced Math Equation Editor and Table Editor
// RittDoc Editor - Enhanced editing capabilities

// ============================================================================
// MATH EQUATION EDITOR
// ============================================================================

const MathEditor = {
    currentEquation: '',
    previewElement: null,

    // Math templates for common structures
    templates: {
        fraction: { latex: '\\frac{a}{b}', display: 'a/b', description: 'Fraction' },
        sqrt: { latex: '\\sqrt{x}', display: '√x', description: 'Square Root' },
        nthroot: { latex: '\\sqrt[n]{x}', display: 'ⁿ√x', description: 'Nth Root' },
        power: { latex: 'x^{n}', display: 'xⁿ', description: 'Exponent' },
        subscript: { latex: 'x_{n}', display: 'xₙ', description: 'Subscript' },
        sum: { latex: '\\sum_{i=1}^{n}', display: '∑', description: 'Summation' },
        product: { latex: '\\prod_{i=1}^{n}', display: '∏', description: 'Product' },
        integral: { latex: '\\int_{a}^{b}', display: '∫', description: 'Integral' },
        doubleIntegral: { latex: '\\iint', display: '∬', description: 'Double Integral' },
        tripleIntegral: { latex: '\\iiint', display: '∭', description: 'Triple Integral' },
        limit: { latex: '\\lim_{x \\to a}', display: 'lim', description: 'Limit' },
        matrix2x2: { latex: '\\begin{pmatrix} a & b \\\\ c & d \\end{pmatrix}', display: '[2×2]', description: '2×2 Matrix' },
        matrix3x3: { latex: '\\begin{pmatrix} a & b & c \\\\ d & e & f \\\\ g & h & i \\end{pmatrix}', display: '[3×3]', description: '3×3 Matrix' },
        abs: { latex: '|x|', display: '|x|', description: 'Absolute Value' },
        floor: { latex: '\\lfloor x \\rfloor', display: '⌊x⌋', description: 'Floor' },
        ceil: { latex: '\\lceil x \\rceil', display: '⌈x⌉', description: 'Ceiling' },
        derivative: { latex: '\\frac{d}{dx}', display: 'd/dx', description: 'Derivative' },
        partialDerivative: { latex: '\\frac{\\partial}{\\partial x}', display: '∂/∂x', description: 'Partial Derivative' },
        vector: { latex: '\\vec{v}', display: 'v⃗', description: 'Vector' },
        hat: { latex: '\\hat{x}', display: 'x̂', description: 'Hat (Unit Vector)' },
        bar: { latex: '\\bar{x}', display: 'x̄', description: 'Bar (Mean)' },
        dot: { latex: '\\dot{x}', display: 'ẋ', description: 'Dot (Time Derivative)' },
        ddot: { latex: '\\ddot{x}', display: 'ẍ', description: 'Double Dot' }
    },

    // Greek letters
    greekLetters: {
        lowercase: ['α', 'β', 'γ', 'δ', 'ε', 'ζ', 'η', 'θ', 'ι', 'κ', 'λ', 'μ',
                    'ν', 'ξ', 'ο', 'π', 'ρ', 'σ', 'τ', 'υ', 'φ', 'χ', 'ψ', 'ω'],
        uppercase: ['Α', 'Β', 'Γ', 'Δ', 'Ε', 'Ζ', 'Η', 'Θ', 'Ι', 'Κ', 'Λ', 'Μ',
                    'Ν', 'Ξ', 'Ο', 'Π', 'Ρ', 'Σ', 'Τ', 'Υ', 'Φ', 'Χ', 'Ψ', 'Ω']
    },

    // Operators and relations
    operators: {
        basic: ['+', '−', '×', '÷', '±', '∓', '·', '∘'],
        relations: ['=', '≠', '<', '>', '≤', '≥', '≈', '≡', '∝', '≪', '≫'],
        setTheory: ['∈', '∉', '⊂', '⊃', '⊆', '⊇', '∪', '∩', '∅', '∀', '∃', '∄'],
        logic: ['∧', '∨', '¬', '⇒', '⇔', '⊢', '⊨'],
        arrows: ['→', '←', '↔', '⇒', '⇐', '⇔', '↑', '↓', '↕', '⟶', '⟵', '⟷'],
        misc: ['∞', '∂', '∇', '∆', '√', '∛', '∜', '∠', '⊥', '∥', '≅', '∼']
    },

    // Convert simple LaTeX to Unicode/HTML
    latexToUnicode: function(latex) {
        const conversions = {
            '\\alpha': 'α', '\\beta': 'β', '\\gamma': 'γ', '\\delta': 'δ',
            '\\epsilon': 'ε', '\\zeta': 'ζ', '\\eta': 'η', '\\theta': 'θ',
            '\\iota': 'ι', '\\kappa': 'κ', '\\lambda': 'λ', '\\mu': 'μ',
            '\\nu': 'ν', '\\xi': 'ξ', '\\pi': 'π', '\\rho': 'ρ',
            '\\sigma': 'σ', '\\tau': 'τ', '\\upsilon': 'υ', '\\phi': 'φ',
            '\\chi': 'χ', '\\psi': 'ψ', '\\omega': 'ω',
            '\\Gamma': 'Γ', '\\Delta': 'Δ', '\\Theta': 'Θ', '\\Lambda': 'Λ',
            '\\Xi': 'Ξ', '\\Pi': 'Π', '\\Sigma': 'Σ', '\\Phi': 'Φ',
            '\\Psi': 'Ψ', '\\Omega': 'Ω',
            '\\pm': '±', '\\mp': '∓', '\\times': '×', '\\div': '÷',
            '\\cdot': '·', '\\leq': '≤', '\\geq': '≥', '\\neq': '≠',
            '\\approx': '≈', '\\equiv': '≡', '\\infty': '∞',
            '\\partial': '∂', '\\nabla': '∇', '\\sum': '∑', '\\prod': '∏',
            '\\int': '∫', '\\iint': '∬', '\\iiint': '∭',
            '\\sqrt': '√', '\\forall': '∀', '\\exists': '∃',
            '\\in': '∈', '\\notin': '∉', '\\subset': '⊂', '\\supset': '⊃',
            '\\cup': '∪', '\\cap': '∩', '\\emptyset': '∅',
            '\\rightarrow': '→', '\\leftarrow': '←', '\\leftrightarrow': '↔',
            '\\Rightarrow': '⇒', '\\Leftarrow': '⇐', '\\Leftrightarrow': '⇔',
            '\\to': '→', '\\gets': '←',
            '\\lfloor': '⌊', '\\rfloor': '⌋', '\\lceil': '⌈', '\\rceil': '⌉',
            '\\langle': '⟨', '\\rangle': '⟩',
            '\\perp': '⊥', '\\parallel': '∥', '\\angle': '∠',
            '\\triangle': '△', '\\square': '□', '\\diamond': '◇'
        };

        let result = latex;
        for (const [key, value] of Object.entries(conversions)) {
            result = result.replace(new RegExp(key.replace(/\\/g, '\\\\'), 'g'), value);
        }

        // Handle fractions: \frac{a}{b} -> a/b or a⁄b
        result = result.replace(/\\frac\{([^}]+)\}\{([^}]+)\}/g, '<span class="math-frac"><span class="math-num">$1</span><span class="math-den">$2</span></span>');

        // Handle superscripts: ^{n} -> <sup>n</sup>
        result = result.replace(/\^{([^}]+)}/g, '<sup>$1</sup>');
        result = result.replace(/\^(\w)/g, '<sup>$1</sup>');

        // Handle subscripts: _{n} -> <sub>n</sub>
        result = result.replace(/_{([^}]+)}/g, '<sub>$1</sub>');
        result = result.replace(/_(\w)/g, '<sub>$1</sub>');

        // Handle square root: \sqrt{x} -> √x
        result = result.replace(/√\{([^}]+)\}/g, '√($1)');

        return result;
    },

    // Generate MathML from equation
    toMathML: function(latex) {
        // Basic MathML generation
        let mathml = '<math xmlns="http://www.w3.org/1998/Math/MathML">\n';
        mathml += '  <mrow>\n';

        // Simple conversion for common patterns
        let content = latex;

        // Handle fractions
        content = content.replace(/\\frac\{([^}]+)\}\{([^}]+)\}/g,
            '<mfrac><mrow>$1</mrow><mrow>$2</mrow></mfrac>');

        // Handle superscripts
        content = content.replace(/\^{([^}]+)}/g, '<msup><mo>​</mo><mrow>$1</mrow></msup>');
        content = content.replace(/\^(\w)/g, '<msup><mo>​</mo><mn>$1</mn></msup>');

        // Handle subscripts
        content = content.replace(/_{([^}]+)}/g, '<msub><mo>​</mo><mrow>$1</mrow></msub>');
        content = content.replace(/_(\w)/g, '<msub><mo>​</mo><mn>$1</mn></msub>');

        // Handle sqrt
        content = content.replace(/\\sqrt\{([^}]+)\}/g, '<msqrt><mrow>$1</mrow></msqrt>');
        content = content.replace(/\\sqrt(\w)/g, '<msqrt><mn>$1</mn></msqrt>');

        // Handle sum, integral, product
        content = content.replace(/\\sum_{([^}]+)}\^{([^}]+)}/g,
            '<munderover><mo>∑</mo><mrow>$1</mrow><mrow>$2</mrow></munderover>');
        content = content.replace(/\\int_{([^}]+)}\^{([^}]+)}/g,
            '<msubsup><mo>∫</mo><mrow>$1</mrow><mrow>$2</mrow></msubsup>');
        content = content.replace(/\\prod_{([^}]+)}\^{([^}]+)}/g,
            '<munderover><mo>∏</mo><mrow>$1</mrow><mrow>$2</mrow></munderover>');

        // Convert remaining LaTeX symbols to Unicode
        content = this.latexToUnicode(content);

        // Wrap plain text/numbers
        content = content.replace(/([a-zA-Z])/g, '<mi>$1</mi>');
        content = content.replace(/(\d+)/g, '<mn>$1</mn>');

        mathml += '    ' + content + '\n';
        mathml += '  </mrow>\n';
        mathml += '</math>';

        return mathml;
    },

    // Show the math equation editor dialog
    show: function() {
        const dialog = document.getElementById('mathEquationDialog');
        if (dialog) {
            dialog.style.display = 'flex';
            this.updatePreview();
            document.getElementById('mathLatexInput').focus();
        }
    },

    // Close the dialog
    close: function() {
        const dialog = document.getElementById('mathEquationDialog');
        if (dialog) {
            dialog.style.display = 'none';
        }
    },

    // Update the preview
    updatePreview: function() {
        const input = document.getElementById('mathLatexInput');
        const preview = document.getElementById('mathPreview');

        if (input && preview) {
            const latex = input.value || '';
            const html = this.latexToUnicode(latex);
            preview.innerHTML = html || '<span class="placeholder">Preview will appear here...</span>';
        }
    },

    // Insert template
    insertTemplate: function(templateKey) {
        const template = this.templates[templateKey];
        if (template) {
            const input = document.getElementById('mathLatexInput');
            if (input) {
                const start = input.selectionStart;
                const end = input.selectionEnd;
                const text = input.value;
                input.value = text.substring(0, start) + template.latex + text.substring(end);
                input.selectionStart = input.selectionEnd = start + template.latex.length;
                input.focus();
                this.updatePreview();
            }
        }
    },

    // Insert symbol
    insertSymbol: function(symbol) {
        const input = document.getElementById('mathLatexInput');
        if (input) {
            const start = input.selectionStart;
            const end = input.selectionEnd;
            const text = input.value;
            input.value = text.substring(0, start) + symbol + text.substring(end);
            input.selectionStart = input.selectionEnd = start + symbol.length;
            input.focus();
            this.updatePreview();
        }
    },

    // Insert the equation into the editor
    insert: function(format = 'html') {
        const input = document.getElementById('mathLatexInput');
        if (!input || !input.value.trim()) {
            showNotification('Please enter an equation', 'warning');
            return;
        }

        const latex = input.value.trim();
        let insertContent = '';

        if (format === 'mathml') {
            insertContent = this.toMathML(latex);
        } else {
            // HTML/Unicode format
            insertContent = '<span class="math-equation">' + this.latexToUnicode(latex) + '</span>';
        }

        const editableContent = document.getElementById('htmlEditableContent');
        if (editableContent) {
            editableContent.focus();
            document.execCommand('insertHTML', false, insertContent);
        }

        this.close();
        input.value = '';
        showNotification('Equation inserted', 'success');
    }
};


// ============================================================================
// TABLE EDITOR
// ============================================================================

const TableEditor = {
    selectedCells: [],
    selectionStart: null,
    isSelecting: false,
    currentTable: null,
    contextMenu: null,

    // Initialize table editor
    init: function() {
        this.createContextMenu();
        this.setupEventListeners();
    },

    // Create the context menu
    createContextMenu: function() {
        if (document.getElementById('tableContextMenu')) return;

        const menu = document.createElement('div');
        menu.id = 'tableContextMenu';
        menu.className = 'table-context-menu';
        menu.innerHTML = `
            <div class="context-menu-group">
                <div class="context-menu-title">Row Operations</div>
                <button class="context-menu-item" onclick="TableEditor.insertRowAbove()">
                    <i class="fas fa-arrow-up"></i> Insert Row Above
                </button>
                <button class="context-menu-item" onclick="TableEditor.insertRowBelow()">
                    <i class="fas fa-arrow-down"></i> Insert Row Below
                </button>
                <button class="context-menu-item" onclick="TableEditor.deleteRow()">
                    <i class="fas fa-trash"></i> Delete Row
                </button>
            </div>
            <div class="context-menu-group">
                <div class="context-menu-title">Column Operations</div>
                <button class="context-menu-item" onclick="TableEditor.insertColumnLeft()">
                    <i class="fas fa-arrow-left"></i> Insert Column Left
                </button>
                <button class="context-menu-item" onclick="TableEditor.insertColumnRight()">
                    <i class="fas fa-arrow-right"></i> Insert Column Right
                </button>
                <button class="context-menu-item" onclick="TableEditor.deleteColumn()">
                    <i class="fas fa-trash"></i> Delete Column
                </button>
            </div>
            <div class="context-menu-group">
                <div class="context-menu-title">Cell Operations</div>
                <button class="context-menu-item" onclick="TableEditor.mergeCells()">
                    <i class="fas fa-compress-arrows-alt"></i> Merge Cells
                </button>
                <button class="context-menu-item" onclick="TableEditor.splitCell()">
                    <i class="fas fa-expand-arrows-alt"></i> Split Cell
                </button>
            </div>
            <div class="context-menu-group">
                <div class="context-menu-title">Cell Formatting</div>
                <button class="context-menu-item" onclick="TableEditor.showCellProperties()">
                    <i class="fas fa-cog"></i> Cell Properties
                </button>
                <button class="context-menu-item" onclick="TableEditor.alignLeft()">
                    <i class="fas fa-align-left"></i> Align Left
                </button>
                <button class="context-menu-item" onclick="TableEditor.alignCenter()">
                    <i class="fas fa-align-center"></i> Align Center
                </button>
                <button class="context-menu-item" onclick="TableEditor.alignRight()">
                    <i class="fas fa-align-right"></i> Align Right
                </button>
            </div>
            <div class="context-menu-group">
                <div class="context-menu-title">Table</div>
                <button class="context-menu-item" onclick="TableEditor.showTableProperties()">
                    <i class="fas fa-table"></i> Table Properties
                </button>
                <button class="context-menu-item danger" onclick="TableEditor.deleteTable()">
                    <i class="fas fa-trash"></i> Delete Table
                </button>
            </div>
        `;
        document.body.appendChild(menu);
        this.contextMenu = menu;
    },

    // Setup event listeners
    setupEventListeners: function() {
        // Close context menu on click outside
        document.addEventListener('click', (e) => {
            if (this.contextMenu && !this.contextMenu.contains(e.target)) {
                this.hideContextMenu();
            }
        });

        // Handle keyboard shortcuts for tables
        document.addEventListener('keydown', (e) => {
            if (this.selectedCells.length > 0) {
                if (e.key === 'Delete' || e.key === 'Backspace') {
                    if (!e.target.isContentEditable) {
                        this.clearCellContents();
                    }
                } else if (e.key === 'Escape') {
                    this.clearSelection();
                } else if (e.key === 'Tab') {
                    e.preventDefault();
                    this.navigateCell(e.shiftKey ? -1 : 1);
                }
            }
        });
    },

    // Enable table editing mode
    enableTableEditing: function(container) {
        const tables = container.querySelectorAll('table');

        tables.forEach(table => {
            table.classList.add('editable-table');

            const cells = table.querySelectorAll('td, th');
            cells.forEach(cell => {
                // Make cells editable
                cell.contentEditable = 'true';

                // Mouse down - start selection
                cell.addEventListener('mousedown', (e) => {
                    if (e.button === 2) return; // Right click handled separately

                    if (!e.shiftKey) {
                        this.clearSelection();
                    }

                    this.isSelecting = true;
                    this.selectionStart = cell;
                    this.currentTable = table;
                    this.selectCell(cell);
                });

                // Mouse over - extend selection
                cell.addEventListener('mouseover', (e) => {
                    if (this.isSelecting && this.currentTable === table) {
                        this.extendSelection(cell);
                    }
                });

                // Right click - show context menu
                cell.addEventListener('contextmenu', (e) => {
                    e.preventDefault();
                    if (!this.selectedCells.includes(cell)) {
                        this.clearSelection();
                        this.selectCell(cell);
                    }
                    this.currentTable = table;
                    this.showContextMenu(e.clientX, e.clientY);
                });
            });

            // Mouse up - end selection
            table.addEventListener('mouseup', () => {
                this.isSelecting = false;
            });
        });
    },

    // Select a cell
    selectCell: function(cell) {
        if (!this.selectedCells.includes(cell)) {
            cell.classList.add('cell-selected');
            this.selectedCells.push(cell);
        }
    },

    // Extend selection to include all cells in rectangle
    extendSelection: function(endCell) {
        if (!this.selectionStart || !this.currentTable) return;

        const startPos = this.getCellPosition(this.selectionStart);
        const endPos = this.getCellPosition(endCell);

        const minRow = Math.min(startPos.row, endPos.row);
        const maxRow = Math.max(startPos.row, endPos.row);
        const minCol = Math.min(startPos.col, endPos.col);
        const maxCol = Math.max(startPos.col, endPos.col);

        // Clear current selection
        this.clearSelection();

        // Select all cells in rectangle
        const rows = this.currentTable.querySelectorAll('tr');
        for (let r = minRow; r <= maxRow; r++) {
            const cells = rows[r].querySelectorAll('td, th');
            for (let c = minCol; c <= maxCol; c++) {
                if (cells[c]) {
                    this.selectCell(cells[c]);
                }
            }
        }
    },

    // Get cell position
    getCellPosition: function(cell) {
        const row = cell.parentElement;
        const table = row.parentElement.parentElement || row.parentElement;
        const rows = Array.from(table.querySelectorAll('tr'));
        const rowIndex = rows.indexOf(row);
        const cells = Array.from(row.querySelectorAll('td, th'));
        const colIndex = cells.indexOf(cell);
        return { row: rowIndex, col: colIndex };
    },

    // Clear selection
    clearSelection: function() {
        this.selectedCells.forEach(cell => {
            cell.classList.remove('cell-selected');
        });
        this.selectedCells = [];
        this.selectionStart = null;
        this.hideContextMenu();
    },

    // Show context menu
    showContextMenu: function(x, y) {
        if (!this.contextMenu) return;

        this.contextMenu.style.display = 'block';
        this.contextMenu.style.left = x + 'px';
        this.contextMenu.style.top = y + 'px';

        // Ensure menu stays within viewport
        const rect = this.contextMenu.getBoundingClientRect();
        if (rect.right > window.innerWidth) {
            this.contextMenu.style.left = (x - rect.width) + 'px';
        }
        if (rect.bottom > window.innerHeight) {
            this.contextMenu.style.top = (y - rect.height) + 'px';
        }
    },

    // Hide context menu
    hideContextMenu: function() {
        if (this.contextMenu) {
            this.contextMenu.style.display = 'none';
        }
    },

    // Insert row above
    insertRowAbove: function() {
        if (this.selectedCells.length === 0) return;

        const cell = this.selectedCells[0];
        const row = cell.parentElement;
        const numCols = row.querySelectorAll('td, th').length;

        const newRow = document.createElement('tr');
        for (let i = 0; i < numCols; i++) {
            const newCell = document.createElement('td');
            newCell.innerHTML = '&nbsp;';
            newCell.contentEditable = 'true';
            this.attachCellListeners(newCell);
            newRow.appendChild(newCell);
        }

        row.parentNode.insertBefore(newRow, row);
        this.hideContextMenu();
        showNotification('Row inserted above', 'success');
    },

    // Insert row below
    insertRowBelow: function() {
        if (this.selectedCells.length === 0) return;

        const cell = this.selectedCells[0];
        const row = cell.parentElement;
        const numCols = row.querySelectorAll('td, th').length;

        const newRow = document.createElement('tr');
        for (let i = 0; i < numCols; i++) {
            const newCell = document.createElement('td');
            newCell.innerHTML = '&nbsp;';
            newCell.contentEditable = 'true';
            this.attachCellListeners(newCell);
            newRow.appendChild(newCell);
        }

        row.parentNode.insertBefore(newRow, row.nextSibling);
        this.hideContextMenu();
        showNotification('Row inserted below', 'success');
    },

    // Delete row
    deleteRow: function() {
        if (this.selectedCells.length === 0) return;

        const rows = new Set();
        this.selectedCells.forEach(cell => {
            rows.add(cell.parentElement);
        });

        rows.forEach(row => {
            row.remove();
        });

        this.clearSelection();
        showNotification('Row(s) deleted', 'success');
    },

    // Insert column left
    insertColumnLeft: function() {
        if (this.selectedCells.length === 0 || !this.currentTable) return;

        const colIndex = this.getCellPosition(this.selectedCells[0]).col;
        const rows = this.currentTable.querySelectorAll('tr');

        rows.forEach(row => {
            const cells = row.querySelectorAll('td, th');
            const isHeader = cells[0]?.tagName === 'TH';
            const newCell = document.createElement(isHeader ? 'th' : 'td');
            newCell.innerHTML = '&nbsp;';
            newCell.contentEditable = 'true';
            this.attachCellListeners(newCell);

            if (cells[colIndex]) {
                row.insertBefore(newCell, cells[colIndex]);
            } else {
                row.appendChild(newCell);
            }
        });

        this.hideContextMenu();
        showNotification('Column inserted left', 'success');
    },

    // Insert column right
    insertColumnRight: function() {
        if (this.selectedCells.length === 0 || !this.currentTable) return;

        const colIndex = this.getCellPosition(this.selectedCells[0]).col;
        const rows = this.currentTable.querySelectorAll('tr');

        rows.forEach(row => {
            const cells = row.querySelectorAll('td, th');
            const isHeader = cells[0]?.tagName === 'TH';
            const newCell = document.createElement(isHeader ? 'th' : 'td');
            newCell.innerHTML = '&nbsp;';
            newCell.contentEditable = 'true';
            this.attachCellListeners(newCell);

            if (cells[colIndex + 1]) {
                row.insertBefore(newCell, cells[colIndex + 1]);
            } else {
                row.appendChild(newCell);
            }
        });

        this.hideContextMenu();
        showNotification('Column inserted right', 'success');
    },

    // Delete column
    deleteColumn: function() {
        if (this.selectedCells.length === 0 || !this.currentTable) return;

        const colIndices = new Set();
        this.selectedCells.forEach(cell => {
            colIndices.add(this.getCellPosition(cell).col);
        });

        const sortedIndices = Array.from(colIndices).sort((a, b) => b - a);

        const rows = this.currentTable.querySelectorAll('tr');
        sortedIndices.forEach(colIndex => {
            rows.forEach(row => {
                const cells = row.querySelectorAll('td, th');
                if (cells[colIndex]) {
                    cells[colIndex].remove();
                }
            });
        });

        this.clearSelection();
        showNotification('Column(s) deleted', 'success');
    },

    // Merge cells
    mergeCells: function() {
        if (this.selectedCells.length < 2) {
            showNotification('Select at least 2 cells to merge', 'warning');
            return;
        }

        // Find bounds of selection
        let minRow = Infinity, maxRow = -1, minCol = Infinity, maxCol = -1;
        this.selectedCells.forEach(cell => {
            const pos = this.getCellPosition(cell);
            minRow = Math.min(minRow, pos.row);
            maxRow = Math.max(maxRow, pos.row);
            minCol = Math.min(minCol, pos.col);
            maxCol = Math.max(maxCol, pos.col);
        });

        const rowSpan = maxRow - minRow + 1;
        const colSpan = maxCol - minCol + 1;

        // Combine content
        let combinedContent = '';
        this.selectedCells.forEach(cell => {
            if (cell.textContent.trim()) {
                if (combinedContent) combinedContent += ' ';
                combinedContent += cell.textContent.trim();
            }
        });

        // Get the first cell (top-left)
        const rows = this.currentTable.querySelectorAll('tr');
        const firstCell = rows[minRow].querySelectorAll('td, th')[minCol];

        // Set spans and content
        firstCell.rowSpan = rowSpan;
        firstCell.colSpan = colSpan;
        firstCell.innerHTML = combinedContent || '&nbsp;';

        // Remove other cells
        for (let r = minRow; r <= maxRow; r++) {
            const cells = rows[r].querySelectorAll('td, th');
            for (let c = minCol; c <= maxCol; c++) {
                if (r === minRow && c === minCol) continue;
                if (cells[c]) cells[c].remove();
            }
        }

        this.clearSelection();
        showNotification('Cells merged', 'success');
    },

    // Split cell
    splitCell: function() {
        if (this.selectedCells.length !== 1) {
            showNotification('Select exactly one cell to split', 'warning');
            return;
        }

        const cell = this.selectedCells[0];
        const rowSpan = cell.rowSpan || 1;
        const colSpan = cell.colSpan || 1;

        if (rowSpan === 1 && colSpan === 1) {
            // Show split dialog
            this.showSplitDialog(cell);
            return;
        }

        // Unmerge: reset spans and add back cells
        const pos = this.getCellPosition(cell);
        const rows = this.currentTable.querySelectorAll('tr');

        cell.rowSpan = 1;
        cell.colSpan = 1;

        for (let r = 0; r < rowSpan; r++) {
            const row = rows[pos.row + r];
            if (!row) continue;

            for (let c = 0; c < colSpan; c++) {
                if (r === 0 && c === 0) continue;

                const newCell = document.createElement('td');
                newCell.innerHTML = '&nbsp;';
                newCell.contentEditable = 'true';
                this.attachCellListeners(newCell);

                // Find insertion point
                const existingCells = row.querySelectorAll('td, th');
                const insertIndex = pos.col + c;

                if (existingCells[insertIndex]) {
                    row.insertBefore(newCell, existingCells[insertIndex]);
                } else {
                    row.appendChild(newCell);
                }
            }
        }

        this.clearSelection();
        showNotification('Cell split', 'success');
    },

    // Show split dialog
    showSplitDialog: function(cell) {
        const rows = prompt('Split into how many rows?', '2');
        const cols = prompt('Split into how many columns?', '1');

        if (!rows || !cols) return;

        const numRows = parseInt(rows);
        const numCols = parseInt(cols);

        if (numRows < 1 || numCols < 1) return;

        // TODO: Implement advanced split
        showNotification('Advanced split coming soon', 'info');
        this.hideContextMenu();
    },

    // Align left
    alignLeft: function() {
        this.selectedCells.forEach(cell => {
            cell.style.textAlign = 'left';
        });
        this.hideContextMenu();
    },

    // Align center
    alignCenter: function() {
        this.selectedCells.forEach(cell => {
            cell.style.textAlign = 'center';
        });
        this.hideContextMenu();
    },

    // Align right
    alignRight: function() {
        this.selectedCells.forEach(cell => {
            cell.style.textAlign = 'right';
        });
        this.hideContextMenu();
    },

    // Clear cell contents
    clearCellContents: function() {
        this.selectedCells.forEach(cell => {
            cell.innerHTML = '&nbsp;';
        });
    },

    // Navigate to next/previous cell
    navigateCell: function(direction) {
        if (this.selectedCells.length === 0 || !this.currentTable) return;

        const cell = this.selectedCells[this.selectedCells.length - 1];
        const pos = this.getCellPosition(cell);
        const rows = this.currentTable.querySelectorAll('tr');

        let newRow = pos.row;
        let newCol = pos.col + direction;

        const numCols = rows[newRow].querySelectorAll('td, th').length;

        if (newCol >= numCols) {
            newRow++;
            newCol = 0;
        } else if (newCol < 0) {
            newRow--;
            newCol = numCols - 1;
        }

        if (newRow >= 0 && newRow < rows.length) {
            const cells = rows[newRow].querySelectorAll('td, th');
            if (cells[newCol]) {
                this.clearSelection();
                this.selectCell(cells[newCol]);
                cells[newCol].focus();
            }
        }
    },

    // Show cell properties dialog
    showCellProperties: function() {
        if (this.selectedCells.length === 0) return;

        const dialog = document.getElementById('cellPropertiesDialog');
        if (dialog) {
            dialog.style.display = 'flex';

            // Populate current values from first selected cell
            const cell = this.selectedCells[0];
            document.getElementById('cellWidth').value = cell.style.width || '';
            document.getElementById('cellHeight').value = cell.style.height || '';
            document.getElementById('cellBgColor').value = this.rgbToHex(cell.style.backgroundColor) || '#ffffff';
            document.getElementById('cellBorderColor').value = '#e2e8f0';
            document.getElementById('cellVerticalAlign').value = cell.style.verticalAlign || 'middle';
            document.getElementById('cellPadding').value = parseInt(cell.style.padding) || 8;
        }
        this.hideContextMenu();
    },

    // Apply cell properties
    applyCellProperties: function() {
        const width = document.getElementById('cellWidth').value;
        const height = document.getElementById('cellHeight').value;
        const bgColor = document.getElementById('cellBgColor').value;
        const borderColor = document.getElementById('cellBorderColor').value;
        const verticalAlign = document.getElementById('cellVerticalAlign').value;
        const padding = document.getElementById('cellPadding').value;

        this.selectedCells.forEach(cell => {
            if (width) cell.style.width = width;
            if (height) cell.style.height = height;
            if (bgColor) cell.style.backgroundColor = bgColor;
            if (borderColor) cell.style.borderColor = borderColor;
            if (verticalAlign) cell.style.verticalAlign = verticalAlign;
            if (padding) cell.style.padding = padding + 'px';
        });

        this.closeCellProperties();
        showNotification('Cell properties applied', 'success');
    },

    // Close cell properties
    closeCellProperties: function() {
        const dialog = document.getElementById('cellPropertiesDialog');
        if (dialog) {
            dialog.style.display = 'none';
        }
    },

    // Show table properties dialog
    showTableProperties: function() {
        if (!this.currentTable) return;

        const dialog = document.getElementById('tablePropertiesDialog');
        if (dialog) {
            dialog.style.display = 'flex';

            // Populate current values
            document.getElementById('tableWidth').value = this.currentTable.style.width || '100%';
            document.getElementById('tableBorder').value = parseInt(this.currentTable.getAttribute('border')) || 1;
            document.getElementById('tableCellSpacing').value = parseInt(this.currentTable.getAttribute('cellspacing')) || 0;
            document.getElementById('tableCellPadding').value = parseInt(this.currentTable.getAttribute('cellpadding')) || 8;
            document.getElementById('tableAlign').value = this.currentTable.style.marginLeft === 'auto' &&
                                                          this.currentTable.style.marginRight === 'auto' ? 'center' :
                                                          this.currentTable.style.float || 'none';
        }
        this.hideContextMenu();
    },

    // Apply table properties
    applyTableProperties: function() {
        if (!this.currentTable) return;

        const width = document.getElementById('tableWidth').value;
        const border = document.getElementById('tableBorder').value;
        const spacing = document.getElementById('tableCellSpacing').value;
        const padding = document.getElementById('tableCellPadding').value;
        const align = document.getElementById('tableAlign').value;

        this.currentTable.style.width = width;
        this.currentTable.setAttribute('border', border);
        this.currentTable.setAttribute('cellspacing', spacing);
        this.currentTable.setAttribute('cellpadding', padding);

        // Handle alignment
        if (align === 'center') {
            this.currentTable.style.marginLeft = 'auto';
            this.currentTable.style.marginRight = 'auto';
            this.currentTable.style.float = 'none';
        } else if (align === 'left' || align === 'right') {
            this.currentTable.style.float = align;
            this.currentTable.style.marginLeft = '';
            this.currentTable.style.marginRight = '';
        } else {
            this.currentTable.style.float = 'none';
            this.currentTable.style.marginLeft = '';
            this.currentTable.style.marginRight = '';
        }

        this.closeTableProperties();
        showNotification('Table properties applied', 'success');
    },

    // Close table properties
    closeTableProperties: function() {
        const dialog = document.getElementById('tablePropertiesDialog');
        if (dialog) {
            dialog.style.display = 'none';
        }
    },

    // Delete entire table
    deleteTable: function() {
        if (!this.currentTable) return;

        if (confirm('Are you sure you want to delete this table?')) {
            this.currentTable.remove();
            this.currentTable = null;
            this.clearSelection();
            showNotification('Table deleted', 'success');
        }
        this.hideContextMenu();
    },

    // Helper to attach listeners to new cells
    attachCellListeners: function(cell) {
        cell.addEventListener('mousedown', (e) => {
            if (e.button === 2) return;
            if (!e.shiftKey) this.clearSelection();
            this.isSelecting = true;
            this.selectionStart = cell;
            this.currentTable = cell.closest('table');
            this.selectCell(cell);
        });

        cell.addEventListener('mouseover', (e) => {
            if (this.isSelecting && this.currentTable === cell.closest('table')) {
                this.extendSelection(cell);
            }
        });

        cell.addEventListener('contextmenu', (e) => {
            e.preventDefault();
            if (!this.selectedCells.includes(cell)) {
                this.clearSelection();
                this.selectCell(cell);
            }
            this.currentTable = cell.closest('table');
            this.showContextMenu(e.clientX, e.clientY);
        });
    },

    // Convert RGB to Hex
    rgbToHex: function(rgb) {
        if (!rgb || rgb === 'transparent' || rgb === '') return '#ffffff';

        const match = rgb.match(/^rgb\((\d+),\s*(\d+),\s*(\d+)\)$/);
        if (!match) return rgb;

        const r = parseInt(match[1]).toString(16).padStart(2, '0');
        const g = parseInt(match[2]).toString(16).padStart(2, '0');
        const b = parseInt(match[3]).toString(16).padStart(2, '0');

        return '#' + r + g + b;
    },

    // Show advanced table creation dialog
    showInsertTableDialog: function() {
        const dialog = document.getElementById('insertTableDialog');
        if (dialog) {
            dialog.style.display = 'flex';
            this.updateTablePreview();
        }
    },

    // Close insert table dialog
    closeInsertTableDialog: function() {
        const dialog = document.getElementById('insertTableDialog');
        if (dialog) {
            dialog.style.display = 'none';
        }
    },

    // Update table preview
    updateTablePreview: function() {
        const rows = parseInt(document.getElementById('newTableRows').value) || 3;
        const cols = parseInt(document.getElementById('newTableCols').value) || 3;
        const hasHeader = document.getElementById('newTableHeader').checked;
        const preview = document.getElementById('tablePreviewGrid');

        if (!preview) return;

        let html = '<table class="table-preview-grid">';
        for (let r = 0; r < Math.min(rows, 10); r++) {
            html += '<tr>';
            for (let c = 0; c < Math.min(cols, 10); c++) {
                const tag = (r === 0 && hasHeader) ? 'th' : 'td';
                html += `<${tag}></${tag}>`;
            }
            html += '</tr>';
        }
        html += '</table>';

        if (rows > 10 || cols > 10) {
            html += '<div class="preview-note">Preview limited to 10x10</div>';
        }

        preview.innerHTML = html;
    },

    // Insert new table
    insertNewTable: function() {
        const rows = parseInt(document.getElementById('newTableRows').value) || 3;
        const cols = parseInt(document.getElementById('newTableCols').value) || 3;
        const hasHeader = document.getElementById('newTableHeader').checked;
        const caption = document.getElementById('newTableCaption').value;
        const width = document.getElementById('newTableWidth').value || '100%';

        let tableHTML = `<table style="border-collapse: collapse; width: ${width}; margin: 1rem 0;" border="1">`;

        if (caption) {
            tableHTML += `<caption>${caption}</caption>`;
        }

        if (hasHeader) {
            tableHTML += '<thead><tr>';
            for (let c = 0; c < cols; c++) {
                tableHTML += '<th style="border: 1px solid #e2e8f0; padding: 0.5rem; background: #f8fafc;">Header ' + (c + 1) + '</th>';
            }
            tableHTML += '</tr></thead>';
        }

        tableHTML += '<tbody>';
        const startRow = hasHeader ? 1 : 0;
        for (let r = startRow; r < rows; r++) {
            tableHTML += '<tr>';
            for (let c = 0; c < cols; c++) {
                tableHTML += '<td style="border: 1px solid #e2e8f0; padding: 0.5rem;">&nbsp;</td>';
            }
            tableHTML += '</tr>';
        }
        tableHTML += '</tbody></table>';

        const editableContent = document.getElementById('htmlEditableContent');
        if (editableContent) {
            editableContent.focus();
            document.execCommand('insertHTML', false, tableHTML);

            // Enable editing on the new table
            setTimeout(() => {
                this.enableTableEditing(editableContent);
            }, 100);
        }

        this.closeInsertTableDialog();
        showNotification('Table inserted', 'success');
    }
};


// ============================================================================
// Initialize on DOM ready
// ============================================================================

document.addEventListener('DOMContentLoaded', function() {
    TableEditor.init();

    // Hook into HTML edit mode
    const originalSwitchView = window.switchView;
    if (originalSwitchView) {
        window.switchView = function(view) {
            originalSwitchView(view);

            if (view === 'htmledit') {
                setTimeout(() => {
                    const editableContent = document.getElementById('htmlEditableContent');
                    if (editableContent) {
                        TableEditor.enableTableEditing(editableContent);
                    }
                }, 100);
            }
        };
    }

    // Listen for math input changes
    const mathInput = document.getElementById('mathLatexInput');
    if (mathInput) {
        mathInput.addEventListener('input', () => MathEditor.updatePreview());
    }
});


// Make functions globally available
window.MathEditor = MathEditor;
window.TableEditor = TableEditor;
window.showMathEquationEditor = () => MathEditor.show();
window.closeMathEquationDialog = () => MathEditor.close();
window.insertMathEquation = (format) => MathEditor.insert(format);
window.insertMathTemplate = (template) => MathEditor.insertTemplate(template);
window.insertMathSymbol = (symbol) => MathEditor.insertSymbol(symbol);
window.showAdvancedTableDialog = () => TableEditor.showInsertTableDialog();
window.closeInsertTableDialog = () => TableEditor.closeInsertTableDialog();
window.insertNewTable = () => TableEditor.insertNewTable();
window.updateTablePreview = () => TableEditor.updateTablePreview();
window.applyCellProperties = () => TableEditor.applyCellProperties();
window.closeCellProperties = () => TableEditor.closeCellProperties();
window.applyTableProperties = () => TableEditor.applyTableProperties();
window.closeTableProperties = () => TableEditor.closeTableProperties();
