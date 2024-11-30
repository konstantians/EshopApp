using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ShippingOptionModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.ShippingOptionModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]

public class ShippingOptionController : ControllerBase
{
    private readonly IShippingOptionDataAccess _shippingOptionDataAccess;

    public ShippingOptionController(IShippingOptionDataAccess shippingOptionDataAccess)
    {
        _shippingOptionDataAccess = shippingOptionDataAccess;
    }

    [HttpGet("Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetShippingOptions(int amount, bool includeDeactivated)
    {
        try
        {
            ReturnShippingOptionsAndCodeResponseModel response = await _shippingOptionDataAccess.GetShippingOptionsAsync(amount, includeDeactivated);
            return Ok(response.ShippingOptions);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetShippingOptionById(string id, bool includeDeactivated)
    {
        try
        {
            ReturnShippingOptionAndCodeResponseModel response = await _shippingOptionDataAccess.GetShippingOptionByIdAsync(id, includeDeactivated);
            if (response.ShippingOption is null)
                return NotFound();

            return Ok(response.ShippingOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateShippingOption(CreateShippingOptionRequestModel createShippingOptionRequestModel)
    {
        try
        {
            ShippingOption shippingOption = new ShippingOption();
            shippingOption.Name = createShippingOptionRequestModel.Name;
            shippingOption.Description = createShippingOptionRequestModel.Description;
            shippingOption.ExtraCost = createShippingOptionRequestModel.ExtraCost;
            shippingOption.ContainsDelivery = createShippingOptionRequestModel.ContainsDelivery;
            shippingOption.IsDeactivated = createShippingOptionRequestModel.IsDeactivated;
            shippingOption.ExistsInOrder = createShippingOptionRequestModel.ExistsInOrder;

            ReturnShippingOptionAndCodeResponseModel response = await _shippingOptionDataAccess.CreateShippingOptionAsync(shippingOption);
            if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });

            return CreatedAtAction(nameof(GetShippingOptionById), new { id = response.ShippingOption!.Id, includeDeactivated = true }, response.ShippingOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateShippingOption(UpdateShippingOptionRequestModel updateShippingOptionRequestModel)
    {
        try
        {
            ShippingOption shippingOption = new ShippingOption();
            shippingOption.Id = updateShippingOptionRequestModel.Id;
            shippingOption.Name = updateShippingOptionRequestModel.Name;
            shippingOption.Description = updateShippingOptionRequestModel.Description;
            shippingOption.ExtraCost = updateShippingOptionRequestModel.ExtraCost;
            shippingOption.ContainsDelivery = updateShippingOptionRequestModel.ContainsDelivery;
            shippingOption.IsDeactivated = updateShippingOptionRequestModel.IsDeactivated;
            shippingOption.ExistsInOrder = updateShippingOptionRequestModel.ExistsInOrder;
            shippingOption.Orders = updateShippingOptionRequestModel.OrderIds?.Select(orderId => new Order { Id = orderId }).ToList()!;

            DataLibraryReturnedCodes returnedCode = await _shippingOptionDataAccess.UpdateShippingOptionAsync(shippingOption);
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
    public async Task<IActionResult> DeleteShippingOption(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _shippingOptionDataAccess.DeleteShippingOptionAsync(id);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
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
