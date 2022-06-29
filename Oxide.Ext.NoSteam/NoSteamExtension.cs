// Author:  Kaidoz
// Filename: NoSteamExtension.cs
// Last update: 2019.10.06 20:41

using Oxide.Core;
using Oxide.Core.Extensions;
using System;
using System.IO;
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

        public override VersionNumber Version => new VersionNumber(2, 0, 3);

        public override string Author => "Kaidoz";

        public static NoSteamExtension Instance { get; private set; }

        public static Assembly AssemblySteamworks = null;

        public override void Load()
        {
            if (_loaded)
                return;

            _loaded = true;
            LoadSteamwork();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Loader.NoSteam.InitPlugin();
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