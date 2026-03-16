/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Pub/Sub
    http://addyosmani.com/blog/jquery-1-7s-callbacks-feature-demystified/
   ========================================================================== */
R2V2.PubSubMessages = {};
$.PubSub = function (id) {
    var callbacks, topic = id && R2V2.PubSubMessages[id];

    if (!topic) {
        callbacks = $.Callbacks();

        topic = {
            publish: callbacks.fire,
            subscribe: callbacks.add,
            unsubscribe: callbacks.remove
        };

        if (id) {
            R2V2.PubSubMessages[id] = topic;
        }
    }

    return topic;
};