/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Alpha Index
    extends: R2V2.Facet.Controller
   ========================================================================== */
R2V2.AlphaIndex = R2V2.Facet.Controller.extend({

    _defaultState: { 'show': 'all' },

    getData: function ()
    {
        var state = {
            show: this.hash['show'] || this._defaultState['show'],
            practiceArea: this.hash['practice-area'] || '',
            disciplines: this.hash['disciplines'] || '',
            alpha: this.hash['alpha'] || ''
        };

        return $.param(state);
    }
});


/* ==========================================================================
    AlphaIndex Results
    extends: R2V2.Facet.Results
   ========================================================================== */
R2V2.AlphaIndex.Results = R2V2.Facet.Results.extend({ });


/* ==========================================================================
    AlphaIndex Filters
    extends: R2V2.Facet.Filters
   ========================================================================== */
R2V2.AlphaIndex.Filters = R2V2.Facet.Filters.extend({ });


/* ==========================================================================
    AlphaIndex History
    extends: R2V2.Facet.History
   ========================================================================== */
R2V2.AlphaIndex.History = R2V2.Facet.History.extend({

    _template: _.template('<h2>Current Filters</h2><ul>{{ _.each(history, function(props) { if (props === false) return; }}<li><a href="#remove" data-group-code="{{= props.GroupCode }}">[ <em>X</em> ]</a><p><strong>{{= props.GroupName }}</strong>: {{= props.FilterName }}</p></li>{{ }); }}</ul>{{ if (!(history.length === 1 && history[0].IsStatic)) { }}<p><a href="#clear">Clear Filters</a></p>{{ } }}'),
    _exclusions: ['show', 'alpha'], // list of filter and option group codes to exclude from history display
    
    // constructor in base class

    onHistorySet: function (response)
    {
        // only need "hash" data (query data is unavailable on alpha)
        this._super(response.hash);
    },

    onFilteringSuccess: function (response)
    {
        // combine "filter" and "sort" group data (deep copy)
        this._super(response.FilterGroups);
    }
});
