using EshopApp.EmailLibrary.Models.RequestModels;
using EshopApp.EmailLibrary.Models.ResponseModels;

namespace EshopApp.EmailLibrary.DataAccessLogic
{
    public interface IEmailDataAccess
    {
        Task<bool> DeleteEmailEntryAsync(string id);
        Task<IEnumerable<EmailResponseModel>> GetEmailEntriesAsync();
        Task<IEnumerable<EmailResponseModel>> GetEmailEntriesAsync(int amount);
        Task<EmailResponseModel?> GetEmailEntryAsync(string id);
        Task<IEnumerable<EmailResponseModel>> GetEmailsOfEmailReceiverAsync(string emailReceiver);
        Task<string?> SaveEmailEntryAsync(EmailRequestModel createEmailModel);
    }
}