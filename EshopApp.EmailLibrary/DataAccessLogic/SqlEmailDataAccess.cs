﻿using EshopApp.EmailLibrary.Models.InternalModels.SqlModels;
using EshopApp.EmailLibrary.Models.RequestModels;
using EshopApp.EmailLibrary.Models.ResponseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.EmailLibrary.DataAccessLogic;

//Logging Messages Start With 2 For Success/Information, 3 For Warning And 4 For Error(Almost like HTTP Status Codes). The range is 0-99, for example 1000. 
//The range of codes for this class is is 0-99, for example 2000 or 2099.
public class SqlEmailDataAccess : IEmailDataAccess
{
    private readonly SqlEmailDbContext _context;
    private readonly ILogger<SqlEmailDataAccess> _logger;

    public SqlEmailDataAccess(SqlEmailDbContext context, ILogger<SqlEmailDataAccess> logger = null!)
    {
        _context = context;
        _logger = logger ?? NullLogger<SqlEmailDataAccess>.Instance;
    }

    public async Task<IEnumerable<ApiEmailResponseModel>> GetEmailEntriesAsync()
    {
        try
        {
            List<SqlEmailModel> sqlEmailModels = await _context.Emails.ToListAsync();
            var emailResponseModels = new List<ApiEmailResponseModel>();
            foreach (SqlEmailModel sqlEmailModel in sqlEmailModels)
                emailResponseModels.Add(new ApiEmailResponseModel(sqlEmailModel));

            return emailResponseModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4000, "EmailEntriesCouldNotBeRetrieved"), ex, "An error occurred while trying to retrieve application's email entries. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<ApiEmailResponseModel>> GetEmailEntriesAsync(int amount)
    {
        try
        {
            List<SqlEmailModel> sqlEmailModels = await _context.Emails.OrderByDescending(email => email.SentAt)
                .Take(amount).ToListAsync();

            var emailResponseModels = new List<ApiEmailResponseModel>();
            foreach (SqlEmailModel sqlEmailModel in sqlEmailModels)
                emailResponseModels.Add(new ApiEmailResponseModel(sqlEmailModel));

            return emailResponseModels;
        }
        catch (Exception ex)
        {

            _logger.LogError(new EventId(4001, "EmailEntriesOfGivenAmmountCountNotBeRetrieved"), ex, "An error occurred while trying to retrieve application's email entries with Amount: {Amount}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", amount, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ApiEmailResponseModel?> GetEmailEntryAsync(string id)
    {
        try
        {
            SqlEmailModel? sqlEmailModel = await _context.Emails.FirstOrDefaultAsync(email => email.Id == id);
            var emailResponseModel = sqlEmailModel is not null ? new ApiEmailResponseModel(sqlEmailModel) : null;

            return emailResponseModel;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4002, "EmailEntryCouldNotBeRetrieved"), ex, "An error occurred while trying to retrieve email entry with EmailId: {EmailId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<IEnumerable<ApiEmailResponseModel>> GetEmailsOfEmailReceiverAsync(string emailReceiver)
    {
        try
        {
            List<SqlEmailModel> sqlEmailModels = await _context.Emails.Where(email => email.Receiver == emailReceiver).ToListAsync();
            var emailResponseModels = new List<ApiEmailResponseModel>();
            foreach (SqlEmailModel sqlEmailModel in sqlEmailModels)
                emailResponseModels.Add(new ApiEmailResponseModel(sqlEmailModel));

            return emailResponseModels;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4003, "EmailEntriesOfEmailReceiverCouldNotBeRetrieved"), ex, "An error occurred while trying to retrieve email entries of Receiver: {Receiver}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", emailReceiver, ex.Message, ex.StackTrace);
            throw;
        }
    }


    public async Task<string?> SaveEmailEntryAsync(ApiEmailRequestModel createEmailModel)
    {
        try
        {
            SqlEmailModel sqlEmailModel = new SqlEmailModel(createEmailModel);
            sqlEmailModel.Id = Guid.NewGuid().ToString();
            sqlEmailModel.SentAt = DateTime.Now;

            var result = await _context.Emails.AddAsync(sqlEmailModel);
            await _context.SaveChangesAsync();

            _logger.LogInformation(new EventId(2000, "EmailEntrySuccessfullySaved"), "Successfully created email entry." +
                "EmailId: {EmailId}, SentAt: {SentAt}, Receiver: {Receiver}, Title: {Title}, Content: {Content}",
                result.Entity.Id, DateTime.Now, createEmailModel.Receiver, createEmailModel.Title, createEmailModel.Message);
            return result.Entity.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4004, "EmailEntryCouldNotBeSaved"), ex, "An error occurred while trying to create email entry. " +
                "SentAt: {SentAt}, Receiver: {Receiver} ,Title: {Title}, Content: {Content}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                DateTime.Now, createEmailModel.Receiver, createEmailModel.Title, createEmailModel.Message, ex.Message, ex.StackTrace);
            return null;
        }
    }

    public async Task<bool> DeleteEmailEntryAsync(string id)
    {
        try
        {
            SqlEmailModel? foundEmailEntry = await _context.Emails.FirstOrDefaultAsync(email => email.Id == id);
            if (foundEmailEntry is null)
            {
                _logger.LogWarning(new EventId(3000, "EmailEntryToBeDeletedCouldNotBeFound"), "Attempted to delete null email entry, given EmailId: {EmailId}.", id);
                return false;
            }

            _context.Emails.Remove(foundEmailEntry);
            await _context.SaveChangesAsync();

            _logger.LogInformation(new EventId(2001, "EmailEntrySuccessfullyDeleted"), "Successfully deleted email entry with EmailId: {EmailId}.", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4005, "EmailEntryCouldNotBeDeleted"), ex, "An error occurred while trying to delete email entry with EmailId:{EmailId}. " +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.", id, ex.Message, ex.StackTrace);
            return false;
        }
    }

}
