using HarmonyLib;
using Steamworks;

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

                // ФИКС КРИТИЧЕСКОГО БАГА в КРИВОМ НОУСТИМЕ, КОТОРЫЙ ВЛИЯЛ РОВНО СЧЕТОМ НИ НА ЧТО - kiListHashSet`1[BasePlayer]
                string strCountAll = "ki" + BasePlayer.activePlayerList.Count;

                string strHasNoSteam = "ki";

                // Поддержка так называемого говна куска - GameStores, украинского ватника
                string strCountGs = "fl" + countNoSteam;

                if (string.IsNullOrEmpty(ConVar.Server.tags))
                    ConVar.Server.tags = strHasNoSteam + "," + strCountAll + "," + strCountGs;
                else
                    ConVar.Server.tags += "," + strHasNoSteam + "," + strCountAll + "," + strCountGs;
            }
        }
    }
}
