#region

using System;
using NHibernate;

#endregion

namespace R2V2.Infrastructure.UnitOfWork
{
    public interface IUnitOfWork : ISession
    {
        ISession Session { get; }
        Guid Id { get; }
        int UsageCount { get; }
        void Commit();
        void Delete<T>(object id);
        event Action<IUnitOfWork> Disposed;
        void IncrementUsage();
        void ForceCommit();
        void Execute(IUnitOfWorkCommand command);
    }
}