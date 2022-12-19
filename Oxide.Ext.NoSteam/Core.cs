using Harmony;
using Network;
using Oxide.Ext.NoSteam.Patches;
using Oxide.Ext.NoSteam.Utils;
using Oxide.Ext.NoSteam.Utils.Steam;
using Oxide.Ext.NoSteam.Utils.Steam.Steamworks;
using System.Collections.Generic;
using System.Reflection;


namespace Oxide.Ext.NoSteam
{
    public static class Core
    {
        static Core()
        {
            StatusPlayers = new Dictionary<ulong, BeginAuthResult>();
        }

        internal static HarmonyInstance HarmonyInstance;

        internal static readonly Dictionary<ulong, BeginAuthResult> StatusPlayers;

        internal static void Start()
        {
            DoPatch();
            SteamPatch.PatchSteamBeginPlayer();
            SteamPatch.PatchSteamServerTags();
        }

        private static void DoPatch()
        {
            HarmonyInstance = HarmonyInstance.Create("com.github.rust.exp");
            HarmonyInstance.DEBUG = false;
            HarmonyInstance.PatchAll();
        }

        internal static int CountSteamPlayer()
        {
            int count = 0;

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (CheckIsSteamConnection(player.userID) == true)
                {
                    count++;
                }
            }
            return count;
        }

        internal static bool CheckIsSteamConnection(Connection connection)
        {
            if (connection == null)
                return false;

            var steamTicket = new SteamTicket(connection);

            if (steamTicket.clientVersion == SteamTicket.ClientVersion.Steam || Rust.Defines.appID == 480)
            {
                return true;
            }

            return false;
        }

        internal static void CheckServerParameters()
        {
            if (ConVar.Server.encryption > 0)
            {
                Logger.Print("'server.encryption' should was been '0'");
            }

            if (ConVar.Server.secure == false)
            {
                Logger.Print("'server.secure' should was been '1'");
            }
        }

        internal static bool CheckIsSteamConnection(ulong userid)
        {
            if (Rust.Defines.appID == 480)
                return true;

            if (StatusPlayers.ContainsKey(userid) == false)
                return false;

            return StatusPlayers[userid] == BeginAuthResult.OK;
        }

        internal static bool CheckIsValidConnection(ulong userid, SteamTicket steamTicket)
        {
            if (StatusPlayers.ContainsKey(userid) == false)
                return false;

            bool authResult = false;
            switch (StatusPlayers[userid])
            {
                case BeginAuthResult.OK:
                case BeginAuthResult.GameMismatch:
                    authResult = true;
                    break;
            }

            if (authResult == false)
                return false;

            if (steamTicket.Ticket.SteamID != userid)
                return false;

            return true;
        }
    }
}