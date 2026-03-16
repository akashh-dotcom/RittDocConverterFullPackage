/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Force Numeric
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    /* DATA-API
     * ============== */
    $(function() {
        R2V2.Elements.Body.on('keypress.force-numeric.data-api', 'input.force-numeric', function(e) {
            // backspace (8), enter (13), left arrow (37), right arrow (39) , % = 37 -- Removed
            if (e.which === 0 || e.which === 8 || e.which === 13 || e.which === 39 || /^\d+$/.test(String.fromCharCode(e.which))) return;
            
            e.preventDefault();
        });
    });
})(jQuery, window, document);