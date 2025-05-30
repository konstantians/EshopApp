﻿namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.OrderTests.Models.RequestModels;

internal class TestGatewayUpdateOrderRequestModel
{
    public string? Id { get; set; }
    public string? PaymentProcessorSessionId { get; set; }
    public string? PaymentProcessorPaymentIntentId { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? IsShippingAddressDifferent { get; set; }
    public string? AltFirstName { get; set; }
    public string? AltLastName { get; set; }
    public string? AltCountry { get; set; }
    public string? AltCity { get; set; }
    public string? AltPostalCode { get; set; }
    public string? AltAddress { get; set; }
    public string? AltPhoneNumber { get; set; }

    public string? PaymentStatus { get; set; }
    public string? PaymentCurrency { get; set; }
    public decimal? AmountPaidInEuro { get; set; }
    public decimal? NetAmountPaidInEuro { get; set; }
    public List<TestGatewayOrderItemRequestModel>? OrderItemRequestModels { get; set; }
    public string? UserCouponId { get; set; }
}
