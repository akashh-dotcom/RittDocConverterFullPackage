/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Base Form Panel (used by Modals and Action Menus)
   ========================================================================== */
R2V2.Panel = Class.extend({
    init: function (config)
    {
        if (this.$form || this.$container) {
            // cache DOM elements
            this.$form = this.$form || this.$container.find('form');

            // cache bound/proxy'd method calls
            this.f = _.extend(this.f || {}, {
                onShow: $.proxy(this.onShow, this),
                onOpen: $.proxy(this.onOpen, this),
                onClose: $.proxy(this.onClose, this),
                onFormSave: $.proxy(this.onFormSave, this),
                onFormCancel: $.proxy(this.onFormCancel, this),
                onSaveDataSuccess: $.proxy(this.onSaveDataSuccess, this),
                onSaveDataError: $.proxy(this.onSaveDataError, this)
            });

            // setup
            this.attachEvents();
        }
    },
    
    attachInstanceEvents: function ()
    {
        // form submit/reset events
        this.$form.on({ 'submit.panel': this.f.onFormSave, 'reset.panel': this.f.onFormCancel });

        // enable keyboard form "cancel" for all form fields
        this.$form.find(':input:visible:enabled').on(R2V2.KeyboardMappings.Panel.Cancel, this.f.onFormCancel);
    },

    detachInstanceEvents: function ()
    {
        this.$form.off('.panel');
        this.$form.find(':input:visible:enabled').off();
    },
    
    onOpen: function (event)
    {
        if (this.isShown || this.$container.is(':visible') === false) return;

        this.isShown = true; // set flag
        this.attachInstanceEvents(); // attach form events
    },
    
    onClose: function (event, callback)
    {
        if (this.isShown == null || this.isShown === false) return;
        
        this.isShown = false; // set flag
        this.detachInstanceEvents(); // detach form events
        this.validator && this.validator.resetForm(); // reset validator
        callback && callback(); // callbacks
    },

    onFormSave: function (event)
    {
        // check for empty fields
        if (this.$form.find(':input:visible:enabled:not(button)[value=""]').length > 0)
        {
            event.preventDefault();
            return;
        }
        
        this.save(event);
    },

    onFormCancel: function (event)
    {
        event.stopImmediatePropagation(); // stop "keyup.esc" event from bubbling up;
        event.target.blur(); // blur field so keyboard shortcuts work after
        this.close();
    },

    onSaveDataSuccess: function (response)
    {
        this.closeAll();
    },

    onSaveDataError: function (response)
    {
        this.closeAll();
    },
    
    closeAll: function ()
    {
        this.close(this.$form.parents('[data-toggle="menu"]').last());
    },

    close: function (togglemenu)
    {
        var menu = (togglemenu || this.$form.closest('[data-toggle="menu"]')).data('togglemenu');
        if (menu)
        {
            menu.reset();
            return;
        }
        
        R2V2.Elements.Document.trigger($.Event('keyup', { which: 27 })); // simulate "document" click to close menu
        R2V2.Elements.Body.trigger($.Event('click.dismiss.togglemenu')); // simulate body click to close menu
    }
});