// Author:  Kaidoz
// Filename: NoSteamHelper.cs
// Last update: 2019.10.07 19:20

using System.Collections.Generic;
using Network;
using Newtonsoft.Json;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("NoSteamHelper", "Kaidoz", "1.0.4")]
    [Description("")]
    internal class NoSteamHelper : RustPlugin
    {

        private ConfigData configData;

        class ConfigData
        {
            public Other other;

            public Players players;

            public BlockVPN blockVpn;

            public class Other
            {
                [JsonProperty("Enable visibility noSteam players in servers list(DANGER! Risk get ban)")]
                public bool FakeOnline { get; set; }
            }

            public class Players
            {
                [JsonProperty("Block vpn for nosteam players")]
                public bool BlockVpn { get; set; }

                [JsonProperty("Block smurf accounts for nosteam players")]
                public bool BlockSmurf { get; set; }
            }

            public class BlockVPN
            {
                [JsonProperty("Api key(http://proxycheck.io)")]
                public string Key { get; set; }
            }
        }



        private static List<DataPlayer> _players = new List<DataPlayer>();

        private bool loaded = false;

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

        #endregion

        private void InitData()
        {
            _players =
                Interface.Oxide.DataFileSystem.ReadObject<List<DataPlayer>>("NoSteamHelper/Players");
        }

        private static void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("NoSteamHelper/Players", _players);
        }

        protected override void LoadDefaultConfig()
        {
            var config = new ConfigData
            {
                other = new ConfigData.Other()
                {
                    FakeOnline = false
                },
                players = new ConfigData.Players()
                { 
                    BlockSmurf = true,
                    BlockVpn = true
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
                _players.Add(new DataPlayer(id, steam, lastip));
                SaveData();
            }

            public void ChangeSteam(bool steam)
            {
                Steam = steam;
                SaveData();
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

        private bool IsVpnConnection(Connection connection)
        {
            bool status = false;

            webrequest.EnqueueGet($"http://proxycheck.io/v2/{connection.ipaddress}?key={configData.blockVpn.Key}&vpn=1", (code, response) =>
            {
                status = CheckResult(code, response);

            }, this);

            return status;
        }

        private bool CheckResult(int code, string response)
        {
            if (response.Contains("yes"))
                return true;

            return false;
        }

        #endregion

        #region Hooks

        private void OnServerInitialized(bool loaded)
        {
            if (!loaded)
                this.loaded = loaded;

            LoadConfigVariables();
        }

        private void Init()
        {
            InitData();
        }

        private object OnGameTags(string tags, int countPlayers)
        {
            if (!configData.other.FakeOnline)
                return null;

            var online = BasePlayer.activePlayerList.Count;

            tags = tags.Replace($"cp{countPlayers}", "cp" + online.ToString());

            return tags;
        }

        private object CanNewConnection(Connection connection, bool isSteam)
        {
            if (loaded)
            {
                Puts("Need a restart server");
                return null;
            }

            DataPlayer dataPlayer;

            if(!CheckIsValidPlayer(connection))
                return "Steam Auth Failed.";

            if(configData.players.BlockVpn && IsVpnConnection(connection))
                return "VPN Detected";

            var result = DataPlayer.FindPlayer(connection.userid, out dataPlayer);
            if (result)
            {
                if (dataPlayer.IsSteam() && !isSteam)
                    return "Dont try get access to another player";

                if (!dataPlayer.IsSteam() && isSteam)
                    dataPlayer.ChangeSteam(isSteam);


            }else
                DataPlayer.AddPlayer(connection.userid, isSteam, connection.ipaddress);

            bool isNoSteam = !isSteam;

            if (configData.players.BlockVpn)
            {
                ulong steamid = 0UL;
                if (isNoSteam && IsSmurf(connection, ref steamid))
                    return "Your primary account: " + steamid;
            }

            return null;
        }

        private bool IsSmurf(Connection connection, ref ulong userid)
        {
            foreach(var player in _players)
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

        private bool CheckIsValidPlayer(Connection connection)
        {
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player.Connection.ipaddress == connection.ipaddress)
                {
                    return false;
                }
            }

            return true;

        }

        #endregion
    }
}