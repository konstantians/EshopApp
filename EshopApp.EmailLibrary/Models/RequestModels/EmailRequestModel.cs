namespace EshopApp.EmailLibrary.Models.RequestModels;

public class EmailRequestModel
{
    public string? Receiver { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }
}
