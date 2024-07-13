using EshopApp.EmailLibrary.Models.InternalModels.NoSqlModels;
using EshopApp.EmailLibrary.Models.InternalModels.SqlModels;

namespace EshopApp.EmailLibrary.Models.ResponseModels;

public class EmailResponseModel
{
    public string? Id { get; set; }
    public DateTime SentAt { get; set; }
    public string? Receiver { get; set; }
    public string? Title { get; set; }
    public string? Message { get; set; }

    public EmailResponseModel() { }

    internal EmailResponseModel(SqlEmailModel sqlEmailModel)
    {
        Id = sqlEmailModel.Id;
        SentAt = sqlEmailModel.SentAt;
        Receiver = sqlEmailModel.Receiver;
        Title = sqlEmailModel.Title;
        Message = sqlEmailModel.Message;
    }

    internal EmailResponseModel(NoSqlEmailModel noSqlEmailModel)
    {
        Id = noSqlEmailModel.Id;
        SentAt = noSqlEmailModel.SentAt;
        Receiver = noSqlEmailModel.Receiver;
        Title = noSqlEmailModel.Title;
        Message = noSqlEmailModel.Message;
    }

}
