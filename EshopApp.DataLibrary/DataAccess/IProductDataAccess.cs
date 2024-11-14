using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ProductModels;

namespace EshopApp.DataLibrary.DataAccess;

public interface IProductDataAccess
{
    Task<ReturnProductAndCodeResponseModel> CreateProductAsync(Product product);
    Task<DataLibraryReturnedCodes> DeleteProductAsync(string productId);
    Task<ReturnProductAndCodeResponseModel> GetProductByIdAsync(string id, bool includeDeactivated);
    Task<ReturnProductsAndCodeResponseModel> GetProductsAsync(int amount, bool includeDeactivated);
    Task<ReturnProductsAndCodeResponseModel> GetProductsOfCategoryAsync(string categoryId, int amount, bool includeDeactivated);
    Task<DataLibraryReturnedCodes> UpdateProductAsync(Product updatedProduct);
}