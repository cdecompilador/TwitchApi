using System.Text.Json;

HttpClient client = new HttpClient();

TwitchApi.TwitchSecrets secrets = 
    TwitchApi.TwitchSecrets.Load("Settings.json");

TwitchApi.UserAccessToken accessToken = 
    await TwitchApi.Api.GetUserAccessToken(client, secrets, TwitchApi.Scopes.UserReadFollows);

var conn = new TwitchApi.TwitchConnection(client, secrets, accessToken);

var result = await conn.GetFollowsOf("cdecompilador");
foreach (var u in result)
{
    Console.WriteLine(u.UserName);
}

await accessToken.Revoke(client, secrets);

/*
JsonDocument response = await TwitchApi.Api.TwitchRequest(secrets, accessToken)
    .AtEndpoint("users")
    .WithQueryParam("login", "ibai")
    .GetAsync(client);

Console.WriteLine("Ibai tiene estos viewers: ");
Console.WriteLine(response.RootElement.GetProperty("data"));
*/


/*
using System.Reflection;

public class BuilderWeb
{
}

public class BuilderInjection
{
}

public interface IBuilder<BT>
{
    public BT Build();

    public IBuilder<BT> CreateBuilder();
}

public interface IDepInjection<BT> : IBuilder<BT>
{
    public IServiceCollection Services { get; }
}

public class PresetWrapper
{
    public B CreateBuilder<B, BT>(string[] args)
        where B: IBuilder<BT>, new()
    {
        B b = new B();

        return b;
    }

    public B CreateBuilder<B, BT>(string[] args, B? ignored = default(B?))
        where B: IBuilder<BT>, IDepInjection<BT>, new()
    {
        B b = new B();
        return b;
    }
}

public static class Utilities
{
    public static bool IsAssemblyLoaded(string assemblyName)
    {
        try
        {
            Assembly.Load(assemblyName);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

Utilities.IsAssemblyLoaded("Microsoft.Extensions.Dependen

// Con inyeccion de dependencias
var builder = PresetWrapper.CreateBuilder<BuilderWeb>(args);
var app = builder.Build();
*/

