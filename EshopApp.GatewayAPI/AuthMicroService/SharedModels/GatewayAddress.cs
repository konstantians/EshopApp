using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.SharedModels;

public class GatewayAddress
{
    public string? Id { get; set; }
    [MaxLength(128, ErrorMessage = "Country can not exceed 128 characters")]
    public string? Country { get; set; }
    [MaxLength(128, ErrorMessage = "City can not exceed 128 characters")]
    public string? City { get; set; }
    [MaxLength(128, ErrorMessage = "Postal Code can not exceed 128 characters")]
    public string? PostalCode { get; set; }
    [MaxLength(128, ErrorMessage = "Address can not exceed 128 characters")]
    public string? AddressName { get; set; }
}
