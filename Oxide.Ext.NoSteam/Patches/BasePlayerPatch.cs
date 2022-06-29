using HarmonyLib;

namespace Oxide.Ext.NoSteam.Patches
{
    public static class BasePlayerPatch
    {
        [HarmonyPatch(typeof(BasePlayer), nameof(BasePlayer.PlayerInit))]
        private static class PlayerInitBasePlayerPatch
        {
            [HarmonyPostfix]
            private static void Postfix(BasePlayer __instance)
            {
                if (Core.CheckIsSteamConnection(__instance.userID) == false)
                {
                    __instance.ChatMessage("Server uses NoSteam by Kaidoz. \n" +
                    "Discord: discord.gg/Tn3kzbE\n" +
                    "Github: github.com/Kaidoz/Rust-NoSteam");
                }
            }
        }
    }
}
