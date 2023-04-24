namespace TwitchApi;

public static class ConnState
{
    public const byte UnAuthorized = 0;
    public const byte AppAuthorized = 1 << 0;
    public const byte UserAuthorized = 1 << 2;
}

public class TwitchConnection : IAsyncDisposable
{
    private int _state;
    private HttpClient _client;
    private TwitchSecrets _secrets;
    private AccessToken _accessToken;

    public TwitchConnection(
            HttpClient client,
            TwitchSecrets secrets,
            AccessToken accessToken)
    {
        _state   = ConnState.UserAuthorized;
        _client  = client;
        _secrets = TwitchSecrets.Load("Settings.json");
        _accessToken = accessToken;
    }

    public async ValueTask DisposeAsync()
    {
        await _accessToken.Revoke(_client, _secrets);
        _client.Dispose();
    }

    private async Task<TRes> SendRequest<TReq, TRes>(TReq req)
        where TReq : IRequest<TRes>, new()
    {
        // TODO: Comprobar que el estado y scope de la request coinciden con el 
        // actual
        if ((req.StateRequirements & _state) == 0 && req.StateRequirements != 0)
        {
            throw new Exception(
                $"Invalid Twitch connection state requirements for request {req.ToString()}");
        }

        return await req.Send(_accessToken, _secrets, _client);
    }

    public async Task<List<TwitchUser>> GetFollowsOf(string targetChannel)
    {
        var targetChannelId = await GetUserId(targetChannel);
        return await SendRequest<
            GetFollowedRequest, List<TwitchUser>
        >(new(targetChannelId));
    }

    public async Task<string> GetUserId(string targetChannel)
    {
        return await SendRequest<
            GetUserIdRequest, string
        >(new(targetChannel));
    }

    public async Task<List<TwitchUser>> GetFollowersOf(string targetChannel)
    {
        var targetChannelId = await GetUserId(targetChannel);
        return await SendRequest<
            GetFollowersRequest, List<TwitchUser>
        >(new(targetChannelId));
    }

    public async Task<int> GetFollowersCountOf(string targetChannel)
    {
        var targetChannelId = await GetUserId(targetChannel);
        return await SendRequest<
            GetFollowersCountRequest, int
        >(new(targetChannelId));
    }
}
