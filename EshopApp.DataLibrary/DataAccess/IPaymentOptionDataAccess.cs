using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.PaymentOptionModels;

namespace EshopApp.DataLibrary.DataAccess;
public interface IPaymentOptionDataAccess
{
    Task<ReturnPaymentOptionAndCodeResponseModel> CreatePaymentOptionAsync(PaymentOption paymentOption);
    Task<DataLibraryReturnedCodes> DeletePaymentOptionAsync(string paymentOptionId);
    Task<ReturnPaymentOptionAndCodeResponseModel> GetPaymentOptionByIdAsync(string id, bool includeDeactivated);
    Task<ReturnPaymentOptionsAndCodeResponseModel> GetPaymentOptionsAsync(int amount, bool includeDeactivated);
    Task<DataLibraryReturnedCodes> UpdatePaymentOptionAsync(PaymentOption updatedPaymentOption);
}