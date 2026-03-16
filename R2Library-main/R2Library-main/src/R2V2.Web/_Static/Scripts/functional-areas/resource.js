/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Resource Controller
   ========================================================================== */
R2V2.Resource = Class.extend({
	init: function(config)
	{
		this.config = config || {};
		this.dataUrl = this.config.dataUrl;
		this.titleUrl = this.config.titleUrl;
		this.isbn = this.config.isbn;
		this.hasContent = this.config.hasContent;
		this.contentJson = this.config.contentJson;

		// error check
		if (this.isbn == null) return;

		// cache bound/proxy'd method calls
		this.f = {
			onHistorySet: $.proxy(this.onHistorySet, this),
			onGetDataSuccess: $.proxy(this.onGetDataSuccess, this),
			onGetDataError: $.proxy(this.onGetDataError, this)
		};

		// setup
		this.attachEvents();
	},

	attachEvents: function()
	{
		// subscriptions
		$.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
		$.PubSub(R2V2.PubSubMappings.History.Subscribe).publish();
	},
	
	onHistorySet: function(response)
	{
		var hash = response.hash,
			section = hash['section'];

		// error handling
		if (section == null && !this.hasContent) {
			this.onHistoryError();
			return;
		}

		// same page
		if (section != null && this.section === section) return;

		// cache current
		this.section = section;

		// publish
		$.PubSub(R2V2.PubSubMappings.Resource.Changed).publish(section);

		if (!this.hasContent) {
			this.getContent(section);
		} else {
			$.PubSub(R2V2.PubSubMappings.Resource.Success).publish(this.contentJson);
		}
	},

	onHistoryError: function()
	{
		window.location = this.titleUrl;
	},

	onGetDataSuccess: function(response)
	{
		this.data = response;

		// publish
		$.PubSub(R2V2.PubSubMappings.Resource.Success).publish(this.data);
	},

	onGetDataError: function(response)
	{
		// publish
		$.PubSub(R2V2.PubSubMappings.Resource.Error).publish(response);
	},
	
	getContent: function(section)
	{
		this.trySingleContentRequest();

		// get json data
		$.executeService({ url: this.dataUrl, data: { isbn: this.isbn, section: section } }).then(this.f.onGetDataSuccess).fail(this.f.onGetDataError);
	},

	trySingleContentRequest: function ()
	{
		var url = document.URL;

		if (url.indexOf('#section=') > 0) {
			url = url.replace('#section=', '/');

			var ampIndex = url.indexOf('&');

			if (ampIndex > 1) {
				url = R2V2.Utils.ReplaceAt(url, ampIndex, '#');
			}

			window.location.href = url;
		}
	}
});


/* ==========================================================================
    Resource Figure
   ========================================================================== */
R2V2.Resource.Figure = Class.extend({
	init: function()
	{
		// cache bound/proxy'd method calls
		this.f = {
			onInlineResourceViewChange: $.proxy(this.onInlineResourceViewChange, this)
		};

		// setup
		this.attachEvents();
	},

	attachEvents: function()
	{
		// subscriptions
		$.PubSub(R2V2.PubSubMappings.InlineResourceView.Changed).subscribe(this.f.onInlineResourceViewChange);
	},
	
	onInlineResourceViewChange: function (){} //override this
});


/* ==========================================================================
    Resource Figure Table
   ========================================================================== */
