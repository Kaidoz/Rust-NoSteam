using ConVar;
using Harmony;
using Oxide.Core;
using Oxide.Ext.NoSteam.Utils;
using Oxide.Ext.NoSteam.Utils.Steam;
using Oxide.Ext.NoSteam.Utils.Steam.Steamworks;
using Rust.Platform.Steam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Oxide.Ext.NoSteam.Patches
{
    internal static class SteamPatch
    {
       
        private static Dictionary<ulong, BeginAuthResult> StatusPlayers => Core.StatusPlayers;

        internal static void PatchSteamBeginPlayer()
        {
            var type = SteamworksLoader.Assembly.DefinedTypes.First(x => x.Name == "ISteamGameServer");

            MethodBase OnBeginPlayerSeesion = type.GetDeclaredMethod("BeginAuthSession");

            var harmonyMethod = new HarmonyMethod(typeof(SteamPlatformBeginPlayer2), "HarmonyPostfix");

            Core.HarmonyInstance.Patch(OnBeginPlayerSeesion, null, harmonyMethod);
        }

        internal static void PatchSteamServerTags()
        {
            var type = SteamworksLoader.Assembly.DefinedTypes.First(x => x.Name == "SteamServer");

            MethodBase set_GameTags = type.GetDeclaredMethod("set_GameTags");

            var harmonyMethod = new HarmonyMethod(typeof(SteamServerPatch3), "Prefix");

            Core.HarmonyInstance.Patch(set_GameTags, harmonyMethod);
        }

        private static System.Random rnd = new System.Random();

        private static readonly Regex steamCountRegex = new Regex(@"cp*[0-9]+", RegexOptions.Compiled);

        internal static class SteamServerPatch3
        {
            [HarmonyPrefix]
            public static void Prefix(ref string value)
            {
                int count = Core.CountSteamPlayer();

                object intValue = Interface.CallHook("OnSetTagsCountPlayers", count);

                if (intValue != null && intValue is int)
                    count = (int)intValue;

                if (Config.configData.fakePlayers.Enabled)
                {
                    int rndCount = rnd.Next(Config.configData.fakePlayers.MinCount, Config.configData.fakePlayers.MaxCount);
                    count += rndCount;
                }

                if (count >= Server.maxplayers)
                {
                    count = Server.maxplayers - 1;
                }

                string strCount = "cp" + count;

                value = steamCountRegex.Replace(value, strCount);
            }
        }

        [HarmonyPatch(typeof(SteamInventory), "UpdateSteamInventory")]
        internal static class SteamInventoryPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(BaseEntity.RPCMessage msg)
            {
                if (Rust.Defines.appID == 480)
                    return false;

                if (Core.CheckIsSteamConnection(msg.connection) == false)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.UpdatePlayerSession))]
        internal static class SteamPlatformUpdatePlayer
        {
            [HarmonyPrefix]
            public static bool Prefix(ulong userId, string userName)
            {
                if (Core.CheckIsSteamConnection(userId) == false)
                {
                    return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Auth_Steam), nameof(Auth_Steam.ValidateConnecting))]
        internal static class Auth_SteamPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ulong steamid, ulong ownerSteamID, AuthResponse response, ref bool __result)
            {
                __result = true;

                return false;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.EndPlayerSession))]
        internal static class SteamPlatformEndPlayer
        {
            [HarmonyPrefix]
            public static bool Prefix(ulong userId)
            {
                bool result = true;

                if (Core.CheckIsSteamConnection(userId) == false)
                    result = false;

                if (StatusPlayers.ContainsKey(userId))
                    StatusPlayers.Remove(userId);

                return result;
            }
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.BeginPlayerSession))]
        internal static class SteamPlatformBeginPlayer
        {
            [HarmonyPostfix]
            public static void HarmonyPrefix(ulong userId, byte[] authToken, ref bool __result)
            {
                Core.CheckServerParameters();

                var ticket = new SteamTicket(authToken);

                if (Core.CheckIsValidConnection(userId, ticket) == false)
                {
                    __result = false;

                    if (NoSteamExtension.DEBUG)
                    {
                        Logger.Print("CheckIsValidConnection: " + userId);
                    }

                    return;
                }

                bool IsLicense = ticket.clientVersion == SteamTicket.ClientVersion.Steam;

                var connections = ConnectionAuth.m_AuthConnection;
                var connection = connections.First(x => x.userid == userId);

                connection.authStatus = "ok";

                __result = true;

                object reason = Interface.CallHook("OnBeginPlayerSession", connection, IsLicense);

                if (reason == null)
                {
                    return;
                }

                ConnectionAuth.Reject(connection, reason.ToString(), null);

                return;
            }
        }

        internal static class SteamPlatformBeginPlayer2
        {
            [HarmonyPostfix]
            public static void HarmonyPostfix(IntPtr pAuthTicket, int cbAuthTicket, SteamId steamID, ref BeginAuthResult __result)
            {
                if (NoSteamExtension.DEBUG)
                {
                    Logger.Print("SteamPlatformBeginPlayer2: " + steamID + " " + __result);
                }

                if (StatusPlayers.ContainsKey(steamID) == false)
                {
                    StatusPlayers.Add(steamID, __result);
                }
                else
                {
                    StatusPlayers[steamID] = __result;
                }
            }
        }

        [HarmonyPatch(typeof(SteamPlatform), nameof(SteamPlatform.LoadPlayerStats))]
        private static class SteamPlatformLoadPlayerStats
        {
            [HarmonyPrefix]
            private static bool Prefix(ulong userId, ref Task<bool> __result)
            {
                if (Core.CheckIsSteamConnection(userId) == false || Rust.Defines.appID == 480)
                {
                    __result = Task.FromResult(true);
                    return false;
                }

                return true;
            }
        }
    }
}
