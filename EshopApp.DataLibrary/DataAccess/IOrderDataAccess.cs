using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.OrderModels;

namespace EshopApp.DataLibrary.DataAccess;
public interface IOrderDataAccess
{
    Task<ReturnOrderAndCodeResponseModel> CreateOrderAsync(Order order, bool isFinal = false);
    Task<DataLibraryReturnedCodes> DeleteOrderAsync(string orderId);
    Task<ReturnOrderAndCodeResponseModel> GetOrderByIdAsync(string id);
    Task<ReturnOrderAndCodeResponseModel> GetOrderByPaymentProcessorPaymentIntentIdAsync(string orderPaymentProcessorPaymentIntentId);
    Task<ReturnOrderAndCodeResponseModel> GetOrderByPaymentProcessorSessionIdAsync(string orderPaymentProcessorSessionId);
    Task<ReturnOrdersAndCodeResponseModel> GetOrdersAsync(int amount);
    Task<ReturnOrdersAndCodeResponseModel> GetUserOrdersAsync(int amount, string userId);
    Task<DataLibraryReturnedCodes> UpdateOrderAsync(Order updatedOrder);
    Task<DataLibraryReturnedCodes> UpdateOrderStatusAsync(string newOrderStatus, string orderId, string orderPaymentProcessorSessionId);
}