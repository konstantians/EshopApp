using EshopApp.TransactionLibrary.Models;
using EshopApp.TransactionLibrary.Models.ResponseModels;

namespace EshopApp.TransactionLibrary.Services;
public interface ICheckOutSessionService
{
    Task<ReturnCheckOutSessionIdAndCodeResponseModel?> CreateCheckOutSessionAsync(CheckOutSession checkOutSession);
}