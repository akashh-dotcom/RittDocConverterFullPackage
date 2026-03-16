/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };
R2V2.Admin = { };

/* ==========================================================================
    Admin Utilities
   ========================================================================== */
R2V2.Admin.Utilities = {
    Transfer: function(id, value) {
        $(id).val(value);
    },
    
    TransferMax: function(id, value) {
        if (value == 0) {
            $(id).val(255);
        } 
    },

    PeriodTab: function (e, id) {
        var code = e.which;
        if (code == 46) {
            e.preventDefault();
            $(id).select();
        }
    }

};

/* ==========================================================================
    Admin Actions Menu
    extends: R2V2.ActionsMenu
   ========================================================================== */
R2V2.Admin.ActionsMenu = R2V2.ActionsMenu.extend({
    init: function (config)
    {
        this.config = config || {};
        this.config.self = R2V2.Admin.ActionsMenu;
        
        this._super();
    }
});


/* ==========================================================================
    Admin Actions Menu Search Panel
    extends: R2V2.ActionsMenu.Panel
   ========================================================================== */
R2V2.Admin.ActionsMenu.SearchPanel = R2V2.ActionsMenu.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) return;
        
        // call parent method
        this._super();
    },
    
    save: function (event)
    {
        return;
    }
});


/* ==========================================================================
    Admin Actions Menu Shopping Cart Panel
    extends: R2V2.ActionsMenu.Panel
   ========================================================================== */
R2V2.Admin.ActionsMenu.ShoppingCart = R2V2.ActionsMenu.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        if (this.$container == null || this.$container.length <= 0) return;

        $(document).ready(function () {
            $('.reseller-link').hide();
            $("#reseller-dropdown").on('change', function () {
                if (this.value == '') {
                    $('.reseller-link').hide();
                }
                else {
                    $('.reseller-link').show();
                    var link = $('.reseller-link').attr("href");
                    var resellerParameter = '?resellerId=';
                    var containsResellerId = link.includes(resellerParameter);
                    if (containsResellerId) {
                        $('.reseller-link').attr("href", link.substring(0, link.indexOf(resellerParameter)) + resellerParameter + this.value);
                    }
                    else {
                        $('.reseller-link').attr("href", link + resellerParameter + this.value);
                    }
                }
            });
        });
        /*
        Add the actions for JS in here for dropdown
        */
    }
});


/* ==========================================================================
    Admin Add to Collection
    extends: R2V2.Panel
   ========================================================================== */
R2V2.Admin.AddToCollection = R2V2.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$parent = this.config.$parent;
        this.trigger = this.config.trigger;
        
        if (this.$parent == null || this.$parent.length <= 0 || this.trigger == null) return;

        // cache method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this),
            onGetDataSuccess: $.proxy(this.onGetDataSuccess, this),
            onGetDataError: $.proxy(this.onGetDataError, this),
            onOpen: $.proxy(this.onOpen, this),
            onClose: $.proxy(this.onClose, this),
            onLicenseQuantityChange: $.proxy(this.onLicenseQuantityChange, this),
            onFormSave: $.proxy(this.onFormSave, this),
            onFormCancel: $.proxy(this.onFormCancel, this),
            onSaveDataSuccess: $.proxy(this.onSaveDataSuccess, this),
            onSaveDataError: $.proxy(this.onSaveDataError, this)
        };

        this.attachEvents();
    },

    attachEvents: function ()
    {
        this.$parent.on('click', this.trigger, this.f.onTriggerClick);
        R2V2.Elements.Body.on('keyup', '#collection-add form input', this.f.onLicenseQuantityChange);
    },

    onTriggerClick: function (event)
    {
        event.preventDefault();
        
        // get link
        var target = R2V2.Utils.GetEventTarget(event, 'a');

        // get data
        $.executeService({ url: target.DOM.href, responseDataType: R2V2.ServiceExecutionOptions.Format.HTML }).then(this.f.onGetDataSuccess).fail(this.f.onGetDataError);
    },

    onGetDataSuccess: function (response)
    {
        this.render(response);
    },
    
    onGetDataError: function (response) { },
    
    onLicenseQuantityChange: function (event) 
    {
        var licenseCount = R2V2.Utils.GetEventTarget(event, 'input'),
            $salePrice = this.$container.find('.saleprice'),
            $cartItemTotal = this.$container.find('.cart-item-total');

        var salePrice = Number($salePrice.text().replace(/[^0-9\.]+/g, ''));
        var cartItemTotal = licenseCount.$.val() * salePrice;

        $cartItemTotal.text('$' + cartItemTotal.toFixed(2));
    },

    onFormSave: function (event)
    {
        event.preventDefault();
        
        // get form
        var form = R2V2.Utils.GetEventTarget(event, 'form'),
            emptyFields = form.$.find(':input:visible:enabled:not(button)[value=""], :input:visible:enabled:not(button)[value="0"]');
            
        // check for "empty" fields
        if (emptyFields.length > 0) return;
        
        // post data
        $.executeService({ url: form.DOM.action, data: form.$.serialize(), type: 'POST', contentType: R2V2.ServiceExecutionOptions.ContentType.DEFAULT }).then(this.f.onSaveDataSuccess).fail(this.f.onSaveDataError);
    },
    
    onSaveDataSuccess: function (response)
    {
        // reload page
        window.location = document.URL;
    },
    
    onSaveDataError: function (response)
    {
        this.$container && (this.$container.modal('hide') && this.$container.remove()); // if already have modal in cache, remove it
    },

    render: function (data) 
    {
        this.$container && (this.$container.modal('hide') && this.$container.remove()); // if already have modal in cache, remove it
        this.$container = $(data); // modal $
        this.$form = this.$container.find('form');
        this.$parent.after(this.$container); // insert modal into DOM
        this.$container.on({ 'shown': this.f.onOpen, 'hidden': this.f.onClose }); // attach events
        this.$container.modal(); // open modal
    }
});


