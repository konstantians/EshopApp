using System.Security.Claims;

namespace EshopApp.MVC.Models.AuthModels;

public class UiClaim
{
    public string? Type { get; set; }
    public string? Value { get; set; }

    public UiClaim() { }

    public UiClaim(Claim claim)
    {
        Type = claim.Type;
        Value = claim.Value;
    }
}
