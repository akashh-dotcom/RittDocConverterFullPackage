/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };
R2V2.Facet = { };

/* ==========================================================================
    Facet Controller
   ========================================================================== */
R2V2.Facet.Controller = Class.extend({
    
    _defaultState: { },

    init: function (config)
    {
        this.config = config || {};
        this.dataUrl = this.config.dataUrl;

        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, {
            onHistorySet: $.proxy(this.onHistorySet, this),
            onHistorySetSuccess: $.proxy(this.onHistorySetSuccess, this),
            onHistorySetError: $.proxy(this.onHistorySetError, this),
            onResultsGet: $.proxy(this.onResultsGet, this),
            onResultsGetSuccess: $.proxy(this.onResultsGetSuccess, this)
        });

        // setup
        this.checkDefaultState({ silent: true });
        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
        $.PubSub(R2V2.PubSubMappings.Results.Get).subscribe(this.f.onResultsGet);
        $.PubSub(R2V2.PubSubMappings.History.Subscribe).publish();
    },
    
    onHistorySet: function (event)
    {
        this.query = event.query;
        this.hash = event.hash;

        var foundIssuesWithDefaultState = this.checkDefaultState({ hash: this.hash, silent: false });
        if (foundIssuesWithDefaultState)
        {
            return;
        }

        this.data = this.getData();
        if (this.data == null)
        {
            return;
        }
        
        // call service
        $.executeService({ url: this.dataUrl, data: this.data })
            .then(this.f.onHistorySetSuccess, this.f.onHistorySetError);
    },
    
    onHistorySetSuccess: function (event)
    {
        // publish
        $.PubSub(R2V2.PubSubMappings.Filtering.Success).publish(event);
    },
    
    onHistorySetError: function (event)
    {
        // publish
        $.PubSub(R2V2.PubSubMappings.Filtering.Error).publish(event);
    },
    
    onResultsGet: function (event)
    {
        var data = this.data + '&' + $.param(event);
        
        // call service
        $.executeService({ url: this.dataUrl, data: data })
            .then(this.f.onResultsGetSuccess);
    },
    
    onResultsGetSuccess: function (event)
    {
        // publish
        $.PubSub(R2V2.PubSubMappings.Results.Set).publish(event);
    },

    addToHistory: function (params, silent)
    {
        $.PubSub(R2V2.PubSubMappings.History[silent ? 'Replace' : 'Add']).publish(params);
    },

    checkDefaultState: function (config)
    {
        config = config || {};

        var silent = (config.silent === true);
        this.hash = (config.hash || R2V2.Utils.GetUrlFragment());
        
        if (this.hasEmptySet(silent))
        {
            return true;
        }

        if (this.hasMissingFilters(silent))
        {
            return true;
        }

        return false;
    },

    hasEmptySet: function (silent)
    {
        if (_.isEmpty(this.hash) && _.isEmpty(this._defaultState) === false)
        {
            this.addToHistory(this._defaultState, silent);

            return true;
        }

        return false;
    },

    hasMissingFilters: function (silent)
    {
        var missingFilters = _.difference(_.keys(this._defaultState), _.keys(this.hash));
        
        if (missingFilters.length)
        {
            var state = { };
            _.each(missingFilters, function (filter) { state[filter] = this._defaultState[filter]; }, this);

            this.addToHistory(state, silent);
            
            return true;
        }

        return false;
    }
});


/* ==========================================================================
    Facet Results
   ========================================================================== */
