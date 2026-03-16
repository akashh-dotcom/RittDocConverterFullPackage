/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Accordions
    ========================================================================== */
; (function ($, window, document, undefined) {

    "use strict";
    
    // class definition
    var Accordion = function (element, options) {
        // DOM and $ elements
        this.element = element;
        this.$element = $(element);
        
        // options
        this.options = options;
        
        // constructor call
        init.call(this);
    };
    Accordion.prototype = {
        constructor: Accordion,

        toggle: function($el, open)
        {
            var header = $el.prev(),
                classNames = (header.hasClass(this.options.closedCssClass) || open) ? { add: this.options.openedCssClass, remove: this.options.closedCssClass } : { add: this.options.closedCssClass, remove: this.options.openedCssClass };

            header.toggleDisplay(classNames);
            $el.toggleDisplay(classNames);
        },
        
        toggleGroup: function ($groups)
        {
            // "close" any "open"
            this.$element.find('.' + this.options.openedCssClass).toggleDisplay({ add: this.options.closedCssClass, remove: this.options.openedCssClass });
            
            // "open" all up the chain
            _.each($groups, function(group) { this.toggle($(group), true); }, this);
        }
    };
    

    // private methods
    function init ()
    {
        // cache bound/proxy'd method calls
        this.f = { onHeaderClick: $.proxy(onHeaderClick, this) };

        attachEvents.call(this);
    }
        
    function attachEvents ()
    {
        // events
        this.$element.on('click.accordion.data-api', '.accordionhead, .accordion-heading', this.f.onHeaderClick);
    }
        
    function onHeaderClick (event)
    {
        if ($(event.target).is('a')) return; // follow link href/actions

        var content = $(event.currentTarget).next();
        if ($.trim(content.html()).length === 0) return; // don't toggle empty elements
            
        this.toggle(content);
    }
    

    // utilities
    $.fn.toggleDisplay = function(options) {
        return this.each(function() {
            $(this).removeClass(options.remove).addClass(options.add);
        });
    };
    
    
    // plugin definition
    $.fn.accordion = function (option) {
        return this.each(function () {
            var $this = $(this);

            // build main options before element iteration
            var options = $.extend(true, {}, $.fn.accordion.defaults, typeof option == 'object' && option);
            // element specific options (combine "data" set)
            var data = $this.data('accordion');
            if (data && data instanceof Accordion === false)
            {
                options = $.extend(true, options, data);
                data = null;
            }

            if (!data) $this.data('accordion', (data = new Accordion(this, options)));
            if (typeof option === 'string') data[option]();
        });
    };

    $.fn.accordion.Constructor = Accordion;

    // plugin defaults
    $.fn.accordion.defaults = {
        openedCssClass: 'opened',
        closedCssClass: 'closed'
    };
    
    // plugin data-api
    $(function () {
        $('[data-implement="accordion"]').each(function () {
            var $this = $(this);
            $this.accordion();
        });
    });

})(jQuery, window, document);