/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Print
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    /* DATA-API
     * ============== */
    $(function() {
        R2V2.Elements.Body.on('click.print.data-api', '[data-toggle="print"]', function(e) {
            e.preventDefault();
            
            window.print();
        });
    });
})(jQuery, window, document);