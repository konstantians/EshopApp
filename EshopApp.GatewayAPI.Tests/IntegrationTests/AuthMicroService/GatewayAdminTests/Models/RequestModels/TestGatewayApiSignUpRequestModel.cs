﻿namespace EshopApp.GatewayAPI.Tests.IntegrationTests.AuthMicroService.GatewayAdminTests.Models.RequestModels;
internal class TestGatewayApiSignUpRequestModel
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string? ClientUrl { get; set; }
}