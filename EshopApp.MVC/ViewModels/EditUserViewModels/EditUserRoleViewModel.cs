using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels.EditUserViewModels;

public class EditUserRoleViewModel
{
    [Required]
    public string? UserId { get; set; }
    [Required]
    public string? CurrentRoleId { get; set; }
    [Required]
    public string? NewRoleId { get; set; }
}
