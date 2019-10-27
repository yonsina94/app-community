using Community.Backend.Database.Repositories.Constructor;
using Community.Backend.Services.Base;
using Community.Backend.Services.Infraestructure;
using Comunity.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Community.Backend.Services
{
    public interface ICategoryService : IBaseService<Category>
    {

    }

    public class CategoryService : BaseService<Category>, ICategoryService
    {
        public CategoryService(IRepositoryConstructor constructor) : base(constructor)
        {
        }

        public override async Task<Result> ValidateOnCreateAsync(Category entity)
        {
            var result = await Repository.GetAsync(c => c.Name == entity.Name || c.ShortName == entity.ShortName);
            if (result.Any())
            {
                return Result.AddErrorMessage("Data exist in database");
            }
            else
            {
                return Result;
            }
        }

        public async override Task<Result> ValidateOnDeleteAsync(Category entity)
        {
            var result = await Repository.GetByIDAsync(entity.ID);
            if (result == null)
            {
                return Result.AddErrorMessage("this category not exist in the database");
            }
            else
            {
                return Result;
            }
        }

        public async override Task<Result> ValidateOnDeleteAsync(object id)
        {
            var result = await Repository.GetByIDAsync(id);
            if (result == null)
            {
                return Result.AddErrorMessage("this category not exist in the database");
            }
            else
            {
                return Result;
            }
        }

        public async override Task<Result> ValidateOnUpdateAsync(Category entity)
        {
            var result = await Repository.GetByIDAsync(entity.ID);
            if (result == null)
            {
                return Result.AddErrorMessage("this category not exist in the database");
            }
            else
            {
                return Result;
            }
        }
    }
}
