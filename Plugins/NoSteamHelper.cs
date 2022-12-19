using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;
using System;
using System.Collections.Generic;
using System.IO;
using static Oxide.Plugins.NoSteamHelperModels;

namespace Oxide.Plugins
{
    [Info("NoSteamHelper", "Kaidoz", "1.2.2")]
    [Description("A plugin that extends nosteam features")]
    // Support and other - https://discord.gg/snm7NGCAwH
    public class NoSteamHelper : RustPlugin
    {
        public NoSteamHelper()
        {
            Instance = this;
        }

        public static NoSteamHelper Instance;

        #region Variables

        internal static class Permissions
        {
            internal static string whitelist => Instance.Name + ".whitelist";
        }

        public static ConfigData configData;

        #endregion Variables

        #region Data

        internal static List<DataPlayer> Players = new List<DataPlayer>();

        internal static Dictionary<string, bool> CheckedIps = new Dictionary<string, bool>();

        #endregion Data

        private void InitData()
        {
            permission.RegisterPermission(Permissions.whitelist, this);

            Players =
                Interface.Oxide.DataFileSystem.ReadObject<List<DataPlayer>>("NoSteamHelper/Players");
        }

        protected override void LoadDefaultConfig()
        {
            
            var config = new ConfigData
            {
                Version = this.Version.ToString(),
                players = new ConfigData.Players()
                {
                    BlockSmurf = true,
                    BlockVpn = false,
                    ProtectLegitPlayers = false,
                    whiteListConfig = new ConfigData.Players.WhiteListConfig()
                    {
                        WhiteListEnabled = false
                    }
                },
                mirror = new ConfigData.Mirror(),
                blockVpn = new ConfigData.BlockVPN()
                {
                    Key = "Your key"
                }
            };
            SaveConfig(config);
        }

        private void LoadConfigVariables()
        {
            configData = Config.ReadObject<ConfigData>();

            if (configData.Version != this.Version.ToString())
            {
                Puts("Created default config");
                LoadDefaultConfig();
            }
        }

        private void SaveConfig(ConfigData config) => Config.WriteObject(config, true);

        #region Hooks

        private Exception ExceptionInit = null;

        private void OnServerInitialized()
        {
            if (ExceptionInit != null)
            {
                PrintError(ExceptionInit.ToString());
            }
        }

        private void Init()
        {
            try
            {
                LoadConfigVariables();
                SaveConfig();

                InitData();
            }
            catch (Exception ex)
            {
                ExceptionInit = ex;
            }
        }

        private void InitMirror()
        {
            if (configData.mirror.MirrorEnabled)
            {
                ConVar.Server.maxconnectionsperip = configData.mirror.MaxConnectionsPerIP;
            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            DataPlayer dataPlayer;
            var isExists = DataPlayer.FindPlayer(player.userID, out dataPlayer);

            if (isExists == false)
                PrintWarning("No data player: " + player.UserIDString);

            if (configData.players.BlockVpn)
            {
                if (isExists)
                {
                    if (dataPlayer.IsSteam() == false)
                    {
                        NoSteamHelperController.IsVpnConnection(webrequest, player, player.Connection);
                    }
                }
            }
        }

        private object OnBeginPlayerSession(Connection connection, bool IsLicense)
        {
            string strLicense = IsLicense ? "steam" : "nosteam";

            string strMirror = String.Empty;

            if (configData.mirror.MirrorEnabled)
            {
                if (configData.mirror.IPS.Contains(connection.ipaddress.GetCorrectIp()))
                {
                    strMirror = "(Mirror-Connection)";
                }
            }

            Puts($"Player {connection.username} {connection.userid} ({strLicense}) {strMirror} in process of connecting");

            if (IsLicense == false)
            {
                if (configData.players.whiteListConfig.WhiteListEnabled)
                {
                    string strId = connection.userid.ToString();

                    if (permission.UserHasPermission(strId, Permissions.whitelist) == false)
                    {
                        Puts("'WhiteList' enabled. This player is not whitelisted: " + strId);
                        return "You are not whitelisted";
                    }
                }
            }

            DataPlayer dataPlayer;

            bool isExists = DataPlayer.FindPlayer(connection.userid, out dataPlayer);

            if (isExists)
            {
                if (configData.players.ProtectLegitPlayers)
                {
                    if (dataPlayer.IsSteam() && IsLicense == false)
                    {
                        Puts("'ProtectLegitPlayers' enabled! Attempt connect to legit account from cracked: " + connection.userid);
                        return "Dont try get access to another player";
                    }
                }

                dataPlayer.ChangeSteam(IsLicense);
            }
            else
                DataPlayer.AddPlayer(connection.userid, IsLicense, connection.ipaddress);

            if (configData.players.BlockSmurf)
            {
                ulong steamid = 0UL;
                if (IsLicense == false)
                {
                    if (NoSteamHelperController.IsSmurf(connection, ref steamid))
                    {
                        Puts("'BlockSmurf' enabled. Detected smurf account of cracked player. Main account: " + steamid);
                        return "Your primary account: " + steamid;
                    }

                }

            }

            return null;
        }

        #endregion Hooks

    }


    public class NoSteamHelperModels
    {
        public class ConfigData
        {
            [JsonProperty("Version")]
            public string Version;

