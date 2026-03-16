#region

using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.OpenIdConnect;

using Owin;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Web.Infrastructure.Settings;

#endregion

/*
 * Required for OpenAthens Keystone
 */

namespace R2V2.Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);
            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            var oidcSettings = ServiceLocator.Current.GetInstance<IOidcSettings>();
            var oidcOptions = new OpenIdConnectAuthenticationOptions
            {
                Authority = oidcSettings.Authority,
                ClientId = oidcSettings.ClientId,
                ClientSecret = oidcSettings.ClientSecret,
                PostLogoutRedirectUri = oidcSettings.RedirectUrl,
                RedirectUri = oidcSettings.RedirectUrl,
                ResponseType = OpenIdConnectResponseType.Code,
                Scope = OpenIdConnectScope.OpenId,
                Notifications = new OpenIdConnectAuthenticationNotifications
                {
                    // Suppress redirect for /api routes
                    RedirectToIdentityProvider = notification =>
                    {
                        // Check if the request is for an API route
                        if (notification.Request.Path.StartsWithSegments(new PathString("/api")))
                        {
                            // Skip the redirect and let the 401 pass through
                            notification.HandleResponse();
                            notification.OwinContext.Response.StatusCode = 401;
                        }

                        return Task.CompletedTask;
                    }
                }
            };
            app.UseOpenIdConnectAuthentication(oidcOptions);
        }
    }
}