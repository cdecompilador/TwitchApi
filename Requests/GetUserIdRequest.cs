namespace TwitchApi;

public struct GetUserIdRequest : IRequest<string>
{
    public byte Scope => Scopes.NoScope;
    public byte StateRequirements => 
        ConnState.UserAuthorized | ConnState.AppAuthorized;

    private string _targetName;

    public GetUserIdRequest(string targetName)
    {
        _targetName = targetName;
    }

    public async Task<string> Send(
            AccessToken accessToken,
            TwitchSecrets secrets, 
            HttpClient client)
    {
        var json = await Api.TwitchRequest(secrets, accessToken)
            .AtEndpoint("users")
            .WithQueryParam("login", _targetName)
            .GetAsync(client);

        // Presuponer que existe el usuario y twitch solo nos ha devuelto 1
        return json.RootElement.GetProperty("data")[0]
            .GetProperty("id").GetString()!;
    }
}
