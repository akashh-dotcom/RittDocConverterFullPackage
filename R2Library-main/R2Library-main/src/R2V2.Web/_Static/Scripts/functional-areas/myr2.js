/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    MyR2
   ========================================================================== */
R2V2.MyR2 = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        
        this.$container = this.config.$container;
        this.$addFolderTrigger = this.config.$addFolderTrigger;
        this.folderConfig = this.config.folderConfig;
        this.itemConfig = this.config.itemConfig;

        // error check
        if (this.$container == null || this.$container.length === 0) return;
        
        // cache DOM elements
        this.folders = this.folderConfig.$container.find('[data-' + this.folderConfig.dataName + ']');
        this.items = this.$container.find('[data-' + this.itemConfig.dataName + ']');

        // cache bound/proxy'd method calls
        this.f = {
            onAddFolderTriggerClick: $.proxy(this.onAddFolderTriggerClick, this),
            setupFolders: $.proxy(this.setupFolders, this),
            setupItems: $.proxy(this.setupItems, this)
        };
        
        this.setup();
        this.attachEvents();
    },
    
    setup: function ()
    {
        // Content Folders
        this.folders.each(this.f.setupFolders);
        
        // Content OrderItems
        this.items.each(this.f.setupItems);
    },

    attachEvents: function ()
    {
        // DOM events
        this.$addFolderTrigger.on('click', this.f.onAddFolderTriggerClick);
    },

    onAddFolderTriggerClick: function (event)
    {
        event.preventDefault();

        this.addFolder();
    },
    
    setupFolders: function (i, element)
    {
        new R2V2.MyR2.ExistingFolder(_.extend(this.folderConfig, { $element: $(element) }));
    },

    setupItems: function (i, element)
    {
        new R2V2.MyR2.UserContentItem(_.extend(this.itemConfig, { $element: $(element) }));
    },
    
    addFolder: function ()
    {
        new R2V2.MyR2.NewFolder(this.folderConfig);
    }
});


/* ==========================================================================
    MyR2 User Content
   ========================================================================== */
R2V2.MyR2.UserContent = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        
        this.$element = this.config.$element;
        this.dataName = this.config.dataName;
        this.actionsSelector = this.config.actionsSelector;
        this.dataUrls = this.config.dataUrls;
        
        // error check
        if (this.$element == null || this.$element.length === 0) return;
        
        // cache properties
        this.elementData = this.$element.data(this.dataName);
        if (this.elementData)
        {
            this.id = this.elementData.Id || null;
            this.folderId = this.elementData.FolderId || null;
        }
        
        // cache bound/proxy'd method calls
        this.f = _.extend(this.f || {}, {
            onSaveDataSuccess: $.proxy(this.onSaveDataSuccess, this),
            onActionsClick: $.proxy(this.onActionsClick, this),
            onKeyboardSave: $.proxy(this.onKeyboardSave, this),
            onKeyboardCancel: $.proxy(this.onKeyboardCancel, this)
        });
        
        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        // DOM events
        this.$element.on('click', this.actionsSelector, this.f.onActionsClick);
    },

    onActionsClick: function (event)
    {
        var target = R2V2.Utils.GetEventTarget(event, 'a'); // get event target
        if (target.$.length <= 0) return; // make sure to only capture link clicks
        
        var action = target.DOM.hash.substr(1); // get "action" from hash
        if (this[action] == null) return; // check if "action" is a local method

        event.preventDefault();
        
        this[action]();
    },
    
    onKeyboardSave: function(event)
    {
        this.save();
    },
    
    onKeyboardCancel: function(event)
    {
        this.cancel();
    },
    
    saveData: function (action, data)
    {
        $.executeService({ url: this.dataUrls[action], data: data }).then(this.f.onSaveDataSuccess);
    }
});

/* ==========================================================================
    MyR2 User Content Folders
    extends: R2V2.MyR2.UserContent
   ========================================================================== */
