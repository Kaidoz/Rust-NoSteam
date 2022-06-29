using HarmonyLib;
using Network;
using Oxide.Ext.NoSteam.Patch;
using Oxide.Ext.NoSteam.Utils;
using Oxide.Ext.NoSteam.Utils.Steam;
using Steamworks;
using System.Collections.Generic;
using System.Reflection;


namespace Oxide.Ext.NoSteam
{
    public static class Core
    {
        static Core()
        {
            OnAuthenticatedLocal = typeof(EACServer).GetMethod("OnAuthenticatedLocal", BindingFlags.Static | BindingFlags.NonPublic);

            OnAuthenticatedRemote = typeof(EACServer).GetMethod("OnAuthenticatedRemote", BindingFlags.Static | BindingFlags.NonPublic);

            StatusPlayers = new Dictionary<ulong, BeginAuthResult>();
        }

        internal static Harmony HarmonyInstance;

        internal static readonly MethodInfo OnAuthenticatedLocal;

        internal static readonly MethodInfo OnAuthenticatedRemote;

        internal static readonly Dictionary<ulong, BeginAuthResult> StatusPlayers;

        public static void Start()
        {
            DoPatch();
            SteamPatch.PatchSteamBeginPlayer();
        }

        private static void DoPatch()
        {
            HarmonyInstance = new Harmony("com.github.rust.exp");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
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

        public static bool CheckIsSteamConnection(Connection connection)
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

        public static bool CheckIsSteamConnection(ulong userid)
        {
            if (StatusPlayers.ContainsKey(userid) == false)
                return false;

            if (Rust.Defines.appID == 480)
                return true;

            return StatusPlayers[userid] == BeginAuthResult.OK;
        }

        public static bool CheckIsValidConnection(ulong userid, SteamTicket steamTicket)
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

        private static void AuthenticateLocal(Connection connection)
        {
            OnAuthenticatedLocal.Invoke(null, new object[] { connection });
            OnAuthenticatedRemote.Invoke(null, new object[] { connection });
        }
    }
}