using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.ImageModels;

public class CreateImageRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Name { get; set; }
    [Required]
    [MaxLength(50)]
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
}
