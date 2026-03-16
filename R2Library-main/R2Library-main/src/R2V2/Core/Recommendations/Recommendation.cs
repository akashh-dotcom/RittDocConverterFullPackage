#region

using System;
using System.Text;
using R2V2.Core.Authentication;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Recommendations
{
    public interface IRecommendation : IDebugInfo
    {
        int Id { get; set; }
        int InstitutionId { get; set; }
        int ResourceId { get; set; }
        int RecommendedByUserId { get; set; }
        DateTime? AlertSentDate { get; set; }
        int? AddedToCartByUserId { get; set; }
        DateTime? AddedToCartDate { get; set; }
        int? PurchasedByUserId { get; set; }
        DateTime? PurchaseDate { get; set; }
        int? DeletedByUserId { get; set; }
        DateTime? DeletedDate { get; set; }
        string Notes { get; set; }
        bool RecordStatus { get; set; }

        IUser RecommendedByUser { get; set; }
        IUser AddedToCartByUser { get; set; }
        IUser PurchasedByUser { get; set; }
        IUser DeletedByUser { get; set; }
    }

    public class Recommendation : AuditableEntity, IRecommendation
    {
        public virtual string DeletedNotes { get; set; }
        public virtual int InstitutionId { get; set; }
        public virtual int ResourceId { get; set; }
        public virtual int RecommendedByUserId { get; set; }
        public virtual DateTime? AlertSentDate { get; set; }
        public virtual int? AddedToCartByUserId { get; set; }
        public virtual DateTime? AddedToCartDate { get; set; }
        public virtual int? PurchasedByUserId { get; set; }
        public virtual DateTime? PurchaseDate { get; set; }
        public virtual int? DeletedByUserId { get; set; }
        public virtual DateTime? DeletedDate { get; set; }
        public virtual string Notes { get; set; }
        public virtual bool RecordStatus { get; set; }

        public virtual IUser RecommendedByUser { get; set; }
        public virtual IUser AddedToCartByUser { get; set; }
        public virtual IUser PurchasedByUser { get; set; }
        public virtual IUser DeletedByUser { get; set; }

        public virtual string ToDebugString()
        {
            var sb = new StringBuilder("Recommendation = [");
            sb.AppendFormat("Id: {0}", Id);
            sb.AppendFormat(", InstitutionId: {0}", InstitutionId);
            sb.AppendFormat(", ResourceId: {0}", ResourceId);
            sb.AppendFormat(", RecommendedByUserId: {0}", RecommendedByUserId);
            sb.AppendFormat(", AlertSentDate: {0}", AlertSentDate == null ? "null" : AlertSentDate.Value.ToString("G"));
            sb.AppendFormat(", AddedToCartByUserId: {0}", AddedToCartByUserId);
            sb.AppendFormat(", AddedToCartDate: {0}",
                AddedToCartDate == null ? "null" : AddedToCartDate.Value.ToString("G"));
            sb.AppendFormat(", PurchasedByUserId: {0}", PurchasedByUserId);
            sb.AppendFormat(", PurchaseDate: {0}", PurchaseDate == null ? "null" : PurchaseDate.Value.ToString("G"));
            sb.AppendFormat(", DeletedByUserId: {0}", DeletedByUserId);
            sb.AppendFormat(", DeletedDate: {0}", DeletedDate == null ? "null" : DeletedDate.Value.ToString("G"));
            sb.AppendFormat(", DeletedNotes: {0}", DeletedNotes);

            sb.AppendFormat(", Notes: {0}", Notes);
            sb.Append("]");
            return sb.ToString();
        }
    }
}