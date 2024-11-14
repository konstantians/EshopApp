using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ProductModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.ProductModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.VariantImageModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductDataAccess _productDataAccess;

    public ProductController(IProductDataAccess productDataAccess)
    {
        _productDataAccess = productDataAccess;
    }

    [HttpGet("Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetProducts(int amount, bool includeDeactivated)
    {
        try
        {
            ReturnProductsAndCodeResponseModel response = await _productDataAccess.GetProductsAsync(amount, includeDeactivated);
            return Ok(response.Products);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Category/{category}/Amount/{amount}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetProductsByCategory(string category, int amount, bool includeDeactivated)
    {
        try
        {
            ReturnProductsAndCodeResponseModel response = await _productDataAccess.GetProductsOfCategoryAsync(category, amount, includeDeactivated);
            return Ok(response.Products);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}/includeDeactivated/{includeDeactivated}")]
    public async Task<IActionResult> GetProductById(string id, bool includeDeactivated)
    {
        try
        {
            ReturnProductAndCodeResponseModel response = await _productDataAccess.GetProductByIdAsync(id, includeDeactivated);
            if (response.Product is null)
                return NotFound();

            return Ok(response.Product);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct(CreateProductRequestModel createProductRequestModel)
    {
        try
        {
            Product product = new Product();
            product.Code = createProductRequestModel.Code;
            product.Name = createProductRequestModel.Name;
            product.Description = createProductRequestModel.Description;
            product.IsDeactivated = createProductRequestModel.IsDeactivated;
            product.ExistsInOrder = createProductRequestModel.ExistsInOrder;
            foreach (string categoryId in createProductRequestModel.CategoryIds)
                product.Categories.Add(new Category() { Id = categoryId });

            Variant variant = new Variant();
            variant.SKU = createProductRequestModel.CreateVariantRequestModel!.SKU;
            variant.Price = createProductRequestModel.CreateVariantRequestModel!.Price;
            variant.UnitsInStock = createProductRequestModel.CreateVariantRequestModel!.UnitsInStock;
            variant.IsThumbnailVariant = createProductRequestModel.CreateVariantRequestModel!.IsThumbnailVariant;
            variant.IsDeactivated = createProductRequestModel.IsDeactivated;
            variant.ExistsInOrder = createProductRequestModel.ExistsInOrder;
            variant.ProductId = createProductRequestModel.CreateVariantRequestModel!.ProductId; //This will be overriden, but that is planned and ok
            variant.DiscountId = createProductRequestModel.CreateVariantRequestModel!.DiscountId;
            foreach (string attributeId in createProductRequestModel.CreateVariantRequestModel.AttributeIds)
                variant.Attributes.Add(new AppAttribute() { Id = attributeId });
            foreach (CreateVariantImageRequestModel createVariantImageRequestModel in createProductRequestModel.CreateVariantRequestModel.VariantImageRequestModels)
                variant.VariantImages.Add(new VariantImage() { ImageId = createVariantImageRequestModel.ImageId, IsThumbNail = createVariantImageRequestModel.IsThumbNail });

            product.Variants.Add(variant); //add this all together

            ReturnProductAndCodeResponseModel response = await _productDataAccess.CreateProductAsync(product);
            if (response.ReturnedCode == DataLibraryReturnedCodes.NoVariantWasProvidedForProductCreation)
                return BadRequest(new { ErrorMessage = "NoVariantWasProvidedForProductCreation" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateProductCode)
                return BadRequest(new { ErrorMessage = "DuplicateProductCode" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateVariantSku)
                return BadRequest(new { ErrorMessage = "DuplicateVariantSku" });

            return CreatedAtAction(nameof(GetProductById), new { id = response.Product!.Id, includeDeactivated = true }, response.Product);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProduct(UpdateProductRequestModel updateProductRequestModel)
    {
        try
        {
            Product product = new Product();
            product.Id = updateProductRequestModel.Id;
            product.Code = updateProductRequestModel.Code;
            product.Name = updateProductRequestModel.Name;
            product.Description = updateProductRequestModel.Description;
            product.IsDeactivated = updateProductRequestModel.IsDeactivated;
            product.ExistsInOrder = updateProductRequestModel.ExistsInOrder;
            //if null then null is inserted in variant.Categories otherewise it creates a category object for each categoryId in categoryIds
            product.Categories = updateProductRequestModel.CategoryIds?.Select(categoryId => new Category { Id = categoryId }).ToList()!;
            product.Variants = updateProductRequestModel.VariantIds?.Select(variantId => new Variant { Id = variantId }).ToList()!;

            DataLibraryReturnedCodes returnedCode = await _productDataAccess.UpdateProductAsync(product);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
            else if (returnedCode == DataLibraryReturnedCodes.DuplicateProductCode)
                return BadRequest(new { ErrorMessage = "DuplicateProductCode" });
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
    public async Task<IActionResult> DeleteProduct(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _productDataAccess.DeleteProductAsync(id);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
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
