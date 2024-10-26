using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ImagesModels;
using EshopApp.DataLibraryAPI.Models.RequestModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]

public class ImageController : ControllerBase
{
    private readonly IImageDataAccess _imageDataAccess;

    public ImageController(IImageDataAccess imageDataAccess)
    {
        _imageDataAccess = imageDataAccess;
    }

    [HttpGet("Amount/{amount}/includeSoftDeleted/{includeSoftDeleted}")]
    public async Task<IActionResult> GetImages(int amount, bool includeSoftDeleted)
    {
        try
        {
            ReturnImagesAndCodeResponseModel response = await _imageDataAccess.GetImagesAsync(amount, includeSoftDeleted);
            return Ok(response.Images);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("Id/{id}/includeSoftDeleted/{includeSoftDeleted}")]
    public async Task<IActionResult> GetImageById(string id, bool includeSoftDeleted)
    {
        try
        {
            ReturnImageAndCodeResponseModel response = await _imageDataAccess.GetImageAsync(id, includeSoftDeleted);
            if (response.Image is null)
                return NotFound();

            return Ok(response.Image);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateImage(AppImage image)
    {
        try
        {
            ReturnImageAndCodeResponseModel response = await _imageDataAccess.CreateImageAsync(image);
            if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });

            return CreatedAtAction(nameof(GetImageById), new { id = response.Image!.Id }, response.Image);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateImage(AppImage image)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _imageDataAccess.UpdateImageAsync(image);
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

    [HttpPut]
    public async Task<IActionResult> RestoreDeletedImages(RestoreImagesRequestModel requestModel)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _imageDataAccess.RestoreSoftDeletedImagesAsync(requestModel.ImageIds);
            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImage(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _imageDataAccess.DeleteImageAsync(id);
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
