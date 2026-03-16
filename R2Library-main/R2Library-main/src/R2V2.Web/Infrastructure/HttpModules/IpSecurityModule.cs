#region

using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Caching;

using R2V2.Infrastructure.DependencyInjection;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Helpers;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.HttpModules
{
    public class IpSecurityModule : IHttpModule
    {
        const string IpBlackListKey = "BlackListedIps";
        const string IpBlackListFile = "IpAddressBlackList.txt";
        const string IpWhiteListKey = "WhiteListedIps";
        const string IpWhiteListFile = "IpAddressWhiteList.txt";
        const string CountryCodeBlackListKey = "BlockedCountryCodes";
        const string BlockedIpsByCountryCodeKey = "BlockedIpsByCountryCode";
        const string AllowedIpsByCountryCodeKey = "AllowedIpsByCountryCode";
        const string AllowedCountryCodeKey = "AllowedCountryCode";
        const string CountryCodeBlackListFile = "CountryCodesBlackList.txt";

        private static readonly ILog<IpSecurityModule> Log = new Log<IpSecurityModule>();

        private static readonly string[] DependencyFiles =
            { IpBlackListFile, IpWhiteListFile, CountryCodeBlackListFile };

        private readonly EventHandler _onBeginRequest;
        private IWebSettings _webSettings;

        /// <summary>
        ///     From SCOTT HANSELMAN - An IP Address Blocking HttpModule for ASP.NET in 9 minutes
        ///     SJS - Modified as needed to support blocking IP by country code and scheiderize it. (Meaning, log shit!)
        ///     http://www.hanselman.com/blog/AnIPAddressBlockingHttpModuleForASPNETIn9Minutes.aspx
        /// </summary>
        public IpSecurityModule()
        {
            _onBeginRequest = HandleBeginRequest;
            //Log.Debug("IpSecurityModule() >> <<");
        }

        void IHttpModule.Dispose()
        {
            //Log.Debug("Dispose() >> <<");
        }

        void IHttpModule.Init(HttpApplication context)
        {
            //Log.Debug("Init() <<");
            context.BeginRequest += _onBeginRequest;
            _webSettings = ServiceLocator.Current.GetInstance<IWebSettings>();
            //Log.Debug("Init() <<");
        }

        private void HandleBeginRequest(object sender, EventArgs evargs)
        {
            var app = sender as HttpApplication;

            if (app != null)
            {
                var ipAddress = app.Context.Request.GetHostIpAddress();
                if (string.IsNullOrEmpty(ipAddress))
                {
                    return;
                }

                // is IP white listed?
                var whiteListedIps =
                    _ipWhiteListFile.GetList(app.Context, _webSettings.IpSecurityCacheTimeToLiveInMinutes);
                if (whiteListedIps.ContainsKey(ipAddress))
                {
                    return;
                }

                // is ip address blocked/black listed
                var badIps = _ipBlackListFile.GetList(app.Context, _webSettings.IpSecurityCacheTimeToLiveInMinutes);
                if (badIps != null && badIps.ContainsKey(ipAddress))
                {
                    var count = badIps[ipAddress];
                    count++;
                    badIps[ipAddress] = count;
                    Log.WarnFormat("BLOCKED IP ADDRESS: {0}, block count: {1}", ipAddress, count);
                    app.Context.Response.StatusCode = 403;
                    //app.Context.Response.SuppressContent = true;
                    app.Context.Response.Write(
                        $"HTTP Error 403.0 - Forbidden - r2library.com is currently blocking your IP address, {ipAddress}. Please contact Rittenhouse Customer Service to resolve this issue.");
                    app.Context.Response.End();
                }
                else if (IsCountryCodeBlocked(app.Context, ipAddress))
                {
                    Log.WarnFormat("BLOCKED IP ADDRESS: {0}", ipAddress);
                    app.Context.Response.StatusCode = 403;
                    app.Context.Response.Write(
                        $"HTTP Error 403.0 - Forbidden - r2library.com is currently blocking your IP address, {ipAddress}. Please contact Rittenhouse Customer Service at to resolve this issue. (Blocked Country Code)");
                    app.Context.Response.End();
                }
            }
        }

        private bool IsCountryCodeBlocked(HttpContext context, string ipAddress)
        {
            if (IsIpAddressInBlockedList(context, ipAddress))
            {
                Log.InfoFormat("IP address in blocked list, ip: {0}", ipAddress);
                return true;
            }

            if (IsIpAddressInAllowedList(context, ipAddress))
            {
                //Log.DebugFormat("IP address in allowed list, ip: {0}", ipAddress);
                return false;
            }

            var countryCode = CountryCodeService.GetCountryCodeFromIpAddressFromDb(ipAddress, context);
            if (countryCode == null)
            {
                return false;
            }

            var blockedCountryCodes =
                _countryCodeBlackListFile.GetList(context, _webSettings.IpSecurityCacheTimeToLiveInMinutes);
            if (blockedCountryCodes.ContainsKey(countryCode))
            {
                // ip address is blocked because the country code is blocked.
                var count = blockedCountryCodes[countryCode];
                count++;
                blockedCountryCodes[ipAddress] = count;
                Log.InfoFormat("block country code: {0}, count: {1}", countryCode, count);
                AddIpAddressToBlockedList(context, ipAddress);
                return true;
            }

            LogAllowedCountryCodes(countryCode, context);

            // ip address is allowed because the country code is allowed.
            AddIpAddressToAllowedList(context, ipAddress);
            return false;
        }

        private bool IsIpAddressInAllowedList(HttpContext context, string ipAddress)
        {
            var allowedIps = (Dictionary<string, int>)context.Cache[AllowedIpsByCountryCodeKey];
            if (allowedIps != null && allowedIps.ContainsKey(ipAddress))
            {
                AddIpAddressToAllowedList(context, ipAddress, allowedIps);
                return true;
            }

            return false;
        }

        private void AddIpAddressToAllowedList(HttpContext context, string ipAddress)
        {
            var allowedIps = (Dictionary<string, int>)context.Cache[AllowedIpsByCountryCodeKey];
            AddIpAddressToAllowedList(context, ipAddress, allowedIps);
        }

        private void AddIpAddressToAllowedList(HttpContext context, string ipAddress,
            Dictionary<string, int> allowedIps)
        {
            if (allowedIps == null)
            {
                allowedIps = new Dictionary<string, int> { { ipAddress, 1 } };
                context.Cache.Insert(AllowedIpsByCountryCodeKey, allowedIps,
                    new CacheDependency(_countryCodeBlackListFile.GetFromCurrentContext(context)));
                Log.DebugFormat("AddIpAddressToAllowedList(isAddress: {0}) - allowedIps.Count: {1}", ipAddress,
                    allowedIps.Count);
                return;
            }

            if (allowedIps.ContainsKey(ipAddress))
            {
                var count = allowedIps[ipAddress];
                count++;
                allowedIps[ipAddress] = count;
                return;
            }

            allowedIps.Add(ipAddress, 1);
            Log.DebugFormat("AddIpAddressToAllowedList(isAddress: {0}) - allowedIps.Count: {1}", ipAddress,
                allowedIps.Count);
        }

        private bool IsIpAddressInBlockedList(HttpContext context, string ipAddress)
        {
            var blockedIps = (Dictionary<string, int>)context.Cache[BlockedIpsByCountryCodeKey];
            if (blockedIps != null && blockedIps.ContainsKey(ipAddress))
            {
                AddIpAddressToBlockedList(context, ipAddress, blockedIps);
                return true;
            }

            return false;
        }

        private void AddIpAddressToBlockedList(HttpContext context, string ipAddress)
        {
            var blockedIps = (Dictionary<string, int>)context.Cache[BlockedIpsByCountryCodeKey];
            AddIpAddressToBlockedList(context, ipAddress, blockedIps);
        }

        private void AddIpAddressToBlockedList(HttpContext context, string ipAddress,
            Dictionary<string, int> blockedIps)
        {
            if (blockedIps == null)
            {
                Log.DebugFormat("AddIpAddressToBlockedList() - blockedIps is null, ip: {0}", ipAddress);
                blockedIps = new Dictionary<string, int> { { ipAddress, 1 } };
                context.Cache.Insert(BlockedIpsByCountryCodeKey, blockedIps,
                    new CacheDependency(_countryCodeBlackListFile.GetFromCurrentContext(context)));
                return;
            }

            if (blockedIps.ContainsKey(ipAddress))
            {
                var count = blockedIps[ipAddress];
                count++;
                blockedIps[ipAddress] = count;
                Log.DebugFormat("AddIpAddressToBlockedList() - blockedIps contained ip, ip: {0}, count: {1}", ipAddress,
                    count);
                return;
            }

            blockedIps.Add(ipAddress, 1);
            Log.DebugFormat("AddIpAddressToBlockedList() - blockedIps contained ip, ip: {0}, count: 1", ipAddress);
        }

        private void LogAllowedCountryCodes(string countryCode, HttpContext context)
        {
            var allowedCountryCodes = (AllowedCountryCodes)context.Cache[AllowedCountryCodeKey];
            if (allowedCountryCodes == null)
            {
                allowedCountryCodes = new AllowedCountryCodes();
                context.Cache.Insert(AllowedCountryCodeKey, allowedCountryCodes);
            }

            allowedCountryCodes.IncrementAllowedCountryCodeCount(countryCode);
        }

        // ReSharper disable InconsistentNaming
        private static readonly IpSecurityFileData _ipBlackListFile =
            new IpSecurityFileData(IpBlackListFile, IpBlackListKey, DependencyFiles);

        private static readonly IpSecurityFileData _ipWhiteListFile =
            new IpSecurityFileData(IpWhiteListFile, IpWhiteListKey, DependencyFiles);

        private static readonly IpSecurityFileData _countryCodeBlackListFile =
            new IpSecurityFileData(CountryCodeBlackListFile, CountryCodeBlackListKey, DependencyFiles);
        // ReSharper restore InconsistentNaming
    }
}