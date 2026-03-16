#region

using Microsoft.Owin;
using Owin;
using R2V2.Web;

#endregion

/*
 * Required for OpenAthens Keystone
 */

[assembly: OwinStartup(typeof(Startup))]

namespace R2V2.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}