namespace EshopApp.AuthLibrary.Models;
public class Address
{
    public string? Id { get; set; } = Guid.NewGuid().ToString();
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? PostalCode { get; set; }
    public string? AddressName { get; set; }
    public string? UserId { get; set; }
    public AppUser? AppUser { get; set; }
}
