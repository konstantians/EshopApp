using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.RoleModels;

public class ApiCreateRoleRequestModel
{
    [Required]
    public string? RoleName { get; set; }
    public List<ApiCustomClaim> Claims { get; set; } = new List<ApiCustomClaim>();
}
