#region

using System;
using System.Text;

#endregion

namespace R2V2.Core.Institution
{
    public enum InstitutionResourceAuditType
    {
        ResourceAddedToCart = 1,
        ResourceDeletedFromCart = 2,
        CartResourceUpdated = 3,
        PurchasedResource = 4,
        ComplimentaryResourceLicenseGranted = 5,
        ResourceLicenceCountUpdated = 6,
        PdaResourceAdded = 7,
        PdaResourceDeleted = 8,
        PdaResourceView = 9,
        PdaResourceAddedToCart = 10,
        AddArchivedResourceToInstitution = 11,
        ResourceSaleable = 12,
        ResourceNotSaleable = 13,
        PdaResourceDeletedFromCart = 14,
        PdaResourceAddedViaProfile = 15
    }

    public static class InstitutionResourceAuditTypeExtension
    {
        public static string ToDescription(this InstitutionResourceAuditType type)
        {
            switch (type)
            {
                case InstitutionResourceAuditType.ResourceAddedToCart:
                    return "Resource Added to Cart";
                case InstitutionResourceAuditType.ResourceDeletedFromCart:
                    return "Resource Deleted from Cart";
                case InstitutionResourceAuditType.CartResourceUpdated:
                    return "Cart Resource Updated";
                case InstitutionResourceAuditType.PurchasedResource:
                    return "Resource Purchased";
                case InstitutionResourceAuditType.ComplimentaryResourceLicenseGranted:
                    return "Complimentary Resource License Granted";
                case InstitutionResourceAuditType.ResourceLicenceCountUpdated:
                    return "Resource Licence Count Updated";
                case InstitutionResourceAuditType.PdaResourceAdded:
                    return "PDA Resource Added";
                case InstitutionResourceAuditType.PdaResourceDeleted:
                    return "PDA Resource Deleted";
                case InstitutionResourceAuditType.PdaResourceView:
                    return "PDA Resource View";
                case InstitutionResourceAuditType.PdaResourceAddedToCart:
                    return "PDA Resource Added To Cart";
                case InstitutionResourceAuditType.ResourceSaleable:
                    return "Resource has become Saleable";
                case InstitutionResourceAuditType.ResourceNotSaleable:
                    return "Resource has become NOT Saleable";
                case InstitutionResourceAuditType.PdaResourceAddedViaProfile:
                    return "Added resource to PDA via Profile";
                case InstitutionResourceAuditType.PdaResourceDeletedFromCart:
                    return "PDA resource deleted from cart";
                default:
                    return "n/a";
            }
        }
    }

    public class InstitutionResourceAudit : IDebugInfo
    {
        public virtual int Id { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int? UserId { get; set; }
        public virtual short AuditTypeId { get; set; }
        public virtual int LicenseCount { get; set; }
        public virtual decimal SingleLicensePrice { get; set; }
        public virtual string PoNumber { get; set; }
        public virtual string CreatorId { get; set; }
        public virtual DateTime CreationDate { get; set; }
        public virtual string EventDescription { get; set; }
        public virtual bool Legacy { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("InstitutionResourceAudit = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", UserId: {0}", UserId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", AuditTypeId: {0}", AuditTypeId);
            sb.AppendFormat(", LicenseCount: {0}", LicenseCount);
            sb.AppendFormat(", SingleLicensePrice: {0}", SingleLicensePrice);
            sb.AppendFormat(", PoNumber: {0}", PoNumber);
            sb.AppendFormat(", CreatorId: {0}", CreatorId);
            sb.AppendFormat(", CreationDate: {0}", CreationDate);
            sb.AppendFormat(", EventDescription: {0}", EventDescription);
            sb.AppendFormat(", Legacy: {0}", Legacy);
            sb.Append("]");
            return sb.ToString();
        }
    }
}