R2V2.MyR2.UserContentFolder = R2V2.MyR2.UserContent.extend({
    
    _modes: {
        edit: 'edit',
        view: 'view'
    },
    
    init: function (config)
    {
        // cache proxy'd method calls
        this.f = {
            onFieldFocus: $.proxy(this.onFieldFocus, this),
            onUserContentItemMoved: $.proxy(this.onUserContentItemMoved, this),
            onUserContentItemRemoved: $.proxy(this.onUserContentItemRemoved, this)
        };
        
        // call parent method
        this._super(config);

        // cache elements and properties
        this.$field = this.$element.find('input');
        this.$items = this.$element.find('.titlelist');
        this.$heading = this.$items.prev('.accordion-heading');
        this.isDefaultGroup = this.$field.length === 0; // can't edit (rename/delete) default group so there is no input field
    },
    
    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.UserContentItem.Moved).subscribe(this.f.onUserContentItemMoved);
        $.PubSub(R2V2.PubSubMappings.UserContentItem.Removed).subscribe(this.f.onUserContentItemRemoved);
        
        // DOM events
        this._super();
    },
    
    onFieldFocus: function (event)
    {
        this.$field.blur();
    },

    onUserContentItemMoved: function (response)
    {
        if (this.id !== response.newFolderId && this.id !== response.previousFolderId) return;

        if (this.id === response.newFolderId)
        {
            response.$item.detach();
            this.$items.append(response.$item); // append item
            this.isDefaultGroup === false && this.$heading.removeClass('closed').addClass('opened'); // configure "folder"
            response.$item.removeData('user-content-item').data({ 'user-content-item': { Id: response.itemId, FolderId: response.newFolderId } });
            return;
        }
        
        if (this.isDefaultGroup || $.trim(this.$items.html()).length) return;
        
        this.$heading.removeClass('opened');
    },
    
    onUserContentItemRemoved: function (response)
    {
        if (this.isDefaultGroup || this.id !== response.folderId || $.trim(this.$items.html()).length) return;
        
        this.$heading.removeClass('opened');
    },
    
    onSaveDataSuccess: function (response)
    {
        // publish
        $.PubSub(R2V2.PubSubMappings.UserContentFolder.Updated).publish(response);

        // no "id" if removed
        if (response.Id == null) return;

        // set properties
        this.id = response.Id;

        // update element data
        this.$element.data(this.dataName, { Id: this.id });
    },
    
    save: function()
    {
        this.name = $.trim(this.$field.val());

        this.saveData(this.saved ? 'rename' : 'save');
        this.changeMode(this._modes.view);
        this.saved = true;
    },
    
    rename: function()
    {
        this.changeMode(this._modes.edit);
    },
    
    cancel: function()
    {
        if (this.saved)
        {
            this.$field.val(this.name);
            this.changeMode(this._modes.view);
            return;
        }
        this.$element.remove();
    },
    
    remove: function()
    {
        this.saveData('remove');
        this.$element.remove();
    },

    changeMode: function(mode)
    {
        if (mode === this._modes.edit)
        {
            this.$element.addClass(this._modes.edit);
            this.$field.on(R2V2.KeyboardMappings.Folder.Save, this.f.onKeyboardSave).on(R2V2.KeyboardMappings.Folder.Cancel, this.f.onKeyboardCancel).off('focus', this.f.onFieldFocus);
            this.$field.focus();
            return;
        }
        this.$element.removeClass(this._modes.edit);
        this.$field.blur();
        this.$field.off(R2V2.KeyboardMappings.Folder.Save).off(R2V2.KeyboardMappings.Folder.Cancel).on('focus', this.f.onFieldFocus);
    },

    saveData: function (action)
    {
        var data = this.$element.data(this.dataName);
        
        this._super(action, { id: data ? data.Id : null, name: this.name });
    }
});


/* ==========================================================================
    MyR2 New Folder
    extends: R2V2.MyR2.UserContentFolder
   ========================================================================== */
R2V2.MyR2.NewFolder = R2V2.MyR2.UserContentFolder.extend({
    init: function (config)
    {
        this.$container = config.$container;
        
        // error check
        if (this.$container == null || this.$container.length === 0) return;

        config.$element = $(config.template());

        this._super(config);

        this.$container.append(this.$element);
        
        this.changeMode(this._modes.edit);
    } 
});


/* ==========================================================================
    MyR2 Existing Folder
    extends: R2V2.MyR2.UserContentFolder
   ========================================================================== */
R2V2.MyR2.ExistingFolder = R2V2.MyR2.UserContentFolder.extend({
    init: function (config)
    {
        this._super(config);
        
        this.saved = true;
        this.name = this.$field.val();
        
        this.changeMode(this._modes.view);
    }
});


/* ==========================================================================
    MyR2 User Content Item
    extends: R2V2.MyR2.UserContent
   ========================================================================== */
R2V2.MyR2.UserContentItem = R2V2.MyR2.UserContent.extend({
    
    // constructor in R2V2.MyR2.UserContent

    onSaveDataSuccess: function (response)
    {
        this.$element.remove();

        // publish
        $.PubSub(R2V2.PubSubMappings.UserContentItem.Removed).publish({ folderId: this.folderId });
    },
    
    remove: function()
    {
        this.saveData('remove');
    },

    saveData: function (action)
    {
        // get data (instead of using cached Id and FolderId) in case item was moved
        var data = this.$element.data(this.dataName);
        if (data == null) return;

        this._super(action, { id: data.Id, folderId: data.FolderId });
    }
});


/* ==========================================================================
    MyR2 User Content Export Panel
    extends: R2V2.Panel
    ========================================================================== */
