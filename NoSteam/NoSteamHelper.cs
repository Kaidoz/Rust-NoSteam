// Author:  Kaidoz
// Filename: NoSteamHelper.cs
// Last update: 2019.10.07 19:20

using System.Collections.Generic;
using Network;
using Oxide.Core;

namespace Oxide.Plugins
{
    [Info("NoSteamHelper", "Kaidoz", "1.0.3")]
    [Description("")]
    internal class NoSteamHelper : RustPlugin
    {
        private static List<DataPlayer> _players = new List<DataPlayer>();

        private bool _loaded = false;

        #region API

        private object IsPlayerNoSteam(ulong steamid)
        {
            DataPlayer dataPlayer;
            var result = DataPlayer.FindPlayer(steamid, out dataPlayer);

            if (result == false)
            {
                Puts("Player no found");
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

        public class DataPlayer
        {
            public bool Steam;
            public ulong SteamId;

            public DataPlayer(ulong id, bool steam)
            {
                SteamId = id;
                Steam = steam;
            }

            public static void AddPlayer(ulong id, bool steam)
            {
                _players.Add(new DataPlayer(id, steam));
                SaveData();
            }

            public void ChangeSteam(bool steam)
            {
                this.Steam = steam;
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

        #region Hooks

        private void OnServerInitialized(bool loaded)
        {
            if (!loaded)
                _loaded = loaded;
            InitData();
        }

        private object CanNewConnection(Connection connection, bool isSteam)
        {
            if (_loaded)
            {
                Puts("Need a restart server");
                return null;
            }

            DataPlayer dataPlayer;

            var result = DataPlayer.FindPlayer(connection.userid, out dataPlayer);
            if (result)
            {
                if (dataPlayer.IsSteam() && !isSteam)
                    return "Don't try get access to another player";

                if (!dataPlayer.IsSteam() && isSteam)
                    dataPlayer.ChangeSteam(isSteam);

                return null;
            }

            DataPlayer.AddPlayer(connection.userid, isSteam);
            return null;
        }

        #endregion
    }
}