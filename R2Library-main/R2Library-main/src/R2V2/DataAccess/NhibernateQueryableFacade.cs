#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using R2V2.Infrastructure.UnitOfWork;

#endregion

namespace R2V2.DataAccess
{
    public class NhibernateQueryableFacade<T> : IQueryable<T>
    {
        private readonly IQueryable<T> _queryable;

        public NhibernateQueryableFacade(IUnitOfWork unitOfWork)
        {
            _queryable = unitOfWork.Query<T>();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _queryable.GetEnumerator();
        }

        public Expression Expression => _queryable.Expression;

        public Type ElementType => _queryable.ElementType;

        public IQueryProvider Provider => _queryable.Provider;
    }
}