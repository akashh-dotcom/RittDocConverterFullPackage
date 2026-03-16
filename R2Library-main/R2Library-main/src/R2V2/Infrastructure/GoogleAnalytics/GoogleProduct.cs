#region

using System;
using System.Text;
using Newtonsoft.Json;

#endregion

namespace R2V2.Infrastructure.GoogleAnalytics
{
    [Serializable]
    public class GoogleProduct
    {
        public string id { get; set; }
        public string name { get; set; }
        public decimal price { get; set; }
        public string category { get; set; }
        public int quantity { get; set; }
        public string list { get; set; }
        public int position { get; set; }
        public string action { get; set; }
        public string partner { get; set; }
        public string publisher { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("GoogleProduct = [");
            sb.AppendFormat(", id: {0}", id);
            sb.AppendFormat(", name: {0}", name);
            sb.AppendFormat(", price: {0}", price);
            sb.AppendFormat(", category: {0}", category);
            sb.AppendFormat(", quantity: {0}", quantity);
            sb.AppendFormat(", list: {0}", list);
            sb.AppendFormat(", position: {0}", position);
            sb.AppendFormat(", action: {0}", action);
            sb.AppendFormat(", partner: {0}", partner);
            sb.AppendFormat(", publisher: {0}", publisher);
            sb.Append("]");
            return sb.ToString();
        }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}