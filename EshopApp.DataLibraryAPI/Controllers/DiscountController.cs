using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.DiscountModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.DiscountModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class DiscountController : ControllerBase
{
    private readonly IDiscountDataAccess _discountDataAccess;

    public DiscountController(IDiscountDataAccess discountDataAccess)
    {
        _discountDataAccess = discountDataAccess;
    }

    [HttpGet("Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetDiscounts(int amount, bool includeDeactivated)
    {
        try
        {
            ReturnDiscountsAndCodeResponseModel response = await _discountDataAccess.GetDiscountsAsync(amount, includeDeactivated);
            return Ok(response.Discounts);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetDiscountById(string id, bool includeDeactivated)
    {
        try
        {
            ReturnDiscountAndCodeResponseModel response = await _discountDataAccess.GetDiscountByIdAsync(id, includeDeactivated);
            if (response.Discount is null)
                return NotFound();

            return Ok(response.Discount);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateDiscount(CreateDiscountRequestModel createDiscountRequestModel)
    {
        try
        {
            Discount discount = new Discount();
            discount.Name = createDiscountRequestModel.Name;
            discount.Percentage = createDiscountRequestModel.Percentage;
            discount.IsDeactivated = createDiscountRequestModel.IsDeactivated;

            ReturnDiscountAndCodeResponseModel response = await _discountDataAccess.CreateDiscountAsync(discount);
            if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });

            return CreatedAtAction(nameof(GetDiscountById), new { id = response.Discount!.Id, includeDeactivated = false }, response.Discount);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDiscount(UpdateDiscountRequestModel updateDiscountRequestModel)
    {
        try
        {
            Discount discount = new Discount();
            discount.Id = updateDiscountRequestModel.Id;
            discount.Name = updateDiscountRequestModel.Name;
            discount.Percentage = updateDiscountRequestModel.Percentage;
            discount.IsDeactivated = updateDiscountRequestModel.IsDeactivated;
            discount.Variants = updateDiscountRequestModel.VariantIds?.Select(variantId => new Variant { Id = variantId }).ToList()!;

            DataLibraryReturnedCodes returnedCode = await _discountDataAccess.UpdateDiscountAsync(discount);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
            else if (returnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDiscount(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _discountDataAccess.DeleteDiscountAsync(id);
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
