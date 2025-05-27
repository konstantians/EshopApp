using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.EditUserViewModels;

public class EditUserAccountBasicSettingsViewModel
{
    [Required]
    public string? UserId { get; set; }

    [MaxLength(128, ErrorMessage = "Firstname can not exceed 128 characters")]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? FirstName { get; set; }

    [MaxLength(128, ErrorMessage = "Lastname can not exceed 128 characters")]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? LastName { get; set; }

    [RegularExpression(@"^$|^\+?\d{1,4}[\s\-]?\(?\d{1,3}\)?[\s\-]?\d{1,4}[\s\-]?\d{1,4}[\s\-]?\d{1,4}$", ErrorMessage = "Invalid phone number")]
    [Display(Name = "Phone Number")]
    [DisplayFormat(ConvertEmptyStringToNull = false)]
    public string? PhoneNumber { get; set; }

    public bool AccountActivated { get; set; }
}
