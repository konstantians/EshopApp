using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CartModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;
public class CartDataAccess : ICartDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<CartDataAccess> _logger;

    public CartDataAccess(AppDataDbContext appDataDbContext, ILogger<CartDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<CartDataAccess>.Instance;
    }

    public async Task<ReturnCartAndCodeResponseModel> GetCardByIdAsync(string id)
    {
        try
        {
            Cart? foundCart = await _appDataDbContext.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v!.Discount)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v!.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v!.VariantImages)
                            .ThenInclude(vi => vi.Image)
                .FirstOrDefaultAsync(cart => cart.Id == id);
            return new ReturnCartAndCodeResponseModel(foundCart!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCartByIdFailure"), ex, "An error occurred while retrieving the cart with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCartAndCodeResponseModel> GetCardOfUserAsync(string userId)
    {
        try
        {
            Cart? foundCart = await _appDataDbContext.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v!.Discount)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v!.Product)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Variant)
                        .ThenInclude(v => v!.VariantImages)
                            .ThenInclude(vi => vi.Image)
                .FirstOrDefaultAsync(cart => cart.UserId == userId);
            return new ReturnCartAndCodeResponseModel(foundCart!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetCartByUserIdFailure"), ex, "An error occurred while retrieving the cart of the user with UserId={userId}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnCartAndCodeResponseModel> CreateCartAsync(Cart cart)
    {
        try
        {

            Cart? existingCart = await _appDataDbContext.Carts.FirstOrDefaultAsync(otherCart => otherCart.UserId == cart.UserId);
            if (existingCart is not null)
                return new ReturnCartAndCodeResponseModel(null!, DataLibraryReturnedCodes.UserAlreadyHasACart);

            cart.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Carts.FirstOrDefaultAsync(otherCart => otherCart.Id == cart.Id) is not null)
                cart.Id = Guid.NewGuid().ToString();

            List<string?> existingCartItemIds = _appDataDbContext.CartItems.Select(cartItem => cartItem.Id).ToList();
            List<Variant> variants = _appDataDbContext.Variants.ToList();
            foreach (CartItem cartItem in cart.CartItems)
            {
                Variant? foundVariant = variants.FirstOrDefault(existingVariant => existingVariant.Id == cartItem.VariantId);
                if (foundVariant is null)
                    return new ReturnCartAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidVariantIdWasGiven);

                if (cartItem.Quantity > foundVariant.UnitsInStock)
                    return new ReturnCartAndCodeResponseModel(null!, DataLibraryReturnedCodes.InsufficientStockForVariant);

                cartItem.Id = Guid.NewGuid().ToString();
                while (existingCartItemIds.Contains(cartItem.Id))
                    cartItem.Id = Guid.NewGuid().ToString();
                cartItem.CartId = cart.Id; //this is not needed, but I want this to be explicit
            }

            DateTime dateTimeNow = DateTime.Now;
            cart.CreatedAt = dateTimeNow;
            await _appDataDbContext.Carts.AddAsync(cart);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateCartSuccess"), "The cart was successfully created.");
            return new ReturnCartAndCodeResponseModel(cart, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateCartFailure"), ex, "An error occurred while creating cart. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    //this could update the quantity too, but it less efficient(when the user adds something to the cart this is what triggers)
    public async Task<ReturnCartItemAndCodeResponseModel> CreateCartItemAsync(CartItem newCartItem)
    {
        try
        {
            Variant? foundVariant = await _appDataDbContext.Variants.FirstOrDefaultAsync(variant => variant.Id == newCartItem.VariantId);
            if (foundVariant is null)
                return new ReturnCartItemAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidVariantIdWasGiven);

            Cart? foundCart = await _appDataDbContext.Carts
                .Include(cart => cart.CartItems)
                    .ThenInclude(cartItem => cartItem.Variant)
                .FirstOrDefaultAsync(cart => cart.Id == newCartItem.CartId);
            if (foundCart is null)
                return new ReturnCartItemAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidCartIdWasGiven);

            CartItem? existingCartItemInCartWithGivenVariant = foundCart.CartItems.FirstOrDefault(cartItem => cartItem.Variant!.Id == newCartItem.VariantId);
            if (existingCartItemInCartWithGivenVariant is not null)
            {
                if (existingCartItemInCartWithGivenVariant.Quantity + newCartItem.Quantity > foundVariant.UnitsInStock)
                    return new ReturnCartItemAndCodeResponseModel(null!, DataLibraryReturnedCodes.InsufficientStockForVariant);

                existingCartItemInCartWithGivenVariant.Quantity += newCartItem.Quantity;
                await _appDataDbContext.SaveChangesAsync();
                _logger.LogInformation(new EventId(9999, "CreateCartItemSuccess"), "The cart item already existed in the cart with CartId={cartId} and thus only the quantity was updated.", newCartItem.CartId);
                return new ReturnCartItemAndCodeResponseModel(existingCartItemInCartWithGivenVariant, DataLibraryReturnedCodes.VariantAlreadyInCartAndThusOnlyTheQuantityValueWasAdjusted);
            }

            if (newCartItem.Quantity > foundVariant.UnitsInStock)
                return new ReturnCartItemAndCodeResponseModel(null!, DataLibraryReturnedCodes.InsufficientStockForVariant);

            newCartItem.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.CartItems.FirstOrDefaultAsync(otherCart => otherCart.Id == newCartItem.Id) is not null)
                newCartItem.Id = Guid.NewGuid().ToString();

            await _appDataDbContext.CartItems.AddAsync(newCartItem);
            await _appDataDbContext.SaveChangesAsync();
            _logger.LogInformation(new EventId(9999, "CreateCartItemSuccess"), "The cart item was successfully created and added to the cart with CartId={cartId}.", newCartItem.CartId);
            return new ReturnCartItemAndCodeResponseModel(newCartItem, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateCartItemFailure"), ex, "An error occurred while creating the cartItem. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    //this could update the quantity too, but it less efficient(when the user adds something to the cart this is what triggers)
    public async Task<DataLibraryReturnedCodes> CreateCartItemsAsync(List<CartItem> newCartItems)
    {
        try
        {
            if (newCartItems.Count == 0)
                return DataLibraryReturnedCodes.NoError;

            string? cartId = newCartItems[0].CartId;

            // Ensure all items target same cart
            if (newCartItems.Any(x => x.CartId != cartId))
                return DataLibraryReturnedCodes.InvalidCartIdWasGiven;

            Cart? foundCart = await _appDataDbContext.Carts.Include(cart => cart.CartItems).FirstOrDefaultAsync(cart => cart.Id == cartId);

            if (foundCart is null)
                return DataLibraryReturnedCodes.InvalidCartIdWasGiven;

            List<string?> variantIds = newCartItems.Select(newCartItem => newCartItem.VariantId).Distinct().ToList();

            Dictionary<string, Variant> variants = await _appDataDbContext.Variants.Where(variant => variantIds.Contains(variant.Id!)).ToDictionaryAsync(variant => variant.Id!);

            if (variants.Count != variantIds.Count)
                return DataLibraryReturnedCodes.InvalidVariantIdWasGiven;

            foreach (var newCartItem in newCartItems)
            {
                //skip all those who do not have variantId
                if (newCartItem.VariantId is null)
                    continue;

                Variant variant = variants[newCartItem.VariantId];

                CartItem? existingItem = foundCart.CartItems.FirstOrDefault(ci => ci.VariantId == newCartItem.VariantId);

                if (existingItem is not null)
                {
                    if (existingItem.Quantity + newCartItem.Quantity > variant.UnitsInStock)
                        return DataLibraryReturnedCodes.InsufficientStockForVariant;

                    existingItem.Quantity += newCartItem.Quantity;
                }
                else
                {
                    if (newCartItem.Quantity > variant.UnitsInStock)
                        return DataLibraryReturnedCodes.InsufficientStockForVariant;

                    newCartItem.Id = Guid.NewGuid().ToString();
                    foundCart.CartItems.Add(newCartItem);
                }
            }

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateCartItemsSuccess"), "Successfully added cart items to CartId={cartId}", cartId);

            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateCartItemsFailure"), ex, "Error creating cart items.");
            throw;
        }
    }

    //this is more efficient if the change only happens on the quantity
    public async Task<DataLibraryReturnedCodes> UpdateCartItemAsync(CartItem updatedCartItem)
    {
        try
        {
            if (updatedCartItem.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            CartItem? foundCartItem = await _appDataDbContext.CartItems
                .Include(cartItem => cartItem.Variant)
                .FirstOrDefaultAsync(cartItem => cartItem.Id == updatedCartItem.Id);

            if (foundCartItem is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateCartItemFailureDueToNullCart"), "The cart item with Id={id} was not found and thus the update could not proceed.", updatedCartItem.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (updatedCartItem.Quantity is not null && updatedCartItem.Quantity > foundCartItem.Variant!.UnitsInStock)
                return DataLibraryReturnedCodes.InsufficientStockForVariant;

            foundCartItem.Quantity = updatedCartItem.Quantity ?? foundCartItem.Quantity;

            await _appDataDbContext.SaveChangesAsync();
            _logger.LogInformation(new EventId(9999, "UpdateCartItemSuccess"), "The cart item with Id={id} was successfully updated.", updatedCartItem.Id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateCartItemFailure"), ex, "An error occurred while updating the cart item with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedCartItem.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteCartItemAsync(string id)
    {
        try
        {
            CartItem? foundCartItem = await _appDataDbContext.CartItems.FirstOrDefaultAsync(cart => cart.Id == id);
            if (foundCartItem is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteCartItemFailureDueToNullCartItem"), "The cart item with Id={id} was not found and thus the deletion could not proceed.", id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.CartItems.Remove(foundCartItem);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteCartItemSuccess"), "The cart item with Id={id} was successfully deleted.", id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteCartItemFailure"), ex, "An error occurred while deleting the cart item with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> ClearCartAsync(string cartId)
    {
        try
        {
            List<CartItem> cartItems = await _appDataDbContext.CartItems.Where(cartItem => cartItem.CartId == cartId).ToListAsync();
            if (!cartItems.Any())
            {
                _logger.LogInformation(new EventId(9999, "ClearCartAlreadyEmpty"), "No cart item found for cart with Id={cartId}. That means there was a logical error or the cart was already empty", cartId);
                return DataLibraryReturnedCodes.NoError;
            }

            _appDataDbContext.CartItems.RemoveRange(cartItems);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "ClearCartSuccess"), "All cart items for cart with Id={cartId} were successfully removed.", cartId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "ClearCartFailure"), ex, "An error occurred while clearing the cart with Id={cartId}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", cartId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteCartByIdAsync(string id)
    {
        try
        {
            Cart? foundCart = await _appDataDbContext.Carts.FirstOrDefaultAsync(cart => cart.Id == id);
            if (foundCart is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteCartFailureByIdDueToNullCart"), "The cart with Id={id} was not found and thus the deletion could not proceed.", id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.Carts.Remove(foundCart);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteCartByIdSuccess"), "The cart with Id={id} was successfully deleted.", id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteCartByIdFailure"), ex, "An error occurred while deleting the cart with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteUserCartAsync(string userId)
    {
        try
        {
            Cart? foundCart = await _appDataDbContext.Carts.FirstOrDefaultAsync(cart => cart.UserId == userId);
            if (foundCart is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteUserCartFailureDueToNullCart"), "The cart of the user with UserId={userId} was not found and thus the deletion could not proceed.", userId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.Carts.Remove(foundCart);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteUserCartSuccess"), "The cart of the user with UserId={userId} was successfully deleted.", userId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteUserCartFailure"), ex, "An error occurred while deleting the cart of the user with UserId={userId}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
