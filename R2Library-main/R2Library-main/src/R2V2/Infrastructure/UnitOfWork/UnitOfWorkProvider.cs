#region

using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NHibernate;
using NHibernate.Engine;
using NHibernate.Stat;
using NHibernate.Type;
using R2V2.Extensions;
using R2V2.Infrastructure.Storages;
using NHibernate.Linq;

#endregion

namespace R2V2.Infrastructure.UnitOfWork
{
 public class UnitOfWorkProvider : IUnitOfWorkProvider
 {
 public const string ActiveUoWKey = "Uow.Stateful.Active";
 private readonly ILocalStorageService _localStorageService;
 private readonly ISessionFactory _sessionFactory;

 public UnitOfWorkProvider(ISessionFactory sessionFactory, ILocalStorageService localStorageService)
 {
 _sessionFactory = sessionFactory;
 _localStorageService = localStorageService;
 }

 public IUnitOfWork Current
 {
 get
 {
 if (HasCurrent)
 {
 return _localStorageService.Get(ActiveUoWKey).As<IUnitOfWork>();
 }

 throw new InvalidOperationException("No unit of work! Did you start one?");
 }
 set
 {
 if (HasCurrent)
 {
 Current.ForceCommit();
 //Current.Dispose(DisposingScope.Request);
 }

 _localStorageService.Put(ActiveUoWKey, value);
 }
 }

 public bool HasCurrent => _localStorageService.Has(ActiveUoWKey);

 protected IUnitOfWork ActiveUnitOfWork
 {
 get
 {
 if (!HasCurrent)
 Start(UnitOfWorkScope.NewOrCurrent);

 return Current;
 }
 }

 public IUnitOfWork Start()
 {
 return Start(UnitOfWorkScope.NewOrCurrent);
 }

 public IUnitOfWork Start(UnitOfWorkScope scope)
 {
 IUnitOfWork uow;

 // - DO NOT USE UNLESS YOU HAVE MY OK!!! - SJS -1/21/2013
 if (scope == UnitOfWorkScope.New)
 {
 uow = new UnitOfWork(_sessionFactory.OpenSession());
 uow.Session.EnableFilter("SoftDeletesFilter");
 return uow;
 }

 var debug = new StringBuilder()
 .AppendFormat("HasCurrent: {0}, _localStorageService.Has(ActiveUoWKey): {1}", HasCurrent,
 _localStorageService.Has(ActiveUoWKey));
 if (scope == UnitOfWorkScope.NewOrCurrent && HasCurrent)
 {
 debug.AppendFormat(" --> Start() - Id: {0} -> NewOrCurrent & HasCurrent ", Current.Id);
 uow = Current;
 }
 else
 {
 uow = new UnitOfWork(_sessionFactory.OpenSession());
 uow.Session.EnableFilter("SoftDeletesFilter");
 debug.AppendFormat(" --> Start() - Id: {0} -> NewOrCurrent & !HasCurrent ", uow.Id);

 uow.Disposed += OnCurrentDisposed;
 Current = uow;
 }

 uow.IncrementUsage();
 debug.AppendFormat("uow.UsageCount: {0}", uow.UsageCount);
 return uow;
 }

 public ISession Session => ActiveUnitOfWork.Session;

 public void Commit()
 {
 ActiveUnitOfWork.Commit();
 }

 public void Delete<T>(object id)
 {
 ActiveUnitOfWork.Delete<T>(id);
 }

 public event Action<IUnitOfWork> Disposed;

 public Guid Id => ActiveUnitOfWork.Id;

 public void IncrementUsage()
 {
 ActiveUnitOfWork.IncrementUsage();
 }

 public void ForceCommit()
 {
 ActiveUnitOfWork.ForceCommit();
 }

 public void Execute(IUnitOfWorkCommand command)
 {
 ActiveUnitOfWork.Execute(command);
 }

 public int UsageCount => ActiveUnitOfWork.UsageCount;

