using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Oxide.Plugins
{
    [Info("AuthMe", "Kaidoz", "1.6.1")]
    // UI developed by Skuli Dropek
    [Description("Authorization for NoSteam(cracked) players. ")]
    public class AuthMe : RustPlugin
    {

        [PluginReference] Plugin ImageLibrary;
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

            public DataPlayer()
            {

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
        }

        public static List<DataPlayer> _players = new List<DataPlayer>();

        [JsonProperty("Information of players")]
        private static Dictionary<ulong, DataAuthorize> _dataAuthorizes = new Dictionary<ulong, DataAuthorize>();

        [JsonProperty("Layer UI")] private static readonly string Layer = "UI.Login";

        private readonly Dictionary<string, string> _cacheHexColors = new Dictionary<string, string>();

        [PluginReference("NoSteamHelper")] private Plugin NoSteamHelper;

        [ConsoleCommand("auth")]
        private void CmdAuthConsole(ConsoleSystem.Arg arg)
        {
            BasePlayer player;
            if (ParsePlayer(arg, out player) == false)
                return;

            if (IsSteamPlayer(player))
                return;

            if (_dataAuthorizes.ContainsKey(player.userID) == false)
                PlayerInit(player);

            if (!arg.HasArgs() || arg.FullString.Length == 0)
            {
                ErrorRegistrationPanel(player, GetMessageLanguage("Registration.EmptyPassword", player.UserIDString));
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
                ErrorRegistrationPanel(player, GetMessageLanguage("Authorized.BadPassword", player.UserIDString));
                return;
            }

            dataAuthorize.IsAuthed = true;
            dataAuthorize.AuthPlayer();
            SaveData();
        }

        private void ForceAuthorization(BasePlayer player)
        {
            if (player == null)
                return;

            var buffer = _dataAuthorizes[player.userID];

            if (buffer.IsAuthed)
                return;

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

                ["Help.Registration"] = "Hello! To sign up!\n" +
                                        "You will not need to enter password from IP: {0}\n" +
                                        "Command: auth <your password>",
                ["Help.Authorization"] = "Hello! To log in!\n" +
                                         "You will not need to enter password from IP: {0}\n",
            }, this);

            #endregion Lang-En

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

                ["Authorized.BadPassword"] = "Вы ввели Authorized.BadPassword, попробуйте ещё раз!",
                ["Registration.Need"] = "Создать пароль",
                ["Authorized.Need"] = "Авторизироваться",
                ["Help.Info"] = "Вся  находится в консоле",

                ["Help.Registration"] =
                    "Приветствую! Чтобы зарегистрироваться - придумай и впиши пароль\n" +
                    "Вам больше не придётся вводить пароль c IP: {0}\n",
                ["Help.Authorization"] = "Приветствую! Вы зарегистрированы - впиши свой пароль!\n" +
                                         "Вам больше не придётся вводить пароль c IP: {0}\n"
            }, this, "ru");

            #endregion Lang-RU
        }

        #endregion Messages

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
            SaveData();

            ForceAuthorization(player);
            BleedingPanel(player);
            //DrawInterface(player);
        }
        private void BleedingPanel(BasePlayer player)
        {

            var title = string.IsNullOrEmpty(_dataAuthorizes[player.userID].Password)
              ? GetMessageLanguage("Registration.Need", player.UserIDString)
              : GetMessageLanguage("Authorized.Need", player.UserIDString);

            var subtitle = string.IsNullOrEmpty(_dataAuthorizes[player.userID].Password)
              ? GetMessageLanguage("Help.Registration", player.UserIDString)
              : GetMessageLanguage("Help.Authorization", player.UserIDString);

            subtitle = subtitle.Replace("{0}", ParseIp(player.net.connection.ipaddress));

            var container = new CuiElementContainer();
            container.Add(new CuiPanel
            {
                CursorEnabled = false,
                Image = { Color = "0 0 0 0.8" },
                RectTransform = { AnchorMin = "0 0", AnchorMax = "1 1", OffsetMin = "0 0", OffsetMax = "0 0" }
            }, "Overlay", Layer);

            container.Add(new CuiLabel
            {
                RectTransform = { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-526.75 -37.132", OffsetMax = "526.75 155.132" },
                Text = { Text = title, Font = "robotocondensed-bold.ttf", FontSize = 100, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" }
            }, Layer, "BleedingOutText");

            container.Add(new CuiElement
            {
                Name = "InputField_5268Image",
                Parent = Layer,
                Components = {
                    new CuiRawImageComponent { Png = GetImage("InputField") },
                    new CuiRectTransformComponent { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-289.383 -134.914", OffsetMax = "274.778 -74.934" }
                }
            });

            string pass = "";

            container.Add(new CuiElement
            {
                Name = "InputField_5268",
                Parent = Layer,
                Components = {
                    new CuiInputFieldComponent { Text = pass, Color = "1 1 1 1", Command = $"auth {pass}", Font = "robotocondensed-bold.ttf", FontSize = 25, Align = TextAnchor.MiddleCenter, CharsLimit = 17, IsPassword = false },
                    new CuiRectTransformComponent { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-289.383 -134.914", OffsetMax = "274.778 -74.934" }
                }
            });

            container.Add(new CuiElement
            {
                Name = "Image_2396",
                Parent = Layer,
                Components = {
                    new CuiRawImageComponent { Png = GetImage("Button") },
                    new CuiRectTransformComponent { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "211.222 -124", OffsetMax = "274.778 -84" }
                }
            });

            container.Add(new CuiLabel
            {
                RectTransform = { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-406.964 -107.75", OffsetMax = "431.676 15.75" },
                Text = { Text = subtitle, Font = "robotocondensed-bold.ttf", FontSize = 20, Align = TextAnchor.MiddleCenter, Color = "0.7490196 0.7176471 0.7137255 1" },
            }, Layer, "Label_2648");

            CuiHelper.DestroyUi(player, Layer);
            CuiHelper.AddUi(player, container);
        }

        private void ErrorRegistrationPanel(BasePlayer player, string message)
        {
            var container = new CuiElementContainer();
            container.Add(new CuiPanel
            {
                CursorEnabled = false,
                Image = { Color = "0 0 0 0.99" },
                RectTransform = { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-155.754 -76.151", OffsetMax = "144.246 73.849" }
            }, "Overlay", "ErrorRegistrationPanel");

            container.Add(new CuiLabel
            {
                Text = { Text = message, Font = "robotocondensed-regular.ttf", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "1 1 1 1" },
                RectTransform = { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-100.945 -17.706", OffsetMax = "108.525 26.522" },
            }, "ErrorRegistrationPanel", "Label_6853");

            container.Add(new CuiButton
            {
                Button = { Color = "1 1 1 0.9", Close = "ErrorRegistrationPanel" },
                Text = { Text = "OK", Font = "robotocondensed-regular.ttf", FontSize = 14, Align = TextAnchor.MiddleCenter, Color = "0 0 0 1" },
                RectTransform = { AnchorMin = "0.5 0.5", AnchorMax = "0.5 0.5", OffsetMin = "-50.945 -66.009", OffsetMax = "49.055 -34.409" }
            }, "ErrorRegistrationPanel", "Button_2446");

            CuiHelper.DestroyUi(player, "ErrorRegistrationPanel");
            CuiHelper.AddUi(player, container);
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

            #endregion CuiElements

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

        private void OnServerInitialized()
        {

            LoadPlugins();
            LoadData();
            LoadMessages();
            InitPlayers();

            AddImage("https://i.imgur.com/WRHScfF_d.png", "InputField");
            AddImage("https://i.imgur.com/SvHJNE1.png", "Button");
        }

        private void OnServerSave()
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
            if (!buffer.IsAuthed && !string.Equals(arg.cmd.FullName, "global.auth", StringComparison.OrdinalIgnoreCase))
                return false;

            return null;
        }

        private void OnPlayerSpawn(BasePlayer player)
        {
            timer.Once(1, () => PlayerInit(player));
        }


        #endregion Hooks

        #region Data

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("AuthMe/Users", _dataAuthorizes);
        }

        private void LoadData()
        {
            //_dataAuthorizes.
            _dataAuthorizes = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, DataAuthorize>>("AuthMe/Users");
        }

        #endregion Data

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

        private bool IsSteamPlayer(BasePlayer player, bool repeated = false)
        {
            DataPlayer dataPlayer;
            if (DataPlayer.FindPlayer(player.userID, out dataPlayer))
            {
                return dataPlayer.Steam;
            }
            else if (repeated == false)
            {
                _players = Interface.Oxide.DataFileSystem.ReadObject<List<DataPlayer>>("NoSteamHelper/Players");

                return IsSteamPlayer(player, true);
            }

            return false;
        }

        private string GetImage(string fileName, ulong skin = 0)
        {
            var imageId = (string)plugins.Find("ImageLibrary").CallHook("GetImage", fileName, skin);
            if (!string.IsNullOrEmpty(imageId))
                return imageId;
            return string.Empty;
        }
        public bool AddImage(string url, string shortname, ulong skin = 0) => (bool)ImageLibrary?.Call("AddImage", url, shortname, skin);

        #endregion Helper
    }
}