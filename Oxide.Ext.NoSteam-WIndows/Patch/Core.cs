using HarmonyLib;
using Network;
using Oxide.Core;
using Oxide.Ext.NoSteam.Helper;
using Rust.Platform.Steam;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppServer = CompanionServer.Server;

namespace Oxide.Ext.NoSteam.Patch
{
    internal static class Core
    {
        private static Dictionary<ulong, bool> Players = new Dictionary<ulong, bool>();

        private static ConnectionAuth connectionAuth = default;

        private static FieldInfo field_waitingList;

        private static MethodInfo OnAuthenticatedLocal;

        private static MethodInfo OnAuthenticatedRemote;

        public static void Do()
        {
            ParseReflections();
            DoPatch();
        }

        public static void DoPatch()
        {
            var harmony = new Harmony("com.github.rust.exp");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void ParseReflections()
        {
            field_waitingList = typeof(Auth_Steam).GetRuntimeFields().First(x => x.Name == "waitingList");

            OnAuthenticatedLocal = typeof(EACServer).GetMethod("OnAuthenticatedLocal", BindingFlags.Static | BindingFlags.NonPublic);
            OnAuthenticatedRemote = typeof(EACServer).GetMethod("OnAuthenticatedRemote", BindingFlags.Static | BindingFlags.NonPublic);
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.LoadPlayerStats))]
        private static class SteamPlatformLoadPlayerStats
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId, ref Task<bool> __result)
            {
                if (CheckIsNoSteamConnection(userId))
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
                if (connectionAuth == default)
                    connectionAuth = __instance;

                if (DeveloperList.Contains(connection.userid))
                {
                    ConnectionAuth.Reject(connection, "Developer SteamId");
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamInventory), nameof(SteamInventory.OnRpcMessage))]
        private static class SteamInventoryPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(BasePlayer player, uint rpc, Message msg)
            {
                if (CheckIsNoSteamConnection(msg.connection))
                    return false;

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
                if (CheckIsNoSteamConnection(userId))
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.EndPlayerSession))]
        private static class SteamPlatformEndPlayer
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId)
            {
                if (CheckIsNoSteamConnection(userId))
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
                bool IsLicense = Steamworks.SteamServer.BeginAuthSession(authToken, userId);

                var ticket = new SteamTicket(authToken);

                ticket.GetClientVersion();

                if (Players.ContainsKey(userId))
                    Players[userId] = IsLicense;
                else
                {
                    Players.Add(userId, IsLicense);
                }

                var connections = ConnectionAuth.m_AuthConnection;
                var connection = connections.First(x => x.userid == userId);

                if (IsLicense == false)
                {
                    connection.authStatus = "ok";

                    OnAuthenticatedLocal.Invoke(null, new object[] { connection });
                    OnAuthenticatedRemote.Invoke(null, new object[] { connection });
                }

                __result = true;

                object reason = Interface.CallHook("OnBeginPlayerSession", connection, IsLicense);

                if (reason == null)
                {
                    return false;
                }


                ConnectionAuth.Reject(connection, reason.ToString(), null);

                return false;
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
                if (CheckIsNoSteamConnection(__instance.userID))
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

        private static readonly Regex SteamServerRegular = new Regex(@"cp?.\d", RegexOptions.Compiled);

        [HarmonyPatch(typeof(Steamworks.SteamServer))]
        [HarmonyPatch("set_GameTags")]
        private static class SteamServerPatch3
        {
            [HarmonyPrefix]
            private static bool Prefix(ref string value)
            {
                string count = default;

                string tags = SteamServerRegular.Replace(value, match =>
                {
                    if (Interface.CallHook("IsShowCracked") != null)
                    {
                        count = "cp" + BasePlayer.activePlayerList.Count;
                    }
                    else
                    {
                        count = "cp" + Core.CountSteamPlayer().ToString();
                    }
                    return count;
                });

                if (string.IsNullOrEmpty(count))
                {
                    return true;
                }

                object result = Interface.CallHook(
                    "OnGameTags",
                    tags,
                    count.Replace("cp", ""));

                if (result == null)
                    value = tags;
                else
                    value = result.ToString();

                return true;
            }
        }

        private static int CountSteamPlayer()
        {
            int count = 0;

            foreach (var player in BasePlayer.activePlayerList)
            {
                if (Players.ContainsKey(player.userID) && Players[player.userID] == true)
                {
                    count++;
                }
            }
            return count;
        }

        public static bool CheckIsNoSteamConnection(Connection connection)
        {
            var steamTicket = new SteamTicket(connection);

            //steamTicket.GetClientVersion();

            //if (steamTicket.clientVersion == SteamTicket.ClientVersion.Steam)
            if (Players.ContainsKey(connection.userid) == true && Players[connection.userid] == true)
            {
                return false;
            }
            return true;
        }

        public static bool CheckIsNoSteamConnection(ulong userid)
        {
            if (Players.ContainsKey(userid) == true && Players[userid] == true)
            {
                return false;
            }
            return true;
        }

        public static void Log(string text)
        {
            Interface.Oxide.LogWarning("[NoSteam] " + text);
        }
    }
}