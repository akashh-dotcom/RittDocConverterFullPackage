/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Search Controller
    extends: R2V2.Facet.Controller
   ========================================================================== */
R2V2.Search = R2V2.Facet.Controller.extend({

    _defaultState: { 'include': 1 },
    
    init: function (config)
    {
        this.hasTocAccess = config.hasTocAccess || false;
        this._defaultState = { 'include': config.include || this._defaultState.include };

        // call parent
        this._super(config);
    },

    getData: function ()
    {
        var state = {
            q: this.query || '',
            include: this.hash['include'] || this._defaultState['include'],
            tocAvailable: this.hash['toc-available'] || '',
            sortBy: this.hash['sort-by'] || '',
            field: this.hash['field'] || '',
            filter: this.hash['filter-by'] || '',
            practiceArea: this.hash['practice-area'] || '',
            disciplines: this.hash['disciplines'] || '',
            page: this.hash['page'] || '',
            pageSize: this.hash['results-per-page'] || '',
            author: this.hash['author'] || '',
            title: this.hash['title'] || '',
            publisher: this.hash['publisher'] || '',
            editor: this.hash['editor'] || '',
            isbn: this.hash['isbn'] || '',
            within: this.hash['within'] || '',
            year: this.hash['year'] || ''
        };

        // someone w/out TOC access tries to update the URL with it
        if (state.tocAvailable && this.hasTocAccess === false)
        {
            $.PubSub(R2V2.PubSubMappings.History.Remove).publish('toc-available');
            return null;
        }
        
        return $.param(state);
    }
});


/* ==========================================================================
    Search Results
    extends: R2V2.Facet.Results
   ========================================================================== */
R2V2.Search.Results = R2V2.Facet.Results.extend({
    
    _loadingText: 'Searching...',
    _debugTemplate: _.template('<!-- debug search time: {{= TotalSearchTime }} -->'),
    
    renderContent: function ()
    {
        // set content data
        var data = this.data.HtmlSnippets;
        
        // update content
        this.$container.empty().append(data.paging).append(data.totals).append(data.results).append(this._debugTemplate(this.data));
        
        // implement tooltips, fixed menus, and see more/less features
        this.$container.tips().fixed({ selector: '[data-implement="fixed"]' }).moreless({ selector: '[data-toggle="moreless"]' });
    }
});


/* ==========================================================================
    Search Filters
    extends: R2V2.Facet.Filters
   ========================================================================== */
R2V2.Search.Filters = R2V2.Facet.Filters.extend({ });


/* ==========================================================================
    Search History
    extends: R2V2.Facet.History
   ========================================================================== */
R2V2.Search.History = R2V2.Facet.History.extend({
    
    _template: _.template('<h2>Current Search</h2><ul>{{ _.each(history, function(props) { if (props === false) return; }}{{ if (props.IsStatic) { }}<li><p class="search-terms"><strong>{{= props.Name }}</strong>: {{= props.Value }}</p></li>{{ } else { }}<li><a href="#remove" data-group-code="{{= props.GroupCode }}">[ <em>X</em> ]</a><p><strong>{{= props.GroupName }}</strong>: {{= props.FilterName }}</p></li>{{ } }}{{ }); }}</ul>{{ if (!(history.length === 1 && history[0].IsStatic)) { }}<p><a href="#clear">Clear Search Filters</a></p>{{ } }}'),
    _exclusions: ['include'], // list of filter and option group codes to exclude from history display

    // constructor in base class
    
    onHistorySet: function (response)
    {
        // combine query and hash data
        this._super($.extend(true, { query: response.query }, response.hash));
    },
    
    onFilteringSuccess: function (response)
    {
        // combine "filter" and "option" group data (deep copy)
        this._super($.extend(true, response.FilterGroups, response.OptionGroups));
    }
});


/* ==========================================================================
    Search Elsewhere
   ========================================================================== */
