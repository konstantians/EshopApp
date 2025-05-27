using Microsoft.AspNetCore.Identity;

namespace EshopApp.MVC.Models;

public class UiRole : IdentityRole
{
    public int UsersInRoleCount { get; set; }

    public List<UiClaim> Claims { get; set; } = new List<UiClaim>();

    public UiRole() { }

    public UiRole(string givenRoleName) : base(roleName: givenRoleName) { }
}
