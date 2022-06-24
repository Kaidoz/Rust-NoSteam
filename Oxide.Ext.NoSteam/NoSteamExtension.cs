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

        public static bool DEBUG = true;

        public NoSteamExtension(ExtensionManager manager) : base(manager)
        {
            Instance = this;
        }

        public override string Name => "NoSteam";

        public override VersionNumber Version => new VersionNumber(2, 0, 0);

        public override string Author => "Kaidoz";

        public static NoSteamExtension Instance { get; private set; }

        public static Assembly AssemblySteamworks = null;

        public override void Load()
        {
            if (_loaded)
                return;

            _loaded = true;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            Loader.NoSteam.InitPlugin();

        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
#if DEBUG
            Interface.GetMod().LogDebug("CurrentDomain_AssemblyResolve1");

#endif
            if (args.Name.Contains("Facepunch.Steamworks") == false)
                return null;

            if (AssemblySteamworks != null)
                return AssemblySteamworks;

#if DEBUG
            Interface.GetMod().LogDebug("CurrentDomain_AssemblyResolve2");

#endif
            string pathWin = Path.Combine(Interface.GetMod().ExtensionDirectory, "Facepunch.Steamworks.Win64.dll");

            string pathLinux = Path.Combine(Interface.GetMod().ExtensionDirectory, "Facepunch.Steamworks.Posix.dll");

            if (File.Exists(pathWin))
                AssemblySteamworks = Assembly.UnsafeLoadFrom(pathWin);
            else
                if (File.Exists(pathLinux))
                AssemblySteamworks = Assembly.UnsafeLoadFrom(pathLinux);

#if DEBUG
            Interface.GetMod().LogDebug("CurrentDomain_AssemblyResolve3");

#endif
            return AssemblySteamworks;
        }

        public override void OnModLoad()
        {
            Load();
        }
    }
}