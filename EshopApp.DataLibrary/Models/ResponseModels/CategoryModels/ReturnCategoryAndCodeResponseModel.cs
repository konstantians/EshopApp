namespace EshopApp.DataLibrary.Models.ResponseModels.CategoryModels;

public class ReturnCategoryAndCodeResponseModel
{
    public Category? Category { get; set; }
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCategoryAndCodeResponseModel() { }
    public ReturnCategoryAndCodeResponseModel(Category category, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        Category = category;
        ReturnedCode = libraryReturnedCodes;
    }
}
