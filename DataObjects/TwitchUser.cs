using System.Text.Json;

namespace TwitchApi;

public struct TwitchUser
{
    public string Id { init; get; }
    public string LoginName { init; get; }
    public string UserName { init; get; }

    public static TwitchUser Deserialize(JsonElement jsonNode)
    {
        return new()
        {
            Id = jsonNode.GetProperty("broadcaster_id").GetString()!,
            LoginName = jsonNode.GetProperty("broadcaster_login").GetString()!,
            UserName = jsonNode.GetProperty("broadcaster_name").GetString()!
        };
    }

    public static List<TwitchUser> DeserializeMany(JsonDocument json)
    {
        var jsonNode = json.RootElement.GetProperty("data");
        var result = new List<TwitchUser>();
        var len = jsonNode.GetArrayLength();
        for (var i = 0; i < len; i++)
        {
            result.Add(TwitchUser.Deserialize(jsonNode[i]));
        }

        return result;
    }
}
