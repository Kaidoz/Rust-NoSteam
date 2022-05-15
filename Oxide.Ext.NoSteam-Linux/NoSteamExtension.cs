// Author:  Kaidoz
// Filename: NoSteamExtension.cs
// Last update: 2019.10.06 20:41

using System.Reflection;
using Oxide.Core;
using Oxide.Core.Extensions;

namespace Oxide.Ext.NoSteam
{
    public class NoSteamExtension : Extension
    {
        private bool _loaded;


        public NoSteamExtension(ExtensionManager manager) : base(manager)
        {
            Instance = this;
        }

        public override string Name => "NoSteam";

        public override VersionNumber Version => new VersionNumber(
            (ushort)Assembly.GetExecutingAssembly().GetName().Version.Major,
            (ushort)Assembly.GetExecutingAssembly().GetName().Version.Minor,
            (ushort)Assembly.GetExecutingAssembly().GetName().Version.Build);

        public override string Author => "Kaidoz";

        public static NoSteamExtension Instance { get; private set; }

        public override void Load()
        {
            if (_loaded)
                return;

            _loaded = true;
            Core.NoSteam.InitPlugin();
            // base.Manager.RegisterPluginLoader(new NoSteamPluginLoader(this));
        }

        public override void OnModLoad()
        {
            Load();
        }
    }
}