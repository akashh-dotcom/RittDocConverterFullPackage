/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Log
    usage: log('inside coolFunc', this, arguments);
    paulirish.com/2009/log-a-lightweight-wrapper-for-consolelog/
   ========================================================================== */
window.log = function () {
    log.history = log.history || [];   // store logs to an array for reference
    log.history.push(arguments);
    if (this.console) {
        arguments.callee = arguments.callee.caller;
        var newarr = [].slice.call(arguments);
        (typeof console.log === 'object' ? log.apply.call(console.log, console, newarr) : console.log.apply(console, newarr));
    }
};

// make it safe to use console.log always
(function (b) { function c() { } for (var d = "assert,clear,count,debug,dir,dirxml,error,exception,firebug,group,groupCollapsed,groupEnd,info,log,memoryProfile,memoryProfileEnd,profile,profileEnd,table,time,timeEnd,timeStamp,trace,warn".split(","), a; a = d.pop(); ) { b[a] = b[a] || c } })((function () {
try { console.log(); return window.console; } catch (err) { return window.console = {}; } 
})());


/* ==========================================================================
    Underscore Template Settings
   ========================================================================== */
_.templateSettings = {
    interpolate: /\{\{\=(.+?)\}\}/g,
    evaluate: /\{\{(.+?)\}\}/g,
    escape: /\{\{-(.*?)\}\}/g
};


/* ==========================================================================
    jQuery Validator Settings
   ========================================================================== */
(function () {
    jQuery.validator.addMethod('multiemail',
        function(value, element) {
            if (this.optional(element)) return true;
        
            var emails = value.split(/[;]+/), // split element by ;
                valid = true;
        
            for (var i in emails)
            {
                value = emails[i];
                valid = valid && jQuery.validator.methods.email.call(this, $.trim(value), element);
            }
            
            return valid;
        },
        jQuery.validator.messages.email
    );

    jQuery.validator.addMethod('passwordCompare', function (value, element, param) {
        var target = $(param[0]).unbind(".validate-equalTo").bind("blur.validate-equalTo", function() { $(element).valid(); }),
            target2 = $(param[1]).unbind(".validate-equalTo").bind("blur.validate-equalTo", function() { $(element).valid(); });
        
		return (value == target.val() && target2.val().length > 0) || (value.length == 0 && target.val().length == 0 && target2.val().length == 0);
    });
    
    var setValidationValues = function (options, ruleName, value) {
        options.rules[ruleName] = value;
        if (options.message)
        {
            options.messages[ruleName] = options.message;
        }
    };
    
    $.validator.unobtrusive.adapters.add("passwordcompare", ["newPassword", "currentPassword"], function (options) {
        var prefix = options.element.name.substr(0, options.element.name.lastIndexOf(".") + 1),
        newPassword = $(options.form).find(":input[name=" + appendModelPrefix(options.params.newPassword, prefix) + "]")[0],
        currentPassword = $(options.form).find(":input[name=" + appendModelPrefix(options.params.currentPassword, prefix) + "]")[0];

        setValidationValues(options, "passwordCompare", [ newPassword, currentPassword ]);
    });
    
    jQuery.validator.addMethod('passwordvalid', function (value, element, param) {
        if (!value) {
            return true;
        }
        var v1 = new RegExp('[A-Z]');
        var v2 = new RegExp('[a-z]');
        var v3 = new RegExp('[0-9]');
        var v4 = new RegExp('[`~!@@#$%^&*()<>?:,./\\\\;\'|-]');

        var val = value;

        var test = 0;
        test += v1.test(val) ? 1 : 0;
        test += v2.test(val) ? 1 : 0;
        test += v3.test(val) ? 1 : 0;
        test += v4.test(val) ? 1 : 0;
        // console.log(v4.test(val));
        var valueLength = val.length;

        return test >= 3 && (valueLength > 7 && valueLength < 21);
        
        });

    $.validator.unobtrusive.adapters.add("passwordvalid", ["newPassword"], function (options) {
        var prefix = options.element.name.substr(0, options.element.name.lastIndexOf(".") + 1),
            newPassword = $(options.form).find(":input[name=" + appendModelPrefix(options.params.newPassword, prefix) + "]")[0];

        setValidationValues(options, "passwordvalid", [newPassword]);
    });
    
    jQuery.validator.addMethod('datetenyearsvalid', function (value, element, param) {

        if (value) {
            if (value.indexOf("/") > 0) {
                var dateParts = value.split("/");

                var dateEntered = new Date(dateParts[2], (dateParts[0] - 1), dateParts[1]);
                var currentDate = new Date();
                var minDate = new Date(currentDate.getFullYear() - 10, currentDate.getMonth(), currentDate.getDate());
                var maxDate = new Date(currentDate.getFullYear() + 10, currentDate.getMonth(), currentDate.getDate());

                return ((dateEntered > minDate) + (dateEntered < maxDate)) == 2;

            } else {
                return false;
            }
        }
        //Do nothing if no value is present
        return true;
    });

    $.validator.unobtrusive.adapters.add("datetenyearsvalid", ["datetovalidate"], function (options) {
        var prefix = options.element.name.substr(0, options.element.name.lastIndexOf(".") + 1);
        var datetovalidate = $(options.form).find(":input[name=" + appendModelPrefix(options.params.datetovalidate, prefix) + "]")[0];
        
        setValidationValues(options, "datetenyearsvalid", [ datetovalidate ]);
    });

    function appendModelPrefix(value, prefix) {
        if (value.indexOf("*.") === 0) {
            value = value.replace("*.", prefix);
        }
        value = value.split('.').join('\\.');
        return value;
    }
 
})();


/* ==========================================================================
    Utilities
   ========================================================================== */
R2V2.Utils = {
    GetEventTarget: function (event, node)
    {
        // get DOM and $ target
        var target = event.target, $target = $(target);

        if (node != null)
        {
            // if target is NOT "node", get first parent "node"
            $target = $target.is(node) === false && $target.parents(node + ':first') || $target;
            // get DOM target
            target = $target[0];
        }

        return { DOM: target, $: $target };
    },
    
    UpdateQueryStringParameter: function(uri, key, value)
    {
        var re = new RegExp("([?|&])" + key + "=.*?(&|$)", "i"),
            separator = uri.indexOf('?') !== -1 ? "&" : "?";
        
        if (uri.match(re)) {
            return uri.replace(re, '$1' + key + "=" + value + '$2');
        }
        return uri + separator + key + "=" + value;
    },

    GetQuerystring: function ()
    {
        return $.deparam.querystring();
    },

    GetUrlFragment: function ()
    {
        return $.deparam.fragment();
    },

    ToUrlParams: function (params)
    {
        return $.param(params);
    },

    GetCurrentUrlWithoutHash: function ()
    {
    	return window.location.href.replace(/#.*$/, "");
    },

    CopyToClipboard: function (textToCopy)
    {
    	$("body")
			.append($('<input type="text" name="hiddenCopyToClipboard" class="hiddenCopyToClipboard"/>')
			.val(textToCopy))
			.find(".hiddenCopyToClipboard")
            .select();

    	try {
    		document.execCommand('copy');
    	} catch (err) {
    		console.log("Error copying to clipboard: " + err);
        }

    	$(".hiddenCopyToClipboard").remove();
    },

    ReplaceAt: function (s, index, character)
    {
		// Replace character at index in string s
    	return s.substr(0, index) + character + s.substr(index+character.length);
    }
};