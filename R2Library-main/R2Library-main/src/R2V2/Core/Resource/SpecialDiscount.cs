#region

using System;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class SpecialDiscount : AuditableEntity, ISoftDeletable, IDebugInfo
    {
//SELECT [iSpecialDiscountId]
//      ,[iDiscountPercentage]
//      ,[iSpecialId]
//      ,[vchIconName]
//      ,[vchCreatorId]
//      ,[dtCreationDate]
//      ,[vchUpdaterId]
//      ,[dtLastUpdate]
//      ,[tiRecordStatus]
//  FROM [dbo].[tSpecialDiscount]
        public virtual int DiscountPercentage { get; set; }
        public virtual int SpecialId { get; set; }
        public virtual string IconName { get; set; }

        public virtual Special Special { get; set; }

        public virtual string ToDebugString()
        {
            throw new NotImplementedException();
        }

        public virtual bool RecordStatus { get; set; }
    }
}