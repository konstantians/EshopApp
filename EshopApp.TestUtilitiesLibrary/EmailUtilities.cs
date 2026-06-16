using AngleSharp;
using System.Text;

namespace EshopApp.TestUtilitiesLibrary;
public class EmailUtilities
{
    public static string? ReadLastEmailFile(bool deleteEmailFile)
    {
        string directoryPath = @"C:\ProgramData\Changemaker Studios\Papercut SMTP\Incoming";

        List<string> emlFiles = Directory.GetFiles(directoryPath, "*.eml")
            .OrderByDescending(f => new FileInfo(f).LastWriteTime)
            .ToList();

        string? lastEmailFile = emlFiles.FirstOrDefault();
        if (lastEmailFile == null)
            return null;

        string emailFileContent = File.ReadAllText(lastEmailFile);

        if (deleteEmailFile)
            File.Delete(lastEmailFile);

        return emailFileContent;
    }

    public static async Task<string?> GetLastEmailLink(bool deleteEmailFile)
    {
        var directoryPath = @"C:\ProgramData\Changemaker Studios\Papercut SMTP\Incoming";

        var lastEmailFile = Directory.GetFiles(directoryPath, "*.eml")
            .Select(f => new FileInfo(f))
            .OrderByDescending(f => f.LastWriteTime)
            .FirstOrDefault();

        if (lastEmailFile == null)
            return null;

        var emlContent = await File.ReadAllTextAsync(lastEmailFile.FullName);

        // Extract HTML body (everything after the first blank line)
        var htmlPart = emlContent.Split(new[] { "\r\n\r\n" }, 2, StringSplitOptions.None).Last();

        // If base64 encoded, decode it
        string html;
        try
        {
            var bytes = Convert.FromBase64String(htmlPart);
            html = Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            html = htmlPart;
        }

        var context = BrowsingContext.New(Configuration.Default);
        var document = await context.OpenAsync(req => req.Content(html));

        var link = document.QuerySelector("a[href]")?.GetAttribute("href");

        if (deleteEmailFile)
            File.Delete(lastEmailFile.FullName);

        return link;
    }

    public static void DeleteAllEmailFiles()
    {
        string directoryPath = @"C:\ProgramData\Changemaker Studios\Papercut SMTP\Incoming";

        List<string> emlFiles = Directory.GetFiles(directoryPath, "*.eml").ToList();

        foreach (string emlFile in emlFiles)
            File.Delete(emlFile);
    }
}