R2V2.Facet.Results = Class.extend({
    
    _loadingText: 'Loading...',
    _hiddenCssClassName: 'hidden',

    init: function (config) {
        // config settings
        this.config = config || { };
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length === 0) return;
        
        // load more settings
        this.page = this.config.page || null;
        this.pageSize = this.config.pageSize || null;
        this.resultsSelector = this.config.resultsSelector;
        // load more flags
        this.hasMore = this.page != null && this.pageSize != null;
        // load more cache
        this.$more = $();
        this.$loadMore = $('<p class="load-more"><a href="#load-more" class="btn"><span>Load More</span></a> <a href="#load-all">Load All</a></p>');
        this.$loadAll = this.$loadMore.find('a[href="#load-all"]');
        
        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, {
            onHistorySet: $.proxy(this.onHistorySet, this),
            onFilteringSuccess: $.proxy(this.onFilteringSuccess, this),
            onFilteringError: $.proxy(this.onFilteringError, this),
            onResultsSet: $.proxy(this.onResultsSet, this),
            onLoadMoreClick: $.proxy(this.onLoadMoreClick, this)
        });
        
        // setup
        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
        $.PubSub(R2V2.PubSubMappings.Filtering.Success).subscribe(this.f.onFilteringSuccess);
        $.PubSub(R2V2.PubSubMappings.Filtering.Error).subscribe(this.f.onFilteringError);
        $.PubSub(R2V2.PubSubMappings.Results.Set).subscribe(this.f.onResultsSet);
        
        // DOM
        this.$container.on('click', '.load-more', this.f.onLoadMoreClick);
    },
    
    onHistorySet: function (event)
    {
        this.displayLoadAll = (event.hash.alpha && event.hash.alpha !== "All") || false;
        
        this.renderLoading();
    },

    onFilteringSuccess: function (event)
    {
        this.data = event;
        
        // render
        this.renderContent();
    },
    
    onFilteringError: function (event)
    {
        this.data = event;
        
        // render
        this.renderError();
    },
    
    onResultsSet: function (event)
    {
        this.data = event.HtmlSnippets.results;

        if (!this.data) {
            console.warn('No results found');
            return;
        }
        
        // render
        this.renderMore();
    },
    
    onLoadMoreClick: function (event)
    {
        event.preventDefault();
        var target = R2V2.Utils.GetEventTarget(event, 'a'); // get DOM and $ targets
        if (target.$.is('a') === false) return;
        if (target.DOM.hash.substr(1) === 'load-all')
        {
            this.showAll = true;
            if (this.$more.length === this.pageSize) this.loadMore();
            this.hideLoadMoreControls();
            this.showMore();
            return;
        }
        // load more
        this[(this.$more.length < this.pageSize) ? 'hideLoadMoreControls' : 'loadMore']();
        // show more
        this.showMore();
    },
    
    renderLoading: function ()
    {
        this.$container.empty().append('<div class="loading">' + this._loadingText + '</div><div class="loading-backdrop"></div>');
    },
    
    renderContent: function ()
    {
        // update content (either HtmlSnippets.results or "data" itself)
        this.$container.empty().append(this.data.HtmlSnippets ? this.data.HtmlSnippets.results : this.data);
        
        // implement tooltips and fixed menus
        this.$container.tips().fixed({ selector: '[data-implement="fixed"]' });

        // configure "load more" button
        this.configureLoadMoreButton();
    },
    
    renderMore: function ()
    {
        if (this.data == null || this.data === '')
        {
            this.hideLoadMoreControls();
            return;
        }
        
        if (this.showAll)
        {
            // insert "all" into DOM
            this.$resultsContainer.html(this.data);
            return;
        }

        // get next set of cached results
        this.$more = $(this.data).filter(this.resultsSelector).addClass(this._hiddenCssClassName);
        
        // insert "more" into DOM
        this.$resultsContainer = this.$loadMore.prev();
        this.$resultsContainer.append(this.$more);
    },
    
    renderError: function ()
    {
        // update content
        this.$container.empty().append(R2V2.Messages.FilteringError);
    },
    
    showMore: function ()
    {
        this.$more.removeClass(this._hiddenCssClassName);
    },
    
    loadMore: function () {
        $.PubSub(R2V2.PubSubMappings.Results.Get).publish({ page: ++this.page, pageSize: this.pageSize, showAll: this.showAll });
    },
    
    configureLoadMoreButton: function () {
        // only continue if "has more"
        if (this.hasMore === false) return;
        // only continue if current display equals page size
        var len = $(this.$container).find(this.resultsSelector).length;
        if (len !== this.pageSize) return;
        
        // show button
        this.showLoadMoreControls();
        
        // reset page
        this.page = 1;
        this.showAll = false;
        // load initial
        this.loadMore();
    },
    
    showLoadMoreControls: function ()
    {
        this.$loadAll[this.displayLoadAll ? 'show' : 'hide']();
        this.$container.append(this.$loadMore);
    },
    
    hideLoadMoreControls: function ()
    {
        this.$loadMore.remove();
    }
});


