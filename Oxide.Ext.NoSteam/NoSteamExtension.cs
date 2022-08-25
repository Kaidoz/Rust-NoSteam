// Author:  Kaidoz
// Filename: NoSteamExtension.cs
// Last update: 2019.10.06 20:41

using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Ext.NoSteam.Utils;
using System;
using System.IO;
using System.Net;
using System.Reflection;

namespace Oxide.Ext.NoSteam
{
    public class NoSteamExtension : Extension
    {
        private bool _loaded;

        public static bool DEBUG = false;

        public NoSteamExtension(ExtensionManager manager) : base(manager)
        {
            Instance = this;
        }

        public override string Name => "NoSteam";

        public override VersionNumber Version => new VersionNumber(2, 1, 1);

        public override string Author => "Kaidoz";

        public static NoSteamExtension Instance { get; private set; }

        public static Assembly AssemblySteamworks = null;

        public override void Load()
        {
            if (_loaded)
                return;

            Update();
            _loaded = true;
            LoadSteamwork();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Loader.NoSteam.InitPlugin();
        }

        private void Update()
        {
            var webClient = new WebClient();

            string version = webClient.DownloadString("https://raw.githubusercontent.com/Kaidoz/Rust-NoSteam/master/Build/version.txt");

            Logger.Print($"Check update NoSteam. Actual version is {version}...");

            if (version != null && Version.ToString() != version)
            {
                Logger.Print($"Updating NoSteam[{version}]...");

                byte[] data = webClient.DownloadData("https://raw.githubusercontent.com/Kaidoz/Rust-NoSteam/master/Build/Oxide.Ext.NoSteam.dll");

                string path = Path.Combine(Interface.GetMod().ExtensionDirectory, "Oxide.Ext.NoSteam.dll");

                File.WriteAllBytes(path, data);

                Logger.Print("Restarting server...");

                ServerMgr.RestartServer("Restarting", 0);
            }
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {

            if (args.Name.Contains("Facepunch.Steamworks") == false)
                return null;

            if (AssemblySteamworks != null)
                return AssemblySteamworks;

            string pathWin = Path.Combine(Interface.GetMod().ExtensionDirectory, "Facepunch.Steamworks.Win64.dll");

            string pathLinux = Path.Combine(Interface.GetMod().ExtensionDirectory, "Facepunch.Steamworks.Posix.dll");

            if (File.Exists(pathWin))
                LoadSteamworks(pathWin);
            else
                if (File.Exists(pathLinux))
                LoadSteamworks(pathLinux);

            return AssemblySteamworks;
        }

        private void LoadSteamwork()
        {
            string pathWin = Path.Combine(Interface.GetMod().ExtensionDirectory, "Facepunch.Steamworks.Win64.dll");

            string pathLinux = Path.Combine(Interface.GetMod().ExtensionDirectory, "Facepunch.Steamworks.Posix.dll");

            if (File.Exists(pathWin))
                LoadSteamworks(pathWin);
            else
                if (File.Exists(pathLinux))
                LoadSteamworks(pathLinux);
        }

        private void LoadSteamworks(string path)
        {
            AssemblySteamworks = Assembly.UnsafeLoadFrom(path);
        }

        public override void OnModLoad()
        {
            Load();
        }
    }
}