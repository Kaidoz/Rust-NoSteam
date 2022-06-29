using HarmonyLib;
using System;
using AppServer = CompanionServer.Server;

namespace Oxide.Ext.NoSteam.Patches
{
    public static class OtherPatch
    {
        [HarmonyPatch(typeof(DeveloperList), nameof(DeveloperList.Contains), new Type[] { typeof(string) })]
        private class ConnectionAuthPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(string steamid, ref bool __result)
            {
                __result = false;

                return false;
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
    }
}
