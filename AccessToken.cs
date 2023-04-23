using System.Text.Json;

namespace TwitchApi;

public abstract class AccessToken
{
    public string Token { set; get; }

    public AccessToken(string token) => Token = token;

    public async Task<bool> IsValid(HttpClient client)
    {
        JsonDocument json = await new RequestBuilder(Api.OAuthApiUrl)
            .AtEndpoint("validate")
            .WithHeader("Authorization", "OAuth " + Token)
            .GetAsync(client);
        Console.WriteLine(json.ToString());
        if (!json.RootElement.TryGetProperty("status", out _)) {
            return false;
        }

        return true;
    }

    public async Task Revoke(HttpClient client, TwitchSecrets secrets)
    {
        try
        {
            await new RequestBuilder(Api.OAuthApiUrl)
                .AtEndpoint("revoke")
                .WithQueryParam("client_id", secrets.ClientId)
                .WithQueryParam("token", Token)
                .PostAsync(client);
        }
        catch (Exception)
        {
            // No es realmente una excepción, simplemente es la única petición
            // de la que no esperamos un body
        }
    }

    public abstract Task Refresh(HttpClient client, TwitchSecrets secrets);
}

/// Un Access Token a la api de twitch, sea de aplicacion o de usuario, ya que
/// OAuth usa el mismo esquema de token y ambos expiran
public class AppAccessToken : AccessToken
{
    public AppAccessToken(string token) : base(token)
    {
    }

    // Para el app access token no hay una entrada en la api de oauth para 
    // refrescarlo en específico, pero siempre se puede pedir uno nuevo
    public override async Task Refresh(HttpClient client, TwitchSecrets secrets)
    {
        JsonDocument json = await new RequestBuilder(Api.OAuthApiUrl)
            .AtEndpoint("token")
            .WithQueryParam("client_id", secrets.ClientId)
            .WithQueryParam("client_secret", secrets.ClientSecret)
            .WithQueryParam("grant_type", "client_credentials")
            .PostAsync(client);

        // En caso de que el usuario haya eliminado la app esto crasheará
        // lo cual está bien
        Token = json.RootElement.GetProperty("access_token").GetString()
            ?? throw new Exception("Fatal error: Twitch App might have been deleted");
    }
}

public class UserAccessToken : AccessToken 
{
    public string RefreshToken { set; get; }
    // TODO: public Scope Scope { set; get; }

    public UserAccessToken(string token, string refreshToken /* Scope scope */)
        : base(token)
    {
        RefreshToken = refreshToken;
        // Scope = scope;
    }

    public override async Task Refresh(HttpClient client, TwitchSecrets secrets)
    {
        JsonDocument json = await new RequestBuilder(Api.OAuthApiUrl)
            .AtEndpoint("token")
            .WithQueryParam("client_id", secrets.ClientId)
            .WithQueryParam("client_secret", secrets.ClientSecret)
            .WithQueryParam("grant_type", "refresh_token")
            .WithQueryParam("refresh_token", RefreshToken)
            .PostAsync(client);

        // FIXME: En caso de que no se pueda refrescar el token es un error 
        // fatal
        Token = json.RootElement.GetProperty("access_token").GetString()
            ?? throw new Exception("Fatal error: Cannot refresh token");
        RefreshToken = json.RootElement.GetProperty("refresh_token").GetString()!;
    }
}
