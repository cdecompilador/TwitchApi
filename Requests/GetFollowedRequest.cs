using System.Text.Json;

namespace TwitchApi;

public struct GetFollowedRequest : IRequest<List<TwitchUser>>
{
    public byte Scope => Scopes.UserReadFollows;
    public byte StateRequirements => ConnState.UserAuthorized;

    private string _targetId;

    public GetFollowedRequest(string targetId)
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
        var request = Api.TwitchRequest(secrets, accessToken)
            .AtEndpoint("channels/followed")
            .WithQueryParam("user_id", _targetId);

        // Deserializar y Añadir los usuarios
        JsonDocument json = await request.GetAsync(client);
        users.AddRange(TwitchUser.DeserializeMany(json));

        // Ahora con la paginación enviar queries hasta que se acaben las
        // páginas
        var page = Pagination.Load(json);
        while (page != null)
        {
            // Deserializar y Añadir los usuarios
            json = await Api.TwitchRequest(secrets, accessToken)
                .AtEndpoint("channels/followed")
                .WithQueryParam("user_id", _targetId)
                .WithQueryParam("after", page)
                .GetAsync(client);
            users.AddRange(TwitchUser.DeserializeMany(json));

            // Actualizar el valor de la página actual
            page = Pagination.Load(json);
        }

        return users;
    }
}

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
