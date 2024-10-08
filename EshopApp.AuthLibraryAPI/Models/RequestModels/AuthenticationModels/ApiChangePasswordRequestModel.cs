﻿using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.AuthenticationModels;

public class ApiChangePasswordRequestModel
{
    [Required]
    public string? CurrentPassword { get; set; }
    [Required]
    public string? NewPassword { get; set; }
}
