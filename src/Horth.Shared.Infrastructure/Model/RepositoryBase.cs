using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Irc.Infrastructure.Model;
using Microsoft.EntityFrameworkCore;

namespace Horth.Service.Email.Shared.Model
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected readonly DbContext Context;

        public Repository(DbContext context)
        {
            Context = context;
        }

        public virtual TEntity Get(int id)
        {
            return Context.Set<TEntity>().Find(id);
        }
        public virtual TEntity Get(string key)
        {
            return Context.Set<TEntity>().Find(key);
        }

        public virtual TEntity Get(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().FirstOrDefault(predicate);
        }


        public virtual IList<TEntity> GetAll()
        {
            return Context.Set<TEntity>().ToList();
        }
        public virtual async Task<IList<TEntity>> GetAllAsync()
        {
            List<TEntity> ret = null;
            await Task.Run(() =>
            {
                ret = Context.Set<TEntity>().ToList();
            });
            return ret;
        }

        public virtual async Task<IList<TEntity>> GetAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            List<TEntity> ret = null;
            await Task.Run(() =>
            {
                ret = Where(predicate).ToList();
            });
            return ret;
        }

        public virtual async Task<TEntity> GetAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var ret = default(TEntity);
            await Task.Run(() =>
            {
                ret = Get(predicate);
            });
            return ret;
        }

        public virtual IList<TEntity> GetList(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where<TEntity>(predicate).ToList();
        }
        public virtual async Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IList<TEntity> ret = null;
            await Task.Run(() =>
            {
                ret = GetList(predicate);
            });
            return ret;
        }


        public virtual IQueryable<TEntity> Where(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where<TEntity>(predicate);
        }
        public virtual async Task<IQueryable<TEntity>> WhereAsync(Expression<Func<TEntity, bool>> predicate)
        {
            IQueryable<TEntity> ret = null;
            await Task.Run(() =>
            {
                ret = Where(predicate);
            });
            return ret;
        }


        public TEntity GetFirst(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where(predicate).FirstOrDefault();
        }

        public virtual async Task<TEntity> GetFirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            TEntity ret = null;
            await Task.Run(() =>
            {
                ret = GetFirst(predicate);
            });
            return ret;
        }

        public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            int ret = 0;
            await Task.Run(() =>
            {
                ret = Count(predicate);
            });
            return ret;
        }

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Count(predicate);
        }

        public int Count()
        {
            return Context.Set<TEntity>().Count();
        }

        public void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public void AddRange(IList<TEntity> entities)
        {
            Context.Set<TEntity>().AddRange(entities);
        }

        public void Remove(int id)
        {
            Context.Set<TEntity>().Remove(Get(id));
        }
        public void Remove(Expression<Func<TEntity, bool>> predicate)
        {
            var e = GetFirst(predicate);
            Context.Set<TEntity>().Remove(e);
            Context.SaveChanges();
        }
        public async Task RemoveAsync(Expression<Func<TEntity, bool>> predicate)
        {
            await Task.Run(() =>
            {
                Remove(predicate);
            });
        }
        public void Remove(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
            Context.SaveChanges();
        }

        public async Task RemoveRangeAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var items = await GetAllAsync(predicate);
            await Task.Run(() =>
            {
                RemoveRange(items);
            });
        }
        public void RemoveRange(IList<TEntity> entities)
        {
            Context.Set<TEntity>().RemoveRange(entities);
            Context.SaveChanges();
        }

        public void DetectChangesOff()
        {
            Context.ChangeTracker.AutoDetectChangesEnabled=false;
        }
        public void DetectChangesOn()
        {
            Context.ChangeTracker.DetectChanges();
            Context.ChangeTracker.AutoDetectChangesEnabled = true;
        }

        //public DataTableResult<TDto> GetJson<TDto>(OoTableParameters dt)
        //{
        //    return dt.GetJson<TEntity,TDto>(Context.Set<TEntity>(), _mapper);
        //}
        //public DataTableResult<TDto> GetJson<TDto>(OoTableParameters dt, Expression<Func<TEntity, bool>> predicate)
        //{
        //    return dt.GetJson<TEntity,TDto>(Context.Set<TEntity>().Where(predicate), _mapper);
        //}
        //public async Task<DataTableResult<TDto>> GetJsonAsync<TDto>(OoTableParameters dt)
        //{
        //    DataTableResult<TDto> jData = null;
        //    await Task.Run(() =>
        //    {
        //        jData = dt.GetJson<TEntity, TDto>(Context.Set<TEntity>(),_mapper);
        //    });
        //    return jData;
        //}
        //public List<DataTabViewModel> GetTabData(Expression<Func<TEntity, string>> predicate)
        //{
        //    return Context.Set<TEntity>().GroupBy(predicate)
        //        .OrderBy(p => p.Id)
        //        .Select(g => new DataTabViewModel { Name = g.Id, Count = g.Count() }).ToList();
        //}
        //protected async Task<DataTableResult<TDto>> GetJsonAsync<TDto>(OoTableParameters dt, Expression<Func<TEntity, bool>> predicate)
        //{
        //    DataTableResult<TDto> jData = null;
        //    await Task.Run(() =>
        //    {
        //        jData = dt.GetJson<TEntity, TDto>(Context.Set<TEntity>().Where(predicate),_mapper);
        //    });
        //    return jData;
        //}
        //public async Task<List<DataTabViewModel>> GetTabDataAsync(Expression<Func<TEntity, string>> predicate)
        //{
        //    List<DataTabViewModel> ret = null;
        //    await Task.Run(() =>
        //    {
        //        ret = Context.Set<TEntity>().GroupBy(predicate)
        //            .OrderBy(p => p.Id)
        //            .Select(g => new DataTabViewModel { Name = g.Id, Count = g.Count() }).ToList();
        //    });
        //    return ret;

        //}
    }
}
