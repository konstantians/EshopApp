namespace EshopApp.TestUtilitiesLibrary;
public class EmailUtilities
{
    public static List<string>? ReadLastEmailFile()
    {
        string directoryPath = @"C:\ProgramData\Changemaker Studios\Papercut SMTP\Incoming";

        List<string> emlFiles = Directory.GetFiles(directoryPath, "*.eml")
                                .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                                .ToList();

        string? lastEmailFile = emlFiles.FirstOrDefault();
        if (lastEmailFile == null)
            return null;

        List<string> emailFileLines = File.ReadAllLines(lastEmailFile).ToList();
        return emailFileLines;
    }

    public static string? GetLastEmailLink()
    {
        string directoryPath = @"C:\ProgramData\Changemaker Studios\Papercut SMTP\Incoming";

        // Get all .eml files in the directory
        List<string> emlFiles = Directory.GetFiles(directoryPath, "*.eml")
                                .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                                .ToList();

        // Get the last one
        string? lastEmailFile = emlFiles.FirstOrDefault();
        if (lastEmailFile is null)
            return null!;

        // Read the email content using 'using' to ensure the file is closed properly
        string emailContent;
        using (StreamReader reader = new StreamReader(lastEmailFile))
        {
            emailContent = reader.ReadToEnd();
        }

        // Split email content into words
        string[] wordsOfLastEmail = emailContent.Split(" ");

        // Get the last word that is the link and remove email formatting
        string link = wordsOfLastEmail.Last().Replace("=\r\n", "").Replace("\r\n", "");
        link = link.Replace("=3D", "=");

        // Delete the file now that we have the link
        File.Delete(lastEmailFile);

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
