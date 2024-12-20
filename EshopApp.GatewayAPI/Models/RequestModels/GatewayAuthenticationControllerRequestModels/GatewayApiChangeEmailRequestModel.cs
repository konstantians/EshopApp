﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayAuthenticationControllerRequestModels;

public class GatewayApiChangeEmailRequestModel
{
    [Required]
    [EmailAddress]
    public string? NewEmail { get; set; }
    [Required]
    [Url]
    public string? ClientUrl { get; set; }
}
