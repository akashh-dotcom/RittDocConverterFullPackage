/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

/* ==========================================================================
    Toggle Selections (select/unselect all)
    ========================================================================== */
; (function ($, window, document, undefined) {
    
    "use strict"; // jshint ;_;
    
    /* SELECTIONS CLASS DEFINITION
     * ====================== */
    var Selections = function(element, options) {
        this.options = options;

        this.$element = $(element);
        this.$fields = this.$element.find('input');

        this.$element.on('click', this.$fields, $.proxy(function(e) { configureTrigger.call(this, e.target); }, this));

        this.toggle();
    };
    Selections.prototype = {
        constructor: Selections,
        
        toggle: function ()
        {
            return this[!this.isSelected ? 'select' : 'deselect']();
        },
            
        select: function ()
        {
            var e = $.Event('selected');

            this.$element.trigger(e);

            if (this.isSelected || e.isDefaultPrevented()) return;

            this.isSelected = true;

            updateFields.call(this, true);
        },
            
        deselect: function ( e )
        {
            e && e.preventDefault();

            e = $.Event('deselected');

            this.$element.trigger(e);
            
            if (!this.isSelected || e.isDefaultPrevented()) return;

            this.isSelected = false;
            
            updateFields.call(this, false);
        }
    };
    

    /* SELECTIONS PRIVATE METHODS
     * ===================== */
    function updateFields(status)
    {
        this.$fields.each(function() { $(this).attr({ 'checked': status }); });
    }
    
    function configureTrigger(el)
    {
        if (el.checked === true || this.isSelected === false) return;
        
        this.options.$trigger.attr({ 'checked': false });
        this.isSelected = false;
    }
    

    /* SELECTIONS PLUGIN DEFINITION
    * ======================= */
    $.fn.selections = function(option) {
        return this.each(function() {
            var $this = $(this),
                data = $this.data('selections'),
                options = $.extend({ }, $this.data(), typeof option == 'object' && option);
            
            if (!data) $this.data('selections', (data = new Selections(this, options)));
            
            if (typeof option == 'string') data[option]();
        });
    };

    $.fn.selections.Constructor = Selections;


    /* SELECTIONS DATA-API
     * ============== */
    $(function() {
        R2V2.Elements.Body.on('click.selections.data-api', '[data-toggle="selections"]', function(e) {
            var $this = $(this),
                $target = $($this.attr('data-target')),
                option = $target.data('selections') ? 'toggle' : $.extend({ }, $target.data(), $this.data(), { $trigger: $this });
            
            $target.selections(option);
        });
    });
})(jQuery, window, document);