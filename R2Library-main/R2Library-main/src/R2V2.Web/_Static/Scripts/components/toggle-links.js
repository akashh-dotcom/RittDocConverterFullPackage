/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Toggle Links
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    /* DATA-API
     * ============== */
    $(function() {
        R2V2.Elements.Body.on('click.links.data-api', '[data-toggle="link"]', function(e) {
            var $this = $(this),
                href = $this.attr('href');
            
            e.preventDefault();

            $('a[href=' + href + ']').filter(function () { return this !== e.target; }).click();
        });
    });
})(jQuery, window, document);