using SDG.Unturned;
using Steamworks;

namespace Rocket.Unturned.Helpers
{

    internal static class RocketUtilities
    {
        public static bool TryGetSteamIdFromText(string text, out CSteamID? steamId)
        {
            steamId = null;
            if (text.StartsWith("7656"))
            {
                if (ulong.TryParse(text, out var parsedSteamId) == false)
                {
                    return false;
                }
                steamId = new CSteamID(parsedSteamId);
                return true;
            }
            var player = GetPlayerFromName(text);
            if (player == null)
            {
                return false;
            }

            steamId = player.channel.owner.playerID.steamID;
            return true;
        }
        public static SDG.Unturned.Player? GetPlayerFromName(string targetPlayerName)
        {
            SDG.Unturned.Player? targetPlayer = null;
            if (targetPlayerName.StartsWith("7656"))
            {
                targetPlayer = PlayerTool.getPlayer(new CSteamID(ulong.Parse(targetPlayerName)));
            }
            if (targetPlayer == null)
            {
                targetPlayer = PlayerTool.getPlayer(targetPlayerName);
            }
            return targetPlayer;
        }
    }
}