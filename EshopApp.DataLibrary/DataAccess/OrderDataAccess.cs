using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.OrderModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Globalization;

namespace EshopApp.DataLibrary.DataAccess;
public class OrderDataAccess : IOrderDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<OrderDataAccess> _logger;

    public OrderDataAccess(AppDataDbContext appDataDbContext, ILogger<OrderDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<OrderDataAccess>.Instance;
    }

    public async Task<ReturnOrdersAndCodeResponseModel> GetOrdersAsync(int amount)
    {
        try
        {
            List<Order> orders = await _appDataDbContext.Orders
                .Include(o => o.OrderAddress)
                .Include(o => o.PaymentDetails)
                    .ThenInclude(pd => pd!.PaymentOption)
                .Include(o => o.ShippingOption)
                .Include(o => o.UserCoupon)
                    .ThenInclude(uc => uc!.Coupon)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Discount)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Image)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v!.Product)
                .Take(amount)
                .ToListAsync();

            return new ReturnOrdersAndCodeResponseModel(orders, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetOrdersFailure"), ex, "An error occurred while retrieving the orders. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnOrdersAndCodeResponseModel> GetUserOrdersAsync(int amount, string userId)
    {
        try
        {
            List<Order> orders = await _appDataDbContext.Orders
                .Include(o => o.OrderAddress)
                .Include(o => o.PaymentDetails)
                    .ThenInclude(pd => pd!.PaymentOption)
                .Include(o => o.ShippingOption)
                .Include(po => po.UserCoupon)
                    .ThenInclude(uc => uc!.Coupon)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Discount)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Image)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v!.Product)
                .Where(order => order.UserId == userId)
                .Take(amount)
                .ToListAsync();

            return new ReturnOrdersAndCodeResponseModel(orders, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetUserOrdersFailure"), ex, "An error occurred while retrieving the orders of the user with UserId={userId}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", userId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnOrderAndCodeResponseModel> GetOrderByIdAsync(string id)
    {
        try
        {
            Order? order = await _appDataDbContext.Orders
                .Include(o => o.OrderAddress)
                .Include(o => o.PaymentDetails)
                    .ThenInclude(pd => pd!.PaymentOption)
                .Include(o => o.ShippingOption)
                .Include(o => o.UserCoupon)
                    .ThenInclude(uc => uc!.Coupon)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Discount)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Image)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Variant)
                        .ThenInclude(v => v!.Product)
                .FirstOrDefaultAsync(order => order.Id == id);

            return new ReturnOrderAndCodeResponseModel(order!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetOrderByIdFailure"), ex, "An error occurred while retrieving the order with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnOrderAndCodeResponseModel> CreateOrderAsync(Order order)
    {
        try
        {
            decimal? finalPrice = 0;
            DateTime dateTimeNow = DateTime.Now;
            //this if should never fail, but exists here as a fail safe
            if (order.PaymentDetails is null)
                return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.ThePaymentDetailsObjectCanNotBeNull);
            else if (order.PaymentDetails.PaymentOptionId is null)
                return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.TheIdOfThePaymentOptionCanNotBeNull);
            else if (order.ShippingOptionId is null)
                return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.TheIdOfTheShippingOptionCanNotBeNull);
            else if (order.OrderItems is null || !order.OrderItems.Any())
                return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.TheOrderMustHaveAtLeastOneOrderItem);
            else if (order.OrderAddress is null)
                return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.TheOrderAddressObjectCanNotBeNull);

            order.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Orders.FirstOrDefaultAsync(otherOrder => otherOrder.Id == order.Id) is not null)
                order.Id = Guid.NewGuid().ToString();

            order.Comment = order.Comment ?? "";
            order.OrderStatus = "Pending";
            order.CreatedAt = dateTimeNow;
            order.ModifiedAt = dateTimeNow;
            order.PaymentDetails.PaymentStatus = "Pending";
            order.PaymentDetails.PaymentCurrency = "N/A"; //this will change after payment
            order.PaymentDetails!.AmountPaidInEuro = 0;
            order.PaymentDetails!.NetAmountPaidInEuro = 0;
            order.OrderAddress.IsShippingAddressDifferent = order.OrderAddress.IsShippingAddressDifferent ?? false;

            order.OrderAddress.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.OrderAddresses.FirstOrDefaultAsync(otherOrderAddress => otherOrderAddress.Id == order.OrderAddress.Id) is not null)
                order.OrderAddress.Id = Guid.NewGuid().ToString();
            order.OrderAddress.OrderId = order.Id; //this is not needed here, but I am setting it explicitly

            order.PaymentDetails.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.PaymentDetails.FirstOrDefaultAsync(otherPaymentDetails => otherPaymentDetails.Id == order.PaymentDetails.Id) is not null)
                order.PaymentDetails.Id = Guid.NewGuid().ToString();
            order.PaymentDetails.OrderId = order.Id; //this is not needed here, but I am setting it explicitly

            if (order.OrderAddress.IsShippingAddressDifferent.Value && (order.OrderAddress.AltFirstName is null || order.OrderAddress.AltLastName is null ||
                order.OrderAddress.AltCountry is null || order.OrderAddress.City is null || order.OrderAddress.AltPostalCode is null ||
                order.OrderAddress.AltAddress is null || order.OrderAddress.AltPhoneNumber is null))
                return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.AllTheAltFieldsNeedToBeFilledIfDifferentShippingAddress);

            PaymentOption? paymentOption = await _appDataDbContext.PaymentOptions.FirstOrDefaultAsync(paymentOption => paymentOption.Id == order.PaymentDetails.PaymentOptionId);
            if (paymentOption is null)
                return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidPaymentOption);
            order.PaymentDetails.PaymentOptionExtraCostAtOrder = paymentOption.ExtraCost;
            paymentOption.ExistsInOrder = true;
            paymentOption.ModifiedAt = dateTimeNow;

            ShippingOption? shippingOption = await _appDataDbContext.ShippingOptions.FirstOrDefaultAsync(shippingOption => shippingOption.Id == order.ShippingOptionId);
            if (shippingOption is null)
                return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidShippingOption);
            order.ShippingCostAtOrder = shippingOption.ExtraCost;
            shippingOption.ExistsInOrder = true;
            shippingOption.ModifiedAt = dateTimeNow;

            if (order.UserCouponId is not null)
            {
                UserCoupon? foundUserCoupon = await _appDataDbContext.UserCoupons
                    .Include(userCoupon => userCoupon.Coupon)
                    .FirstOrDefaultAsync(userCoupon => userCoupon.Id == order.UserCouponId && userCoupon.UserId == order.UserId); //this also checks that the user owns t he coupon
                if (foundUserCoupon is null)
                    return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidCouponIdWasGiven);
                else if (foundUserCoupon.IsDeactivated!.Value || foundUserCoupon.Coupon!.IsDeactivated!.Value)
                    return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.CouponCodeCurrentlyDeactivated);
                else if (foundUserCoupon.TimesUsed >= foundUserCoupon.Coupon.UsageLimit)
                    return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.CouponUsageLimitExceeded);

                if (foundUserCoupon is not null && !foundUserCoupon.ExistsInOrder!.Value) //The timesUsed property of the user coupon will be updated only when the order is confirmed
                {
                    foundUserCoupon.ModifiedAt = dateTimeNow;
                    foundUserCoupon.ExistsInOrder = true;
                }

                order.UserCouponId = foundUserCoupon!.Id;
                order.UserCoupon = foundUserCoupon!;
                order.CouponDiscountPercentageAtOrder = foundUserCoupon.Coupon!.DiscountPercentage;
            }
            else
            {
                order.UserCoupon = null;
                order.CouponDiscountPercentageAtOrder = 0;
            }

            List<string?> orderItemsIds = _appDataDbContext.OrderItems.Select(orderItem => orderItem.Id).ToList();
            foreach (OrderItem orderItem in order.OrderItems)
            {
                Variant? foundVariant = await _appDataDbContext.Variants
                    .Include(variant => variant.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
                    .Include(variant => variant.Discount)
                    .FirstOrDefaultAsync(variant => variant.Id == orderItem.VariantId && !variant.IsDeactivated!.Value);

                if (foundVariant is null)
                    return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.InvalidVariantIdWasGiven);
                else if (foundVariant.UnitsInStock < orderItem.Quantity)
                    return new ReturnOrderAndCodeResponseModel(null!, DataLibraryReturnedCodes.InsufficientStockForVariant);

                if (!foundVariant.ExistsInOrder!.Value) //The variant units in stock will only be updated after the order has been confirmed
                {
                    foundVariant.ExistsInOrder = true;
                    foundVariant.ModifiedAt = dateTimeNow;
                }

                orderItem.Id = Guid.NewGuid().ToString();
                while (orderItemsIds.Contains(orderItem.Id))
                    orderItem.Id = Guid.NewGuid().ToString();

                orderItem.OrderId = order.Id; //this is not needed here, but I am setting it explicitly
                orderItem.Variant = foundVariant;
                orderItem.Discount = foundVariant.Discount;
                orderItem.DiscountId = foundVariant.DiscountId;

                orderItem.DiscountPercentageAtOrder = foundVariant.Discount is not null && !foundVariant.Discount.IsDeactivated!.Value ? foundVariant.Discount.Percentage : 0;
                if (foundVariant.VariantImages is not null && foundVariant.VariantImages.Any())
                {
                    AppImage? chosenImage = foundVariant.VariantImages.FirstOrDefault(variantImage => variantImage.IsThumbNail)?.Image;
                    orderItem.Image = chosenImage ?? foundVariant.VariantImages[0].Image;
                    orderItem.ImageId = orderItem.Image!.Id;
                    orderItem.Image.ExistsInOrder = true;
                    orderItem.Image.ModifiedAt = dateTimeNow;
                }
                else
                {
                    orderItem.Image = null;
                    orderItem.ImageId = null;
                }

                decimal? discountedUnitPrice = foundVariant.Discount is not null ? foundVariant.Price * foundVariant.Discount.Percentage / 100 : 0;
                orderItem.UnitPriceAtOrder = foundVariant.Price - discountedUnitPrice;
                finalPrice += orderItem.UnitPriceAtOrder * orderItem.Quantity;
            }

            finalPrice = finalPrice + order.ShippingCostAtOrder + order.PaymentDetails.PaymentOptionExtraCostAtOrder;
            finalPrice = finalPrice - (finalPrice * order.CouponDiscountPercentageAtOrder / 100);
            order.FinalPrice = finalPrice;

            await _appDataDbContext.Orders.AddAsync(order);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateOrderSuccess"), "The order was successfully created.");
            return new ReturnOrderAndCodeResponseModel(order, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateOrderFailure"), ex, "An error occurred while creating the order. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateOrderStatusAsync(string newOrderStatus, string orderId)
    {
        try
        {
            DateTime dateTimeNow = DateTime.Now;
            if (newOrderStatus != "Pending" && newOrderStatus != "Confirmed" && newOrderStatus != "Processed" && newOrderStatus != "Shipped" && newOrderStatus != "Completed"
                && newOrderStatus != "Canceled" && newOrderStatus != "NoShow" && newOrderStatus != "RefundPending" && newOrderStatus != "RefundFailed" && newOrderStatus != "Refunded" && newOrderStatus != "Failed")
                return DataLibraryReturnedCodes.InvalidOrderStatus;

            Order? foundOrder = await _appDataDbContext.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Variant)
                .Include(order => order.PaymentDetails)
                .Include(order => order.ShippingOption)
                .Include(order => order.UserCoupon)
                    .ThenInclude(userCoupon => userCoupon!.Coupon)
                .FirstOrDefaultAsync(order => orderId == order.Id);

            if (foundOrder is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateOrderStatusFailureDueToNullOrder"), "The order with Id={id} was not found and thus the status update could not proceed.", orderId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            //Because of Stripe the refundpending can be bypassed. This is not the typical case, but to avoid race conditions I allow the bypass of RefundPending
            if (foundOrder.OrderStatus == "Failed" || foundOrder.OrderStatus == "Refunded" || foundOrder.OrderStatus == "NoShow" || foundOrder.OrderStatus == "Canceled")
                return DataLibraryReturnedCodes.OrderStatusHasBeenFinalizedAndThusOrderStatusCanNotBeAltered;
            else if (foundOrder.OrderStatus == "Completed" && newOrderStatus != "RefundPending" && newOrderStatus != "RefundFailed" && newOrderStatus != "Refunded")
                return DataLibraryReturnedCodes.InvalidNewOrderState;
            else if (foundOrder.OrderStatus == "Shipped" && newOrderStatus != "NoShow" && newOrderStatus != "Completed" && newOrderStatus != "RefundPending" && newOrderStatus != "RefundFailed" && newOrderStatus != "Refunded")
                return DataLibraryReturnedCodes.InvalidNewOrderState;
            else if (foundOrder.OrderStatus == "Processed" && newOrderStatus != "Canceled" && newOrderStatus != "Shipped" && newOrderStatus != "Completed"
                && newOrderStatus != "RefundPending" && newOrderStatus != "RefundFailed" && newOrderStatus != "Refunded")
                return DataLibraryReturnedCodes.InvalidNewOrderState;
            else if (foundOrder.OrderStatus == "Confirmed" && newOrderStatus != "Canceled" && newOrderStatus != "Processed" && newOrderStatus != "RefundPending" && newOrderStatus != "RefundFailed" && newOrderStatus != "Refunded")
                return DataLibraryReturnedCodes.InvalidNewOrderState;
            else if (foundOrder.OrderStatus == "Pending" && newOrderStatus != "Confirmed" && newOrderStatus != "Failed")
                return DataLibraryReturnedCodes.InvalidNewOrderState;
            else if (foundOrder.OrderStatus == "RefundPending" && newOrderStatus != "RefundFailed" && newOrderStatus != "Refunded")
                return DataLibraryReturnedCodes.InvalidNewOrderState;
            else if (foundOrder.OrderStatus == "RefundFailed" && newOrderStatus != "RefundPending")
                return DataLibraryReturnedCodes.InvalidNewOrderState;
            else if (foundOrder.OrderStatus == "Processed" && !foundOrder.ShippingOption!.ContainsDelivery!.Value && newOrderStatus == "Shipped")
                return DataLibraryReturnedCodes.ThisOrderDoesNotContainShippingAndThusTheShippedStatusIsInvalid;

            //the update of the usercoupon and the quantity should only happen after the order has been confirmed
            if (foundOrder.OrderStatus == "Pending" && newOrderStatus == "Confirmed")
            {
                if (foundOrder.UserCoupon is not null)
                    foundOrder.UserCoupon!.TimesUsed++;

                foreach (OrderItem orderItem in foundOrder.OrderItems)
                    orderItem.Variant!.UnitsInStock -= orderItem.Quantity;
            }

            if (newOrderStatus == "Canceled" || newOrderStatus == "Refunded" || newOrderStatus == "NoShow" || newOrderStatus == "Failed") //maybe in noshow do not reverse the coupon usage?
            {
                foundOrder.PaymentDetails!.PaymentStatus = "Unpaid";
                foundOrder.PaymentDetails!.AmountPaidInEuro = 0;
                foundOrder.PaymentDetails!.NetAmountPaidInEuro = 0;
                if (foundOrder.UserCoupon is not null)
                    foundOrder.UserCoupon.TimesUsed--;

                if (newOrderStatus != "Refunded") //I will let the admin decide how to handle the inventory in case of refunds, because maybe the products are problematic
                {
                    foreach (OrderItem orderItem in foundOrder.OrderItems)
                        orderItem.Variant!.UnitsInStock += orderItem.Quantity;
                }
            }

            foundOrder.ModifiedAt = dateTimeNow;
            foundOrder.OrderStatus = newOrderStatus;
            await _appDataDbContext.SaveChangesAsync();

            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateOrderStatusFailure"), ex, "An error occurred while updating the order status. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    //this update can either find an order based on id or based on payment processor session id or the payment processor payment intent id(needed for webhooks)
    public async Task<DataLibraryReturnedCodes> UpdateOrderAsync(Order updatedOrder)
    {
        try
        {
            DateTime dateTimeNow = DateTime.Now;

            Order? foundOrder = await _appDataDbContext.Orders
                .Include(order => order.OrderAddress)
                .Include(order => order.PaymentDetails)
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Variant)
                        .ThenInclude(variant => variant!.Product)
                .Include(order => order.OrderItems) //this part exists to check for existsInOrder for variants
                    .ThenInclude(orderItem => orderItem.Variant)
                        .ThenInclude(variant => variant!.OrderItems)
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Image)
                        .ThenInclude(image => image!.OrderItems)
                .Include(order => order.UserCoupon)
                    .ThenInclude(userCoupon => userCoupon!.Coupon)
                .FirstOrDefaultAsync(order => (order.Id != null && order.Id == updatedOrder.Id) ||
            (updatedOrder.PaymentDetails != null && updatedOrder.PaymentDetails.PaymentProcessorSessionId != null && order.PaymentDetails!.PaymentProcessorSessionId == updatedOrder.PaymentDetails.PaymentProcessorSessionId) ||
            (updatedOrder.PaymentDetails != null && updatedOrder.PaymentDetails.PaymentProcessorPaymentIntentId != null && order.PaymentDetails!.PaymentProcessorPaymentIntentId == updatedOrder.PaymentDetails.PaymentProcessorPaymentIntentId));
            if (foundOrder is null && updatedOrder.Id is not null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateOrderFailureDueToNullOrder"), "The order with Id={id} was not found and thus the update could not proceed.", updatedOrder.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }
            else if (foundOrder is null && updatedOrder.PaymentDetails is not null && updatedOrder.PaymentDetails.PaymentProcessorSessionId is not null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateOrderFailureDueToNullOrder"), "The order with PaymentProcessorSessionId={id} was not found and thus the update could not proceed.", updatedOrder.Id);
                return DataLibraryReturnedCodes.OrderNotFoundWithGivenPaymentProcessorSessionId;
            }
            else if (foundOrder is null && updatedOrder.PaymentDetails is not null && updatedOrder.PaymentDetails.PaymentProcessorPaymentIntentId is not null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateOrderFailureDueToNullOrder"), "The order with PaymentProcessorPaymentIntentId={id} was not found and thus the update could not proceed.", updatedOrder.Id);
                return DataLibraryReturnedCodes.OrderNotFoundWithGivenPaymentProcessorPaymentIntentId;
            }
            else if (foundOrder is null)
                return DataLibraryReturnedCodes.TheOrderIdThePaymentProcessorSessionIdAndPaymentIntentIdCanNotBeAllNull;

            if (foundOrder.OrderStatus == "Canceled" || foundOrder.OrderStatus == "NoShow" || foundOrder.OrderStatus == "Completed" || foundOrder.OrderStatus == "Refunded" || foundOrder.OrderStatus == "Failed")
                return DataLibraryReturnedCodes.OrderStatusHasBeenFinalizedAndThusTheOrderCanNotBeAltered;

            //in the case of completed and shipped order statuses only few things can change
            if (updatedOrder.PaymentDetails!.PaymentCurrency is not null && ValidateCurrencyISOCode(updatedOrder.PaymentDetails.PaymentCurrency))
                foundOrder.PaymentDetails!.PaymentCurrency = updatedOrder.PaymentDetails.PaymentCurrency;

            //maybe add chekcs if currency is not set correctly, because it might lead to weird states????
            foundOrder.PaymentDetails!.PaymentStatus = updatedOrder.PaymentDetails?.PaymentStatus ?? foundOrder.PaymentDetails.PaymentStatus;
            foundOrder.PaymentDetails!.AmountPaidInEuro = updatedOrder.PaymentDetails?.AmountPaidInEuro ?? foundOrder.PaymentDetails.AmountPaidInEuro;
            foundOrder.PaymentDetails!.NetAmountPaidInEuro = updatedOrder.PaymentDetails?.NetAmountPaidInEuro ?? foundOrder.PaymentDetails.NetAmountPaidInEuro;
            if (foundOrder.PaymentDetails.PaymentProcessorPaymentIntentId is null && updatedOrder.PaymentDetails is not null)
                foundOrder.PaymentDetails.PaymentProcessorPaymentIntentId = updatedOrder.PaymentDetails.PaymentProcessorPaymentIntentId;
            if (foundOrder.PaymentDetails.PaymentProcessorPaymentIntentId is null && updatedOrder.PaymentDetails is not null)
                foundOrder.PaymentDetails.PaymentProcessorPaymentIntentId = updatedOrder.PaymentDetails.PaymentProcessorPaymentIntentId;

            foundOrder.ModifiedAt = dateTimeNow;
            if (foundOrder.OrderStatus == "Completed" || foundOrder.OrderStatus == "Shipped") //this can happen if the user paid with cash from the administrator, when it comes to shipped I just leave the option open
            {
                await _appDataDbContext.SaveChangesAsync();
                return DataLibraryReturnedCodes.NoError;
            }

            //in the case of pending, confirmed and processed states most of the properties can change since the order is not on a critical state
            decimal? finalPrice = 0;

            //possible orderAddress change
            if (updatedOrder.OrderAddress is not null)
            {
                foundOrder.OrderAddress!.Email = updatedOrder.OrderAddress.Email ?? foundOrder.OrderAddress.Email;
                foundOrder.OrderAddress!.FirstName = updatedOrder.OrderAddress.FirstName ?? foundOrder.OrderAddress.FirstName;
                foundOrder.OrderAddress!.LastName = updatedOrder.OrderAddress.LastName ?? foundOrder.OrderAddress.LastName;
                foundOrder.OrderAddress!.PhoneNumber = updatedOrder.OrderAddress.PhoneNumber ?? foundOrder.OrderAddress.PhoneNumber;
                foundOrder.OrderAddress!.Country = updatedOrder.OrderAddress.Country ?? foundOrder.OrderAddress.Country;
                foundOrder.OrderAddress!.City = updatedOrder.OrderAddress.City ?? foundOrder.OrderAddress.City;
                foundOrder.OrderAddress!.PostalCode = updatedOrder.OrderAddress.PostalCode ?? foundOrder.OrderAddress.PostalCode;
                foundOrder.OrderAddress!.Address = updatedOrder.OrderAddress.Address ?? foundOrder.OrderAddress.Address;
                foundOrder.OrderAddress!.IsShippingAddressDifferent = updatedOrder.OrderAddress?.IsShippingAddressDifferent ?? foundOrder.OrderAddress.IsShippingAddressDifferent;
            }

            if (updatedOrder.OrderAddress is not null && updatedOrder.OrderAddress.IsShippingAddressDifferent is not null && updatedOrder.OrderAddress.IsShippingAddressDifferent.Value)
            {
                foundOrder.OrderAddress!.AltFirstName = updatedOrder.OrderAddress.AltFirstName ?? foundOrder.OrderAddress.AltFirstName;
                foundOrder.OrderAddress!.AltLastName = updatedOrder.OrderAddress.AltLastName ?? foundOrder.OrderAddress.AltLastName;
                foundOrder.OrderAddress!.AltPhoneNumber = updatedOrder.OrderAddress.AltPhoneNumber ?? foundOrder.OrderAddress.AltPhoneNumber;
                foundOrder.OrderAddress!.AltCountry = updatedOrder.OrderAddress.AltCountry ?? foundOrder.OrderAddress.AltCountry;
                foundOrder.OrderAddress!.AltCity = updatedOrder.OrderAddress.AltCity ?? foundOrder.OrderAddress.AltCity;
                foundOrder.OrderAddress!.AltPostalCode = updatedOrder.OrderAddress.AltPostalCode ?? foundOrder.OrderAddress.AltPostalCode;
                foundOrder.OrderAddress!.AltAddress = updatedOrder.OrderAddress.AltAddress ?? foundOrder.OrderAddress.AltAddress;
            }
            //if there was a different shipping and now there is not a different shipping address remove all the alt fields
            else if (foundOrder.OrderAddress!.IsShippingAddressDifferent!.Value && updatedOrder.OrderAddress is not null && updatedOrder.OrderAddress.IsShippingAddressDifferent is not null && !updatedOrder.OrderAddress.IsShippingAddressDifferent.Value)
            {
                foundOrder.OrderAddress!.AltFirstName = null;
                foundOrder.OrderAddress!.AltLastName = null;
                foundOrder.OrderAddress!.AltPhoneNumber = null;
                foundOrder.OrderAddress!.AltCountry = null;
                foundOrder.OrderAddress!.AltCity = null;
                foundOrder.OrderAddress!.AltPostalCode = null;
                foundOrder.OrderAddress!.AltAddress = null;
            }

            //possible coupon change
            if (updatedOrder.UserCouponId is not null && updatedOrder.UserCouponId != foundOrder.UserCouponId)
            {
                if (foundOrder.UserCoupon is not null)
                {
                    foundOrder.UserCoupon.TimesUsed--;
                    foundOrder.UserCoupon.ModifiedAt = dateTimeNow;

                    UserCoupon? currentCoupon = await _appDataDbContext.UserCoupons.Include(userCoupon => userCoupon.Orders).FirstOrDefaultAsync(userCoupon => foundOrder.UserCoupon.Id == userCoupon.Id); //this always exists obviously
                    if (currentCoupon!.Orders.Count == 1)
                        foundOrder.UserCoupon.ExistsInOrder = false;
                }

                UserCoupon? foundUserCoupon = await _appDataDbContext.UserCoupons.Include(userCoupon => userCoupon.Coupon).FirstOrDefaultAsync(userCoupon => userCoupon.Id == updatedOrder.UserCouponId);

                if (foundUserCoupon is null || foundUserCoupon.UserId != foundOrder.UserId) //this also user owns the coupon
                    return DataLibraryReturnedCodes.InvalidCouponIdWasGiven;
                else if (foundUserCoupon.IsDeactivated!.Value || foundUserCoupon.Coupon!.IsDeactivated!.Value)
                    return DataLibraryReturnedCodes.CouponCodeCurrentlyDeactivated;
                else if (foundUserCoupon.TimesUsed >= foundUserCoupon.Coupon.UsageLimit)
                    return DataLibraryReturnedCodes.CouponUsageLimitExceeded;

                foundUserCoupon.ExistsInOrder = true;
                foundUserCoupon.ModifiedAt = dateTimeNow;
                foundUserCoupon.TimesUsed++;
                foundOrder.UserCouponId = foundUserCoupon.Id;
                foundOrder.UserCoupon = foundUserCoupon;
                foundOrder.CouponDiscountPercentageAtOrder = foundUserCoupon.Coupon!.DiscountPercentage;
            }

            List<AppImage> oldImages = foundOrder.OrderItems.Where(orderItem => orderItem.Image is not null).Select(orderItem => orderItem.Image!).ToList();
            if (updatedOrder.OrderItems is not null)
            {
                //check if the shouldexistInOrderFlag 
                foreach (AppImage orderItemImage in oldImages)
                {
                    //this means that the image only exists in one order(maybe in multiple order items, but ONLY in one order), which means that if this image/images are removed from the order then the image could be deleted
                    if (orderItemImage.OrderItems.Count == 1 || orderItemImage.OrderItems.All(orderItem => orderItem.OrderId == orderItemImage.OrderItems.FirstOrDefault()?.OrderId))
                    {
                        orderItemImage.ExistsInOrder = false;
                        orderItemImage.ModifiedAt = dateTimeNow;
                    }
                }

                foreach (OrderItem orderItem in foundOrder.OrderItems)
                {
                    if (orderItem.Variant!.OrderItems.Count == 1)
                        orderItem.Variant.ExistsInOrder = false;

                    orderItem.Variant.UnitsInStock += orderItem.Quantity;
                    orderItem.Variant.ModifiedAt = dateTimeNow;
                }

                _appDataDbContext.OrderItems.RemoveRange(foundOrder.OrderItems);
                foundOrder.OrderItems.Clear(); //remove the previous order items
            }

            //possible changes when it comes to orderItems if the orderItems list of the updatedOrder is not null
            List<string?> orderItemsIds = _appDataDbContext.OrderItems.Select(orderItem => orderItem.Id).ToList();
            foreach (OrderItem orderItem in updatedOrder.OrderItems ?? Enumerable.Empty<OrderItem>())
            {
                Variant? foundVariant = await _appDataDbContext.Variants
                    .Include(variant => variant.VariantImages)
                    .ThenInclude(variantImage => variantImage.Image)
                    .Include(variant => variant.Discount)
                    .FirstOrDefaultAsync(variant => variant.Id == orderItem.VariantId && !variant.IsDeactivated!.Value);

                if (foundVariant is null)
                    return DataLibraryReturnedCodes.InvalidVariantIdWasGiven;
                else if (foundVariant.UnitsInStock < orderItem.Quantity)
                    return DataLibraryReturnedCodes.InsufficientStockForVariant;

                foundVariant.UnitsInStock -= orderItem.Quantity;
                foundVariant.ExistsInOrder = true;
                foundVariant.ModifiedAt = dateTimeNow;

                orderItem.Id = Guid.NewGuid().ToString();
                while (orderItemsIds.Contains(orderItem.Id))
                    orderItem.Id = Guid.NewGuid().ToString();
                orderItem.OrderId = foundOrder.Id;
                orderItem.Variant = foundVariant;
                orderItem.Discount = foundVariant.Discount;
                orderItem.DiscountId = foundVariant.DiscountId;
                orderItem.DiscountPercentageAtOrder = foundVariant.Discount is not null && !foundVariant.Discount.IsDeactivated!.Value ? foundVariant.Discount.Percentage : 0;
                if (foundVariant.VariantImages is not null && foundVariant.VariantImages.Any())
                {
                    AppImage? chosenImage = foundVariant.VariantImages.FirstOrDefault(variantImage => variantImage.IsThumbNail)?.Image;
                    orderItem.Image = chosenImage ?? foundVariant.VariantImages[0].Image;
                    orderItem.ImageId = orderItem.Image!.Id;
                    orderItem.Image.ExistsInOrder = true;
                }
                else
                {
                    orderItem.Image = null;
                    orderItem.ImageId = null;
                }

                decimal? discountedUnitPrice = foundVariant.Discount is not null ? foundVariant.Price * foundVariant.Discount.Percentage / 100 : 0;
                orderItem.UnitPriceAtOrder = foundVariant.Price - discountedUnitPrice;
                foundOrder.OrderItems.Add(orderItem);

                finalPrice += orderItem.UnitPriceAtOrder * orderItem.Quantity;
            }

            finalPrice = finalPrice + foundOrder.ShippingCostAtOrder + foundOrder.PaymentDetails.PaymentOptionExtraCostAtOrder;
            finalPrice = finalPrice - (finalPrice * foundOrder.CouponDiscountPercentageAtOrder / 100);
            foundOrder.FinalPrice = finalPrice;

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateOrderSuccess"), "The order was successfully updated.");
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateOrderFailure"), ex, "An error occurred while updating the order. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteOrderAsync(string orderId)
    {
        try
        {
            Order? foundOrder = await _appDataDbContext.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Variant)
                        .ThenInclude(variant => variant!.OrderItems)
                .Include(order => order.OrderItems)
                    .ThenInclude(orderItem => orderItem.Image)
                        .ThenInclude(image => image!.OrderItems)
                .Include(order => order.UserCoupon)
                    .ThenInclude(userCoupon => userCoupon!.Orders)
                .Include(order => order.ShippingOption)
                    .ThenInclude(shippingOption => shippingOption!.Orders)
                .Include(order => order.PaymentDetails)
                    .ThenInclude(paymentDetails => paymentDetails!.PaymentOption)
                        .ThenInclude(paymentOption => paymentOption!.PaymentDetails)
                .FirstOrDefaultAsync(paymentOption => paymentOption.Id == orderId);
            if (foundOrder is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteOrderFailureDueToNullOrder"), "The order with Id={id} was not found and thus the deletion could not proceed.", orderId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            DateTime dateTimeNow = DateTime.Now;
            List<AppImage> orderItemsImages = foundOrder.OrderItems.Where(orderItem => orderItem.Image is not null).Select(orderItem => orderItem.Image!).ToList();
            foreach (AppImage orderItemImage in orderItemsImages)
            {
                //this means that the image only exists in one order(maybe in multiple order items, but ONLY in one order), which means that if this image/images are removed from the order then the image could be deleted
                if (orderItemImage.OrderItems.Count == 1 || orderItemImage.OrderItems.All(orderItem => orderItem.OrderId == orderItemImage.OrderItems.FirstOrDefault()?.OrderId))
                {
                    orderItemImage.ExistsInOrder = false;
                    orderItemImage.ModifiedAt = dateTimeNow;
                }
            }

            foreach (OrderItem orderItem in foundOrder.OrderItems)
            {
                if (orderItem.Variant!.OrderItems.Count == 1)
                {
                    orderItem.Variant!.ExistsInOrder = false;
                    orderItem.Variant!.ModifiedAt = dateTimeNow;
                }

                //foundOrder.OrderStatus != "Refunded"
                if (foundOrder.OrderStatus != "Canceled" && foundOrder.OrderStatus != "NoShow" && foundOrder.OrderStatus != "Failed"
                    && foundOrder.OrderStatus != "Completed" && foundOrder.OrderStatus != "Refunded" && foundOrder.OrderStatus != "Pending") //if it is on a final state there is no reason for reversal. The pending will never happen, but who knows
                {
                    orderItem.Variant!.UnitsInStock += orderItem.Quantity;
                    orderItem.Variant!.ModifiedAt = dateTimeNow;
                }
            }

            if (foundOrder.ShippingOption!.Orders.Count == 1) //this means that the shipping option only exists in one order
            {
                foundOrder.ShippingOption.ExistsInOrder = false;
                foundOrder.ShippingOption.ModifiedAt = dateTimeNow;
            }

            if (foundOrder.PaymentDetails!.PaymentOption!.PaymentDetails.Count == 1) //this means that the payment option only exists in one order
            {
                foundOrder.PaymentDetails.PaymentOption.ExistsInOrder = false;
                foundOrder.PaymentDetails.PaymentOption.ModifiedAt = dateTimeNow;
            }

            if (foundOrder.UserCoupon is not null)
            {
                if (foundOrder.UserCoupon.Orders.Count == 1) //this means that the user coupon only exists in one order
                {
                    foundOrder.UserCoupon.ExistsInOrder = false;
                    foundOrder.UserCoupon.ModifiedAt = dateTimeNow;
                }

                //if the order was not in a final state reduce the TimesUsed property on the userCoupon
                if (foundOrder.OrderStatus != "Canceled" && foundOrder.OrderStatus != "Refunded" && foundOrder.OrderStatus != "NoShow"
                    && foundOrder.OrderStatus != "Failed" && foundOrder.OrderStatus != "Completed" && foundOrder.OrderStatus != "Pending")
                {
                    foundOrder.UserCoupon.TimesUsed--;
                    foundOrder.UserCoupon.ModifiedAt = dateTimeNow;
                }
            }

            _appDataDbContext.Orders.Remove(foundOrder);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteOrderSuccess"), "The order was successfully deleted with Id={id}.", orderId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteOrderFailure"), ex, "An error occurred while deleting the order with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", orderId, ex.Message, ex.StackTrace);
            throw;
        }
    }

    private bool ValidateCurrencyISOCode(string givenISOCurrencyCode)
    {
        HashSet<string> isoCurrencyCodes = CultureInfo.GetCultures(CultureTypes.SpecificCultures)
             .Select(culture => new RegionInfo(culture.Name))
             .Select(region => region.ISOCurrencySymbol.ToLowerInvariant())
             .Distinct()
             .ToHashSet();

        isoCurrencyCodes.RemoveWhere(currencyCode => string.IsNullOrWhiteSpace(currencyCode));

        return isoCurrencyCodes.Contains(givenISOCurrencyCode);
    }
}
