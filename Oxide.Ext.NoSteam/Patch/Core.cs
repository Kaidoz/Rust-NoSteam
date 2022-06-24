using HarmonyLib;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Ext.NoSteam.Utils.Steam;
using Rust.Platform.Steam;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppServer = CompanionServer.Server;

namespace Oxide.Ext.NoSteam.Patch
{
    public static class Core
    {
        static Core()
        {

        }

        private static Harmony _Harmony;

        private static readonly MethodInfo OnAuthenticatedLocal = typeof(EACServer).GetMethod("OnAuthenticatedLocal", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly MethodInfo OnAuthenticatedRemote = typeof(EACServer).GetMethod("OnAuthenticatedRemote", BindingFlags.Static | BindingFlags.NonPublic);

        private static readonly Dictionary<ulong, BeginAuthResult> Players = new Dictionary<ulong, BeginAuthResult>();

        public static void Do()
        {
            DoPatch();
            PatchBeginPlayer();
        }

        private static void DoPatch()
        {
            _Harmony = new Harmony("com.github.rust.exp");
            _Harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void PatchBeginPlayer()
        {
            var type = NoSteamExtension.AssemblySteamworks.DefinedTypes.First(x => x.Name == "ISteamGameServer");

            MethodBase OnBeginPlayerSeesion = type.GetDeclaredMethod("BeginAuthSession");

            var harmonyMethod = new HarmonyMethod(typeof(SteamPlatformBeginPlayer2), "HarmonyPostfix");

            _Harmony.Patch(OnBeginPlayerSeesion, null, harmonyMethod);
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.LoadPlayerStats))]
        private static class SteamPlatformLoadPlayerStats
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId, ref Task<bool> __result)
            {
                if (CheckIsSteamConnection(userId) == false || Rust.Defines.appID == 480)
                {
                    __result = Task.FromResult(true);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(EACServer), nameof(EACServer.OnJoinGame))]
        private static class Patch01
        {
            [HarmonyPrefix]
            private static bool Prefix(Connection connection)
            {
                OnAuthenticatedLocal.Invoke(null, new object[] { connection });
                OnAuthenticatedRemote.Invoke(null, new object[] { connection });
                return false;
            }
        }

