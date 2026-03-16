#region

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using NHibernate.Type;
using R2V2.Extensions;
using R2V2.Infrastructure.DependencyInjection;
using R2V2.Infrastructure.Logging;

#endregion


namespace R2V2.Infrastructure.UnitOfWork
{
    public interface IUnitOfWorkCommand
    {
        void Execute(IUnitOfWork uow);
    }

    [DoNotRegisterWithContainer]
    public class UnitOfWork : IUnitOfWork
    {
        //private readonly ILog<UnitOfWork> _log;
        private bool _hasBeenDisposed;

        //private static readonly ILog<UnitOfWork> Log = new Log<UnitOfWork>();

        public UnitOfWork(ISession session)
        {
            Session = session;
            Session.FlushMode = FlushMode.Never;
            Id = Guid.NewGuid();
            UsageCount = 0;
            //_log = ServiceLocator.Current.GetInstance<ILog<UnitOfWork>>();
        }

        /// <summary>
        ///     Get the current Unit of Work and return the associated <c>ITransaction</c> object.
        /// </summary>
        public ITransaction Transaction => Session.Transaction;

        public Guid Id { get; }

        public void IncrementUsage()
        {
            //Not threadsafe....
            UsageCount++;
        }

        public ISession Session { get; }

        public void Commit()
        {
            ForceCommit();
        }

        public void ForceCommit()
        {
            Session.Flush();
        }

        public void Execute(IUnitOfWorkCommand command)
        {
            command.Execute(this);
        }

        public int UsageCount { get; private set; }

        public void Delete<T>(object obj)
        {
            var entityType = typeof(T);
            Session.Delete("{0}.{1}".Args(entityType.Namespace, entityType.Name), obj);
        }

        public void Flush()
        {
            Session.Flush();
        }

        public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.FlushAsync(cancellationToken);
        }

        public IDbConnection Disconnect()
        {
            return Session.Disconnect();
        }

        public void Reconnect()
        {
            Session.Reconnect();
        }

        public void Reconnect(IDbConnection connection)
        {
            // Fix: Convert IDbConnection to DbConnection before passing to Session.Reconnect
            if (connection is DbConnection dbConnection)
            {
                Session.Reconnect(dbConnection);
            }
            else
            {
                throw new ArgumentException("connection must be of type DbConnection", nameof(connection));
            }
        }

        public IDbConnection Close()
        {
            return Session.Close();
        }

        public void CancelQuery()
        {
            Session.CancelQuery();
        }

        public bool IsDirty()
        {
            return Session.IsDirty();
        }

        public bool IsReadOnly(object entityOrProxy)
        {
            return Session.IsReadOnly(entityOrProxy);
        }

        public void SetReadOnly(object entityOrProxy, bool readOnly)
        {
            Session.SetReadOnly(entityOrProxy, readOnly);
        }

        public object GetIdentifier(object obj)
        {
            return Session.GetIdentifier(obj);
        }

        public bool Contains(object obj)
        {
            return Session.Contains(obj);
        }

        public void Evict(object obj)
        {
            if (Session.Contains(obj))
            {
                Session.Evict(obj);
            }
        }

        public object Load(Type theType, object id, LockMode lockMode)
        {
            return Session.Load(theType, id, lockMode);
        }

        public object Load(string entityName, object id, LockMode lockMode)
        {
            return Session.Load(entityName, id, lockMode);
        }

        public object Load(Type theType, object id)
        {
            return Session.Load(theType, id);
        }

        public T Load<T>(object id, LockMode lockMode)
        {
            return Session.Load<T>(id, lockMode);
        }

        public T Load<T>(object id)
        {
            return Session.Load<T>(id);
        }

        public object Load(string entityName, object id)
        {
            return Session.Load(entityName, id);
        }

        public Task<object> LoadAsync(Type theType, object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync(theType, id, lockMode, cancellationToken);
        }

        public Task<object> LoadAsync(string entityName, object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync(entityName, id, lockMode, cancellationToken);
        }

        public Task<object> LoadAsync(Type theType, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync(theType, id, cancellationToken);
        }

