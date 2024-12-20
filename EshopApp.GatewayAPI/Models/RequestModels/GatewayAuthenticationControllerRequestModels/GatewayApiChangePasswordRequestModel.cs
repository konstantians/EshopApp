﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.Models.RequestModels.GatewayAuthenticationControllerRequestModels;

public class GatewayApiChangePasswordRequestModel
{
    [Required]
    public string? CurrentPassword { get; set; }
    [Required]
    public string? NewPassword { get; set; }
}
