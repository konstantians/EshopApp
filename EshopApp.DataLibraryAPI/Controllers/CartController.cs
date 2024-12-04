using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CartModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.CartModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartDataAccess _cartDataAccess;

    public CartController(ICartDataAccess cartDataAccess)
    {
        _cartDataAccess = cartDataAccess;
    }

    [HttpGet("Id/{id}")]
    public async Task<IActionResult> GetCartById(string id)
    {
        try
        {
            ReturnCartAndCodeResponseModel response = await _cartDataAccess.GetCardByIdAsync(id);
            if (response.Cart is null)
                return NotFound();

            return Ok(response.Cart);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("UserId/{userId}")]
    public async Task<IActionResult> GetCartOfUser(string userId)
    {
        try
        {
            ReturnCartAndCodeResponseModel response = await _cartDataAccess.GetCardOfUserAsync(userId);
            if (response.Cart is null)
                return NotFound();

            return Ok(response.Cart);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateCart(CreateCartRequestModel createCartRequestModel)
    {
        try
        {
            Cart cart = new Cart();

            cart.UserId = createCartRequestModel.UserId;
            List<CartItem> cartItems = new List<CartItem>();
            foreach (CreateCartItemRequestModel createCartItemRequestModel in createCartRequestModel.CreateCartItemRequestModels)
                cartItems.Add(new CartItem() { Quantity = createCartItemRequestModel.Quantity, VariantId = createCartItemRequestModel.VariantId });

            cart.CartItems = cartItems;

            ReturnCartAndCodeResponseModel response = await _cartDataAccess.CreateCartAsync(cart);
            if (response.ReturnedCode == DataLibraryReturnedCodes.UserAlreadyHasACart)
                return BadRequest(new { ErrorMessage = "UserAlreadyHasACart" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidVariantIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidVariantIdWasGiven" });

            return CreatedAtAction(nameof(GetCartById), new { id = response.Cart!.Id }, response.Cart);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost("CartItem")]
    public async Task<IActionResult> CreateCartItem(CreateCartItemRequestModel createCartItemRequestModel)
    {
        try
        {
            CartItem cartItem = new CartItem();
            cartItem.CartId = createCartItemRequestModel.CartId;
            cartItem.VariantId = createCartItemRequestModel.VariantId;
            cartItem.Quantity = createCartItemRequestModel.Quantity;

            ReturnCartItemAndCodeResponseModel response = await _cartDataAccess.CreateCartItemAsync(cartItem);
            if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidVariantIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidVariantIdWasGiven" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidCartIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidCartIdWasGiven" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.VariantAlreadyInCartAndThusOnlyTheQuantityValueWasAdjusted)
                return Ok(response.CartItem);

            return Created("", response.CartItem);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut("CartItem")]
    public async Task<IActionResult> UpdateCartItem(UpdateCartItemRequestModel updateCartItemRequestModel)
    {
        try
        {
            CartItem cartItem = new CartItem();
            cartItem.CartId = updateCartItemRequestModel.CartId;
            cartItem.Quantity = updateCartItemRequestModel.Quantity;
            cartItem.VariantId = updateCartItemRequestModel.VariantId;

            DataLibraryReturnedCodes returnedCode = await _cartDataAccess.UpdateCartItemAsync(cartItem);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("CartItem/{id}")]
    public async Task<IActionResult> DeleteCartItem(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _cartDataAccess.DeleteCartItemAsync(id);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound();

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("Id/{id}")]
    public async Task<IActionResult> DeleteCartById(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _cartDataAccess.DeleteCartByIdAsync(id);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound();

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("UserId/{userId}")]
    public async Task<IActionResult> DeleteCartByUserId(string userId)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _cartDataAccess.DeleteUserCartAsync(userId);
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
