/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    History Back
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    /* DATA-API
     * ============== */
    $(function() {
        R2V2.Elements.Body.on('click.history-back.data-api', '[data-history="back"]', function(e) {
            e.preventDefault();

            history.go(-1);
        });
    });
})(jQuery, window, document);