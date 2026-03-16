#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using R2V2.Core.Institution;
using R2V2.Infrastructure.Logging;

#endregion

namespace R2V2.Infrastructure.Authentication
{
    public class TrustedAuthenticationService
    {
        private readonly IQueryable<Institution> _institutions;
        private readonly ILog<TrustedAuthenticationService> _log;

        public TrustedAuthenticationService(IQueryable<Institution> institutions,
            ILog<TrustedAuthenticationService> log)
        {
            _institutions = institutions;
            _log = log;
        }

        public int AttemptTrustedAuthentication(string accountNumber, string hashKey, string timeStamp)
        {
            var institution = _institutions.FirstOrDefault(x => x.AccountNumber == accountNumber);
            if (institution == null)
            {
                return -1;
            }

            if (string.IsNullOrWhiteSpace(institution.TrustedKey))
            {
                _log.ErrorFormat(
                    "Institution: {0}  -- does not have there institution.TrustedKey Set. Please set this before attempting Trusted Authenication. ",
                    institution.AccountNumber);
                return -1;
            }

            var timeStart = DateTime.UtcNow.AddMinutes(30);
            var timeEnd = DateTime.UtcNow.AddMinutes(-30);

            var year = timeStamp.Substring(0, 4);
            var month = timeStamp.Substring(4, 2);
            var day = timeStamp.Substring(6, 2);
            var hour = timeStamp.Substring(8, 2);
            var minute = timeStamp.Substring(10, 2);
            var second = timeStamp.Substring(12, 2);

            var formatedDate = $"{month}/{day}/{year} {hour}:{minute}:{second}";

            var userTimeStamp = DateTime.Parse(formatedDate);

            if (userTimeStamp <= timeStart && userTimeStamp >= timeEnd &&
                !string.IsNullOrWhiteSpace(institution.TrustedKey))
            {
                var generatedHashKey = GetHashKey(institution.AccountNumber, institution.TrustedKey, userTimeStamp);

                //For some reason the generatedHashKey contains + where spaces should be and this is the only way i could fix the it.
                //I tried URL encoding and decoding but no luck. 
                //This matches the new key generator that was added to project.
                if (generatedHashKey == hashKey.Replace(" ", "+"))
                {
                    return institution.Id;
                }
            }

            return -1;
        }

        private string GetHashKey(string accountNumber, string info, DateTime timestamp)
        {
            var sb = new StringBuilder();

            sb.Append(accountNumber).Append("|").Append(timestamp);

            var encoding = new ASCIIEncoding();

            var saltBytes = encoding.GetBytes(info);

            var hashedString = ComputeSha1Hash(sb.ToString(), saltBytes);

            return hashedString;
        }

        private string ComputeSha1Hash(string plaintext, IList<byte> saltBytes)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plaintext);
            var plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Count];

            for (var i = 0; i < plainTextBytes.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainTextBytes[i];
            }

            for (var i = 0; i < saltBytes.Count; i++)
            {
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];
            }

            HashAlgorithm hash = new SHA1Managed();

            var hashBytes = hash.ComputeHash(plainTextWithSaltBytes);

            var hashWithSaltBytes = new byte[hashBytes.Length + saltBytes.Count];

            for (var i = 0; i < hashBytes.Length; i++)
            {
                hashWithSaltBytes[i] = hashBytes[i];
            }

            for (var i = 0; i < saltBytes.Count; i++)
            {
                hashWithSaltBytes[hashBytes.Length + i] = saltBytes[i];
            }

            var hashValue = Convert.ToBase64String(hashWithSaltBytes);

            return hashValue;
        }


        public string GetInstitutionTrustedAuthUrlParameters(WebServiceAuthentication webServiceAuthentication)
        {
            var institution = _institutions.FirstOrDefault(x => x.Id == webServiceAuthentication.InstitutionId);
            if (institution == null)
            {
                return null;
            }

            var currentTime = DateTime.UtcNow;
            var formatedTimeStamp =
                $"{currentTime.Year}{currentTime.Month:00}{currentTime.Day:00}{currentTime.Hour:00}{currentTime.Minute:00}{currentTime.Second:00}";

            var generatedHashKey = GetHashKey(institution.AccountNumber, institution.TrustedKey, currentTime);

            return $"timestamp={formatedTimeStamp}&hash={generatedHashKey}";
        }

        public WebTrustedAuthentication GetWebTrustedAuthentication(WebServiceAuthentication webServiceAuthentication,
            string authenticationKey)
        {
            var institution = _institutions.FirstOrDefault(x => x.Id == webServiceAuthentication.InstitutionId);
            if (institution == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(institution.TrustedKey))
            {
                return new WebTrustedAuthentication
                {
                    ErrorMessage =
                        "Institution Level Trusted Security Key is not setup. Please Contact Rittenhouse to address this issue."
                };
            }

            var currentTime = DateTime.UtcNow;
            var formatedTimeStamp =
                $"{currentTime.Year}{currentTime.Month:00}{currentTime.Day:00}{currentTime.Hour:00}{currentTime.Minute:00}{currentTime.Second:00}";

            var generatedHashKey = GetHashKey(institution.AccountNumber, institution.TrustedKey, currentTime);

            if (string.IsNullOrWhiteSpace(authenticationKey))
            {
                return new WebTrustedAuthentication { ErrorMessage = "AuthenticationKey must be provided." };
            }

            if (webServiceAuthentication.AuthenticationKey == authenticationKey)
            {
                if (string.IsNullOrWhiteSpace(webServiceAuthentication.Institution.TrustedKey))
                {
                    return new WebTrustedAuthentication
                    {
                        ErrorMessage =
                            "The institutions trusted authentication key has not been set. Please use the contact us link on R2library.com to resolve this issue"
                    };
                }

                return new WebTrustedAuthentication { Hash = generatedHashKey, Timestamp = formatedTimeStamp };
            }

            return authenticationKey.Length != 24
                ? new WebTrustedAuthentication { ErrorMessage = "AuthenticationKey is not the correct length" }
                : new WebTrustedAuthentication
                {
                    ErrorMessage =
                        "The IP address and the authentication key provided cannot be found. Please use the contact us link on R2library.com to resolve this issue."
                };
        }
    }
}