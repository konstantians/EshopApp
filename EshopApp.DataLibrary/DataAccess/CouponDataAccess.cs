using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CouponModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;
public class CouponDataAccess : ICouponDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<CouponDataAccess> _logger;

    public CouponDataAccess(AppDataDbContext appDataDbContext, ILogger<CouponDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<CouponDataAccess>.Instance;
    }

    public async Task<ReturnCouponsAndCodeResponseModel> GetCouponsAsync(int amount, bool includeDeactivated)
    {
        try
        {
            List<Coupon> coupons;
            if (!includeDeactivated)
            {
                coupons = await _appDataDbContext.Coupons
                    .Include(c => c.UserCoupons)
                    .Where(coupon => !coupon.IsDeactivated!.Value)
                    .Take(amount)
                    .ToListAsync();
            }
            else
            {
                coupons = await _appDataDbContext.Coupons
                    .Include(c => c.UserCoupons)
                    .Take(amount)
                    .ToListAsync();
            }

            return new ReturnCouponsAndCodeResponseModel(coupons, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCouponsFailure"), ex, "An error occurred while retrieving the coupons. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCouponAndCodeResponseModel> GetCouponByIdAsync(string id, bool includeDeactivated)
    {
        try
        {
            Coupon? foundCoupon;
            if (!includeDeactivated)
            {
                foundCoupon = await _appDataDbContext.Coupons
                    .Include(c => c.UserCoupons)
                    .FirstOrDefaultAsync(coupon => coupon.Id == id && !coupon.IsDeactivated!.Value);
            }
            else
            {
                foundCoupon = await _appDataDbContext.Coupons
                    .Include(c => c.UserCoupons)
                    .FirstOrDefaultAsync(coupon => coupon.Id == id);
            }

            return new ReturnCouponAndCodeResponseModel(foundCoupon!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCouponByIdFailure"), ex, "An error occurred while retrieving the coupon with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCouponAndCodeResponseModel> CreateCouponAsync(Coupon coupon)
    {
        try
        {
            if (!coupon.IsUserSpecific!.Value && (coupon.StartDate is null || coupon.ExpirationDate is null))
                return new ReturnCouponAndCodeResponseModel(null!, DataLibraryReturnedCodes.StartAndExpirationDatesCanNotBeNullForUniversalCoupons);
            else if (coupon.IsUserSpecific!.Value && coupon.DefaultDateIntervalInDays is null)
                return new ReturnCouponAndCodeResponseModel(null!, DataLibraryReturnedCodes.DefaultDateIntervalInDaysCanNotBeNullForUserSpecificCoupons);

            coupon.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Coupons.FirstOrDefaultAsync(otherCoupon => otherCoupon.Id == coupon.Id) is not null)
                coupon.Id = Guid.NewGuid().ToString();

            DateTime dateTimeNow = DateTime.Now;
            coupon.CreatedAt = dateTimeNow;
            coupon.ModifiedAt = dateTimeNow;
            coupon.Description = coupon.Description ?? "";
            coupon.IsUserSpecific = coupon.IsUserSpecific ?? false; //once set this can not change
            coupon.IsDeactivated = coupon.IsDeactivated ?? false;
            coupon.Code = coupon.Code ?? Guid.NewGuid().ToString().Substring(0, 6);
            while (await _appDataDbContext.Coupons.FirstOrDefaultAsync(otherUserCoupon => otherUserCoupon.Code == coupon.Code) is not null)
                coupon.Code = Guid.NewGuid().ToString().Substring(0, 6);

            //NoTrigger should be assigned to the triggerEvent if the coming trigger event does not have a valid value
            //or if the trigger event is not user specific
            if (coupon.TriggerEvent is null || !coupon.IsUserSpecific.Value || (coupon.TriggerEvent != "OnSignUp" &&
                coupon.TriggerEvent != "OnFirstOrder" && coupon.TriggerEvent != "OnEveryFiveOrders" &&
                coupon.TriggerEvent != "OnEveryTenOrders" && coupon.TriggerEvent != "NoTrigger"))
                coupon.TriggerEvent = "NoTrigger";

            if (coupon.IsUserSpecific.Value)
            {
                coupon.StartDate = null;
                coupon.ExpirationDate = null;
            }
            else
                coupon.DefaultDateIntervalInDays = null;

            await _appDataDbContext.Coupons.AddAsync(coupon);

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateCouponSuccess"), "The coupon was successfully created.");
            return new ReturnCouponAndCodeResponseModel(coupon, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateCouponFailure"), ex, "An error occurred while creating the coupon. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateCouponAsync(Coupon updatedCoupon)
    {
        try
        {
            if (updatedCoupon.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            Coupon? foundCoupon = await _appDataDbContext.Coupons.Include(coupon => coupon.UserCoupons).FirstOrDefaultAsync(coupon => coupon.Id == updatedCoupon.Id);
            if (foundCoupon is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateCouponFailureDueToNullCoupon"), "The coupon with Id={id} was not found and thus the update could not proceed.", updatedCoupon.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            //if null do nothing
            foundCoupon.Description = updatedCoupon.Description ?? foundCoupon.Description;
            foundCoupon.DiscountPercentage = updatedCoupon.DiscountPercentage ?? foundCoupon.DiscountPercentage;
            foundCoupon.UsageLimit = updatedCoupon.UsageLimit ?? foundCoupon.UsageLimit;
            foundCoupon.IsDeactivated = updatedCoupon.IsDeactivated ?? foundCoupon.IsDeactivated;

            //if null do nothing and in this specific case make sure that it does not exist inside an order
            if (updatedCoupon.Code is not null && !foundCoupon.UserCoupons.Any(usercoupon => usercoupon.ExistsInOrder!.Value))
            {
                if (await _appDataDbContext.Coupons.AnyAsync(existingCoupon => existingCoupon.Code == updatedCoupon.Code && existingCoupon.Id != updatedCoupon.Id))
                    return DataLibraryReturnedCodes.DuplicateEntityCode;
                foundCoupon.Code = updatedCoupon.Code;
            }

            if (foundCoupon.IsUserSpecific!.Value) //if it is user specific the intervalInDays DOES matter
            {
                foundCoupon.DefaultDateIntervalInDays = updatedCoupon.DefaultDateIntervalInDays ?? foundCoupon.DefaultDateIntervalInDays;
            }
            else //if it is universal the coupons date DOES matter
            {
                foundCoupon.StartDate = updatedCoupon.StartDate ?? foundCoupon.StartDate;
                foundCoupon.ExpirationDate = updatedCoupon.ExpirationDate ?? foundCoupon.ExpirationDate;
            }

            if (updatedCoupon.TriggerEvent is not null && foundCoupon.IsUserSpecific!.Value &&
                (updatedCoupon.TriggerEvent == "OnSignUp" ||
                updatedCoupon.TriggerEvent == "OnFirstOrder" || updatedCoupon.TriggerEvent == "OnEveryFiveOrders" ||
                updatedCoupon.TriggerEvent == "OnEveryTenOrders" || updatedCoupon.TriggerEvent == "NoTrigger"))
                foundCoupon.TriggerEvent = updatedCoupon.TriggerEvent;

            //For now this only removes user coupons if needed, but can add them. In general the UpdateCoupon should probably not be used to add or remove coupons from users since the addCouponToUser exists
            //I just did it, because it was easy to do for the remove, but I think the add would complicate a lot the code, since I would have to preserve the previous codes.
            //Also another thing to keep in mind is that usercoupons can not exist on their own(they depend the coupon), so creating new here using the update method seems wrong(since it should not create other entities)
            if (updatedCoupon.UserCoupons is not null && !updatedCoupon.UserCoupons.Any())
                foundCoupon.UserCoupons.Clear();
            else if (updatedCoupon.UserCoupons is not null)
            {
                List<string> updatedUserCoupons = updatedCoupon.UserCoupons.Select(userCoupon => userCoupon.Id!).ToList(); // just add them here, for filtering below
                List<UserCoupon> filteredUserCoupons = await _appDataDbContext.UserCoupons
                    .Where(databaseUserCoupon => updatedUserCoupons.Contains(databaseUserCoupon.Id!))
                    .ToListAsync();

                foreach (var filteredUserCoupon in filteredUserCoupons)
                {
                    if (!foundCoupon.IsUserSpecific!.Value) //if the coupon is universal the values of the userCoupons should be updated accordingly since the rest of the coupon was most likely updated
                    {
                        filteredUserCoupon.Code = foundCoupon.Code;
                        filteredUserCoupon.StartDate = foundCoupon.StartDate;
                        filteredUserCoupon.ExpirationDate = foundCoupon.ExpirationDate;
                    }
                    else
                    {
                        filteredUserCoupon.StartDate = DateTime.Now;
                        filteredUserCoupon.ExpirationDate = DateTime.Now.AddDays((double)foundCoupon.DefaultDateIntervalInDays!);
                    }
                }

                foundCoupon.UserCoupons.Clear();
                foundCoupon.UserCoupons.AddRange(filteredUserCoupons);
            }

            foundCoupon.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateCouponSuccess"), "The coupon was successfully updated.");
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateCouponFailure"), ex, "An error occurred while updating the coupon with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedCoupon.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteCouponAsync(string couponId)
    {
        try
        {
            Coupon? foundCoupon = await _appDataDbContext.Coupons.FirstOrDefaultAsync(coupon => coupon.Id == couponId);
            if (foundCoupon is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteCouponFailureDueToNullCoupon"), "The coupon with Id={id} was not found and thus the deletion could not proceed.", couponId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (foundCoupon.UserCoupons.Any(userCoupon => userCoupon.ExistsInOrder!.Value))
            {
                //the if statement here is only for performance, because potentionally a pointless database update is prevented
                if (!foundCoupon.IsDeactivated!.Value)
                {
                    foundCoupon.IsDeactivated = true;
                    await _appDataDbContext.SaveChangesAsync();
                }

                _logger.LogInformation(new EventId(9999, "DeleteCouponSuccessButSetToDeactivated"), "The coupon with Id={id} exists in an order and thus can not be fully deleted until that order is deleted, " +
                    "but it was correctly deactivated.", couponId);
                return DataLibraryReturnedCodes.NoErrorButNotFullyDeleted;
            }

            _appDataDbContext.Coupons.Remove(foundCoupon);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteCouponSuccess"), "The coupon was successfully deleted with Id={id}.", couponId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteCouponFailure"), ex, "An error occurred while deleting the coupon with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", couponId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnUserCouponsAndCodeResponseModel> GetCouponsOfUserAsync(string userId, bool includeDeactivated)
    {
        try
        {
            List<UserCoupon> userCoupons;
            if (!includeDeactivated)
            {
                //a userCoupon can not exist without being connected to a coupon & a coupon has the IsDeactivated property always filled
                userCoupons = await _appDataDbContext.UserCoupons
                    .Include(userCoupon => userCoupon.Coupon)
                    .Where(userCoupon => !userCoupon.Coupon!.IsDeactivated!.Value && userCoupon.UserId == userId)
                    .ToListAsync();
            }
            else
            {
                userCoupons = await _appDataDbContext.UserCoupons
                    .Include(userCoupon => userCoupon.Coupon)
                    .Where(userCoupon => userCoupon.UserId == userId)
                    .ToListAsync();
            }

            return new ReturnUserCouponsAndCodeResponseModel(userCoupons, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCouponsOfUserFailure"), ex, "An error occurred while retrieving the coupons. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnUserCouponAndCodeResponseModel> AddCouponToUserAsync(UserCoupon userCoupon)
    {
        try
        {
            if (userCoupon.CouponId is null)
                return new ReturnUserCouponAndCodeResponseModel(null!, DataLibraryReturnedCodes.TheIdOfTheCouponCanNotBeNull);
            else if (userCoupon.UserId is null)
                return new ReturnUserCouponAndCodeResponseModel(null!, DataLibraryReturnedCodes.TheIdOfTheUserCanNotBeNull);

            Coupon? foundCoupon = await _appDataDbContext.Coupons.FirstOrDefaultAsync(otherCoupon => otherCoupon.Id == userCoupon.CouponId);
            if (foundCoupon is null)
                return new ReturnUserCouponAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidCouponIdWasGiven);

            userCoupon.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.UserCoupons.FirstOrDefaultAsync(otherUserCoupon => otherUserCoupon.Id == userCoupon.Id) is not null)
                userCoupon.Id = Guid.NewGuid().ToString();


            userCoupon.TimesUsed = userCoupon.TimesUsed ?? 0;
            userCoupon.IsDeactivated = userCoupon.IsDeactivated ?? false;
            userCoupon.ExistsInOrder = userCoupon.ExistsInOrder ?? false;

            DateTime dateTimeNow = DateTime.Now;
            userCoupon.CreatedAt = dateTimeNow;
            userCoupon.ModifiedAt = dateTimeNow;

            //in case where the coupon is universal just copy most of the information for safery to the user coupon(I could have left those field null, but I think this is more safe)
            if (!foundCoupon.IsUserSpecific!.Value)
            {
                userCoupon.StartDate = foundCoupon.StartDate;
                userCoupon.ExpirationDate = foundCoupon.ExpirationDate;
                userCoupon.Code = foundCoupon.Code;
            }
            else if (foundCoupon.IsUserSpecific!.Value && userCoupon.StartDate is not null && userCoupon.ExpirationDate is not null)
            {
                userCoupon.Code = userCoupon.Code ?? Guid.NewGuid().ToString().Substring(0, 6);
                while (await _appDataDbContext.UserCoupons.FirstOrDefaultAsync(otherUserCoupon => otherUserCoupon.Code == userCoupon.Code) is not null)
                    userCoupon.Code = Guid.NewGuid().ToString().Substring(0, 6);
            }
            //if the startdate and the expiration date are null, but the coupon is user specific
            else
            {
                userCoupon.Code = userCoupon.Code ?? Guid.NewGuid().ToString().Substring(0, 6);
                while (await _appDataDbContext.UserCoupons.FirstOrDefaultAsync(otherUserCoupon => otherUserCoupon.Code == userCoupon.Code) is not null)
                    userCoupon.Code = Guid.NewGuid().ToString().Substring(0, 6);
                userCoupon.StartDate = dateTimeNow;
                userCoupon.ExpirationDate = dateTimeNow.AddDays((double)foundCoupon.DefaultDateIntervalInDays!);
            }

            await _appDataDbContext.UserCoupons.AddAsync(userCoupon);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "AddCouponToUserSuccess"), "The coupon was successfully added to user.");
            return new ReturnUserCouponAndCodeResponseModel(userCoupon, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "AddCouponToUserFailure"), ex, "An error occurred while adding the coupon to user. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateUserCouponAsync(UserCoupon updatedUserCoupon)
    {
        try
        {
            if (updatedUserCoupon.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            UserCoupon? foundUserCoupon = await _appDataDbContext.UserCoupons.Include(userCoupon => userCoupon.Coupon).FirstOrDefaultAsync(userCoupon => userCoupon.Id == updatedUserCoupon.Id);
            if (foundUserCoupon is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateUserCouponFailureDueToNullCoupon"), "The user coupon with Id={id} was not found and thus the update could not proceed.", updatedUserCoupon.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (foundUserCoupon.Coupon!.IsUserSpecific!.Value && !foundUserCoupon.ExistsInOrder!.Value
                && updatedUserCoupon.Code is not null)
            {
                if (await _appDataDbContext.UserCoupons.AnyAsync(existingUserCoupon => existingUserCoupon.Code == updatedUserCoupon.Code && existingUserCoupon.Id != updatedUserCoupon.Id))
                    return DataLibraryReturnedCodes.DuplicateEntityCode;
                foundUserCoupon.Code = updatedUserCoupon.Code;
            }

            if (foundUserCoupon.Coupon!.IsUserSpecific!.Value)
            {
                foundUserCoupon.StartDate = updatedUserCoupon.StartDate ?? foundUserCoupon!.StartDate;
                foundUserCoupon.ExpirationDate = updatedUserCoupon.ExpirationDate ?? foundUserCoupon!.ExpirationDate;
            }

            foundUserCoupon.TimesUsed = updatedUserCoupon.TimesUsed ?? foundUserCoupon.TimesUsed;
            foundUserCoupon.ModifiedAt = DateTime.Now;

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateUserCouponSuccess"), "The user coupon was successfully updated.");
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateUserCouponFailure"), ex, "An error occurred while updating the user coupon with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedUserCoupon.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> RemoveCouponFromUser(string userCouponId)
    {
        try
        {
            if (userCouponId is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            UserCoupon? foundUserCoupon = await _appDataDbContext.UserCoupons.FirstOrDefaultAsync(userCoupon => userCoupon.Id == userCouponId);
            if (foundUserCoupon is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteUserCouponFailureDueToNullUserCoupon"), "The user coupon with Id={id} was not found and thus the deletion could not proceed.", userCouponId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (foundUserCoupon.ExistsInOrder!.Value)
            {
                //the if statement here is only for performance, because potentionally a pointless database update is prevented
                if (!foundUserCoupon.IsDeactivated!.Value)
                {
                    foundUserCoupon.IsDeactivated = true;
                    await _appDataDbContext.SaveChangesAsync();
                }

                _logger.LogInformation(new EventId(9999, "DeleteUserCouponSuccessButSetToDeactivated"), "The user coupon with Id={id} exists in an order and thus can not be fully deleted until that order is deleted, " +
                    "but it was correctly deactivated.", userCouponId);
                return DataLibraryReturnedCodes.NoErrorButNotFullyDeleted;
            }

            _appDataDbContext.UserCoupons.Remove(foundUserCoupon);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteUserCouponSuccess"), "The user coupon was successfully deleted with Id={id}.", userCouponId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteUserCouponFailure"), ex, "An error occurred while deleting the user coupon with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userCouponId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> RemoveAllCouponsOfUser(string userId)
    {
        try
        {
            if (userId is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            // If they exist in order change their deactivated value
            await _appDataDbContext.UserCoupons
                .Where(coupon => coupon.UserId == userId && coupon.ExistsInOrder!.Value && !coupon.IsDeactivated!.Value)
                .ExecuteUpdateAsync(setters => setters.SetProperty(coupon => coupon.IsDeactivated, true));

            // For the rest remove simply remove them
            await _appDataDbContext.UserCoupons
                .Where(coupon => coupon.UserId == userId && !coupon.ExistsInOrder!.Value)
                .ExecuteDeleteAsync();

            _logger.LogInformation(new EventId(9999, "RemoveAllTheCouponsOfUserSuccess"), "The coupons of the user with UserId={UserId} have all been removed or set to deactivated.", userId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "RemoveAllTheCouponsOfUserFailure"), ex, "An error occurred while deleting coupons of the user with UserId={UserId}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

}
