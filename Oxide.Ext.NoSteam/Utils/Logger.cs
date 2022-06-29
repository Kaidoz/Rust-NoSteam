using Oxide.Core;

namespace Oxide.Ext.NoSteam.Utils
{
    public static class Logger
    {
        public static void Print(string text)
        {
            Interface.Oxide.LogWarning("[NoSteam] " + text);
        }
    }
}
