using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.PaymentOptionModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;
public class PaymentOptionDataAccess : IPaymentOptionDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<PaymentOptionDataAccess> _logger;

    public PaymentOptionDataAccess(AppDataDbContext appDataDbContext, ILogger<PaymentOptionDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<PaymentOptionDataAccess>.Instance;
    }

    public async Task<ReturnPaymentOptionsAndCodeResponseModel> GetPaymentOptionsAsync(int amount, bool includeDeactivated)
    {
        try
        {
            List<PaymentOption> paymentOptions;
            if (!includeDeactivated)
            {
                paymentOptions = await _appDataDbContext.PaymentOptions
                    .Include(po => po.PaymentDetails)
                        .ThenInclude(pd => pd.Order)
                            .ThenInclude(o => o!.OrderItems)
                    .Where(paymentOption => !paymentOption.IsDeactivated!.Value)
                    .Take(amount)
                    .ToListAsync();
            }
            else
            {
                paymentOptions = await _appDataDbContext.PaymentOptions
                    .Include(po => po.PaymentDetails)
                        .ThenInclude(pd => pd.Order)
                            .ThenInclude(o => o!.OrderItems)
                    .Take(amount)
                    .ToListAsync();
            }


            return new ReturnPaymentOptionsAndCodeResponseModel(paymentOptions, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetPaymentOptionsFailure"), ex, "An error occurred while retrieving the payment options. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnPaymentOptionAndCodeResponseModel> GetPaymentOptionByIdAsync(string id, bool includeDeactivated)
    {
        try
        {
            PaymentOption? foundPaymentOption;

            if (!includeDeactivated)
            {
                foundPaymentOption = await _appDataDbContext.PaymentOptions
                    .Include(po => po.PaymentDetails)
                        .ThenInclude(pd => pd.Order)
                            .ThenInclude(o => o!.OrderItems)
                    .FirstOrDefaultAsync(paymentOption => paymentOption.Id == id && !paymentOption.IsDeactivated!.Value);
            }
            else
            {
                foundPaymentOption = await _appDataDbContext.PaymentOptions
                    .Include(po => po.PaymentDetails)
                        .ThenInclude(pd => pd.Order)
                            .ThenInclude(o => o!.OrderItems)
                    .FirstOrDefaultAsync(paymentOption => paymentOption.Id == id);
            }

            return new ReturnPaymentOptionAndCodeResponseModel(foundPaymentOption!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetPaymentOptionByIdFailure"), ex, "An error occurred while retrieving the payment option with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnPaymentOptionAndCodeResponseModel> CreatePaymentOptionAsync(PaymentOption paymentOption)
    {
        try
        {
            if (await _appDataDbContext.PaymentOptions.AnyAsync(existingPaymentOption => existingPaymentOption.Name == paymentOption.Name))
                return new ReturnPaymentOptionAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateEntityName);

            if (await _appDataDbContext.PaymentOptions.AnyAsync(existingPaymentOption => existingPaymentOption.NameAlias == paymentOption.NameAlias))
                return new ReturnPaymentOptionAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateEntityNameAlias);

            paymentOption.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.PaymentOptions.FirstOrDefaultAsync(otherPaymentOption => otherPaymentOption.Id == paymentOption.Id) is not null)
                paymentOption.Id = Guid.NewGuid().ToString();

            paymentOption.Description = paymentOption.Description ?? "";
            paymentOption.IsDeactivated = paymentOption.IsDeactivated ?? false;
            paymentOption.ExistsInOrder = paymentOption.ExistsInOrder ?? false;

            DateTime dateTimeNow = DateTime.Now;
            paymentOption.CreatedAt = dateTimeNow;
            paymentOption.ModifiedAt = dateTimeNow;
            await _appDataDbContext.PaymentOptions.AddAsync(paymentOption);

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreatePaymentOptionSuccess"), "The payment option was successfully created.");
            return new ReturnPaymentOptionAndCodeResponseModel(paymentOption, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreatePaymentOptionFailure"), ex, "An error occurred while creating the payment option. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdatePaymentOptionAsync(PaymentOption updatedPaymentOption)
    {
        try
        {
            if (updatedPaymentOption.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            PaymentOption? foundPaymentOption = await _appDataDbContext.PaymentOptions.FirstOrDefaultAsync(paymentOption => paymentOption.Id == updatedPaymentOption.Id);
            if (foundPaymentOption is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdatePaymentOptionFailureDueToNullPaymentOption"), "The payment option with Id={id} was not found and thus the update could not proceed.", updatedPaymentOption.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            //if null do nothing
            if (updatedPaymentOption.Name is not null)
            {
                if (await _appDataDbContext.PaymentOptions.AnyAsync(existingPaymentOption => existingPaymentOption.Name == updatedPaymentOption.Name && existingPaymentOption.Id != updatedPaymentOption.Id))
                    return DataLibraryReturnedCodes.DuplicateEntityName;
                foundPaymentOption.Name = updatedPaymentOption.Name;
            }

            //if null do nothing
            if (updatedPaymentOption.NameAlias is not null)
            {
                if (await _appDataDbContext.PaymentOptions.AnyAsync(existingPaymentOption => existingPaymentOption.NameAlias == updatedPaymentOption.NameAlias && existingPaymentOption.Id != updatedPaymentOption.Id))
                    return DataLibraryReturnedCodes.DuplicateEntityNameAlias;
                foundPaymentOption.NameAlias = updatedPaymentOption.NameAlias;
            }

            //probably I will not allow in the UI to update payment details like this, but it does not cost me much to do that here
            if (updatedPaymentOption.PaymentDetails is not null && !updatedPaymentOption.PaymentDetails.Any())
                foundPaymentOption.PaymentDetails.Clear();
            else if (updatedPaymentOption.PaymentDetails is not null)
            {
                List<string> updatedPaymentDetailsIds = updatedPaymentOption.PaymentDetails.Select(updatePaymentDetails => updatePaymentDetails.Id!).ToList(); // just add them here, for filtering below
                List<PaymentDetails> filteredUpdatedPaymentDetails = await _appDataDbContext.PaymentDetails
                    .Where(databasePaymentDetails => updatedPaymentDetailsIds.Contains(databasePaymentDetails.Id!))
                    .ToListAsync();

                foundPaymentOption.PaymentDetails.Clear();
                foundPaymentOption.PaymentDetails.AddRange(filteredUpdatedPaymentDetails);
            }

            foundPaymentOption.Description = updatedPaymentOption.Description ?? foundPaymentOption.Description;
            foundPaymentOption.ExtraCost = updatedPaymentOption.ExtraCost ?? foundPaymentOption.ExtraCost;
            foundPaymentOption.IsDeactivated = updatedPaymentOption.IsDeactivated ?? foundPaymentOption.IsDeactivated;
            foundPaymentOption.ExistsInOrder = updatedPaymentOption.ExistsInOrder ?? foundPaymentOption.ExistsInOrder;
            foundPaymentOption.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdatePaymentOptionSuccess"), "The payment option was successfully updated.");
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdatePaymentOptionFailure"), ex, "An error occurred while updating the payment option with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedPaymentOption.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeletePaymentOptionAsync(string paymentOptionId)
    {
        try
        {
            PaymentOption? foundPaymentOption = await _appDataDbContext.PaymentOptions.FirstOrDefaultAsync(paymentOption => paymentOption.Id == paymentOptionId);
            if (foundPaymentOption is null)
            {
                _logger.LogWarning(new EventId(9999, "DeletePaymentOptionFailureDueToNullPaymentOption"), "The payment option with Id={id} was not found and thus the deletion could not proceed.", paymentOptionId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (foundPaymentOption.ExistsInOrder!.Value)
            {
                //the if statement here is only for performance, because potentionally a pointless database update is prevented
                if (!foundPaymentOption.IsDeactivated!.Value)
                {
                    foundPaymentOption.IsDeactivated = true;
                    await _appDataDbContext.SaveChangesAsync();
                }

                _logger.LogInformation(new EventId(9999, "DeletePaymentOptionSuccessButSetToDeactivated"), "The payment option with Id={id} exists in an order and thus can not be fully deleted until that order is deleted, " +
                    "but it is now correctly deactivated.", paymentOptionId);
                return DataLibraryReturnedCodes.NoErrorButNotFullyDeleted;
            }

            _appDataDbContext.PaymentOptions.Remove(foundPaymentOption);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeletePaymentOptionSuccess"), "The payment option was successfully deleted with Id={id}.", paymentOptionId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeletePaymentOptionFailure"), ex, "An error occurred while deleting the payment option with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", paymentOptionId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
