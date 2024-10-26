﻿using EshopApp.DataLibrary.Models;
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
            //because it might not be very clear, the variant can have many variant images with each variant image having only one image. The reason why it is clunky it is because IsThumbnail property needs to be in the bridge table
            List<Variant> variants = await _appDataDbContext.Variants
                .Include(v => v.VariantImages)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Categories)
                .Include(v => v.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
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
                .Include(v => v.VariantImages)
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
                .Include(v => v.VariantImages)
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
                .Include(v => v.VariantImages)
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

            if (await _appDataDbContext.Variants.AnyAsync(existingVariant => existingVariant.SKU == variant.Id))
                return new ReturnVariantAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateVariantSku);

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
                .Include(v => v.VariantImages)
                .Include(v => v.Attributes)
                .Include(v => v.Discount)
                .FirstOrDefaultAsync();

            if (foundVariant is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateVariantFailureDueToNullVariant"), "The variant with Id={id} was not found and thus the update could not proceed.", updatedVariant.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (updatedVariant.SKU is not null)
            {
                if (await _appDataDbContext.Variants.AnyAsync(existingVariant => existingVariant.SKU == updatedVariant.Id))
                    return DataLibraryReturnedCodes.DuplicateVariantSku;

                foundVariant.SKU = updatedVariant.SKU;
            }

            foundVariant.Price = updatedVariant.Price;
            foundVariant.UnitsInStock = updatedVariant.UnitsInStock;
            foundVariant.IsThumbnailVariant = updatedVariant.IsThumbnailVariant;

            if (updatedVariant.Discount is not null)
            {
                Discount? databaseDiscount = await _appDataDbContext.Discounts.FirstOrDefaultAsync(discount => discount.Id == updatedVariant.Discount.Id);
                if (databaseDiscount is not null)
                    foundVariant.Discount = updatedVariant.Discount;
            }
            else
                foundVariant.Discount = null;

            if (updatedVariant.VariantImages != null && !updatedVariant.VariantImages.Any())
            {
                foundVariant.VariantImages.Clear();
            }
            //here this is a bit complicated, but the idea is that variantImage bridge entity has an id that always remains the same,
            //but theoretically either a new connection was added or previous connections where removed. So because the variantImage can not exist
            //before this moment(in the case of the new connection) the typical filtering can not take place
            else if (updatedVariant.VariantImages != null)
            {
                //TODO what if the images do not exist? That assumes that the connection is done when the images exist, but in a lot of cases that is not the case... Think about it

                //List<string> updatedVariantImages = updatedVariant.VariantImages.Select(variantImage => variantImage.Id!).ToList(); // just add them here, for filtering below
                /*List<VariantImage> filteredVariantImages = await _appDataDbContext.VariantImages
                    .Where(databaseVariantImage => updatedVariantImages.Contains(databaseVariantImage.Id!))
                    .ToListAsync();
                */
                foundVariant.VariantImages.Clear();
                foundVariant.VariantImages.AddRange(updatedVariant.VariantImages);
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
}