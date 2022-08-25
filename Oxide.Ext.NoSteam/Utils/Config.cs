using Newtonsoft.Json;
using Oxide.Core;
using System;
using System.IO;
using System.Reflection;

namespace Oxide.Ext.NoSteam.Utils
{
    internal static class Config
    {

        internal static ConfigData configData;

        internal class ConfigData
        {
            [JsonProperty("Enabled auto update")]
            public bool EnabledAutoUpdate { get; set; } = true;

            //[JsonProperty("Enabled public data")]
            //public bool IsEnabledPublicDataBans { get; set; } = true;

            [JsonProperty("AppId (480 - SpaceWar, 252490 - Rust)")]
            public int? AppId { get; set; } = 252490;


            public string PatronCode { get; set; } = string.Empty;

            [JsonIgnore]
            public bool CheckPatronCode
            {
                get
                {
                    if(string.IsNullOrEmpty(PatronCode))
                    {
                        return false;
                    }

                    if (MD5Hash.Calculate(PatronCode) == "25a87896e4bb894363a33c1093da97dd")
                    {
                        return true;
                    }

                    return false;
                }
            }

            [JsonProperty("FakePlayers(work only with 480 AppId)")]
            public FakePlayers fakePlayers { get; set; } = new FakePlayers();

            public string Version { get; set; }

            internal ConfigData()
            {
            }

            internal ConfigData(string version)
            {
                Version = version;
            }

            public class FakePlayers
            {
                public bool Enabled { get; set; } = false;
                public int MinCount { get; set; } = 0;

                public int MaxCount { get; set; } = 0;
            }
        }

        internal static void LoadConfig()
        {

            string path = Path.Combine(Interface.Oxide.ConfigDirectory, "NoSteam" + ".json");

            if (File.Exists(path) == false)
            {
                LoadDefaultConfig();
                return;
            }

            try
            {
                string text = File.ReadAllText(path);

                configData = JsonConvert.DeserializeObject<ConfigData>(text);

                if(configData.Version != NoSteamExtension.Instance.Version.ToString())
                {
                    LoadDefaultConfig(configData);
                }    
            }
            catch
            {

            }

            CheckConfig();
        }

        internal static void LoadDefaultConfig(ConfigData OldConfigData = null)
        {
            string path = Path.Combine(Interface.Oxide.ConfigDirectory, "NoSteam" + ".json");

            configData = new ConfigData(NoSteamExtension.Instance.Version.ToString());

            if (OldConfigData != null)
            {
                configData.PatronCode = OldConfigData.PatronCode;
                configData.fakePlayers = OldConfigData.fakePlayers;
                configData.AppId = OldConfigData.AppId;
            }
            

            SaveConfig();
        }

        internal static void CheckConfig()
        {
            if (configData == null || configData.Version != NoSteamExtension.Instance.Version.ToString())
            {
                LoadDefaultConfig(configData);
            }
        }

        internal static void SaveConfig()
        {
            string path = Path.Combine(Interface.Oxide.ConfigDirectory, "NoSteam" + ".json");

            string text = JsonConvert.SerializeObject(configData, Formatting.Indented);

            File.WriteAllText(path, text);
        }
    }
}
