namespace TwitchApi;

public struct GetFollowersRequest : IRequest<List<TwitchUser>>
{
    public byte Scope => Scopes.ModeratorReadFollowers;
    public byte StateRequirements => ConnState.UserAuthorized;

    private string _targetId;

    public GetFollowersRequest(string targetId)
    {
        _targetId = targetId;
    }

    public async Task<List<TwitchUser>> Send(
            AccessToken accessToken,
            TwitchSecrets secrets, 
            HttpClient client)
    {
        var users = new List<TwitchUser>();

        // La primera request va sin paginación
        var json = await Api.TwitchRequest(secrets, accessToken)
            .AtEndpoint("channels/followers")
            .WithQueryParam("broadcaster_id", _targetId)
            .GetAsync(client);

        // Deserializar y Añadir los usuarios
        users.AddRange(TwitchUser.DeserializeMany(json));

        Console.WriteLine(json.RootElement.ToString());

        // Ahora con la paginación enviar queries hasta que se acaben las
        // páginas
        var page = Pagination.Load(json);
        while (page != null)
        {
            // Deserializar y Añadir los usuarios
            json = await Api.TwitchRequest(secrets, accessToken)
                .AtEndpoint("channels/followers")
                .WithQueryParam("broadcast_id", _targetId)
                .WithQueryParam("after", page)
                .GetAsync(client);
            users.AddRange(TwitchUser.DeserializeMany(json));

            // Actualizar el valor de la página actual
            page = Pagination.Load(json);
        }

        return users;
    }
}
