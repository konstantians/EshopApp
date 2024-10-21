namespace EshopApp.DataLibrary.Models.ResponseModels.CategoryModels;

public class ReturnCategoriesAndCodeResponseModel
{
    public List<Category> Categories { get; set; } = new List<Category>();
    public DataLibraryReturnedCodes ReturnedCode { get; set; }

    public ReturnCategoriesAndCodeResponseModel() { }
    public ReturnCategoriesAndCodeResponseModel(List<Category> categories, DataLibraryReturnedCodes libraryReturnedCodes)
    {
        foreach (var category in categories ?? Enumerable.Empty<Category>())
            Categories.Add(category);
        ReturnedCode = libraryReturnedCodes;
    }

}
