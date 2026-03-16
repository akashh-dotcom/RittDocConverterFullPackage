/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    View
    Images
        * inline content
        * default small image -> large image
    Tables
        * inline content
        * default not displayed -> inline table
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    // class definition
    var View = function(content, options) {
        this.options = options;

        this.$element = $(content).delegate('[data-dismiss="view"]', 'click.dismiss.view', $.proxy(this.hide, this));
    };
    View.prototype = {
        constructor: View,
        
        toggle: function ()
        {
            return this[!this.isShown ? 'show' : 'hide']();
        },
            
        show: function ()
        {
            var that = this, e = $.Event('show');

            this.$element.trigger(e);

            if (this.isShown || e.isDefaultPrevented()) return;

            this.isShown = true;

            backdrop.call(this, function() {
                that.$element.addClass(that.options.cssClassName);

                that.$element.trigger('shown');
            });
        },
            
        hide: function ( e )
        {
            e && e.preventDefault();

            e = $.Event('hide');

            this.$element.trigger(e);
            
            if (!this.isShown || e.isDefaultPrevented()) return;

            this.isShown = false;
            
            this.$element.removeClass(this.options.cssClassName).trigger('hidden');

            backdrop.call(this);
        }
    };


    // private methods
    function backdrop (callback)
    {
        if (this.isShown && this.options.backdrop)
        {
            this.$backdrop = $('<div class="view-backdrop" />').appendTo(document.body);
            
            if (this.options.backdrop != 'static')
            {
                this.$backdrop.click($.proxy(this.hide, this));
            }

            callback();
        }
        else if (!this.isShown && this.$backdrop)
        {
            removeBackdrop.call(this);
        }
        else if (callback)
        {
            callback();
        }
    }
    
    function removeBackdrop()
    {
        this.$backdrop.remove();
        this.$backdrop = null;
    }


    // plugin definition
    $.fn.view = function(option) {
        return this.each(function() {
            var $this = $(this),
                data = $this.data('view'),
                options = $.extend({ }, $.fn.view.defaults, $this.data(), typeof option == 'object' && option);
            
            if (!data) $this.data('view', (data = new View(this, options)));
            
            if (typeof option == 'string') data[option]();
            else if (options.show) data.show();
        });
    };

    $.fn.view.Constructor = View;

    // plugin defaults
    $.fn.view.defaults = {
        show: true,
        backdrop: false,
        cssClassName: 'inline'
    };

    // plugin data-api
    $(function() {
        R2V2.Elements.Body.on('click.view.data-api', '[data-view="inline-resource"]', function(e) {
            var $this = $(this),
                $container = $this.parents('.figure').first(),
                option = $container.data('view') ? 'toggle' : $.extend({ }, $container.data(), $this.data());
            
            e.preventDefault();
            $container.view(option);

	        // publish
            $.PubSub(R2V2.PubSubMappings.InlineResourceView.Changed).publish({ figure: $container });
        });
    });
})(jQuery, window, document);