namespace TwitchApi;

public interface IRequest<R>
{
    public byte Scope { get; }
    public byte StateRequirements { get; } 

    public Task<R> Send(
            AccessToken accessToken,
            TwitchSecrets secrets, 
            HttpClient client);
}
