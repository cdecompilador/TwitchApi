using System.Text.Json;

namespace TwitchApi;

/// Secretos para la interacción entre nuestro servidor y la api de twitch, 
/// usamos un OAuth de apliación en vez de usuario ya que es bastante trivial
/// y no creo que necesitemos nada más
public struct TwitchSecrets
{
    public string ClientId { init; get;}
    public string ClientSecret { init; get; }

    public static TwitchSecrets Load(string path)
    {
        // Leer el archivo que contiene las settings entero y extraer el nodo
        // json que contenga la información que queremos deserializar, 
        // la lectura no es asíncrona ya que solo se hace 1 vez
        string jsonContents = File.ReadAllText(path);
        JsonElement appSecretsNode = JsonDocument.Parse(jsonContents)
            .RootElement.GetProperty("TwitchSecrets");

        // Crear el TwitchAppSecret
        var appSecrets = JsonSerializer.Deserialize<TwitchSecrets>(appSecretsNode);

        return appSecrets;
    }
}
