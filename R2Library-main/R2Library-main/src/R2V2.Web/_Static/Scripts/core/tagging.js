/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Tagging
   ========================================================================== */
R2V2.Tagging= Class.extend({
    
    _dataType: 'track',
    
    // constructor
    init: function (config)
    {
        // config
        this.config = config || { };
        this.dataType = this.config.dataType || this._dataType;
        
        // cache bound/proxy'd method calls
        this.f = {
            onTagBroadcast: $.proxy(this.onTagBroadcast, this),
            onTrackLinkClick: $.proxy(this.onTrackLinkClick, this)
        };

        // setup
        this.attachEvents();
    },

    attachEvents: function ()
    {
        // subscriptions
        R2V2.Elements.Document.on(R2V2.PubSubMappings.Tag, this.f.onTagBroadcast);
        
        // DOM events
        R2V2.Elements.Body.on('click', 'a[data-' + this.dataType + ']', this.f.onTrackLinkClick);
    },
    
    onTagBroadcast: function (event)
    {
        this.tag(event);
    },
    
    onTrackLinkClick: function (event)
    {
        var target = R2V2.Utils.GetEventTarget(event, 'a');
        
        var data = target.$.data(this.dataType);
        if (data == null) return;
        
        this.tag(data);
    },
    
    tag: function (data)
    {
        if (typeof _gaq == 'undefined') return;

       var gaData = ['_trackEvent', data.category || R2V2.Config.Tracking.Category, data.action || R2V2.Config.Tracking.Action];
        data.opt_label && gaData.push(data.opt_label);
        data.opt_value && gaData.push(data.opt_value);
        
        _gaq.push(gaData);
    }
});


/* ==========================================================================
    Create OnDOMReady
    ========================================================================== */
; (function ($) {
    
    new R2V2.Tagging();
    
})(jQuery);