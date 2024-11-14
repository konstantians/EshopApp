using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels.ImageModels;

public class UpdateImageRequestModel
{
    [Required]
    [MaxLength(50)]
    public string? Id { get; set; }
    [MaxLength(50)]
    public string? Name { get; set; }
    [MaxLength(75)]
    public string? ImagePath { get; set; }
    public bool? ShouldNotShowInGallery { get; set; }
    public bool? ExistsInOrder { get; set; }
}
