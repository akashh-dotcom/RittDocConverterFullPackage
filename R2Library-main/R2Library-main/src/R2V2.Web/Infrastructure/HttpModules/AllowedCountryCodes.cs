#region

using System.Collections.Generic;
using System.Text;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Infrastructure.HttpModules
{
    public class AllowedCountryCodes
    {
        private static readonly ILog<AllowedCountryCodes> Log = new Log<AllowedCountryCodes>();
        private readonly Dictionary<string, int> _allowedCountryCodes = new Dictionary<string, int>();

        private long _allowedRequestCount;

        public void IncrementAllowedCountryCodeCount(string countryCode)
        {
            _allowedRequestCount++;
            if (_allowedCountryCodes.ContainsKey(countryCode))
            {
                var count = _allowedCountryCodes[countryCode] + 1;
                _allowedCountryCodes[countryCode] = count;
            }
            else
            {
                _allowedCountryCodes.Add(countryCode, 1);
                var infoMsg = new StringBuilder();
                infoMsg.AppendFormat(
                    "_allowedRequestCount: {0}, _allowedCountryCodes.Count: {1}, _allowedCountryCodes = [",
                    _allowedRequestCount, _allowedCountryCodes.Count);
                foreach (var allowedCountryCode in _allowedCountryCodes)
                {
                    infoMsg.AppendFormat("{0}={1}, ", allowedCountryCode.Key, allowedCountryCode.Value);
                }

                infoMsg.Append("]");
                Log.Info(infoMsg);
            }
        }
    }
}