R2V2.Resource.Figure.Table = R2V2.Resource.Figure.extend({
	init: function()
	{
		this._super();

		this.contentWidth = $('#resource-content>div').width();
		this.maxBodyHeight = 500;

		var that = this;
		$('.figure').each(function() {
			var $figure = $(this);
			that.prepareFigtable($figure);
	
			if ($figure.is('.inline')) {
				this.setFigtableTopScroll($figure);
			}		
		});		
	},

	onInlineResourceViewChange: function(response)
	{
		var $figure = response.figure;
		var freezeTables = $figure.find('table[data-freeze-header-col], table[data-freeze-header-row]');
		
		var that = this;
		freezeTables.each(function() {
			var $table = $(this);

			that.setFreezeHeader($table);

			that.adjustDimensions($table);
		});	
		
		this.setFigtableTopScroll($figure);	
	},	

	prepareFigtable: function($figure)
	{
		var tables = $figure.find('table');
			
		var that = this;
		tables.each(function() {
			var $table = $(this);

			var emptyCaption = $table.find('caption:empty');
			if (emptyCaption.length > 0) {
				emptyCaption.css('display', 'none');
				$table.css('border-top', '0px');
			}

			if ($table.attr('data-freeze-header-row') || $table.attr('data-freeze-header-row')) {
				that.setFreezeHeader($table);

				that.prepareLayout($table);

				if ($figure.is('.inline')) this.adjustDimensions($table);
			}
		});	
	},
	
	setFreezeHeader: function($table)
	{
		this.freezeHeaderRow = $table.data('freeze-header-row');
		this.freezeHeaderCol = $table.data('freeze-header-col');

		if (this.freezeHeaderRow || this.freezeHeaderCol) {
			$table.addClass('freeze-header');
			
			if (this.freezeHeaderRow) $table.addClass('freeze-header-row');
			if (this.freezeHeaderCol) $table.addClass('freeze-header-col');	
		}	
	},

	prepareLayout:	function($table)
	{
		if ($table.length == 0) return;

		var $thead = $table.find('thead');
		var $tbody = $table.find('tbody');

		$tbody.wrap('<tbody><tr><td class="datacols"><div><table></table></div></td></tr></tbody>');
		$tbody.before($thead);
	},

	splitCells: function ($lGroup, $rGroup, width) {
		var tag = $rGroup.prop('tagName').toLowerCase() == 'thead' ? 'th' : 'td';

		var arrSelector = [];
		for (var i = 1; i <= width; i++) {
			arrSelector.push(tag + ':nth-col(' + i + ')');
		}
		var selector = arrSelector.join();
		var $cells = $rGroup.find(selector);

		var aRows = [];
		this.clones = [];
		var $row;
		var idx = -1, cIdx = -1;
		var self = this;
		$cells.each(function () {
			var cell = this;
			var $cell = $(this);
			var $tr = $cell.parent();

			$cell.removeAttr('rowspan');
			var colspan = parseInt($cell.attr('colspan') || 1);
			if (colspan > width) $cell.attr('colspan', width);

			if (tag == 'th') $cell.height(self.getCellHeight(cell));

			if ($row && $tr[0] == $row[0]) {
				aRows[idx].push($cell);
			} else {
				$row = $tr;
				aRows.push([]);
				aRows[++idx].push($cell);
			}

			if (colspan > width || $tr.children().length == 1) {
				var $clone = $cell.clone();
				$clone.empty().html('&nbsp;');

				if (colspan > 1) {
					$clone.attr('colspan', colspan - width);
				}
				$tr.prepend($clone);

				self.clones.push([]);
				++cIdx;
				self.clones[cIdx].push($cell);
				self.clones[cIdx].push($clone);
			}
		});

		for (i = 0; i < aRows.length; i++) {
			var cells = aRows[i];
			var $tr = $('<tr/>');
			for (var j = 0; j < cells.length; j++) {
				$tr.append(cells[j]);
			}
			$lGroup.append($tr);
		}
	},

	adjustDimensions: function($table)
	{
		if ($table.data('table-prepared') == 'true') return true;

		var $rParent = $table.find('td.datacols'),
			$rBody = $rParent.find('tbody'),
			$rTable = $rParent.find('table'),
			$rHead = $rParent.find('thead'),
			$rDiv = $rParent.find('>div'),
			$fakeVScroll;

		var $r = $rHead.length > 0 ? $rHead : $rBody;
		var $firstCells = $r.find('tr:first').children();
		var firstColspan = parseInt($firstCells.first().attr('colspan') || 1);
		this.freezeHeaderCol = this.freezeHeaderCol && $table.width() > this.contentWidth && ($firstCells.length > 1 || firstColspan > 1);

		$table.css('width', this.contentWidth - 2);

		if (this.freezeHeaderCol) {
			$rParent.before('<td class="headercol"><div><table><thead></thead><tbody></tbody></table></div></td>');
		}

		var $lParent = $table.find('td.headercol'),
			$lHead = $lParent.find('thead'),
			$lBody = $lParent.find('tbody'),
			$lDiv = $lParent.find('>div');

		if (this.freezeHeaderCol) {
			this.cacheCellHeights($rTable, firstColspan);
			this.splitCells($lHead, $rHead, firstColspan);
			this.splitCells($lBody, $rBody, firstColspan);
			this.resetCellHeights();
		}

		$rParent.find('tr th[rowspan]:first-child, tr td[rowspan]:first-child').each(function () {
			var $cell = $(this);
			var $row = $cell.parent();
			var rowspan = $cell.attr('rowspan');
			while (rowspan > 1) {
				$row = $row.next();
				$row.find('th:first-child, td:first-child').css('border-left', '1px solid #DDDBD0');
				rowspan--;
			}
		});

		var hasVScroll = $rBody.height() > this.maxBodyHeight && $rHead.length > 0;
		var bodyHeight = hasVScroll ? this.maxBodyHeight : $rBody.height();
		this.freezeHeaderRow = this.freezeHeaderRow && hasVScroll;

		if (!this.freezeHeaderRow && !this.freezeHeaderCol) return true;

		if (this.freezeHeaderRow) {
			$rParent.after('<td class="fakevscroll"><div><div>&nbsp;</div></div></td>');

			$fakeVScroll = $table.find("td.fakevscroll>div");

			var that = this;
			$fakeVScroll.scroll(function() {
				$table.find(that.scrollSelector).scrollTop($fakeVScroll.scrollTop());
			});
		} else {
			$rDiv.scroll(function() {
				$lDiv.scrollTop($rDiv.scrollTop());
			});			
		}

		if (this.freezeHeaderRow) {
			$lBody.css('height', bodyHeight + 'px');	
		}

		var $lParentCell = $lParent.find('td:first');
		$lParent.innerWidth($lParentCell.innerWidth());
		var lParentWidth = $lParent.width();
		var rParentWidth = this.contentWidth - lParentWidth - 2;
		
		if (!this.freezeHeaderRow) {
			$rDiv.width(rParentWidth);
		} else {
			var $fakeVScrollParent = $table.find("td.fakevscroll");
			$fakeVScroll = $fakeVScrollParent.find(">div");
			$rDiv.css('width', rParentWidth - parseFloat($fakeVScroll.css('width')) + 2);
		}

		var maxHeadroom = 0;

		if (this.freezeHeaderCol) {
			var rHeight = $rHead.height();
			var $th = $rHead.find('tr:last th:first');
			var thHeight = $th.height();

			maxHeadroom = Math.max(this.getBoundingClientRect($lHead[0]).height, this.getBoundingClientRect($rHead[0]).height);
			$lHead.height(maxHeadroom);
			$rHead.height(maxHeadroom);

			$th.height(thHeight + Math.abs(maxHeadroom - rHeight)-1);
		}

		$lHead.find('th:first').width($lBody.find('td:first').width());
		
		if (this.freezeHeaderRow) {
			this.cacheCellWidths($rTable);
			$rHead.css('display', 'block');
			$rBody.css('display', 'block');
			this.resetCellWidths();

			var $fakeVScrollDiv = $fakeVScroll.find("div");
			if (this.freezeHeaderCol) {
				$fakeVScrollDiv.height($table.find('tbody:first').height());
			} else {
				$fakeVScrollDiv.height($rTable.height() + 17);
			}
			
			if ($('html').is('.ie7, .ie8')) {
				this.scrollSelector = "td.headercol div, td.datacols div";
				
				if (hasVScroll) {
					$lDiv.css('height', bodyHeight - 17 + 'px');
					$rDiv.css('height', bodyHeight + 'px');					
				}
				
				$lDiv.css('overflow-y', 'hidden');
				$rDiv.css('overflow-y', 'hidden');
		
			} else {
				this.scrollSelector = "td.headercol tbody, td.datacols tbody";
			}
			
			$rBody.css('height', bodyHeight + 'px');

			var shrinkScroll = this.freezeHeaderCol ? maxHeadroom : $rHead.height();
			$fakeVScroll.height($rDiv.height() - shrinkScroll - 1);
	
			$fakeVScroll.css('margin-left', -1);

			var fakeVScrollWidth = $.browser.msie || $.browser.mozilla ? 18 : 17;
			$fakeVScroll.width(fakeVScrollWidth);
			$fakeVScrollDiv.width(fakeVScrollWidth);
		}
		
		if (this.freezeHeaderCol && !this.freezeHeaderRow && hasVScroll) {
			var maxBodyroom = Math.max(this.getBoundingClientRect($lBody[0]).height, this.getBoundingClientRect($rBody[0]).height);
			$lBody.height(maxBodyroom);
			$rBody.height(maxBodyroom);
			
			var minDivroom = Math.min(this.getBoundingClientRect($lDiv[0]).height, this.getBoundingClientRect($rDiv[0]).height);
			$lDiv.height(minDivroom - 17);
			$rDiv.height(minDivroom);			
		}

		$table.data('table-prepared', 'true');
		return true;
	},
	
	getBoundingClientRect: function(element)
	{
		if (element == undefined) return element;
	
		var rect = element.getBoundingClientRect();
		if (rect.width == undefined) {
			var r = {};

			/*IE8 does not support these properties*/
			r.width = Math.abs(rect.right - rect.left);
			r.height = Math.abs(rect.top - rect.bottom);

			r.right = rect.right;
			r.left = rect.left;
			r.top = rect.top;
			r.bottom = rect.bottom;

			rect = r;
		}

		return rect;
	},

	cacheCellHeights: function ($table, width) {
		var self = this;
		var col = width + 1;

		this.cells = $table.find('td:nth-col(1), td:nth-col(' + col + ')');

		this.cellHeights = this.cells.map(function () {
			var rowspan = parseInt($(this).attr('rowspan') || 1);
			return self.getCellHeight(this, rowspan);
		});
	},

	resetCellHeights: function () {
		for (var i = 0; i < this.cells.length; i++) {
			var $cell = $(this.cells[i]);
			var height = this.cellHeights[i];
			$cell.height(height);
			$cell.css('min-height', height);
		}

		for (i = 0; i < this.clones.length; i++) {
			$cell = this.clones[i][0];
			var cell = $cell[0];
			var $clone = this.clones[i][1];
			height = this.getCellHeight(cell)+1;
			$cell.height(height);
			$cell.css('min-height', height);
			$clone.height(height);
			$clone.css('min-height', height);
		}
	},

	cacheCellWidths: function($table)
	{
		this.cells = $table.find('thead th, tbody td');
		var self = this;
		this.cellWidths = this.cells.map(function () {
			var cell = this;

			var colspan = parseInt($(cell).attr('colspan') || 1);
			return self.getCellWidth(cell, colspan);
		});
	},
	
	resetCellWidths: function()
	{
		for (var i = 0; i < this.cells.length; i++) {
			var $cell = $(this.cells[i]);
			var width = this.cellWidths[i];
			$cell.width(width);
			$cell.css('min-width', width);
		}

	},

	getCellWidth: function (cell, colspan)
	{
	    var $cell = $(cell);
	
	    if ($.browser.msie || $.browser.mozilla) {
	    	var width = this.getBoundingClientRect(cell).width - parseInt($cell.css('padding-left')) - parseInt($cell.css('padding-right')) - parseInt($cell.css('border-left-width'));
		    return width;
	    }

        return $cell.width();
	},

	getCellHeight: function(cell, rowspan)
	{		
		var $cell = $(cell);

		if (($.browser.msie || $.browser.mozilla)) {
			var height = this.getBoundingClientRect(cell).height - parseInt($cell.css('padding-top')) - parseInt($cell.css('padding-bottom'));
			if(rowspan > 1) height += (rowspan - 1);
			return height;
		}

		return $cell.height();
	},

	setHeights: function(cells, heights)
	{		
		for (var i = 0; i < cells.length; i++) {
			$(cells[i]).css("height", heights[i]);	
		}	
	},
	
	setFigtableTopScroll: function($figure)
	{
		var $figtable = $figure.find(".figtable");
		var $topscroll = $figure.find(".topscroll");
		
		if ($figtable.length == 0 || $topscroll.is(".scrollable")) return;	

		var $table = $figtable.find("table");
		var tableWidth = parseInt($table.width());
		var figureWidth = parseInt($figure.width());

		if (tableWidth > figureWidth) {
			var $topscrollwrapper = $figure.find(".topscrollwrapper");		
			$topscrollwrapper.scroll(function(){
				$figtable.scrollLeft($topscrollwrapper.scrollLeft());
			});
			$figtable.scroll(function(){
				$topscrollwrapper.scrollLeft($figtable.scrollLeft());
			});

			$topscroll.width(tableWidth);
			$topscroll.addClass("scrollable");
			$topscrollwrapper.addClass("scrollable");
		}
	}
});


