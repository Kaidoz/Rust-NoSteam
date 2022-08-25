using Oxide.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Ext.NoSteam.Utils
{
    internal static class SteamworksLoader
    {
        static SteamworksLoader()
        {
            LoadSteamworks();
        }

        internal static Assembly Assembly = null;

        private static void LoadSteamworks()
        {
            string pathWin = Path.Combine(Interface.GetMod().ExtensionDirectory, "Facepunch.Steamworks.Win64.dll");

            string pathLinux = Path.Combine(Interface.GetMod().ExtensionDirectory, "Facepunch.Steamworks.Posix.dll");

            if (File.Exists(pathWin))
                LoadSteamworks(pathWin);
            else
                if (File.Exists(pathLinux))
                LoadSteamworks(pathLinux);
        }

        private static void LoadSteamworks(string path)
        {
            Assembly = Assembly.LoadFrom(path);
        }
    }
}
