using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace EshopApp.AuthLibrary.Models;

public class AppUser : IdentityUser
{
    //add list of addresses here
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    [NotMapped]
    public string? UserRoleName { get; set; }
}
