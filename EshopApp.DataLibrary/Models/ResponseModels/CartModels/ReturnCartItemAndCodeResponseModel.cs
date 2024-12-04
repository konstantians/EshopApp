namespace EshopApp.DataLibrary.Models.ResponseModels.CartModels;
public class ReturnCartItemAndCodeResponseModel
{
    public CartItem? CartItem { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCartItemAndCodeResponseModel() { }
    public ReturnCartItemAndCodeResponseModel(CartItem cartItem, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        CartItem = cartItem;
        ReturnedCode = libraryReturnedCodes;
    }

}
