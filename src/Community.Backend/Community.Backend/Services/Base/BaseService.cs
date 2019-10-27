using Community.Backend.Database.Repositories.Base;
using Community.Backend.Database.Repositories.Constructor;
using Community.Backend.Services.Infraestructure;
using Comunity.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Community.Backend.Services.Base
{
    public interface IBaseService<Tmodel> where Tmodel : class, IBaseModel
    {
        Task<Result> ValidateOnCreateAsync(Tmodel entity);
        Task<Result> ValidateOnUpdateAsync(Tmodel entity);
        Task<Result> ValidateOnDeleteAsync(Tmodel entity);
        Task<Result> ValidateOnDeleteAsync(object id);

        Task<List<Tmodel>> GetAllAsync();
        Task<Tmodel> GetByIDAsync(object id);
        Task<Result> UpdateAsync(Tmodel entity);
        Task<Result> UpdateRangeAsync(IEnumerable<Tmodel> entities);
        Task<Result> CreateAsync(Tmodel entity);
        Task<Result> CreateRangeAsync(IEnumerable<Tmodel> entities);
        Task<Result> DeleteAsync(object id);
        Task<Result> DeleteAsync(Tmodel entity);
        Task<Result> DeleteRangeAsync(IEnumerable<Tmodel> entities);

        Task<TResult> AdvanceQueryAsync<TResult>(TResult result, Func<IRepositoryConstructor, Task<TResult>> operation) where TResult : class;
    }
    public abstract class BaseService<Tmodel> : IBaseService<Tmodel> where Tmodel : class, IBaseModel
    {
        protected Result Result { get; set; }
        protected readonly IRepositoryConstructor Constructor;
        protected IBaseRepository<Tmodel> Repository { get => Constructor.GetRepository<Tmodel>(); }

        public BaseService(IRepositoryConstructor constructor)
        {
            Result = new Result();
            Constructor = constructor;
        }

        public virtual async Task<TResult> AdvanceQueryAsync<TResult>(TResult result, Func<IRepositoryConstructor, Task<TResult>> operation) where TResult : class => await operation(Constructor);

        public virtual async Task<Tmodel> GetByIDAsync(object id)
        {
            return await Repository.GetByIDAsync(id);
        }

        public virtual async Task<List<Tmodel>> GetAllAsync()
        {
            return (await Repository.GetAllAsync()).ToList();
        }

        public virtual async Task<Result> CreateAsync(Tmodel entity)
        {
            try
            {
                if ((await ValidateOnCreateAsync(entity)).ExecutedSuccesfully)
                {
                    await Repository.InsertAsync(entity);
                    await Repository.CommitChangesAsync();
                    return Result.AddMessage($"{typeof(Tmodel).Name} inserted successfully !");
                }
                else
                {
                    return Result.AddErrorMessage("Fail ");
                }
            }
            catch (Exception ex)
            {
                Result.AddErrorMessage("", ex);
                return Result;
            }
        }

        public virtual async Task<Result> CreateRangeAsync(IEnumerable<Tmodel> entities)
        {
            var errorFounds = 0;
            try
            {
                foreach (var model in entities)
                {
                    if ((await ValidateOnCreateAsync(model)).ExecutedSuccesfully)
                    {
                        errorFounds++;
                    }
                }

                if (errorFounds > 0)
                {
                    return Result.AddErrorMessage($"Error in data for insert: {errorFounds}");
                }
                else
                {
                    await Repository.InsertRangeAsync(entities);
                    await Repository.CommitChangesAsync();
                    return Result.AddMessage("models save successfully");
                }
            }
            catch (Exception ex)
            {
                return Result.AddErrorMessage("Error creating entities in DB", ex);
            }
        }

        public virtual async Task<Result> DeleteAsync(object id)
        {
            try
            {
                var entity = await Repository.GetByIDAsync(id);
                if (entity != null)
                {
                    var result = await ValidateOnDeleteAsync(entity);
                    if (result.ExecutedSuccesfully)
                    {
                        await Repository.DeleteAsync(id);
                        await Repository.CommitChangesAsync();
                        Result = Result.AddMessage("entity delete successfully");
                    }
                    else
                    {
                        Result = Result.AddErrorMessage("").AppendTaskResultData(result);

                    }
                }
                return Result;
            }
            catch (Exception ex)
            {
                return Result.AddErrorMessage("Error deleting this entity from DB", ex);
            }
        }

        public virtual async Task<Result> DeleteAsync(Tmodel entity)
        {
            try
            {
                var result = await ValidateOnDeleteAsync(entity);
                if (result.ExecutedSuccesfully)
                {
                    await Repository.DeleteAsync(entity);
                    await Repository.CommitChangesAsync();
                    return Result.AddMessage("entity delete successfully");
                }
                else
                {
                    return Result.AddErrorMessage("").AppendTaskResultData(result);
                }
            }
            catch (Exception ex)
            {
                return Result.AddErrorMessage("Error deleting this entity from DB", ex);
            }
        }

        public virtual async Task<Result> DeleteRangeAsync(IEnumerable<Tmodel> entities)
        {
            try
            {
                var result = new Result();
                foreach (var entity in entities)
                {
                    result = await ValidateOnDeleteAsync(entity);
                    if (!result.ExecutedSuccesfully)
                    {
                        break;
                    }
                }
                if (result.ExecutedSuccesfully)
                {
                    await Repository.DeleteRangeAsync(entities);
                    await Repository.CommitChangesAsync();
                    return Result.AddMessage("Entittes deleted successfully !");
                }
                else
                {
                    return Result.AddErrorMessage("").AppendTaskResultData(result);
                }
            }
            catch (Exception ex)
            {
                return Result.AddErrorMessage("Error on delete the list of entities", ex);
            }
        }

        public virtual async Task<Result> UpdateAsync(Tmodel entity)
        {
            try
            {
                var result = await ValidateOnUpdateAsync(entity);
                if (result.ExecutedSuccesfully)
                {
                    await Repository.UpdateAsync(entity);
                    await Repository.CommitChangesAsync();
                    Result = Result.AddMessage($"{entity.GetType().Name} entity update successfully !");
                }
                else
                {
                    Result = Result.AddErrorMessage($"").AppendTaskResultData(result);
                }
                return Result;
            }
            catch (Exception ex)
            {
                return Result.AddErrorMessage("", ex);
            }
        }

        public virtual async Task<Result> UpdateRangeAsync(IEnumerable<Tmodel> entities)
        {
            try
            {
                var result = new Result();
                foreach (var entity in entities)
                {
                    result = await ValidateOnUpdateAsync(entity);
                    if (!result.ExecutedSuccesfully)
                    {
                        break;
                    }
                }
                if (!result.ExecutedSuccesfully)
                {
                    Result = Result.AddErrorMessage($"error when try to eliminate {typeof(Tmodel).Name} entity list").AppendTaskResultData(result);
                }
                else
                {
                    await Repository.UpdateRangeAsync(entities);
                    await Repository.CommitChangesAsync();
                    Result = Result.AddMessage($"list of {typeof(Tmodel).Name} entities update succefully !");
                }
                return Result;
            }
            catch (Exception ex)
            {
                return Result.AddErrorMessage($"error when try to update the list of {typeof(Tmodel)} entities in database", ex);
            }
        }

        public abstract Task<Result> ValidateOnCreateAsync(Tmodel entity);

        public abstract Task<Result> ValidateOnDeleteAsync(Tmodel entity);

        public abstract Task<Result> ValidateOnUpdateAsync(Tmodel entity);

        public abstract Task<Result> ValidateOnDeleteAsync(object id);
    }
}
