#region

using System;
using System.Web;
using Newtonsoft.Json;
using R2V2.Infrastructure.Cryptography;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Web.Infrastructure.Authentication
{
    public class PassiveAuthenticationCookieService
    {
        private const string PassiveAuthCookieName = "R2-PassiveAuth";
        private static readonly RijndaelCipher RijndaelCipher = new RijndaelCipher();
        private readonly ILog<PassiveAuthenticationCookieService> _log;

        public PassiveAuthenticationCookieService(ILog<PassiveAuthenticationCookieService> log)
        {
            _log = log;
        }

        /// <summary>
        ///     Set passive authentication cookie after a successful IP Authentication
        /// </summary>
        public void SetPassiveAuthenticationCookie(int instititionId, string ipAddress)
        {
            try
            {
                var passiveAuthenticationCookie = new PassiveAuthenticationCookie
                {
                    PassiveAuthenticationType = PassiveAuthenticationType.IpAuthentication,
                    Timestamp = DateTime.Now,
                    InstitutionId = instititionId,
                    IpAddress = ipAddress
                };
                SetCookie(passiveAuthenticationCookie);
            }
            catch (Exception ex)
            {
                _log.Error($"SetPassiveAuthenticationCookie(IP) Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Set passive authentication cookie after a successful Referrer Authentication
        /// </summary>
        public void SetPassiveAuthenticationCookie(int instititionId, string ipAddress, string referrer,
            string accountNumber)
        {
            try
            {
                var passiveAuthenticationCookie = new PassiveAuthenticationCookie
                {
                    PassiveAuthenticationType = PassiveAuthenticationType.ReferrerAuthentication,
                    Timestamp = DateTime.Now,
                    InstitutionId = instititionId,
                    IpAddress = ipAddress,
                    Referrer = referrer,
                    AccountNumber = accountNumber
                };
                SetCookie(passiveAuthenticationCookie);
            }
            catch (Exception ex)
            {
                _log.Error($"SetPassiveAuthenticationCookie(referrer) Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Set passive authentication cookie after a successful Trusted Authentication
        /// </summary>
        public void SetPassiveAuthenticationCookie(int instititionId, string ipAddress, string accountNumber,
            string hashKey, string timestamp)
        {
            try
            {
                var passiveAuthenticationCookie = new PassiveAuthenticationCookie
                {
                    PassiveAuthenticationType = PassiveAuthenticationType.TrustedAuthentication,
                    Timestamp = DateTime.Now,
                    InstitutionId = instititionId,
                    IpAddress = ipAddress,
                    AccountNumber = accountNumber,
                    TrushedAuthHash = hashKey,
                    TrushedAuthTimestamp = timestamp
                };
                SetCookie(passiveAuthenticationCookie);
            }
            catch (Exception ex)
            {
                _log.Error($"SetPassiveAuthenticationCookie(trusted) Exception: {ex.Message}", ex);
            }
        }

        public int GetInstitutionIdFromPassiveAuthCookie()
        {
            try
            {
                var cookieValue = GetCookieValue();
                if (string.IsNullOrWhiteSpace(cookieValue))
                {
                    //_log.Debug("GetInstitutionIdFromPassiveAuthCookie() = 0 - cookie was null");
                    return 0;
                }

                var urlDecodedValue = HttpUtility.UrlDecode(cookieValue);
                _log.Debug($"GetInstitutionIdFromPassiveAuthCookie() - urlDecodedValue: {urlDecodedValue}");

                var wrapper = JsonConvert.DeserializeObject<PassiveAuthenticationCookieWrapper>(urlDecodedValue);

                var passiveAuthenticationCookie = GetDencryptedCookieValue(wrapper.Value);
                if (passiveAuthenticationCookie != null)
                {
                    _log.Debug(
                        $"GetInstitutionIdFromPassiveAuthCookie() = {passiveAuthenticationCookie.InstitutionId}");
                    return passiveAuthenticationCookie.InstitutionId;
                }

                _log.Debug("GetInstitutionIdFromPassiveAuthCookie() = 0 - parsing cookie value failed");
                return 0;
            }
            catch (Exception ex)
            {
                _log.Error($"GetInstitutionIdFromPassiveAuthCookie() = 0 - Exception: {ex.Message}", ex);
                return 0;
            }
        }

        private string GetEncryptedCookieValue(PassiveAuthenticationCookie passiveAuthenticationCookie)
        {
            var valueJson = JsonConvert.SerializeObject(passiveAuthenticationCookie, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            _log.Debug($"GetEncryptedCookieValue() - valueJson: {valueJson}");

            var encryptedValue = RijndaelCipher.Encrypt(valueJson);
            _log.Debug($"GetEncryptedCookieValue() - encryptedValue: {encryptedValue}");

            return encryptedValue;
        }

        private PassiveAuthenticationCookie GetDencryptedCookieValue(string cookieValue)
        {
            var json = RijndaelCipher.Decrypt(cookieValue);
            _log.Debug($"GetDencryptedCookieValue() - json: {json}");
            var passiveAuthenticationCookie = JsonConvert.DeserializeObject<PassiveAuthenticationCookie>(json);
            return passiveAuthenticationCookie;
        }

        public void ClearCookie()
        {
            var cookie = HttpContext.Current.Request.Cookies[PassiveAuthCookieName];
            HttpContext.Current.Response.Cookies.Remove(PassiveAuthCookieName);
            if (cookie != null)
            {
                cookie.Expires = DateTime.Now.AddDays(-10);
                cookie.Value = null;
                HttpContext.Current.Response.SetCookie(cookie);
            }
        }

        private void SetCookie(PassiveAuthenticationCookie passiveAuthenticationCookie)
        {
            var wrapper = new PassiveAuthenticationCookieWrapper
            {
                Version = 1,
                Timestamp = DateTime.Now,
                Value = GetEncryptedCookieValue(passiveAuthenticationCookie)
            };

            var cookieJson = JsonConvert.SerializeObject(wrapper, new JsonSerializerSettings
            {
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            _log.Debug($"SetCookie() - cookieJson: {cookieJson}");

            var encodedJson = HttpUtility.UrlEncode(cookieJson);
            _log.Debug($"SetCookie() - encodedJson: {encodedJson}");

            var httpCookie = new HttpCookie(PassiveAuthCookieName)
            {
                Value = encodedJson,
                Expires = DateTime.MinValue
            };
            var response = HttpContext.Current.Response;
            response.Cookies.Add(httpCookie);
        }

        private string GetCookieValue()
        {
            var httpCookie = HttpContext.Current.Request.Cookies[PassiveAuthCookieName];
            var value = httpCookie?.Value;
            _log.Debug($"GetCookieValue() - Cookie: {PassiveAuthCookieName} = {value}");
            return value;
        }
    }
}