using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CartModels;

namespace EshopApp.DataLibrary.DataAccess;
public interface ICartDataAccess
{
    Task<ReturnCartAndCodeResponseModel> CreateCartAsync(Cart cart);
    Task<ReturnCartItemAndCodeResponseModel> CreateCartItemAsync(CartItem newCartItem);
    Task<DataLibraryReturnedCodes> DeleteCartByIdAsync(string id);
    Task<DataLibraryReturnedCodes> DeleteCartItemAsync(string id);
    Task<DataLibraryReturnedCodes> DeleteUserCartAsync(string userId);
    Task<ReturnCartAndCodeResponseModel> GetCardByIdAsync(string id);
    Task<ReturnCartAndCodeResponseModel> GetCardOfUserAsync(string userId);
    Task<DataLibraryReturnedCodes> UpdateCartItemAsync(CartItem updatedCartItem);
}