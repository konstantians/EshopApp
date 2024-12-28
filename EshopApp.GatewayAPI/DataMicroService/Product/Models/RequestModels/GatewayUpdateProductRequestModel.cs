﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.DataMicroService.Product.Models.RequestModels;

public class GatewayUpdateProductRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Code { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    public string? Description { get; set; }
    public bool? IsDeactivated { get; set; }
    public List<string>? CategoryIds { get; set; }
    public List<string>? VariantIds { get; set; }
}