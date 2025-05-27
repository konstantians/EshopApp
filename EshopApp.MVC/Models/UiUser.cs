namespace EshopApp.MVC.Models;

public class UiUser
{
    public string? Id { get; set; }
    public string? Email { get; set; }
    public string? UserRoleName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTime? LockoutEnd { get; set; }
    //public Cart? Cart { get; set; }
    //public List<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
    //public List<Order> Orders { get; set; } = new List<Order>();
}