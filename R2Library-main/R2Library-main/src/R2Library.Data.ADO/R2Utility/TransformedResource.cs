#region

using System;
using System.Text;

#endregion

namespace R2Library.Data.ADO.R2Utility
{
    public class TransformedResource
    {
        public virtual int Id { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual string Isbn { get; set; }
        public virtual DateTime DateStarted { get; set; }
        public virtual DateTime? DateCompleted { get; set; }
        public virtual bool Successfully { get; set; }
        public virtual string Results { get; set; }

        public string ToDebugString()
        {
            var sb = new StringBuilder("TransformedResource = [")
                .AppendFormat("Id = {0}", Id)
                .AppendFormat(", ResourceId = {0}", ResourceId)
                .AppendFormat(", Isbn = {0}", Isbn)
                .AppendFormat(", DateStarted = {0:u}", DateStarted)
                .AppendFormat(", DateCompleted = {0:u}", DateCompleted)
                .AppendFormat(", Successfully = {0}", Successfully)
                .AppendFormat(", Results = {0}", Results)
                .Append("]");
            return sb.ToString();
        }
    }
}