#region

using R2V2.Core.SuperType;

#endregion

namespace R2V2.Infrastructure.DataAccess
{
    public class RecordStatusPersistanceDecorator : IPersistInstanceDecorator
    {
        public void Execute<T>(T instance)
        {
            if (!(instance is ISoftDeletable softDeletable))
                return;

            softDeletable.RecordStatus = true;
        }
    }
}