R2V2.Admin.AddToCollection2 = R2V2.Panel.extend({
    init: function (config) {
        this.config = config || {};
        this.$parent = this.config.$parent;
        this.trigger = this.config.trigger;

        if (this.$parent == null || this.$parent.length <= 0 || this.trigger == null) return;

        // cache method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this),
            onTriggerClickSave: $.proxy(this.onTriggerClickSave, this),
            onGetDataSuccess: $.proxy(this.onGetDataSuccess, this),
            onSaveDataSuccess: $.proxy(this.onSaveDataSuccess, this),
            onGetDataError: $.proxy(this.onGetDataError, this),
            onOpen: $.proxy(this.onOpen, this),
            onClose: $.proxy(this.onClose, this),
            onLicenseQuantityChange: $.proxy(this.onLicenseQuantityChange, this)
        };

        this.attachEvents();
    },
    attachEvents: function () {
        this.$parent.on('click', this.trigger, this.f.onTriggerClick);
        R2V2.Elements.Body.on('keyup', '#collection-add input', this.f.onLicenseQuantityChange);
    },
    onOpen: function() {
        // console.log("onOpen");

        var $addToCart = this.$container.find('.addToCartLink'),
            $addToSavedCart = this.$container.find('.addToSavedCartLink');

        if ($addToCart && $addToCart.length > 0) {
            $addToCart.click(this.f.onTriggerClickSave);
        }

        if ($addToSavedCart && $addToSavedCart.length > 0) {
            $addToSavedCart.click(this.f.onTriggerClick);
        }

        if (this.$container) {
            var $savedCartLinks = this.$container.find('.savedCartLink');
            if ($savedCartLinks && $savedCartLinks.length > 0) {
                // console.log($savedCartLinks);
                var that = this;
                $savedCartLinks.each(function (index) {
                    $(this).click(that.f.onTriggerClickSave);
                });
            }
        }
    },
    onTriggerClick: function (event) {
        event.preventDefault();
        // get link
        var target = R2V2.Utils.GetEventTarget(event, 'a');
        // get data
        // console.log(target.DOM.href);
        $.executeService({ url: target.DOM.href, responseDataType: R2V2.ServiceExecutionOptions.Format.HTML }).then(this.f.onGetDataSuccess).fail(this.f.onGetDataError);
    },
    onTriggerClickSave: function (event) {
        // console.log("onTriggerClickSave");
        event.preventDefault();
        // get link
        var target = R2V2.Utils.GetEventTarget(event, 'a');
        // get data
        // console.log(target.DOM.href);
        $.executeService({ url: target.DOM.href, responseDataType: R2V2.ServiceExecutionOptions.Format.HTML }).then(this.f.onSaveDataSuccess).fail(this.f.onGetDataError);
    },
    onGetDataSuccess: function (response) {
        // console.log("onGetDataSuccess");
        this.render(response);
    },
    onSaveDataSuccess: function (response) {
        // console.log("onSaveDataSuccess");
        // console.log(response);
        // console.log(document.URL);
        window.location = document.URL;
    },
    onGetDataError: function (response) {},
    onLicenseQuantityChange: function (event) {
        var licenseCount = R2V2.Utils.GetEventTarget(event, 'input'),
            $salePrice = this.$container.find('.saleprice'),
            $cartItemTotal = this.$container.find('.cart-item-total'),
            $addToCart = this.$container.find('.addToCartLink'),
            $addToSavedCart = this.$container.find('.addToSavedCartLink');

        var salePrice = Number($salePrice.text().replace(/[^0-9\.]+/g, ''));
        var cartItemTotal = licenseCount.$.val() * salePrice;

        $cartItemTotal.text('$' + cartItemTotal.toFixed(2));

        if ($addToCart && $addToCart.attr("href")) {
            var link = $addToCart.attr("href");
            link = link.replace(/(NumberOfLicenses=)\d*/i, 'NumberOfLicenses=' + licenseCount.$.val());
            $addToCart.attr("href", link);
            $addToCart.click(this.f.onTriggerClickSave);
        }

        if ($addToSavedCart && $addToSavedCart.attr("href")) {
            var link2 = $addToSavedCart.attr("href");
            link2 = link2.replace(/(NumberOfLicenses=)\d*/i, 'NumberOfLicenses=' + licenseCount.$.val());
            $addToSavedCart.attr("href", link2);
            $addToSavedCart.click(this.f.onTriggerClick);
        }
    },
    render: function (data) {
        // console.log("render");
        this.$container && (this.$container.modal('hide') && this.$container.remove()); // if already have modal in cache, remove it
        this.$container = $(data); // modal $
        this.$form = this.$container.find('form');
        this.$parent.after(this.$container); // insert modal into DOM
        this.$container.on({ 'shown': this.f.onOpen, 'hidden': this.f.onClose }); // attach events
        this.$container.modal(); // open modal
    }
});