/* ==========================================================================
    Resource Taber's Entry
   ========================================================================== */
R2V2.Resource.Tabers = Class.extend({
	init: function(config)
	{
		this.config = config || {};
		this.termContentUrl = this.config.termContentUrl;
		//this.enableTerms = this.config.enableTerms;
		this.showAllTerms = this.config.showAllTerms;
		this.$container = this.config.$container;

		// error check
		if(this.termContentUrl == null) return;

		// cache bound/proxy'd method calls
		this.f = {
			onClick: $.proxy(this.onClick, this),
			onMouseEnter: $.proxy(this.onMouseEnter, this),
			onMouseLeave: $.proxy(this.onMouseLeave, this),
			onGetTabersEntrySuccess: $.proxy(this.onGetTabersEntrySuccess, this),
			onGetTabersEntryError: $.proxy(this.onGetTabersEntryError, this)
		};

		//Only show tabers tooltips when showAllTerms == true
		if (!this.showAllTerms) return;

		this.setStyles();

		// setup
		this.attachEvents();
	},

	setStyles: function()
	{
        var $term = $('.tabers-term');
        $term.addClass("active");
        
        $term.addClass("show-all");
	},

	attachEvents: function()
	{
		// DOM events
		this.$container.on('mouseenter', '.tabers-term.active', this.f.onMouseEnter );
		this.$container.on('mouseleave', '.tabers-term.active', this.f.onMouseLeave );
		$('body').on('click', null, this.f.onClick );
	},

	onClick: function(event)
	{
		this.hideAllTooltips();
	},
	
	onMouseLeave: function(event)
	{
		// Clear prevention of mouse movement from over-triggering
		clearTimeout(this.timeout);

		if (!$('.tooltip.tabers').is(":hover")) {
			this.hideAllTooltips();
		}
	},
	
	onMouseEnter: function(event)
	{
		this.hideAllTooltips();

		// Prevent mouse movement from over-triggering
		var t = this;
		this.timeout = setTimeout(
			function() {
				t.$target = R2V2.Utils.GetEventTarget(event, 'span').$;

				t.$target.attr({ rel: "tooltip" });	
				t.$target.tooltip({ trigger: 'click' });

				var text = t.$target.attr('data-original-title');
				if (text) {
					t.showTooltip(text);
					return;
				}

				var termId = t.$target.data('term-id');
				t.getTabersEntry(termId);
			}
		,100);
	},

	hideAllTooltips: function()
	{
		$("[rel='tooltip']").tooltip('hide');
	},

	showTooltip: function(text)
	{
		this.$target.tooltip('hide')
			.attr('data-original-title', text)
			.tooltip('fixTitle')
			.tooltip('show');

		$('.tooltip').addClass("tabers");
		$('.tooltip.tabers').on('mouseleave', this.f.onMouseLeave);
	},

	onGetTabersEntrySuccess: function(response)
	{
		var text = this.addSearchText(response.Content);

		this.showTooltip(text);

		// publish
		//$.PubSub(R2V2.PubSubMappings.Resource.Tabers.Success).publish(this.data);
	},

	onGetTabersEntryError: function(response)
	{
		// publish
		//$.PubSub(R2V2.PubSubMappings.Resource.Tabers.Error).publish(response);
	},    

	getTabersEntry: function(termId)
	{
		$.executeService({ url: this.termContentUrl, data: { termId: termId } }).then(this.f.onGetTabersEntrySuccess).fail(this.f.onGetTabersEntryError);
	},
	
	addSearchText: function(content)
	{
		var link = this.$target.closest("[href*='/search?q=']");
		if (link.length == 0) return content;

		var term = link.text().trim();
		
		return content
				.replace('tabers-search', 'tabers-search active')
				.replace(/%%term%%/g, term);
	}
});

/* ==========================================================================
    Resource Results
   ========================================================================== */
