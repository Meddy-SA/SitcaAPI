using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Sitca.DataAccess.Data.Repository.IRepository
{
    public interface IRepository<T> where T : class
    {
        Task<T> Get(int id);

        Task<IEnumerable<T>> GetAll(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null
            );

        Task<T> GetFirstOrDefault(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null
            );

        Task Add(T entity);

        void Remove(T entity);

        Task Remove(int id);
    }
}
