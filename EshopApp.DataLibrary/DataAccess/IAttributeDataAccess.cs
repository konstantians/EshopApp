using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.AttributeModels;

namespace EshopApp.DataLibrary.DataAccess;

public interface IAttributeDataAccess
{
    Task<ReturnAttributeAndCodeResponseModel> CreateAttributeAsync(AppAttribute attribute);
    Task<DataLibraryReturnedCodes> DeleteAttributeAsync(string attributeId);
    Task<ReturnAttributeAndCodeResponseModel> GetAttributeByIdAsync(string id);
    Task<ReturnAttributesAndCodeResponseModel> GetAttributesAsync(int amount);
    Task<DataLibraryReturnedCodes> UpdateAttributeAsync(AppAttribute updatedAttribute);
}