R2V2.Resource.Results = Class.extend({
    
    _loadingText: 'Loading...',

    init: function (config)
    {
        // config settings
        this.config = config || {};
        this.$container = this.config.$container;
        this.hasContent = this.config.hasContent;
        this.goTo = this.config.goTo;
        if (this.$container == null || this.$container.length === 0) return;

        // cache bound/proxy'd method calls
        this.f = {
            onHistorySet: $.proxy(this.onHistorySet, this),
            onResourceSuccess: $.proxy(this.onResourceSuccess, this),
            onResourceError: $.proxy(this.onResourceError, this),
            onBookmarkClick: $.proxy(this.onBookmarkClick, this)
        };
        
        // setup
        this.attachEvents();
    },

    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.History.Set).subscribe(this.f.onHistorySet);
        $.PubSub(R2V2.PubSubMappings.Resource.Success).subscribe(this.f.onResourceSuccess);
        $.PubSub(R2V2.PubSubMappings.Resource.Error).subscribe(this.f.onResourceError);
        
        // DOM events
        this.$container.on('click', '[data-history="bookmark"] a[href*=#]', this.f.onBookmarkClick);
    },

    onHistorySet: function (response)
    {
        var section = response.hash['section'],
            goTo = response.hash['goto'];
        
        if (section != null && section === this.section) return;
	 
        this.section = section;

        if (goTo) {
            this.goTo = goTo;
        }
	    
	    if (!this.hasContent) {
		    this.renderLoading();
	    }
    },
    
    onResourceSuccess: function (response)
    {
        this.data = response;

        // render
	    if(!this.hasContent) this.renderContent(this.data.Html);
	    
		// try and find the section and jump to that
        this.goToElement();
    },
    
    onResourceError: function (response)
    {
        this.data = response;

        // render
        this.renderError();
    },
    
    onBookmarkClick: function (event)
    {        
        var target = R2V2.Utils.GetEventTarget(event, 'a'); // get DOM and $ targets
        
        if (target.$.hasClass('trigger')) return; // "trigger" is used with UI enhancement plugins

	    var href = target.$.attr('href');

	    if (document.URL.indexOf(href, document.URL.length - href.length) === -1) return; // if current url does not end with href, allow default event

		event.preventDefault();

        var value = href.replace( /^#/ , ''), // get the url from the link's href attribute, stripping any leading #
            hash = $.deparam.fragment(value),
            params = value.split('=');

        if (params.length === 1)
        {
            this.goTo = params[0];
            this.goToElement();
            return;
        }

        this.goTo = hash['goto'];
        this.goToElement();

        if (hash['section'] === this.section)
        {
            $.PubSub(R2V2.PubSubMappings.History.Add).publish({ 'goto': hash['goto']});
            return;
        }
        $.PubSub(R2V2.PubSubMappings.History.Add).publish(hash, 2);
    },
    
    renderLoading: function (data)
    {
        this.$container.empty().append('<h2>' + this._loadingText + '</h2>');
    },
    
    renderContent: function (html)
    {
        // update content
        this.$container.empty().html(html);
    },
    
    renderError: function () {  },

    goToElement: function ()
    {
        // This is the equivalent of an anchor to content (ie. <a href="#someContent">go to some content</a>).
        // If the given ID exists on the DOM, scroll to the beginning of that element.
	
        if (this.goTo == null) return;

        var $element = $('#' + this.goTo);
        if ($element == null || $element.length === 0) {
	        //Make actual anchors work
	        $element = $('a[name="' + this.goTo + '"]');
	        if ($element == null || $element.length === 0) return;
        }
	
        if ($element.is('div.figure')) {
        	//Figures may have nested figures. We want to trigger the click event for the enlarge button of the selected (outermost) figure.
        	//This is the reason for the call to .last(). But we want to limit which buttons we consider (only enlarge buttons). 	-DRJ
			var $linkEnlarge = $element.find('a.btn-image-enlarge, a.btn-table-view, a.btn-fullsize-view').last();
			$element.hasClass('inline') == false && $linkEnlarge.hasClass('external') == false && $linkEnlarge.trigger('click'); // trigger click event
		}

        var offset = $element.offset();
        if (offset == null) return;

        R2V2.Elements.Window.scrollTop(offset.top - 96); // offset "fixed" menus
    }
});


/* ==========================================================================
    Resource Nav
   ========================================================================== */
R2V2.Resource.Nav = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        this.isbn = this.config.isbn;

        if (this.$container == null || this.$container.length === 0) return;
        
        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, {
            onResourceChanged: $.proxy(this.onResourceChanged, this),
            onResourceSuccess: $.proxy(this.onResourceSuccess, this)
        });

        // setup
        this.attachEvents();
    },

    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.Resource.Changed).subscribe(this.f.onResourceChanged);
        $.PubSub(R2V2.PubSubMappings.Resource.Success).subscribe(this.f.onResourceSuccess);
    },
    
    onResourceChanged: function (sectionId)
    {
        // check internal cache
        if (this._cache[sectionId] == null) return;
        
        // update from cache
        this.data = this._cache[sectionId];
        this.render();
    },

    onResourceSuccess: function (response)
    {
        // set data
        this.data = response.Navigation;
        // return if no data or already cached
        if (this.data == null || this.data.Current == null || this.data.Current.Id == null || this._cache[this.data.Current.Id]) return;
        
        // save to cache
        this.data.isbn = this.isbn;
        this._cache[this.data.Current.Id] = this.data;

        // build
        this.render();
    },
    
    render: function ()
    {
        // create
        this.nav = this._template(this.data);
        
        // add to DOM
        this.$container.html(this.nav);
    } 
});


/* ==========================================================================
    Resource Breadcrumb Nav
    extends: R2V2.Resource.Nav
   ========================================================================== */
R2V2.Resource.BreadcrumbNav = R2V2.Resource.Nav.extend({
	_cache: {},
	_template: _.template('<ul>{{ if (Book && Book.Name) { }}<li><a href="/resource/title/{{= isbn }}">{{= Book.Name }}</a></li>{{ } }}{{ if (Part && Part.Name && (Part.Id != Current.Id && Part.Name != Chapter.Name)) { }}<li>{{ if (Part.Id) { }}<a href="/resource/detail/{{= isbn }}/{{= Part.Id }}">{{= Part.Name }}</a>{{ } else { }}Part.Name{{ } }}</li>{{ } }}{{ if (Chapter && Chapter.Name && (Chapter.Id != Current.Id && Chapter.Name != Section.Name)) { }}<li>{{ if (Chapter.Id) { }}<a href="/resource/detail/{{= isbn }}/{{= Chapter.Id }}">{{= Chapter.Name }}</a>{{ } else { }}Chapter.Name{{ } }}</li>{{ } }}{{ if (Section && Section.Name) { }}<li>{{= Section.Name }}</li>{{ } }}</ul>')
});


/* ==========================================================================
    Resource Section Nav
    extends: R2V2.Resource.Nav
   ========================================================================== */
R2V2.Resource.SectionNav = R2V2.Resource.Nav.extend({
    
    _cache: {},
    _template: _.template('{{ if (Previous) { }}<div class="previous"><a href="{{ if (Previous.Id) { }}/resource/detail/{{= isbn }}/{{= Previous.Id }}{{ } else { }}/resource/title/{{= isbn }}{{ } }}">&lt;&lt; Previous</a></div>{{ } }}{{ if (Next && Next.Id) { }}<div class="next"><a href="/resource/detail/{{= isbn }}/{{= Next.Id }}">Next &gt;&gt;</a></div>{{ } }}'),
    
    init: function (config)
    {
        this.type = config.type;
        
        this.f = {
            onKeyboardNext: $.proxy(this.onKeyboardNext, this),
            onKeyboardPrev: $.proxy(this.onKeyboardPrev, this)
        };
        
        this._super(config);
    },

    attachEvents: function ()
    {
        R2V2.Elements.Document.on(R2V2.KeyboardMappings.Resource.Next, this.f.onKeyboardNext).on(R2V2.KeyboardMappings.Resource.Previous, this.f.onKeyboardPrev);

        this._super();
    },
    
    onKeyboardNext: function (event)
    {
        this.changePage($('.next a'));
    },
    
    onKeyboardPrev: function (event)
    {
        this.changePage($('.previous a'));
    },
    
    changePage: function ($link)
    {
        if ($link.length <= 0) return;

        var link = $link.get(0);
        this.type == 'title' || link.href.hash == null ? window.location = link.href : $.PubSub(R2V2.PubSubMappings.History.Add).publish(link.href.hash);
    }
});


