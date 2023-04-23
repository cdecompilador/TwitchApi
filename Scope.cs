using System.Text;

namespace TwitchApi;

// TODO: No se me ocurre una forma de hacerlo mejor por ahora, ojalá hubiese 
// macros aquí :'(
public class Scopes
{
    public const byte NoScope = 0;
    public const byte ChannelManagePolls = 1 << 0;
    public const byte ChannelManageBroadcast = 1 << 1;
    public const byte ChannelManageVips = 1 << 2;
    public const byte UserReadFollows = 1 << 3;
    public const byte UserReadSubscriptions = 1 << 4;

    public static string ToUrlEncodedString(byte scopes)
    {
        StringBuilder sb = new StringBuilder();
        if ((scopes & Scopes.ChannelManagePolls) != 0) 
            sb.Append("channel:manage:polls ");
        if ((scopes & Scopes.ChannelManageBroadcast) != 0)
            sb.Append("channel:manage:broadcast ");
        if ((scopes & Scopes.ChannelManageVips) != 0)
            sb.Append("channel:manage:vips ");
        if ((scopes & Scopes.UserReadFollows) != 0)
            sb.Append("user:read:follows ");
        if ((scopes & Scopes.UserReadSubscriptions) != 0)
            sb.Append("user:read:subscriptions ");

        if (sb.Length != 0)
            sb.Remove(sb.Length - 1, 1);

        return sb.ToString();
    }

    public static void UpdateFromString(ref int scopes, string scopeStr)
    {
        if (scopeStr == "channel:manage:polls")
            scopes &= Scopes.ChannelManagePolls;
        if (scopeStr == "channel:manage:broadcast")
            scopes &= Scopes.ChannelManageBroadcast;
        if (scopeStr == "channel:manage:vips")
            scopes &= Scopes.ChannelManageVips;
        if (scopeStr == "user:read:follows")
            scopes &= Scopes.UserReadFollows;
        if (scopeStr == "user:read:subscriptions")
            scopes &= Scopes.UserReadSubscriptions;
    }
}