/* ==========================================================================
    Admin Add to PDA Collection
    extends: R2V2.Panel
   ========================================================================== */
R2V2.Admin.PdaCollection = R2V2.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$parent = this.config.$parent;
        this.triggers = this.config.triggers;

        if (this.$parent == null || this.$parent.length <= 0 || this.triggers == null) return;

        // cache method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this),
            onSuccess: $.proxy(this.onSuccess, this),
            onError: $.proxy(this.onError, this),
            onOpen: $.proxy(this.onOpen, this),
            onClose: $.proxy(this.onClose, this)
        };

        this.attachEvents();
    },

    attachEvents: function ()
    {
        _.each(this.triggers, function(trigger) {
            this.$parent.on('click', trigger, this.f.onTriggerClick);
        }, this);
    },

    onTriggerClick: function (event)
    {
        event.preventDefault();

        // get link
        var target = R2V2.Utils.GetEventTarget(event, 'a');

        // get page to load into modal
        this.result({ url: target.DOM.href }).then(this.f.onSuccess).fail(this.f.onError);
    },

    onSuccess: function (response)
    {
        this.render(response);
    },

    onError: function(response) {
    },
    onOpen: function() {
    },

    onClose: function ()
    {
        this.$container.remove();
    },

    result: function (config)
    {
        var url = config.url || null,
            $iframe,
            resultDfd,
            result;
        
        if (url == null) return false;

        resultDfd = $.Deferred(function (dfd) {
            $iframe = $('<iframe name="iframe-modal" id="iframe-modal">');
            $iframe.attr('src', url);
            result = $('<div class="modal iframe">').append($iframe);

            if (result.length > 0) dfd.resolve(result);
            else dfd.reject();
        });

        return resultDfd.promise();
    },

    render: function (data) 
    {
        this.$container && (this.$container.modal('hide') && this.$container.remove()); // if already have modal in cache, remove it
        this.$container = $(data); // modal $
        this.$form = this.$container.find('form');

        this.$parent.after(this.$container); // insert modal into DOM

        this.$container.on({ 'shown': this.f.onOpen, 'hidden': this.f.onClose }); // attach events
        this.$container.modal({
            keyboard: false,
            backdrop: 'static'
        });
    }

});

R2V2.Admin.Configuration = Class.extend({
    init: function (config) {
        this.config = config || {};
        this.parent = this.config.parent;
        this.$dismiss = this.config.$dismiss;

        if (this.$dismiss == null || this.$dismiss.length <= 0) return;
        if (this.parent == null) return;

        //cache method calls
        this.f = {
            onDismiss: $.proxy(this.onDismiss, this)
        }

        this.attachEvents();
    },

    attachEvents: function () {
        this.$dismiss.on('click', this.f.onDismiss);
    },

    onDismiss: function (event) {
        // get event trigger
        var trigger = R2V2.Utils.GetEventTarget(event, 'a');

        if (trigger.DOM !== undefined) {
            event.preventDefault();

            if ($(trigger.DOM).data('dismiss') === 'modal') this.close();
            if ($(trigger.DOM).data('dismiss') === 'navigation') {
                this.navigate(trigger.DOM.href);
                this.close();
            }
        }

        return;
    },

    navigate: function (href) {
        if (href === parent.location.href) {
            window.parent.location.reload(true);
        }
        else window.parent.location = href;
    },

    close: function () {
        this.parent.$('.modal.iframe').modal('hide');
    }

});


