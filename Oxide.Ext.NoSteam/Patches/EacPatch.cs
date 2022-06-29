using HarmonyLib;
using Network;

namespace Oxide.Ext.NoSteam.Patch
{
    internal class EacPatch
    {
        [HarmonyPatch(typeof(EACServer), nameof(EACServer.OnJoinGame))]
        private static class Patch01
        {
            [HarmonyPrefix]
            private static bool Prefix(Connection connection)
            {
                Core.OnAuthenticatedLocal.Invoke(null, new object[] { connection });
                Core.OnAuthenticatedRemote.Invoke(null, new object[] { connection });
                return false;
            }
        }
    }
}
