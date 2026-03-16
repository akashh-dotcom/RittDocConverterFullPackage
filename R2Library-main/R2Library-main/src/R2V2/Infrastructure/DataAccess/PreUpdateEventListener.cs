#region

using System;
using System.Threading;
using System.Threading.Tasks;

using R2V2.Infrastructure.DependencyInjection;
using NHibernate;
using NHibernate.Event;
using NHibernate.Persister.Entity;
using R2V2.Core.SuperType;
using R2V2.Extensions;

#endregion

namespace R2V2.Infrastructure.DataAccess
{
    public class PreUpdateEventListener : IPreUpdateEventListener
    {
        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            if (NHibernateUtil.IsInitialized(@event.Entity))
            {
                ServiceLocator.Current.GetInstance<AuditablePersistenceDecorator>().Execute(@event.Entity);

                if (@event.Entity is IAuditable)
                {
                    var auditable = @event.Entity.As<IAuditable>();
                    Set(@event.Persister, @event.State, "CreationDate", auditable.CreationDate);
                    Set(@event.Persister, @event.State, "LastUpdated", auditable.LastUpdated);
                    Set(@event.Persister, @event.State, "CreatedBy", auditable.CreatedBy);
                    Set(@event.Persister, @event.State, "UpdatedBy", auditable.UpdatedBy);
                }
            }

            return false;
        }

        private void Set(IEntityPersister persister, object[] state, string propertyName, object value)
        {
            var index = Array.IndexOf(persister.PropertyNames, propertyName);
            if (index == -1)
            {
                return;
            }

            state[index] = value;
        }

        public Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}