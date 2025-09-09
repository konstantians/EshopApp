using EshopApp.MVC.Models.DataModels;

namespace EshopApp.MVC.ViewModels.CategoryModels;

public class ManageCategoryViewModel
{
    public List<UiCategory> Categories { get; set; } = new List<UiCategory>();
    public List<UiProduct> Products { get; set; } = new List<UiProduct>();
    public CreateCategoryViewModel? CreateCategoryViewModel { get; set; }
    public List<UpdateCategoryViewModel> UpdateCategoryViewModel { get; set; } = new List<UpdateCategoryViewModel>();
}
