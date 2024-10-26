using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.VariantModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class VariantController : ControllerBase
{
    private readonly IVariantDataAccess _variantDataAccess;

    public VariantController(IVariantDataAccess variantDataAccess)
    {
        _variantDataAccess = variantDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetVariants(int amount)
    {
        try
        {
            ReturnVariantsAndCodeResponseModel response = await _variantDataAccess.GetVariantsAsync(amount);
            return Ok(response.Variants);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Id/{id}")]
    public async Task<IActionResult> GetVariantById(string id)
    {
        try
        {
            ReturnVariantAndCodeResponseModel response = await _variantDataAccess.GetVariantByIdAsync(id);
            if (response.Variant is null)
                return NotFound();

            return Ok(response.Variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Sku/{sku}")]
    public async Task<IActionResult> GetVariantBySku(string sku)
    {
        try
        {
            ReturnVariantAndCodeResponseModel response = await _variantDataAccess.GetVariantBySKUAsync(sku);
            if (response.Variant is null)
                return NotFound();

            return Ok(response.Variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateVariant(Variant variant)
    {
        try
        {
            ReturnVariantAndCodeResponseModel response = await _variantDataAccess.CreateVariantAsync(variant);
            if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidProductIdWasGiven)
                return BadRequest(new { ErrorMessage = "InvalidProductIdWasGiven" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateVariantSku)
                return BadRequest(new { ErrorMessage = "DuplicateVariantSku" });

            return CreatedAtAction(nameof(GetVariantById), new { id = response.Variant!.Id }, response.Variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateVariant(Variant variant)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _variantDataAccess.UpdateVariantAsync(variant);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "EntityNotFoundWithGivenId" });
            else if (returnedCode == DataLibraryReturnedCodes.DuplicateVariantSku)
                return BadRequest(new { ErrorMessage = "DuplicateVariantSku" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVariant(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _variantDataAccess.DeleteVariantAsync(id);
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
