using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.Models.EmailModels;

public class ContactEmailModel
{
    [Required]
    [MaxLength(50, ErrorMessage = "First Name can not exceed 128 characters")]
    public string? FirstName { get; set; }
    [Required]
    [MaxLength(50, ErrorMessage = "Last Name can not exceed 128 characters")]
    public string? LastName { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    public string? Subject { get; set; }
    [Required]
    [MaxLength(300, ErrorMessage = "message can not exceed 300 characters")]
    public string? Message { get; set; }
}
