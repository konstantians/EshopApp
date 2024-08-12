using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.RoleModels;

public class ApiReplaceRoleOfUserRequestModel
{

    [Required]
    public string? UserId { get; set; }

    [Required]
    public string? CurrentRoleId { get; set; }
    [Required]
    public string? NewRoleId { get; set; }
}
