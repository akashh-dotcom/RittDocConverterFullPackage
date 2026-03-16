#region

using System.Web.Optimization;
using R2V2.Infrastructure.Initializers;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure
{
    public class BundleInitializer : IInitializer
    {
        private readonly IWebSettings _webSettings;

        public BundleInitializer(IWebSettings webSettings)
        {
            _webSettings = webSettings;
        }

        public void Initialize()
        {
            RegisterBundles(BundleTable.Bundles);
        }

        private void RegisterBundles(BundleCollection bundles)
        {
            /*
             SCRIPTS
             */
            var scripts = _webSettings.MinifyJavascript
                ? new Bundle("~/_Static/bundles/js", new JsMinify())
                : new Bundle("~/_Static/bundles/js");

            // Libraries
            scripts.Include("~/_Static/Scripts/libraries/underscore-min.js");
            scripts.Include("~/_Static/Scripts/libraries/simple-inheritance.js");
            scripts.Include("~/_Static/Scripts/libraries/jquery.unobtrusive-ajax.min.js");
            scripts.Include("~/_Static/Scripts/libraries/jquery.validate.min.js");
            scripts.Include("~/_Static/Scripts/libraries/jquery.validate.unobtrusive.min.js");
            scripts.Include("~/_Static/Scripts/libraries/jquery.ba-bbq.min.js");
            scripts.Include("~/_Static/Scripts/libraries/jquery-ui.min.js");
            // Third-party Plugins
            scripts.Include("~/_Static/Scripts/packages/jquery.placeholder.js");
            scripts.Include("~/_Static/Scripts/packages/jquery.column.js");
            scripts.Include("~/_Static/Scripts/packages/jquery.hotkeys.js");
            scripts.IncludeDirectory("~/_Static/Scripts/packages/twitter", "*.js");
            scripts.Include("~/_Static/Scripts/packages/flowplayer-5.4.3/flowplayer.js");
            scripts.Include("~/_Static/Scripts/packages/slick/slick.min.js");
            scripts.Include("~/_Static/Scripts/packages/selectize/selectize.min.js");
            // Lookup Objects
            scripts.IncludeDirectory("~/_Static/Scripts/lookups", "*.js");
            // Core
            scripts.Include("~/_Static/Scripts/core/utilities.js");
            scripts.Include("~/_Static/Scripts/core/service.js");
            scripts.Include("~/_Static/Scripts/core/pubsub.js");
            scripts.Include("~/_Static/Scripts/core/history.js");
            scripts.Include("~/_Static/Scripts/core/polyfills.js");
            scripts.Include("~/_Static/Scripts/core/security.js");
            scripts.Include("~/_Static/Scripts/core/links.js");
            scripts.Include("~/_Static/Scripts/core/faceting.js");
            scripts.Include("~/_Static/Scripts/core/panel.js");
            scripts.Include("~/_Static/Scripts/core/actions-menu.js");
            scripts.Include("~/_Static/Scripts/core/help.js");
            // Components
            scripts.IncludeDirectory("~/_Static/Scripts/components", "*.js");
            // Functional Areas (business logic)
            scripts.IncludeDirectory("~/_Static/Scripts/functional-areas", "*.js");

            bundles.Add(scripts);

            /*
             SITE STYLES
             */
            var styles = new Bundle("~/_Static/bundles/css"); //, new CssMinify());
            // Reset
            styles.Include("~/_Static/Css/packages/h5bp/reset.css");
            // Base Layout
            styles.Include("~/_Static/Css/core/layout.css");
            // Grid system and page structure
            styles.Include("~/_Static/Css/base/grid.css");
            // Base CSS
            styles.Include("~/_Static/Css/base/typography.css");
            styles.Include("~/_Static/Css/base/forms.css");
            styles.Include("~/_Static/Css/base/tables.css");
            styles.Include("~/_Static/Css/base/content.css");
            // Components
            styles.IncludeDirectory("~/_Static/Css/packages/twitter", "*.css", true);
            styles.Include("~/_Static/Css/packages/flowplayer-5.4.3/minimalist.css");
            styles.Include("~/_Static/Css/packages/slick/slick.css");
            styles.Include("~/_Static/Css/packages/selectize/selectize.css");
            styles.IncludeDirectory("~/_Static/Css/components", "*.css");
            // Specific "page" or "functional" CSS
            styles.IncludeDirectory("~/_Static/Css/functional-areas", "*.css");
            // Media Queries
            styles.Include("~/_Static/Css/core/media-queries.css");
            // Utility (non-semantic helper) classes
            styles.Include("~/_Static/Css/packages/h5bp/helper-classes.css");
            styles.Include("~/_Static/Css/core/utilities.css");
            // Print
            styles.Include("~/_Static/Css/core/print.css");
            // Font
            styles.IncludeDirectory("~/_Static/Css/packages/font-awesome-4.7.0", "*.css", true);
            // Libraries
            styles.IncludeDirectory("~/_Static/Css/libraries", "*.css");

            bundles.Add(styles);

            /*
             EMAIL STYLES
             */
            var emailStyles = new Bundle("~/_Static/Css/r2.email.css", new CssMinify());

            // Reset
            emailStyles.Include("~/_Static/Css/packages/h5bp/reset.css");
            // Base Layout
            emailStyles.Include("~/_Static/Css/core/layout.email.css");
            // Grid system and page structure
            emailStyles.Include("~/_Static/Css/base/grid.css");
            // Base CSS
            emailStyles.Include("~/_Static/Css/base/typography.css");
            emailStyles.Include("~/_Static/Css/base/content.css");
            // Components
            emailStyles.Include("~/_Static/Css/components/alertbox.css");
            emailStyles.Include("~/_Static/Css/components/featurebox.css");
            emailStyles.Include("~/_Static/Css/components/figures.css");
            // Specific "page" or "functional" CSS
            emailStyles.Include("~/_Static/Css/functional-areas/admin.css");
            // Utility (non-semantic helper) classes
            emailStyles.Include("~/_Static/Css/packages/h5bp/helper-classes.css");

            bundles.Add(emailStyles);
        }
    }
}