R2V2.Search.Elsewhere = Class.extend({
    
    _template: _.template('({{= Count }})'),

    init: function (config)
    {
        this.config = config || {};
        this.dataUrl = this.config.dataUrl;
        this.$pubmed = this.config.$pubmed;
        this.$mesh = this.config.$mesh;
        
        // error check
        if ((this.$pubmed == null || this.$pubmed.length === 0) && (this.$mesh == null || this.$mesh.length === 0)) return;
        
        // cache bound/proxy'd method calls
        this.f = {
            onHistorySet: $.proxy(this.onHistorySet, this),
            onGetDataSuccess: $.proxy(this.onGetDataSuccess, this)
        };

        // setup
        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
    },
    
    onHistorySet: function (response)
    {
        if (response.query == null || (this.query && this.query === response.query)) return; // only call if query changes
        this.query = response.query;
        
        $.executeService({ url: this.dataUrl, data: { q: this.query } }).then(this.f.onGetDataSuccess);
    },
    
    onGetDataSuccess: function (response)
    {
        if (response == null)
        {
            this.onGetDataError(response);
            return;
        }
        
        this.data = response;
        this.render();
    },
    
    render: function ()
    {
        this.$pubmed.length && this.$pubmed.html(this._template({ Count: this.data.PubMed }));
        this.$mesh.length && this.$mesh.html(this._template({ Count: this.data.Mesh }));
    }
});


R2V2.Search.ExternalSearch = Class.extend({

    init: function (config) {
        this.config = config || {};
        this.dataUrl = this.config.dataUrl;
        this.$selector = this.config.$selector;

        // error check
        if ((this.$selector == null || this.$selector.length === 0)) return;

        $.ajax({
            url: config.dataUrl,
            dataType: 'json',
            success: function (data) {
                $(config.$selector).html("(" +  data["esearchresult"]["count"].replace(/(\d)(?=(\d\d\d)+(?!\d))/g, "$1,")  + ")" );
            },
            error: function (error) {
                console.log(error);
            }
        });
    }
});


/* ==========================================================================
    Search Actions Menu
    extends: R2V2.ActionsMenu
   ========================================================================== */
R2V2.Search.ActionsMenu = R2V2.ActionsMenu.extend({
    init: function (config)
    {
        this.config = config || {};
        this.config.self = R2V2.Search.ActionsMenu;

        this._super();
    }
});


/* ==========================================================================
    Search Actions Menu Panel
    extends: R2V2.ActionsMenu.Panel
   ========================================================================== */
R2V2.Search.ActionsMenu.Panel = R2V2.ActionsMenu.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        
        if (this.$container == null || this.$container.length <= 0) return;

        // cache bound/proxy'd method calls
        this.f = {
            onFilteringSuccess: $.proxy(this.onFilteringSuccess, this),
            onHistorySet: $.proxy(this.onHistorySet, this)
        };

        // call parent method
        this._super();
    },

    attachEvents: function ()
    {
        // // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
        $.PubSub(R2V2.PubSubMappings.Filtering.Success).subscribe(this.f.onFilteringSuccess);

        // call parent method
        this._super();
    },

    onHistorySet: function (response)
    {
        if (response == null) return;
        
        this.params = {
            q: response.query,
            include: response.hash['include'],
            tocAvailable: response.hash['toc-available'],
            sortBy: response.hash['sort-by'],
            field: response.hash['field'],
            filter: response.hash['filter-by'],
            practiceArea: response.hash['practice-area'],
            disciplines: response.hash['disciplines'],
            page: response.hash['page'],
            pageSize: response.hash['results-per-page'],
            author: response.hash['author'],
            title: response.hash['title'],
            publisher: response.hash['publisher'],
            editor: response.hash['editor'],
            isbn: response.hash['isbn'],
            within: response.hash['within'],
            year: response.hash['year']
        };
    },

    onFilteringSuccess: function (response)
    {
        if (response == null) return;
        this.params = $.extend(true, { total: response.TotalResults }, this.params);
        
        // set "page" data
        this.$container.data('params', this.params);
    }
});


/* ==========================================================================
    Search Actions Menu Save Query Panel
    extends: R2V2.Search.ActionsMenu.Panel
   ========================================================================== */
