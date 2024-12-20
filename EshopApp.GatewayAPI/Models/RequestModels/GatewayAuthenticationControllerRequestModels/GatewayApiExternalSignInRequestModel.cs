﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayAuthenticationControllerRequestModels;

public class GatewayApiExternalSignInRequestModel
{
    [Required]
    public string? IdentityProviderName { get; set; }
    [Required]
    [Url]
    public string? ReturnUrl { get; set; }
}
