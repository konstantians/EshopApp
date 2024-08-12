using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.RoleModels;

public class ApiCreateRoleRequestModel
{
    [Required]
    public string? RoleName { get; set; }
    public List<Claim> Claims { get; set; } = new List<Claim>();
}
