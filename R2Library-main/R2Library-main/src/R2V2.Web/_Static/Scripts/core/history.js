/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Browser History
    Utilizes jQuery BBQ plugin
   ========================================================================== */
R2V2.History = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.query = {};
        this.hash = {};

        // cache bound/proxy'd method calls
        this.f = {
            onHashChange: $.proxy(this.onHashChange, this),
            subscribe: $.proxy(this.subscribe, this),
            add: $.proxy(this.add, this),
            replace: $.proxy(this.replace, this),
            remove: $.proxy(this.remove, this),
            get: $.proxy(this.get, this)
        };

        // setup
        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Add).subscribe(this.f.add);
        $.PubSub(R2V2.PubSubMappings.History.Replace).subscribe(this.f.replace);
        $.PubSub(R2V2.PubSubMappings.History.Remove).subscribe(this.f.remove);
        $.PubSub(R2V2.PubSubMappings.History.Subscribe).subscribe(this.f.subscribe);
    },
    
    onHashChange: function (event)
    {
        var prevHash = this.hash;

        // query and hash "location" values
        this.query = R2V2.Utils.GetQuerystring(),
        this.hash = R2V2.Utils.GetUrlFragment();

        var isHashSame = _.isEqual(prevHash, this.hash);
        var isPageSame = (prevHash.page && this.hash.page && (prevHash.page !== '1') && (prevHash.page === this.hash.page));

        if (!isHashSame && isPageSame)
        {
            this.add({ page: '1' }); // resets to first page
            return;
        }

        // set history
        this.set({ query: this.query.q, hash: this.hash });
    },
    
    subscribe: function ()
    {
        R2V2.Elements.Window.on('hashchange', this.f.onHashChange).trigger('hashchange');
    },
    
    add: function (state, mergeMode)
    {
        mergeMode ? $.bbq.pushState(state, mergeMode) : $.bbq.pushState(state);
    },

    replace: function (params)
    {
        var state = '#' + R2V2.Utils.ToUrlParams(params);
        
        location.replace(state);
    },
    
    remove: function (state)
    {
        $.bbq.removeState(state);
    },
    
    get: function (key)
    {
        return $.bbq.getState(key);
    },
    
    set: function (params)
    {
        $.PubSub(R2V2.PubSubMappings.History.Set).publish(params);
    }
});

/* ==========================================================================
    Create OnDOMReady
    ========================================================================== */
; (function ($) {
    
    new R2V2.History();
    
})(jQuery);