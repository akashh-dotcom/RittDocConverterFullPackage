/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    ActionsMenu
   ========================================================================== */
R2V2.ActionsMenu = Class.extend({
    init: function ()
    {
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) return;

        // cache DOM elements
        this.$panels = this.$container.find('[data-panel]');

        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, { onClick: $.proxy(this.onClick, this) });

        // setup
        this.setup();
    },

    setup: function()
    {
        if (this.$panels.length <= 0) return;
        
        var types = { email: 'EmailPanel', marc: 'MarcPanel',  topics: 'TopicPanel', search: 'SearchPanel', shoppingcart: 'ShoppingCart', exportcitation: 'ExportCitationPanel', savequery: 'SaveQueryPanel', toc: 'TocPanel' };

        _.each(this.$panels, function (panel) {
            var $panel = $(panel),
                key = $panel.data('panel'),
                type = types[key] || 'Panel',
                config = $.extend({ }, this.config || { }, { $container: $panel, type: key });

            if (this.config.self[type] == null)
            {
                new R2V2.ActionsMenu[type](config);
            }
            else
            {
                new this.config.self[type](config);
            }
        }, this);
    }
});


/* ==========================================================================
    ActionsMenu Panel
    extends: R2V2.Panel
   ========================================================================== */
R2V2.ActionsMenu.Panel = R2V2.Panel.extend({
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.Menu.Opened).subscribe(this.f.onOpen);
        $.PubSub(R2V2.PubSubMappings.Menu.Closed).subscribe(this.f.onClose);
    },

    save: function (event)
    {
        event.preventDefault();

        // get form action and data
        var dataUrl = this.$form.attr('action'),
            formData = $.param(this.$container.data('params')) + '&' + this.$form.serialize();
            
        // execute service call
        $.executeService({ url: dataUrl, data: formData }).then(this.f.onSaveDataSuccess).fail(this.f.onSaveDataError);
    }
});


/* ==========================================================================
    ActionsMenu Email Panel
    extends: R2V2.ActionsMenu.Panel
   ========================================================================== */
R2V2.ActionsMenu.EmailPanel = R2V2.ActionsMenu.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) return;

        // call parent method
        this._super();
    },
    
    onFormSave: function (event)
    {
        event.preventDefault();
        
        // validate
        this.validator = this.$form.validate();
        if (this.validator.form() === false) return;

        this.save(event);
    },
    
    save: function (event)
    {
        // get form action and data
        var dataUrl = this.$form.attr('action'),
            formData = this.$form.serialize();

        // execute service call
        $.executeService({ url: dataUrl, data: formData, type: 'POST', contentType: R2V2.ServiceExecutionOptions.ContentType.DEFAULT }).then(this.f.onSaveDataSuccess).fail(this.f.onSaveDataError);
    }
});

/* ==========================================================================
    ActionsMenu Marc Panel
    extends: R2V2.ActionsMenu.Panel
   ========================================================================== */
R2V2.ActionsMenu.MarcPanel = R2V2.ActionsMenu.Panel.extend({
    init: function (config) {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) return;

        // call parent method
        this._super();
    }

});