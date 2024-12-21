using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace EshopApp.GatewayAPI.Models;

public class GatewayClaim
{
    [Required]
    public string? Type { get; set; }
    [Required]
    public string? Value { get; set; }

    public GatewayClaim() { }

    public GatewayClaim(Claim claim)
    {
        Type = claim.Type;
        Value = claim.Value;
    }

}
