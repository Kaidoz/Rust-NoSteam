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
    [Info("NoSteamHelper", "Kaidoz", "1.2.0")]
    [Description("A plugin that extends nosteam features")]
    internal class NoSteamHelper : RustPlugin
    {
        public NoSteamHelper()
        {
            Instance = this;
        }

        internal static NoSteamHelper Instance;

        #region Variables

        internal static class Permissions
        {
            internal static string whitelist => Instance.Name + ".whitelist";
        }

        public static ConfigData ConfigData;

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
                AppId = 252490,
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
                blockVpn = new ConfigData.BlockVPN()
                {
                    Key = "Your key"
                }
            };
            SaveConfig(config);
        }

        private void LoadConfigVariables()
        {
            ConfigData = Config.ReadObject<ConfigData>();

            if (ConfigData.Version != this.Version.ToString())
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

                NoSteamHelperController.InitAppId();
            }
            catch (Exception ex)
            {
                ExceptionInit = ex;
            }
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            DataPlayer dataPlayer;
            var isExists = DataPlayer.FindPlayer(player.userID, out dataPlayer);

            if (isExists == false)
                PrintWarning("No data player: " + player.UserIDString);

            if (ConfigData.players.BlockVpn)
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
            Puts($"Player({strLicense}) in process of connecting");

            if (IsLicense == false)
            {
                if (ConfigData.players.whiteListConfig.WhiteListEnabled)
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
                if (ConfigData.players.ProtectLegitPlayers)
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

            if (ConfigData.players.BlockSmurf)
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

            [JsonProperty("AppId Rust-'252490' Cracked Rust-'480'")]
            public int? AppId;

            [JsonProperty("Players")]
            public Players players;

            [JsonProperty("Block VPN Config")]
            public BlockVPN blockVpn;

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

        internal static void InitAppId()
        {
            Rust.Defines.appID = NoSteamHelper.ConfigData.AppId != null ? (uint)NoSteamHelper.ConfigData.AppId : 0;
        }

        internal static void IsVpnConnection(WebRequests webrequest, BasePlayer player, Connection connection)
        {
            if (NoSteamHelper.ConfigData.blockVpn.Key == "Your key" || string.IsNullOrEmpty(NoSteamHelper.ConfigData.blockVpn.Key))
                return;

            bool status = false;
            string ip = connection.ipaddress.GetCorrectIp();

            if (NoSteamHelper.CheckedIps.ContainsKey(ip) && NoSteamHelper.CheckedIps[ip])
                player.Kick("VPN Detected");

            webrequest.EnqueueGet($"http://proxycheck.io/v2/{ip}?key={NoSteamHelper.ConfigData.blockVpn.Key}&vpn=1", (code, response) =>
            {
                status = CheckResult(code, response, ip);

                if (status)
                    player.Kick("VPN Detected");

                NoSteamHelper.CheckedIps.Add(ip, status);
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