        [HarmonyPatch(typeof(ConnectionAuth), nameof(ConnectionAuth.OnNewConnection))]
        private class ConnectionAuthPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(ConnectionAuth __instance, Connection connection)
            {
                if (DeveloperList.Contains(connection.userid))
                {
                    ConnectionAuth.Reject(connection, "Developer SteamId");
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamInventory), "UpdateSteamInventory")]
        private static class SteamInventoryPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(BaseEntity.RPCMessage msg)
            {
                if (Rust.Defines.appID == 480)
                    return false;

                if (CheckIsSteamConnection(msg.connection) == false)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Auth_Steam), nameof(Auth_Steam.ValidateConnecting))]
        private static class Auth_SteamPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(ref bool __result, ulong steamid, ulong ownerSteamID, AuthResponse response)
            {
                __result = true;

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.UpdatePlayerSession))]
        private static class SteamPlatformUpdatePlayer
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId, string userName)
            {
                if (CheckIsSteamConnection(userId) == false)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.EndPlayerSession))]
        private static class SteamPlatformEndPlayer
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId)
            {
                if (CheckIsSteamConnection(userId) == false)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.BeginPlayerSession))]
        private static class SteamPlatformBeginPlayer
        {
            [HarmonyPrefix]
            private static bool HarmonyPrefix(ulong userId, byte[] authToken, ref bool __result)
            {
                SteamServer.BeginAuthSession(authToken, userId);

                if (CheckIsValidConnection(userId) == false)
                {
                    __result = false;

                    if (NoSteamExtension.DEBUG)
                    {
                        Log("CheckIsValidConnection: " + userId);
                    }

                    return false;
                }

                var ticket = new SteamTicket(authToken);

                bool IsLicense = ticket.clientVersion == SteamTicket.ClientVersion.Steam;

                var connections = ConnectionAuth.m_AuthConnection;
                var connection = connections.First(x => x.userid == userId);

                connection.authStatus = "ok";

                AuthenticateLocal(connection);

                __result = true;

                if (Interface.CallHook("OnBeginPlayerSessionToken", ticket) != null)
                {
                    string tk = JsonConvert.SerializeObject(ticket, Formatting.Indented);
                    Interface.GetMod().LogWarning(tk);
                }

                object reason = Interface.CallHook("OnBeginPlayerSession", connection, IsLicense);

                if (reason == null)
                {
                    return false;
                }

                ConnectionAuth.Reject(connection, reason.ToString(), null);

                return false;
            }
        }

        private static class SteamPlatformBeginPlayer2
        {
            [HarmonyPostfix]
            private static void HarmonyPostfix(IntPtr pAuthTicket, int cbAuthTicket, SteamId steamID, ref BeginAuthResult __result)
            {
                if (NoSteamExtension.DEBUG)
                {
                    Log("SteamPlatformBeginPlayer2: " + steamID + " " + __result);
                    //StackTrace.
                }

                if (Players.ContainsKey(steamID) == false)
                {
                    Players.Add(steamID, __result);
                }
                else
                {
                    Players[steamID] = __result;
                }
            }
        }

        [HarmonyPatch(typeof(ServerMgr))]
        [HarmonyPatch("get_AvailableSlots")]
        private static class ServerPatch3
        {
            [HarmonyPrefix]
            private static bool Prefix(ref int __result)
            {
                __result = ConVar.Server.maxplayers - CountSteamPlayer();

                return false;
            }
        }

        [HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.PlayerInit))]
        private static class PlayerInitBasePlayerPatch
        {
            [HarmonyPostfix]
            private static void Postfix(BasePlayer __instance)
            {
                if (CheckIsSteamConnection(__instance.userID) == false)
                {
                    __instance.ChatMessage("Server uses NoSteam. \n" +
                    "Discord: discord.gg/Tn3kzbE\n" +
                    "Github: github.com/Kaidoz/Rust-NoSteam");
                }
            }
        }

        [HarmonyPatch(typeof(AppServer), nameof(AppServer.Initialize))]
        private static class CompanionServerPatch
        {
            [HarmonyPrefix]
            private static bool Prefix()
            {
                ConVar.App.port = -1;
                return false;
            }
        }

        private static readonly Regex steamCountRegex = new Regex(@"cp?.\d", RegexOptions.Compiled);

        private static readonly Regex nosteamCountRegex = new Regex(@"ki?.\d", RegexOptions.Compiled);

        [HarmonyPatch(typeof(SteamServer))]
        [HarmonyPatch("set_GameTags")]
        private static class SteamServerPatch3
        {
            [HarmonyPrefix]
            private static bool Prefix(ref string value)
            {
                int count = CountSteamPlayer();
                int countNoSteam = BasePlayer.activePlayerList.Count - count;
                string strCount = "cp" + count;
                string strCountNoSteam = "ki" + countNoSteam;

                value = steamCountRegex.Replace(value, strCount);

                if (value.Contains("ki") == false)
                {
                    value = value.Replace("ptrak", "ptrak," + strCountNoSteam);
                }
                else
                    value = nosteamCountRegex.Replace(value, strCountNoSteam);

                return true;
            }
        }

        private static int CountSteamPlayer()
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

            if (steamTicket.clientVersion == SteamTicket.ClientVersion.Steam)
            {
                return true;
            }

            return false;
        }

        public static bool CheckIsSteamConnection(ulong userid)
        {
            if (Rust.Defines.appID == 480)
                return true;

            return Players[userid] == BeginAuthResult.OK;
        }

        public static bool CheckIsValidConnection(ulong userid)
        {
            return Players[userid] != BeginAuthResult.InvalidTicket;
        }

        private static void AuthenticateLocal(Connection connection)
        {
            OnAuthenticatedLocal.Invoke(null, new object[] { connection });
            OnAuthenticatedRemote.Invoke(null, new object[] { connection });
        }

        public static void Log(string text)
        {
            Interface.Oxide.LogWarning("[NoSteam] " + text);
        }
    }
}