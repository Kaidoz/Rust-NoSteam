// Author:  Kaidoz
// Filename: AuthMe.cs
// Last update: 2019.10.09 20:32

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AuthMe", "Kaidoz", "1.5.6")]
    [Description("Authorization for NoSteam(cracked) players")]
    public class AuthMe : RustPlugin
    {
        private static readonly ObservableCollection<ulong> ListSteamPlayers = new ObservableCollection<ulong>();

        [JsonProperty("Информация об игроках")]
        private static Dictionary<ulong, DataAuthorize> _dataAuthorizes = new Dictionary<ulong, DataAuthorize>();

        [JsonProperty("Слой с интерфейсом")] private static readonly string Layer = "UI.Login";

        private readonly Dictionary<string, string> _cacheHexColors = new Dictionary<string, string>();

        [PluginReference("NoSteamHelper")] private Plugin NoSteamHelper;

        [ConsoleCommand("auth")]
        private void CmdAuthConsole(ConsoleSystem.Arg arg)
        {
            BasePlayer player;
            if (!ParsePlayer(arg, out player))
                return;

            if (IsSteamPlayer(player))
                return;

            if (!_dataAuthorizes.ContainsKey(player.userID))
                PlayerInit(player);

            if (!arg.HasArgs() || arg.FullString.Length == 0)
            {
                arg.ReplyWithObject(GetMessageLanguage("Registration.EmptyPassword", player.UserIDString));
                return;
            }

            var password = arg.FullString;
            var dataAuthorize = _dataAuthorizes[player.userID];
            if (dataAuthorize.IsAuthed)
                return;

            if (dataAuthorize.Player == null)
                dataAuthorize.Player = player;

            if (string.IsNullOrEmpty(dataAuthorize.Password))
            {
                arg.ReplyWithObject(GetMessageLanguage("Registration.Successful", player.UserIDString).Replace("{0}", password));
                dataAuthorize.Password = password;
            }
            else if (password == dataAuthorize.Password)
            {
                arg.ReplyWithObject(GetMessageLanguage("Authorized.Successful", player.UserIDString));
                player.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, false);
            }

            if (password != dataAuthorize.Password)
            {
                arg.ReplyWithObject(GetMessageLanguage("Authorized.BadPassword", player.UserIDString));
                return;
            }

            dataAuthorize.IsAuthed = true;
            dataAuthorize.AuthPlayer();
        }

        private void ForceAuthorization(BasePlayer player)
        {
            if (player == null)
                return;

            var buffer = _dataAuthorizes[player.userID];

            if (buffer.IsAuthed)
                return;
            try
            {
                if (string.IsNullOrEmpty(buffer.Password))
                {
                    player.ChatMessage(GetMessageLanguage("Help.NoRegistrationWarning", player.UserIDString));
                    player.SendConsoleCommand(
                        $"echo {GetMessageLanguage("Help.Registration", player.UserIDString).Replace("{0}", ParseIp(player.net.connection.ipaddress))}");

                    buffer.Timer = timer.Once(10, () => ForceAuthorization(player));
                    return;
                }

                player.SetPlayerFlag(BasePlayer.PlayerFlags.ChatMute, true);
                player.SendConsoleCommand(
                    $"echo {GetMessageLanguage("Help.Authorization", player.UserIDString).Replace("{0}", ParseIp(player.net.connection.ipaddress))}");
                player.Teleport(buffer.LastPosition);
                buffer.Timer = timer.Once(2, () => ForceAuthorization(player));
            }
            catch { }
        }

        private void InitPlayers()
        {
            foreach (var player in BasePlayer.activePlayerList)
                PlayerInit(player);
        }

        private void LoadPlugins()
        {
            if (NoSteamHelper == null)
                NoSteamHelper = plugins.Find("NoSteamHelper");
        }

        #region Messages

        private void LoadMessages()
        {
            #region Lang-En

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Authorized.Ip"] = "<size=16>You successful <color=#4286f4>authorized</color>:</size>" +
                                    "\n<size=10>You already authorized from IP: {0}</size>",

                ["Help.NoRegistrationWarning"] =
                    "<size=16>Your account is <color=#4286f4>not protected</color>!</size>" +
                    "\nCheck Console (<color=#4286f4>F1</color>) to fix trouble!",


                ["Registration.EmptyPassword"] = "Password is empty!",

                ["Registration.Successful"] = "You successful registered on server!\n" +
                                              "Your password: {0}",

                ["Authorized.Successful"] = "You successful authorized!",

                ["Authorized.BadPassword"] = "You entered wrong password!" +
                                             "Command to authorize: auth <your password>",
                ["Registration.Need"] = "SIGN UP",
                ["Authorized.Need"] = "LOG IN",
                ["Help.Info"] = "All information in console",

                ["Help.Registration"] = "Hello! To sign up, enter auth <your password> in console !\n" +
                                        "You will not need to enter password from IP: {0}\n" +
                                        "Command: auth <your password>",
                ["Help.Authorization"] = "Hello! To log in, enter auth <your password> in console!\n" +
                                         "You will not need to enter password from IP: {0}\n" +
                                         "Command: auth <your password>"
            }, this);

            #endregion

            #region Lang-RU

            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Authorized.Ip"] = "<size=16>Вы автоматически <color=#4286f4>авторизовались</color>:</size>" +
                                    "\n<size=10>Вы уже авторизовались с IP: {0}</size>",

                ["Help.NoRegistrationWarning"] =
                    "<size=16>Ваша учётная запись в <color=#4286f4>опасности</color>!</size>" +
                    "\nЗагляните в консоль (<color=#4286f4>F1</color>) для устранения проблемы!",


                ["Registration.EmptyPassword"] = "Вы не ввели пароль, либо его длина слишком мала!",

                ["Registration.Successful"] = "Вы успешно зарегистрировались на сервере!\n" +
                                              "Выбранный пароль: {0}",

                ["Authorized.Successful"] = "Вы успешно авторизовались на сервере!",

                ["Authorized.BadPassword"] = "Вы ввели Authorized.BadPassword, попробуйте ещё раз!" +
                                             "Команда авторизации: auth <ваш пароль>",
                ["Registration.Need"] = "Создать пароль",
                ["Authorized.Need"] = "Авторизироваться",
                ["Help.Info"] = "Вся  находится в консоле",

                ["Help.Registration"] =
                    "Приветствую! Чтобы зарегистрироваться - придумай и впиши пароль в консоль!\n" +
                    "Вам больше не придётся вводить пароль c IP: {0}\n" +
                    "Команда: auth <придуманный пароль>",
                ["Help.Authorization"] = "Приветствую! Вы зарегистрированы - впиши свой пароль в консоль!\n" +
                                         "Вам больше не придётся вводить пароль c IP: {0}\n" +
                                         "Команда: auth <придуманный пароль>"
            }, this, "ru");

            #endregion
        }

        #endregion

        private void PlayerInit(BasePlayer player)
        {
            if (IsSteamPlayer(player))
                return;

            var IP = ParseIp(player.net.connection.ipaddress);

            if (!_dataAuthorizes.ContainsKey(player.userID))
            {
                var dataAuthorize = new DataAuthorize
                {
                    ListAuthedIps = new List<string>(),
                    LastPosition = player.transform.position,
                    IsAuthed = false,
                    Player = player
                };
                DataAuthorize.AddPlayerToData(player.userID, dataAuthorize);
            }
            else
            {
                var dataAuthorize = _dataAuthorizes[player.userID];
                dataAuthorize.LastPosition = player.transform.position;
                dataAuthorize.IsAuthed = false;

                if (dataAuthorize.ListAuthedIps.Contains(IP))
                {
                    dataAuthorize.IsAuthed = true;
                    player.ChatMessage(GetMessageLanguage("Authorized.Ip", player.UserIDString).Replace("{0}", IP));
                    return;
                }
            }

            ForceAuthorization(player);
            DrawInterface(player);
        }

        private void DrawInterface(BasePlayer player)
        {
            CuiHelper.DestroyUi(player, Layer);
            var container = new CuiElementContainer();

            var action = string.IsNullOrEmpty(_dataAuthorizes[player.userID].Password)
                ? GetMessageLanguage("Registration.Need", player.UserIDString)
                : GetMessageLanguage("Authorized.Need", player.UserIDString);

            #region CuiElements

            container.Add(new CuiPanel
            {
                FadeOut = 2f,
                CursorEnabled = true,
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1", OffsetMax = "0 0" },
                Image = { FadeIn = 2f, Color = HexToRustFormat("#000000") }
            }, "Overlay", Layer);

            container.Add(new CuiButton
            {
                FadeOut = 2f,
                RectTransform = { AnchorMin = "0.3347656 0.4388889", AnchorMax = "0.6652344 0.5611111" },
                Text =
                    {
                        FadeIn = 2f, Text = "", FontSize = 32, Font = "robotocondensed-bold.ttf",
                        Align = TextAnchor.MiddleCenter
                    },
                Button = { FadeIn = 2f, Color = HexToRustFormat("#8484844A") }
            }, Layer, Layer + ".Hide");

            container.Add(new CuiLabel
            {
                FadeOut = 2f,
                RectTransform = { AnchorMin = "0 0.3", AnchorMax = "1 1" },
                Text =
                    {
                        FadeIn = 2f, Text = action, FontSize = 38, Font = "robotocondensed-bold.ttf",
                        Align = TextAnchor.MiddleCenter
                    }
            }, Layer + ".Hide", Layer + ".Hide1");

            container.Add(new CuiLabel
            {
                FadeOut = 2f,
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 0.5" },
                Text =
                    {
                        FadeIn = 2f, Text = GetMessageLanguage("Help.Info", player.UserIDString), FontSize = 22,
                        Font = "robotocondensed-regular.ttf",
                        Align = TextAnchor.MiddleCenter
                    }
            }, Layer + ".Hide", Layer + ".Hide2");

            #endregion

            CuiHelper.AddUi(player, container);
        }

        private class DataAuthorize
        {
            [JsonIgnore] public BasePlayer Player;
            [JsonIgnore] public bool IsAuthed;
            [JsonIgnore] public Vector3 LastPosition;

            [JsonProperty("Авторизованные IP адреса")]
            public List<string> ListAuthedIps;

            [JsonProperty("Пароль пользователя")] public string Password;

            [JsonIgnore] public Timer Timer;

            public void AuthPlayer()
            {
                if (Timer != null && !Timer.Destroyed)
                    Timer.Destroy();

                string ip = ParseIp(Player.net.connection.ipaddress);

                if (!ListAuthedIps.Contains(ip))
                    ListAuthedIps.Add(ip);
                DestroyUi();
            }

            private void DestroyUi()
            {
                CuiHelper.DestroyUi(Player, Layer + ".Hide2");
                CuiHelper.DestroyUi(Player, Layer + ".Hide1");
                CuiHelper.DestroyUi(Player, Layer + ".Hide");
                CuiHelper.DestroyUi(Player, Layer);
            }

            public static void AddPlayerToData(ulong steamid, DataAuthorize dataAuthorize)
            {
                _dataAuthorizes.Add(steamid, dataAuthorize);
            }
        }

        #region Hooks

        private void Init()
        {
            ListSteamPlayers.CollectionChanged += ListSteamPlayers_CollectionChanged;
            LoadPlugins();
            LoadData();
            LoadMessages();
            InitPlayers();
        }

        private void ListSteamPlayers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            SaveData();
        }

        private object OnServerCommand(ConsoleSystem.Arg arg)
        {
            BasePlayer player;
            if (arg.IsServerside)
                return null;

            if (!ParsePlayer(arg, out player))
                return false;

            if (IsSteamPlayer(player))
                return null;

            if (!_dataAuthorizes.ContainsKey(player.userID))
                return false;

            var buffer = _dataAuthorizes[player.userID];
            if (!buffer.IsAuthed && arg.cmd.FullName.ToLower() != "global.auth")
                return false;

            return null;
        }

        private void OnPlayerDisconnected(BasePlayer player, string reason)
        {
            if (ListSteamPlayers.Contains(player.userID)) ListSteamPlayers.Remove(player.userID);
        }

        private void OnPlayerInit(BasePlayer player)
        {
            if (player.IsReceivingSnapshot)
            {
                timer.Once(0.5f, () => OnPlayerInit(player));
                return;
            }
            PlayerInit(player);
        }

        #endregion

        #region Data

        private static void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("AuthMe/Users", _dataAuthorizes);
        }

        private void LoadData()
        {
            _dataAuthorizes = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, DataAuthorize>>("AuthMe/Users");
        }

        #endregion

        #region Helper

        private static string ParseIp(string input)
        {
            if (input.Contains(":"))
                return input.Split(':')[0];

            return input;
        }

        private string GetMessageLanguage(string key, string userId)
        {
            return lang.GetMessage(key, this, userId);
        }

        private string HexToRustFormat(string hex)
        {
            if (string.IsNullOrEmpty(hex)) hex = "#FFFFFFFF";

            if (_cacheHexColors.ContainsKey(hex))
                return _cacheHexColors[hex];

            var str = hex.Trim('#');

            if (str.Length == 6)
                str += "FF";

            var r = byte.Parse(str.Substring(0, 2), NumberStyles.HexNumber);
            var g = byte.Parse(str.Substring(2, 2), NumberStyles.HexNumber);
            var b = byte.Parse(str.Substring(4, 2), NumberStyles.HexNumber);
            var a = byte.Parse(str.Substring(6, 2), NumberStyles.HexNumber);

            Color color = new Color32(r, g, b, a);

            var textColor = string.Format("{0:F2} {1:F2} {2:F2} {3:F2}", color.r, color.g, color.b, color.a);

            _cacheHexColors.Add(hex, textColor);

            return textColor;
        }

        private bool ParsePlayer(ConsoleSystem.Arg arg, out BasePlayer basePlayer)
        {
            basePlayer = null;
            if (arg.Connection.connected == false)
                return false;

            var player = arg.Player();
            if (player == null)
                return false;
            basePlayer = player;

            return true;
        }

        private bool IsSteamPlayer(BasePlayer player)
        {
            if (NoSteamHelper == null)
                return false;

            if (ListSteamPlayers.Contains(player.userID))
            {
                return true;
            }

            if (NoSteamHelper.Call("IsPlayerNoSteam", player.userID) == null)
            {
                ListSteamPlayers.Add(player.userID);
                return true;
            }

            return false;
        }

        #endregion

        #region Credits

        // Hougan, the original author of this plugin

        #endregion
    }
}