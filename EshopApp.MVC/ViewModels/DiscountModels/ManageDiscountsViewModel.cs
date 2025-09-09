using EshopApp.MVC.Models.DataModels;

namespace EshopApp.MVC.ViewModels.DiscountModels;

public class ManageDiscountsViewModel
{
    public List<UiDiscount> Discounts { get; set; } = new List<UiDiscount>();
    public List<UiVariant> Variants { get; set; } = new List<UiVariant>();
    public CreateDiscountViewModel? CreateDiscountViewModel { get; set; }
    public List<UpdateDiscountViewModel> UpdateDiscountViewModels { get; set; } = new List<UpdateDiscountViewModel>();
}