/* ==========================================================================
    Admin IFrameModal 
    Implements iFrame-based Modal
   ========================================================================== */
R2V2.Admin.IFrameModal = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.parent = this.config.parent;
        this.$dismiss = this.config.$dismiss;

        if (this.$dismiss == null || this.$dismiss.length <= 0) return;
        if (this.parent == null) return;

        //cache method calls
        this.f = { 
            onDismiss: $.proxy(this.onDismiss, this)
        }

        this.attachEvents();
    },

    attachEvents: function ()
    {
        this.$dismiss.on('click', this.f.onDismiss);
    },

    onDismiss: function (event) 
    {
        // get event trigger
        var trigger = R2V2.Utils.GetEventTarget(event, 'a');

        if(trigger.DOM !== undefined)
        {
            event.preventDefault();

            if($(trigger.DOM).data('dismiss') === 'modal') this.close();
            if($(trigger.DOM).data('dismiss') === 'navigation') this.navigate(trigger.DOM.href);
        }

        return;
    },

    navigate: function (href)
    {
        if (href === parent.location.href) window.parent.location.reload(true); 
        else window.parent.location = href;
    },

    close: function () 
    {
        this.parent.$('.modal.iframe').modal('hide');
    }

});

/* ==========================================================================
    Admin ScrollTo 
    Implements auto-scroll to url hash (accounts for toolbar offset)
   ========================================================================== */

R2V2.Admin.ScrollTo = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.location = config.location;

        this.location = (this.location !== '') ? this.location.split('#')[1] : '';

        this.f = {
            scroll: $.proxy(this.scroll, this)
        };

        $(R2V2.Elements.Window).load(function (event) {
            event.preventDefault();
            $(R2V2.Elements.Window).trigger('hashchange');
        });

        $(R2V2.Elements.Window).on('hashchange', this.f.scroll);
    },

    scroll: function (event)
    {
        var toolbarHeight = 82,
            $el,
            elOffset;

        event.preventDefault();

        if (this.location !== '')
        {
            this.location = 'book-' + event.fragment;
            $el = $('a[name="' + this.location + '"]'),
            elOffset = parseInt(($el.offset().top - toolbarHeight), 10);
            $('body,html').scrollTop(elOffset);
        }
    }
});


R2V2.Admin.SelectedItems = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        this.trigger = this.config.trigger;

        if (this.$container == null || this.$container.length <= 0 || this.trigger == null) return;

        // cache method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this),
            selected: $.proxy(this.selected, this),
            toggleSelections: $.proxy(this.toggleSelections, this)
        };

        this.$items = this.$container.find(this.trigger);

        this.attachEvents();
    },

    attachEvents: function ()
    {
        this.$container.on('click', this.trigger, this.f.onTriggerClick);
    },

    onTriggerClick: function (event)
    {
        // get checkbox
        var target = R2V2.Utils.GetEventTarget(event, 'input');

        if (target.DOM.id === 'item-select-all') this.f.toggleSelections(target);

        $.PubSub(R2V2.PubSubMappings.Selection.Changed).publish( this.f.selected() );
    },

    toggleSelections: function (target)
    {
        //Need to make sure not more than 300 Ids are concatentated at once. there is a MAX limit to Query String Parameters. This should only affect Bulk deleting IP addresses.
        if (this.$items.length > 300) {
            this.$items.slice(0, 300).attr('checked', target.DOM.checked);
        } else {
            this.$items.attr('checked', target.DOM.checked);
        }
    },

    selected: function ()
    {
        //selected checkboxes, leave out 'check-all' checkbox
        var selectedResources = _.filter(this.$items, function (item) {
            return item.checked == true && item.name !== 'item-select-all';
        });

        //name attributes
        return _.pluck(selectedResources, 'name');
    }

});

