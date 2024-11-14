using System.Text.Json;

namespace EshopApp.TestUtilitiesLibrary;
public class JsonUtilities
{
    public static async Task<string?> GetSingleStringValueFromBody(HttpResponseMessage response, string key)
    {
        string? responseBody = await response.Content.ReadAsStringAsync();
        var keyValue = JsonSerializer.Deserialize<Dictionary<string, string>>(responseBody);
        keyValue!.TryGetValue(key, out string? value);
        return value;
    }
}
