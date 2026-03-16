/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Fixed
   ========================================================================== */
; (function ($, window, document, undefined) {

    "use strict";

    // class definition
    var Fixed = function ($el, options) {
        // DOM and $ elements
        this.$el = $el;
        
        // options
        this.options = options;

        // constructor call
        this.init();
    };
    Fixed.prototype = {
        constructor: Fixed,

        init: function ()
        {
            // get offset
            var elOffset = parseInt(this.$el.addClass(this.options.cssClass).css('top'));
            this.$el.removeClass(this.options.cssClass);
            elOffset = elOffset || this.options.offset;
            
            // get top
            this.elTop = this.$el.length && this.$el.offset().top - elOffset;
            
            // flags
            this.isFixed = 0;
            
            this.process();
            this.attachEvents();
        },
        
        attachEvents: function ()
        {
            // bound/proxy'd method calls
            this.f = { onWindowScroll: $.proxy(this.onWindowScroll, this) };

            // events
            R2V2.Elements.Window.on('scroll', this.f.onWindowScroll);
        },

        onWindowScroll: function (event)
        {
            this.process();
        },
        
        process: function()
        {
            var scrollTop = R2V2.Elements.Window.scrollTop();
            
            if (scrollTop >= this.elTop && !this.isFixed)
            {
                this.isFixed = 1;
                this.$el.addClass(this.options.cssClass);
            }
            else if (scrollTop <= this.elTop && this.isFixed)
            {
                this.isFixed = 0;
                this.$el.removeClass(this.options.cssClass);
            }
        }
    };

    // plugin definition
    $.fn.fixed = function (option) {
        return this.each(function () {
            var $this = $(this);

            // build main options before element iteration
            var options = $.extend(true, {}, $.fn.fixed.defaults, typeof option == 'object' && option);
            
            // get element via "selector"
            if (options.selector) $this = $this.find(options.selector);
            
            // element specific options (combine "data" set)
            var data = $this.data('fixed');
            if (data && data instanceof Fixed === false) {
                options = $.extend(true, options, data);
                data = null;
            }
            
            if (!data) $this.data('fixed', (data = new Fixed($this, options)));
            if (typeof option === 'string') data[option]();
        });
    };

    $.fn.fixed.Constructor = Fixed;

    // plugin defaults
    $.fn.fixed.defaults = {
        offset: 0,
        cssClass: 'fixed',
        selector: false
    };

    // plugin data-api
    $(function () {
        $('[data-implement="fixed"]').each(function () {
            var $this = $(this);
            $this.fixed();
        });
    });

})(jQuery, window, document);