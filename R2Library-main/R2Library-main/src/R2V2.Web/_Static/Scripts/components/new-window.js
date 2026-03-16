/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    New Window
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    /* DATA-API
     * ============== */
    $(function() {
        R2V2.Elements.Body.on('click.window.data-api', '[data-toggle="window"]', function(e) {
            var $this = $(this),
                href = $this.attr('href'),
                data = $this.data();
            
            e.preventDefault();
            
            window.open(href, data.target || '', data.specs || '');
        });
    });
})(jQuery, window, document);