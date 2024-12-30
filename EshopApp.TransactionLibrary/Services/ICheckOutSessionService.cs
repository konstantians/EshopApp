using EshopApp.TransactionLibrary.Models;
using EshopApp.TransactionLibrary.Models.ResponseModels;

namespace EshopApp.TransactionLibrary.Services;
public interface ICheckOutSessionService
{
    Task<ReturnSessionIdSessionUrlAndCodeResponseModel?> CreateCheckOutSessionAsync(CheckOutSession checkOutSession);
}