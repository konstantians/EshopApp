using EshopApp.EmailLibrary.Models.RequestModels;
namespace EshopApp.EmailLibrary.Models.InternalModels.NoSqlModels;

internal class NoSqlEmailModel
{
    public string? Id { get; set; }
    public DateTime SentAt { get; set; }
    public string? Receiver { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }

    public NoSqlEmailModel()
    {

    }

    public NoSqlEmailModel(ApiEmailRequestModel emailRequestModel)
    {
        Receiver = emailRequestModel.Receiver;
        Title = emailRequestModel.Title;
        Message = emailRequestModel.Message;
    }

}
