#region

using System;
using System.Reflection;

#endregion

namespace R2V2.Web.Models.Ping
{
    public class PingData // : BaseModel
    {
        public PingData()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            MachineName = Environment.MachineName;
            RequestTimestamp = DateTime.Now;
            Version = version.ToString();
        }

        public string DatabaseStatus { get; set; }
        public string ClientIpAddress { get; set; }
        public string Version { get; private set; }
        public string MachineName { get; set; }
        public DateTime AssemplyTimestamp { get; set; }
        public DateTime RequestTimestamp { get; set; }
        public DateTime AppStartTimestamp { get; set; }
    }
}