// Author:  Kaidoz
// Filename: NoSteamExtension.cs
// Last update: 2019.10.06 20:41

using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Ext.NoSteam.Utils;
using Oxide.Plugins;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using VLB;
using static ConsoleSystem;

namespace Oxide.Ext.NoSteam
{
    public class NoSteamExtension : Extension
    {
        private bool _loaded;

        public static bool DEBUG = true;

        public NoSteamExtension(ExtensionManager manager) : base(manager)
        {
            Instance = this;
        }

        public override string Name => "NoSteam";

        public override VersionNumber Version => new VersionNumber(2, 1, 9);

        public override string Author => "Kaidoz";

        public static NoSteamExtension Instance { get; private set; }

        

        public override void Load()
        {
            if (_loaded)
                return;

            LoadConfig();

            if (Utils.Config.configData.EnabledAutoUpdate)
                Update();

            _loaded = true;
            //LoadSteamwork();
            Loader.NoSteam.InitPlugin();
        }

        public static void LoadConfig()
        {
            Utils.Config.LoadConfig();

            Rust.Defines.appID = Utils.Config.configData.AppId != null ? (uint)Utils.Config.configData.AppId : 252490;
        }

        private void Update()
        {
            var webClient = new WebClient();

            var version = GetVersion(webClient);

            Logger.Print($"Check update NoSteam. Remote version is {version}...");

            if (CheckIsOutdated(version))
            {
                Logger.Print($"Updating NoSteam[{version}]...");

                DownloadPlugin(webClient);

                Logger.Print("Restarting server...");

                ServerMgr.RestartServer("Restarting", 0);

                Run(Option.Server, "restart 0 update-nosteam");
            }
        }

        private string GetVersion(WebClient webClient)
        {
            string urlVersion = "https://raw.githubusercontent.com/Kaidoz/Rust-NoSteam/master/Build/version.txt";

            string version = webClient.DownloadString(urlVersion);

            return version;
        }

        private void DownloadPlugin(WebClient webClient)
        {
            string urlDll = "https://raw.githubusercontent.com/Kaidoz/Rust-NoSteam/master/Build/Oxide.Ext.NoSteam.dll";

            byte[] data = webClient.DownloadData(urlDll);

            string pathToDll = Path.Combine(Interface.GetMod().ExtensionDirectory, "Oxide.Ext.NoSteam.dll");

            File.WriteAllBytes(pathToDll, data);
        }

        private bool CheckIsOutdated(string version)
        {
            var vers = System.Version.Parse(version);

            var pluginVersion = new System.Version(Version.Major, Version.Minor, Version.Patch);
            if (vers.CompareTo(pluginVersion) > 0)
                return true;

            return false;
        }

        public override void OnModLoad()
        {
            Load();
        }
    }
}