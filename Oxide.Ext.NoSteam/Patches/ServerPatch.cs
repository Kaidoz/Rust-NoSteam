using HarmonyLib;
using Steamworks;
using System;

namespace Oxide.Ext.NoSteam.Patches
{
    internal class ServerPatch
    {

        [HarmonyPatch(typeof(ServerMgr))]
        [HarmonyPatch("get_AvailableSlots")]
        private static class ServerPatch3
        {
            [HarmonyPrefix]
            private static bool Prefix(ref int __result)
            {
                __result = ConVar.Server.maxplayers - Core.CountSteamPlayer();

                return false;
            }
        }

        private static string _serverTags;

        private static string _customTags;

        private static string ServerTags
        {
            get
            {
                return _serverTags;
            }
            set
            {
                if (string.IsNullOrEmpty(_serverTags) == false)
                    return;

                if (value != _customTags)
                    _serverTags = value;
            }
        }

        private static DateTime dateTime = DateTime.MinValue;

        private static bool checkedTags = false;

        [HarmonyPatch(typeof(ServerMgr), "UpdateServerInformation")]
        internal static class UpdateServerInformationPatch
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                if (IsServerLoaded == false)
                    return;

                int count = Core.CountSteamPlayer();

                int countNoSteam = BasePlayer.activePlayerList.Count - count;

                string strCountNoSteam = "ki" + BasePlayer.activePlayerList.Count;
                string strHasNoSteamNew = "kdz";
                string strHasNoSteam = "ki";
                string strCountGs = "fl" + countNoSteam;

                if (checkedTags == false)
                {
                    ServerTags = ConVar.Server.tags;
                    checkedTags = true;
                }

                ServerTags = ConVar.Server.tags;

                _customTags = strHasNoSteamNew + "," + strHasNoSteam + "," + strCountNoSteam + "," + strCountGs;

                if (string.IsNullOrEmpty(ServerTags))
                {
                    ConVar.Server.tags = _customTags;
                }

                else
                    ConVar.Server.tags = ServerTags + "," + _customTags;
            }
        }

        private static bool IsServerLoaded
        {
            get
            {
                if (dateTime == DateTime.MinValue)
                {
                    dateTime = DateTime.Now;
                }

                if (DateTime.Now.Subtract(dateTime).TotalSeconds < 20)
                    return false;

                return true;
            }

        }
    }
}
