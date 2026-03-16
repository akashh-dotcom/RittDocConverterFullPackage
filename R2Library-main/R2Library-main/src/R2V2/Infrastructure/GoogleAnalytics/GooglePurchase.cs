#region

using System;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace R2V2.Infrastructure.GoogleAnalytics
{
    [Serializable]
    public class GooglePurchase
    {
        public string id { get; set; }
        public decimal revenue { get; set; }
        public decimal tax { get; set; }
        public decimal shipping { get; set; }
        public string coupon { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("GooglePurchase = [");
            sb.AppendFormat(", id: {0}", id);
            sb.AppendFormat(", revenue: {0}", revenue);
            sb.AppendFormat(", tax: {0}", tax);
            sb.AppendFormat(", shipping: {0}", shipping);
            sb.AppendFormat(", coupon: {0}", coupon);

            sb.Append("]");
            return sb.ToString();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}