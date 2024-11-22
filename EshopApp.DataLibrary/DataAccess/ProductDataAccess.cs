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

    public ProductDataAccess(AppDataDbContext appDataDbContext, ILogger<ProductDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<ProductDataAccess>.Instance;
    }

    public async Task<ReturnProductsAndCodeResponseModel> GetProductsAsync(int amount, bool includeDeactivated)
    {
        try
        {
            List<Product> products;
            if (!includeDeactivated)
            {
                products = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                        .ThenInclude(variantImage => variantImage.Image)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Where(product => !product.IsDeactivated!.Value)
                    .Take(amount)
                .ToListAsync();
            }
            else
            {
                products = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                        .ThenInclude(variantImage => variantImage.Image)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Take(amount)
                .ToListAsync();
            }
            return new ReturnProductsAndCodeResponseModel(products, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductsFailure"), ex, "An error occurred while retrieving the products. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnProductsAndCodeResponseModel> GetProductsOfCategoryAsync(string categoryId, int amount, bool includeDeactivated)
    {
        try
        {
            List<Product> products;
            if (!includeDeactivated)
            {
                products = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Where(product => !product.IsDeactivated!.Value && product.Categories.Any(category => category.Id == categoryId)) //add the products that are part of the given category
                .Take(amount)
                .ToListAsync();
            }
            else
            {
                products = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .Where(product => product.Categories.Any(category => category.Id == categoryId)) //add the products that are part of the given category
                .Take(amount)
                .ToListAsync();
            }

            return new ReturnProductsAndCodeResponseModel(products, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetProductsOfCategoryFailure"), ex, "An error occurred while retrieving the products of category with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", categoryId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnProductAndCodeResponseModel> GetProductByIdAsync(string id, bool includeDeactivated)
    {
        try
        {
            Product? foundProduct;
            if (!includeDeactivated)
            {
                foundProduct = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                        .ThenInclude(variantImage => variantImage.Image)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .FirstOrDefaultAsync(product => product.Id == id && !product.IsDeactivated!.Value);
            }
            else
            {
                foundProduct = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.VariantImages)
                        .ThenInclude(variantImage => variantImage.Image)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Discount)
                .Include(p => p.Variants)
                    .ThenInclude(v => v.Attributes)
                .FirstOrDefaultAsync(product => product.Id == id);
            }

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
            //a product can not be created without having at least one variant
            if (product.Variants is null || !product.Variants.Any())
            {
                _logger.LogWarning(new EventId(9999, "CreateProductFailureBecauseVariantWasNotProvided"), "The product could not be created, because variant information was not provided.");
                return new ReturnProductAndCodeResponseModel(null!, DataLibraryReturnedCodes.NoVariantWasProvidedForProductCreation);
            }

            if (await _appDataDbContext.Products.AnyAsync(existingProduct => existingProduct.Code == product.Code))
                return new ReturnProductAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateEntityCode);

            if (await _appDataDbContext.Products.AnyAsync(existingProduct => existingProduct.Name == product.Name))
                return new ReturnProductAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateEntityName);

            product.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Products.FirstOrDefaultAsync(otherProduct => otherProduct.Id == product.Id) is not null)
                product.Id = Guid.NewGuid().ToString();

            product.Description = product.Description ?? "";
            product.IsDeactivated = product.IsDeactivated ?? false;
            product.ExistsInOrder = product.ExistsInOrder ?? false;

            DateTime dateTimeNow = DateTime.Now;
            product.CreatedAt = dateTimeNow;
            product.ModifiedAt = dateTimeNow;

            //retrieve the existing categories and add connections to the newly created product(that also filters)
            if (product.Categories != null && product.Categories.Any())
            {
                List<string> categoryIds = product.Categories.Select(c => c.Id!).ToList();
                List<Category> existingCategories = await _appDataDbContext.Categories
                    .Where(category => categoryIds.Contains(category.Id!))
                    .ToListAsync();

                product.Categories.Clear();
                foreach (var category in existingCategories)
                    product.Categories.Add(category);
            }

            bool noThumbnailVariantYet = true;
            foreach (var variant in product.Variants)
            {
                if (await _appDataDbContext.Variants.AnyAsync(existingVariant => existingVariant.SKU == variant.SKU))
                    return new ReturnProductAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateVariantSku);

                variant.Id = Guid.NewGuid().ToString();
                while (await _appDataDbContext.Variants.FirstOrDefaultAsync(otherVariant => otherVariant.Id == variant.Id) is not null)
                    variant.Id = Guid.NewGuid().ToString();

                variant.IsDeactivated = variant.IsDeactivated ?? false;
                variant.ExistsInOrder = variant.ExistsInOrder ?? false;
                variant.UnitsInStock = variant.UnitsInStock ?? 0;

                variant.CreatedAt = dateTimeNow;
                variant.ModifiedAt = dateTimeNow;
                if (variant.IsThumbnailVariant.HasValue && variant.IsThumbnailVariant.Value && noThumbnailVariantYet)
                    noThumbnailVariantYet = false;
                else if (!noThumbnailVariantYet) //if there is at least one thumbnailvariant set every other to false
                    variant.IsThumbnailVariant = false;

                //this filters that the attributes that are passed in are valid
                if (variant.Attributes is not null && variant.Attributes.Any())
                {
                    List<string?> attributeIds = variant.Attributes.Where(attr => attr.Id != null).Select(attr => attr.Id).ToList() ?? new List<string?>()!; //the where clause is a bit excessive, but who knows
                    List<AppAttribute> validAttributes = await _appDataDbContext.Attributes.Where(attr => attributeIds.Contains(attr.Id!)).ToListAsync();
                    variant.Attributes = validAttributes;
                }

                if (variant.VariantImages is not null && variant.VariantImages.Any())
                {
                    //this filters to find the images that are valid
                    List<string?> imagesIds = variant.VariantImages.Where(variantImage => variantImage.ImageId != null).Select(variantImage => variantImage.ImageId).ToList() ?? new List<string?>()!;
                    List<AppImage> validImages = await _appDataDbContext.Images.Where(attr => imagesIds.Contains(attr.Id!)).ToListAsync();
                    string variantImageThatShouldBeThumbnail = variant.VariantImages.FirstOrDefault(variantImage => variantImage.IsThumbNail)?.ImageId!;

                    variant.VariantImages.Clear();
                    foreach (AppImage validImage in validImages)
                    {
                        VariantImage variantImage = new VariantImage();
                        variantImage.Id = Guid.NewGuid().ToString();
                        while (await _appDataDbContext.VariantImages.FirstOrDefaultAsync(otherVariantImage => otherVariantImage.Id == variantImage.Id) is not null)
                            variantImage.Id = Guid.NewGuid().ToString();

                        variantImage.VariantId = variant.Id;
                        variantImage.ImageId = validImage.Id;
                        variantImage.IsThumbNail = variantImageThatShouldBeThumbnail is not null && variantImageThatShouldBeThumbnail == validImage.Id;
                        variant.VariantImages.Add(variantImage);
                    }
                }
            }
            //if no variant was found as thumbnail yet set the first variant as thumbnail
            if (noThumbnailVariantYet)
                product.Variants[0].IsThumbnailVariant = true;


            await _appDataDbContext.Products.AddAsync(product);

            await _appDataDbContext.SaveChangesAsync(); //this will necessarily also create at least one variant

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

            Product? foundProduct = await _appDataDbContext.Products
                .Include(p => p.Categories)
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(product => product.Id == updatedProduct.Id);

            if (foundProduct is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateProductFailureDueToNullProduct"), "The product with Id={id} was not found and thus the update could not proceed.", updatedProduct.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (updatedProduct.Code is not null)
            {
                if (await _appDataDbContext.Products.AnyAsync(existingProduct => existingProduct.Code == updatedProduct.Code && existingProduct.Id != updatedProduct.Id))
                    return DataLibraryReturnedCodes.DuplicateEntityCode;

                foundProduct.Code = updatedProduct.Code;
            }

            if (updatedProduct.Name is not null)
            {
                if (await _appDataDbContext.Products.AnyAsync(existingProduct => existingProduct.Name == updatedProduct.Name && existingProduct.Id != updatedProduct.Id))
                    return DataLibraryReturnedCodes.DuplicateEntityName;

                foundProduct.Name = updatedProduct.Name;
            }

            foundProduct.Description = updatedProduct.Description ?? foundProduct.Description;
            foundProduct.IsDeactivated = updatedProduct.IsDeactivated ?? foundProduct.IsDeactivated;
            foundProduct.ExistsInOrder = updatedProduct.ExistsInOrder ?? foundProduct.ExistsInOrder;

            if (updatedProduct.Categories != null && !updatedProduct.Categories.Any())
            {
                foundProduct.Categories.Clear();
            }
            else if (updatedProduct.Categories != null)
            {
                List<string> updatedProductCategoryNames = updatedProduct.Categories.Select(category => category.Id!).ToList(); // just add them here, for filtering below
                List<Category> filteredCategories = await _appDataDbContext.Categories
                    .Where(databaseCategory => updatedProductCategoryNames.Contains(databaseCategory.Id!))
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

            if (foundProduct.ExistsInOrder!.Value)
            {
                foundProduct.IsDeactivated = true;
                await _appDataDbContext.SaveChangesAsync();

                _logger.LogInformation(new EventId(9999, "DeleteProductSuccessButSetToDeactivated"), "The product with Id={id} exists in an order and thus can not be fully deleted until that order is deleted, " +
                    "but it was correctly deactivated.", productId);
                return DataLibraryReturnedCodes.NoErrorButNotFullyDeleted;
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
