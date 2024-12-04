namespace EshopApp.DataLibrary.Models.ResponseModels.CartModels;
public class ReturnCartAndCodeResponseModel
{
    public Cart? Cart { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCartAndCodeResponseModel() { }
    public ReturnCartAndCodeResponseModel(Cart cart, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Cart = cart;
        ReturnedCode = libraryReturnedCodes;
    }

}
