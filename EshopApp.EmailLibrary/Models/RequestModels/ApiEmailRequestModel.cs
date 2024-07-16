using System.ComponentModel.DataAnnotations;

namespace EshopApp.EmailLibrary.Models.RequestModels;

public class ApiEmailRequestModel
{
    [Required]
    [EmailAddress]
    public string? Receiver { get; set; }
    [Required]
    public string? Title { get; set; }
    [Required]
    public string? Message { get; set; }
}