/* ==========================================================================
    Resource Actions Menu
    extends: R2V2.ActionsMenu
   ========================================================================== */
R2V2.Resource.ActionsMenu = R2V2.ActionsMenu.extend({
    init: function (config)
    {
        this.config = config || {};
        this.config.self = R2V2.Resource.ActionsMenu;
        this.config.title = this.config.title || null;
        this.config.resourceId = this.config.resourceId || null;
        this.config.isbn = this.config.isbn || null;
        this.config.library = this.config.library || null;

        this._super();
    }
});


/* ==========================================================================
    Resource Actions Menu Panel
    extends: R2V2.ActionsMenu.Panel
   ========================================================================== */
R2V2.Resource.ActionsMenu.Panel = R2V2.ActionsMenu.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        this.title = this.config.title;
        this.resourceId = this.config.resourceId;
        this.isbn = this.config.isbn;
        this.library = this.config.library;
        this.sectionId = null;
        this.sectionTitle = null;

        if (this.$container == null || this.$container.length <= 0) return;

        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, { onResourceSuccess: $.proxy(this.onResourceSuccess, this) });

        // call parent method
        this._super();
        
        this.setup();
    },

    setup: function ()
    {
        // set "page" data
        this.params = { title: this.title, resourceId: this.resourceId, library: this.library, isbn: this.isbn };
        this.setData();
    },

    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.Resource.Success).subscribe(this.f.onResourceSuccess);

        // call parent method
        this._super();
    },

    onResourceSuccess: function (data)
    {
        if (data == null || data.Navigation == null || data.Navigation.Current == null || data.Navigation.Current.Id == null || data.Navigation.Section == null || data.Navigation.Section.Name == null) return;
        
        // set "page" data
        this.params = $.extend(this.$container.data('params'), { sectionId: data.Navigation.Current.Id, sectionTitle: data.Navigation.Section.Name });
        this.setData();
    },
    
    setData: function ()
    {
        this.$container.data('params', this.params);
    }
});


/* ==========================================================================
    Resource Actions Menu Search Panel
    extends: R2V2.Resource.ActionsMenu.Panel
   ========================================================================== */
R2V2.Resource.ActionsMenu.SearchPanel = R2V2.Resource.ActionsMenu.Panel.extend({
    
    // constructor in R2V2.Resource.ActionsMenu.Panel

    save: function (event)
    {
        event.preventDefault();

        window.location = this.$form.attr('action') + '?' + this.$form.serialize().replace('&', '#');
    }
});


/* ==========================================================================
    Resource Actions Menu TOC Panel
    extends: R2V2.Resource.ActionsMenu.Panel
   ========================================================================== */
R2V2.Resource.ActionsMenu.TocPanel = R2V2.Resource.ActionsMenu.Panel.extend({
    
    _activeCssClass: 'selected',
    
    init: function (config)
    {
        // cache bound/proxy'd method calls
        this.f = { onResourceChanged: $.proxy(this.onResourceChanged, this) };
        
        this._super(config);
    },

    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.Resource.Changed).subscribe(this.f.onResourceChanged);
        $.PubSub(R2V2.PubSubMappings.Resource.Success).subscribe(this.f.onResourceSuccess);
        $.PubSub(R2V2.PubSubMappings.Menu.Opened).subscribe(this.f.onOpen);
    },
    
    onResourceChanged: function (sectionId)
    {
        // set current section Id
        this.sectionId = sectionId;
        
        // simulate body click to close menu
        R2V2.Elements.Body.trigger($.Event('click.dismiss.togglemenu'));
    },
    
    onOpen: function ()
    {
        if (this.$targetParents == null || this.$targetParents.length === 0) return;

        // scroll container to target's header
        this.$container.scrollTop(this.$targetParents.first().prev().get(0).offsetTop);
    },
    
    onResourceSuccess: function(data)
    {
        if (this.accordion == null) this.accordion = this.$container.find('.accordion').data('accordion');
        
        this.$target && this.$target.removeClass(this._activeCssClass);
        this.$target = this.$container.find('a[href*=' + this.sectionId + ']').last().addClass(this._activeCssClass);
        this.$targetParents = this.$target.parents('.accordioncontent') || $();
        this.accordion.toggleGroup(this.$targetParents);
    }
});


/* ==========================================================================
    Resource Actions Menu Topic Panel
    extends: R2V2.Resource.ActionsMenu.Panel
   ========================================================================== */
R2V2.Resource.ActionsMenu.TopicPanel = R2V2.Resource.ActionsMenu.Panel.extend({
    
    _template: _.template('<strong>Other topics in this book related to this chapter</strong><ul>{{ _.each(topics, function(topic) { }}<li><a href="/Search?q={{= topic }}#within={{= isbn }}">{{= topic }}</a></li>{{ }); }}</ul>'),

    // constructor in R2V2.Resource.ActionsMenu.Panel
    
    onResourceSuccess: function (response)
    {
        // clear container
        this.$container.empty();
        
        // check data
        if (response == null || response.Topics == null || response.Topics.length == 0) return;

        // build html
        var html = this._template({ isbn: this.isbn, topics: response.Topics });
        
        // insert html
        this.$container.append(html);
    }
});


/* ==========================================================================
    Resource Actions Menu Email Panel
    extends: R2V2.Resource.ActionsMenu.Panel
   ========================================================================== */
R2V2.Resource.ActionsMenu.EmailPanel = R2V2.Resource.ActionsMenu.Panel.extend({
    
    // constructor in R2V2.Resource.ActionsMenu.Panel

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
        
        // if set, add "section"
        if (this.params && this.params.sectionId) formData += '&Section=' + this.params.sectionId;

        // execute service call
        $.executeService({ url: dataUrl, data: formData, type: 'POST', contentType: R2V2.ServiceExecutionOptions.ContentType.DEFAULT }).then(this.f.onSaveDataSuccess).fail(this.f.onSaveDataError);
    }
});


/* ==========================================================================
    Resource Actions Menu Export Citation Panel
    extends: R2V2.Resource.ActionsMenu.Panel
   ========================================================================== */
R2V2.Resource.ActionsMenu.ExportCitationPanel = R2V2.Resource.ActionsMenu.Panel.extend({
    
    // constructor in R2V2.Resource.ActionsMenu.Panel
    
    init: function (config)
    {
        this.f = { onFormatSelect: $.proxy(this.onFormatSelect, this) };

        // call parent method
        this._super(config);
    },

    attachEvents: function ()
    {
        this.$container.find('ul.radio-group').on('change', this.f.onFormatSelect);

        // call parent method
        this._super();
    },
    
    onFormatSelect: function (event)
    {
        // get DOM and $ targets
        var target = R2V2.Utils.GetEventTarget(event, 'input');
        
        // toggle visibility depending on val
        this.$target && this.$target.hide();
        this.$target = $('#' + target.DOM.value).show();
    },
    
    onFormSave: function (event)
    {
        // allow default form post in order to download file
        
        this.closeAll();
    }
});


/* ==========================================================================
    Resource Save Image Panel
    extends: R2V2.Panel
   ========================================================================== */
