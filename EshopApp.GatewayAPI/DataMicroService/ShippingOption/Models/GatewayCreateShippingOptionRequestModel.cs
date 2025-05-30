﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.ShippingOption.Models;

public class GatewayCreateShippingOptionRequestModel
{
    [MaxLength(50)]
    [Required]
    public string? Name { get; set; }
    public string? Description { get; set; }
    [Range(0, int.MaxValue, ErrorMessage = "The extra cost property must be a non-negative integer.")]
    public decimal? ExtraCost { get; set; }
    public bool? ContainsDelivery { get; set; }
    public bool? IsDeactivated { get; set; }
    public bool? ExistsInOrder { get; set; }
}
