#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class SpecialResource : AuditableEntity, ISoftDeletable, IDebugInfo
    {
//SELECT [iSpecialResourceId]
//      ,[iSpecialDiscountId]
//      ,[iResourceId]
//      ,[vchCreatorId]
//      ,[dtCreationDate]
//      ,[vchUpdaterId]
//      ,[dtLastUpdate]
//      ,[tiRecordStatus]
//  FROM [dbo].[tSpecialResource]

        public virtual int ResourceId { get; set; }
        public virtual SpecialDiscount Discount { get; set; }
        public virtual int DiscountId { get; set; }

        public virtual string ToDebugString()
        {
            throw new NotImplementedException();
        }

        public virtual bool RecordStatus { get; set; }
    }
}