﻿namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.CategoryTests.Models;
internal class TestGatewayUpdateCategoryRequestModel
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<string>? ProductIds { get; set; }
}
