using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ProductModels;

namespace EshopApp.DataLibrary.DataAccess
{
    public interface IProductDataAccess
    {
        Task<ReturnProductAndCodeResponseModel> CreateProductAsync(Product product);
        Task<DataLibraryReturnedCodes> DeleteProductAsync(string productId);
        Task<ReturnProductAndCodeResponseModel> GetProductByIdAsync(string id);
        Task<ReturnProductsAndCodeResponseModel> GetProductsAsync(int amount);
        Task<ReturnProductsAndCodeResponseModel> GetProductsOfCategoryAsync(string categoryId, int amount);
        Task<DataLibraryReturnedCodes> UpdateProductAsync(Product updatedProduct);
    }
}