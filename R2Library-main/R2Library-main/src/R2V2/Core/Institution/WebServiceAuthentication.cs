#region

using System;
using System.Text;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Institution
{
    [Serializable]
    public class WebServiceAuthentication : AuditableEntity, ISoftDeletable, IDebugInfo
    {
        public virtual Institution Institution { get; set; }
        public virtual int InstitutionId { get; set; }

        public virtual int OctetA { get; set; }
        public virtual int OctetB { get; set; }
        public virtual int OctetC { get; set; }
        public virtual int OctetD { get; set; }
        public virtual long IpNumber { get; set; }

        public virtual string AuthenticationKey { get; set; }

        public virtual string ToDebugString()
        {
            return new StringBuilder("IpAddressRange = [ ")
                .AppendFormat("Id: {0}", Id)
                .AppendFormat(", [{0}.{1}.{2}.{3}]", OctetA, OctetB, OctetC, OctetD)
                .AppendFormat(", [{0}]", IpNumber)
                .AppendFormat(", Instituion.Id: {0}", Institution.Id)
                .AppendFormat(", Instituion.AccountNumber: {0}", Institution.AccountNumber)
                .AppendFormat(", Instituion.Name: {0}", Institution.Name)
                .AppendFormat(", Instituion.AccountStatusId: {0}", Institution.AccountStatusId)
                .AppendFormat(", Instituion.StartDate: {0}",
                    Institution.Trial.StartDate == null ? "" : $"{Institution.Trial.StartDate.Value:d}")
                .AppendFormat(", Instituion.EndDate: {0}",
                    Institution.Trial.EndDate == null ? "" : $"{Institution.Trial.EndDate.Value:d}")
                .Append("]")
                .ToString();
        }

        public virtual bool RecordStatus { get; set; }

        public virtual void PopulateDecimal()
        {
            IpNumber = CalculateIpNumber(OctetA, OctetB, OctetC, OctetD);
        }

        public static long CalculateIpNumber(int octetA, int octetB, int octetC, int octetD)
        {
            var ipNumberA = 16777216L * octetA;
            var ipNumberB = 65536L * octetB;
            var ipNumberC = 256L * octetC;
            var ipNumber = ipNumberA + ipNumberB + ipNumberC + octetD;
            return ipNumber;
        }
    }
}