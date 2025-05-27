using EshopApp.MVC.Models;
using System.ComponentModel.DataAnnotations;

namespace EshopApp.MVC.ViewModels;

public class CreateRoleViewModel
{
    [Required]
    public string? RoleName { get; set; }
    public List<UiClaim>? Claims { get; set; }
}
