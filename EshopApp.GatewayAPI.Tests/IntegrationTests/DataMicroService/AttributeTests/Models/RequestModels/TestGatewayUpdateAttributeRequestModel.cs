﻿namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.AttributeTests.Models.RequestModels;
internal class TestGatewayUpdateAttributeRequestModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<string>? VariantIds { get; set; }
}
