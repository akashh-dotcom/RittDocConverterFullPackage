/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Toggle Menu
   ========================================================================== */
; (function ($, window, document, undefined) {

    "use strict";

    // class definition
    var ToggleMenu = function (element, options) {
        // cache $ elements
        this.$element = $(element).delegate('[data-dismiss="menu"]', 'click.togglemenu', $.proxy(this.reset, this));
        
        // options
        this.options = options;
        
        // constructor call
        this.init();
    };
    ToggleMenu.prototype = {
        constructor: ToggleMenu,

        init: function ()
        {
            // cache DOM elements
            this.triggers = this.options.trigger ? $(this.options.trigger) : this.$element.find('> li > a'); // default "element" is list so default trigger is immediate list item link
            
            // cache bound/proxy'd method calls
            this.f = {
                onTriggerClick: $.proxy(this.onTriggerClick, this),
                onModalShow: $.proxy(this.onModalShow, this)
            };
            
            this.attachEvents();
        },
        
        attachEvents: function ()
        {
            this.triggers.on('click', this.f.onTriggerClick);
            $('.modal').on('show', this.f.onModalShow);
        },
        
        onTriggerClick: function (event)
        {
            // get event trigger
            var trigger = R2V2.Utils.GetEventTarget(event, 'a');

            // hide toggle menu if "trigger" does not have sub menu (print, export)
            if (trigger.DOM.hash == null || $(trigger.DOM.hash).length <= 0)
            {
                R2V2.Elements.Body.trigger($.Event('click.dismiss.togglemenu'));
                return;
            }
            
            event.preventDefault();

            // get event target
            this.$target = this.options.target ? $(this.options.trigger) : R2V2.Utils.GetEventTarget(event, 'li')['$'];
            
            if (this.$menu)
            {
                this.close();
                
                if (this.$menu[0] === this.$target[0])
                {
                    this.$menu = null;
                    return;
                }
            }

            this.$trigger = trigger.$;

            this.open();
        },
    
        onModalShow: function(event)
        {
            this.reset();
        },
        
        open: function ()
        {
            // add CSS class to new "target"
            this.$menu = this.$target.addClass(this.options.openCssClass);
            
            // add CSS class to container
            this.$element.addClass(this.options.menuToggledCssClass);
            
            // publish
            $.PubSub(R2V2.PubSubMappings.Menu.Opened).publish({ menu: this.$menu });
            
            // handle events
            attachInstanceEvents.call(this);

            // accessibility notification
            this.$trigger.attr('aria-expanded', true);
        },
        
        close: function ()
        {
            // remove CSS class from "target"
            this.$menu.removeClass(this.options.openCssClass);
            
            // remove CSS class from container
            this.$element.removeClass(this.options.menuToggledCssClass);

            // reset any nested menus (for nested "toggle menus")
            resetNestedMenu.call(this);
            
            // publish
            $.PubSub(R2V2.PubSubMappings.Menu.Closed).publish({ menu: this.$menu });

            // handle events
            detachInstanceEvents.call(this);

            // accessibility notification
            this.$trigger.attr('aria-expanded', false);
        },
        
        reset: function (event)
        {
            event && event.preventDefault();
            
            if (this.$menu == null) return;
            
            this.close();
            this.$menu = null;
        }
    };


    // private methods
    function attachInstanceEvents()
    {
        // only "parent" menus handle events
        if (this.$element.parents('.' + this.options.menuToggledCssClass).length > 0) return;

        var that = this;
        
        // bind simulated click event to "esc" key
        R2V2.Elements.Document.on(R2V2.KeyboardMappings.Menu.Close, function (event) {
            // reset any "toggled" nested menus
            if (that.$element.find('.' + that.options.menuToggledCssClass).length)
            {
                resetNestedMenu.call(that);
                return;
            }
            
            // close this menu
            simulateClickEvent.call(that);
        });
        
        // bind simulated click event to click events outside menu
        R2V2.Elements.Body.on('click.dismiss.togglemenu', function (event) {
            // check if "click" is outside menu
            if ($(event.target).closest(that.$element).length > 0) return;

            // close this menu
            simulateClickEvent.call(that);
        });
    }
        
    function detachInstanceEvents()
    {
        // only "parent" menus handle events
        if (this.$element.parents('.' + this.options.menuToggledCssClass).length > 0) return;
        
        // unbind "esc" key logic
        R2V2.Elements.Document.off(R2V2.KeyboardMappings.Menu.Close);
            
        // unbind body "click" logic
        R2V2.Elements.Body.off('click.dismiss.togglemenu');
    }
    
    function resetNestedMenu()
    {
        this.$menu.find('[data-toggle="menu"]').togglemenu('reset');
    }

    function simulateClickEvent()
    {
        this.reset();
    }
    

    // plugin definition
    $.fn.togglemenu = function (option) {
        return this.each(function () {
            var $this = $(this);

            // build main options before element iteration
            var options = $.extend(true, {}, $.fn.togglemenu.defaults, typeof option == 'object' && option);
            // element specific options (combine "data" set)
            var data = $this.data('togglemenu');
            if (data && data instanceof ToggleMenu === false) {
                options = $.extend(true, options, data);
                data = null;
            }

            if (!data) $this.data('togglemenu', (data = new ToggleMenu(this, options)));
            if (typeof option === 'string') data[option]();
        });
    };

    $.fn.togglemenu.Constructor = ToggleMenu;

    // plugin defaults
    $.fn.togglemenu.defaults = {
        openCssClass: 'open',
        menuToggledCssClass: 'toggled'
    };

    // plugin data-api
    $(function () {
        $('[data-toggle="menu"]').each(function () {
            var $this = $(this),
                $target = $($this.attr('data-target')),
                option = $.extend({ }, $target.data(), $this.data());
            
            $this.togglemenu(option);
        });
    });

})(jQuery, window, document);