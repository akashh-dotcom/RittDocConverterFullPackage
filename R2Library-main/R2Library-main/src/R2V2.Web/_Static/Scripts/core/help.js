/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Help
    ========================================================================== */
R2V2.Help = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.dataUrl = this.config.dataUrl || null;
        
        // error check
        if (this.dataUrl == null) return;

        // cache bound/proxy'd method calls
        this.f = {
            onKeyboardEvent: $.proxy(this.onKeyboardEvent, this),
            onGetDataSuccess: $.proxy(this.onGetDataSuccess, this),
            onGetDataError: $.proxy(this.onGetDataError, this)
        };

        this.attachEvents();
    },

    attachEvents: function ()
    {
        // DOM events
        R2V2.Elements.Document.on(R2V2.KeyboardMappings.Help.Open, this.f.onKeyboardEvent);
    },

    onKeyboardEvent: function (event)
    {
        event.preventDefault();
        
        // check if modal is currently open
        if (R2V2.Elements.Body.hasClass('modal-open')) return;
        
        // show modal if it already has been shown, otherwise get content
        this[this.$content ? 'show' : 'getData']();
    },
    
    onGetDataSuccess: function (response)
    {
        this.$content = $(response);
        R2V2.Elements.Body.append(this.$content);
        
        this.show();
    },
    
    onGetDataError: function(response) {  },

    getData: function ()
    {
        $.ajax(this.dataUrl).done(this.f.onGetDataSuccess).fail(this.f.onGetDataError);
    },

    show: function ()
    {
        this.$content.modal('show');
    }
});