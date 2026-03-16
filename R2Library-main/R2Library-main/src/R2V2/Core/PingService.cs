#region

using System.Linq;

#endregion

namespace R2V2.Core
{
    public class PingService
    {
        private readonly IQueryable<Ping> _pings;

        public PingService(IQueryable<Ping> pings)
        {
            _pings = pings;
        }

        public string GetPingValue()
        {
            var ping = _pings.Single(x => x.Id == 1);
            return ping.StatusCode;
        }
    }
}