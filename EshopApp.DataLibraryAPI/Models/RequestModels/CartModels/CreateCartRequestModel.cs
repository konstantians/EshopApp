using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.CartModels;

public class CreateCartRequestModel
{
    [Required]
    public string? UserId { get; set; }
    public List<CreateCartItemRequestModel> CreateCartItemRequestModels { get; set; } = new List<CreateCartItemRequestModel>();
}
