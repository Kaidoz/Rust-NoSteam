using Harmony;
using Oxide.Ext.NoSteam.Language;
using Oxide.Ext.NoSteam.Utils;
using System;
using System.Linq;

namespace Oxide.Ext.NoSteam.Patches
{
    public static class BasePlayerPatch
    {
        private static DateTime lastTime = DateTime.Now;

        private static readonly Random rnd = new Random();

        
        [HarmonyPatch(
            typeof(EACServer), 
            nameof(EACServer.OnStartLoading)
            )]
        private static class PlayerInitBasePlayerPatch
        {
            [HarmonyPrefix]
            private static void Prefix(Network.Connection connection)
            {
                var player = BasePlayer.FindByID(connection.userid);

                if (player == null)
                    return;

                if (Core.CheckIsSteamConnection(player.userID) == false)
                {
                    player.ChatMessage(MessageService.Get(player.userID, nameof(Messages.AdvertMessage)));
                }
            }
        }


        [HarmonyPatch(typeof(SaveRestore), "DoAutomatedSave")]
        private static class DoAutomatedSavePatch
        {
            [HarmonyPrefix]
            private static void Prefix()
            {
                if (DateTime.UtcNow.Subtract(lastTime).TotalMinutes > rnd.Next(10, 15))
                {
                    if (Config.configData.CheckPatronCode == false)
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
                        if (Config.configData.CheckPatronCode == false)
                            player.ChatMessage(MessageService.Get(player.userID, nameof(Messages.AdvertMessage)));
                    }
                }

            }
        }
    }
}
