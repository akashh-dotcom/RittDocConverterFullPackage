/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Browse Controller
    extends: R2V2.Facet.Controller
   ========================================================================== */
R2V2.Browse = R2V2.Facet.Controller.extend({

    _defaultState: { 'include': 1, 'type': 'publications' },

    init: function (config) {

        this._defaultState = {
            'include': config.include || this._defaultState.include,
            'type': config.type || this._defaultState.type
        };

        // call parent
        this._super(config);
    },

    getData: function ()
    {
        var state = {
            type: this.hash['type'] || this._defaultState['type'],
            id: this.hash['discipline'] || this.hash['author'] || this.hash['publisher'] || '',
            include: this.hash['include'] || this._defaultState['include'],
            tocAvailable: this.hash['toc-available'] || false,
            practiceArea: this.hash['practice-area'] || '',
            disciplineId: this.hash['disciplineId'] || '',
            publisherId: this.hash['publisherId'] || '',
            sortBy: this.hash['sort-by'] || 'title',
            alpha: this.hash['alpha'] || ''
        };

        // clear previous/state exceptions (discipline, author, or publisher IDs)
        if (this.hash['discipline'] !== '' && this.hash['discipline'] != null && state.type !== 'disciplines')
        {
            $.PubSub(R2V2.PubSubMappings.History.Remove).publish('discipline');
            return null;
        }

        if (this.hash['author'] !== '' && this.hash['author'] != null && state.type !== 'authors')
        {
            $.PubSub(R2V2.PubSubMappings.History.Remove).publish('author');
            return null;
        }
        
        if (this.hash['publisher'] !== '' && this.hash['publisher'] != null && state.type !== 'publishers')
        {
            $.PubSub(R2V2.PubSubMappings.History.Remove).publish('publisher');
            return null;
        }

        if (state.type != "collections") {
            //disciplineId
            $('#disciplineId').parent().hide();
            $('#disciplineId').hide();
            $('#publisherId').parent().hide();
            $('#publisherId').hide();
        } else {
            $('#disciplineId').parent().show();
            $('#disciplineId').show();
            $('#publisherId').parent().show();
            $('#publisherId').show();
        }

        return $.param(state);
    }
});


/* ==========================================================================
    Browse Results
    extends: R2V2.Facet.LoadMoreResults
   ========================================================================== */
R2V2.Browse.Results = R2V2.Facet.Results.extend({ });


/* ==========================================================================
    Browse Filters
    extends: R2V2.Facet.Filters
   ========================================================================== */
R2V2.Browse.Filters = R2V2.Facet.Filters.extend({ });


/* ==========================================================================
    Browse History
    extends: R2V2.Facet.History
   ========================================================================== */
R2V2.Browse.History = R2V2.Facet.History.extend({

    _template: _.template('<h2>Current Filters</h2><ul>{{ _.each(history, function(props) { if (props === false) return; }}<li><a href="#remove" data-group-code="{{= props.GroupCode }}">[ <em>X</em> ]</a><p><strong>{{= props.GroupName }}</strong>: {{= props.FilterName }}</p></li>{{ }); }}</ul>{{ if (!(history.length === 1 && history[0].IsStatic)) { }}<p><a href="#clear">Clear Filters</a></p>{{ } }}'),
    _exclusions: ['include', 'type', 'alpha'], // list of filter and option group codes to exclude from history display

    // constructor in base class
    
    onHistorySet: function (event)
    {
        // only need "hash" data (query data is unavailable on browse)
        this._super(event.hash);
    },
    
    onFilteringSuccess: function (event)
    {
        // combine "filter" and "sort" group data (deep copy)
        this._super($.extend(true, event.FilterGroups, event.SortGroups));
    }
});


/* ==========================================================================
    Browse Sort
   ========================================================================== */
R2V2.Browse.Sort = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length === 0) return;
        
        // cache elements
        this.id = this.$container.attr('id');
        this.$title = this.$container.prev('h2');
        this.$group = this.$title.add(this.$container);
        this.$publisherSortContainer = this.$group.find('a[href="#publisher"]').parent('li');
        
        // cache bound/proxy'd method calls
        this.f = {
             onHistorySet: $.proxy(this.onHistorySet, this),
             onFilteringSuccess: $.proxy(this.onFilteringSuccess, this)
        };

        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
        $.PubSub(R2V2.PubSubMappings.Filtering.Success).subscribe(this.f.onFilteringSuccess);
    },
    
    onHistorySet: function (event)
    {
        this.$publisherSortContainer[event.hash && event.hash.type === 'publishers' ? 'hide': 'show']();
    },
    
    onFilteringSuccess: function (event)
    {
        this[event.SortGroups[this.id] == null ? 'hide' : 'show']();
    },
    
    show: function ()
    {
        this.$group.show();
    },
    
    hide: function ()
    {
        this.$group.hide();
    }
});