using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ProductModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;

//TODO just add logging codes at some point
public class ProductDataAccess : IProductDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<ProductDataAccess> _logger;

    ProductDataAccess(AppDataDbContext appDataDbContext, ILogger<ProductDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<ProductDataAccess>.Instance;
    }

    public async Task<ReturnProductsAndCodeResponseModel> GetProductsAsync(int amount)
    {
        try
        {
            List<Product> products = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Take(amount)
                .ToListAsync();
            return new ReturnProductsAndCodeResponseModel(products, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductsFailure"), ex, "An error occurred while retrieving the products. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnProductsAndCodeResponseModel> GetProductsOfCategoryAsync(string categoryId, int amount)
    {
        try
        {

            List<Product> products = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Where(product => product.Categories.Any(category => category.Id == categoryId)) //add the products that are part of the given category
                .Take(amount)
                .ToListAsync();

            return new ReturnProductsAndCodeResponseModel(products, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductsOfCategoryFailure"), ex, "An error occurred while retrieving the products of category with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", categoryId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnProductAndCodeResponseModel> GetProductByIdAsync(string id)
    {
        try
        {
            Product? foundProduct = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Images)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .FirstOrDefaultAsync(product => product.Id == id);
            return new ReturnProductAndCodeResponseModel(foundProduct!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductByIdFailure"), ex, "An error occurred while retrieving the product with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnProductAndCodeResponseModel> CreateProductAsync(Product product)
    {
        try
        {
            product.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Products.FirstOrDefaultAsync(otherProduct => otherProduct.Id == product.Id) is not null)
                product.Id = Guid.NewGuid().ToString();

            DateTime dateTimeNow = DateTime.Now;
            product.CreatedAt = dateTimeNow;
            product.ModifiedAt = dateTimeNow;
            await _appDataDbContext.Products.AddAsync(product);

            await _appDataDbContext.SaveChangesAsync();

            //TODO does this also create a variant? Good question 
            _logger.LogInformation(new EventId(9999, "CreateProductSuccess"), "The product was successfully created.");
            return new ReturnProductAndCodeResponseModel(product, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateProductFailure"), ex, "An error occurred while creating the product. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    //will be used as a general way to update product in my initial conception of the UI
    public async Task<DataLibraryReturnedCodes> UpdateProductAsync(Product updatedProduct)
    {
        try
        {
            if (updatedProduct.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            Product? foundProduct = await _appDataDbContext.Products.FirstOrDefaultAsync(product => product.Id == updatedProduct.Id);
            if (foundProduct is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateProductFailureDueToNullProduct"), "The product with Id={id} was not found and thus the update could not proceed.", updatedProduct.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            foundProduct.Code = updatedProduct.Code ?? foundProduct.Code;
            foundProduct.Name = updatedProduct.Name ?? foundProduct.Name;
            foundProduct.Description = updatedProduct.Description;

            if (updatedProduct.Categories != null && !updatedProduct.Categories.Any())
            {
                foundProduct.Categories.Clear();
            }
            else if (updatedProduct.Categories != null)
            {
                List<string> updatedProductCategoryNames = updatedProduct.Categories.Select(category => category.Name!).ToList(); // just add them here, for filtering below
                List<Category> filteredCategories = await _appDataDbContext.Categories
                    .Where(databaseCategory => updatedProductCategoryNames.Contains(databaseCategory.Name!))
                    .ToListAsync();

                foundProduct.Categories.Clear();
                foundProduct.Categories.AddRange(filteredCategories);
            }

            if (updatedProduct.Variants != null && !updatedProduct.Variants.Any())
            {
                foundProduct.Variants.Clear();
            }
            else if (updatedProduct.Variants != null)
            {
                List<string> updatedProductVariants = updatedProduct.Variants.Select(variant => variant.Id!).ToList(); // just add them here, for filtering below
                List<Variant> filteredVariants = await _appDataDbContext.Variants
                    .Where(databaseVariant => updatedProductVariants.Contains(databaseVariant.Id!))
                    .ToListAsync();

                foundProduct.Variants.Clear();
                foundProduct.Variants.AddRange(filteredVariants);
            }

            foundProduct.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateProductSuccess"), "The product with Id={id} was successfully updated.", updatedProduct.Id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateProductFailure"), ex, "An error occurred while updating the product with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedProduct.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    //this might be optional
    /*public async Task<DataLibraryReturnedCodes> UpdateProductCategories(string id, List<string> categoryNames)
    {
        try
        {
            Product? foundProduct = await _appDataDbContext.Products.FirstOrDefaultAsync(product => product.Id == id);
            if (foundProduct is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateProductCategoriesFailureDueToNullProduct"), "The product with Id={id} was not found and thus the update could not proceed.", id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            List<Category> filteredCategories = await _appDataDbContext.Categories
                .Where(category => categoryNames.Contains(category.Name!))
                .ToListAsync();

            foundProduct.Categories.Clear();
            filteredCategories.ForEach(filteredCategory => foundProduct.Categories.Add(filteredCategory));

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateProductCategoriesSuccess"), "The product's categories with Id={id} were successfully updated.", id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateProductCategoriesFailure"), ex, "An error occurred while updating the categories of the product with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }
    */

    public async Task<DataLibraryReturnedCodes> DeleteProductAsync(string productId)
    {
        try
        {
            Product? foundProduct = await _appDataDbContext.Products.FirstOrDefaultAsync(product => product.Id == productId);
            if (foundProduct is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteProductFailureDueToNullProduct"), "The product with Id={id} was not found and thus the delete could not proceed.", productId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.Products.Remove(foundProduct);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteProductSuccess"), "The product with Id={id} was successfully deleted.", productId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteProductFailure"), ex, "An error occurred while deleting the product with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", productId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
