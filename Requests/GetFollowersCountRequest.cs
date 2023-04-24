namespace TwitchApi;

// Necesito crear un tipo de request distinto ya que puedes hacer la
// misma petición que en GetFollowers pero sin ningun scope
public struct GetFollowersCountRequest : IRequest<int>
{
    public byte Scope => Scopes.NoScope;
    public byte StateRequirements => 
        ConnState.UserAuthorized | ConnState.AppAuthorized;

    private string _targetId;

    public GetFollowersCountRequest(string targetId)
    {
        _targetId = targetId;
    }

    public async Task<int> Send(
            AccessToken accessToken,
            TwitchSecrets secrets,
            HttpClient client)
    {
        var json = await Api.TwitchRequest(secrets, accessToken)
            .AtEndpoint("channels/followers")
            .WithQueryParam("broadcaster_id", _targetId)
            .GetAsync(client);

        return json.RootElement.GetProperty("total").GetInt32()!;
    }
}