 public void Dispose()
 {
 if (HasCurrent)
 {
 Current.Dispose();
 }

 FireDisposed();
 }

 public void Flush()
 {
 ActiveUnitOfWork.Flush();
 }

 public Task FlushAsync(CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.FlushAsync(cancellationToken);
 }

 public IDbConnection Disconnect()
 {
 return ActiveUnitOfWork.Disconnect();
 }

 public void Reconnect()
 {
 ActiveUnitOfWork.Reconnect();
 }

 public void Reconnect(IDbConnection connection)
 {
 // Call the DbConnection overload on the underlying implementation when available
 if (connection is DbConnection dbConnection)
 {
 ActiveUnitOfWork.Reconnect(dbConnection);
 }
 else
 {
 // If underlying implementation exposes IDbConnection overload (custom UnitOfWork), prefer that
 // but IUnitOfWork (ISession) defines Reconnect(DbConnection) so throw if not convertible
 throw new ArgumentException("connection must be a DbConnection for underlying session reconnect.", nameof(connection));
 }
 }

 public IDbConnection Close()
 {
 if (HasCurrent)
 {
 return ActiveUnitOfWork.Close();
 }

 return null;
 }

 public void CancelQuery()
 {
 ActiveUnitOfWork.CancelQuery();
 }

 public bool IsDirty()
 {
 return ActiveUnitOfWork.IsDirty();
 }

 public bool IsReadOnly(object entityOrProxy)
 {
 return ActiveUnitOfWork.IsReadOnly(entityOrProxy);
 }

 public void SetReadOnly(object entityOrProxy, bool readOnly)
 {
 ActiveUnitOfWork.SetReadOnly(entityOrProxy, readOnly);
 }

 public object GetIdentifier(object obj)
 {
 return ActiveUnitOfWork.GetIdentifier(obj);
 }

 public bool Contains(object obj)
 {
 return ActiveUnitOfWork.Contains(obj);
 }

 public void Evict(object obj)
 {
 ActiveUnitOfWork.Evict(obj);
 }

 public object Load(Type theType, object id, LockMode lockMode)
 {
 return ActiveUnitOfWork.Load(theType, id, lockMode);
 }

 public object Load(string entityName, object id, LockMode lockMode)
 {
 return ActiveUnitOfWork.Load(entityName, id, lockMode);
 }

 public object Load(Type theType, object id)
 {
 return ActiveUnitOfWork.Load(theType, id);
 }

 public T Load<T>(object id, LockMode lockMode)
 {
 return ActiveUnitOfWork.Load<T>(id, lockMode);
 }

 public T Load<T>(object id)
 {
 return ActiveUnitOfWork.Load<T>(id);
 }

 public object Load(string entityName, object id)
 {
 return ActiveUnitOfWork.Load(entityName, id);
 }

 public void Load(object obj, object id)
 {
 ActiveUnitOfWork.Load(obj, id);
 }

 public Task<object> LoadAsync(Type theType, object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LoadAsync(theType, id, lockMode, cancellationToken);
 }

 public Task<object> LoadAsync(string entityName, object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LoadAsync(entityName, id, lockMode, cancellationToken);
 }

 public Task<object> LoadAsync(Type theType, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LoadAsync(theType, id, cancellationToken);
 }

 public Task<T> LoadAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LoadAsync<T>(id, lockMode, cancellationToken);
 }

 public Task<T> LoadAsync<T>(object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LoadAsync<T>(id, cancellationToken);
 }

 public Task<object> LoadAsync(string entityName, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LoadAsync(entityName, id, cancellationToken);
 }

 public Task LoadAsync(object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LoadAsync(obj, id, cancellationToken);
 }

 public void Replicate(object obj, ReplicationMode replicationMode)
 {
 ActiveUnitOfWork.Replicate(obj, replicationMode);
 }

 public void Replicate(string entityName, object obj, ReplicationMode replicationMode)
 {
 ActiveUnitOfWork.Replicate(entityName, obj, replicationMode);
 }

