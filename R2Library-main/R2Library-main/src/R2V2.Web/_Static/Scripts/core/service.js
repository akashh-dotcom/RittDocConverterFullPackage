/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };
R2V2.Services = R2V2.Services || {};


/* ==========================================================================
    Global Ajax Prefilter
   ========================================================================== */
$.ajaxPrefilter(function (options, localOptions, xhr) {
    if (options.checkSuccessStatus === false) return;

    var normalizedRequest = $.Deferred();

    xhr.pipe(function(response) { // filter SUCCESS
        if (response == null || response.Successful === false)
        {
            normalizedRequest.rejectWith(this, arguments);
        }
        else
        {
            normalizedRequest.resolveWith(this, arguments);
        }
    }, function(xhr, status, error) { // filter ERROR
        // handle cases where user leaves page before ajax call completes
        if (!xhr.getAllResponseHeaders())
        {
            xhr.abort();
            return;
        }

        // capture aborts
        if (status === 'abort') return;

        normalizedRequest.rejectWith(xhr, status, error);
    });
    
    xhr = normalizedRequest.promise(xhr);
    xhr.success = xhr.done;
    xhr.error = xhr.fail;
});


/* ==========================================================================
    Service Wrappers
   ========================================================================== */
$.executeService = function (config) {
    var url = config.url || null,
        data = config.data || '{}',
        type = config.type || 'GET',
        checkSuccessStatus = config.checkSuccessStatus || true, // custom property for ajax "filtering"
        cache = config.cache || false,
        contentType = config.contentType || R2V2.ServiceExecutionOptions.ContentType.JSON,
        responseDataType = config.responseDataType || R2V2.ServiceExecutionOptions.Format.JSON,
        responseHandler = config.responseHandler || $.defaultHandleResponseEnvelopeCallback,
        errorHandler = config.errorHandler || $.defaultHandleErrorCallback;
    
    if (url == null) return false;
    
    var xhr = $.ajax({
        url: url,
        type: type,
        contentType: contentType,
        cache: cache,
        data: data,
        dataType: responseDataType,
        checkSuccessStatus: checkSuccessStatus
    });
    
    xhr.then(responseHandler).fail(errorHandler);

    return xhr;
};

$.CopySelectedRadioToHidden = function(config) {
    var form = config.form;
    var hiddenCheckedVal = config.hiddenChecked;
    var hiddenUnCheckedVal = config.hiddenUnChecked;
    var button = config.button;
    
    $(button).click(function () {

        var inputs = $( form + " input[type='radio']");
        var checked = [];
        var unChecked = [];
        $.each(inputs, function () {
            if ($(this).attr('value') == 'True' && $(this).attr('checked')) {
                checked.push($(this).attr('name'));
            } if ($(this).attr('value') == 'False' && $(this).attr('checked')) {
                unChecked.push($(this).attr('name'));
            }
        });
        $(hiddenCheckedVal).val(checked);
        $(hiddenUnCheckedVal).val(unChecked);
    });
};

/* ==========================================================================
    Default Deferred Response Handler Pipeline
   ========================================================================== */
R2V2.Services.DeferredPipeline = R2V2.Services.DeferredPipeline || {};


/* ==========================================================================
    Default Empty Response Handler
   ========================================================================== */
R2V2.Services.DeferredPipeline.EmptyResponseHandler = function (deferred, response) {
    if (response == null || response === '') deferred.reject();
        
    deferred.resolve(response);
};


/* ==========================================================================
    Default Timeout Handler
   ========================================================================== */
R2V2.Services.DeferredPipeline.TimeoutWidgetResponseHandler = function (deferred, response) {
    deferred.resolve(response);
};


/* ==========================================================================
    Default Informational Message Handler
   ========================================================================== */
R2V2.Services.DeferredPipeline.InformationalMessagesResponseHandler = function (deferred, response) {
    deferred.resolve(response);
};


/* ==========================================================================
    Default Error Message Handler
   ========================================================================== */
R2V2.Services.DeferredPipeline.ErrorMessagesResponseHandler = function (deferred, response) {
    deferred.resolve(response);
};


