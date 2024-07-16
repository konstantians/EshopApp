using EshopApp.EmailLibrary.Models.RequestModels;
using EshopApp.EmailLibrary.Models.ResponseModels;

namespace EshopApp.EmailLibrary.DataAccessLogic;

public interface IEmailDataAccess
{
    Task<bool> DeleteEmailEntryAsync(string id);
    Task<IEnumerable<ApiEmailResponseModel>> GetEmailEntriesAsync();
    Task<IEnumerable<ApiEmailResponseModel>> GetEmailEntriesAsync(int amount);
    Task<ApiEmailResponseModel?> GetEmailEntryAsync(string id);
    Task<IEnumerable<ApiEmailResponseModel>> GetEmailsOfEmailReceiverAsync(string emailReceiver);
    Task<string?> SaveEmailEntryAsync(ApiEmailRequestModel createEmailModel);
}