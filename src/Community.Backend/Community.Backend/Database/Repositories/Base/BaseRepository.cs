using Comunity.Models.Base;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Data.SqlClient;
using LinqKit;

namespace Community.Backend.Database.Repositories.Base
{
    public interface IBaseRepository<Tmodel> where Tmodel :class,IBaseModel
    {
        Task<int> CommitChangesAsync();
        Task<Tmodel> GetByIDAsync(object id);
        Task<IQueryable<Tmodel>> GetAsync(Expression<Func<Tmodel, bool>> where, string includeProperties = "");
        Task<IQueryable<Tmodel>> GetAsync(Expression<Func<Tmodel, bool>> where, params Expression<Func<Tmodel, object>>[] include);
        Task<IQueryable<Tmodel>> GetAsync(params Expression<Func<Tmodel, object>>[] include);
        Task<IQueryable<Tmodel>> GetAllAsync();
        Task<int> CountAsync();
        Task<Tmodel> InsertAsync(Tmodel entity);
        Task<Tmodel> UpdateAsync(Tmodel entity);
        Task<Tmodel> UpdateAsync(Tmodel entity, object id);
        Task UpdatePropertyAsync<Type>(Expression<Func<Tmodel, Type>> property, Tmodel entity);
        Task DeleteAsync(Tmodel Entity);
        Task DeleteAsync(object id);
        Task DeleteAsync(Expression<Func<Tmodel, bool>> primaryKeys);
        Task<SqlDataReader> RunAsync(string query);
        Task DeleteRangeAsync(IEnumerable<Tmodel> entities);
        Task<IEnumerable<Tmodel>> InsertRangeAsync(IEnumerable<Tmodel> entities);
        Task<IEnumerable<Tmodel>> UpdateRangeAsync(IEnumerable<Tmodel> entities);
    }
    public class BaseRepository<Tmodel>:IBaseRepository<Tmodel> where Tmodel:class,IBaseModel
    {
        protected readonly DatabaseContext Context;

        public BaseRepository(DatabaseContext context)
        {
            Context = context;
        }

        public async Task<int> CommitChangesAsync()
        {
            return await Context.SaveChangesAsync();
        }

        public virtual async Task<Tmodel> GetByIDAsync(object id)
        {
            return await Context.Set<Tmodel>().FindAsync(id);
        }

        public virtual async Task<IQueryable<Tmodel>> GetAsync(Expression<Func<Tmodel, bool>> where, string includeProperties = "")
        {
            return await Task.Run(()=> {
                var query = Context.Set<Tmodel>().AsQueryable();

                foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty);
                }

                if (where != null)
                    query = query.AsExpandable().Where(where);

                return query;
            });
        }

        public virtual async Task<IQueryable<Tmodel>> GetAsync(Expression<Func<Tmodel, bool>> @where, params Expression<Func<Tmodel, object>>[] include)
        {
            return await Task.Run(() =>
            {
                var query = Context.Set<Tmodel>().AsQueryable();

                foreach (var includeProperty in include)
                {
                    query = query.Include(includeProperty);
                }

                if (where != null)
                    query = query.AsExpandable().Where(where);

                return query;
            });
        }

        public virtual async Task<IQueryable<Tmodel>> GetAsync(params Expression<Func<Tmodel, object>>[] include)
        {
            return await Task.Run(() =>
            {
                var query = Context.Set<Tmodel>().AsQueryable().AsExpandable();

                foreach (var includeProperty in include)
                {
                    query = query.Include(includeProperty);
                }

                return query;
            });
        }

        public virtual async Task<IQueryable<Tmodel>> GetAllAsync()
        {
            return await Task.Run(() => Context.Set<Tmodel>().AsQueryable());
        }

        public virtual async Task<int> CountAsync()
        {
            return await Task.Run(()=> Context.Set<Tmodel>().Count());
        }

        public virtual async Task<Tmodel> InsertAsync(Tmodel entity)
        {
            await Context.Set<Tmodel>().AddAsync(entity);
            return entity;
        }

        public virtual async Task<IEnumerable<Tmodel>> InsertRangeAsync(IEnumerable<Tmodel> entity)
        {
            await Context.Set<Tmodel>().AddRangeAsync(entity);
            return entity;
        }

        public virtual async Task<Tmodel> UpdateAsync(Tmodel entity)
        {
            return await Task.Run(() =>
            {
                Context.Set<Tmodel>().Attach(entity);
                Context.Entry(entity).State = EntityState.Modified;

                return entity;
            });
        }

        public virtual async Task<Tmodel> UpdateAsync(Tmodel entity, object id)
        {
            var entry = Context.Entry(entity);

            if (entry.State == EntityState.Detached)
            {
                var attachedEntity = await GetByIDAsync(id);

                if (attachedEntity != null)
                {
                    var attachedEntry = Context.Entry(attachedEntity);
                    attachedEntry.CurrentValues.SetValues(entity);
                }
                else
                {
                    entry.State = EntityState.Modified;
                }
            }
            return entity;
        }

        public virtual async Task<IEnumerable<Tmodel>> UpdateRangeAsync(IEnumerable<Tmodel> entities){
                return await Task.Run(()=>{
                    var result = new List<Tmodel>();
                foreach(var entity in entities){
                    Context.Set<Tmodel>().Attach(entity);
                    Context.Entry(entity).State = EntityState.Modified;
                    result.Add(entity);
                }
                return result;
                });
        }

        public virtual async Task UpdatePropertyAsync<Type>(Expression<Func<Tmodel, Type>> property, Tmodel entity)
        {
            Context.Set<Tmodel>().Attach(entity);
            Context.Entry(entity).Property(property).IsModified = true;
            await Context.SaveChangesAsync();
        }

        public virtual async Task DeleteAsync(Tmodel entity)
        {
            await Task.Run(() => Context.Set<Tmodel>().Remove(entity));
        }

        public virtual async Task DeleteRangeAsync(IEnumerable<Tmodel> entity)
        {
            await Task.Run(()=> Context.Set<Tmodel>().RemoveRange(entity));
        }


        public virtual async Task DeleteAsync(object id)
        {
            var entity = await GetByIDAsync(id);
            await DeleteAsync(entity);
        }

        public virtual async Task DeleteAsync(Expression<Func<Tmodel, bool>> primaryKeys)
        {
            var entity = (await GetAsync(primaryKeys)).FirstOrDefault();
            await DeleteAsync(entity);
        }

        public virtual async Task<SqlDataReader> RunAsync(string query)
        {
            var connection = Context.Database.GetDbConnection();

            SqlConnection conn = new SqlConnection(connection.ConnectionString);

            using SqlCommand command = new SqlCommand(query, conn);
            await conn.OpenAsync();

            return await command.ExecuteReaderAsync();
        }
    }
}
