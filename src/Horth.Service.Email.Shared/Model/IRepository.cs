using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Irc.Infrastructure.Model
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity Get(int id);
        TEntity Get(string key);
        TEntity Get(Expression<Func<TEntity, bool>> predicate);

        IList<TEntity> GetAll();
        Task<IList<TEntity>> GetAllAsync();
        Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate);

        IList<TEntity> GetList(Expression<Func<TEntity, bool>> predicate);
        Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate);


        TEntity GetFirst(Expression<Func<TEntity, bool>> predicate);
        Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate);

        int Count();
        int Count(Expression<Func<TEntity, bool>> predicate);
        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

        IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate);
        Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate);

        void Add(TEntity entity);
        void AddRange(IList<TEntity> entities);

        void Remove(int id);
        void Remove(TEntity entity);
        void RemoveRange(IList<TEntity> entities);
        void Remove(Expression<Func<TEntity, bool>> predicate);
        Task RemoveAsync(Expression<Func<TEntity, bool>> predicate);
        Task RemoveRangeAsync(Expression<Func<TEntity, bool>> predicate);

        void DetectChangesOff();
        void DetectChangesOn();

    }
}
