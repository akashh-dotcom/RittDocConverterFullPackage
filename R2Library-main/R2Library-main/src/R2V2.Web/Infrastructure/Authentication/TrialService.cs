#region

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using R2V2.Infrastructure.Logging;
using R2V2.Web.Infrastructure.Settings;

#endregion

namespace R2V2.Web.Infrastructure.Authentication
{
    public class TrialService
    {
        private readonly IClientSettings _clientSettings;
        private readonly ILog<TrialService> _log;

        public TrialService(ILog<TrialService> log, IClientSettings clientSettings)
        {
            _log = log;
            _clientSettings = clientSettings;
        }

        public RittenhouseCustomer ValidateRittenhouseAccount(string userName, string password)
        {
            var rittenhouseCustomer = new RittenhouseCustomer();
            try
            {
                var dateTimeStamp = $"{DateTime.Now:yyyyMMddHHmmss}";
                var hash = GetHash(userName, dateTimeStamp);


                var urlBuilder = new StringBuilder();
                urlBuilder.Append(_clientSettings.TrialServiceLoginUrl);
                urlBuilder.Append($"?userName={Uri.EscapeDataString(userName)}");
                urlBuilder.Append($"&password={Uri.EscapeDataString(password)}");
                urlBuilder.Append($"&hash={Uri.EscapeDataString(hash)}");
                urlBuilder.Append($"&timeStamp={Uri.EscapeDataString(dateTimeStamp)}");

                var request = WebRequest.Create(urlBuilder.ToString());
                request.Credentials = CredentialCache.DefaultCredentials;
                var responseFromServer = "";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var dataStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(dataStream))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                }

                rittenhouseCustomer = JsonConvert.DeserializeObject<RittenhouseCustomer>(responseFromServer);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                rittenhouseCustomer.Message = "Error Validating your account";
            }

            return rittenhouseCustomer;
        }

        public RittenhouseCustomer ValidateRittenhouseAccount(string accountNumber)
        {
            var rittenhouseCustomer = new RittenhouseCustomer();
            try
            {
                var dateTimeStamp = $"{DateTime.Now:yyyyMMddHHmmss}";
                var hash = GetHash(accountNumber, dateTimeStamp);


                var urlBuilder = new StringBuilder();
                urlBuilder.Append(_clientSettings.TrialServiceAccountNumberUrl);
                urlBuilder.Append($"?accountNumber={Uri.EscapeDataString(accountNumber)}");
                urlBuilder.Append($"&hash={Uri.EscapeDataString(hash)}");
                urlBuilder.Append($"&timeStamp={Uri.EscapeDataString(dateTimeStamp)}");

                var request = WebRequest.Create(urlBuilder.ToString());
                request.Credentials = CredentialCache.DefaultCredentials;
                var responseFromServer = "";
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    using (var dataStream = response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(dataStream))
                        {
                            responseFromServer = reader.ReadToEnd();
                        }
                    }
                }

                rittenhouseCustomer = JsonConvert.DeserializeObject<RittenhouseCustomer>(responseFromServer);
            }
            catch (Exception ex)
            {
                _log.Error(ex.Message, ex);
                rittenhouseCustomer.Message = "Error Validating yoru account";
            }

            return rittenhouseCustomer;
        }

        private string GetHash(string userName, string dateTimeStamp)
        {
            var textToHash = $"[{userName}]~R3d~'{dateTimeStamp}'";
            _log.Debug($"textToHash: {textToHash}");
            var calculatedHash = CalculateHash(textToHash);
            _log.Debug($"calculatedHash: {calculatedHash}");

            return calculatedHash;
        }

        private string CalculateHash(string text)
        {
            var bytes = Encoding.Unicode.GetBytes(text);

            using (var hashAlgorithm = HashAlgorithm.Create("SHA1"))
            {
                if (hashAlgorithm != null)
                {
                    var inArray = hashAlgorithm.ComputeHash(bytes);
                    return Convert.ToBase64String(inArray);
                }
            }

            return null;
        }
    }

    public class RittenhouseCustomer
    {
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
        public string EmailAddress { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string IsR2Customer { get; set; }
        public string Territory { get; set; }
        public string R2Type { get; set; }
        public string Message { get; set; }
    }
}