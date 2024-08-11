using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace EshopApp.AuthLibrary.Models;

public class AppRole : IdentityRole
{
    [NotMapped]
    public List<Claim> Claims { get; set; } = new List<Claim>();

    public AppRole()
    {
        
    }

    public AppRole(string givenRoleName) : base(roleName: givenRoleName)
    {
        
    }
}
