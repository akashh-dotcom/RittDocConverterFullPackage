/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Tips (extends Tooltip logic)
   ========================================================================== */
; (function ($, window, document, undefined) {

    "use strict";

    // class definition
    var Tips = function ($el) {
        // DOM and $ elements
        this.$el = $el;

        // constructor call
        this.init();
    };
    Tips.prototype = {
        constructor: Tips,

        init: function () {
            this.current = null;

            this.$el.tooltip({ selector: 'a[rel=tooltip]', trigger: 'manual' });

            this.attachEvents();
        },

        attachEvents: function () {
            // bound/proxy'd method calls
            this.f = {
                show: $.proxy(this.show, this),
                hide: $.proxy(this.hide, this)
            };

            // events
            this.$el
                .on('focus mouseenter', 'a[rel=tooltip]', this.f.show)
                .on('blur mouseleave', 'a[rel=tooltip]', this.f.hide);
        },

        show: function (event) {
            var el = event.currentTarget ? event.currentTarget : event;

            if (this.current !== el) {
                this.hide(this.current);
            }
            $(el).tooltip('show');
            this.current = el;
        },

        hide: function (event) {
            if (!event) {
                return;
            }

            var el = event.currentTarget ? event.currentTarget : event;

            $(el).tooltip('hide');
            this.current = null;
        }
    };

    // plugin definition
    $.fn.tips = function () {
        return this.each(function () {
            new Tips($(this));
        });
    };

    $.fn.tips.Constructor = Tips;

})(jQuery, window, document);