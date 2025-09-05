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

    public VariantDataAccess(AppDataDbContext appDataDbContext, ILogger<VariantDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<VariantDataAccess>.Instance;
    }

    public async Task<ReturnVariantsAndCodeResponseModel> GetVariantsAsync(int amount, bool includeDeactivated)
    {
        try
        {
            //because it might not be very clear, the variant can have many variant images with each variant filteredImage having only one filteredImage. The reason why it is clunky it is because IsThumbnail property needs to be in the bridge table
            List<Variant> variants;
            if (!includeDeactivated)
            {
                variants = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Where(variant => !variant.IsDeactivated!.Value)
                    .Take(amount)
                .ToListAsync();
            }
            else
            {
                variants = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Take(amount)
                .ToListAsync();
            }

            return new ReturnVariantsAndCodeResponseModel(variants, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantsFailure"), ex, "An error occurred while retrieving the variants. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantsAndCodeResponseModel> GetVariantByTheirSKUsAsync(List<string> skus, bool includeDeactivated)
    {
        try
        {
            //because it might not be very clear, the variant can have many variant images with each variant filteredImage having only one filteredImage. The reason why it is clunky it is because IsThumbnail property needs to be in the bridge table
            List<Variant> variants;
            if (!includeDeactivated)
            {
                variants = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Where(variant => !variant.IsDeactivated!.Value && skus.Contains(variant.SKU!))
                .ToListAsync();
            }
            else
            {
                variants = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Where(variant => skus.Contains(variant.SKU!))
                .ToListAsync();
            }

            return new ReturnVariantsAndCodeResponseModel(variants, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantByTheirSKUsAsync"), ex, "An error occurred while retrieving the variants. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }


    public async Task<ReturnVariantAndCodeResponseModel> GetVariantByIdAsync(string id, bool includeDeactivated)
    {
        try
        {
            Variant? foundVariant;
            if (!includeDeactivated)
            {
                foundVariant = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variant => variant.Id == id && !variant.IsDeactivated!.Value);
            }
            else
            {
                foundVariant = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variant => variant.Id == id);
            }

            return new ReturnVariantAndCodeResponseModel(foundVariant!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetVariantByIdFailure"), ex, "An error occurred while retrieving the variant with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnVariantAndCodeResponseModel> GetVariantBySKUAsync(string sku, bool includeDeactivated)
    {
        try
        {
            Variant? foundVariant;
            if (!includeDeactivated)
            {
                foundVariant = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variant => variant.SKU == sku && !variant.IsDeactivated!.Value);
            }
            else
            {
                foundVariant = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .FirstOrDefaultAsync(variant => variant.SKU == sku);
            }

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

            if (await _appDataDbContext.Variants.AnyAsync(existingVariant => existingVariant.SKU == variant.SKU))
                return new ReturnVariantAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateVariantSku);

            variant.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Variants.FirstOrDefaultAsync(otherVariant => otherVariant.Id == variant.Id) is not null)
                variant.Id = Guid.NewGuid().ToString();

            variant.IsDeactivated = variant.IsDeactivated ?? false;
            variant.ExistsInOrder = variant.ExistsInOrder ?? false;
            variant.UnitsInStock = variant.UnitsInStock ?? 0;

            DateTime dateTimeNow = DateTime.Now;
            variant.CreatedAt = dateTimeNow;
            variant.ModifiedAt = dateTimeNow;

            //this if statement exists only to save dbms resources(it is necessary logically, but it could have been added in a more readable way if not for dbms resources)
            if (variant.IsThumbnailVariant.HasValue && variant.IsThumbnailVariant.Value)
            {
                //this is done to change the other variant that is thumbnail to false
                Variant? existingVariantThatIsThumbnailVariant = await _appDataDbContext.Variants.FirstOrDefaultAsync(otherVariant => otherVariant.ProductId == variant.ProductId && otherVariant.IsThumbnailVariant!.Value);
                if (existingVariantThatIsThumbnailVariant is not null)
                    existingVariantThatIsThumbnailVariant.IsThumbnailVariant = false;
            }
            //this field can not be null in the database and thus we need to set it manually
            else if (!variant.IsThumbnailVariant.HasValue)
                variant.IsThumbnailVariant = false;

            if (variant.DiscountId == "")
                variant.DiscountId = null;
            //check that the variant.discountId exists and if does not exist then remove the false connection
            else if (variant.DiscountId != null)
            {
                var foundDiscount = await _appDataDbContext.Discounts.FirstOrDefaultAsync(existingDiscount => existingDiscount.Id == variant.DiscountId);
                if (foundDiscount is null)
                    variant.DiscountId = null;
                else
                    variant.Discount = foundDiscount;
            }

            int variantImageThatIsThumbnailIndex = variant.VariantImages.FindIndex(img => img.IsThumbNail); //find the first variant image that does not have IsThumbnail = false
            for (int i = 0; i < variant.VariantImages.Count; i++)
            {
                VariantImage variantImage = variant.VariantImages[i];
                variantImage.Id = Guid.NewGuid().ToString();
                while (await _appDataDbContext.VariantImages.AnyAsync(img => img.Id == variantImage.Id))
                    variantImage.Id = Guid.NewGuid().ToString();

                variantImage.VariantId = variant.Id;
                if (variantImageThatIsThumbnailIndex == -1 && i == 0)
                    variantImage.IsThumbNail = true;
                else if (variantImageThatIsThumbnailIndex != -1)
                    variantImage.IsThumbNail = i == variantImageThatIsThumbnailIndex; //set everything to false manually(other than the correct image), because there might be other images with isthumbnail = true
            }

            //this filters that the attributes that are passed in are valid
            if (variant.Attributes is not null && variant.Attributes.Any())
            {
                List<string?> attributeIds = variant.Attributes.Where(attr => attr.Id != null).Select(attr => attr.Id).ToList() ?? new List<string?>()!; //the where clause is a bit excessive, but who knows
                List<AppAttribute> validAttributes = await _appDataDbContext.Attributes.Where(attr => attributeIds.Contains(attr.Id!)).ToListAsync();
                variant.Attributes = validAttributes;
            }

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
                .Include(v => v.VariantImages)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .FirstOrDefaultAsync(variant => variant.Id == updatedVariant.Id);

            if (foundVariant is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateVariantFailureDueToNullVariant"), "The variant with Id={id} was not found and thus the update could not proceed.", updatedVariant.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            //for null sku do nothing
            if (updatedVariant.SKU is not null)
            {
                if (await _appDataDbContext.Variants.AnyAsync(existingVariant => existingVariant.SKU == updatedVariant.SKU && existingVariant.Id != updatedVariant.Id))
                    return DataLibraryReturnedCodes.DuplicateVariantSku;

                foundVariant.SKU = updatedVariant.SKU;
            }

            //for null values do nothing
            foundVariant.Price = updatedVariant.Price ?? foundVariant.Price;
            foundVariant.UnitsInStock = updatedVariant.UnitsInStock ?? foundVariant.UnitsInStock;
            foundVariant.IsDeactivated = updatedVariant.IsDeactivated ?? foundVariant.IsDeactivated;
            foundVariant.ExistsInOrder = updatedVariant.ExistsInOrder ?? foundVariant.ExistsInOrder;

            //if null do nothing otherwise if else
            if (updatedVariant.IsThumbnailVariant.HasValue && !updatedVariant.IsThumbnailVariant.Value)
                foundVariant.IsThumbnailVariant = false;
            //if there is a potential change in the isthumbnail only then do this, because it takes database resources
            else if (updatedVariant.IsThumbnailVariant.HasValue && updatedVariant.IsThumbnailVariant.Value)
            {
                //check if there is another variant that is thumbnail
                Variant? otherVariant = await _appDataDbContext.Variants.FirstOrDefaultAsync(variant => variant.IsThumbnailVariant!.Value && variant.ProductId == foundVariant.ProductId);

                //if there is not just set the variant to what the user wants
                if (otherVariant is null)
                    foundVariant.IsThumbnailVariant = updatedVariant.IsThumbnailVariant;
                else
                {
                    //order matters, because the otherVariant and the foundVariant could be the same
                    otherVariant.IsThumbnailVariant = false;
                    foundVariant.IsThumbnailVariant = true;
                }
            }

            //this checks only based on the navigation property discountId and ignores the discount property if it is set.
            //Maybe not the best approach and I should check both? Well it works for now...
            if (updatedVariant.DiscountId is not null && updatedVariant.DiscountId != "")
            {
                //this checks if the discountId exists in the database
                Discount? databaseDiscount = await _appDataDbContext.Discounts.FirstOrDefaultAsync(discount => discount.Id == updatedVariant.DiscountId);
                foundVariant.DiscountId = databaseDiscount is not null ? databaseDiscount.Id : foundVariant.DiscountId;
            }
            else if (updatedVariant.DiscountId == "")
            {
                foundVariant.Discount = null;
                foundVariant.DiscountId = null;
            }

            //in case of null do nothing otherwise
            //check if the variantImages is an empty list
            if (updatedVariant.VariantImages != null && !updatedVariant.VariantImages.Any())
            {
                _appDataDbContext.VariantImages.RemoveRange(foundVariant.VariantImages);
                foundVariant.VariantImages.Clear();
            }
            else if (updatedVariant.VariantImages != null)
            {
                // here we filter that the images exist in the system. In the beginning in general only the imageId is contained in the variantImage
                List<string> updatedImagesIds = updatedVariant.VariantImages.Select(variantImage => variantImage.ImageId!).ToList();
                VariantImage? newThumbnailImage = updatedVariant.VariantImages.FirstOrDefault(variantImage => variantImage.IsThumbNail);
                List<AppImage> filteredImages = await _appDataDbContext.Images
                    .Where(databaseImage => updatedImagesIds.Contains(databaseImage.Id!))
                    .ToListAsync();

                _appDataDbContext.VariantImages.RemoveRange(foundVariant.VariantImages);
                foundVariant.VariantImages.Clear(); //remove all the variantImages
                //and then rebuild them
                foreach (var filteredImage in filteredImages)
                {
                    VariantImage variantImage = new VariantImage();
                    variantImage.Id = Guid.NewGuid().ToString();
                    while (await _appDataDbContext.Variants.FirstOrDefaultAsync(otherVariant => otherVariant.Id == variantImage.Id) is not null)
                        variantImage.Id = Guid.NewGuid().ToString();
                    variantImage.VariantId = updatedVariant.Id; //updatedvariant.id and foundvariant.id at that point must be the same
                    variantImage.ImageId = filteredImage.Id;
                    variantImage.IsThumbNail = newThumbnailImage?.ImageId == filteredImage.Id || (newThumbnailImage == null && foundVariant.VariantImages.Count == 0); //if the first condition fails then the condition after the or triggers

                    foundVariant.VariantImages.Add(variantImage);
                }
            }

            //in case of null do nothing otherwise
            //check if the attributes is an empty list
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

            if (foundVariant.ExistsInOrder!.Value)
            {
                //the if statement here is only for performance, because potentionally a pointless database update is prevented
                if (!foundVariant.IsDeactivated!.Value)
                {
                    foundVariant.IsDeactivated = true;
                    await _appDataDbContext.SaveChangesAsync();
                }

                _logger.LogInformation(new EventId(9999, "DeleteProductSuccessButSetToDeactivated"), "The variant with Id={id} exists in an order and thus can not be fully deleted until that order is deleted, " +
                    "but it was correctly deactivated.", variantId);
                return DataLibraryReturnedCodes.NoErrorButNotFullyDeleted;
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
}
