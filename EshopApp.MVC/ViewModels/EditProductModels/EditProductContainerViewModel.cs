using EshopApp.MVC.ViewModels.CreateProductModels;

namespace EshopApp.MVC.ViewModels.EditProductModels;

public class EditProductContainerViewModel
{
    public EditProductViewModel EditProductViewModel { get; set; } = new EditProductViewModel();
    public List<EditVariantViewModel> EditVariantViewModels { get; set; } = new List<EditVariantViewModel>();
    public CreateVariantViewModel CreateVariantViewModel { get; set; } = new CreateVariantViewModel();
}