R2V2.Admin.SelectedItemsPostOnly = Class.extend({
    init: function (config) {
        this.config = config || {};
        this.$container = this.config.$container;
        this.trigger = this.config.trigger;

        if (this.$container == null || this.$container.length <= 0 || this.trigger == null) return;

        // cache method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this),
            selected: $.proxy(this.selected, this),
            toggleSelections: $.proxy(this.toggleSelections, this)
        };

        this.$items = this.$container.find(this.trigger);

        this.attachEvents();
    },

    attachEvents: function () {
        this.$container.on('click', this.trigger, this.f.onTriggerClick);
    },

    onTriggerClick: function (event) {
        // get checkbox
        var target = R2V2.Utils.GetEventTarget(event, 'input');

        if (target.DOM.id === 'item-select-all') this.f.toggleSelections(target);

        $.PubSub(R2V2.PubSubMappings.Selection.Changed).publish(this.f.selected());

        var selectedValues = $('#SelectedInstitutionIds').val();
        //Need to do something different with item-select-all
        if (target.DOM.id === 'item-select-all') {
            if (target.DOM.checked) {
                var selectedResources = _.filter(this.$items, function (item) {
                    return item.checked == true && item.name !== 'item-select-all';
                });
                selectedResources.forEach(function(item) {
                    if (selectedValues === '') {
                        $('#SelectedInstitutionIds').val(item.name);
                    } else {
                        $('#SelectedInstitutionIds').val(selectedValues + ',' + item.name);
                    }
                    selectedValues = $('#SelectedInstitutionIds').val();
                });

            } else {
                $('#SelectedInstitutionIds').val('');
            }
        } else {
            if (target.DOM.checked) {
                if (selectedValues === '') {
                    $('#SelectedInstitutionIds').val(target.DOM.name);
                } else {
                    $('#SelectedInstitutionIds').val(selectedValues + ',' + target.DOM.name);
                }
            } else {
                if (selectedValues === target.DOM.name) {
                    $('#SelectedInstitutionIds').val('');
                } else {
                    var newValue = selectedValues.replace(target.DOM.name + ',', '');
                    if (newValue === selectedValues) {
                        newValue = selectedValues.replace(',' + target.DOM.name, '');
                    }
                    $('#SelectedInstitutionIds').val(newValue);
                }
            }
        }

        
        
    },

    toggleSelections: function(target) {
        this.$items.attr('checked', target.DOM.checked);
    },

    selected: function () {
        //selected checkboxes, leave out 'check-all' checkbox
        var selectedResources = _.filter(this.$items, function (item) {
            //alert(item);
            return item.checked == true && item.name !== 'item-select-all';
        });

        //name attributes
        return _.pluck(selectedResources, 'name');
    }

});

R2V2.Admin.IFrameModalLauncher = Class.extend({
    init: function (config) {
        this.config = config || {};
        this.$parent = this.config.$parent;
        this.trigger = this.config.trigger;
        this.$trigger = this.$parent.find(this.trigger);
        this.enabled = true;

        if (this.$parent == null || this.$parent.length <= 0 || this.trigger == null) return;

        // cache method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this),
            onChange: $.proxy(this.onChange, this),
            onSuccess: $.proxy(this.onSuccess, this),
            onChanged: $.proxy(this.onChanged, this),
            toggle: $.proxy(this.toggle, this)
        };

        this.attachEvents();
    },

    attachEvents: function () {
        this.$trigger.on('click', this.f.onTriggerClick);
    },

    onSuccess: function (response) {
        this.render(response);
    },

    onError: function (response) { },
    onOpen: function () { },

    onClose: function () {
        this.$container.remove();
    },

    doTriggerClick: function (event, target, url) {
        event.preventDefault();

        if (target.$.hasClass('disabled')) return;
        
        // get page to load into modal
        if (target.$.data('implement') === 'modal' && this.enabled) {
            this.result({ url: url }).then(this.f.onSuccess).fail(this.f.onError);
        }
        else {
            location.href = url;
        }
    },

    result: function (config) {
        var url = config.url || null,
        $iframe,
        resultDfd,
        result;

        if (url == null) return false;

        resultDfd = $.Deferred(function (dfd) {
            $iframe = $('<iframe name="iframe-modal" id="iframe-modal">');
            $iframe.attr('src', url);
            result = $('<div class="modal iframe">').append($iframe);

            if (result.length > 0) dfd.resolve(result);
            else dfd.reject();
        });

        return resultDfd.promise();
    },

    render: function (data) {
        this.$container && (this.$container.modal('hide') && this.$container.remove()); // if already have modal in cache, remove it
        this.$container = $(data); // modal $
        this.$form = this.$container.find('form');

        this.$parent.after(this.$container); // insert modal into DOM

        this.$container.on({ 'shown': this.f.onOpen, 'hidden': this.f.onClose }); // attach events
        this.$container.modal({
            keyboard: false,
            backdrop: 'static'
        });
    }
});

R2V2.Admin.ExpressCheckout = R2V2.Admin.IFrameModalLauncher.extend({
    init: function (config) {
        this._super(config);
        this.$isbns = this.$parent.find('textarea');
    },

    onTriggerClick: function (event) {
        var target = R2V2.Utils.GetEventTarget(event, 'a');
        var url = R2V2.Utils.UpdateQueryStringParameter(target.DOM.href, 'isbns', encodeURIComponent(this.$isbns.val()));

        this.doTriggerClick(event, target, url);
    }
});