/* ==========================================================================
    Default Client Command Handler
   ========================================================================== */
R2V2.Services.DeferredPipeline.ClientCmdResponseHandler = function (deferred, response) {
    if (response.ClientCmdQueue && response.ClientCmdQueue.length > 0)
    {
        var calls = [];

        for (var index in response.ClientCmdQueue) {
            var cmd = response.ClientCmdQueue[index];
            calls.push($.ajax({
                type: "get",
                dataType: "jsonp",
                contentType: R2V2.ServiceExecutionOptions.ContentType.JSON,
                url: cmd
            }));

        }

        $.when.apply($, calls).done( function () { deferred.resolve(response); });
    }
    else
    {
        deferred.resolve(response);
    }
};


/* ==========================================================================
    Default Redirect to Location Handler
   ========================================================================== */
R2V2.Services.DeferredPipeline.RedirectToLocationResponseHandler = function (deferred, response) {
    if (response.RedirectUrl)
    {
        if (window.location == response.RedirectUrl)
        {
            window.location.reload(false);
        }
        window.location = response.RedirectUrl;

        deferred.reject();
    }
    else
    {
        deferred.resolve(response);
    }
};


/* ==========================================================================
    Service Response Handler
   ========================================================================== */
R2V2.Services.ResponseHandler = function (config) {
    this.config = config || { };
        
    var workflowOrder = [
        'EmptyResponseHandler',
        'TimeoutWidgetResponseHandler',
        'InformationalMessagesResponseHandler',
        'ErrorMessagesResponseHandler',
        'ClientCmdResponseHandler',
        'RedirectToLocationResponseHandler'
    ];
    var workflow = _.map(workflowOrder, function(handler) { return this.config[handler] || R2V2.Services.DeferredPipeline[handler]; }, this);
    this.workflow = this.config.workflow || workflow;

    return this.init();
};
R2V2.Services.ResponseHandler.prototype = {
    init: function ()
    {
        this.f = { execute: $.proxy(this.execute, this) };
            
        return (this.f.execute);
    },
        
    execute: function(response)
    {
        var xhr = arguments[2];

        // cache main deferred
        this.masterDeferred = $.Deferred();
        xhr = this.masterDeferred.promise(xhr);
        xhr.success = xhr.done;
        xhr.error = xhr.fail;
            
        // check for "handlers"
        if (this.workflow.length)
        {
            // invoke first "handler"
            this.invoke(this.workflow.shift(), response);
        }
        else
        {
            // nothing to do, resolve the main deferred
            this.masterDeferred.resolve();
        }

        return (this.masterDeferred.promise());
    },
        
    invoke: function(handler, response)
    {
        var that = this,
            deferred = $.Deferred();

        deferred.promise().then(
            function() { // "done" callback
                    
                // "handler" was resolved, get next
                var next = that.workflow.shift();
                    
                // check for next handler
                if (next)
                {
                    // recursively invoke handler
                    return that.invoke(next, response);
                }
                    
                // no more handlers are available (workflow is complete), so resolve main deferred, passing-through the response
                that.masterDeferred.resolve.apply(that.masterDeferred, response);
            },
            function() { // "fail" callback
                // "handler" was rejected, so reject the main deferred passing-through the rejected response
                that.masterDeferred.reject.apply(that.masterDeferred, arguments);
            }
        );
            
        try {
            // create an invocation arguments collection so that we can seamlessly pass-through any previously-resolved response (the deferred result will always be the first argument in this argument collection)
            var handlerArguments = [deferred].concat(response);
                
            // call the callback with the given arguments (the deferred result and any response)
            handler.apply(window, handlerArguments);
        } catch(syncError) {
            // if there was a synchronous error in the handler that was not caught, let's return the native error.
            this.masterDeferred.reject(syncError);
        }
    }
};


/* ==========================================================================
    Default Response Handlers
   ========================================================================== */
$.defaultHandleResponseEnvelopeCallback = new R2V2.Services.ResponseHandler();

$.defaultHandleErrorCallback = function (xhr, status, error) { };