/* ==========================================================================
    Facet Filters
   ========================================================================== */
R2V2.Facet.Filters = Class.extend({
    
    _cache: { },
    _selector: '[data-history="bookmark"]',
    
    init: function ()
    {
        // cache DOM elements
        this.$groups = $(this._selector);

        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, { onDomEvent: $.proxy(this.onDomEvent, this) });

        this.setup();
        this.attachEvents();
    },
    
    setup: function()
    {
        _.each(this.$groups, this.create, this);
    },
    
    attachEvents: function()
    {
        var linkSelector = this._selector + ' a[href*="#"]';
        var checkboxSelector = 'input:checkbox' + this._selector;
        var selectSelector = 'select' + this._selector;

        // DOM events
        R2V2.Elements.Body.on('click', linkSelector, { targetSelector: 'a' }, this.f.onDomEvent);
        R2V2.Elements.Body.on('change', checkboxSelector, this.f.onDomEvent);
        R2V2.Elements.Body.on('change', selectSelector, this.f.onDomEvent);
    },
    
    onDomEvent: function (event)
    {
        event.preventDefault();
        
        var target = R2V2.Utils.GetEventTarget(event, event.data ? event.data.targetSelector : null); // get DOM and $ targets
        if (target.$.hasClass('trigger')) return; // "trigger" is used with UI enhancement plugins

        var id = target.DOM.id || target.$.closest('[data-history="bookmark"]').attr('id');
        if (id == null) return;

        var group = this._cache[id];
        if (group == null)
        {
            this.create($('#' + id).get(0));
            group = this._cache[id];
        }
        group.set(target.$);
    },
    
    create: function (group)
    {
        var $group = $(group), // $ element
            id = group.id || null,
            groupId = $group.data('bookmark-group') || null, // for "checkbox group" filters
            type = 'LinkFilter', // default group type
            params = { $container: $group, id: id, groupId: groupId },
            isSelect = $group.is('select'),
            isCheckbox = $group.is('input:checkbox');

        if (isSelect) type = 'SelectFilter';
        if (isCheckbox) type = groupId ? 'CheckboxGroupFilter' : 'CheckboxFilter';
	    if (type == 'CheckboxGroupFilter') params.defaultValue = 1;

        this._cache[id] = new R2V2.Facet[type](params);
    }
});


/* ==========================================================================
    Facet Filter
   ========================================================================== */
R2V2.Facet.Filter = Class.extend({
    
    _template: _.template('({{= Count }})'),
    
    init: function (config)
    {
        // config settings
        this.config = config || { };
        this.$container = this.config.$container;
        
        if (this.$container == null || this.$container.length === 0) return;
        
        // client ID
        this.id = this.config.id || null;
        // group ID for "bookmark groups"
        this.groupId = this.config.groupId || null;

	    this.defaultValue = this.config.defaultValue || null;

        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, {
            onHistorySet: $.proxy(this.onHistorySet, this),
            onFilteringSuccess: $.proxy(this.onFilteringSuccess, this)
        });

        // setup
        this.setup();
        this.attachEvents();
    },

    setup: function () // default
    {
        console.log('config', this.config)
        // filter value
        this.value = this.$container.val() || null;
        
        // flags
        this.hasCounts = this.$container.data('has-counts') || false;
        if (this.hasCounts === false) return;

        // counts
        this.$countDisplay = this.$container.parent().find('.result-count') || null;
    },

    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
        $.PubSub(R2V2.PubSubMappings.Filtering.Success).subscribe(this.f.onFilteringSuccess);
    },
    
    onHistorySet: function (event)
    {
        this.render(event.hash[this.groupId || this.id]);
    },
    
    onFilteringSuccess: function(event)
    {
        if (this.hasCounts === false) return;
        
        event = event.FilterGroups[this.groupId || this.id];
        event && this.renderCounts(event);
    },
    
    renderCounts: function (data) // default
    {
        this.$countDisplay.html(this._template(data));
    }
});


/* ==========================================================================
    Facet Link Filter
   ========================================================================== */
