#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace R2V2.Web.Infrastructure.Authentication
{
    [Serializable]
    public class PassiveAuthenticationStatus
    {
        private readonly SortedList<DateTime, AuthReferrer> _authReferrers = new SortedList<DateTime, AuthReferrer>();
        private readonly SortedList<DateTime, IpAddress> _ipAddresses = new SortedList<DateTime, IpAddress>();
        private readonly SortedList<DateTime, TrustedHash> _trustedHashes = new SortedList<DateTime, TrustedHash>();

        public IEnumerable<IpAddress> IpAddresses => _ipAddresses.Values;
        public IEnumerable<AuthReferrer> AuthReferrers => _authReferrers.Values;
        public IEnumerable<TrustedHash> TrustedHashes => _trustedHashes.Values;
        public AthensIdentifier AthensIdentifier { get; private set; }

        public void AddIpAddress(string ip, bool successful)
        {
            var ipAddress = new IpAddress
            {
                Value = ip,
                Timestamp = DateTime.Now,
                Successful = successful
            };

            _ipAddresses.Add(ipAddress.Timestamp, ipAddress);
            if (_ipAddresses.Count > 5)
            {
                _ipAddresses.RemoveAt(0);
            }
        }

        public void AddAuthReferrer(string referrer, bool successful)
        {
            var authReferrer = new AuthReferrer
            {
                Value = referrer,
                Timestamp = DateTime.Now,
                Successful = successful
            };

            _authReferrers.Add(authReferrer.Timestamp, authReferrer);
            if (_authReferrers.Count > 5)
            {
                _ipAddresses.RemoveAt(0);
            }
        }

        public void AddTrustedHash(string hash, bool successful)
        {
            var trustedHash = new TrustedHash
            {
                Value = hash,
                Timestamp = DateTime.Now,
                Successful = successful
            };

            _trustedHashes.Add(trustedHash.Timestamp, trustedHash);
            if (_trustedHashes.Count > 5)
            {
                _trustedHashes.RemoveAt(0);
            }
        }

        public void SetAthensIdentifier(string scopedAffiliation, string targetedId, bool successful)
        {
            var combinedIdentifier = $"{scopedAffiliation}_^~_{targetedId}";
            AthensIdentifier = new AthensIdentifier
            {
                Value = combinedIdentifier,
                Timestamp = DateTime.Now,
                Successful = successful
            };
        }

        public void ClearAthensIdentifier()
        {
            AthensIdentifier = null;
        }

        public bool WasIpAddressAuthPreviouslyAttempted(string ip)
        {
            return _ipAddresses.Values.Any(x => x.Value == ip);
        }

        public bool WasReferrerAuthPreviouslyAttempted(string referrer)
        {
            return _authReferrers.Values.Any(x => x.Value == referrer);
        }

        public bool WasTrustedAuthPreviouslyAttempted(string hash)
        {
            return _authReferrers.Values.Any(x => x.Value == hash);
        }

        public bool WasAthensAuthPreviouslyAttempted(string scopedAffiliation, string targetedId)
        {
            if (AthensIdentifier == null)
            {
                return false;
            }

            var combinedIdentifier = $"{scopedAffiliation}_^~_{targetedId}";
            return AthensIdentifier.Value == combinedIdentifier;
        }
    }

    /// <summary>
    /// </summary>
    public interface IPassiveAuthenticationStatusItem
    {
        string Value { get; set; }
        DateTime Timestamp { get; set; }
        bool Successful { get; set; }
    }

    /// <summary>
    /// </summary>
    [Serializable]
    public abstract class PassiveAuthenticationStatusItem : IPassiveAuthenticationStatusItem
    {
        public string Value { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Successful { get; set; }
    }

    /// <summary>
    /// </summary>
    [Serializable]
    public class IpAddress : PassiveAuthenticationStatusItem
    {
    }

    /// <summary>
    /// </summary>
    [Serializable]
    public class AuthReferrer : PassiveAuthenticationStatusItem
    {
    }

    /// <summary>
    /// </summary>
    [Serializable]
    public class TrustedHash : PassiveAuthenticationStatusItem
    {
    }

    /// <summary>
    /// </summary>
    [Serializable]
    public class AthensIdentifier : PassiveAuthenticationStatusItem
    {
    }
}