// Author:  Kaidoz
// Filename: Core.cs
// Last update: 2019.10.06 4:59

using System.Linq;
using System.Reflection;
using Network;
using Oxide.Core;
using Oxide.Core.Logging;
using Oxide.Ext.NoSteam.Helper;
using Steamworks;

namespace Oxide.Ext.NoSteam.Patch
{
    internal class Core
    {
        private static readonly HookManager HookManager = new HookManager();

        public static void Do()
        {
            var origMeth = FindMethod(typeof(ConnectionAuth), "OnNewConnection");
            var replMeth = FindMethod(typeof(Runtime), "OnNewConnection");

            var origMeth2 = typeof(ServerMgr).GetRuntimeMethods().Single(x => x.Name == "DoTick");
            var replMeth2 = FindMethod(typeof(Runtime), "DoTick");

            var origMeth3 = FindMethod(typeof(ConnectionAuth), "Reject");
            var replMeth3 = FindMethod(typeof(Runtime), "Reject");

            HookManager.Hook(origMeth, replMeth);
            HookManager.Hook(origMeth2, replMeth2);
            HookManager.Hook(origMeth3, replMeth3);
        }

        private static MethodInfo FindMethod(System.Type type, string name)
        {
            try
            {
                return type.GetMethods().Single(x => x.Name == name);
            }
            catch
            {
                Interface.Oxide.RootLogger.Write(LogType.Info, "[NoSteam] Error at FindMethod: " + type.Name + " " + name);
                return null;
            }
        }

        public static void CheckConnection(Connection connection)
        {
            connection.authStatus = "ok";
            SteamTicket steamTicket = new SteamTicket(connection);
            if (steamTicket.Ticket.Token.AppID == 480)
                connection.authStatus = "ok";
        }
    }
}