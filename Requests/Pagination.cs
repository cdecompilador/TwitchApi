using System.Text.Json;

namespace TwitchApi;

public struct Pagination
{
    public static string? Load(JsonDocument json)
    {
        try
        {
            return json.RootElement
                .GetProperty("pagination")
                .GetProperty("cursor")
                .GetString();
        }
        catch (Exception)
        {
            return null;
        }
    }
}
