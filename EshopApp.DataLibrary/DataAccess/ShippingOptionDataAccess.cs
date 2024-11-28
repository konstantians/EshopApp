using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ShippingOptionModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;
public class ShippingOptionDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<ShippingOptionDataAccess> _logger;

    public ShippingOptionDataAccess(AppDataDbContext appDataDbContext, ILogger<ShippingOptionDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<ShippingOptionDataAccess>.Instance;
    }

    public async Task<ReturnShippingOptionsAndCodeResponseModel> GetShippingOptionsOptionsAsync(int amount, bool includeDeactivated)
    {
        try
        {
            List<ShippingOption> shippingOptions;
            if (!includeDeactivated)
            {
                shippingOptions = await _appDataDbContext.ShippingOptions
                    .Include(so => so.Orders)
                        .ThenInclude(o => o!.OrderItems)
                    .Where(shippingOption => !shippingOption.IsDeactivated!.Value)
                    .Take(amount)
                    .ToListAsync();
            }
            else
            {
                shippingOptions = await _appDataDbContext.ShippingOptions
                    .Include(so => so.Orders)
                        .ThenInclude(o => o!.OrderItems)
                    .Take(amount)
                    .ToListAsync();
            }


            return new ReturnShippingOptionsAndCodeResponseModel(shippingOptions, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetShippingOptionsFailure"), ex, "An error occurred while retrieving the shipping options. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnShippingOptionAndCodeResponseModel> GetShippingOptionByIdAsync(string id, bool includeDeactivated)
    {
        try
        {
            ShippingOption? foundShippingOption;

            if (!includeDeactivated)
            {
                foundShippingOption = await _appDataDbContext.ShippingOptions
                    .Include(so => so.Orders)
                        .ThenInclude(o => o!.OrderItems)
                    .FirstOrDefaultAsync(shippingOption => shippingOption.Id == id && !shippingOption.IsDeactivated!.Value);
            }
            else
            {
                foundShippingOption = await _appDataDbContext.ShippingOptions
                    .Include(so => so.Orders)
                        .ThenInclude(o => o!.OrderItems)
                    .FirstOrDefaultAsync(shippingOption => shippingOption.Id == id);
            }

            return new ReturnShippingOptionAndCodeResponseModel(foundShippingOption!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetShippingOptionByIdFailure"), ex, "An error occurred while retrieving the shipping option with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnShippingOptionAndCodeResponseModel> CreateShippingOptionAsync(ShippingOption shippingOption)
    {
        try
        {
            if (await _appDataDbContext.ShippingOptions.AnyAsync(existingShippingOption => existingShippingOption.Name == shippingOption.Name))
                return new ReturnShippingOptionAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateEntityName);

            shippingOption.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.ShippingOptions.FirstOrDefaultAsync(otherShippingOption => otherShippingOption.Id == shippingOption.Id) is not null)
                shippingOption.Id = Guid.NewGuid().ToString();

            shippingOption.Description = shippingOption.Description ?? "";
            shippingOption.ContainsDelivery = shippingOption.ContainsDelivery ?? false;
            shippingOption.IsDeactivated = shippingOption.IsDeactivated ?? false;
            shippingOption.ExistsInOrder = shippingOption.ExistsInOrder ?? false;

            DateTime dateTimeNow = DateTime.Now;
            shippingOption.CreatedAt = dateTimeNow;
            shippingOption.ModifiedAt = dateTimeNow;
            await _appDataDbContext.ShippingOptions.AddAsync(shippingOption);

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateShippingOptionSuccess"), "The shipping option was successfully created.");
            return new ReturnShippingOptionAndCodeResponseModel(shippingOption, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateShippingOptionFailure"), ex, "An error occurred while creating the shipping option. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateShippingOptionAsync(ShippingOption updatedShippingOption)
    {
        try
        {
            if (updatedShippingOption.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            ShippingOption? foundShippingOption = await _appDataDbContext.ShippingOptions.FirstOrDefaultAsync(shippingOption => shippingOption.Id == updatedShippingOption.Id);
            if (foundShippingOption is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateShippingOptionFailureDueToNullShippingOption"), "The shipping option with Id={id} was not found and thus the update could not proceed.", updatedShippingOption.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            //if null do nothing
            if (updatedShippingOption.Name is not null)
            {
                if (await _appDataDbContext.ShippingOptions.AnyAsync(existingShippingOption => existingShippingOption.Name == updatedShippingOption.Name && existingShippingOption.Id != updatedShippingOption.Id))
                    return DataLibraryReturnedCodes.DuplicateEntityName;
                foundShippingOption.Name = updatedShippingOption.Name;
            }

            //probably I will not allow in the UI to update orders like this, but it does not cost me much to do that here
            if (updatedShippingOption.Orders is not null && !updatedShippingOption.Orders.Any())
                foundShippingOption.Orders.Clear();
            else if (updatedShippingOption.Orders is not null)
            {
                List<string> updatedOrderIds = updatedShippingOption.Orders.Select(updateOrderDetails => updateOrderDetails.Id!).ToList(); // just add them here, for filtering below
                List<Order> filteredUpdatedOrders = await _appDataDbContext.Orders
                    .Where(databaseOrders => updatedOrderIds.Contains(databaseOrders.Id!))
                    .ToListAsync();

                foundShippingOption.Orders.Clear();
                foundShippingOption.Orders.AddRange(filteredUpdatedOrders);
            }

            foundShippingOption.Description = updatedShippingOption.Description ?? foundShippingOption.Description;
            foundShippingOption.ExtraCost = updatedShippingOption.ExtraCost ?? foundShippingOption.ExtraCost;
            foundShippingOption.ContainsDelivery = updatedShippingOption.ContainsDelivery ?? foundShippingOption.ContainsDelivery;
            foundShippingOption.IsDeactivated = updatedShippingOption.IsDeactivated ?? foundShippingOption.IsDeactivated;
            foundShippingOption.ExistsInOrder = updatedShippingOption.ExistsInOrder ?? foundShippingOption.ExistsInOrder;
            foundShippingOption.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateShippingOptionSuccess"), "The shipping option was successfully updated.");
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateShippingOptionFailure"), ex, "An error occurred while updating the shipping option with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedShippingOption.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteShippingOptionAsync(string shippingOptionId)
    {
        try
        {
            ShippingOption? foundShippingOption = await _appDataDbContext.ShippingOptions.FirstOrDefaultAsync(shippingOption => shippingOption.Id == shippingOptionId);
            if (foundShippingOption is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteShippingOptionFailureDueToNullShippingOption"), "The shipping option with Id={id} was not found and thus the deletion could not proceed.", shippingOptionId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (foundShippingOption.ExistsInOrder!.Value)
            {
                //the if statement here is only for performance, because potentionally a pointless database update is prevented
                if (!foundShippingOption.IsDeactivated!.Value)
                {
                    foundShippingOption.IsDeactivated = true;
                    await _appDataDbContext.SaveChangesAsync();
                }

                _logger.LogInformation(new EventId(9999, "DeleteShippingOptionSuccessButSetToDeactivated"), "The shipping option with Id={id} exists in an order and thus can not be fully deleted until that order is deleted, " +
                    "but it is now correctly deactivated.", shippingOptionId);
                return DataLibraryReturnedCodes.NoErrorButNotFullyDeleted;
            }

            _appDataDbContext.ShippingOptions.Remove(foundShippingOption);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteShippingOptionSuccess"), "The shipping option was successfully deleted with Id={id}.", shippingOptionId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteShippingOptionFailure"), ex, "An error occurred while deleting the shipping option with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", shippingOptionId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
