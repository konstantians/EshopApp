using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.RoleModels;

public class ApiUpdateClaimsOfRoleRequestModel
{
    [Required]
    public string? RoleId{ get; set; }
    [Required]
    public List<Claim> NewClaims { get; set; } = new List<Claim>();

}
