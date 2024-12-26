using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Tests.IntegrationTests.DataMicroService.SharedModels;
internal class TestGatewayCreateVariantImageRequestModel
{
    public bool IsThumbNail { get; set; }
    [Required]
    [MaxLength(50)]
    public string? ImageId { get; set; }
}
