using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.DiscountModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;

//TODO just add logging codes at some point
public class DiscountDataAccess : IDiscountDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<DiscountDataAccess> _logger;

    DiscountDataAccess(AppDataDbContext appDataDbContext, ILogger<DiscountDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<DiscountDataAccess>.Instance;
    }

    public async Task<ReturnDiscountsAndCodeResponseModel> GetDiscountsAsync(int amount)
    {
        try
        {
            List<Discount> discounts = await _appDataDbContext.Discounts
                .Include(d => d.Variants)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p!.Categories)
                .Include(d => d.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(d => d.Variants)
                    .ThenInclude(v => v.Attributes)
                .Take(amount)
                .ToListAsync();

            return new ReturnDiscountsAndCodeResponseModel(discounts, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetDiscountsFailure"), ex, "An error occurred while retrieving the discounts. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnDiscountAndCodeResponseModel> GetDiscountByIdAsync(string id)
    {
        try
        {
            Discount? foundDiscount = await _appDataDbContext.Discounts
                .Include(d => d.Variants)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p!.Categories)
                .Include(d => d.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Include(d => d.Variants)
                    .ThenInclude(v => v.Attributes)
                .FirstOrDefaultAsync(discount => discount.Id == id);
            return new ReturnDiscountAndCodeResponseModel(foundDiscount!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetDiscountByIdFailure"), ex, "An error occurred while retrieving the discount with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnDiscountAndCodeResponseModel> CreateDiscountAsync(Discount discount)
    {
        try
        {
            if (await _appDataDbContext.Discounts.AnyAsync(existingDiscount => existingDiscount.Name == discount.Name))
                return new ReturnDiscountAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateEntityName);

            discount.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Discounts.FirstOrDefaultAsync(otherDiscount => otherDiscount.Id == discount.Id) is not null)
                discount.Id = Guid.NewGuid().ToString();

            DateTime dateTimeNow = DateTime.Now;
            discount.CreatedAt = dateTimeNow;
            discount.ModifiedAt = dateTimeNow;
            await _appDataDbContext.Discounts.AddAsync(discount);

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateDiscountSuccess"), "The discount was successfully created.");
            return new ReturnDiscountAndCodeResponseModel(discount, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateDiscountFailure"), ex, "An error occurred while creating the discount. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateDiscountAsync(Discount updatedDiscount)
    {
        try
        {
            if (updatedDiscount.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            Discount? foundDiscount = await _appDataDbContext.Discounts.Include(discount => discount.Variants).FirstOrDefaultAsync(discount => discount.Id == updatedDiscount.Id);
            if (foundDiscount is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateDiscountFailureDueToNullDiscount"), "The discount with Id={id} was not found and thus the update could not proceed.", updatedDiscount.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (updatedDiscount.Name is not null)
            {
                if (await _appDataDbContext.Discounts.AnyAsync(existingDiscount => existingDiscount.Name == updatedDiscount.Name))
                    return DataLibraryReturnedCodes.DuplicateEntityName;
                foundDiscount.Name = updatedDiscount.Name;
            }

            foundDiscount.Percentage = updatedDiscount.Percentage;
            if (updatedDiscount.Variants != null && !updatedDiscount.Variants.Any())
            {
                foundDiscount.Variants.Clear();
            }
            else if (updatedDiscount.Variants != null)
            {
                List<string> updatedVariant = updatedDiscount.Variants.Select(variant => variant.Id!).ToList(); // just add them here, for filtering below
                List<Variant> filteredVariants = await _appDataDbContext.Variants
                    .Where(databaseVariant => updatedVariant.Contains(databaseVariant.Id!))
                    .ToListAsync();

                foundDiscount.Variants.Clear();
                foundDiscount.Variants.AddRange(filteredVariants);
            }

            foundDiscount.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateDiscountSuccess"), "The discount was successfully updated.");
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateDiscountFailure"), ex, "An error occurred while updating the discount with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedDiscount.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteDiscountAsync(string discountId)
    {
        try
        {
            Discount? foundDiscount = await _appDataDbContext.Discounts.FirstOrDefaultAsync(discount => discount.Id == discountId);
            if (foundDiscount is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteDiscountFailureDueToNullDiscount"), "The discount with Id={id} was not found and thus the deletion could not proceed.", discountId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.Discounts.Remove(foundDiscount);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteDiscountSuccess"), "The discount was successfully deleted with Id={id}.", discountId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteDiscountFailure"), ex, "An error occurred while deleting the discount with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", discountId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
