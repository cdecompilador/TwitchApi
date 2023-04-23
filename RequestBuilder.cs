using System.Text.Json;

namespace TwitchApi;

public struct RequestBuilder
{
	private Dictionary<string, string> _query;
    private Dictionary<string, string> _headers;
    private bool _urlEncodedOnBody;
    private Func<HttpClient, Task<bool>>? _condFn;
    private string _apiUrl;

	public RequestBuilder(string apiUrl)
	{
		_query = new Dictionary<string, string>();
        _headers = new Dictionary<string, string>();
        _urlEncodedOnBody = false;
        _apiUrl = apiUrl;
	}

    public RequestBuilder AtEndpoint(string endpoint)
    {
        _apiUrl = _apiUrl + "/" + endpoint;

        return this;
    }

	public RequestBuilder WithQueryParam(string key, string val)
	{
        _query.Add(key, val);

		return this;	
	}

    public RequestBuilder WithHeader(string key, string val)
    {
        _headers.Add(key, val);
        
        return this;
    }

    public RequestBuilder WithPreCondition(Func<HttpClient, Task<bool>> condFn)
    {
        _condFn = condFn;

        return this;
    }

    public RequestBuilder WithUrlEncodedOnBody(bool onBody)
    {
        _urlEncodedOnBody = onBody;

        return this;
    }

    /// Todos los métodos HTTP de la api de twitch usan el mismo formato, 
    /// datos de entrada url encoded y datos de autorización en el header, este
    /// método privado generaliza para todos los métodos
    private async Task<JsonDocument> SendAsync(
        HttpClient client, HttpMethod method)
    {
        if (_condFn != null)
        {
            if (!await _condFn(client))
            {
                throw new Exception("Precodition of Request has not been met"); 
            }
        }

        // Crear el contenido de la petición POST y asegurar que es valido
        // usando la implementación concreta de HttpContent
        var urlEncodedContent = new FormUrlEncodedContent(_query);

        if (!_urlEncodedOnBody)
        {
            _apiUrl = await GetUrlEncodedUri();
            Console.WriteLine("{0} {1}", method.ToString(), _apiUrl);
        }

        var request = new HttpRequestMessage(method, _apiUrl);

        if (_urlEncodedOnBody)
        {
            request.Content = urlEncodedContent;
            Console.WriteLine("{0} {1}", method.ToString(), _apiUrl);
        }

        // Crear el header de la petición POST (opcional)
        foreach(var header in _headers)
        {
            request.Headers.Add(header.Key, header.Value);
        }

        // Enviar petición a la api de twitch y retornar el contenido de la
        // respuesta, comprobando antes que no sea inválida
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            throw new Exception("Twitch api returned error by invalid request");
        }

        // Solo extraer el contenido de la respuesta como JSON que es lo que
        // usa siempre la api de twitch
        return JsonDocument.Parse(response.Content.ReadAsStream());
    }

	public async Task<JsonDocument> PostAsync(HttpClient client)
	{
        return await SendAsync(client, HttpMethod.Post);
	}

    public async Task<JsonDocument> GetAsync(HttpClient client)
    {
        return await SendAsync(client, HttpMethod.Get);
    }

    public async Task<JsonDocument> PatchAsync(HttpClient client)
    {
        return await SendAsync(client, HttpMethod.Patch);
    }

    public async Task<string> GetUrlEncodedUri()
    {
        var urlQuery = "?" + await new FormUrlEncodedContent(_query)
            .ReadAsStringAsync();
        return _apiUrl + urlQuery;
    }
}
