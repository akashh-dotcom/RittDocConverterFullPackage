#region

using System.Text;

#endregion

namespace R2V2.Core.Email
{
    public class InstitutionResourceEmailRecipient : IDebugInfo
    {
        public virtual int Id { get; set; }
        public virtual string EmailAddress { get; set; }
        public virtual char AddressType { get; set; }

        public virtual InstitutionResourceEmail InstitutionResourceEmail { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("InstitutionResourceEmail = [");
            sb.Append($"Id: {Id}");
            sb.Append($", EmailAddress: {EmailAddress}");
            sb.Append($", AddressType: {AddressType}");
            sb.Append("]");
            return sb.ToString();
        }
    }
}