        public Task<T> LoadAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync<T>(id, lockMode, cancellationToken);
        }

        public Task<T> LoadAsync<T>(object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync<T>(id, cancellationToken);
        }

        public Task<object> LoadAsync(string entityName, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync(entityName, id, cancellationToken);
        }

        public Task LoadAsync(object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LoadAsync(obj, id, cancellationToken);
        }

        public void Replicate(object obj, ReplicationMode replicationMode)
        {
            Session.Replicate(obj, replicationMode);
        }

        public void Replicate(string entityName, object obj, ReplicationMode replicationMode)
        {
            Session.Replicate(entityName, obj, replicationMode);
        }

        public Task ReplicateAsync(object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.ReplicateAsync(obj, replicationMode, cancellationToken);
        }

        public Task ReplicateAsync(string entityName, object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.ReplicateAsync(entityName, obj, replicationMode, cancellationToken);
        }

        public object Save(object obj)
        {
            //_log.DebugFormat("Save(obj: {0})", obj);
            return Session.Save(obj);
        }

        public void Save(object obj, object id)
        {
            //_log.DebugFormat("Save(obj: {0}, id: {1})", obj, id);
            Session.Save(obj, id);
        }

        public object Save(string entityName, object obj)
        {
            //_log.DebugFormat("Save(entityName: {0}, obj: {1})", entityName, obj);
            return Session.Save(entityName, obj);
        }

        public object Save(string entityName, object obj, object id)
        {
            //_log.DebugFormat("Save(entityName: {0}, obj: {1}, id: {2})", entityName, obj, id);
            Session.Save(entityName, obj, id);
            return id;
        }

        public Task<object> SaveAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.SaveAsync(obj, cancellationToken);
        }

        public Task SaveAsync(object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.SaveAsync(obj, id, cancellationToken);
        }

        public Task<object> SaveAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.SaveAsync(entityName, obj, cancellationToken);
        }

        public Task SaveAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.SaveAsync(entityName, obj, id, cancellationToken);
        }

        public void SaveOrUpdate(object obj)
        {
            //_log.DebugFormat("SaveOrUpdate(obj: {0})", obj);
            Session.SaveOrUpdate(obj);
        }

        public void SaveOrUpdate(string entityName, object obj)
        {
            //_log.DebugFormat("SaveOrUpdate(entityName: {0}, obj: {1})", entityName, obj);
            Session.SaveOrUpdate(entityName, obj);
        }

        public void SaveOrUpdate(string entityName, object obj, object id)
        {
            //_log.DebugFormat("SaveOrUpdate(entityName: {0}, obj: {1}, id: {2})", entityName, obj, id);
            Session.SaveOrUpdate(entityName, obj, id);
        }

        public Task SaveOrUpdateAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.SaveOrUpdateAsync(obj, cancellationToken);
        }

        public Task SaveOrUpdateAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.SaveOrUpdateAsync(entityName, obj, cancellationToken);
        }

        public Task SaveOrUpdateAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.SaveOrUpdateAsync(entityName, obj, id, cancellationToken);
        }

        public void Update(object obj)
        {
            //_log.DebugFormat("Update(obj: {0})", obj);
            Session.Update(obj);
        }

        public void Update(object obj, object id)
        {
            //_log.DebugFormat("Update(obj: {0}, id: {1})", obj, id);
            Session.Update(obj, id);
        }

        public void Update(string entityName, object obj)
        {
            //_log.DebugFormat("Update(entityName: {0}, obj: {1})", entityName, obj);
            Session.Update(entityName, obj);
        }

        public void Update(string entityName, object obj, object id)
        {
            //_log.DebugFormat("Update(entityName: {0}, obj: {1}, id: {2})", entityName, obj, id);
            Session.Update(entityName, obj, id);
        }

        public Task UpdateAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.UpdateAsync(obj, cancellationToken);
        }

        public Task UpdateAsync(object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.UpdateAsync(obj, id, cancellationToken);
        }

        public Task UpdateAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.UpdateAsync(entityName, obj, cancellationToken);
        }

        public Task UpdateAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.UpdateAsync(entityName, obj, id, cancellationToken);
        }

        public object Merge(object obj)
        {
            return Session.Merge(obj);
        }

        public object Merge(string entityName, object obj)
        {
            return Session.Merge(entityName, obj);
        }

        public T Merge<T>(T entity) where T : class
        {
            return Session.Merge(entity);
        }

        public T Merge<T>(string entityName, T entity) where T : class
        {
            return Session.Merge(entityName, entity);
        }

        public Task<object> MergeAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.MergeAsync(obj, cancellationToken);
        }

        public Task<object> MergeAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.MergeAsync(entityName, obj, cancellationToken);
        }

        public Task<T> MergeAsync<T>(T entity, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Session.MergeAsync(entity, cancellationToken);
        }

        public Task<T> MergeAsync<T>(string entityName, T entity, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Session.MergeAsync(entityName, entity, cancellationToken);
        }

        public void Persist(object obj)
        {
            Session.Persist(obj);
        }

        public void Persist(string entityName, object obj)
        {
            Session.Persist(entityName, obj);
        }

        public Task PersistAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.PersistAsync(obj, cancellationToken);
        }

        public Task PersistAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.PersistAsync(entityName, obj, cancellationToken);
        }

        public void Delete(object obj)
        {
            Session.Delete(obj);
        }

        public void Delete(string entityName, object obj)
        {
            Session.Delete(entityName, obj);
        }

        public int Delete(string query)
        {
            return Session.Delete(query);
        }

        public int Delete(string query, object value, IType type)
        {
            return Session.Delete(query, value, type);
        }

        public int Delete(string query, object[] values, IType[] types)
        {
            return Session.Delete(query, values, types);
        }

        public Task DeleteAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.DeleteAsync(obj, cancellationToken);
        }

        public Task DeleteAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.DeleteAsync(entityName, obj, cancellationToken);
        }

        public void Lock(object obj, LockMode lockMode)
        {
            Session.Lock(obj, lockMode);
        }

        public void Lock(string entityName, object obj, LockMode lockMode)
        {
            Session.Lock(entityName, obj, lockMode);
        }

        public Task LockAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LockAsync(obj, lockMode, cancellationToken);
        }

        public Task LockAsync(string entityName, object obj, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.LockAsync(entityName, obj, lockMode, cancellationToken);
        }

        public void Refresh(object obj)
        {
            Session.Refresh(obj);
        }

        public void Refresh(object obj, LockMode lockMode)
        {
            Session.Refresh(obj, lockMode);
        }

        public Task RefreshAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.RefreshAsync(obj, cancellationToken);
        }

        public Task RefreshAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.RefreshAsync(obj, lockMode, cancellationToken);
        }

        public LockMode GetCurrentLockMode(object obj)
        {
            return Session.GetCurrentLockMode(obj);
        }

        public ITransaction BeginTransaction()
        {
            return Session.BeginTransaction();
        }

        public ITransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return Session.BeginTransaction(isolationLevel);
        }

        public ICriteria CreateCriteria<T>() where T : class
        {
            return Session.CreateCriteria<T>();
        }

        public ICriteria CreateCriteria<T>(string alias) where T : class
        {
            return Session.CreateCriteria<T>(alias);
        }

        public ICriteria CreateCriteria(Type persistentClass)
        {
            return Session.CreateCriteria(persistentClass);
        }

        public ICriteria CreateCriteria(Type persistentClass, string alias)
        {
            return Session.CreateCriteria(persistentClass, alias);
        }

        public ICriteria CreateCriteria(string entityName)
        {
            return Session.CreateCriteria(entityName);
        }

        public ICriteria CreateCriteria(string entityName, string alias)
        {
            return Session.CreateCriteria(entityName, alias);
        }

        public IQueryOver<T, T> QueryOver<T>() where T : class
        {
            return Session.QueryOver<T>();
        }

        public IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class
        {
            return Session.QueryOver(alias);
        }

        public IQueryOver<T, T> QueryOver<T>(string entityName) where T : class
        {
            return Session.QueryOver<T>(entityName);
        }

        public IQueryOver<T, T> QueryOver<T>(string entityName, Expression<Func<T>> alias) where T : class
        {
            return Session.QueryOver(entityName, alias);
        }

        public IQuery CreateQuery(string queryString)
        {
            return Session.CreateQuery(queryString);
        }

        public IQuery CreateFilter(object collection, string queryString)
        {
            return Session.CreateFilter(collection, queryString);
        }

        public IQuery GetNamedQuery(string queryName)
        {
            return Session.GetNamedQuery(queryName);
        }

        public ISQLQuery CreateSQLQuery(string queryString)
        {
            return Session.CreateSQLQuery(queryString);
        }

        public void Clear()
        {
            //Log.Debug("Clear() >><<");
            Session.Clear();
        }

        public object Get(Type clazz, object id)
        {
            return Session.Get(clazz, id);
        }

        public object Get(Type clazz, object id, LockMode lockMode)
        {
            return Session.Get(clazz, id, lockMode);
        }

        public object Get(string entityName, object id)
        {
            return Session.Get(entityName, id);
        }

        public T Get<T>(object id)
        {
            return Session.Get<T>(id);
        }

        public T Get<T>(object id, LockMode lockMode)
        {
            return Session.Get<T>(id, lockMode);
        }

        public Task<object> GetAsync(Type clazz, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.GetAsync(clazz, id, cancellationToken);
        }

        public Task<object> GetAsync(Type clazz, object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.GetAsync(clazz, id, lockMode, cancellationToken);
        }

        public Task<object> GetAsync(string entityName, object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.GetAsync(entityName, id, cancellationToken);
        }

        public Task<T> GetAsync<T>(object id, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.GetAsync<T>(id, cancellationToken);
        }

        public Task<T> GetAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Session.GetAsync<T>(id, lockMode, cancellationToken);
        }

        public string GetEntityName(object obj)
        {
            return Session.GetEntityName(obj);
        }

        public IFilter EnableFilter(string filterName)
        {
            return Session.EnableFilter(filterName);
        }

        public IFilter GetEnabledFilter(string filterName)
        {
            return Session.GetEnabledFilter(filterName);
        }

        public void DisableFilter(string filterName)
        {
            Session.DisableFilter(filterName);
        }

        public IMultiQuery CreateMultiQuery()
        {
            return Session.CreateMultiQuery();
        }

        public ISession SetBatchSize(int batchSize)
        {
            return Session.SetBatchSize(batchSize);
        }

        public ISessionImplementor GetSessionImplementation()
        {
            return Session.GetSessionImplementation();
        }

        public IMultiCriteria CreateMultiCriteria()
        {
            return Session.CreateMultiCriteria();
        }

        public ISession GetSession(EntityMode entityMode)
        {
            return Session.GetSession(entityMode);
        }

        public EntityMode ActiveEntityMode
        {
            get
            {
                // ISession does not expose ActiveEntityMode, but UnitOfWork implements ISession,
                // so we can safely return a default or throw if not supported.
                // If you have access to the underlying session implementation that exposes ActiveEntityMode,
                // cast Session to that type. Otherwise, return a default value or throw.
                if (Session is UnitOfWork uow)
                {
                    return uow.ActiveEntityMode;
                }
                // Fallback: return default or throw
                return EntityMode.Poco;
            }
        }

        public FlushMode FlushMode
        {
            get => Session.FlushMode;
            set => Session.FlushMode = value;
        }

        public CacheMode CacheMode
        {
            get => Session.CacheMode;
            set => Session.CacheMode = value;
        }

        public ISessionFactory SessionFactory => Session.SessionFactory;

        public IDbConnection Connection => Session.Connection;

        public bool IsOpen => Session.IsOpen;

        public bool IsConnected => Session.IsConnected;

        public bool DefaultReadOnly
        {
            get => Session.DefaultReadOnly;
            set => Session.DefaultReadOnly = value;
        }


        public ISessionStatistics Statistics => Session.Statistics;

        public event Action<IUnitOfWork> Disposed;

        public void Dispose()
        {
            UsageCount--;

            if (UsageCount <= 0 && !_hasBeenDisposed)
            {
                ForceDispose();
            }
        }

        public void ForceDispose()
        {
            if (_hasBeenDisposed)
            {
                return;
            }

            _hasBeenDisposed = true;

            Session.Dispose();

            FireDisposed();

            //GC.SuppressFinalize(this);
        }

        private void FireDisposed()
        {
            if (Disposed != null)
            {
                Disposed(this);
            }
        }

        public Task<bool> IsDirtyAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task EvictAsync(object obj, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync(string query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync(string query, object value, IType type, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<int> DeleteAsync(string query, object[] values, IType[] types, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<IQuery> CreateFilterAsync(object collection, string queryString, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<string> GetEntityNameAsync(object obj, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public ISharedSessionBuilder SessionWithOptions()
        {
            throw new NotImplementedException();
        }

        DbConnection ISession.Disconnect()
        {
            throw new NotImplementedException();
        }

        public void Reconnect(DbConnection connection)
        {
            throw new NotImplementedException();
        }

        DbConnection ISession.Close()
        {
            return Session?.Close();
        }

        public void Load(object obj, object id)
        {
            throw new NotImplementedException();
        }

        public void JoinTransaction()
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> Query<T>()
        {
            return Session.Query<T>();
        }

        public IQueryable<T> Query<T>(string entityName)
        {
            // entityName is ignored here; return typed queryable from session
            return Session.Query<T>();
        }

        DbConnection ISession.Connection { get; }

        void ISession.Save(string entityName, object obj, object id)
        {
            throw new NotImplementedException();
        }
    }

    public static class UnitOfWorkExtensions
    {
        private static readonly ILog<UnitOfWork> Log = new Log<UnitOfWork>();

        public static void ExcludeSoftDeletedValues(this IUnitOfWork uow)
        {
            Log.Info("ExcludeSoftDeletedValues()");
            uow.Session.EnableFilter("SoftDeletesFilter");
        }

        public static void IncludeSoftDeletedValues(this IUnitOfWork uow)
        {
            Log.Info("IncludeSoftDeletedValues()");
            uow.Session.DisableFilter("SoftDeletesFilter");
        }
    }
}