R2V2.Search.ActionsMenu.SaveQueryPanel = R2V2.Search.ActionsMenu.Panel.extend({ });


/* ==========================================================================
    Search Actions Menu Email Panel
    extends: R2V2.Search.ActionsMenu.Panel
   ========================================================================== */
R2V2.Search.ActionsMenu.EmailPanel = R2V2.Search.ActionsMenu.Panel.extend({
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
            formData = $.param(this.$container.data('params')) + '&' + this.$form.serialize();
        
        // execute service call
        $.executeService({ url: dataUrl, data: formData, type: 'POST', contentType: R2V2.ServiceExecutionOptions.ContentType.DEFAULT }).then(this.f.onSaveDataSuccess).fail(this.f.onSaveDataError);
    }
});


/* ==========================================================================
    Search Button Disable
   ========================================================================== */
R2V2.Search.Disable = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length === 0) return;

        this.$button = this.$container.find('button[type="submit"]:first');

        // cache bound/proxy'd method calls
        this.f = {
            onFilteringStart: $.proxy(this.onFilteringStart, this),
            onFilteringComplete: $.proxy(this.onFilteringComplete, this)
        };

        this.attachEvents();
    },

    attachEvents: function ()
    {
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onFilteringStart);
        $.PubSub(R2V2.PubSubMappings.Filtering.Success).subscribe(this.f.onFilteringComplete);
        $.PubSub(R2V2.PubSubMappings.Filtering.Error).subscribe(this.f.onFilteringComplete);
    },

    onFilteringStart: function (event)
    {
        this.$button.attr('disabled', 'disabled');
    },

    onFilteringComplete: function () 
    {
        this.$button.removeAttr('disabled');
    }

});

/* ==========================================================================
    Global Search
   ========================================================================== */
R2V2.Search.Global = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length === 0) return;

        // cache DOM elements
        this.$form = this.$container.find('form');
        this.action = this.$form.attr('action');
        this.$q = this.$container.find('input[name="q"]');
        this.$options = this.$container.find('#search-options');
        this.$yearFields = this.$options.find('select');
        
		this.include = new R2V2.Search.CompositeCheckboxGroup({groupId: "include", $container: this.$options, defaultValue:"1", required: true });
		this.$tocAvailable = this.$options.find('#search-toc-available');

        // cache values
		if (this.$q.offset() == null) return;
         
		this.qTopPosition = this.$q.offset().top;
        this.value = this.$q.val();

        // cache bound/proxy'd method calls
        this.f = {
            onFormSubmit: $.proxy(this.onFormSubmit, this),
            onKeyboardFocus: $.proxy(this.onKeyboardFocus, this),
            onKeyboardCancel: $.proxy(this.onKeyboardCancel, this),
            onQueryFocus: $.proxy(this.onQueryFocus, this),
            onQueryBlur: $.proxy(this.onQueryBlur, this),
            onHistorySet: $.proxy(this.onHistorySet, this)
        };

        // setup
        this.setup();
        this.attachEvents();
    },

    setup: function ()
    {
        new R2V2.Search.Global.Options({ $container: this.$options, $form: this.$form, include: this.include });
    },
    
    attachEvents: function ()
    {
        // DOM events
        R2V2.Elements.Document.on(R2V2.KeyboardMappings.Search.Focus, this.f.onKeyboardFocus);
        this.$form.on('submit', this.f.onFormSubmit);
        this.$q.on('focus', this.f.onQueryFocus).on('blur', this.f.onQueryBlur);
	    
		$.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
    },

    onKeyboardFocus: function (event)
    {
        // check if modal is currently open
        if (R2V2.Elements.Body.hasClass('modal-open')) return;
        
        // simulate body click to close any open menus
        R2V2.Elements.Body.trigger('click');

        // focus field
        this.$q.focus();
        
        // if search is not in viewport, scroll to it
        if (this.$q.get(0).getBoundingClientRect().top < 0) R2V2.Elements.Window.scrollTop(this.qTopPosition - 15); // offset for "breathing room"
    },

    onKeyboardCancel: function ()
    {
        // set field to original value and blur field
        this.$q.val(this.value).blur();
    },

    onQueryFocus: function ()
    {
        // select field and attach keyboard cancel logic
        this.$q.select().on(R2V2.KeyboardMappings.Search.Cancel, this.f.onKeyboardCancel);
    },
    
    onQueryBlur: function ()
    {
        // detach keyboard cancel logic
        this.$q.off(R2V2.KeyboardMappings.Search.Cancel);
    },

    onFormSubmit: function (event)
    {
        event.preventDefault();

        var isQueryOnlySearch = this.$container.hasClass('toggled') === false,
            query = this.$q.val() === '' ? '' : this.$q.serialize(),
            options = this.$options.find(':input[type="text"][value!=""]').serialize(),
	        checkboxes = this.$options.find('input:checkbox:not([composite-checkbox-group])').serialize(),
            years = this.$options.find('select[value!=""]').serialize();

        // return if all fields are empty OR if query is empty when a query only search is performed
        if ((query === '' && options === '' && years === '') || (query === '' && isQueryOnlySearch)) return;

	    var include = "include=" + this.include.value();
		options = (options === '') ? include : options + '&' + include;
	    
        if (checkboxes !== '')
        {
            options += '&' + checkboxes;
        }

        // combine years into one param
        if (years !== '')
        {
            years = _.map(this.$yearFields, function (field) { return parseInt($(field).val() || $(field).data('default')); });
            years = 'year=' + ((years[0] === years[1]) ? years[0] : years[0] + '-' + years[1]);
            options += '&' + years;
        }
        
        // build URL
        var url = this.action + '?' + query + '#' + options;
        if (isQueryOnlySearch)
        {
            url = this.action + '?' + query;
        }
        if (query === '')
        {
            url = this.action + '#' + options;
        }
        
        window.location = url;
    },

	onHistorySet: function(response)
    {
        this.include.render(response.hash['include']);

        // if (this.$tocAvailable && response.hash['toc-available'] == "true") this.$tocAvailable.attr({ 'checked': true });

        if (!this.$tocAvailable) {
            return;
        }

        this.$tocAvailable.attr({ 'checked': response.hash['toc-available'] == "true" });
    }
});


