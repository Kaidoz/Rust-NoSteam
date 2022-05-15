using Facepunch;
using HarmonyLib;
using Network;
using Oxide.Core;
using Oxide.Ext.NoSteam.Helper;
using Rust.Platform.Steam;
using SilentOrbit.ProtocolBuffers;
using Steamworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;
using Facepunch.Math;
using Facepunch.Extend;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;

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


        /*

                [HarmonyPatch(typeof(ConnectionAuth))]
                [HarmonyPatch("OnNewConnection")]
                private static class ConnectionAuthPatch
                {
                    [HarmonyPrefix]
                    private static bool Prefix(ConnectionAuth __instance, Connection connection)
                    {
                        OnNewConnection(__instance, connection);

                        return false;
                    }

                    private static void OnNewConnection(ConnectionAuth __instance, Connection connection)
                    {
                        connection.connected = false;
                        if (connection.token == null || connection.token.Length < 32)
                        {
                            ConnectionAuth.Reject(connection, "Invalid Token", null);
                            return;
                        }
                        if (connection.userid == 0UL)
                        {
                            ConnectionAuth.Reject(connection, "Invalid SteamID", null);
                            return;
                        }
        #if !DEBUG
                        if (connection.protocol != Rust.Protocol.network)
                        {
                            if (!DeveloperList.Contains(connection.userid))
                            {
                                ConnectionAuth.Reject(connection, "Incompatible Version", null);
                                return;
                            }
                            DebugEx.Log("Not kicking " + connection.userid + " for incompatible protocol (is a developer)", StackTraceLogType.None);
                        }
        #endif
                        if (ServerUsers.Is(connection.userid, ServerUsers.UserGroup.Banned))
                        {
                            ServerUsers.User user = ServerUsers.Get(connection.userid);
                            string text = ((user != null) ? user.notes : null) ?? "no reason given";
                            string text2 = (user != null && user.expiry > 0L) ? (" for " + (user.expiry - (long)Epoch.Current).FormatSecondsLong()) : "";
                            ConnectionAuth.Reject(connection, string.Concat(new string[]
                            {
                        "You are banned from this server",
                        text2,
                        " (",
                        text,
                        ")"
                            }), null);
                            return;
                        }
                        if (ServerUsers.Is(connection.userid, ServerUsers.UserGroup.Moderator))
                        {
                            DebugEx.Log(connection.ToString() + " has auth level 1", StackTraceLogType.None);
                            connection.authLevel = 1u;
                        }
                        if (ServerUsers.Is(connection.userid, ServerUsers.UserGroup.Owner))
                        {
                            DebugEx.Log(connection.ToString() + " has auth level 2", StackTraceLogType.None);
                            connection.authLevel = 2u;
                        }
                        if (DeveloperList.Contains(connection.userid))
                        {
                            connection.authLevel = 0u;
                        }
                        if (Interface.CallHook("IOnUserApprove", connection) != null)
                        {
                            return;
                        }
                        ConnectionAuth.m_AuthConnection.Add(connection);
                        __instance.StartCoroutine(AuthorisationRoutine(__instance, connection));
                    }

                    private static IEnumerator AuthorisationRoutine(ConnectionAuth __instance, Connection connection)
                    {
                        yield return __instance.StartCoroutine(AuthSteam(connection));
                        yield return __instance.StartCoroutine(Auth_EAC.Run(connection));

                        if (connection.rejected || !connection.active)
                        {
                            yield break;
                        }

                        if (__instance.IsAuthed(connection.userid))
                        {
                            ConnectionAuth.Reject(connection, "You are already connected as a player!", null);
                            yield break;
                        }

                        __instance.Approve(connection);
                        yield break;
                    }

                    private static IEnumerator AuthSteam(Connection connection)
                    {
                        connection.authStatus = "";
                        if (!PlatformService.Instance.BeginPlayerSession(connection.userid, connection.token))
                        {
                            DebugEx.Log("Steam Auth Failed - AuthSteam", StackTraceLogType.None);
                            ConnectionAuth.Reject(connection, "Steam Auth Failed", null);
                            yield break;
                        }
                        var waitingList = (List<Connection>)field_waitingList.GetValue(null);
                        waitingList.Add(connection);
                        Stopwatch timeout = Stopwatch.StartNew();
                        while (timeout.Elapsed.TotalSeconds < 30.0 && connection.active && !(connection.authStatus != ""))
                        {
                            yield return null;
                        }
                        waitingList.Remove(connection);

                        yield break;
                    }
                }

                */

        [HarmonyPatch(typeof(ConnectionAuth))]
        [HarmonyPatch("OnNewConnection")]
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

                bool IsNosteam = !CheckIsNoSteamConnection(connection);
                var message = Interface.CallHook("CanNewConnection", connection, IsNosteam);

                if (message != null)
                {
                    ConnectionAuth.Reject(connection, message.ToString());
                    return false;
                }

                return true;
            }
        }


        [HarmonyPatch(typeof(SteamInventory))]
        [HarmonyPatch("OnRpcMessage")]
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

        [HarmonyPatch(typeof(EACServer))]
        [HarmonyPatch("OnJoinGame")]
        private static class Patch01
        {
            [HarmonyPrefix]
            private static bool Prefix(Connection connection)
            {
                if (CheckIsNoSteamConnection(connection))
                {
                    OnAuthenticatedLocal.Invoke(null, new object[] { connection });
                    OnAuthenticatedRemote.Invoke(null, new object[] { connection });
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(ServerMgr))]
        [HarmonyPatch("OnNetworkMessage")]
        private static class ServerMgrPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(Message packet)
            {
                if (packet.type == Message.Type.DisconnectReason)
                {
                    Interface.CallHook("OnClientDisconnect", packet.connection, "Disconnected");
                    Network.Net.sv.Kick(packet.connection, "Disconnected", false);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Auth_Steam))]
        [HarmonyPatch("ValidateConnecting")]
        private static class Auth_SteamPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(ref bool __result, ulong steamid, ulong ownerSteamID, AuthResponse response)
            {
                __result = true;

                return false;
            }
        }


        [HarmonyPatch(typeof(ConnectionAuth))]
        [HarmonyPatch("Reject")]
        class ConnectionAuthPatch2
        {
            [HarmonyPrefix]
            static bool Prefix(Connection connection, string strReason)
            {
                if (strReason.Contains("Steam Auth Timeout"))
                {
                    if (Interface.CallHook("OnSteamAuthFailed", connection) == null)
                    {
                        string userName = ConVar.Server.censorplayerlist ? RandomUsernames.Get(connection.userid + (ulong)((long)UnityEngine.Random.Range(0, 100000))) : connection.username;
                        PlatformService.Instance.UpdatePlayerSession(connection.userid, userName);
                        return false;
                    }
                }

                return true;
            }
        }

        #region STEAM


        [HarmonyPatch(typeof(SteamPlatform))]
        [HarmonyPatch("UpdatePlayerSession")]
        private static class SteamPlatformUpdatePlayer
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId, string userName)
            {
                if (Players.ContainsKey(userId) == false || Players[userId] == false)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform))]
        [HarmonyPatch("LoadPlayerStats")]
        private static class SteamPlatformLoadPlayerStats
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId, ref Task<bool> __result)
            {
                if (Players.ContainsKey(userId) == false || Players[userId] == false)
                {
                    __result = Task.FromResult(true);
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform))]
        [HarmonyPatch("EndPlayerSession")]
        private static class SteamPlatformEndPlayer
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId)
            {
                if (Players.ContainsKey(userId) == false || Players[userId] == false)
                    return false;

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform))]
        [HarmonyPatch("BeginPlayerSession")]
        private static class SteamPlatformBeginPlayer
        {
            [HarmonyPostfix]
            private static bool Postfix(ulong userId, byte[] authToken, ref bool __result)
            {
                var ticket = new SteamTicket(authToken);

                ticket.GetClientVersion();

                if (Players.ContainsKey(userId))
                    Players[userId] = __result;
                else
                {
                    Players.Add(userId, __result);
                }

                __result = true;

                return false;
            }
        }

        #endregion

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

        [HarmonyPatch(typeof(BasePlayer))]
        [HarmonyPatch("PlayerInit")]
        private static class PlayerInitBasePlayerPatch
        {
            [HarmonyPostfix]
            private static void Postfix(BasePlayer __instance)
            {
                __instance.ChatMessage("asdr");
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
                SteamTicket steamTicket = new SteamTicket(player.Connection);

                steamTicket.GetClientVersion();

                if (steamTicket.clientVersion == SteamTicket.ClientVersion.Steam)
                {
                    count++;
                }
            }
            return count;
        }

        public static bool CheckIsNoSteamConnection(Connection connection)
        {
            var steamTicket = new SteamTicket(connection);

            steamTicket.GetClientVersion();

            if (steamTicket.clientVersion == SteamTicket.ClientVersion.NoSteam)
            {
                return true;
            }
            return false;
        }
    }
}