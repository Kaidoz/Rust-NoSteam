// Author:  Kaidoz
// Filename: NoSteamHelper.cs
// Last update: 2019.10.07 19:20

using System;
using System.Collections.Generic;
using ConVar;
using Network;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using Oxide.Core.Plugins;
using Oxide.Core.RemoteConsole;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("NoSteamHelper", "Kaidoz", "1.1.0")]
    [Description("")]
    internal class NoSteamHelper : RustPlugin
    {
        #region Variables

        private readonly Dictionary<string, string> DiscordHeaders = new Dictionary<string, string>
        {
                { "Content-Type", "application/json" }
        };

        public static ConfigData configData;

        public static NoSteamHelper Instance;

        #endregion Variables

        #region Class

        public class ConfigData
        {
            [JsonProperty("Other")]
            public Other other;

            [JsonProperty("Players")]
            public Players players;

            [JsonProperty("Block VPN Config")]
            public BlockVPN blockVpn;

            public class Other
            {
                [JsonProperty("Enable visibility nosteam players in servers list(DANGER! Risk get ban)")]
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
                other = new ConfigData.Other()
                {
                    FakeOnline = false
                },
                players = new ConfigData.Players()
                {
                    BlockSmurf = true,
                    BlockVpn = false
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

        /*private void DeleteConfig()
        {
            Config.Clear
        }*/

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
                if (SteamId == 76561198874259939)
                    return false;

                return Steam;
            }
        }

        #region Discord

        #region Class

        private class ContentType
        {
            public string content;
            public string username;
            public string avatar_url;

            public ContentType(string text, string name = null, string avatar = null)
            {
                content = text;
                username = name;
                avatar_url = avatar;
            }
        }

        #endregion Class


        private void SendMsgDiscord(ContentType contentType)
        {
            webrequest.Enqueue("discord web hook", JsonConvert.SerializeObject(contentType), (code, response) => { }, this, RequestMethod.POST, DiscordHeaders);
        }

        private void SendMessage(string message)
        {
            //Server.Command("");
        }

        #endregion Discord

        #region VPN

        private void IsVpnConnection(BasePlayer player, Connection connection)
        {
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

        private void OnServerInitialized(bool loaded)
        {
            Instance = this;
            LoadConfigVariables();
            SaveConfig();
        }

        private void Init()
        {
            InitData();
        }

        private object IsShowCracked()
        {
            if (!configData.other.FakeOnline)
                return null;

            return true;
        }

        private object OnGameTags(string tags, string online)
        {
            return null;
        }

        private void OnPlayerConnected(BasePlayer player)
        {
            if (configData.players.BlockVpn)
            {
                IsVpnConnection(player, player.Connection);
            }
        }

        private object CanNewConnection(Connection connection, bool isSteam)
        {
            DataPlayer dataPlayer;

            if (!CheckIsValidPlayer(connection))
                return "Steam Auth Failed.";

            var result = DataPlayer.FindPlayer(connection.userid, out dataPlayer);
            if (result)
            {
                if (dataPlayer.IsSteam() && !isSteam)
                    return "Dont try get access to another player";

                if (!dataPlayer.IsSteam() && isSteam)
                    dataPlayer.ChangeSteam(isSteam);
            }
            else
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

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
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

        private bool CheckIsValidPlayer(Connection connection)
        {
            string IpAddress = connection.ipaddress.Split(':')[0];
            foreach (var player in BasePlayer.activePlayerList)
            {
                if (player.Connection.ipaddress.Split(':')[0] == IpAddress)
                {
                    return true;
                }
            }

            return true;
        }

        private void SendConsoleCommand(string cmd)
        {
            Server.Command(cmd);
        }

        #endregion Hooks
    }
}