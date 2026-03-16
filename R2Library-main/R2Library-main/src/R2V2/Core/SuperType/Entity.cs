#region

using System;

#endregion

namespace R2V2.Core.SuperType
{
    [Serializable]
    public abstract class Entity<T> : IEntity<T>
    {
        public virtual T Id { get; set; }
    }

    [Serializable]
    public abstract class Entity : Entity<int>, IEntity
    {
    }
}