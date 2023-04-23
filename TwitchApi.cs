using System.Text.Json;
using System.Diagnostics;
using System.Net;

namespace TwitchApi;

public class Api
{
    // Este peculiar diseño de crear las requests y guardar el url base de
    // twitch y twitch-oauth es necesario porque la api además usa endpoints, 
    // para evitar cambios de diseño en el futuro, en caso de necesitar un
    // endpoint de las apis simplemente añado un método estático que cree el 
    // builder pasandole el url base + subruta
	public static readonly string TwitchApiUrl = "https://api.twitch.tv/helix";
    public static readonly string OAuthApiUrl = "https://id.twitch.tv/oauth2";
	
	public static RequestBuilder TwitchRequest(
            TwitchSecrets secrets,
            AccessToken accessToken)
    {
	    return new RequestBuilder(TwitchApiUrl)
            .WithPreCondition(async client => 
            {
                if (!await accessToken.IsValid(client))
                {
                    await accessToken.Refresh(client, secrets);
                }

                return true;
            })
            .WithHeader("Authorization", "Bearer " + accessToken.Token)
            .WithHeader("Client-Id", secrets.ClientId);
	}

    public static RequestBuilder OAuthRequest()
    {
        // TODO: Tal vez poner en el futuro una flag que active el uso de
        // x-www-form-urlencoded dentro del contenido del body, ya que en los
        // docs dice explicitamente que OAuth lo espera ahí, pero si no da 
        // problemas por ahora se queda así
        return new RequestBuilder(OAuthApiUrl);
    }

    // Esto es un método estático ya que tener un constructor asíncrono, nonono
    public static async Task<AppAccessToken> GetAppAccessToken(
            HttpClient client,
            TwitchSecrets secrets)
    {
        var accessToken = new AppAccessToken("");
        await accessToken.Refresh(client, secrets);

        return accessToken;
    }

    // Esto es un método estático ya que tener un constructor asíncrono, nonono
    public static async Task<UserAccessToken> GetUserAccessToken(
            HttpClient client,
            TwitchSecrets secrets,
            byte scopes)
    {
        // Crear la URI de la petición del code
        var uri = await TwitchApi.Api.OAuthRequest()
            .AtEndpoint("authorize")
            .WithQueryParam("response_type", "code")
            .WithQueryParam("client_id", secrets.ClientId)
            .WithQueryParam("redirect_uri", "http://localhost:9050/")
            .WithQueryParam("scope", Scopes.ToUrlEncodedString(scopes))
            .GetUrlEncodedUri();

        Console.WriteLine(uri);
        Console.WriteLine("Enter the URL and give permission to the APP");

        // Abrir el navegador con la petición
        Process.Start("rundll32", new[] { "url.dll,FileProtocolHandler", uri });

        // Esperar al redirect con el código en la URI
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:9050/");
        listener.Start();
        string code = "";
        while (true)
        {
            if (code != "") break;

            var context = listener.GetContext();
            var request = context.Request;

            code = request.QueryString["code"] ?? "";
        }

        // Con el código ahora pedir el access token
        JsonDocument json = await OAuthRequest()
            .AtEndpoint("token")
            .WithQueryParam("client_id", secrets.ClientId)
            .WithQueryParam("client_secret", secrets.ClientSecret)
            .WithQueryParam("code", code)
            .WithQueryParam("grant_type", "authorization_code")
            // La redirect_uri da error si es distinta a la que la app ha 
            // configurado, tal vez haya que meterla a secrets o forzar usar
            // localhost, total no la usamos ya que nuestro servicio es 
            // server to server
            .WithQueryParam("redirect_uri", "http://localhost:9050/")
            .WithUrlEncodedOnBody(true)
            .PostAsync(client);

        return new UserAccessToken(
            json.RootElement.GetProperty("access_token").GetString()
                ?? throw new Exception("Probably invalid secret code"),
            json.RootElement.GetProperty("refresh_token").GetString()!
        );
    }
}
