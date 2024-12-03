using EshopApp.DataLibrary.DataAccess;
using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.OrderModels;
using EshopApp.DataLibraryAPI.Models.RequestModels.OrderModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.DataLibraryAPI.Controllers;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IOrderDataAccess _orderDataAccess;

    public OrderController(IOrderDataAccess orderDataAccess)
    {
        _orderDataAccess = orderDataAccess;
    }

    [HttpGet("Amount/{amount}")]
    public async Task<IActionResult> GetOrders(int amount)
    {
        try
        {
            ReturnOrdersAndCodeResponseModel response = await _orderDataAccess.GetOrdersAsync(amount);
            return Ok(response.Orders);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrderById(string id)
    {
        try
        {
            ReturnOrderAndCodeResponseModel response = await _orderDataAccess.GetOrderByIdAsync(id);
            if (response.Order is null)
                return NotFound();

            return Ok(response.Order);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequestModel createOrderRequestModel)
    {
        try
        {
            Order order = new Order();
            order.Comment = createOrderRequestModel.Comment;
            order.UserId = createOrderRequestModel.UserId;
            order.ShippingOptionId = createOrderRequestModel.ShippingOptionId;
            order.UserCouponId = createOrderRequestModel.UserCouponId;

            OrderAddress orderAddress = new OrderAddress();
            orderAddress.Email = createOrderRequestModel.Email;
            orderAddress.FirstName = createOrderRequestModel.FirstName;
            orderAddress.LastName = createOrderRequestModel.LastName;
            orderAddress.Country = createOrderRequestModel.Country;
            orderAddress.City = createOrderRequestModel.City;
            orderAddress.PostalCode = createOrderRequestModel.PostalCode;
            orderAddress.Address = createOrderRequestModel.Address;
            orderAddress.PhoneNumber = createOrderRequestModel.PhoneNumber;
            orderAddress.IsShippingAddressDifferent = createOrderRequestModel.IsShippingAddressDifferent;
            orderAddress.AltFirstName = createOrderRequestModel.AltFirstName;
            orderAddress.AltLastName = createOrderRequestModel.AltLastName;
            orderAddress.AltCountry = createOrderRequestModel.AltCountry;
            orderAddress.AltCity = createOrderRequestModel.AltCity;
            orderAddress.AltPostalCode = createOrderRequestModel.AltPostalCode;
            orderAddress.AltAddress = createOrderRequestModel.AltAddress;
            orderAddress.AltPhoneNumber = createOrderRequestModel.AltPhoneNumber;

            PaymentDetails paymentDetails = new PaymentDetails()
            {
                PaymentProcessorSessionId = createOrderRequestModel.PaymentProcessorSessionId,
                PaymentOptionId = createOrderRequestModel.PaymentOptionId
            };

            foreach (OrderItemRequestModel createOrderItemRequestModel in createOrderRequestModel.OrderItemRequestModels)
            {
                OrderItem orderItem = new OrderItem();
                orderItem.Quantity = createOrderItemRequestModel.Quantity;
                orderItem.VariantId = createOrderItemRequestModel.VariantId;
                order.OrderItems.Add(orderItem);
            }

            order.OrderAddress = orderAddress;
            order.PaymentDetails = paymentDetails;

            ReturnOrderAndCodeResponseModel response = await _orderDataAccess.CreateOrderAsync(order);
            if (response.ReturnedCode == DataLibraryReturnedCodes.TheOrderMustHaveAtLeastOneOrderItem)
                return BadRequest(new { ErrorMessage = "TheOrderMustHaveAtLeastOneOrderItem" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.AllTheAltFieldsNeedToBeFilledIfDifferentShippingAddress)
                return BadRequest(new { ErrorMessage = "AllTheAltFieldsNeedToBeFilledIfDifferentShippingAddress" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidPaymentOption)
                return NotFound(new { ErrorMessage = "InvalidPaymentOption" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidShippingOption)
                return NotFound(new { ErrorMessage = "InvalidShippingOption" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidCouponIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidCouponIdWasGiven" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.CouponCodeCurrentlyDeactivated)
                return BadRequest(new { ErrorMessage = "CouponCodeCurrentlyDeactivated" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.CouponUsageLimitExceeded)
                return BadRequest(new { ErrorMessage = "CouponUsageLimitExceeded" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.InvalidVariantIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidVariantIdWasGiven" });
            else if (response.ReturnedCode == DataLibraryReturnedCodes.InsufficientStockForVariant)
                return BadRequest(new { ErrorMessage = "InsufficientStockForVariant" });

            return CreatedAtAction(nameof(GetOrderById), new { id = response.Order!.Id }, response.Order);
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut]
    public async Task<IActionResult> UpdateOrder(UpdateOrderRequestModel updateOrderRequestModel)
    {
        try
        {
            Order order = new Order();
            order.Id = updateOrderRequestModel.Id;
            order.UserCouponId = updateOrderRequestModel.UserCouponId;

            OrderAddress orderAddress = new OrderAddress();
            orderAddress.Email = updateOrderRequestModel.Email;
            orderAddress.FirstName = updateOrderRequestModel.FirstName;
            orderAddress.LastName = updateOrderRequestModel.LastName;
            orderAddress.Country = updateOrderRequestModel.Country;
            orderAddress.City = updateOrderRequestModel.City;
            orderAddress.PostalCode = updateOrderRequestModel.PostalCode;
            orderAddress.Address = updateOrderRequestModel.Address;
            orderAddress.PhoneNumber = updateOrderRequestModel.PhoneNumber;
            orderAddress.IsShippingAddressDifferent = updateOrderRequestModel.IsShippingAddressDifferent;
            orderAddress.AltFirstName = updateOrderRequestModel.AltFirstName;
            orderAddress.AltLastName = updateOrderRequestModel.AltLastName;
            orderAddress.AltCountry = updateOrderRequestModel.AltCountry;
            orderAddress.AltCity = updateOrderRequestModel.AltCity;
            orderAddress.AltPostalCode = updateOrderRequestModel.AltPostalCode;
            orderAddress.AltAddress = updateOrderRequestModel.AltAddress;
            orderAddress.AltPhoneNumber = updateOrderRequestModel.AltPhoneNumber;

            PaymentDetails paymentDetails = new PaymentDetails()
            {
                PaymentProcessorSessionId = updateOrderRequestModel.PaymentProcessorSessionId,
                PaymentStatus = updateOrderRequestModel.PaymentStatus,
                PaymentCurrency = updateOrderRequestModel.PaymentCurrency,
                AmountPaidInCustomerCurrency = updateOrderRequestModel.AmountPaidInCustomerCurrency,
                AmountPaidInEuro = updateOrderRequestModel.AmountPaidInEuro,
                NetAmountPaidInEuro = updateOrderRequestModel.NetAmountPaidInEuro
            };

            foreach (OrderItemRequestModel createOrderItemRequestModel in updateOrderRequestModel.OrderItemRequestModels ?? Enumerable.Empty<OrderItemRequestModel>())
            {
                OrderItem orderItem = new OrderItem();
                orderItem.Quantity = createOrderItemRequestModel.Quantity;
                orderItem.VariantId = createOrderItemRequestModel.VariantId;
                order.OrderItems.Add(orderItem);
            }

            order.OrderAddress = orderAddress;
            order.PaymentDetails = paymentDetails;

            DataLibraryReturnedCodes returnedCode = await _orderDataAccess.UpdateOrderAsync(order);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
            else if (returnedCode == DataLibraryReturnedCodes.OrderNotFoundWithGivenPaymentProcessorSessionId)
                return NotFound(new { ErrorMessage = "OrderNotFoundWithGivenPaymentProcessorSessionId" });
            else if (returnedCode == DataLibraryReturnedCodes.TheOrderIdAndThePaymentProcessorSessionIdCanNotBeBothNull)
                return BadRequest(new { ErrorMessage = "TheOrderIdAndThePaymentProcessorSessionIdCanNotBeBothNull" });
            else if (returnedCode == DataLibraryReturnedCodes.OrderStatusHasBeenFinalizedAndThusTheOrderCanNotBeAltered)
                return BadRequest(new { ErrorMessage = "OrderStatusHasBeenFinalizedAndThusTheOrderCanNotBeAltered" });
            else if (returnedCode == DataLibraryReturnedCodes.InvalidCouponIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidCouponIdWasGiven" });
            else if (returnedCode == DataLibraryReturnedCodes.CouponCodeCurrentlyDeactivated)
                return BadRequest(new { ErrorMessage = "CouponCodeCurrentlyDeactivated" });
            else if (returnedCode == DataLibraryReturnedCodes.CouponUsageLimitExceeded)
                return BadRequest(new { ErrorMessage = "CouponUsageLimitExceeded" });
            else if (returnedCode == DataLibraryReturnedCodes.InvalidVariantIdWasGiven)
                return NotFound(new { ErrorMessage = "InvalidVariantIdWasGiven" });
            else if (returnedCode == DataLibraryReturnedCodes.InsufficientStockForVariant)
                return BadRequest(new { ErrorMessage = "InsufficientStockForVariant" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpPut("UpdateOrderStatus")]
    public async Task<IActionResult> UpdateOrderStatus(UpdateOrderStatusRequestModel updateOrderStatusRequestModels)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _orderDataAccess.UpdateOrderStatusAsync(updateOrderStatusRequestModels.NewOrderStatus!, updateOrderStatusRequestModels.OrderId!);
            if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound(new { ErrorMessage = "EntityNotFoundWithGivenId" });
            else if (returnedCode == DataLibraryReturnedCodes.OrderStatusHasBeenFinalizedAndThusOrderStatusCanNotBeAltered)
                return BadRequest(new { ErrorMessage = "OrderStatusHasBeenFinalizedAndThusOrderStatusCanNotBeAltered" });
            else if (returnedCode == DataLibraryReturnedCodes.InvalidOrderStatus)
                return BadRequest(new { ErrorMessage = "InvalidOrderState" });
            else if (returnedCode == DataLibraryReturnedCodes.InvalidNewOrderState)
                return BadRequest(new { ErrorMessage = "InvalidNewOrderState" });
            else if (returnedCode == DataLibraryReturnedCodes.ThisOrderDoesNotContainShippingAndThusTheShippedStatusIsInvalid)
                return BadRequest(new { ErrorMessage = "ThisOrderDoesNotContainShippingAndThusTheShippedStatusIsInvalid" });

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteOrder(string id)
    {
        try
        {
            DataLibraryReturnedCodes returnedCode = await _orderDataAccess.DeleteOrderAsync(id);
            if (returnedCode == DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull)
                return BadRequest(new { ErrorMessage = "TheIdOfTheEntityCanNotBeNull" });
            else if (returnedCode == DataLibraryReturnedCodes.EntityNotFoundWithGivenId)
                return NotFound();

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
