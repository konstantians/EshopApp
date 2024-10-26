using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ProductModels;
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

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetProducts(int amount)
    {
        try
        {
            ReturnProductsAndCodeResponseModel response = await _productDataAccess.GetProductsAsync(amount);
            return Ok(response.Products);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Category/{category}/Amount/{amount}")]
    public async Task<IActionResult> GetProductsByCategory(string category, int amount)
    {
        try
        {
            ReturnProductsAndCodeResponseModel response = await _productDataAccess.GetProductsOfCategoryAsync(category, amount);
            return Ok(response.Products);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        try
        {
            ReturnProductAndCodeResponseModel response = await _productDataAccess.GetProductByIdAsync(id);
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
    public async Task<IActionResult> CreateProduct(Product product)
    {
        try
        {
            ReturnProductAndCodeResponseModel response = await _productDataAccess.CreateProductAsync(product);
            if (response.ReturnedCode == DataLibraryReturnedCodes.NoVariantWasProvidedForProductCreation)
                return BadRequest(new { ErrorMessage = "NoVariantWasProvidedForProductCreation" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateProductCode)
                return BadRequest(new { ErrorMessage = "DuplicateProductCode" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });

            return CreatedAtAction(nameof(GetProductById), new { id = response.Product!.Id }, response.Product);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProduct(Product product)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _productDataAccess.UpdateProductAsync(product);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "EntityNotFoundWithGivenId" });
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
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return BadRequest(new { ErrorMessage = "EntityNotFoundWithGivenId" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
