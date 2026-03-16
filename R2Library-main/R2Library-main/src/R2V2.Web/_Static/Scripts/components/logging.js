/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Logging
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    /* DATA-API
     * ============== */
    $(function() {
        R2V2.Elements.Body.on('click.log.data-api', '[data-log]', function(e) {
            // execute service call
            $.executeService({ url: $(this).data('log') });
        });
    });
})(jQuery, window, document);