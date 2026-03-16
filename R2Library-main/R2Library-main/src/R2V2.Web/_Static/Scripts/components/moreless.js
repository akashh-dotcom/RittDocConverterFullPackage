/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    More/Less Toggle
   ========================================================================== */
; (function ( $, window, document, undefined ) {
	
	"use strict";
	
	// class definition
	var MoreLess = function ( $el, options )
	{
		this.$el = $el;
	    this.options = options;
        if (this.$el == null) return;
	    
        // merge options
        this.options.moreConfig = { type: this.options.moreType, html: this.options.moreHtml };
	    this.options.lessConfig = { type: this.options.lessType, html: this.options.lessHtml };

	    this.init();
	};
	MoreLess.prototype = {
	    constructor: MoreLess,
	    
		init: function ()
		{
		    this.setup();
		    
		    if (this.numberOfElements < this.options.numberOfVisible) return;
		    
            this.render();
            this.attachEvents();
		},
	    
        setup: function ()
		{
            // get visible elements
            this.$elements = this.$el.find(this.options.elementSelector);
		    this.$elements = this.$elements.filter(':visible');
            this.numberOfElements = this.$elements.length;

            // "unwrap" elements if already done
            if (this.$toggledElements) {
                this.$toggledElements.unwrap();
            }
            
            // create $ object of elements that are toggled
            this.toggledElements = $.grep(this.$elements, $.proxy(function(n, i) { return i >= this.options.numberOfVisible; }, this));
            this.$toggledElements = $(this.toggledElements);
            this.toggledContainerId = _.uniqueId('moreless');
            this.$toggledElements.wrapAll('<div id="' + this.toggledContainerId  + '"></div>');
            this.$toggledContainer = $('#' + this.toggledContainerId);
            this.$toggledContainer.hide();
		},
	    
        render: function ()
        {
            // add "more/less" link
            this.$template = $('<p><a href="#' + this.options.moreConfig.type + '" class="trigger ' + this.options.moreConfig.type + '" role="button" aria-expanded="false" aria-controls="' + this.toggledContainerId + '">' + this.options.moreConfig.html + '</a></p>');
            this.$link = this.$template.find('a');
            this.link = this.$link[0];
            this.$el.after(this.$template);
        },
		
		attachEvents: function ()
		{
		    // bound/proxy'd method calls
            this.f = {onTriggerAction: $.proxy(this.onTriggerAction, this) };
		    
            // DOM events
			this.$template.on('click', this.f.onTriggerAction);
		},

        onTriggerAction: function ( event )
        {
            event.preventDefault();
            
            this.toggle(this.options[(this.link.hash.substr(1) == this.options.moreConfig.type) ? 'lessConfig' : 'moreConfig']);
        },
	    
        toggle: function ( config )
        {
            this.$link
                .attr({ 'href': '#' + config.type, 'class': 'trigger ' + config.type })
                .attr('aria-expanded', config.type !== this.options.moreConfig.type)
                .html(config.html);
            this.$toggledContainer.toggle();
        },
	    
        expand: function ()
        {
            this.toggle(this.options.lessConfig);
        },
	    
        collapse: function ()
        {
            this.toggle(this.options.moreConfig);
        },
	    
        reset: function ()
        {
            // this.setup();
            
            this.$template[(this.numberOfElements < this.options.numberOfVisible) ? 'hide' : 'show']();
        }
	};
	
	// plugin definition
	$.fn.moreless = function ( option ) {
		return this.each(function () {
			var $this = $(this);
		    
            // build main options before element iteration
		    var options = $.extend(true, {}, $.fn.moreless.defaults, typeof option == 'object' && option);
		    
            // get element via "selector"
            if (options.selector) $this = $this.find(options.selector);

			// element specific options (combine "data" set)
            var data = $this.data('moreless');
            if (data && data instanceof MoreLess === false)
            {
                options = $.extend(true, options, data);
                data = null;
            }
		    
            if (!data) $this.data('moreless', (data = new MoreLess($this, options)));
		    if (typeof option === 'string') data[option]();
		});
	};

    $.fn.moreless.Constructor = MoreLess;
	
	// plugin defaults
	$.fn.moreless.defaults = {
	    elementSelector: 'li',
        numberOfVisible: 5,
	    moreType: 'more',
        moreHtml: 'see more...',
	    lessType: 'less',
        lessHtml: 'see less...',
	    selector: false
	};
    
    // plugin data-api
    $(function() {
        $('[data-toggle="moreless"]').each(function () {
            var $this = $(this);
            $this.moreless();
        });
    });

})( jQuery, window, document );