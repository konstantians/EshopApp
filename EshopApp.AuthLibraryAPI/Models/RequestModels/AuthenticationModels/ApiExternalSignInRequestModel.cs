﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AuthenticationModels;

public class ApiExternalSignInRequestModel
{
    [Required]
    public string? IdentityProviderName { get; set; }
    [Required]
    [Url]
    public string? ReturnUrl { get; set; }
}
