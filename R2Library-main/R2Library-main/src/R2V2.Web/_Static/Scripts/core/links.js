/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Link Handling
    * Disabled
    * Tooltip
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    /* DATA-API
     * ============== */
    $(function() {
        // disable "disabled" and "tooltip" links globally
        R2V2.Elements.Body.on('click', 'a.disabled', false).on('click', 'a[rel=tooltip]', false);
    });
})(jQuery, window, document);