R2V2.Resource.SaveImage = R2V2.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) {
            return;
        }
        
        this.resourceId = this.config.resourceId || null;
        this.isbn = this.config.isbn || null;
        this.library = this.config.library || null;
        this.resourceData = $.param({ resourceId: this.config.resourceId, library: this.config.library, isbn: this.config.isbn });
        
        // call parent method
        this._super();
    },

    attachEvents: function ()
    {
        // DOM events
        this.$container.on({ 'show': this.f.onShow });
        this.$container.on({ 'shown': this.f.onOpen, 'hidden': this.f.onClose });
    },

    onShow: function (e)
    {
        $(e.relatedTarget).closest('.actions-figure').before(this.$container);
    },

    save: function (event)
    {
        event.preventDefault();

        this.trigger = this.$container.data('trigger');
        if (this.trigger == null) {
            return;
        }

        // get resource data
        this.$asset = this.trigger.parents('[data-user-content-image]');
        this.assetData = this.$asset.data('user-content-image');

        // get form action and data
        var dataUrl = this.$form.attr('action'),
            formData = this.assetData + '&' + this.resourceData + '&' + this.$form.serialize();

        // execute service call
        $.executeService({ url: dataUrl, data: formData }).then(this.f.onSaveDataSuccess).fail(this.f.onSaveDataError);
    }
});


/* ==========================================================================
    Resource Enlarge Image Panel
    extends: R2V2.Panel
   ========================================================================== */
R2V2.Resource.EnlargeImage = R2V2.Panel.extend({
    init: function (config)
    {
        this.config = config || {};

        this.$modal = this.config.$modal;
        this.$button = this.config.$button;
        this.$modalHeader, this.$modalBody;

        if (this.$button == null || this.$button.length <= 0) {
            return;
        }

        this.attachEvents();
    },

    attachEvents: function ()
    {
        var self = this;

        this.$button.on('click', function (event) {
            event.preventDefault();

            var $target = $(event.target);

            $target.closest('.figure').find('.actions-figure').before(self.$modal);
            
            self.$modal.modal();

            self.$modalHeader = self.$modalHeader || self.$modal.find('.modal-header h3');
            self.$modalBody = self.$modalBody || self.$modal.find('.modal-body');

            var headerText = $target.closest('.figure').find('.figcaption p').text();
            var $body = $target.closest('.figure').find('.figimage img').clone();

            self.renderHeader(headerText);
            self.renderBody($body);
        });
    },

    renderHeader: function (text)
    {
        var MAXLENGTH = 65;

        if (text.length < 1) {
            return;
        }

        if (text.length > MAXLENGTH) {
            text = jQuery.trim(text).substring(0, MAXLENGTH).split(" ").slice(0, -1).join(" ") + "...";
        }

        this.$modalHeader.text(text);
    },

    renderBody: function ($content)
    {
        this.$modalBody.empty();
        this.$modalBody.html($content);
    }
});

/* ==========================================================================
    Resource Direct Link Image Panel
    extends: R2V2.Panel
   ========================================================================== */
R2V2.Resource.DirectLinkImage = R2V2.Panel.extend({
    init: function (config)
    {
        this.config = config || {};

        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) {
            return;
        }

        this.$imageDirectLinkContainer = this.config.$imageDirectLinkContainer;
        this.$imageDirectLink = this.$imageDirectLinkContainer.find("#imageDirectLink");
        this.$directLinkTitle = this.$imageDirectLinkContainer.find("h3");// this.config.directLinkTitle;

        this.proxyPrefix = this.config.proxyPrefix;
        
        this.addDirectLinks();
        
        this.f = {
            onClick: $.proxy(this.onClick, this),
            onDirectLinkOpen: $.proxy(this.onDirectLinkOpen, this),
            onCopyLinkToClipboard: $.proxy(this.onCopyLinkToClipboard, this),
            onDirectLinkNavigate: $.proxy(this.onDirectLinkNavigate, this),
            onHidden: $.proxy(this.onHidden, this),
        };
        
        this.attachEvents();
    },

    addDirectLinks: function ()
    {
        this.$container.find(".btn-image-enlarge").each(function () {
            var enlargeButton = $(this).parent();
            var directLinkHtml = '<li><a class="ir btn-image-direct" href="#imageDirectLinkContainer" data-toggle="modal">Direct Link Image</a></li>';
            
            enlargeButton.after(directLinkHtml);
        });
    },

    attachEvents: function ()
    {
        this.$container.on('click', '.btn-image-direct', this.f.onDirectLinkOpen);

        this.$imageDirectLinkContainer.on('click', '#btnCopyLinkToClipboard', this.f.onCopyLinkToClipboard);
        this.$imageDirectLinkContainer.on('click', '#imageDirectLink', this.f.onDirectLinkNavigate);
    },

    onDirectLinkOpen: function (event)
    {
        var $target = R2V2.Utils.GetEventTarget(event, 'a').$;
        var $target2 = R2V2.Utils.GetEventTarget(event, 'div').$;
        var divContainer = $target2.parent();

        $target.closest('.actions-figure').before(this.$imageDirectLinkContainer);

        var href = this.proxyPrefix + R2V2.Utils.GetCurrentUrlWithoutHash() + "#goto=" + divContainer.attr('id');

        this.$imageDirectLink
			.attr('href', href)
			.text(href);
        
        var headerText = $target.closest('.figure').find('.figcaption p').text();

        this.renderHeader(headerText);
    },
    
    renderHeader: function (text)
    {
        var MAXLENGTH = 65;

        if (text.length < 1) {
            return;
        }

        if (text.length > MAXLENGTH) {
            text = jQuery.trim(text).substring(0, MAXLENGTH).split(" ").slice(0, -1).join(" ") + "...";
        }

        this.$directLinkTitle.text(text);
    },

    onDirectLinkNavigate: function ()
    {
        var $buttonCancel = this.$imageDirectLinkContainer.find("#btnCancel");
        $buttonCancel.trigger('click');
    },

    onCopyLinkToClipboard: function ()
    {
        var href = this.$imageDirectLink.attr('href');
        R2V2.Utils.CopyToClipboard(href);
    }
});


/* ==========================================================================
    Q and A
   ========================================================================== */
R2V2.Resource.Qanda = R2V2.Panel.extend({
	init: function (config) {
		this.config = config || {};
		this.$container = this.config.$container;
		if (this.$container == null || this.$container.length <= 0) return;

		this.$qandaSets = this.$container.find('.qandaset');

		this.$qandaSets.each(function () {
			var $qandaset = $(this);
			var $answer = $qandaset.find('tr.answer');
			var hasHiddenAnswer = $answer.hasClass('hiddenanswer') || false;

			if (hasHiddenAnswer) {
				$qandaset.after('<div class="qandaset-view-answer"><a class="ir btn-answer-view">View Answer</a></div>');
			}
		});

		this.f = {
			onClick: $.proxy(this.onClick, this)
		};

		this.attachEvents();
	},

	attachEvents: function () {
		// DOM events
		this.$container.on('click', 'a.btn-answer-view', this.f.onClick);
	},

	onClick: function (event) {
		var $target = R2V2.Utils.GetEventTarget(event, 'a').$;
		var $div = $target.closest('div.qandaset-view-answer');
		var $qandaset = $div.prevAll('.qandaset:first');
		var $answer = $qandaset.find('tr.answer');

		$answer.toggle();
		$div.toggleClass('inline');
	}
});


/* ==========================================================================
    Resource Video Player
   ========================================================================== */