R2V2.Admin.AddSelectedItems = R2V2.Admin.IFrameModalLauncher.extend({
    init: function (config) {
        this._super(config);

        this.selected = [];

        //cache DOM
        this.$buttons = this.$trigger;
    },

    attachEvents: function () {
        $.PubSub(R2V2.PubSubMappings.Selection.Changed).subscribe(this.f.onChanged);
        this._super();
    },

    onChanged: function (changed) {
        this.enabled = (changed.length > 0) ? true : false;

        if (changed.length > 0) this.selected = changed.join(",");
        else this.selected = [];

        this.f.toggle(this.enabled);
    },

    toggle: function (enable) {
        if (enable === true) this.$buttons.removeClass('disabled');
        else this.$buttons.addClass('disabled');
    },

    onTriggerClick: function (event) {
        var target = R2V2.Utils.GetEventTarget(event, 'a');
        var url = R2V2.Utils.UpdateQueryStringParameter(target.DOM.href, 'resources', this.selected);

        this.doTriggerClick(event, target, url);
    }
});


R2V2.Admin.ConcatSelectedItems = R2V2.Admin.IFrameModalLauncher.extend({
    init: function (config) {
        this._super(config);

        this.selected = [];

        //cache DOM
        this.$buttons = this.$trigger;
        this.$objectToPopulate = config.$objectToPopulate;
    },

    attachEvents: function () {
        $.PubSub(R2V2.PubSubMappings.Selection.Changed).subscribe(this.f.onChanged);
        this._super();
    },

    onChanged: function (changed) {
        this.enabled = (changed.length > 0) ? true : false;

        if (changed.length > 0) this.selected = changed.join(",");
        else this.selected = [];

        this.f.toggle(this.enabled);
    },

    toggle: function (enable) {
        if (enable === true) this.$buttons.removeClass('disabled');
        else this.$buttons.addClass('disabled');
    },

    onTriggerClick: function (event) {
        var target = R2V2.Utils.GetEventTarget(event, 'a');
        var url = R2V2.Utils.UpdateQueryStringParameter(target.DOM.href, this.$objectToPopulate, this.selected);

        this.doTriggerClick(event, target, url);
    }
});

/* ==========================================================================
    Admin Edit License Count
    extends: R2V2.Panel
   ========================================================================== */
R2V2.Admin.EditLicenseCount = R2V2.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$parent = this.config.$parent;
        this.trigger = this.config.trigger;

        if (this.$parent == null || this.$parent.length <= 0 || this.trigger == null) return;

        // cache method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this),
            onGetDataSuccess: $.proxy(this.onGetDataSuccess, this),
            onGetDataError: $.proxy(this.onGetDataError, this),
            onOpen: $.proxy(this.onOpen, this),
            onClose: $.proxy(this.onClose, this),
            onFormSave: $.proxy(this.onFormSave, this),
            onFormCancel: $.proxy(this.onFormCancel, this),
            onSaveDataSuccess: $.proxy(this.onSaveDataSuccess, this),
            onSaveDataError: $.proxy(this.onSaveDataError, this)
        };

        this.attachEvents();
    },

    attachEvents: function ()
    {
        this.$parent.on('click', this.trigger, this.f.onTriggerClick);
    },

    onTriggerClick: function (event)
    {
        event.preventDefault();

        // get link
        var target = R2V2.Utils.GetEventTarget(event, 'a');

        // get data
        $.executeService({ url: target.DOM.href, responseDataType: R2V2.ServiceExecutionOptions.Format.HTML }).then(this.f.onGetDataSuccess, this.f.onGetDataError);
    },

    onGetDataSuccess: function (response)
    {
        this.render(response);
    },

    onGetDataError: function (response) { },

    onFormSave: function (event)
    {
        event.preventDefault();

        // get form
        var form = R2V2.Utils.GetEventTarget(event, 'form'),
            emptyFields = form.$.find(':input:visible:enabled:not(button)[value=""]');

        // check for "empty" fields
        if (emptyFields.length > 0) return;

        // post data
        $.executeService({ url: form.DOM.action, data: form.$.serialize(), type: 'POST', contentType: R2V2.ServiceExecutionOptions.ContentType.DEFAULT }).then(this.f.onSaveDataSuccess).fail(this.f.onSaveDataError);
    },

    onSaveDataSuccess: function (response)
    {
        // reload page
        window.location = document.URL;
    },

    onSaveDataError: function (response)
    {
        this.$container && (this.$container.modal('hide') && this.$container.remove()); // if already have modal in cache, remove it
    },

    render: function (data)
    {
        this.$container && (this.$container.modal('hide') && this.$container.remove()); // if already have modal in cache, remove it
        this.$container = $(data); // modal $
        this.$form = this.$container.find('form');
        this.$parent.after(this.$container); // insert modal into DOM
        this.$container.on({ 'shown': this.f.onOpen, 'hidden': this.f.onClose }); // attach events
        this.$container.modal(); // open modal
    }
});


