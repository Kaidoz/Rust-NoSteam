using Epic.OnlineServices.AntiCheatCommon;
using Harmony;
using Network;
using System.Reflection;
using System.Collections.Generic;

namespace Oxide.Ext.NoSteam.Patches
{
    internal static class EacPatch
    {
        internal static readonly MethodInfo OnAuthenticatedLocal;

        internal static readonly MethodInfo OnAuthenticatedRemote;
        static EacPatch()
        {
            OnAuthenticatedLocal = typeof(EACServer).GetMethod("OnAuthenticatedLocal", BindingFlags.Static | BindingFlags.NonPublic);

            OnAuthenticatedRemote = typeof(EACServer).GetMethod("OnAuthenticatedRemote", BindingFlags.Static | BindingFlags.NonPublic);
        }

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
                
                OnAuthenticatedLocal.Invoke(null, new object[] { connection });
                OnAuthenticatedRemote.Invoke(null, new object[] { connection });

                return false;
            }
        }
    }
}
