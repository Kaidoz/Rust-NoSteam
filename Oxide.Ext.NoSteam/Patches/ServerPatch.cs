using Harmony;
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


        [HarmonyPatch(typeof(ServerMgr), nameof(ServerMgr.GamemodeName))]
        internal static class UpdateServerInformationPatch
        {
            [HarmonyPostfix]
            public static void Postfix(ref string __result)
            {
                int count = Core.CountSteamPlayer();

                int countNoSteam = BasePlayer.activePlayerList.Count - count;

                string strCountNoSteam = "ki" + BasePlayer.activePlayerList.Count;
                string strHasNoSteamNew = "kdz";
                string strHasNoSteam = "ki";
                string strCountGs = "fl" + countNoSteam;


                string tags = "," + strHasNoSteamNew + "," + strHasNoSteam + "," + strCountNoSteam + "," + strCountGs;


                __result += tags;
            }
        }

    }
}
