﻿using ConVar;
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
        [HarmonyPatch(typeof(Facepunch.CommandLine), "Initalize")]
        private static class CompanionServerPatch
        {
            [HarmonyPostfix]
            private static void Postfix()
            {
                Server.encryption = 0;
                Server.secure = true;
            }
        }
    }
}
