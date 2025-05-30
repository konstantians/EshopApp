﻿using EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.VariantTests.Models.RequestModels;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.ProductTests.Models.RequestModels;
internal class TestGatewayCreateProductRequestModel
{
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string> CategoryIds { get; set; } = new List<string>();
    public TestGatewayCreateVariantRequestModel? CreateVariantRequestModel { get; set; }
}
