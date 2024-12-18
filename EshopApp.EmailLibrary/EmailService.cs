using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;

namespace EshopApp.EmailLibrary;

//Logging Messages Start With 2 For Success/Information, 3 For Warning And 4 For Error(Almost like HTTP Status Codes). The range is 0-99, for example 1000. 
//The range of codes for this class is is 100-199, for example 2100 or 2199.
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger = null!)
    {
        _configuration = configuration;
        _logger = logger ?? NullLogger<EmailService>.Instance;
    }

    /// <summary>
    /// This method sends an email using the emailSender string parameter to the given email, which is given by
    /// the emailReceiver string parameter. This method should only be used in case you either want to hardcode
    /// the defauly sender email("kinnaskonstantinos0@gmail.com") or you know what you are doing with the other 
    /// email you want to send the email. If you are not certain on what to do just use the simplified overload
    /// of the SendEmail method, which does not have a parameter for the emailSender(it is hardcoded).
    /// </summary>
    /// <param name="emailSender">The email account which will send the email</param>
    /// <param name="emailReceiver">The email account which will receive the email</param>
    /// <param name="title">The title of the email</param>
    /// <param name="body">The context of the email</param>
    /// <returns>Confirmation on whether or not the email was send successfully</returns>
    public async Task<bool> SendEmailAsync(string emailSender, string emailReceiver, string title, string body)
    {
        SmtpSettings? smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        using var message = new MailMessage(emailSender, emailReceiver);

        message.Subject = title;
        message.Body = body;
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(smtpSettings!.Host, smtpSettings.Port);
        smtpClient.EnableSsl = smtpSettings.EnableSsl;
        smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

        try
        {
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation(new EventId(2100, "EmailSentSuccessfullyAlternative"), "Successfully sent email. " +
                "EmailSender:{EmailSender}, EmailReceiver:{EmailReceiver}, Title:{Title}, Body:{Body}",
                emailSender, emailReceiver, title, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4100, "EmailSentFailureAlternative"), ex, "An error occurred while trying to send email. " +
                "EmailSender:{EmailSender}, EmailReceiver:{EmailReceiver}, Title:{Title}, Body:{Body}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                emailSender, emailReceiver, title, body, ex.Message, ex.StackTrace);
            return false;
        }
    }

    /// <summary>
    /// This method sends an email to the given email, which is given by the emailReceiver string parameter. This method
    /// is a simplified version of the other SendEmail overload, which also takes as an extra parameter the email sender. If
    /// you do not know, which to use you should use this one, because the hardcoded email this method is using has already been
    /// configured and it works correctly. In conclusion this method should be the default method for sending emails.
    /// </summary>
    /// <param name="emailReceiver">The email account which will receive the email</param>
    /// <param name="title">The title of the email</param>
    /// <param name="body">The context of the email</param>
    /// <returns>Confirmation on whether or not the email was send successfully</returns>
    public async Task<bool> SendEmailAsync(string emailReceiver, string title, string body)
    {
        SmtpSettings? smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        using var message = new MailMessage("kinnaskonstantinos0@gmail.com", emailReceiver);

        message.Subject = title;
        message.Body = body;
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(smtpSettings!.Host, smtpSettings.Port);
        smtpClient.EnableSsl = smtpSettings.EnableSsl;
        smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

        try
        {
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation(new EventId(2101, "EmailSentSuccessfully"), "Successfully sent email. EmailReceiver:{EmailReceiver}, Title:{Title}, Body:{Body}",
                emailReceiver, title, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4101, "EmailSentFailure"), ex, "An error occurred while trying to send email. " +
                "EmailReceiver:{EmailReceiver}, Title:{Title}, Body:{Body}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                emailReceiver, title, body, ex.Message, ex.StackTrace);
            return false;
        }
    }

    /// <summary>
    /// used to send messages from kinnaskonstantinos0@gmail.com to kinnaskonstantinos0@gmail.com adding the email of the user
    /// in the body of the email. The reason why this has to be done like that is because the application can not force the user
    /// to send an email to our email, so this is the simplest way to make it work.
    /// </summary>
    /// <param name="emailSender">The email of the sender which will be passed into the body of the email</param>
    /// <param name="title">The title of the contact form message</param>
    /// <param name="body">The context of the contact form message</param>
    /// <returns>Confirmation on whether or not the email was send successfully</returns>
    public async Task<bool> SendContactFormEmailAsync(string emailSender, string title, string body)
    {
        SmtpSettings? smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

        using var message = new MailMessage("kinnaskonstantinos0@gmail.com", "kinnaskonstantinos0@gmail.com");
        message.Subject = title;
        message.Body = $"From: {emailSender}\n\n{body}";
        message.IsBodyHtml = false;

        using var smtpClient = new SmtpClient(smtpSettings!.Host, smtpSettings.Port);
        smtpClient.EnableSsl = smtpSettings.EnableSsl;
        smtpClient.Credentials = new NetworkCredential(smtpSettings.Username, smtpSettings.Password);

        try
        {
            await smtpClient.SendMailAsync(message);
            _logger.LogInformation(new EventId(2102, "ContactFormEmailSentSuccessfully"), "Successfully sent contact form email. EmailSender:{EmailSender}, Title:{Title}, Body:{Body}",
                emailSender, title, body);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(4102, "ContactFormEmailSentFailure"), ex, "An error occurred while trying to send email from contact form. " +
                "EmailSender:{EmailSender}, Title:{Title}, Body:{Body}" +
                "ExceptionMessage: {ExceptionMessage}. StackTrace: {StackTrace}.",
                emailSender, title, body, ex.Message, ex.StackTrace);
            return false;
        }
    }

    public bool ValidateSmtpServer()
    {
        try
        {
            SmtpSettings? smtpSettings = _configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

            using var client = new TcpClient(smtpSettings!.Host!, smtpSettings!.Port);
            using var stream = client.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

            // Read server response (greeting message)
            Console.WriteLine(reader.ReadLine());

            // Send EHLO command to the server
            writer.WriteLine($"EHLO {smtpSettings!.Host!}");
            Console.WriteLine(reader.ReadLine());

            // Send AUTH LOGIN command to start authentication
            writer.WriteLine("AUTH LOGIN");
            Console.WriteLine(reader.ReadLine());

            // Send base64-encoded username
            writer.WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(smtpSettings!.Username!)));
            Console.WriteLine(reader.ReadLine());

            // Send base64-encoded password
            writer.WriteLine(Convert.ToBase64String(Encoding.ASCII.GetBytes(smtpSettings!.Password!)));
            var response = reader.ReadLine();
            Console.WriteLine(response);

            // Check if authentication succeeded (successful response usually starts with "235"). I have also left here the 250 status code as valid for papercut
            string? returnedStatusCode = response?.Substring(0, 3);
            if (returnedStatusCode is null || (returnedStatusCode != "235" && returnedStatusCode != "250"))
                return false;

            return true;
        }
        catch
        {
            return false;
        }
    }
}
