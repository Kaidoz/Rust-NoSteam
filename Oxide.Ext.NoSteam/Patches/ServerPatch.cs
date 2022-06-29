using HarmonyLib;

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

        [HarmonyPatch(typeof(ServerMgr), "UpdateServerInformation")]
        private static class UpdateServerInformationPatch
        {
            [HarmonyPrefix]
            private static void Prefix()
            {
                int count = Core.CountSteamPlayer();

                int countNoSteam = BasePlayer.activePlayerList.Count - count;

                string strCountNoSteam = "ki" + countNoSteam;
                string strHasNoSteam = "ki";
                string strCountGs = "fl" + countNoSteam;

                ConVar.Server.tags = strCountNoSteam + "," + strCountGs + "," + strHasNoSteam;
            }
        }
    }
}
