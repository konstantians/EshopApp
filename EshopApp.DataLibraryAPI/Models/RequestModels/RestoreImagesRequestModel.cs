using System.ComponentModel.DataAnnotations;

namespace EshopApp.DataLibraryAPI.Models.RequestModels;

public class RestoreImagesRequestModel
{
    [Required]
    public List<string> ImageIds { get; set; } = new List<string>();
}
