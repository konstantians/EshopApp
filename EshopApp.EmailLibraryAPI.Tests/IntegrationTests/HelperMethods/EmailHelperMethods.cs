namespace EshopApp.EmailLibraryAPI.Tests.IntegrationTests.HelperMethods;

internal class EmailHelperMethods
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


    public static void DeleteAllEmailFiles()
    {
        string directoryPath = @"C:\ProgramData\Changemaker Studios\Papercut SMTP\Incoming";

        List<string> emlFiles = Directory.GetFiles(directoryPath, "*.eml").ToList();

        foreach (string emlFile in emlFiles)
            File.Delete(emlFile);
    }

}
