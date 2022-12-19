using Harmony;
using Network;
using System;

namespace Oxide.Ext.NoSteam.Patches
{
    internal class ServerPatch
    {

        [HarmonyPatch(typeof(ServerMgr))]
        [HarmonyPatch("get_AvailableSlots")]
        internal static class ServerPatch3
        {
            [HarmonyPrefix]
            public static bool Prefix(ref int __result)
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

        [HarmonyPatch(typeof(Bootstrap), nameof(Bootstrap.Init_Tier0))]
        internal static class Init_Tier0_Patch
        {
            [HarmonyPostfix]
            public static void Postfix()
            {
                Bootstrap.NetworkInitRaknet();
            }
        }

        [HarmonyPatch(typeof(Server), "get_ProtocolId")]
        internal static class get_ProtocolId_Patch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref string __result)
            {
                __result = "sw";

                return false;
            }
        }
    }
}
