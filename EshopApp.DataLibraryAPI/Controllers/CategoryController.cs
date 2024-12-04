using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CategoryModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.CategoryModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly ICategoryDataAccess _categoryDataAccess;

    public CategoryController(ICategoryDataAccess categoryDataAccess)
    {
        _categoryDataAccess = categoryDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetCategories(int amount)
    {
        try
        {
            ReturnCategoriesAndCodeResponseModel response = await _categoryDataAccess.GetCategoriesAsync(amount);
            return Ok(response.Categories);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCategoryById(string id)
    {
        try
        {
            ReturnCategoryAndCodeResponseModel response = await _categoryDataAccess.GetCategoryByIdAsync(id);
            if (response.Category is null)
                return NotFound();

            return Ok(response.Category);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCategory(CreateCategoryRequestModel createCategoryRequestModel)
    {
        try
        {
            Category category = new Category();
            category.Name = createCategoryRequestModel.Name;
            ReturnCategoryAndCodeResponseModel response = await _categoryDataAccess.CreateCategoryAsync(category);
            if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });

            return CreatedAtAction(nameof(GetCategoryById), new { id = response.Category!.Id }, response.Category);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCategory(UpdateCategoryRequestModel updateCategoryRequestModel)
    {
        try
        {
            Category category = new Category();
            category.Id = updateCategoryRequestModel.Id;
            category.Name = updateCategoryRequestModel.Name;
            category.Products = updateCategoryRequestModel.ProductIds?.Select(productId => new Product { Id = productId }).ToList()!;

            DataLibraryReturnedCodes returnedCode = await _categoryDataAccess.UpdateCategoryAsync(category);
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
    public async Task<IActionResult> DeleteCategory(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _categoryDataAccess.DeleteCategoryAsync(id);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound();

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
