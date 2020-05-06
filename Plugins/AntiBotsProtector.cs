using System.Collections.Generic;
using System.Text.RegularExpressions;
using Network;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("AntiBotsProtector", "Kaidoz", "0.1")]
    [Description("")]
    public class AntiBotsProtector : RustPlugin
    {
        private const string Steam64 = "^7656119([0-9]{10})$";

        private object CanNewConnection(Connection connection, bool isSteam)
        {
            if (isSteam)
                return null;

            if (CheckIsValidPlayer(connection))
            {
                return "Steam Auth Failed.";
            }

            return null;
        }

        private bool CheckIsValidPlayer(Connection connection)
        {
            if (!Regex.IsMatch(connection.ownerid.ToString(), Steam64))
                return false;

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player.Connection.ipaddress == connection.ipaddress)
                {
                    player.Kick("Steam Auth Failed.");
                    return false;
                }
            }

            return true;

        }
    }
}