 public Task ReplicateAsync(object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.ReplicateAsync(obj, replicationMode, cancellationToken);
 }

 public Task ReplicateAsync(string entityName, object obj, ReplicationMode replicationMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.ReplicateAsync(entityName, obj, replicationMode, cancellationToken);
 }

 public object Save(object obj)
 {
 return ActiveUnitOfWork.Save(obj);
 }

 public void Save(object obj, object id)
 {
 ActiveUnitOfWork.Save(obj, id);
 }

 public object Save(string entityName, object obj)
 {
 return ActiveUnitOfWork.Save(entityName, obj);
 }

 public void Save(string entityName, object obj, object id)
 {
 ActiveUnitOfWork.Save(entityName, obj, id);
 }

 public Task<object> SaveAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.SaveAsync(obj, cancellationToken);
 }

 public Task SaveAsync(object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.SaveAsync(obj, id, cancellationToken);
 }

 public Task<object> SaveAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.SaveAsync(entityName, obj, cancellationToken);
 }

 public Task SaveAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.SaveAsync(entityName, obj, id, cancellationToken);
 }

 public void SaveOrUpdate(object obj)
 {
 ActiveUnitOfWork.SaveOrUpdate(obj);
 }

 public void SaveOrUpdate(string entityName, object obj)
 {
 ActiveUnitOfWork.SaveOrUpdate(entityName, obj);
 }

 public void SaveOrUpdate(string entityName, object obj, object id)
 {
 ActiveUnitOfWork.SaveOrUpdate(entityName, obj, id);
 }

 public Task SaveOrUpdateAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.SaveOrUpdateAsync(obj, cancellationToken);
 }

 public Task SaveOrUpdateAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.SaveOrUpdateAsync(entityName, obj, cancellationToken);
 }

 public Task SaveOrUpdateAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.SaveOrUpdateAsync(entityName, obj, id, cancellationToken);
 }

 public void Update(object obj)
 {
 ActiveUnitOfWork.Update(obj);
 }

 public void Update(object obj, object id)
 {
 ActiveUnitOfWork.Update(obj, id);
 }

 public void Update(string entityName, object obj)
 {
 ActiveUnitOfWork.Update(entityName, obj);
 }

 public void Update(string entityName, object obj, object id)
 {
 ActiveUnitOfWork.Update(entityName, obj, id);
 }

 public Task UpdateAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.UpdateAsync(obj, cancellationToken);
 }

 public Task UpdateAsync(object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.UpdateAsync(obj, id, cancellationToken);
 }

 public Task UpdateAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.UpdateAsync(entityName, obj, cancellationToken);
 }

 public Task UpdateAsync(string entityName, object obj, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.UpdateAsync(entityName, obj, id, cancellationToken);
 }

 public object Merge(object obj)
 {
 return ActiveUnitOfWork.Merge(obj);
 }

 public object Merge(string entityName, object obj)
 {
 return ActiveUnitOfWork.Merge(entityName, obj);
 }

 public T Merge<T>(T entity) where T : class
 {
 return ActiveUnitOfWork.Merge(entity);
 }

 public T Merge<T>(string entityName, T entity) where T : class
 {
 return ActiveUnitOfWork.Merge(entityName, entity);
 }

 public Task<object> MergeAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.MergeAsync(obj, cancellationToken);
 }

 public Task<object> MergeAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.MergeAsync(entityName, obj, cancellationToken);
 }

 public Task<T> MergeAsync<T>(T entity, CancellationToken cancellationToken = default(CancellationToken)) where T : class
 {
 return ActiveUnitOfWork.MergeAsync(entity, cancellationToken);
 }

 public Task<T> MergeAsync<T>(string entityName, T entity, CancellationToken cancellationToken = default(CancellationToken)) where T : class
 {
 return ActiveUnitOfWork.MergeAsync(entityName, entity, cancellationToken);
 }

 public void Persist(object obj)
 {
 ActiveUnitOfWork.Persist(obj);
 }

 public void Persist(string entityName, object obj)
 {
 ActiveUnitOfWork.Persist(entityName, obj);
 }

 public Task PersistAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.PersistAsync(obj, cancellationToken);
 }

 public Task PersistAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.PersistAsync(entityName, obj, cancellationToken);
 }

 public void Delete(object obj)
 {
 ActiveUnitOfWork.Delete(obj);
 }

 public void Delete(string entityName, object obj)
 {
 ActiveUnitOfWork.Delete(entityName, obj);
 }

 public int Delete(string query)
 {
 return ActiveUnitOfWork.Delete(query);
 }

 public int Delete(string query, object value, IType type)
 {
 return ActiveUnitOfWork.Delete(query, value, type);
 }

 public int Delete(string query, object[] values, IType[] types)
 {
 return ActiveUnitOfWork.Delete(query, values, types);
 }

 public Task DeleteAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.DeleteAsync(obj, cancellationToken);
 }

 public Task DeleteAsync(string entityName, object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.DeleteAsync(entityName, obj, cancellationToken);
 }

 public void Lock(object obj, LockMode lockMode)
 {
 ActiveUnitOfWork.Lock(obj, lockMode);
 }

 public void Lock(string entityName, object obj, LockMode lockMode)
 {
 ActiveUnitOfWork.Lock(entityName, obj, lockMode);
 }

 public Task LockAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LockAsync(obj, lockMode, cancellationToken);
 }

 public Task LockAsync(string entityName, object obj, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.LockAsync(entityName, obj, lockMode, cancellationToken);
 }

 public void Refresh(object obj)
 {
 ActiveUnitOfWork.Refresh(obj);
 }

 public void Refresh(object obj, LockMode lockMode)
 {
 ActiveUnitOfWork.Refresh(obj, lockMode);
 }

 public Task RefreshAsync(object obj, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.RefreshAsync(obj, cancellationToken);
 }

 public Task RefreshAsync(object obj, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.RefreshAsync(obj, lockMode, cancellationToken);
 }

 public LockMode GetCurrentLockMode(object obj)
 {
 return ActiveUnitOfWork.GetCurrentLockMode(obj);
 }

 public ITransaction BeginTransaction()
 {
 return ActiveUnitOfWork.BeginTransaction();
 }

 public ITransaction BeginTransaction(IsolationLevel isolationLevel)
 {
 return ActiveUnitOfWork.BeginTransaction(isolationLevel);
 }

 public ICriteria CreateCriteria<T>() where T : class
 {
 return ActiveUnitOfWork.CreateCriteria<T>();
 }

 public ICriteria CreateCriteria<T>(string alias) where T : class
 {
 return ActiveUnitOfWork.CreateCriteria<T>(alias);
 }

 public ICriteria CreateCriteria(Type persistentClass)
 {
 return ActiveUnitOfWork.CreateCriteria(persistentClass);
 }

 public ICriteria CreateCriteria(Type persistentClass, string alias)
 {
 return ActiveUnitOfWork.CreateCriteria(persistentClass, alias);
 }

 public ICriteria CreateCriteria(string entityName)
 {
 return ActiveUnitOfWork.CreateCriteria(entityName);
 }

 public ICriteria CreateCriteria(string entityName, string alias)
 {
 return ActiveUnitOfWork.CreateCriteria(entityName, alias);
 }

 public IQueryOver<T, T> QueryOver<T>() where T : class
 {
 return ActiveUnitOfWork.QueryOver<T>();
 }

 public IQueryOver<T, T> QueryOver<T>(Expression<Func<T>> alias) where T : class
 {
 return ActiveUnitOfWork.QueryOver(alias);
 }

 public IQueryOver<T, T> QueryOver<T>(string entityName) where T : class
 {
 return ActiveUnitOfWork.QueryOver<T>(entityName);
 }

 public IQueryOver<T, T> QueryOver<T>(string entityName, Expression<Func<T>> alias) where T : class
 {
 return ActiveUnitOfWork.QueryOver(entityName, alias);
 }

 public IQuery CreateQuery(string queryString)
 {
 return ActiveUnitOfWork.CreateQuery(queryString);
 }

 public IQuery CreateFilter(object collection, string queryString)
 {
 return ActiveUnitOfWork.CreateFilter(collection, queryString);
 }

 public IQuery GetNamedQuery(string queryName)
 {
 return ActiveUnitOfWork.GetNamedQuery(queryName);
 }

 public ISQLQuery CreateSQLQuery(string queryString)
 {
 return ActiveUnitOfWork.CreateSQLQuery(queryString);
 }

 public void Clear()
 {
 if (HasCurrent)
 {
 ActiveUnitOfWork.Clear();
 }
 }

 public object Get(Type clazz, object id)
 {
 return ActiveUnitOfWork.Get(clazz, id);
 }

 public object Get(Type clazz, object id, LockMode lockMode)
 {
 return ActiveUnitOfWork.Get(clazz, id, lockMode);
 }

 public object Get(string entityName, object id)
 {
 return ActiveUnitOfWork.Get(entityName, id);
 }

 public T Get<T>(object id)
 {
 return ActiveUnitOfWork.Get<T>(id);
 }

 public T Get<T>(object id, LockMode lockMode)
 {
 return ActiveUnitOfWork.Get<T>(id, lockMode);
 }

 public Task<object> GetAsync(Type clazz, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.GetAsync(clazz, id, cancellationToken);
 }

 public Task<object> GetAsync(Type clazz, object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.GetAsync(clazz, id, lockMode, cancellationToken);
 }

 public Task<object> GetAsync(string entityName, object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.GetAsync(entityName, id, cancellationToken);
 }

 public Task<T> GetAsync<T>(object id, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.GetAsync<T>(id, cancellationToken);
 }

 public Task<T> GetAsync<T>(object id, LockMode lockMode, CancellationToken cancellationToken = default(CancellationToken))
 {
 return ActiveUnitOfWork.GetAsync<T>(id, lockMode, cancellationToken);
 }

 public string GetEntityName(object obj)
 {
 return ActiveUnitOfWork.GetEntityName(obj);
 }

 public IFilter EnableFilter(string filterName)
 {
 return ActiveUnitOfWork.EnableFilter(filterName);
 }

 public IFilter GetEnabledFilter(string filterName)
 {
 return ActiveUnitOfWork.GetEnabledFilter(filterName);
 }

 public void DisableFilter(string filterName)
 {
 ActiveUnitOfWork.DisableFilter(filterName);
 }

 public IMultiQuery CreateMultiQuery()
 {
 return ActiveUnitOfWork.CreateMultiQuery();
 }

 public ISession SetBatchSize(int batchSize)
 {
 return ActiveUnitOfWork.SetBatchSize(batchSize);
 }

 public ISessionImplementor GetSessionImplementation()
 {
 return ActiveUnitOfWork.GetSessionImplementation();
 }

 public IMultiCriteria CreateMultiCriteria()
 {
 return ActiveUnitOfWork.CreateMultiCriteria();
 }

 public ISession GetSession(EntityMode entityMode)
 {
 return ActiveUnitOfWork.GetSession(entityMode);
 }

 public EntityMode ActiveEntityMode
 {
 get
 {
 // ISession does not have ActiveEntityMode, but UnitOfWorkProvider implements ISession,
 // and exposes GetSession(EntityMode) which returns an ISession that may have ActiveEntityMode.
 // So, use GetSession(EntityMode.Poco) and cast to the concrete type if needed.
 var session = ActiveUnitOfWork.Session;
 // Try to cast to UnitOfWorkProvider or ISessionImplementor if available, otherwise fallback
 if (session is UnitOfWorkProvider uowProvider)
 {
 return uowProvider.ActiveEntityMode;
 }
 // If GetSession(EntityMode) is available, use it
 var sessionWithEntityMode = session.GetSession(EntityMode.Poco);
 if (sessionWithEntityMode is ISession sessionImpl)
 {
 // Try to get ActiveEntityMode property via reflection if present
 var prop = sessionImpl.GetType().GetProperty("ActiveEntityMode");
 if (prop != null)
 {
 return (EntityMode)prop.GetValue(sessionImpl);
 }
 }
 // Fallback to default
 return EntityMode.Poco;
 }
 }

 public FlushMode FlushMode
 {
 get => ActiveUnitOfWork.FlushMode;
 set => ActiveUnitOfWork.FlushMode = value;
 }

 public CacheMode CacheMode
 {
 get => ActiveUnitOfWork.CacheMode;
 set => ActiveUnitOfWork.CacheMode = value;
 }

 public ISessionFactory SessionFactory => ActiveUnitOfWork.SessionFactory;

 public IDbConnection Connection => ActiveUnitOfWork.Connection;

 public bool IsOpen => ActiveUnitOfWork.IsOpen;

 public bool IsConnected => ActiveUnitOfWork.IsConnected;

 public bool DefaultReadOnly
 {
 get => ActiveUnitOfWork.DefaultReadOnly;
 set => ActiveUnitOfWork.DefaultReadOnly = value;
 }

 public ITransaction Transaction => ActiveUnitOfWork.Transaction;

 public ISessionStatistics Statistics => ActiveUnitOfWork.Statistics;

 private void OnCurrentDisposed(IUnitOfWork obj)
 {
 _localStorageService.Remove(ActiveUoWKey);
 obj.Disposed -= OnCurrentDisposed;
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
 return ActiveUnitOfWork.IsDirtyAsync(cancellationToken);
 }

 public Task EvictAsync(object obj, CancellationToken cancellationToken = default)
 {
 return ActiveUnitOfWork.EvictAsync(obj, cancellationToken);
 }

 public Task<int> DeleteAsync(string query, CancellationToken cancellationToken = default)
 {
 return ActiveUnitOfWork.DeleteAsync(query, cancellationToken);
 }

 public Task<int> DeleteAsync(string query, object value, IType type, CancellationToken cancellationToken = default)
 {
 return ActiveUnitOfWork.DeleteAsync(query, value, type, cancellationToken);
 }

 public Task<int> DeleteAsync(string query, object[] values, IType[] types, CancellationToken cancellationToken = default)
 {
 return ActiveUnitOfWork.DeleteAsync(query, values, types, cancellationToken);
 }

 public Task<IQuery> CreateFilterAsync(object collection, string queryString, CancellationToken cancellationToken = default)
 {
 return ActiveUnitOfWork.CreateFilterAsync(collection, queryString, cancellationToken);
 }

 public Task<string> GetEntityNameAsync(object obj, CancellationToken cancellationToken = default)
 {
 return ActiveUnitOfWork.GetEntityNameAsync(obj, cancellationToken);
 }

 public ISharedSessionBuilder SessionWithOptions()
 {
 return ActiveUnitOfWork.SessionWithOptions();
 }

 DbConnection ISession.Disconnect()
 {
 return ActiveUnitOfWork.Disconnect() as DbConnection;
 }

 public void Reconnect(DbConnection connection)
 {
 ActiveUnitOfWork.Reconnect(connection);
 }

 DbConnection ISession.Close()
 {
 return ActiveUnitOfWork.Close() as DbConnection;
 }

 public void JoinTransaction()
 {
 ActiveUnitOfWork.JoinTransaction();
 }

 public IQueryable<T> Query<T>()
 {
 return ActiveUnitOfWork.Query<T>();
 }

 public IQueryable<T> Query<T>(string entityName)
 {
 return ActiveUnitOfWork.Query<T>(entityName);
 }

 DbConnection ISession.Connection { get; }
 }

 public enum UnitOfWorkScope
 {
 /// <summary>
 /// DO NOT USE UNLESS YOU HAVE MY OK!!! - SJS -1/21/2013
 /// </summary>
 New,

 /// <summary>
 /// This is the default and should always be used unless you really know what you are doing.
 /// </summary>
 NewOrCurrent
 }
}