#region

using System;
using System.Collections.Generic;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Core.Resource
{
    public class Special : AuditableEntity, ISoftDeletable, IDebugInfo
    {
//SELECT [iSpecialId]
//      ,[vchName]
//      ,[dtStartDate]
//      ,[dtEndDate]
//      ,[vchCreatorId]
//      ,[dtCreationDate]
//      ,[vchUpdaterId]
//      ,[dtLastUpdate]
//      ,[tiRecordStatus]
//  FROM [dbo].[tSpecial]

        public virtual string Name { get; set; }
        public virtual DateTime StartDate { get; set; }
        public virtual DateTime EndDate { get; set; }

        public virtual IList<SpecialDiscount> Discounts { get; set; }

        public virtual string ToDebugString()
        {
            throw new NotImplementedException();
        }

        public virtual bool RecordStatus { get; set; }
    }
}