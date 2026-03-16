#region

using System.Linq;
using FluentNHibernate.Mapping;
using R2V2.Core.SuperType;
using R2V2.DataAccess.NHibernateMaps;
using R2V2.Extensions;

#endregion

namespace R2V2.DataAccess
{
    public class BaseMap<T> : ClassMap<T>
    {
        public BaseMap()
        {
            var interfaces = typeof(T).GetInterfaces();
            if (interfaces.IsEmpty())
                return;

            if (interfaces.Any(i => i == typeof(ISoftDeletable)))
            {
                Map(x => (x as ISoftDeletable).RecordStatus, "tiRecordStatus");
                ApplyFilter<SoftDeleteFilter>();
            }

            if (interfaces.All(i => i != typeof(IAuditable)))
                return;

            Map(x => (x as IAuditable).CreatedBy, "vchCreatorId");
            Map(x => (x as IAuditable).CreationDate, "dtCreationDate");
            Map(x => (x as IAuditable).UpdatedBy, "vchUpdaterId");
            Map(x => (x as IAuditable).LastUpdated, "dtLastUpdate");
        }
    }
}