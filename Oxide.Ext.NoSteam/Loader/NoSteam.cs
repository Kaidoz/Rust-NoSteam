﻿using ConVar;
using Oxide.Core;
using Oxide.Core.Logging;
using Oxide.Core.Plugins;
using System;

namespace Oxide.Ext.NoSteam.Loader
{
    internal class NoSteam : CSPlugin
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
            Output("[NoSteam] Author: Kaidoz" +
                "\n Discord:discord.gg/Tn3kzbE" +
                "\n Github: github.com/Kaidoz/Rust-NoSteam");
            Init();
            try
            {
                Core.Start();
            }
            catch (Exception ex)
            {
                Output("Error patching: " + ex);
            }
        }

        private static void Init()
        {
            Server.encryption = 0;
        }

        public static void Output(string text)
        {
            Interface.Oxide.RootLogger.Write(LogType.Info, text);
        }
    }
}