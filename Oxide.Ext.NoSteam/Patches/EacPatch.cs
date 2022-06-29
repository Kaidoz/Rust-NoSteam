using HarmonyLib;
using Network;

namespace Oxide.Ext.NoSteam.Patches
{
    internal class EacPatch
    {
        [HarmonyPatch(typeof(EACServer), nameof(EACServer.OnJoinGame))]
        private static class Patch01
        {
            [HarmonyPrefix]
            private static bool Prefix(Connection connection)
            {
                if (Core.CheckIsSteamConnection(connection.userid) == true && Rust.Defines.appID != 480)
				{
                    return true;
				}

                Core.OnAuthenticatedLocal.Invoke(null, new object[] { connection });
                Core.OnAuthenticatedRemote.Invoke(null, new object[] { connection });

                return false;
            }
        }
    }
}
