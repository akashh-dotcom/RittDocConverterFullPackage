var R2V2 = R2V2 || { };

/* ==========================================================================
    Service
   ========================================================================== */
R2V2.ServiceExecutionOptions = {
    Format: {
        JSON: 'json',
        JSONP: 'jsonp',
        HTML: 'html',
        TEXT: 'text',
        CUSTOM: 'custom'
    },
    ContentType: {
        DEFAULT: 'application/x-www-form-urlencoded; charset=UTF-8',
        JSON: 'application/json; charset=utf-8'
    }
};