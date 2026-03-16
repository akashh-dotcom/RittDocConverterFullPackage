#region

using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Event.Default;
using NHibernate.Persister.Entity;
using R2V2.Core.SuperType;

#endregion

namespace R2V2.Infrastructure.DataAccess
{
    public class DeleteEventListenerDirector : DefaultDeleteEventListener
    {
        protected override void DeleteEntity(
            IEventSource session,
            object entity,
            EntityEntry entityEntry,
            bool isCascadeDeleteEnabled,
            IEntityPersister persister,
            ISet<object> transientEntities)
        {
            var deletable = entity as ISoftDeletable;
            if (deletable != null)
            {
                deletable.RecordStatus = false;

                CascadeBeforeDelete(session, persister, deletable, entityEntry, transientEntities);
                CascadeAfterDelete(session, persister, deletable, transientEntities);
            }
            else
            {
                base.DeleteEntity(session, entity, entityEntry, isCascadeDeleteEnabled, persister, transientEntities);
            }
        }
    }
}