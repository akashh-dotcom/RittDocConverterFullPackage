var R2V2 = R2V2 || { };

/* ==========================================================================
    Keyboard Shortcut Mappings
   ========================================================================== */
R2V2.KeyboardMappings = {
    Resource: {
        Next: 'keyup.right.resource',
        Previous: 'keyup.left.resource'
    },
    
    Modal: {
        Close: 'keyup.esc.modal'
    },
    
    Menu: {
        Close: 'keyup.esc.menu'
    },
    
    Folder: {
        Save: 'keyup.return.folder',
        Cancel: 'keyup.esc.folder'
    },
    
    Panel: {
        Save: 'keyup.return.panel',
        Cancel: 'keyup.esc.panel'
    },
    
    Search: {
        Focus: 'keyup./.search',
        Cancel: 'keyup.esc.search'
    },
    
    Help: {
        Open: 'keyup.shift_/.help'
    }
}