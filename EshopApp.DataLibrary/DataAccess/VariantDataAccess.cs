using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.VariantModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;

//TODO just add logging codes at some point
public class VariantDataAccess : IVariantDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<VariantDataAccess> _logger;

    VariantDataAccess(AppDataDbContext appDataDbContext, ILogger<VariantDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<VariantDataAccess>.Instance;
    }

    public async Task<ReturnVariantsAndCodeResponseModel> GetVariantsAsync(int amount)
    {
        try
        {
            List<Variant> variants = await _appDataDbContext.Variants
                .Include(v => v.Images)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Take(amount)
                .ToListAsync();

            return new ReturnVariantsAndCodeResponseModel(variants, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantsFailure"), ex, "An error occurred while retrieving the variants. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    /*public async Task<ReturnVariantsAndCodeResponseModel> GetVariantsOfProductAsync(string productId, int amount)
    {
        try
        {
            List<Variant> variants = await _appDataDbContext.Variants
                .Include(v => v.Images)
                .Include(v => v.Attributes)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Where(variant => variant.Product != null && variant.Product.Id == productId)
                .Take(amount)
                .ToListAsync();

            return new ReturnVariantsAndCodeResponseModel(variants, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            //log the exception here
            throw;
        }
    }
    */ //might not be needed

    public async Task<ReturnVariantAndCodeResponseModel> GetVariantByIdAsync(string id)
    {
        try
        {
            Variant? foundVariant = await _appDataDbContext.Variants
                .Include(v => v.Images)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variant => variant.Id == id);

            return new ReturnVariantAndCodeResponseModel(foundVariant!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantByIdFailure"), ex, "An error occurred while retrieving the variant with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantAndCodeResponseModel> GetVariantBySKUAsync(string sku)
    {
        try
        {
            Variant? foundVariant = await _appDataDbContext.Variants
                .Include(v => v.Images)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variant => variant.SKU == sku);
            return new ReturnVariantAndCodeResponseModel(foundVariant!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantBySkuFailure"), ex, "An error occurred while retrieving the variant with SKU={sku}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", sku, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantAndCodeResponseModel> CreateVariantAsync(Variant variant)
    {
        try
        {
            Product? foundProduct = await _appDataDbContext.Products.FirstOrDefaultAsync(product => product.Id == variant.ProductId);
            if (foundProduct is null)
            {
                _logger.LogWarning(new EventId(9999, "CreateVariantFailureDueToNullProduct"), "Tried to create variant for null product.");
                return new ReturnVariantAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidProductIdWasGiven);
            }

            variant.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Variants.FirstOrDefaultAsync(otherVariant => otherVariant.Id == variant.Id) is not null)
                variant.Id = Guid.NewGuid().ToString();

            DateTime dateTimeNow = DateTime.Now;
            variant.CreatedAt = dateTimeNow;
            variant.ModifiedAt = dateTimeNow;
            await _appDataDbContext.Variants.AddAsync(variant);

            await _appDataDbContext.SaveChangesAsync();
            _logger.LogInformation(new EventId(9999, "CreateVariantSuccess"), "The variant was successfully created.");
            return new ReturnVariantAndCodeResponseModel(variant, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateVariantFailure"), ex, "An error occurred while creating the variant. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateVariantAsync(Variant updatedVariant)
    {
        try
        {
            if (updatedVariant.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            Variant? foundVariant = await _appDataDbContext.Variants
                .Include(v => v.Images)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .FirstOrDefaultAsync();

            if (foundVariant is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateVariantFailureDueToNullVariant"), "The variant with Id={id} was not found and thus the update could not proceed.", updatedVariant.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            foundVariant.SKU = updatedVariant.SKU ?? foundVariant.SKU;
            foundVariant.Price = updatedVariant.Price;
            foundVariant.UnitsInStock = updatedVariant.UnitsInStock;

            if (updatedVariant.Discount is not null)
            {
                Discount? databaseDiscount = await _appDataDbContext.Discounts.FirstOrDefaultAsync(discount => discount.Id == updatedVariant.Discount.Id);
                if (databaseDiscount is not null)
                    foundVariant.Discount = updatedVariant.Discount;
            }
            else
                foundVariant.Discount = null;

            if (updatedVariant.Images != null && !updatedVariant.Images.Any())
            {
                foundVariant.Images.Clear();
            }
            else if (updatedVariant.Images != null)
            {
                List<string> updatedVariantImages = updatedVariant.Images.Select(variantImage => variantImage.Id!).ToList(); // just add them here, for filtering below
                List<VariantImage> filteredVariantImages = await _appDataDbContext.VariantImages
                    .Where(databaseVariantImage => updatedVariantImages.Contains(databaseVariantImage.Id!))
                    .ToListAsync();

                foundVariant.Images.Clear();
                foundVariant.Images.AddRange(filteredVariantImages);
            }

            if (updatedVariant.Attributes != null && !updatedVariant.Attributes.Any())
            {
                foundVariant.Attributes.Clear();
            }
            else if (updatedVariant.Attributes != null)
            {
                List<string> updatedVariantAttributes = updatedVariant.Attributes.Select(variantAttribute => variantAttribute.Id!).ToList(); // just add them here, for filtering below
                List<AppAttribute> filteredVariantAttributes = await _appDataDbContext.Attributes
                    .Where(databaseVariantAttribute => updatedVariantAttributes.Contains(databaseVariantAttribute.Id!))
                    .ToListAsync();

                foundVariant.Attributes.Clear();
                foundVariant.Attributes.AddRange(filteredVariantAttributes);
            }

            foundVariant.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateVariantSuccess"), "The variant with Id={id} was successfully updated.", updatedVariant.Id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateVariantFailure"), ex, "An error occurred while updating the variant. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    //TODO figure out how to add or remove discount from variant. Something a long those lines
    /*public async Task<DataLibraryReturnedCodes> RemoveDiscountFromVariantAsync(Variant updatedVariant)
    {
        try
        {
            if (updatedVariant.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            Variant? foundVariant = await _appDataDbContext.Variants.FirstOrDefaultAsync(variant => variant.Id == updatedVariant.Id);
            if (foundVariant is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateVariantFailureDueToNullVariant"), "The variant with Id={id} was not found and thus the update could not proceed.", updatedVariant.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            foundVariant.SKU = updatedVariant.SKU ?? foundVariant.SKU;
            foundVariant.Price = updatedVariant.Price;
            foundVariant.UnitsInStock = updatedVariant.UnitsInStock;
            foundVariant.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateVariantSuccess"), "The variant with Id={id} was successfully updated.", updatedVariant.Id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateVariantFailure"), ex, "An error occurred while updating the variant. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }*/

    public async Task<DataLibraryReturnedCodes> DeleteVariantAsync(string variantId)
    {
        try
        {
            Variant? foundVariant = await _appDataDbContext.Variants.FirstOrDefaultAsync(variant => variant.Id == variantId);
            if (foundVariant is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteVariantFailureDueToNullProduct"), "The variant with Id={id} was not found and thus the update could not proceed.", variantId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.Variants.Remove(foundVariant);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteVariantSuccess"), "The variant with Id={id} was successfully deleted.", variantId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteVariantFailure"), ex, "An error occurred while deleting the variant with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", variantId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantsImagesAndCodeResponseModel> GetVariantImagesAsync(int amount)
    {
        try
        {
            List<VariantImage> variantImages = await _appDataDbContext.VariantImages
                .Include(vI => vI.Variant)
                    .ThenInclude(v => v!.Product)
                        .ThenInclude(p => p!.Categories)
                .Include(vI => vI.Variant)
                    .ThenInclude(v => v!.Attributes)
                .Take(amount)
                .ToListAsync();

            return new ReturnVariantsImagesAndCodeResponseModel(variantImages, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantImagesFailure"), ex, "An error occurred while retrieving the variant images. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantAndCodeResponseModel> GetVariantImageByIdAsync(string id)
    {
        try
        {
            Variant? foundVariant = await _appDataDbContext.Variants
                .Include(v => v.Images)
                .Include(v => v.Attributes)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variantImage => variantImage.Id == id);
            return new ReturnVariantAndCodeResponseModel(foundVariant!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantImageByIdFailure"), ex, "An error occurred while retrieving the variant image with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantImageAndCodeResponseModel> CreateVariantImageAsync(VariantImage variantImage)
    {
        try
        {
            Variant? foundVariant = await _appDataDbContext.Variants.FirstOrDefaultAsync(variant => variant.Id == variantImage.VariantId);
            if (foundVariant is null)
            {
                _logger.LogWarning(new EventId(9999, "CreateVariantImageFailureDueToNullVariant"), "Tried to create variant image for null product.");
                return new ReturnVariantImageAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidVariantIdWasGiven);
            }

            variantImage.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.VariantImages.FirstOrDefaultAsync(otherVariantImage => otherVariantImage.Id == variantImage.Id) is not null)
                variantImage.Id = Guid.NewGuid().ToString();

            DateTime dateTimeNow = DateTime.Now;
            variantImage.CreatedAt = dateTimeNow;
            variantImage.ModifiedAt = dateTimeNow;
            await _appDataDbContext.VariantImages.AddAsync(variantImage);

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateVariantImageSuccess"), "The variant image was successfully created.");
            return new ReturnVariantImageAndCodeResponseModel(variantImage, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateVariantImageFailure"), ex, "An error occurred while creating the variant image. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateVariantImageAsync(VariantImage updatedVariantImage)
    {
        try
        {
            if (updatedVariantImage.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            VariantImage? foundVariantImage = await _appDataDbContext.VariantImages.FirstOrDefaultAsync(variantImage => variantImage.Id == updatedVariantImage.Id);
            if (foundVariantImage is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateVariantImageFailureDueToNullVariantImage"), "The variant image with Id={id} was not found and thus the update could not proceed.", updatedVariantImage.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            foundVariantImage.ImagePath = updatedVariantImage.ImagePath ?? foundVariantImage.ImagePath;
            foundVariantImage.IsThumbNail = updatedVariantImage.IsThumbNail;
            foundVariantImage.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogWarning(new EventId(9999, "UpdateVariantImageSuccess"), "The variant image with Id={id} was successfully updated.", updatedVariantImage.Id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateVariantImageFailure"), ex, "An error occurred while updating the variant image with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedVariantImage.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteVariantImageAsync(string variantImageId)
    {
        try
        {
            VariantImage? foundVariantImage = await _appDataDbContext.VariantImages.FirstOrDefaultAsync(variantImage => variantImage.Id == variantImageId);
            if (foundVariantImage is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteVariantImageFailureDueToNullVariantImage"), "The variant image with Id={id} was not found and thus the delete could not proceed.", variantImageId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.VariantImages.Remove(foundVariantImage);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteVariantImageSuccess"), "The variant image with Id={id} was successfully deleted.", variantImageId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateVariantImageFailure"), ex, "An error occurred while deleting the variant image with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", variantImageId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
