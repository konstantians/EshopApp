﻿using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.AttributeModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]

public class AttributeController : ControllerBase
{
    private readonly IAttributeDataAccess _attributeDataAccess;

    public AttributeController(IAttributeDataAccess attributeDataAccess)
    {
        _attributeDataAccess = attributeDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetAttributes(int amount)
    {
        try
        {
            ReturnAttributesAndCodeResponseModel response = await _attributeDataAccess.GetAttributesAsync(amount);
            return Ok(response.Attributes);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAttributeById(string id)
    {
        try
        {
            ReturnAttributeAndCodeResponseModel response = await _attributeDataAccess.GetAttributeByIdAsync(id);
            if (response.Attribute is null)
                return NotFound();

            return Ok(response.Attribute);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateAttribute(AppAttribute attribute)
    {
        try
        {
            ReturnAttributeAndCodeResponseModel response = await _attributeDataAccess.CreateAttributeAsync(attribute);
            if (response.ReturnedCode == DataLibraryReturnedCodes.DuplicateEntityName)
                return BadRequest(new { ErrorMessage = "DuplicateEntityName" });

            return CreatedAtAction(nameof(GetAttributeById), new { id = response.Attribute!.Id }, response.Attribute);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateAttribute(AppAttribute attribute)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _attributeDataAccess.UpdateAttributeAsync(attribute);
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
    public async Task<IActionResult> DeleteAttribute(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _attributeDataAccess.DeleteAttributeAsync(id);
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
