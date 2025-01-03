﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.GatewayAPI.AuthMicroService.GatewayAdmin.Models.RequestModels;

public class GatewayCreateUserRequestModel
{
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Password { get; set; }
    [Phone]
    public string? PhoneNumber { get; set; }
    public bool SendEmailNotification { get; set; }
}
