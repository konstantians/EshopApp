using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.VariantImageModels;

public class CreateVariantImageRequestModel
{
    public bool IsThumbNail { get; set; }
    [Required]
    [MaxLength(50)]
    public string? ImageId { get; set; }

}
