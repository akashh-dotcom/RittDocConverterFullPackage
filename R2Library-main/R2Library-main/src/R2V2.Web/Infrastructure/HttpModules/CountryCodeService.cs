#region

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Core.RequestLogger;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Infrastructure.HttpModules
{
    public class CountryCodeService
    {
        const string IpAddressToCountryCodeKey = "IpAddressToCountryCode";
        private static readonly ILog<CountryCodeService> Log = new Log<CountryCodeService>();

        public static string GetCountryCodeFromIpAddressFromDb(string ipAddress, HttpContext context)
        {
            var countryCodes = (StringDictionary)context.Cache[IpAddressToCountryCodeKey];
            if (countryCodes == null)
            {
                countryCodes = new StringDictionary();
                context.Cache.Insert(IpAddressToCountryCodeKey, countryCodes);
            }

            if (countryCodes.ContainsKey(ipAddress))
            {
                return countryCodes[ipAddress];
            }

            try
            {
                var ip = new IpAddress(ipAddress);

                var ip2Locations = ServiceLocator.Current.GetInstance<IQueryable<Ip2Location>>();
                var ip2Location =
                    ip2Locations.FirstOrDefault(x => x.IpTo <= ip.IntegerValue && x.IpFrom >= ip.IntegerValue);
                if (ip2Location == null)
                {
                    return null;
                }

                //Added another check before we insert. Throws an exception if 2 users coming from exproxy at same time.
                if (countryCodes.ContainsKey(ipAddress))
                {
                    return countryCodes[ipAddress];
                }

                countryCodes.Add(ipAddress, ip2Location.CountryCode);

                Log.DebugFormat("Ip2Location Lookup -> IP: {0}, {1}", ipAddress, ip2Location.ToDebugString());
                return ip2Location.CountryCode;
            }
            catch (Exception ex)
            {
                Log.WarnFormat("API call timeout - {0}", ex.Message);
                return null;
            }
        }
    }
}