/* ==========================================================================
    Global Search Options
    extends: R2V2.Panel
   ========================================================================== */
R2V2.Search.Global.Options = R2V2.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) return;
        this.$form = this.config.$form;
	    this.include = this.config.include;

        // cache DOM elements
        this.lookup = { };
        _.each(this.$container.find(':input'), function(field) { if (field.name == null || field.name === '') return; this[field.name] = $(field); }, this.lookup);
        this.$clear = this.$container.find('a.action');
        
        // cache bound/proxy'd method calls
        this.f = {
            onHistorySet: $.proxy(this.onHistorySet, this),
            onHistoryRemove: $.proxy(this.onHistoryRemove, this),
            onClearClick: $.proxy(this.onClearClick, this),
            focusField: $.proxy(this.focusField, this)
        };
        
        // call parent method
        this._super();
    },

    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
        $.PubSub(R2V2.PubSubMappings.History.Remove).subscribe(this.f.onHistoryRemove);
        $.PubSub(R2V2.PubSubMappings.Menu.Opened).subscribe(this.f.onOpen);
        $.PubSub(R2V2.PubSubMappings.Menu.Closed).subscribe(this.f.onClose);
        
        // DOM events
        this.$clear.on('click', this.f.onClearClick);
    },
    
    attachInstanceEvents: function ()
    {
        // focus first container form element
        this.$container.find(':input:visible:enabled:first').focus();
        
        // enable keyboard form "save" for container "select" boxes
        this.$container.find('select:visible:enabled').on(R2V2.KeyboardMappings.Panel.Save, this.f.onFormSave);

        // enable keyboard form "cancel" for container form fields
        this.$container.find(':input:visible:enabled').on(R2V2.KeyboardMappings.Panel.Cancel, this.f.onFormCancel);
    },

    detachInstanceEvents: function ()
    {
        this.$container.find(':input:visible:enabled').off();
    },
    
    onHistorySet: function(response)
    {
        _.each(response.hash, function(value, name) {
            // year ranges are combined into one param
            if (name === 'year' && value.indexOf('-') > 0)
            {
                var years = value.split('-');
                this.lookup['year-start'].val(years[0]);
                this.lookup['year-end'].val(years[1]);
                return;
            }

            // other fields
            if (this.lookup[name] == null) {
                return;
            }

            this.lookup[name].val(value);
        }, this);
    },
    
    onHistoryRemove: function (state)
    {
        _.each(_.flatten([state]), function (name) {
            // don't need to reset "checkbox" types
            if (name === 'toc-available') {
                return;
            }

            // year ranges are combined into one param
            if (name === 'year')
            {
                this.lookup['year-start'].val(null);
                this.lookup['year-end'].val(null);
                return;
            }
            
            // other fields
            if (this.lookup[name] == null) {
                return;
            }

            this.lookup[name].val(null || '');
        }, this);
    },

    onClose: function (event)
    {
        // call parent method
        this._super(event, this.f.focusField);
    },
    
    onFormSave: function ()
    {
        this.$form.submit(); // save logic is in R2V2.Search.Global
    },
    
    onClearClick: function (event)
    {
        event.preventDefault();
        
        this.$form.find(':input:not(:checkbox)').val('').blur();
	    this.$form.find(':input:checkbox').attr({'checked': false});
	    this.include.reset();
    },
    
    focusField: function ()
    {
        this.$form.find(':input:visible:enabled:first').focus();
    }
});