R2V2.Facet.LinkFilter = R2V2.Facet.Filter.extend({
    setup: function () // override parent
    {
        // create internal lookup
        this.lookup = { };
        _.each(this.$container.find('a'), function(link) {
            // elements
            var code = link.hash.substr(1),
                $link = $(link),
                $countDisplay = $link.find('.result-count');
            
            // create 
            this[code] = { link: link, $link: $link, $countDisplay: ($countDisplay.length) ? $countDisplay : null, $parent: $link.parent('li') };
        }, this.lookup);
        
        // flags
        this.hasCounts = this.$container.data('has-counts') || false;
    },
    
    set: function ($target)
    {
        var value = $target.get(0).hash.replace( /^#/ , ''); // get the url from the link's href attribute, stripping any leading #
        
        if (value === '')
        {
            $.PubSub(R2V2.PubSubMappings.History.Remove).publish(this.id);
            return;
        }
        
        // build state
        var state = { };
        state[this.id] = value;

        // publish
        $.PubSub(R2V2.PubSubMappings.History.Add).publish(state);
    },

    render: function (bookmark)
    {
        // clear any previous "selected"
        if (this.$selected) {
            this.$selected.$parent.removeClass('selected');
            this.$selected.$link.removeAttr('tabindex');
        }

        // check if item is bookmarked
        if (bookmark == null || this.lookup[bookmark] == null) return;
        
        // "select" current
        this.$selected = this.lookup[bookmark];

        // add "selected" state
        this.$selected.$parent.addClass('selected');
        this.$selected.$link.attr('tabindex', -1);
    },
    
    renderCounts: function (data) // override parent
    {
        // check if all counts are 0
        if (_.any(_.pluck(data.Filters, "Count"), function(count) { return parseInt(count) > 0; }) === false) return;
        console.log('renderCounts - data', data)
        // update links UI (counts, display)
        var filterData;
        _.each(this.lookup, function(props, code) {
            filterData = data.Filters[code];
            if (data.Code == "disciplineId" || data.Code == "publisherId") {
                console.log("filterData", filterData);
                if (filterData == null) {
                    props.$parent.hide();
                    return;
                }
                
            }
            // check if data is valid
            if (filterData == null || filterData.Count == null) return;
            
            if ((filterData.Count == 0 || filterData.Count == ''))
            {
                // hide link
                props.$parent.hide();
                return;
            }

            // update count if element has count display
            props.$countDisplay && props.$countDisplay.html(this._template(filterData));
            
            // show link
            props.$parent.show();
        }, this);
        
        // rerun "moreless" plugins
        this.$container.data('moreless') && this.$container.moreless('reset');
    }
});


/* ==========================================================================
    Facet Select Filter
   ========================================================================== */
R2V2.Facet.SelectFilter = R2V2.Facet.Filter.extend({
    set: function ()
    {
        // build state
        var state = { };
        state[this.id] = this.$container.val();
        
        // publish
        $.PubSub(R2V2.PubSubMappings.History.Remove).publish('page');
        $.PubSub(R2V2.PubSubMappings.History.Add).publish(state, 0);
    },

    render: function (bookmark)
    {
        // clear any previous
        this.$container.val();

        // check if item is bookmarked
        if (bookmark == null) return;
        
        // "select" current
        this.$container.val(bookmark);
    }
});


/* ==========================================================================
    Facet Checkbox Filter
   ========================================================================== */
R2V2.Facet.CheckboxFilter = R2V2.Facet.Filter.extend({
    set: function ()
    {
        var value = this.$container.attr('checked') ? this.$container.val() : 0;

        if (value == false) // "==" because 0 means false as well
        {
            $.PubSub(R2V2.PubSubMappings.History.Remove).publish(this.id);
            return;
        }
        
        // build state
        var state = { };
        state[this.id] = value;
        
        // publish
        $.PubSub(R2V2.PubSubMappings.History.Remove).publish('page');
        $.PubSub(R2V2.PubSubMappings.History.Add).publish(state, 0);
    },
    
    render: function (bookmark)
    {
        // clear any previous
        this.$container.attr('checked', false);

        // check if item is bookmarked
        if (bookmark == null) return;
        
        // "select" current
        this.$container.attr('checked', true);
    }
});


/* ==========================================================================
    Facet Checkbox Group Filter
   ========================================================================== */
R2V2.Facet.CheckboxGroupFilter = R2V2.Facet.Filter.extend({		
    set: function ()
    {
        var $selected = R2V2.Elements.Body.find('[data-bookmark-group=' + this.groupId + ']').filter(':checked');
        
        if ($selected.length === 0)
        {
            $.PubSub(R2V2.PubSubMappings.History.Remove).publish(this.groupId);
            return;
        }

        // calculate value
        var values = _.invoke($selected, function() { return parseInt(this.value); }),
            value = _.reduce(values, function(memo, num) { return memo | num; }, 0);

        // build state
        var state = { };
        state[this.groupId] = value;
        
        // publish
        $.PubSub(R2V2.PubSubMappings.History.Add).publish(state, 0);
    },
    
    render: function (bookmark)
    {
        // clear any previous
        this.$container.attr({ 'checked': false, 'disabled': false });

	    bookmark = bookmark || this.defaultValue;
        
        // check if group is bookmarked
        if (bookmark == null) return;

        // "select" current
        if (bookmark == this.value)
        {
            this.$container.attr({ 'checked': true, 'disabled': true });
            return;
        }

        if (bookmark == (bookmark | this.value))
        {
            this.$container.attr({ 'checked': true, 'disabled': false });
            return;
        }
    },
    
    renderCounts: function (data) // override parent
    {
        data = data.Filters[this.id];
        
        if (data == null || data.Count === 0 || data.Count === '') return;
        this.$countDisplay.html(this._template(data));
    }
});


/* ==========================================================================
    Facet History
   ========================================================================== */
R2V2.Facet.History = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length === 0) return;
        
        // cache bound/proxy'd method calls
        this.f = {
            onHistorySet: $.proxy(this.onHistorySet, this),
            onFilteringSuccess: $.proxy(this.onFilteringSuccess, this),
            onClick: $.proxy(this.onClick, this)
        };

        // setup
        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
        $.PubSub(R2V2.PubSubMappings.Filtering.Success).subscribe(this.f.onFilteringSuccess);
        
        // DOM events
        this.$container.on('click', 'a', this.f.onClick);
    },
    
    onHistorySet: function (event)
    {
        // check for "exclusions"
        if (this._exclusions == null)
        {
            this.selections = event;
            return;
        }
        
        // build "selections" (remove "excluded" filter groups)
        this.selections = { };
        _.each(event, function (value, key) { if (_.indexOf(this._exclusions, key) >= 0) return; this.selections[key] = value; }, this);
    },
    
    onFilteringSuccess: function (event)
    {
        this.data = event;
        this.render();
    },
    
    onClick: function (event)
    {
        event.preventDefault();

        var target = R2V2.Utils.GetEventTarget(event, 'a');
        this[target.DOM.hash.substr(1)](target.$);
    },
    
    render: function ()
    {
        // check for "template"
        if (this._template == null) return;
        
        // check "selections"
        if (_.isEmpty(this.selections))
        {
            this.$container.hide();
            return;
        }
        
        // create history data by combining "filter data" and "selections" (b/c data is combination of "filter" and "option" group data always try "filter" first)
        var history = _.map(this.selections, function (filterCode, groupCode) {
            if (groupCode === 'query' && filterCode != null)
            {
                return {
                    IsStatic: true,
                    Name: 'Search Terms',
                    Value: filterCode
                };
            }
            
            if (this[groupCode] == null) return false;
            
            return {
                IsStatic: false,
                GroupName: this[groupCode].Name,
                GroupCode: groupCode,
                FilterName: (this[groupCode].Filters && this[groupCode].Filters[filterCode]) ? this[groupCode].Filters[filterCode].Name : this[groupCode].Value,
                FilterCode: filterCode
            };
        }, this.data);
        
        // update
        this.$container.show().html(this._template({ history: history }));
    },
    
    remove: function ($el)
    {
        $.PubSub(R2V2.PubSubMappings.History.Remove).publish($el.data('group-code'));
    },
    
    clear: function ()
    {
        $.PubSub(R2V2.PubSubMappings.History.Remove).publish(_.keys(this.selections));
    }
});