R2V2.MyR2.UserContentItem.ExportPanel = R2V2.Panel.extend({
    init: function (config)
    {
        this.config = config || {};
        
        this.$container = this.config.$container;
        
        // error check
        if (this.$container == null || this.$container.length <= 0) return;

        // call parent method
        this._super();
    },

    attachEvents: function ()
    {
        // DOM events
        this.$container.on({ 'shown': this.f.onOpen, 'hidden': this.f.onClose });
    },
    
    onFormSave: function (event)
    {
        // allow default form post in order to download file
        
        this.closeAll();
    }
});


/* ==========================================================================
    MyR2 User Content Move Panel
    extends: R2V2.Panel
    ========================================================================== */
R2V2.MyR2.UserContentItem.MovePanel = R2V2.Panel.extend({

    _optionsTemplate: _.template('{{ _.each(Folders, function(folder) { }}<option value="{{= folder.Id }}">{{= folder.FolderName }}</option>{{ }); }}'),

    init: function (config)
    {
        this.config = config || {};
        
        this.$container = this.config.$container;
        this.dataUrl = this.config.dataUrl;
        
        // error check
        if (this.$container == null || this.$container.length <= 0) return;

        // cache proxy'd method calls
        this.f = {
            onFolderUpdated: $.proxy(this.onFolderUpdated, this),
            onFolderUpdatedResponse: $.proxy(this.onFolderUpdatedResponse, this)
        };

        // call parent method
        this._super();
    },

    attachEvents: function ()
    {
        // subscriptions
        $.PubSub(R2V2.PubSubMappings.UserContentFolder.Updated).subscribe(this.f.onFolderUpdated);
        
        // DOM events
        this.$container.on({ 'shown': this.f.onOpen, 'hidden': this.f.onClose });
    },

    onFolderUpdated: function(event)
    {
        $.executeService({ url: this.dataUrl, responseDataType: R2V2.ServiceExecutionOptions.Format.JSON }).then(this.f.onFolderUpdatedResponse);
    },
    
    onFolderUpdatedResponse: function(response)
    {
        this.$form.find('select[name="newFolderId"]').empty().append(this._optionsTemplate(response));
    },

    onSaveDataSuccess: function(response)
    {
        this._super(response);
        
        // publish
        $.PubSub(R2V2.PubSubMappings.UserContentItem.Moved).publish({ $item: this.$resource, itemId: this.resourceData.Id, newFolderId: this.newFolderId, previousFolderId: this.resourceData.FolderId });
    },
    
    save: function (event)
    {
        event.preventDefault();

        this.trigger = this.$container.data('trigger');
        if (this.trigger == null) return;

        // get resource data
        this.$resource = this.trigger.parents('[data-user-content-item]');
        this.resourceData = this.$resource.data('user-content-item');
        
        // get new folder id
        this.newFolderId = parseInt(this.$form.find('select:visible').val());
        
        // make sure new folder is not current folder
        if (this.resourceData.FolderId === this.newFolderId) return;
        
        // get form action and data
        var dataUrl = this.$form.attr('action'),
            formData = $.param(this.resourceData) + '&' + this.$form.serialize();
        
        // execute service call
        $.executeService({ url: dataUrl, data: formData }).then(this.f.onSaveDataSuccess, this.f.onSaveDataError);
    }
});


/* ==========================================================================
    MyR2 Saved Searches
   ========================================================================== */
R2V2.MyR2.SavedSearches = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        
        this.$triggers = this.config.$triggers;

        // error check
        if (this.$triggers == null || this.$triggers.length === 0) return;
        
        // cache bound/proxy'd method calls
        this.f = {
            onTriggerClick: $.proxy(this.onTriggerClick, this),
            onExecuteServiceSuccess: $.proxy(this.onExecuteServiceSuccess, this),
            onExecuteServiceFail: $.proxy(this.onExecuteServiceFail, this)
        };
        
        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        // DOM events
        this.$triggers.on('click', this.f.onTriggerClick);
    },

    onTriggerClick: function (event)
    {
        event.preventDefault();

        // get target
        var target = R2V2.Utils.GetEventTarget(event, 'a');
        // get container
        this.container = target.$.parents('p');
        
        // execute service
        $.executeService({ url: target.DOM.href }).then(this.f.onExecuteServiceSuccess, this.f.onExecuteServiceFail);
    },
    
    onExecuteServiceSuccess: function (response)
    {
        if (response.Successful === false)
        {
            this.onExecuteServiceFail();
            return;
        }
        
        if (this.container.siblings('p').length === 0) 
        {
            this.container.after('<p>No saved searches available</p>');
        }
        this.container.remove();
    },
    
    onExecuteServiceFail: function (response) { }
});