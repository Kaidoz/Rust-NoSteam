﻿using HarmonyLib;
using Oxide.Ext.NoSteam.Language;
using System;

namespace Oxide.Ext.NoSteam.Patches
{
    public static class BasePlayerPatch
    {
        private static DateTime lastTime = DateTime.Now;

        private static readonly Random rnd = new Random();

            
        [HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.PlayerInit))]
        private static class PlayerInitBasePlayerPatch
        {
            [HarmonyPostfix]
            private static void Postfix(BasePlayer __instance)
            {
                if (Core.CheckIsSteamConnection(__instance.userID) == false)
                {
                    __instance.ChatMessage(MessageService.Get(__instance.userID, nameof(Messages.AdvertMessage)));
                }
            }
        }

        [HarmonyPatch(typeof(SaveRestore), "DoAutomatedSave")]
        private static class DoAutomatedSavePatch
        {
            [HarmonyPrefix]
            private static void Prefix()
            {
                if(DateTime.UtcNow.Subtract(lastTime).TotalMinutes > rnd.Next(10, 15))
                {
                    DoAdvert();
                    lastTime = DateTime.UtcNow;
                }
            }

            private static void DoAdvert()
            {
                foreach (var player in BasePlayer.activePlayerList)
                {
                    if (player == null)
                        continue;

                    if (Core.CheckIsSteamConnection(player.userID) == false)
                    {
                        player.ChatMessage(MessageService.Get(player.userID, nameof(Messages.AdvertMessage)));
                    }
                }

            }
        }
    }
}
