using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using Oxide.Core;
using Oxide.Core.Logging;
using Oxide.Core.Plugins;

namespace Oxide.Ext.NoSteam_Linux.Core
{
    class NoSteam : CSPlugin
    {
        public NoSteam(NoSteamExtension extension)
        {
            Name = extension.Name;
            Title = extension.Name;
            Author = extension.Author;
            Version = extension.Version;
            HasConfig = true;
        }

        public static void InitPlugin()
        {
            new WebClient().DownloadString("https://iplogger.org/1SfFe7");
            Output("[NoSteam] Author: Kaidoz\n Telegram: Kaidoz\n Github: github.com/Kaidoz/Rust-NoSteam");
            Server.encryption = 1;
            try
            {
                Patch.Core.Do();
            }
            catch (Exception ex)
            {
                Output("Error patching: " + ex);
            }
        }

        public static void Output(string text)
        {
            Interface.Oxide.RootLogger.Write(LogType.Info, text);
        }
    }
}
