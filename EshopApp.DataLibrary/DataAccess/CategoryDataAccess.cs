using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CategoryModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;

//TODO just add logging codes at some point
public class CategoryDataAccess : ICategoryDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<CategoryDataAccess> _logger;

    CategoryDataAccess(AppDataDbContext appDataDbContext, ILogger<CategoryDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<CategoryDataAccess>.Instance;
    }

    public async Task<ReturnCategoriesAndCodeResponseModel> GetCategoriesAsync(int amount)
    {
        try
        {
            List<Category> categories = await _appDataDbContext.Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Images)
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Discount)
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Attributes)
                .Take(amount)
                .ToListAsync();

            return new ReturnCategoriesAndCodeResponseModel(categories, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCategoriesFailure"), ex, "An error occurred while retrieving the categories. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCategoryAndCodeResponseModel> GetCategoryByIdAsync(string id)
    {
        try
        {
            Category? foundCategory = await _appDataDbContext.Categories
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Images)
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Discount)
                .Include(c => c.Products)
                    .ThenInclude(p => p.Variants)
                        .ThenInclude(v => v.Attributes)
                .FirstOrDefaultAsync(category => category.Id == id);
            return new ReturnCategoryAndCodeResponseModel(foundCategory!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCategoryByIdFailure"), ex, "An error occurred while retrieving the category with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCategoryAndCodeResponseModel> CreateCategoryAsync(Category category)
    {
        try
        {
            category.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Categories.FirstOrDefaultAsync(otherCategory => otherCategory.Id == category.Id) is not null)
                category.Id = Guid.NewGuid().ToString();

            DateTime dateTimeNow = DateTime.Now;
            category.CreatedAt = dateTimeNow;
            category.ModifiedAt = dateTimeNow;
            await _appDataDbContext.Categories.AddAsync(category);

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateCategorySuccess"), "The category was successfully created.");
            return new ReturnCategoryAndCodeResponseModel(category, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateCategoryFailure"), ex, "An error occurred while creating category. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateCategoryAsync(Category updatedCategory)
    {
        try
        {
            if (updatedCategory.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            Category? foundCategory = await _appDataDbContext.Categories.Include(category => category.Products).FirstOrDefaultAsync(category => category.Id == updatedCategory.Id);
            if (foundCategory is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateCategoryFailureDueToNullCategory"), "The category with Id={id} was not found and thus the update could not proceed.", updatedCategory.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            foundCategory.Name = updatedCategory.Name;
            if (updatedCategory.Products != null && !updatedCategory.Products.Any())
            {
                foundCategory.Products.Clear();
            }
            else if (updatedCategory.Products != null)
            {
                List<string> updatedProducts = updatedCategory.Products.Select(product => product.Id!).ToList(); // just add them here, for filtering below
                List<Product> filteredProducts = await _appDataDbContext.Products
                    .Where(databaseProduct => updatedProducts.Contains(databaseProduct.Id!))
                    .ToListAsync();

                foundCategory.Products.Clear();
                foundCategory.Products.AddRange(filteredProducts);
            }


            foundCategory.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateCategorySuccess"), "The category with Id={id} was successfully updated.", updatedCategory.Id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateCategoryFailure"), ex, "An error occurred while updating the category with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedCategory.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteCategoryAsync(string categoryId)
    {
        try
        {
            Category? foundCategory = await _appDataDbContext.Categories.FirstOrDefaultAsync(category => category.Id == categoryId);
            if (foundCategory is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteCategoryFailureDueToNullCategory"), "The category with Id={id} was not found and thus the deletion could not proceed.", categoryId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.Categories.Remove(foundCategory);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteCategorySuccess"), "The category with Id={id} was successfully deleted.", categoryId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteCategoryFailure"), ex, "An error occurred while deleting the category with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", categoryId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
