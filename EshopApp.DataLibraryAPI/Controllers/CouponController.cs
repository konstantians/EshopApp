using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CouponModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.CouponModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class CouponController : ControllerBase
{
    private readonly ICouponDataAccess _couponDataAccess;

    public CouponController(ICouponDataAccess couponDataAccess)
    {
        _couponDataAccess = couponDataAccess;
    }

    [HttpGet("Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetCoupons(int amount, bool includeDeactivated)
    {
        try
        {
            ReturnCouponsAndCodeResponseModel response = await _couponDataAccess.GetCouponsAsync(amount, includeDeactivated);
            return Ok(response.Coupons);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetCouponById(string id, bool includeDeactivated)
    {
        try
        {
            ReturnCouponAndCodeResponseModel response = await _couponDataAccess.GetCouponByIdAsync(id, includeDeactivated);
            if (response.Coupon is null)
                return NotFound();

            return Ok(response.Coupon);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCoupon(CreateCouponRequestModel createCouponRequestModel)
    {
        try
        {
            Coupon coupon = new Coupon();
            coupon.Code = createCouponRequestModel.Code;
            coupon.Description = createCouponRequestModel.Description;
            coupon.DiscountPercentage = createCouponRequestModel.DiscountPercentage;
            coupon.UsageLimit = createCouponRequestModel.UsageLimit;
            coupon.DefaultDateIntervalInDays = createCouponRequestModel.DefaultDateIntervalInDays;
            coupon.IsUserSpecific = createCouponRequestModel.IsUserSpecific;
            coupon.IsDeactivated = createCouponRequestModel.IsDeactivated;
            coupon.TriggerEvent = createCouponRequestModel.TriggerEvent;
            coupon.StartDate = createCouponRequestModel.StartDate;
            coupon.ExpirationDate = createCouponRequestModel.ExpirationDate;

            ReturnCouponAndCodeResponseModel response = await _couponDataAccess.CreateCouponAsync(coupon);
            if (response.ReturnedCode == DataLibraryReturnedCodes.StartAndExpirationDatesCanNotBeNullForUniversalCoupons)
                return BadRequest(new { ErrorMessage = "StartAndExpirationDatesCanNotBeNullForUniversalCoupons" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DefaultDateIntervalInDaysCanNotBeNullForUserSpecificCoupons)
                return BadRequest(new { ErrorMessage = "DefaultDateIntervalInDaysCanNotBeNullForUserSpecificCoupons" });

            return CreatedAtAction(nameof(GetCouponById), new { id = response.Coupon!.Id, includeDeactivated = true }, response.Coupon);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCoupon(UpdateCouponRequestModel updateCouponRequestModel)
    {
        try
        {
            Coupon updatedCoupon = new Coupon();
            updatedCoupon.Id = updateCouponRequestModel.Id;
            updatedCoupon.Code = updateCouponRequestModel.Code;
            updatedCoupon.Description = updateCouponRequestModel.Description;
            updatedCoupon.DiscountPercentage = updateCouponRequestModel.DiscountPercentage;
            updatedCoupon.UsageLimit = updateCouponRequestModel.UsageLimit;
            updatedCoupon.DefaultDateIntervalInDays = updateCouponRequestModel.DefaultDateIntervalInDays;
            updatedCoupon.IsDeactivated = updateCouponRequestModel.IsDeactivated;
            updatedCoupon.TriggerEvent = updateCouponRequestModel.TriggerEvent;
            updatedCoupon.StartDate = updateCouponRequestModel.StartDate;
            updatedCoupon.ExpirationDate = updateCouponRequestModel.ExpirationDate;
            updatedCoupon.UserCoupons = updateCouponRequestModel.UserCouponIds?.Select(userCouponId => new UserCoupon { Id = userCouponId }).ToList()!;

            DataLibraryReturnedCodes returnedCode = await _couponDataAccess.UpdateCouponAsync(updatedCoupon);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
            else if (returnedCode == DataLibraryReturnedCodes.DuplicateEntityCode)
                return BadRequest(new { ErrorMessage = "DuplicateEntityCode" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCoupon(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _couponDataAccess.DeleteCouponAsync(id);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound();
            else if (returnedCode == DataLibraryReturnedCodes.NoErrorButNotFullyDeleted)
                return Ok(new { WarningMessage = "NoErrorButNotFullyDeleted" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("userId/{userId}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetCouponsOfUser(string userId, bool includeDeactivated)
    {
        try
        {
            ReturnUserCouponsAndCodeResponseModel response = await _couponDataAccess.GetCouponsOfUserAsync(userId, includeDeactivated);
            return Ok(response.UserCoupons);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("AddCouponToUser")]
    public async Task<IActionResult> AddCouponToUser(AddCouponToUserRequestModel addCouponToUserRequestModel)
    {
        try
        {
            UserCoupon userCoupon = new UserCoupon();
            userCoupon.Code = addCouponToUserRequestModel.Code;
            userCoupon.TimesUsed = addCouponToUserRequestModel.TimesUsed;
            userCoupon.IsDeactivated = addCouponToUserRequestModel.IsDeactivated;
            userCoupon.ExistsInOrder = addCouponToUserRequestModel.ExistInOrder;
            userCoupon.StartDate = addCouponToUserRequestModel.StartDate;
            userCoupon.ExpirationDate = addCouponToUserRequestModel.ExpirationDate;
            userCoupon.UserId = addCouponToUserRequestModel.UserId;
            userCoupon.CouponId = addCouponToUserRequestModel.CouponId;

            ReturnUserCouponAndCodeResponseModel response = await _couponDataAccess.AddCouponToUserAsync(userCoupon);
            if (response.ReturnedCode == DataLibraryReturnedCodes.TheIdOfTheCouponCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheCouponCanNotBeNull" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.TheIdOfTheUserCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheUserCanNotBeNull" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidCouponIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidCouponIdWasGiven" });

            return Created("", response.UserCoupon);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut("UpdateUserCoupon")]
    public async Task<IActionResult> UpdateUserCoupon(UpdateUserCouponRequestModel updateUserCouponRequestModel)
    {
        try
        {
            UserCoupon updatedUserCoupon = new UserCoupon();
            updatedUserCoupon.Id = updateUserCouponRequestModel.Id;
            updatedUserCoupon.Code = updateUserCouponRequestModel.Code;
            updatedUserCoupon.TimesUsed = updateUserCouponRequestModel.TimesUsed;
            updatedUserCoupon.IsDeactivated = updateUserCouponRequestModel.IsDeactivated;
            updatedUserCoupon.ExistsInOrder = updateUserCouponRequestModel.ExistInOrder;
            updatedUserCoupon.StartDate = updateUserCouponRequestModel.StartDate;
            updatedUserCoupon.ExpirationDate = updateUserCouponRequestModel.ExpirationDate;

            DataLibraryReturnedCodes returnedCode = await _couponDataAccess.UpdateUserCouponAsync(updatedUserCoupon);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
            else if (returnedCode == DataLibraryReturnedCodes.DuplicateEntityCode)
                return BadRequest(new { ErrorMessage = "DuplicateEntityCode" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("RemoveCouponFromUser/userCouponId/{userCouponId}")]
    public async Task<IActionResult> RemoveCouponFromUser(string userCouponId)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _couponDataAccess.RemoveCouponFromUser(userCouponId);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound();
            else if (returnedCode == DataLibraryReturnedCodes.NoErrorButNotFullyDeleted)
                return Ok(new { WarningMessage = "NoErrorButNotFullyDeleted" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
