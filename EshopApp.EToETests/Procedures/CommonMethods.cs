namespace EshopApp.EToETests.Procedures;
internal static class CommonMethods
{
    public static async Task<string?> ReadLastEmailLinkWithRetries(int retries = 20, int interval = 1000)
    {
        for (int i = 0; i < retries + 1; i++)
        {
            string? lastEmailLink = TestUtilitiesLibrary.EmailUtilities.GetLastEmailLink(true)!;
            if (lastEmailLink != null)
                return lastEmailLink;
            await Task.Delay(1000);
        }

        return null;
    }
}
