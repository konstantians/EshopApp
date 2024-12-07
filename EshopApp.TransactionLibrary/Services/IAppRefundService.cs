using EshopApp.TransactionLibrary.Models.ResponseModels;

namespace EshopApp.TransactionLibrary.Services;
public interface IAppRefundService
{
    Task<TransactionLibraryReturnedCodes> IssueRefundAsync(string sessionId);
}