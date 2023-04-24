using System.Text.Json;

HttpClient client = new HttpClient();

TwitchApi.TwitchSecrets secrets = 
    TwitchApi.TwitchSecrets.Load("Settings.json");

TwitchApi.UserAccessToken accessToken = 
    await TwitchApi.Api.GetUserAccessToken(
            client,
            secrets,
            TwitchApi.Scopes.UserReadFollows);

var conn = new TwitchApi.TwitchConnection(client, secrets, accessToken);

var result = await conn.GetFollowersCountOf("cdecompilador");
Console.WriteLine(result);
/*
foreach (var u in result)
{
    Console.WriteLine(u.UserName);
}
*/
await conn.DisposeAsync();
