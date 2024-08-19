using System.Security.Claims;

namespace EshopApp.AuthLibraryAPI.Models;

public class CustomClaim
{
    public string? Type { get; set; }
    public string? Value { get; set; }

    public CustomClaim() { }

    public CustomClaim(Claim claim)
    {
        Type = claim.Type;
        Value = claim.Value;
    }
}
