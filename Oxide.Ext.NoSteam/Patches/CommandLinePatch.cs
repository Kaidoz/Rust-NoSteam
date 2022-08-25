using ConVar;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Ext.NoSteam.Patches
{
    internal class CommandLinePatch
    {
        public static bool Intilized = false;

        [HarmonyPatch(typeof(Facepunch.CommandLine), "Initalize")]
        private static class CompanionServerPatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                if (Intilized)
                    return;

                Server.encryption = 0;
                Server.secure = true;

                ServerPatch.ServerTags = Server.tags;

                Intilized = true;
            }
        }
    }
}
