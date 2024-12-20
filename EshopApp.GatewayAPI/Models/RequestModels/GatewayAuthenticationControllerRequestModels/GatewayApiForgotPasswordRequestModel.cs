﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayAuthenticationControllerRequestModels;

public class GatewayApiForgotPasswordRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [Url]
    public string? ClientUrl { get; set; }
}
