/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Security logic
   ========================================================================== */
R2V2.Security = Class.extend({
    init: function ()
    {
        if (R2V2.Config.DisableRightClick === false || R2V2.Config.DisableRightClick == null) return;
        
        // disable contextmenu (right click)
        R2V2.Elements.Document.bind('contextmenu', false);
    }
});

/* ==========================================================================
    Create OnDOMReady
    ========================================================================== */
; (function ($) {
    
    new R2V2.Security();
    
})(jQuery);