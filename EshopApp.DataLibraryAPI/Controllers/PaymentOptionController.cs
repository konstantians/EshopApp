using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.PaymentOptionModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.PaymentOptionModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]

public class PaymentOptionController : ControllerBase
{
    private readonly IPaymentOptionDataAccess _paymentOptionDataAccess;

    public PaymentOptionController(IPaymentOptionDataAccess paymentOptionDataAccess)
    {
        _paymentOptionDataAccess = paymentOptionDataAccess;
    }

    [HttpGet("Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetPaymentOptions(int amount, bool includeDeactivated)
    {
        try
        {
            ReturnPaymentOptionsAndCodeResponseModel response = await _paymentOptionDataAccess.GetPaymentOptionsAsync(amount, includeDeactivated);
            return Ok(response.PaymentOptions);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetPaymentOptionById(string id, bool includeDeactivated)
    {
        try
        {
            ReturnPaymentOptionAndCodeResponseModel response = await _paymentOptionDataAccess.GetPaymentOptionByIdAsync(id, includeDeactivated);
            if (response.PaymentOption is null)
                return NotFound();

            return Ok(response.PaymentOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentOption(CreatePaymentOptionRequestModel createPaymentOptionRequestModel)
    {
        try
        {
            PaymentOption paymentOption = new PaymentOption();
            paymentOption.Name = createPaymentOptionRequestModel.Name;
            paymentOption.NameAlias = createPaymentOptionRequestModel.NameAlias;
            paymentOption.Description = createPaymentOptionRequestModel.Description;
            paymentOption.ExtraCost = createPaymentOptionRequestModel.ExtraCost;
            paymentOption.IsDeactivated = createPaymentOptionRequestModel.IsDeactivated;
            paymentOption.ExistsInOrder = createPaymentOptionRequestModel.ExistsInOrder;

            ReturnPaymentOptionAndCodeResponseModel response = await _paymentOptionDataAccess.CreatePaymentOptionAsync(paymentOption);
            if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityNameAlias)
                return BadRequest(new { ErrorMessage = "DuplicateEntityNameAlias" });

            return CreatedAtAction(nameof(GetPaymentOptionById), new { id = response.PaymentOption!.Id, includeDeactivated = true }, response.PaymentOption);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdatePaymentOption(UpdatePaymentOptionRequestModel updatePaymentOptionRequestModel)
    {
        try
        {
            PaymentOption paymentOption = new PaymentOption();
            paymentOption.Id = updatePaymentOptionRequestModel.Id;
            paymentOption.Name = updatePaymentOptionRequestModel.Name;
            paymentOption.NameAlias = updatePaymentOptionRequestModel.NameAlias;
            paymentOption.Description = updatePaymentOptionRequestModel.Description;
            paymentOption.ExtraCost = updatePaymentOptionRequestModel.ExtraCost;
            paymentOption.IsDeactivated = updatePaymentOptionRequestModel.IsDeactivated;
            paymentOption.ExistsInOrder = updatePaymentOptionRequestModel.ExistsInOrder;
            paymentOption.PaymentDetails = updatePaymentOptionRequestModel.PaymentDetailsIds?.Select(paymentDetailId => new PaymentDetails { Id = paymentDetailId }).ToList()!;

            DataLibraryReturnedCodes returnedCode = await _paymentOptionDataAccess.UpdatePaymentOptionAsync(paymentOption);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
            else if (returnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });
            else if (returnedCode == DataLibraryReturnedCodes.DuplicateEntityNameAlias)
                return BadRequest(new { ErrorMessage = "DuplicateEntityNameAlias" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeletePaymentOption(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _paymentOptionDataAccess.DeletePaymentOptionAsync(id);
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
