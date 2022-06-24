using Network;
using Newtonsoft.Json;
using Oxide.Core;
using System.Collections.Generic;

namespace Oxide.Plugins
{
    [Info("NoSteamHelper", "Kaidoz", "1.1.2")]
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


        private readonly Dictionary<string, string> DiscordHeaders = new Dictionary<string, string>
        {
                { "Content-Type", "application/json" }
        };

        public static ConfigData configData;

        #endregion Variables

        #region Class

        public class ConfigData
        {
            [JsonProperty("AppId Rust-'252495' Cracked Rust-'480'")]
            public int? AppId;

            [JsonProperty("Players")]
            public Players players;

            [JsonProperty("Block VPN Config")]
            public BlockVPN blockVpn;

            public class Players
            {
                [JsonProperty("Block vpn for nosteam players(Need api key)")]
                public bool BlockVpn { get; set; }

                [JsonProperty("Block smurf accounts for nosteam players(recommended TRUE)")]
                public bool BlockSmurf { get; set; }

                [JsonProperty("Block access to license accounts from nosteam(recommended TRUE)")]
                public bool BlockChangerSteamID { get; set; }

                [JsonProperty("White List Config")]
                public WhiteListConfig whiteListConfig { get; set; }

                public class WhiteListConfig
                {
                    [JsonProperty("White List enabled(perm: 'nosteamhelper.whitelist'")]
                    public bool WhiteListEnabled { get; set; }
                }
            }

            public class BlockVPN
            {
                [JsonProperty("Api key(http://proxycheck.io)")]
                public string Key { get; set; }
            }
        }

        #endregion Class

        #region Data

        public static List<DataPlayer> _players = new List<DataPlayer>();

        private static Dictionary<string, bool> _checkedIps = new Dictionary<string, bool>();

        #endregion Data

        #region API

        private object IsPlayerNoSteam(ulong steamid)
        {
            DataPlayer dataPlayer;
            var result = DataPlayer.FindPlayer(steamid, out dataPlayer);

            if (result == false)
            {
                return false;
            }

            if (dataPlayer.IsSteam())
                return null;

            return false;
        }

        #endregion API

        private void InitData()
        {
            permission.RegisterPermission(Permissions.whitelist, this);

            _players =
                Interface.Oxide.DataFileSystem.ReadObject<List<DataPlayer>>("NoSteamHelper/Players");
        }

        private static void SaveDataPlayers()
        {
            Interface.Oxide.DataFileSystem.WriteObject("NoSteamHelper/Players", _players);
        }

        private static void SaveDataIps()
        {
            Interface.Oxide.DataFileSystem.WriteObject("NoSteamHelper/CheckedIps", _checkedIps);
        }

        protected override void LoadDefaultConfig()
        {
            var config = new ConfigData
            {
                AppId = 252495,
                players = new ConfigData.Players()
                {
                    BlockSmurf = true,
                    BlockVpn = false,
                    BlockChangerSteamID = true,
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

        private void LoadConfigVariables() => configData = Config.ReadObject<ConfigData>();

        private void SaveConfig(ConfigData config) => Config.WriteObject(config, true);

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
                lastip = lastip.Split(':')[1];

                _players.Add(new DataPlayer(id, steam, lastip));
                SaveDataPlayers();
            }

            public void ChangeSteam(bool steam)
            {
                Steam = steam;
                SaveDataPlayers();
            }

            public static bool FindPlayer(ulong steamid, out DataPlayer dataPlayer)
            {
                dataPlayer = null;
                foreach (var player in _players)
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

        #region VPN

        private void IsVpnConnection(BasePlayer player, Connection connection)
        {
            if (configData.blockVpn.Key == "Your key" || string.IsNullOrEmpty(configData.blockVpn.Key))
                return;

            bool status = false;
            string ip = connection.ipaddress;
            ip = ip.Substring(0, ip.IndexOf(":"));

            if (_checkedIps.ContainsKey(ip) && _checkedIps[ip])
                player.Kick("VPN Detected");

            webrequest.EnqueueGet($"http://proxycheck.io/v2/{ip}?key={configData.blockVpn.Key}&vpn=1", (code, response) =>
            {
                status = CheckResult(code, response, ip);

                if (status)
                    player.Kick("VPN Detected");

                _checkedIps.Add(ip, status);
                SaveDataIps();
            }, this);
        }

        private bool CheckResult(int code, string response, string ip)
        {
            if (response.Contains("yes"))
                return true;

            if (response.Contains("error"))
            {
                Puts(response.Replace("{", ip + " {"));
            }

            return false;
        }

        #endregion VPN

        #region Hooks

        private void Init()
        {
            LoadConfigVariables();
            SaveConfig();

            InitData();

            InitAppId();
        }

        private void InitAppId()
        {
            Rust.Defines.appID = configData.AppId != null ? (uint)configData.AppId : 0;

            Puts(configData.AppId.ToString());
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
                        IsVpnConnection(player, player.Connection);
                    }

                }

            }
        }

        private object OnBeginPlayerSession(Connection connection, bool playerIsLicense)
        {
            string strLicense = playerIsLicense ? "steam" : "nosteam";
            Puts($"Player({strLicense}) in process of connecting");

            if (playerIsLicense == false)
            {
                if (configData.players.whiteListConfig.WhiteListEnabled)
                {
                    string strId = connection.userid.ToString();

                    if (permission.UserHasPermission(strId, Permissions.whitelist) == false)
                    {
                        return "You are not whitelisted";
                    }
                }
            }

            DataPlayer dataPlayer;

            bool isExists = DataPlayer.FindPlayer(connection.userid, out dataPlayer);

            if (isExists)
            {
                if (configData.players.BlockChangerSteamID)
                {

                    if (dataPlayer.IsSteam() && playerIsLicense == false)
                    {
                        Puts("BlockChangerSteamID Enabled! Attempt connect to license account from nosteam: " + connection.userid);
                        return "Dont try get access to another player";
                    }
                }
            }
            else
                DataPlayer.AddPlayer(connection.userid, playerIsLicense, connection.ipaddress);


            if (configData.players.BlockSmurf)
            {
                ulong steamid = 0UL;
                if (playerIsLicense == false)
                {
                    if (IsSmurf(connection, ref steamid))
                        return "Your primary account: " + steamid;
                }

            }

            return null;
        }

        private bool IsSmurf(Connection connection, ref ulong userid)
        {
            foreach (var player in _players)
            {
                bool isNoSteam = !player.IsSteam();
                if (isNoSteam)
                {
                    if (player.SteamId != connection.userid &&
                        player.LastIp == connection.ipaddress)
                    {
                        userid = player.SteamId;
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Hooks
    }
}