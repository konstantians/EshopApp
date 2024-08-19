using EshopApp.AuthLibraryAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace EshopApp.AuthLibrary.Models;

public class AppRole : IdentityRole
{
    [NotMapped]
    public List<CustomClaim> Claims { get; set; } = new List<CustomClaim>();

    public AppRole() { }

    public AppRole(string givenRoleName) : base(roleName: givenRoleName) { }
}
