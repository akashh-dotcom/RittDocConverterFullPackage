/* ==========================================================================
    Modal
    
    Modified version of:
    bootstrap-modal.js v2.0.3
    http://twitter.github.com/bootstrap/javascript.html#modals
    =========================================================
    Copyright 2012 Twitter, Inc.
    
    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at
    
    http://www.apache.org/licenses/LICENSE-2.0
    
    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
    ========================================================================== */
; (function ($, window, document, undefined) {

    "use strict"; // jshint ;_;

    /* MODAL CLASS DEFINITION
     * ====================== */

    var Modal = function (content, options) {
        this.options = options;

        this.$element = $(content).delegate('[data-dismiss="modal"]', 'click.dismiss.modal', $.proxy(this.hide, this));
    };
    Modal.prototype = {
        constructor: Modal,

        toggle: function (_relatedTarget) {
            return this[!this.isShown ? 'show' : 'hide'](_relatedTarget);
        },

        show: function (_relatedTarget) {
            var that = this, e = $.Event('show', { relatedTarget: _relatedTarget });

            this.$element.trigger(e);

            if (this.isShown || e.isDefaultPrevented()) return;

            $('body').addClass('modal-open');

            this.isShown = true;

            this.$element.attr('aria-modal', true);

            this.enforceFocus();

            escape.call(this);
            backdrop.call(this, function () {
                var transition = $.support.transition && that.$element.hasClass('fade');

                if (!that.$element.parent().length) {
                    that.$element.appendTo(document.body); //don't move modals dom position
                }

                that.$element.show();

                if (transition) {
                    that.$element[0].offsetWidth; // force reflow
                }

                that.$element.addClass('in');

                var transitionComplete = function () {
                    that.$element.focus();
                    that.$element.trigger('shown');
                };

                if (transition) {
                    that.$element.one($.support.transition.end, transitionComplete)
                } else {
                    transitionComplete();
                }    
            });
        },

        hide: function (e) {
            e && e.preventDefault();

            var that = this;
            e = $.Event('hide');

            this.$element.trigger(e);

            if (!this.isShown || e.isDefaultPrevented()) return;

            this.isShown = false;

            this.$element.removeAttr('aria-modal');

            $(document).off('focusin');

            $('body').removeClass('modal-open');

            escape.call(this);

            this.$element.removeClass('in');

            $.support.transition && this.$element.hasClass('fade') ? hideWithTransition.call(this) : hideModal.call(this);
        },

        enforceFocus: function () {
            var that = this;

            $(document)
                .off('focusin') // Guard against infinite focus loop
                .on('focusin', function (event) {
                    var element = that.$element[0];

                    if (document !== event.target && element !== event.target && that.$element.has(event.target).length === 0) {
                        that.$element.focus();
                    }
                });
        }
    };


    /* MODAL PRIVATE METHODS
     * ===================== */

    function hideWithTransition() {
        var that = this,
            timeout = setTimeout(function () {
                that.$element.off($.support.transition.end);
                hideModal.call(that);
            }, 500);

        this.$element.one($.support.transition.end, function () {
            clearTimeout(timeout);
            hideModal.call(that);
        });
    }

    function hideModal(that) {
        this.$element.hide().trigger('hidden');

        backdrop.call(this);
    }

    function backdrop(callback) {
        var that = this, animate = this.$element.hasClass('fade') ? 'fade' : '';

        if (this.isShown && this.options.backdrop) {
            var doAnimate = $.support.transition && animate;

            this.$backdrop = $('<div class="modal-backdrop ' + animate + '" />').appendTo(document.body);

            if (this.options.backdrop != 'static') {
                this.$backdrop.click($.proxy(this.hide, this));
            }

            if (doAnimate) {
                this.$backdrop[0].offsetWidth; // force reflow
            }

            this.$backdrop.addClass('in');

            doAnimate ? this.$backdrop.one($.support.transition.end, callback) : callback();
        }
        else if (!this.isShown && this.$backdrop) {
            this.$backdrop.removeClass('in');

            $.support.transition && this.$element.hasClass('fade') ? this.$backdrop.one($.support.transition.end, $.proxy(removeBackdrop, this)) : removeBackdrop.call(this);
        }
        else if (callback) {
            callback();
        }
    }

    function removeBackdrop() {
        this.$backdrop.remove();
        this.$backdrop = null;
    }

    function escape() {
        var that = this;
        if (this.isShown && this.options.keyboard) {
            $(document).on(R2V2.KeyboardMappings.Modal.Close, function (e) {
                that.hide();
            });
        }
        else if (!this.isShown) {
            $(document).off(R2V2.KeyboardMappings.Modal.Close);
        }
    }


    /* MODAL PLUGIN DEFINITION
    * ======================= */

    $.fn.modal = function (option, _relatedTarget) {
        return this.each(function () {
            var $this = $(this),
                data = $this.data('modal'),
                options = $.extend({}, $.fn.modal.defaults, $this.data(), typeof option == 'object' && option);

            if (!data) $this.data('modal', (data = new Modal(this, options)));

            if (typeof option == 'string') data[option](_relatedTarget);
            else if (options.show) data.show(_relatedTarget);
        });
    };

    $.fn.modal.defaults = {
        backdrop: true,
        keyboard: true,
        show: true
    };

    $.fn.modal.Constructor = Modal;


    /* MODAL DATA-API
     * ============== */
    $(function () {
        $('body').on('click.modal.data-api', '[data-toggle="modal"]', function (e) {
            var $this = $(this),
                href,
                $target = $($this.attr('data-target') || (href = $this.attr('href')) && href.replace(/.*(?=#[^\s]+$)/, '')), //strip for ie7
                option = $target.data('modal') ? 'toggle' : $.extend({}, $target.data(), $this.data());

            e.preventDefault();
            $target.modal(option, this);
            $target.data({ trigger: $this });
        });
    });
})(jQuery, window, document);