/* ==========================================================================
    Admin ToggleOptionalFields
    Toggles optional fields
   ========================================================================== */
R2V2.Admin.ToggleOptionalFields = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.$container = this.config.$container;
        this.$trigger = this.config.$trigger;
        
        if (this.$container == null || this.$container.length <= 0) return;
        if (this.$trigger == null || this.$trigger.length <= 0) return;
        
        // cache method calls
        this.f = { onFilterAction: $.proxy(this.onFilterAction, this) };

        this.attachEvents();
        this.onFilterAction();
    },

    attachEvents: function ()
    {
        this.$trigger.on(this.config.action, this.f.onFilterAction);
    },

    onFilterAction: function (event)
    {
        // toggle visibility depending on val
        var test = this.config.triggerValue ? this.$trigger.val() === this.config.triggerValue : this.$trigger.get(0).checked;
        this.$container.collapse(test ? 'show' : 'hide');
    }
});

/* ==========================================================================
    Admin SelectizeAddAll
    Adds all "select" options
   ========================================================================== */
R2V2.Admin.SelectizeAddAll = Class.extend({    
    init: function (config)
    {
        this.config = config || {};
        this.$select = this.config.$select;
        
        // cache method calls
        this.f = { onItemAdd: $.proxy(this.onItemAdd, this) };

        this.setup();
        this.attachEvents();
    },

    setup: function ()
    {
        this.control = this.$select[0].selectize;
        this.options = _.omit(this.control.options, 'All');
        this.optionsByAlpha = _.sortBy(this.options, 'text');
        this.values = _.pluck(this.optionsByAlpha, 'value');
        this.values = _.without(this.values, 'All');

        this.control.settings.maxItems = this.values.length;
    },

    attachEvents: function ()
    {
        this.control.on('item_add', this.f.onItemAdd);
    },

    onItemAdd: function (value, $item)
    {
        if (value !== 'All')
        {
            return;
        }

        var reset = _.bind(this.reset, this);

        _.delay(reset, 1);
    },

    reset: function ()
    {
        var settings = this.control.settings;

        this.control.destroy();

        settings.items = this.values;
        this.$select.selectize(settings);
        
        this.setup();
        this.attachEvents();
    }
});

/* ==========================================================================
    Admin SelectizeClearAll
    Clears all "selected" options
   ========================================================================== */
R2V2.Admin.SelectizeClearAll = Class.extend({ 
    init: function (config)
    {
        this.config = config || {};
        this.$select = this.config.$select;
        this.$trigger = this.config.$trigger;

        // cache method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this)
        };

        this.attachEvents();
    },

    attachEvents: function ()
    {
        this.$trigger.on('click', this.f.onTriggerClick);
    },

    onTriggerClick: function ()
    {
        var control = this.$select[0].selectize;
        control.clear();
    }
});


/* ==========================================================================
    Admin Marketing Actions Menu
    extends: R2V2.Admin.ActionsMenu
   ========================================================================== */
R2V2.Admin.Marketing = Class.extend({

});


/* ==========================================================================
    Admin Marketing Actions Menu
    extends: R2V2.Admin.ActionsMenu
   ========================================================================== */
R2V2.Admin.Marketing.ActionsMenu = R2V2.ActionsMenu.extend({
    init: function (config) {
        this.config = config || {};
        this.config.self = R2V2.Admin.Marketing.ActionsMenu;

        this._super();

        this.$container = this.config.$container;
        this.$linkExport = this.config.$linkExport;
        this.$exportButton = this.config.$exportButton;

        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, { onExportClick: $.proxy(this.onExportClick, this) });

        this.attachEvents();
    },

    attachEvents: function () {
        if (this.$linkExport) {
            this.$linkExport.on('click', this.f.onExportClick);
        }
    },

    onExportClick: function () {
        this.$exportButton.click();
    }
});


/* ==========================================================================
    Admin Resource Filter Select 
   ========================================================================== */
