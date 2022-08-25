using HarmonyLib;
using Network;
using Oxide.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Ext.NoSteam.Patches
{
    internal static class OxidePatch
    {
        private static bool IsSteamPlayer(ulong steamId) => Core.CheckIsSteamConnection(steamId);

        internal delegate bool OxideHook(ref object __result, string hookname, params object[] args);

        internal static event OxideHook OnHookOxide;

        static OxidePatch()
        {
            OnHookOxide += OxidePatch_OnHookOxide;
        }

        private static bool OxidePatch_OnHookOxide(ref object __result, string hookname, params object[] args)
        {
            if (hookname == "IsSteam")
            {
                if (args.Length != 1)
                    return true;

                HandleIsSteam(ref __result, args);

                return false;
            }

            return true;
        }

        private static bool HandleIsSteam(ref object __result, params object[] args)
        {
            ulong steamid = 0;

            object arg = args[0];

            if (arg is ulong)
            {
                steamid = (ulong)arg;
            }

            if (arg is BasePlayer)
            {
                steamid = (arg as BasePlayer).userID;
            }

            if (arg is Connection)
            {
                steamid = (arg as Connection).userid;
            }

            __result = IsSteamPlayer(steamid);

            return false;
        }

        [HarmonyPatch(typeof(OxideMod), nameof(OxideMod.CallHook))]
        public static class DoTickPatch
        {
            [HarmonyPrefix]
            public static bool Prefix(ref object __result, string hookname, params object[] args)
            {
                bool result = OxidePatch.OnHookOxide?.Invoke(ref __result, hookname, args) == false;

                if (result)
                    return false;

                return true;
            }
        }


    }
}
