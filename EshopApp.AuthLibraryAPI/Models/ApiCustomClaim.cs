using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models;

public class ApiCustomClaim
{
    [Required]
    public string? Type { get; set; }
    [Required]
    public string? Value { get; set; }
}