R2V2.Resource.Video = Class.extend({
	init: function(config)
	{
		this.config = config || {};
		this.$container = this.config.$container;
		if (this.$container == null || this.$container.length <= 0) return;

		this.$videoContainer = this.config.$videoContainer;
		this.$videoDirectLinkContainer = this.config.$videoDirectLinkContainer;
		this.$videoDirectLink = this.$videoDirectLinkContainer.find("#videoDirectLink");
		this.$videoTitle = this.$videoContainer.find("h3");
		this.isbn = this.config.isbn;
		this.section = this.config.section;
		this.mediaBaseUrl = this.config.mediaBaseUrl;
		this.logPlayUrl = "/resource/logvideoplay";
		this.proxyPrefix = this.config.proxyPrefix;

		this.addDirectLinks();

		// cache proxy'd method calls
		this.f = {
			onClick: $.proxy(this.onClick, this),
			onDirectLinkOpen: $.proxy(this.onDirectLinkOpen, this),
			onCopyLinkToClipboard: $.proxy(this.onCopyLinkToClipboard, this),
			onDirectLinkNavigate: $.proxy(this.onDirectLinkNavigate, this),
			onHidden: $.proxy(this.onHidden, this),
			onLogPlaySuccess: $.proxy(this.onLogPlaySuccess, this),
            onLogPlayError: $.proxy(this.onLogPlayError, this),
			onResourceTimeoutExpired: $.proxy(this.onResourceTimeoutExpired, this)
		};

		this.attachEvents();
	},

	attachEvents: function()
    {
        $.PubSub(R2V2.PubSubMappings.Resource.TimeoutExpired).subscribe(this.f.onResourceTimeoutExpired);

		// DOM events
		this.$container.on('click', '.video-link', this.f.onClick);
		this.$container.on('click', '.video-direct-link-open', this.f.onDirectLinkOpen);
		this.$videoDirectLinkContainer.on('click', '#btnCopyLinkToClipboard', this.f.onCopyLinkToClipboard);
		this.$videoDirectLinkContainer.on('click', '#videoDirectLink', this.f.onDirectLinkNavigate);
		this.$videoContainer.on({'hidden': this.f.onHidden });
	},

	onDirectLinkOpen: function (event)
    {
        var $target = R2V2.Utils.GetEventTarget(event, 'a').$;
		var $videoLink = $target.parent().find('.video-link');

		var videoTitle = $.trim($videoLink.text());
		var videoLinkId = $videoLink.attr("id");

		var $videoDirectLinkTitle = this.$videoDirectLinkContainer.find("#videoDirectLinkTitle");
		$videoDirectLinkTitle.text(videoTitle);

		var href = this.proxyPrefix + R2V2.Utils.GetCurrentUrlWithoutHash() + "#goto=" + videoLinkId;
		this.$videoDirectLink
			.attr('href', href)
			.text(href);

		$target.parent().before(this.$videoDirectLinkContainer);
	},

	onDirectLinkNavigate: function () {
		var $buttonCancel = this.$videoDirectLinkContainer.find("#btnCancel");
		$buttonCancel.trigger('click');
	},

	onCopyLinkToClipboard: function()
	{
		var href = this.$videoDirectLink.attr('href');
		R2V2.Utils.CopyToClipboard(href);
	},

	logPlay: function()
	{
		$.executeService({ url: this.logPlayUrl, data: { isbn: this.isbn, section: this.section, mediaUrl: this.mediaUrl } }).then(this.f.onLogPlaySuccess).fail(this.f.onLogPlayError);
	},

	onLogPlaySuccess: function (response)
	{
		this.data = response;

		// publish
		$.PubSub(R2V2.PubSubMappings.Log.Success).publish(this.data);
	},

	onLogPlayError: function (response) {
		// publish
		$.PubSub(R2V2.PubSubMappings.Log.Error).publish(response);
	},

	addDirectLinks: function()
	{
		this.$container.find(".video-link").each(function() {
			var $videoLink = $(this);
			var directLinkHtml = "<a class='video-direct-link-open' href='#videoDirectLinkContainer' data-toggle='modal'>" +
				"<span class='glyphicon glyphicon-link video-direct-link-icon' title='Direct Link'></span></a>";
			$videoLink.after(directLinkHtml);
		});
	}
});


/* ==========================================================================
    Resource Flowplayer Video Player
   ========================================================================== */
R2V2.Resource.Flowplayer = R2V2.Resource.Video.extend({
	init: function (config) {
		this.config = config || {};

		this._super(config);

		this.$flowplayerContainer = this.$videoContainer.find("#flowplayerContainer");

		// cache proxy'd method calls
		this.f.onFlowplayerVideoRead = $.proxy(this.onFlowplayerVideoReady, this);

		this.attachFlowplayerEvents();

		if (window.flowplayer) {
			window.flowplayer.conf.embed = false;
			window.flowplayer.conf.keyboard = false;
			window.flowplayer.conf.key = this.config.flowplayerKey;
		}
	},

    attachFlowplayerEvents: function () {
		// DOM events
		this.$flowplayerContainer.on({ 'ready': this.f.onFlowplayerVideoReady });
	},

    onClick: function (event) {
		var $target = R2V2.Utils.GetEventTarget(event, 'a').$;

		$target.parent().before(this.$videoContainer);

		this.mediaId = $target.data('file');
		this.mediaName = this.mediaId;
		this.mediaUrl = this.mediaBaseUrl + '/' + this.isbn + '/' + this.mediaName;

		this.$flowplayerContainer.css('display', 'block');
		
		this.mediaTitle = $target.text();

		this.setVideo();
	},

	onResourceTimeoutExpired: function () {
	    this.$flowplayerContainer.flowplayer().pause();
	},

	onHidden: function () {
		this.$flowplayerContainer.flowplayer().stop();
	},

	onFlowplayerVideoReady: function (event) {
		this.$flowplayerContainer.flowplayer().play();
		this.logPlay();
	},

	setVideo: function () {
		this.$videoTitle.text(this.mediaTitle);

		this.$flowplayerContainer.flowplayer({
			playlist: [[{ mp4: this.mediaUrl }]]
		});
		this.$flowplayerContainer.flowplayer().load(this.mediaUrl);
	}
});


/* ==========================================================================
    Resource Ooyala Video Player
   ========================================================================== */
R2V2.Resource.Ooyala = R2V2.Resource.Video.extend({
	init: function (config) {
		this._super(config);

		this.$ooyalaContainer = this.$videoContainer.find("#ooyalaContainer");
		this.ooyalaPCode = this.config.ooyalaPCode;
		this.ooyalaPlayerBrandingId = this.config.ooyalaPlayerBrandingId;
	},

	onClick: function (event) {
		var $target = R2V2.Utils.GetEventTarget(event, 'a').$;
		this.mediaId = $target.data('file');

		$target.parent().before(this.$videoContainer);

		if (this.mediaId.slice(0, 7) == "ooyala:") {
			this.mediaName = this.mediaId.slice(7);
			this.mediaUrl = this.mediaId;

			this.$ooyalaContainer.css('display', 'block');
		}

		this.mediaTitle = $target.text();

		this.setVideo();
	},

	onResourceTimeoutExpired: function () {
	    if (!this.ooyalaPlayer) {
	        return;
	    }
	    this.ooyalaPlayer.pause();
    },

	onHidden: function () {
		if (!this.ooyalaPlayer) {
			return;
		}
		this.ooyalaPlayer.pause();
	},

	setVideo: function () {
		this.$videoTitle.text(this.mediaTitle);

		var that = this;
		var play = function () {
			that.ooyalaPlayer.setEmbedCode(that.mediaName);
			that.logPlay();
		}
		
		if (window.OO) {
			if (!that.ooyalaPlayer) {
				OO.ready(function () {
					var playerParam = {
						"pcode": that.ooyalaPCode,
						"playerBrandingId": that.ooyalaPlayerBrandingId,
						"skin": {
							// Config contains the configuration setting for player skin. Change to your local config when necessary.
							"config": '/_Static/Scripts/packages/ooyala-4.12.6/skin-plugin/skin.json'
						}
					};

					that.ooyalaPlayer = OO.Player.create('ooyalaplayer', that.mediaName, playerParam);
					play();
				});
			} else {
				play();
			}
		}
	}
});


