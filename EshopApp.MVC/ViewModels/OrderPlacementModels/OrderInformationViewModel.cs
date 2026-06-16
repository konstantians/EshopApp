using EshopApp.MVC.Models.AuthModels;
using EshopApp.MVC.Models.DataModels;

namespace EshopApp.MVC.ViewModels.OrderPlacementModels;

public class OrderInformationViewModel
{
    public UiUser? User { get; set; }
    public List<UiPaymentOption> UiPaymentOptions { get; set; } = new List<UiPaymentOption>();
    public List<UiShippingOption> UiShippingOptions { get; set; } = new List<UiShippingOption>();
}