            [JsonProperty("Players")]
            public Players players;

            [JsonProperty("Block VPN Config")]
            public BlockVPN blockVpn;

            [JsonIgnore]
            public Mirror mirror;

            public class Players
            {
                [JsonProperty("BlockVPN. Block vpn for nosteam players(Need api key)")]
                public bool BlockVpn { get; set; }

                [JsonProperty("BlockSmurf. Block smurf accounts for cracked players(recommended TRUE)")]
                public bool BlockSmurf { get; set; }

                [JsonProperty("ProtectLegitPlayers. Block access to legit accounts from cracked client(recommended TRUE)")]
                public bool ProtectLegitPlayers { get; set; }

                [JsonProperty("White List Config")]
                public WhiteListConfig whiteListConfig { get; set; }

                public class WhiteListConfig
                {
                    [JsonProperty("WhiteList. White List enabled(perm: 'nosteamhelper.whitelist'")]
                    public bool WhiteListEnabled { get; set; }
                }
            }

            public class BlockVPN
            {
                [JsonProperty("Api key(http://proxycheck.io)")]
                public string Key { get; set; }
            }

            public class Mirror
            {
                [JsonProperty("Enabled")]
                public bool MirrorEnabled { get; set; } = false;

                [JsonProperty("Exclude IPS")]
                public List<string> IPS { get; set; } = new List<string>()
                {
                    "127.0.0.1"
                };

                [JsonProperty("Max connection per IP('Server.MaxConnectionsPerIP')")]
                public int MaxConnectionsPerIP { get; set; } = 20;
            }
        }

        public class DataPlayer
        {
            public bool Steam;
            public ulong SteamId;
            public string LastIp;

            public DataPlayer(ulong id, bool steam, string lastip)
            {
                SteamId = id;
                Steam = steam;
                LastIp = lastip;
            }

            public static void AddPlayer(ulong id, bool steam, string lastip)
            {
                lastip = lastip.GetCorrectIp();


                NoSteamHelper.Players.Add(new DataPlayer(id, steam, lastip));
                NoSteamHelperController.SaveDataPlayers();
            }

            public void ChangeSteam(bool steam)
            {
                Steam = steam;

                NoSteamHelperController.SaveDataPlayers();
            }

            public static bool FindPlayer(ulong steamid, out DataPlayer dataPlayer)
            {
                dataPlayer = null;
                foreach (var player in NoSteamHelper.Players)
                    if (player.SteamId == steamid)
                    {
                        dataPlayer = player;
                        return true;
                    }

                return false;
            }

            public bool IsSteam()
            {
                return Steam;
            }
        }
    }

    public static class NoSteamHelperController
    {

        internal static void IsVpnConnection(WebRequests webrequest, BasePlayer player, Connection connection)
        {
            if (NoSteamHelper.configData.blockVpn.Key == "Your key" || string.IsNullOrEmpty(NoSteamHelper.configData.blockVpn.Key))
                return;

            bool detectedVpn = false;
            string ip = connection.ipaddress.GetCorrectIp();

            if (ip == "127.0.0.1")
                return;

            if (NoSteamHelper.configData.mirror.MirrorEnabled)
            {
                if (NoSteamHelper.configData.mirror.IPS.Contains(ip))
                    return;
            }

            if (NoSteamHelper.CheckedIps.ContainsKey(ip) && NoSteamHelper.CheckedIps[ip])
                player.Kick("VPN Detected");

            if (NoSteamHelper.CheckedIps.ContainsKey(ip))
            {
                return;
            }


            webrequest.EnqueueGet($"http://proxycheck.io/v2/{ip}?key={NoSteamHelper.configData.blockVpn.Key}&vpn=1", (code, response) =>
            {
                detectedVpn = CheckResult(code, response, ip);

                if (detectedVpn)
                    player.Kick("VPN Detected");

                NoSteamHelper.CheckedIps.Add(ip, detectedVpn);
                SaveDataIps();
            }, NoSteamHelper.Instance);
        }

        private static bool CheckResult(int code, string response, string ip)
        {
            if (response.Contains("yes"))
                return true;

            if (response.Contains("error"))
            {
                Interface.Oxide.LogInfo(response.Replace("{", ip + " {"));
            }

            return false;
        }

        internal static bool IsSmurf(Connection connection, ref ulong userid)
        {
            foreach (var player in NoSteamHelper.Players)
            {
                bool IsCracked = player.IsSteam() == false;
                if (IsCracked)
                {
                    if (player.SteamId != connection.userid &&
                        player.LastIp == connection.ipaddress.GetCorrectIp())
                    {
                        userid = player.SteamId;
                        return true;
                    }
                }
            }

            return false;
        }

        public static void SaveDataPlayers()
        {
            Interface.Oxide.DataFileSystem.WriteObject("NoSteamHelper/Players", NoSteamHelper.Players);
        }

        public static void SaveDataIps()
        {
            Interface.Oxide.DataFileSystem.WriteObject("NoSteamHelper/CheckedIps", NoSteamHelper.CheckedIps);
        }
    }

    public static class StringExt
    {
        public static string GetCorrectIp(this string str)
        {
            string ip = str;
            ip = ip.Substring(0, ip.IndexOf(":"));

            return ip;
        }
    }
}