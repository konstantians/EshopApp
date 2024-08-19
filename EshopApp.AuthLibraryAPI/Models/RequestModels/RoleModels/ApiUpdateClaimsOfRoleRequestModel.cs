using System.ComponentModel.DataAnnotations;

namespace EshopApp.AuthLibraryAPI.Models.RequestModels.RoleModels;

public class ApiUpdateClaimsOfRoleRequestModel
{
    [Required]
    public string? RoleId{ get; set; }
    [Required]
    public List<ApiCustomClaim> NewClaims { get; set; } = new List<ApiCustomClaim>();
}
