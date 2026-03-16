/// <reference path="/_Static/Scripts/libraries/jquery-1.7.1-vsdoc.js"/>

var R2V2 = R2V2 || { };

/* ==========================================================================
    Authentication
   ========================================================================== */
R2V2.Auth = Class.extend({
    init: function (config)
    {
        this.config = config || {};
        this.loginConfig = this.config.login;
        this.forgotPasswordConfig = this.config.forgotPassword;
        this.authConfirmationConfig = this.config.authConfirmation;
        this.submitConfig = this.config.submit;
        
        // error checks
        if (this.loginConfig == null || _.isEmpty(this.loginConfig)) return;
        if (this.forgotPasswordConfig == null || _.isEmpty(this.forgotPasswordConfig)) return;
        if (this.authConfirmationConfig == null || _.isEmpty(this.authConfirmationConfig)) return;
        if (this.submitConfig == null || _.isEmpty(this.submitConfig)) return;
        
        // cache method calls
        this.f = {
            onLoginTrigger: $.proxy(this.onLoginTrigger, this),
            onForgotPasswordTrigger: $.proxy(this.onForgotPasswordTrigger, this)
        };

        this.attachEvents();
    },
    
    attachEvents: function ()
    {
        this.loginConfig.$trigger.on('click', this.f.onLoginTrigger);
        this.forgotPasswordConfig.$trigger.on('click', this.f.onForgotPasswordTrigger);
    },

    onLoginTrigger: function (event)
    {
        event.preventDefault();
        
        this.toggle({ show: this.forgotPasswordConfig, hide: this.loginConfig });
        this.forgotPasswordConfig.$userNameField.val(this.loginConfig.$userNameField.val());
    },
    
    onForgotPasswordTrigger: function ()
    {
        this.toggle({ show: this.loginConfig, hide: this.forgotPasswordConfig });
    },
    
    toggle: function(config)
    {
        config.show.$container['show']();
        config.hide.$container['hide']();
        config.hide.$container.find('.validation-message').empty();
    },
    
    authenticate: function(response, config)
    {
        this.authResult = $.parseJSON(response.responseText);
        
        if (this.authResult.Successful === false)
        {
            config.$container.find('.validation-message').empty().html('<p>' + this.authResult.ErrorMessage + '</p>');
            return false;
        }

        if (this.authResult.InstitutionHomePage && R2V2.Config.IsMarketingHome) {
            window.location = this.authResult.InstitutionHomePage;
            return false;
        }

        if (this.authResult.RedirectUrl)
        {
            window.location = this.authResult.RedirectUrl;
            return false;
        }

        return true;
    },
    
    login: function (response)
    {
        if (this.authenticate(response, this.loginConfig) == false)
        {
            this.submitConfig.$button.removeAttr('disabled');
            return;
        }
        
        location.reload(true);
    },

    disableSubmit: function ()
    {
        this.submitConfig.$button.attr('disabled', 'disabled');
    },

    forgotPassword: function (response)
    {
        if (this.authenticate(response, this.forgotPasswordConfig) == false) return;

        this.toggle({ show: this.loginConfig, hide: this.forgotPasswordConfig });
        this.confirm(this.authConfirmationConfig);
    },

    confirm: function (config)
    {
        config.$container.empty().html(config.message);

        setTimeout(function() {
            config.$container.find('p').fadeOut();
        }, 6000);
    }

});


/* ==========================================================================
    Create OnDOMReady
    ========================================================================== */
; (function ($) {
    
    R2V2.Authentication = new R2V2.Auth({
        login: { $container: $('#login'), $trigger: $('.login-links a[href="#forgot-password"]'), $userNameField: $('#login-user') },
        forgotPassword: { $container: $('#forgot-password'), $trigger: $('#forgot-password button[type="reset"]'), $userNameField: $('#forgotpassword-user') },
        authConfirmation: { $container: $('#auth-confirmation'), message: R2V2.Messages.EmailPassword },
        submit: { $button: $('#login button[type="submit"]'), enabled: true }
    });

    R2V2.Authentication.Login = _.bind(R2V2.Authentication.login, R2V2.Authentication);
    R2V2.Authentication.ForgotPassword = _.bind(R2V2.Authentication.forgotPassword, R2V2.Authentication);
    R2V2.Authentication.DisableSubmit = _.bind(R2V2.Authentication.disableSubmit, R2V2.Authentication);

})(jQuery);