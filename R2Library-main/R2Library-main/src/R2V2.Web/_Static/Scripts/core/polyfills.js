/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Polyfill logic
   ========================================================================== */
R2V2.Polyfills = Class.extend({
    init: function ()
    {
        this.generatedcontent(); // generated content
        this.forms(); // form enhancements
    },

    generatedcontent: function ()
    {
        // if generated content not supported, add :before or :after replacements
        if (R2V2.Elements.Html.hasClass('generatedcontent')) return;
        
        $('header').append('<span class="bottomfade"></span>');
        $('ul.login-links').find('li:not(:first-child)').prepend('<span>-</span>');
        $('#browse-and-search .columns').find('li').prepend('<span>- </span>');
        $('#topics .columns').find('li').prepend('<span>- </span>');
        //$('.subcontent .accordioncontent').find('li').prepend('<span>- </span>'); // causes race condition in IE7
        $('.actions-menu').children('li').children('a').append(' <span></span>');
        $('a.external').append(' <span></span>');
    },

    forms: function ()
    {
        // cross-browser input field "placeholder" attributes
        $('input, textarea').placeholder();
    }
});

/* ==========================================================================
    Create OnDOMReady
    ========================================================================== */
; (function ($) {
    
    new R2V2.Polyfills();
    
})(jQuery);