using EshopApp.TransactionLibrary.Models;
using EshopApp.TransactionLibrary.Models.ResponseModels;

namespace EshopApp.TransactionLibrary.Services;
public interface ICheckOutSessionService
{
    Task<HandleCheckOutSessionEventResponseModel?> CreateCheckOutSessionAsync(CheckOutSession checkOutSession);
}