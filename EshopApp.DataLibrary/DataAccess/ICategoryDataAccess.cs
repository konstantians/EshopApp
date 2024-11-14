using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.CategoryModels;

namespace EshopApp.DataLibrary.DataAccess;

public interface ICategoryDataAccess
{
    Task<ReturnCategoryAndCodeResponseModel> CreateCategoryAsync(Category category);
    Task<DataLibraryReturnedCodes> DeleteCategoryAsync(string categoryId);
    Task<ReturnCategoriesAndCodeResponseModel> GetCategoriesAsync(int amount);
    Task<ReturnCategoryAndCodeResponseModel> GetCategoryByIdAsync(string id);
    Task<DataLibraryReturnedCodes> UpdateCategoryAsync(Category updatedCategory);
}