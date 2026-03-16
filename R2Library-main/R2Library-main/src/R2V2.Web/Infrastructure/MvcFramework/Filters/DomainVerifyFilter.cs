#region

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using Newtonsoft.Json;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Infrastructure.MvcFramework.Filters
{
    public class DomainVerifyFilter : AuthorizeAttribute
    {
        private static readonly ILog<DomainVerifyFilter> Log = new Log<DomainVerifyFilter>();

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            try
            {
                var filename = Path.Combine(HttpContext.Current.Server.MapPath("/_configs"), "DomainVerify.json");
                var domains = (Domains)HttpContext.Current.Cache["R2v2.DomainsToVerify"];

                if (domains == null)
                {
                    var jsonFileText = File.ReadAllText(filename);
                    domains = JsonConvert.DeserializeObject<Domains>(jsonFileText);
                    HttpContext.Current.Cache.Insert("R2v2.DomainsToVerify", domains, new CacheDependency(filename),
                        Cache.NoAbsoluteExpiration,
                        new TimeSpan(0, 25, 0));
                }

                if (domains != null && domains.EnableDomainVerification)
                {
                    var url = HttpContext.Current.Request.Url.ToString();
                    var host = HttpContext.Current.Request.Url.Host;
                    var domainPair = domains.DomainPairs.FirstOrDefault(x =>
                        x.IncomingDomain.Equals(host, StringComparison.OrdinalIgnoreCase));
                    if (domainPair != null)
                    {
                        var redirectTo = url.Replace(domainPair.IncomingDomain, domainPair.PrimaryDomain);
                        Log.InfoFormat("Redirecting to primary domain, redirectTo: {0} -- incomming url: ", redirectTo,
                            url);
                        //HttpContext.Current.Response.Redirect(redirectTo, true);
                        filterContext.Result = new RedirectResult(redirectTo);
                    }
                }
            }
            catch (ThreadAbortException taEx)
            {
                // swallow - sjs - 3/18/2016
                Log.Debug(taEx.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
            }
        }
    }
}