using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.VariantModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.VariantImageModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.VariantModels;
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

    [HttpGet("Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetVariants(int amount, bool includeDeactivated)
    {
        try
        {
            ReturnVariantsAndCodeResponseModel response = await _variantDataAccess.GetVariantsAsync(amount, includeDeactivated);
            return Ok(response.Variants);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Id/{id}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetVariantById(string id, bool includeDeactivated)
    {
        try
        {
            ReturnVariantAndCodeResponseModel response = await _variantDataAccess.GetVariantByIdAsync(id, includeDeactivated);
            if (response.Variant is null)
                return NotFound();

            return Ok(response.Variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Sku/{sku}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetVariantBySku(string sku, bool includeDeactivated)
    {
        try
        {
            ReturnVariantAndCodeResponseModel response = await _variantDataAccess.GetVariantBySKUAsync(sku, includeDeactivated);
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
    public async Task<IActionResult> CreateVariant(CreateVariantRequestModel createVariantRequestModel)
    {
        try
        {
            Variant variant = new Variant();
            variant.SKU = createVariantRequestModel.SKU;
            variant.Price = createVariantRequestModel.Price;
            variant.UnitsInStock = createVariantRequestModel.UnitsInStock;
            variant.IsThumbnailVariant = createVariantRequestModel.IsThumbnailVariant;
            variant.IsDeactivated = createVariantRequestModel.IsDeactivated;
            variant.ExistsInOrder = createVariantRequestModel.ExistsInOrder;
            variant.ProductId = createVariantRequestModel.ProductId;
            variant.DiscountId = createVariantRequestModel.DiscountId;
            foreach (string attributeId in createVariantRequestModel.AttributeIds)
                variant.Attributes.Add(new AppAttribute() { Id = attributeId });
            foreach (CreateVariantImageRequestModel variantImage in createVariantRequestModel.VariantImageRequestModels)
                variant.VariantImages.Add(new VariantImage() { IsThumbNail = variantImage.IsThumbNail, ImageId = variantImage.ImageId });

            ReturnVariantAndCodeResponseModel response = await _variantDataAccess.CreateVariantAsync(variant);
            if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidProductIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidProductIdWasGiven" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateVariantSku)
                return BadRequest(new { ErrorMessage = "DuplicateVariantSku" });

            return CreatedAtAction(nameof(GetVariantById), new { id = response.Variant!.Id, includeDeactivated = true }, response.Variant);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateVariant(UpdateVariantRequestModel updateVariantRequestModel)
    {
        try
        {
            Variant variant = new Variant();
            variant.Id = updateVariantRequestModel.Id;
            variant.SKU = updateVariantRequestModel.SKU;
            variant.Price = updateVariantRequestModel.Price;
            variant.UnitsInStock = updateVariantRequestModel.UnitsInStock;
            variant.IsThumbnailVariant = updateVariantRequestModel.IsThumbnailVariant;
            variant.DiscountId = updateVariantRequestModel.DiscountId;
            //if null then null is inserted in variant.Attributes otherewise it creates an appattribute object for each attribute id
            variant.Attributes = updateVariantRequestModel.AttributeIds?.Select(attributeId => new AppAttribute { Id = attributeId }).ToList()!;
            variant.VariantImages = updateVariantRequestModel.ImagesIds?.Select(imageId => new VariantImage { ImageId = imageId, IsThumbNail = updateVariantRequestModel.ImageIdThatShouldBeThumbnail == imageId }).ToList()!;

            DataLibraryReturnedCodes returnedCode = await _variantDataAccess.UpdateVariantAsync(variant);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
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