/* ==========================================================================
    Resource Turnaway
   ========================================================================== */
R2V2.Resource.Turnaway = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) return;

        // cache DOM elements
        this.$form = this.$form || this.$container.find('form');
        this.action = this.$form.attr('action');

        // cache proxy'd method calls
        this.f = {
            onShow: $.proxy(this.onShow, this),
            onSaveDataSuccess: $.proxy(this.onSaveDataSuccess, this),
            onSaveDataError: $.proxy(this.onSaveDataError, this)
        };

        this.attachEvents();
    },

    attachEvents: function ()
    {
        // DOM events
        this.$container.on({ 'show': this.f.onShow });
    },

    onShow: function (event)
    {
        $.executeService({ url: this.action }).then(this.f.onSaveDataSuccess).fail(this.f.onSaveDataError);
    },
    
    onSaveDataSuccess: function (response)
    {
        // GA Track
    },
    
    onSaveDataError: function (response)
    {
        // GA Track
    }
});


/* ==========================================================================
    Resource Timeout
   ========================================================================== */
R2V2.Resource.Timeout = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) return;
        this.titleUrl = this.config.titleUrl;
        this.displayTimeInSeconds = this.config.displayTimeInSeconds;
        this.lockTimeInMinutes = this.config.lockTimeInMinutes;
        this.displayTime = this.displayTimeInSeconds * 1000; // normalize to milliseconds
        this.lockTime = this.lockTimeInMinutes * 60000; // normalize to milliseconds

        // cache DOM elements
        this.$countdownTime = this.$container.find(this.config.countdownTimeSelector);
        
        // cache proxy'd method calls
        this.f = {
            onResourceSuccess: $.proxy(this.onResourceSuccess, this),
            onShow: $.proxy(this.onShow, this),
            onHide: $.proxy(this.onHide, this),
            show: $.proxy(this.show, this),
            display: $.proxy(this.display, this),
            redirect: $.proxy(this.redirect, this)
        };

        this.attachEvents();
        this.set(this.f.show, this.lockTime);
    },
    
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.Resource.Success).subscribe(this.f.onResourceSuccess);
        
        // DOM events
        this.$container.on({ 'show': this.f.onShow, 'hide': this.f.onHide });
    },
    
    onResourceSuccess: function (event)
    {
        this.reset();
    },

    onShow: function (event)
    {
        // timer
        this.clear();
        this.set(this.f.redirect, this.displayTime);
        
        // countdown time display
        this.countdownTime = this.displayTimeInSeconds;
        window.setTimeout(this.f.display, 0);
        this.interval = window.setInterval(this.f.display, 1000);
    },
    
    onHide: function (event)
    {
        this.reset();
    },
    
    set: function (method, time)
    {
        this.timer = window.setTimeout(method, time);
    },
    
    clear: function ()
    {
        window.clearTimeout(this.timer);
        window.clearInterval(this.interval);
    },
    
    reset: function ()
    {
        this.clear();
        this.$countdownTime.html(this.displayTimeInSeconds);
        this.set(this.f.show, this.lockTime);
    },
    
    show: function ()
    {
        this.$container.modal('show');
        $.PubSub(R2V2.PubSubMappings.Resource.TimeoutExpired).publish();
    },

    display: function ()
    {
        this.$countdownTime.html(this.countdownTime--);
    },

    redirect: function ()
    {
        this.clear();
        window.location = this.titleUrl;
    }
});


/* ==========================================================================
    Resource Print Logging
   ========================================================================== */
R2V2.Resource.PrintLogging = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.static = this.config.static || false;
        this.endpoint = this.config.endpoint;
        
        // error handler
        if (this.endpoint == null) return;
        
        // cache elements
        this.$head = $('head');

        // cache bound/proxy'd method calls
        this.f = { onResourceChanged: $.proxy(this.onResourceChanged, this) };
        
        // setup
        if (this.static)
        {
            this.onResourceChanged(null);
        }
        else
        {
            this.attachEvents();
        }
    },

    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.Resource.Changed).subscribe(this.f.onResourceChanged);
    },
    
    onResourceChanged: function (sectionId)
    {
        var section = (sectionId == null) ? '' : '&section=' + sectionId;
        this.$style && this.$style.remove();
        this.$style = $('<style type="text/css" media="print">footer:after { content: url(' + this.endpoint + section + '); }</style>');
        this.$head.append(this.$style);
    }
});


/* ==========================================================================
    Resource See More/Less
   ========================================================================== */
R2V2.Resource.SeeMoreLess = Class.extend({
	init: function(config) {
		this.config = config || {};

		this.showChar = 300;  // How many characters are shown by default
		this.ellipsestext = '...&nbsp;&nbsp;';
		this.moretext = 'see more';
		this.lesstext = 'see less';
		this.runAttachEvents = false;
		this.$moreArray = this.config.$moreArray;
		if (!this.$moreArray) {
			return;
		}
		//alert('this.$moreArray:' + this.$moreArray);

		//alert('runAttachEvents: ' + this.runAttachEvents);
		this.truncateText();
		//alert('runAttachEvents: ' + this.runAttachEvents);
		if (this.runAttachEvents) {
			this.f = _.extend(this.f || {}, { onShowClick: $.proxy(this.onShowClick, this) });

			this.attachEvents();
		}
    },

	truncateText: function () {
		var that = this;
		this.$moreArray.each(function () {
			var content = $(this).html();
			if (content.length > that.showChar + 20) {
				var baseHideText = content.substr(that.showChar, content.length - that.showChar);
				var firstSpaceCount = baseHideText.indexOf(' ');
				that.showChar = firstSpaceCount + that.showChar;

				var showText = content.substr(0, that.showChar);
				var hideText = content.substr(that.showChar, content.length - that.showChar);
                var html = showText + '<span class="moreellipses">' + that.ellipsestext + '&nbsp;</span><span class="morecontent"><span id="morecontent">' + hideText + '</span>&nbsp;&nbsp;<a href="" class="morelink" role="button" aria-expanded="false" aria-controls="morecontent">' + that. moretext + '</a></span>';
				$(this).html(html);
				that.$morecontent = $('.morelink');
				that.runAttachEvents = true;
			}
		});
    },

	onShowClick: function () {
        if (this.$morecontent.hasClass('less')) {
            this.$morecontent
                .removeClass('less')
                .attr('aria-expanded', false)
                .html(this.moretext);
		} else {
            this.$morecontent
                .addClass('less')
                .attr('aria-expanded', true)
                .html(this.lesstext);
        }

		this.$morecontent.parent().prev().toggle();
        this.$morecontent.prev().toggle();

		return false;
    },

	attachEvents: function () {
		this.$morecontent.on('click', this.f.onShowClick);
	}
});