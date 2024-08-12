using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.RoleModels;

public class ApiAddRoleToUserRequestModel
{
    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? RoleId { get; set; }
}