R2V2.Search.CompositeCheckboxGroup = Class.extend({
	init: function(config)
	{
		this.config = config || {};
		this.groupId = this.config.groupId;
		this.$container = this.config.$container;
		this.selector = '[composite-checkbox-group=' + this.groupId + ']';
		this.$checkboxes = this.$container.find(this.selector);
		this.defaultValue = this.config.defaultValue;
		this.required = config.required;
		
		// cache bound/proxy'd method calls
        this.f = {
            onChange: $.proxy(this.onChange, this)
        };
		
		// attach DOM events
        this.$container.on('click', 'input:checkbox' + this.selector, this.f.onChange);

		this.reset();
	},

	render: function(value)
	{
		value = value || this.value();

		_.each(this.$checkboxes, function(checkbox) {
			$(checkbox).attr({
				'checked': this.checked(checkbox.value, value),
				'disabled': this.disabled(checkbox.value, value)
			});
		}, this);
	},
	
	checked: function(boxValue, value)
	{
		return value == (value | boxValue);
	},
	
	disabled: function(boxValue, value)
	{
		return this.required && value == boxValue;
	},
	
	value: function()
	{
		return _.chain(this.$checkboxes.filter(':checked'))
			.invoke(function() { return parseInt(this.value); })
			.reduce(function(memo, num) { return memo | num; }, 0)
			.value();
	},
	
	reset: function()
	{
		this.render(this.defaultValue);
		_.each(this.$checkboxes, function(checkbox) {
			checkbox.blur();
		});
	},
	
	onChange: function()
	{
		this.render();
	}
});



/* ==========================================================================
    Search Typeahead
   ========================================================================== */
R2V2.Search.Typeahead = Class.extend({
	init: function (config) {
		this.config = config || {};
		this.typeaheadUrl = this.config.typeaheadUrl;
		this.typeaheadEnabled = this.config.typeaheadEnabled;
		this.$container = this.config.$container;
		if (this.$container == null || this.$container.length === 0 || !this.typeaheadEnabled) return;

		// cache DOM elements
		this.$q = this.$container.find('input[name="q"]');

		var that = this;
		this.$q.autocomplete({
			source: function (request, response) {
				jQuery.get(that.typeaheadUrl, {
					searchInput: that.$q.val()
				}, function (data) {
					response(data);
				});
			},
			delay: 0
		});
	}
});


/* ==========================================================================
    Create OnDOMReady
   ========================================================================== */
; (function ($) {
    
    new R2V2.Search.Global({ $container: $('#search') });
    
})(jQuery);