R2V2.Admin.ResourceSelect = Class.extend({
    init: function (config) {
        this.config = config || {};

        this.$textLayout = this.config.$textLayout;
        this.$resourceLayout = this.config.$resourceLayout;
        this.$resourceLayoutRadio = this.config.$resourceLayoutRadio;

        this.$resourceId = this.config.$resourceId;
        this.$resourceText = this.config.$resourceText;
        this.$searchUrl = this.config.$searchUrl;
        this.$publisherIdObject = this.config.$publisherIdObject;
        this.$resourceLayoutButtons = this.config.$resourceLayoutButtons;

        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {},
            {
                radioChange: $.proxy(this.radioChange, this)
            });


        this.typeAhead();

        this.attachEvents();
    },

    attachEvents: function () {
        var that = this;
        if (that.$resourceId && that.$resourceLayoutRadio) {
            that.$resourceId.on("change", that.f.radioChange);
        }
    },

    typeAhead: function () {
        var that = this;

        if (that.$textLayout) {
            if (that.$resourceId && that.$resourceId.val() > 0) {
                that.$textLayout.hide();
                that.$resourceLayout.show();
                that.$resourceLayoutButtons.show();
            } else {
                that.$textLayout.show();
                that.$resourceLayout.hide();
                that.$resourceLayoutButtons.hide();
            }
        }

        this.$resourceText.autocomplete({
                scroll: true,
                minLength: 1,
                source: function(request, response) {
                    $.ajax({
                        url: that.$searchUrl,
                        data: { query: request.term, publisherId: $(that.$publisherIdObject).val()    },
                        success: function(data) {
                            var obj = $.parseJSON(data);
                            response(obj);
                        },
                        error: function() {
                            response([]);
                        }
                    });
                },
                select: function (event, ui) {
                    event.preventDefault();
                    $(this).val(ui.item.label);
                    that.$resourceId.val(ui.item.value);
                    if (that.$resourceLayoutRadio) {
                        that.$resourceId.trigger("change");
                    }
                    return false;
                },
                change: function (event, ui) {
                    $(this).val(ui.item ? ui.item.label : "");
                    that.$resourceId.val(ui.item ? ui.item.value : "");
                    if (that.$resourceLayoutRadio) {
                        that.$resourceId.trigger("change");
                    }
                },
                focus: function (event, ui) {
                    event.preventDefault();
                },
                close: function(event, ui) {
                    if (!ui.item) {
                        that.$resourceId.val("");
                        if (that.$resourceLayoutRadio) {
                            that.$resourceId.trigger("change");
                        }
                    }
                    
                },
                open: function(e, ui) { /*Hack for IE11*/
                    $(this).focus();

                }
            })
            .one("focus", function() { /*Hack for IE11*/
                $(this).blur();
            });
    },

    radioChange: function () {
        var that = this;
        var resourceId = that.$resourceId.val();
        if (resourceId && resourceId > 0) {
            that.$textLayout.hide();
            that.$resourceLayout.show();
            that.$resourceLayoutRadio.attr("checked", true);
            that.$resourceLayoutButtons.show();
        }
        else {
            that.$textLayout.show();
            that.$resourceLayout.hide();
            that.$resourceLayoutRadio.attr("checked", false);
            that.$resourceLayoutButtons.hide();
        }

    }
});



/* ==========================================================================
    Admin DatePicker
   ========================================================================== */
R2V2.Admin.DatePicker = Class.extend({
    init: function (config) {
        this.config = config || {};

        this.$dateTextBox = this.config.$dateTextBox;
        this.$startDate = this.config.$startDate;
        this.$endDate = this.config.$endDate;
        this.$endDatePlusYears = this.config.$endDatePlusYears;
        this.$type = this.config.$type;
        this.$format = this.config.$format;

        this.datePicker();
    },
    datePicker: function () {
        var that = this;
        var startDate = that.$startDate;
        if (!that.$startDate) {
            startDate = "01/15/2009";
        }

        var endDate = that.$endDate;
        if (!that.$endDate) {
            var test = new Date();
            var endYear = test.getFullYear();
            if (that.$endDatePlusYears !== undefined) {
                endYear = endYear + that.$endDatePlusYears;
            }
            endDate = test.getMonth() + 1 + "/" + test.getDate() + "/" + endYear;
        }

        var minViewMode = 0;
        if (that.$type !== undefined) {
            switch (that.$type) {
                case "month":
                    minViewMode = 1;
                    break;
                case "year":
                    minViewMode = 2;
                    break;
                default:
                    minViewMode = 0;
                    break;
            }
        }
        var format = "mm/dd/yy";
        if (that.$format !== undefined) {
            format = that.$format;
        }

        that.$dateTextBox.datepicker({
            autoclose: true,
            hideIfNoPrevNext: true,
            dateFormat: format,
            minViewMode: minViewMode,
            startDate: startDate,
            endDate: endDate
        });
    }
});


/* ==========================================================================
    Admin Left Nav
   ========================================================================== */
R2V2.Admin.LeftNav = Class.extend({
    init: function (config) {
        this.config = config || {};

        this.$container = this.config.$container;

        this.$container.find("ul[data-number-of-visible-links]").filter(function() {
            return parseInt($(this).attr("data-number-of-visible-links")) > 0;
        }).each(function () {
                var $el = $(this);

                $el.attr("data-toggle", "moreless");
                $el.moreless.defaults.numberOfVisible = $el.attr("data-number-of-visible-links");
                $el.moreless.defaults.elementSelector = 'li:not(.subgroup li)';

                // rerun "moreless" plugins
                $el.moreless('reset');
        });
    }
});