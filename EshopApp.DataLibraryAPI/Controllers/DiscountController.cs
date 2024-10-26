using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.DiscountModels;
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

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetDiscounts(int amount)
    {
        try
        {
            ReturnDiscountsAndCodeResponseModel response = await _discountDataAccess.GetDiscountsAsync(amount);
            return Ok(response.Discounts);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDiscountById(string id)
    {
        try
        {
            ReturnDiscountAndCodeResponseModel response = await _discountDataAccess.GetDiscountByIdAsync(id);
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
    public async Task<IActionResult> CreateDiscount(Discount discount)
    {
        try
        {
            ReturnDiscountAndCodeResponseModel response = await _discountDataAccess.CreateDiscountAsync(discount);
            if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });

            return CreatedAtAction(nameof(GetDiscountById), new { id = response.Discount!.Id }, response.Discount);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateDiscount(Discount discount)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _discountDataAccess.UpdateDiscountAsync(discount);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "EntityNotFoundWithGivenId" });
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
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

}
