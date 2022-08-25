#define DEBUG
using Newtonsoft.Json;
using Oxide.Core.Libraries;
using Oxide.Core.Libraries.Covalence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


//SteamChecks created with PluginMerge v(1.0.4.0) by MJSU @ https://github.com/dassjosh/Plugin.Merge
namespace Oxide.Plugins
{
    [Info("Steam Checks", "Shady14u", "5.0.8")]
    [Description("Kick players depending on information on their Steam profile")]
    public partial class SteamChecksMod : CovalencePlugin
    {
        #region SteamChecks.cs
        #region Methods (Private)

        /// <summary>
        ///     Checks a steamId, if it would be allowed into the server
        /// </summary>
        /// <param name="steamId">steamId64 of the user</param>
        /// <param name="callback">
        ///     First parameter is true, when the user is allowed, otherwise false
        ///     Second parameter is the reason why he is not allowed, filled out when first is false
        /// </param>
        /// <remarks>
        ///     Asynchronously
        ///     Runs through all checks one-by-one
        ///     1. Bans
        ///     2. Player Summaries (Private profile, Creation time)
        ///     3. Player Level
        ///     Via <see cref="CheckPlayerGameTime"></see>
        ///     4. Game Hours and Count
        ///     5. Game badges, to get amount of games if user has hidden Game Hours
        /// </remarks>
        private void CheckPlayer(string steamId, Action<bool, string> callback)
        {
            // Check Bans first, as they are also visible on private profiles
            GetPlayerBans(steamId, (banStatusCode, banResponse) =>
            {
                if (banStatusCode != (int)StatusCode.Success)
                {
                    ApiError(steamId, "GetPlayerBans", banStatusCode);
                    return;
                }

                if (banResponse.CommunityBan && kickCommunityBan)
                {
                    callback(false, Lang("KickCommunityBan", steamId));
                    return;
                }

                if (banResponse.EconomyBan && kickTradeBan)
                {
                    callback(false, Lang("KickTradeBan", steamId));
                    return;
                }

                if (banResponse.GameBanCount > maxGameBans && maxGameBans > -1)
                {
                    callback(false, Lang("KickGameBan", steamId));
                    return;
                }

                if (banResponse.VacBanCount > maxVACBans && maxVACBans > -1)
                {
                    callback(false, Lang("KickVacBan", steamId));
                    return;
                }

                if (banResponse.LastBan > 0 && banResponse.LastBan < minDaysSinceLastBan && minDaysSinceLastBan > 0)
                {
                    callback(false, Lang("KickVacBan", steamId));
                    return;
                }

                //get Player summaries - we have to check if the profile is public
                GetSteamPlayerSummaries(steamId, (statusCode, sumResult) =>
                {
                    if (statusCode != (int)StatusCode.Success)
                    {
                        ApiError(steamId, "GetSteamPlayerSummaries", statusCode);
                        return;
                    }

                    if (sumResult.LimitedAccount && kickLimitedAccount)
                    {
                        callback(false, Lang("KickLimitedAccount", steamId));
                        return;
                    }


                    if (sumResult.NoProfile && kickNoProfile)
                    {
                        callback(false, Lang("KickNoProfile", steamId));
                        return;
                    }

                    // Is profile not public?
                    if (sumResult.Visibility != PlayerSummary.VisibilityType.Public)
                    {
                        if (kickPrivateProfile)
                        {
                            callback(false, Lang("KickPrivateProfile", steamId));
                            return;
                        }
                        else
                        {
                            // If it is not public, we can cancel checks here and allow the player in
                            callback(true, null);
                            return;
                        }
                    }

                    var dateCreated = DateTimeOffset.FromUnixTimeMilliseconds(sumResult.Timecreated).UtcDateTime;

                    if (maxAccountCreationTime > 0 && DateTime.UtcNow.Subtract(dateCreated).TotalDays < maxAccountCreationTime)
                    {
                        callback(false, Lang("KickMaxAccountCreationTime", steamId));
                        return;
                    }

                    // Check Steam Level
                    if (minSteamLevel > 0)
                    {
                        GetSteamLevel(steamId, (steamLevelStatusCode, steamLevelResult) =>
                        {
                            if (steamLevelStatusCode != (int)StatusCode.Success)
                            {
                                ApiError(steamId, "GetSteamLevel", statusCode);
                                return;
                            }

                            if (minSteamLevel > steamLevelResult)
                            {
                                callback(false, Lang("KickMinSteamLevel", steamId));
                            }
                            else
                            {
                                // Check game time, and amount of games
                                if (minGameCount > 1 || minRustHoursPlayed > 0 || maxRustHoursPlayed > 0 ||
                                minOtherGamesPlayed > 0 || minAllGamesHoursPlayed > 0)
                                    CheckPlayerGameTime(steamId, callback);
                                else // Player now already passed all checks
                                    callback(true, null);
                            }
                        });
                    }
                    // Else, if level check not done, Check game time, and amount of games
                    else if (minGameCount > 1 || minRustHoursPlayed > 0 || maxRustHoursPlayed > 0 ||
                    minOtherGamesPlayed > 0 || minAllGamesHoursPlayed > 0)
                    {
                        CheckPlayerGameTime(steamId, callback);
                    }
                    else // Player now already passed all checks
                    {
                        callback(true, null);
                    }
                });
            });
        }

        /// <summary>
        ///     Checks a steamId, and if it would be allowed into the server
        ///     Called by <see cref="CheckPlayer"></see>
        /// </summary>
        /// <param name="steamId">steamId64 of the user</param>
        /// <param name="callback">
        ///     First parameter is true, when the user is allowed, otherwise false
        ///     Second parameter is the reason why he is not allowed, filled out when first is false
        /// </param>
        /// <remarks>
        ///     Regards those specific parts:
        ///     - Game Hours and Count
        ///     - Game badges, to get amount of games if user has hidden Game Hours
        /// </remarks>
        void CheckPlayerGameTime(string steamId, Action<bool, string> callback)
        {
            GetPlaytimeInformation(steamId, (gameTimeStatusCode, gameTimeResult) =>
            {
                // Players can additionally hide their play time, check
                var gameTimeHidden = false;
                if (gameTimeStatusCode == (int)StatusCode.GameInfoHidden)
                {
                    gameTimeHidden = true;
                }
                // Check if the request failed in general
                else if (gameTimeStatusCode != (int)StatusCode.Success)
                {
                    ApiError(steamId, "GetPlaytimeInformation", gameTimeStatusCode);
                    return;
                }

                // In rare cases, the SteamAPI returns all games, however with the game time set to 0. (when the user has this info hidden)
                if (gameTimeResult != null && (gameTimeResult.PlaytimeRust == 0 || gameTimeResult.PlaytimeAll == 0))
                    gameTimeHidden = true;

                // If the server owner really wants a hour check, we will kick
                if (gameTimeHidden && forceHoursPlayedKick)
                {
                    if (minRustHoursPlayed > 0 || maxRustHoursPlayed > 0 ||
                    minOtherGamesPlayed > 0 || minAllGamesHoursPlayed > 0)
                    {
                        callback(false, Lang("KickHoursPrivate", steamId));
                        return;
                    }
                }
                // Check the times and game count now, when not hidden
                else if (!gameTimeHidden && gameTimeResult != null)
                {
                    if (minRustHoursPlayed > 0 && gameTimeResult.PlaytimeRust < minRustHoursPlayed)
                    {
                        callback(false, Lang("KickMinRustHoursPlayed", steamId));
                        return;
                    }

                    if (maxRustHoursPlayed > 0 && gameTimeResult.PlaytimeRust > maxRustHoursPlayed)
                    {
                        callback(false, Lang("KickMaxRustHoursPlayed", steamId));
                        return;
                    }

                    if (minAllGamesHoursPlayed > 0 && gameTimeResult.PlaytimeAll < minAllGamesHoursPlayed)
                    {
                        callback(false, Lang("KickMinSteamHoursPlayed", steamId));
                        return;
                    }

                    if (minOtherGamesPlayed > 0 &&
                    (gameTimeResult.PlaytimeAll - gameTimeResult.PlaytimeRust) < minOtherGamesPlayed &&
                    gameTimeResult.GamesCount >
                    1) // it makes only sense to check, if there are other games in the result set
                    {
                        callback(false, Lang("KickMinNonRustPlayed", steamId));
                        return;
                    }

                    if (minGameCount > 1 && gameTimeResult.GamesCount < minGameCount)
                    {
                        callback(false, Lang("KickGameCount", steamId));
                        return;
                    }
                }

                // If the server owner wants to check minimum amount of games, but the user has hidden game time
                // We will get the count over an additional API request via badges
                if (gameTimeHidden && minGameCount > 1)
                {
                    GetSteamBadges(steamId, (badgeStatusCode, badgeResult) =>
                    {
                        // Check if the request failed in general
                        if (badgeStatusCode != (int)StatusCode.Success)
                        {
                            ApiError(steamId, "GetPlaytimeInformation", gameTimeStatusCode);
                            return;
                        }

                        var gamesOwned = ParseBadgeLevel(badgeResult, Badge.GamesOwned);
                        if (gamesOwned < minGameCount)
                        {
                            callback(false, Lang("KickGameCount", steamId));
                        }
                        else
                        {
                            // Checks passed
                            callback(true, null);
                        }
                    });
                }
                else
                {
                    // Checks passed
                    callback(true, null);
                }
            });
        }

        private void DoChecks(IPlayer player)
        {
            if (string.IsNullOrEmpty(apiKey))
                return;

            if (player.HasPermission(skipPermission))
            {
                Log("{0} / {1} in whitelist (via permission {2})", player.Name, player.Id, skipPermission);
                return;
            }

            object obj = CallHook("IsSteam", ulong.Parse(player.Id));

            if(obj is bool)
            {
                bool isSteam = (obj as bool?) == true;

                if (isSteam)
                    return;
            }

            // Check temporary White/Blacklist if kicking is enabled
            if (!logInsteadofKick)
            {
                // Player already passed the checks, since the plugin is active
                if (cachePassedPlayers && passedList.Contains(player.Id))
                {
                    Log("{0} / {1} passed all checks already previously", player.Name, player.Id);
                    return;
                }

                // Player already passed the checks, since the plugin is active
                if (cacheDeniedPlayers && failedList.Contains(player.Id))
                {
                    Log("{0} / {1} failed a check already previously", player.Name, player.Id);
                    webrequest.Enqueue($"https://steamcommunity.com/profiles/{player.Id}?xml=1", string.Empty,
                    (code, result) =>
                    {
                        WebHookThumbnail thumbnail = null;
                        if (code >= 200 && code <= 204)
                            thumbnail = new WebHookThumbnail
                            {
                                Url = _steamAvatarRegex.Match(result).Value
                            };
                        SendDiscordMessage(player.Id, "The player failed a steam check previously.", thumbnail);
                    }, this);
                    player.Kick(Lang("KickGeneric", player.Id) + " " + additionalKickMessage);
                    return;
                }
            }

            CheckPlayer(player.Id, (playerAllowed, reason) =>
            {
                if (playerAllowed)
                {
                    Log("{0} / {1} passed all checks", player.Name, player.Id);
                    passedList.Add(player.Id);
                }
                else
                {
                    if (logInsteadofKick)
                    {
                        Log("{0} / {1} would have been kicked. Reason: {2}", player.Name, player.Id, reason);
                        SendDiscordMessage(player.Id,
                        $"The player would have been kicked for steam checks. Reason: {reason}", null);
                    }
                    else
                    {
                        Log("{0} / {1} kicked. Reason: {2}", player.Name, player.Id, reason);
                        failedList.Add(player.Id);
                        webrequest.Enqueue($"https://steamcommunity.com/profiles/{player.Id}?xml=1", string.Empty,
                        (code, result) =>
                        {
                            WebHookThumbnail thumbnail = null;
                            if (code >= 200 && code <= 204)
                                thumbnail = new WebHookThumbnail
                                {
                                    Url = _steamAvatarRegex.Match(result).Value
                                };
                            SendDiscordMessage(player.Id,
                            $"The player was kicked for steam checks. Reason: {reason}", thumbnail);
                        }, this);

                        player.Kick(reason + " " + additionalKickMessage);

                        if (broadcastKick)
                        {
                            foreach (var target in players.Connected)
                            {
                                target.Message(Lang("Console", player.Id), "", player.Name, reason);
                            }
                        }
                    }
                }
            });
        }

        #endregion
        #endregion

        #region SteamChecks.Config.cs
        /// <summary>
        /// Url to the Steam Web API
        /// </summary>
        private const string apiURL = "https://api.steampowered.com";
        private readonly Regex _steamAvatarRegex =
        new Regex(@"(?<=<avatarMedium>[\w\W]+)https://.+\.jpg(?=[\w\W]+<\/avatarMedium>)", RegexOptions.Compiled);
        /// <summary>
        /// Oxide permission for a whitelist
        /// </summary>
        private const string skipPermission = "steamchecks.skip";

        /// <summary>
        /// Timeout for a web request
        /// </summary>
        private const int webTimeout = 2000;

        /// <summary>
        /// This message will be appended to all Kick-messages
        /// </summary>
        private string additionalKickMessage;

        /// <summary>
        /// API Key to use for the Web API
        /// </summary>
        /// <remarks>
        /// https://steamcommunity.com/dev/apikey
        /// </remarks>
        private string apiKey;

        /// <summary>
        /// AppID of the game, where the plugin is loaded
        /// </summary>
        private uint appId;

        /// <summary>
        /// Broadcast kick via chat?
        /// </summary>
        private bool broadcastKick;

        /// <summary>
        /// Cache players, which joined and failed the checks
        /// </summary>
        private bool cacheDeniedPlayers;

        /// <summary>
        /// Cache players, which joined and successfully completed the checks
        /// </summary>
        private bool cachePassedPlayers;

        private List<string> discordRolesToMention = new List<string>();

        private string discordWebHookUrl;

        /// <summary>
        /// Set of steamIds, which failed the steam check test on joining
        /// </summary>
        /// <remarks>
        /// Resets after a plugin reload
        /// </remarks>
        private HashSet<string> failedList;

        /// <summary>
        /// Kick user, when his hours are hidden
        /// </summary>
        /// <remarks>
        /// A lot of steam users have their hours hidden
        /// </remarks>
        private bool forceHoursPlayedKick;

        private bool checkPlayersOnRespawn;

        /// <summary>
        /// Kick when the user has a Steam Community ban
        /// </summary>
        private bool kickCommunityBan;

        /// <summary>
        /// Kick when the user has not set up his steam profile yet
        /// </summary>
        private bool kickNoProfile;

        // <summary>
        /// Kick when the user has limited Account
        /// </summary>
        private bool kickLimitedAccount;

        /// <summary>
        /// Kick when the user has a private profile
        /// </summary>
        /// <remarks>
        /// Most checks depend on a public profile
        /// </remarks>
        private bool kickPrivateProfile;

        /// <summary>
        /// Kick when the user has a Steam Trade ban
        /// </summary>
        private bool kickTradeBan;

        /// <summary>
        /// Just log instead of actually kicking users?
        /// </summary>
        private bool logInsteadofKick;

        /// <summary>
        /// Unix-Time, if the account created by the user is newer/higher than it
        /// he won't be allowed
        /// </summary>
        private long maxAccountCreationTime;

        /// <summary>
        /// Maximum amount of game bans, the user is allowed to have
        /// </summary>
        private int maxGameBans;

        /// <summary>
        /// Maximum amount of rust played
        /// </summary>
        private int maxRustHoursPlayed;

        /// <summary>
        /// Maximum amount of VAC bans, the user is allowed to have
        /// </summary>
        private int maxVACBans;

        /// <summary>
        /// Minimum amount of Steam games played - including Rust
        /// </summary>
        private int minAllGamesHoursPlayed;

        /// <summary>
        /// How old the last VAC ban should minimally
        /// </summary>
        private int minDaysSinceLastBan;

        /// <summary>
        /// Minimum amount of Steam games
        /// </summary>
        private int minGameCount;

        /// <summary>
        /// Minimum amount of Steam games played - except Rust
        /// </summary>
        private int minOtherGamesPlayed;

        /// <summary>
        /// Minimum amount of rust played
        /// </summary>
        private int minRustHoursPlayed;

        /// <summary>
        /// The minimum steam level, the user must have
        /// </summary>
        private int minSteamLevel;

        /// <summary>
        /// Set of steamIds, which already passed the steam check test on joining
        /// </summary>
        /// <remarks>
        /// Resets after a plugin reload
        /// </remarks>
        private HashSet<string> passedList;

        #region Methods (Protected)

        /// <summary>
        /// Loads default configuration options
        /// </summary>
        protected override void LoadDefaultConfig()
        {
            Config["ApiKey"] = "";
            Config["BroadcastKick"] = false;
            Config["LogInsteadofKick"] = false;
            Config["AdditionalKickMessage"] = "";
            Config["CachePassedPlayers"] = true;
            Config["CacheDeniedPlayers"] = false;
            Config["CheckPlayersOnRespawn"] = false;
            Config["Kicking"] = new Dictionary<string, bool>
            {
                ["CommunityBan"] = false,
                ["TradeBan"] = false,
                ["PrivateProfile"] = true,
                ["LimitedAccount"] = false,
                ["NoProfile"] = true,
                ["FamilyShare"] = false,
                ["ForceHoursPlayedKick"] = false,
            };
            Config["Thresholds"] = new Dictionary<string, long>
            {
                ["MaxVACBans"] = 1,
                ["MinDaysSinceLastBan"] = -1,
                ["MaxGameBans"] = 1,
                ["MinSteamLevel"] = 0,
                ["MinAccountCreationTime (days)"] = 7,
                ["MinGameCount"] = 0,
                ["MinRustHoursPlayed"] = 10,
                ["MaxRustHoursPlayed"] = -1,
                ["MinOtherGamesPlayed"] = 2,
                ["MinAllGamesHoursPlayed"] = -1
            };
            Config["DiscordRolesToMention"] = new List<string>();
            Config["discordWebHookUrl"] = "";
        }

        #endregion

        #region Methods (Private)

        /// <summary>
        /// Initializes config options, for every plugin start
        /// </summary>
        private void InitializeConfig()
        {
            apiKey = Config.Get<string>("ApiKey");
            broadcastKick = Config.Get<bool>("BroadcastKick");
            logInsteadofKick = Config.Get<bool>("LogInsteadofKick");
            additionalKickMessage = Config.Get<string>("AdditionalKickMessage");
            cachePassedPlayers = Config.Get<bool>("CachePassedPlayers");
            cacheDeniedPlayers = Config.Get<bool>("CacheDeniedPlayers");

            kickCommunityBan = Config.Get<bool>("Kicking", "CommunityBan");
            kickTradeBan = Config.Get<bool>("Kicking", "TradeBan");
            kickPrivateProfile = Config.Get<bool>("Kicking", "PrivateProfile");
            kickNoProfile = Config.Get<bool>("Kicking", "NoProfile");
            kickLimitedAccount = Config.Get<bool>("Kicking", "LimitedAccount");
            forceHoursPlayedKick = Config.Get<bool>("Kicking", "ForceHoursPlayedKick");
            checkPlayersOnRespawn = Config.Get<bool>("CheckPlayersOnRespawn");
            maxVACBans = Config.Get<int>("Thresholds", "MaxVACBans");
            minDaysSinceLastBan = Config.Get<int>("Thresholds", "MinDaysSinceLastBan");
            maxGameBans = Config.Get<int>("Thresholds", "MaxGameBans");


            minSteamLevel = Config.Get<int>("Thresholds", "MinSteamLevel");

            minRustHoursPlayed = Config.Get<int>("Thresholds", "MinRustHoursPlayed") * 60;
            maxRustHoursPlayed = Config.Get<int>("Thresholds", "MaxRustHoursPlayed") * 60;
            minOtherGamesPlayed = Config.Get<int>("Thresholds", "MinOtherGamesPlayed") * 60;
            minAllGamesHoursPlayed = Config.Get<int>("Thresholds", "MinAllGamesHoursPlayed") * 60;

            minGameCount = Config.Get<int>("Thresholds", "MinGameCount");
            maxAccountCreationTime = Config.Get<long>("Thresholds", "MinAccountCreationTime (days)");

            if (!kickPrivateProfile)
            {
                if (minRustHoursPlayed > 0 || maxRustHoursPlayed > 0 || minOtherGamesPlayed > 0 ||
                minAllGamesHoursPlayed > 0)
                    LogWarning(Lang("WarningPrivateProfileHours"));

                if (minGameCount > 1)
                    LogWarning(Lang("WarningPrivateProfileGames"));

                if (maxAccountCreationTime > 0)
                    LogWarning(Lang("WarningPrivateProfileCreationTime"));

                if (minSteamLevel > 0)
                    LogWarning(Lang("WarningPrivateProfileSteamLevel"));
            }

            discordRolesToMention = Config.Get<List<string>>("DiscordRolesToMention");
            discordWebHookUrl = Config.Get<string>("discordWebHookUrl");
        }

        #endregion
        #endregion

        #region SteamChecks.Class.Discord.cs
        private class WebHookAuthor
        {
            [JsonProperty(PropertyName = "icon_url")]
            public string AuthorIconUrl;

            [JsonProperty(PropertyName = "url")]
            public string AuthorUrl;
            [JsonProperty(PropertyName = "name")]
            public string Name;
        }

        private class WebHookContentBody
        {
            [JsonProperty(PropertyName = "content")]
            public string Content;
        }

        private class WebHookEmbed
        {
            [JsonProperty(PropertyName = "author")]
            public WebHookAuthor Author;

            [JsonProperty(PropertyName = "color")]
            public int Color;

            [JsonProperty(PropertyName = "description")]
            public string Description;

            [JsonProperty(PropertyName = "fields")]
            public List<WebHookField> Fields;

            [JsonProperty(PropertyName = "footer")]
            public WebHookFooter Footer;

            [JsonProperty(PropertyName = "image")]
            public WebHookImage Image;

            [JsonProperty(PropertyName = "thumbnail")]
            public WebHookThumbnail Thumbnail;

            [JsonProperty(PropertyName = "title")]
            public string Title;

            [JsonProperty(PropertyName = "type")]
            public string Type = "rich";
        }

        private class WebHookEmbedBody
        {
            [JsonProperty(PropertyName = "embeds")]
            public WebHookEmbed[] Embeds;
        }

        private class WebHookField
        {
            [JsonProperty(PropertyName = "inline")]
            public bool Inline;

            [JsonProperty(PropertyName = "name")]
            public string Name;

            [JsonProperty(PropertyName = "value")]
            public string Value;
        }

        private class WebHookFooter
        {
            [JsonProperty(PropertyName = "icon_url")]
            public string IconUrl;

            [JsonProperty(PropertyName = "proxy_icon_url")]
            public string ProxyIconUrl;

            [JsonProperty(PropertyName = "text")]
            public string Text;
        }

        private class WebHookImage
        {
            [JsonProperty(PropertyName = "height")]
            public int? Height;

            [JsonProperty(PropertyName = "proxy_url")]
            public string ProxyUrl;

            [JsonProperty(PropertyName = "url")]
            public string Url;

            [JsonProperty(PropertyName = "width")]
            public int? Width;
        }

        private class WebHookThumbnail
        {
            [JsonProperty(PropertyName = "height")]
            public int? Height;

            [JsonProperty(PropertyName = "proxy_url")]
            public string ProxyUrl;

            [JsonProperty(PropertyName = "url")]
            public string Url;

            [JsonProperty(PropertyName = "width")]
            public int? Width;
        }
        #endregion

        #region SteamChecks.Class.GameTimeInformation.cs
        /// <summary>
        /// Struct for the GetOwnedGames API request
        /// </summary>
        private class GameTimeInformation
        {
            #region Constructors

            public GameTimeInformation(int gamesCount, int playtimeRust, int playtimeAll)
            {
                GamesCount = gamesCount;
                PlaytimeRust = playtimeRust;
                PlaytimeAll = playtimeAll;
            }

            #endregion

            #region Properties and Indexers

            /// <summary>
            /// Amount of games the user has
            /// </summary>
            public int GamesCount { get; set; }

            /// <summary>
            /// Play time across all Steam games
            /// </summary>
            public int PlaytimeAll { get; set; }

            /// <summary>
            /// Play time in rust
            /// </summary>
            public int PlaytimeRust { get; set; }

            #endregion

            #region Methods (Public)

            public override string ToString()
            {
                return
                $"Games Count: {GamesCount} - Playtime in Rust: {PlaytimeRust} - Playtime all Steam games: {PlaytimeAll}";
            }

            #endregion
        }
        #endregion

        #region SteamChecks.Class.PlayerBans.cs
        /// <summary>
        /// Struct for the GetPlayerBans/v1 Web API
        /// </summary>
        public class PlayerBans
        {
            #region Properties and Indexers

            /// <summary>
            /// if the user has a community ban
            /// </summary>
            public bool CommunityBan { get; set; }

            /// <summary>
            /// If the user is economy banned
            /// </summary>
            public bool EconomyBan { get; set; }

            /// <summary>
            /// Amount of game bans
            /// </summary>
            public int GameBanCount { get; set; }

            /// <summary>
            /// When the last ban was, in Unix time
            /// </summary>
            /// <remarks>
            /// The steam profile only shows bans in the last 7 years
            /// </remarks>
            public int LastBan { get; set; }

            /// <summary>
            /// Seems to be true, when the steam user has at least one ban
            /// </summary>
            public bool VacBan { get; set; }

            /// <summary>
            /// Amount of VAC Bans
            /// </summary>
            public int VacBanCount { get; set; }

            #endregion

            #region Methods (Public)

            public override string ToString()
            {
                return $"Community Ban: {CommunityBan} - VAC Ban: {VacBan} " +
                $"- VAC Ban Count: {VacBanCount} - Last Ban: {LastBan} " +
                $"- Game Ban Count: {GameBanCount} - Economy Ban: {EconomyBan}";
            }

            #endregion
        }
        #endregion

        #region SteamChecks.Class.PlayerSummary.cs
        /// <summary>
        /// Struct for the GetPlayerSummaries/v2 Web API request
        /// </summary>
        public class PlayerSummary
        {
            #region VisibilityType enum

            /// <summary>
            /// How visible the Steam Profile is
            /// </summary>
            public enum VisibilityType
            {
                Private = 1,
                Friend = 2,
                Public = 3
            }

            #endregion

            #region Properties and Indexers

            /// <summary>
            /// Is the account limited?
            /// </summary>
            /// <remarks>
            /// Will be fulfilled by an additional request directly to the steamprofile with ?xml=1
            /// </remarks>
            public bool LimitedAccount { get; set; }

            /// <summary>
            /// Has the user set up his profile?
            /// </summary>
            /// <remarks>
            /// Will be fulfilled by an additional request directly to the steamprofile with ?xml=1
            /// </remarks>
            public bool NoProfile { get; set; }

            /// <summary>
            /// URL to his steam profile
            /// </summary>
            public string Profileurl { get; set; }

            /// <summary>
            /// When his account was created - in Unix time
            /// </summary>
            /// <remarks>
            /// Will only be filled, if the users profile is public
            /// </remarks>
            public long Timecreated { get; set; }

            public VisibilityType Visibility { get; set; }

            #endregion

            #region Methods (Public)

            public override string ToString()
            {
                return
                $"Steam profile visibility: {Visibility} - Profile URL: {Profileurl} " +
                $"- Account created: {Timecreated} - Limited: {LimitedAccount} - NoProfile: {NoProfile}";
            }

            #endregion
        }
        #endregion

        #region SteamChecks.Class.SteamApiResponse.cs
        public class SteamApiResponse
        {
            #region Properties and Indexers

            public SteamResponse Response { get; set; }

            #endregion
        }

        public class SteamLevelApiResponse
        {
            #region Properties and Indexers

            public SteamLevelResponse Response { get; set; }

            #endregion
        }

        public class SteamBadgeApiResponse
        {
            #region Properties and Indexers

            public SteamBadgeResponse response { get; set; }

            #endregion
        }

        public class SteamBanApiResponse
        {
            #region Properties and Indexers

            public List<SteamBanPlayer> players { get; set; }

            #endregion
        }
        #endregion

        #region SteamChecks.Class.SteamBadge.cs
        public class SteamBadge
        {
            #region Properties and Indexers

            public int badgeid { get; set; }
            public int completion_time { get; set; }
            public int level { get; set; }
            public int scarcity { get; set; }
            public int xp { get; set; }

            #endregion
        }
        #endregion

        #region SteamChecks.Class.SteamGame.cs
        public class SteamGame
        {
            #region Properties and Indexers

            public int appid { get; set; }
            public int? playtime_2weeks { get; set; }
            public int playtime_forever { get; set; }
            public int playtime_linux_forever { get; set; }
            public int playtime_mac_forever { get; set; }
            public int playtime_windows_forever { get; set; }

            #endregion
        }
        #endregion

        #region SteamChecks.Class.SteamPlayer.cs
        public class SteamPlayer
        {
            #region Properties and Indexers

            public string avatar { get; set; }
            public string avatarfull { get; set; }
            public string avatarhash { get; set; }
            public string avatarmedium { get; set; }
            public int commentpermission { get; set; }
            public int communityvisibilitystate { get; set; }
            public string gameextrainfo { get; set; }
            public string gameid { get; set; }
            public string gameserverip { get; set; }
            public string gameserversteamid { get; set; }
            public int lastlogoff { get; set; }
            public string loccountrycode { get; set; }
            public string locstatecode { get; set; }
            public string personaname { get; set; }
            public int personastate { get; set; }
            public int personastateflags { get; set; }
            public string primaryclanid { get; set; }
            public int profilestate { get; set; }
            public string profileurl { get; set; }
            public int timecreated { get; set; }
            public string steamId { get; set; }

            #endregion
        }

        public class SteamBanPlayer
        {
            #region Properties and Indexers

            public string SteamId { get; set; }
            public bool CommunityBanned { get; set; }
            public bool VACBanned { get; set; }
            public int NumberOfVACBans { get; set; }
            public int DaysSinceLastBan { get; set; }
            public int NumberOfGameBans { get; set; }
            public string EconomyBan { get; set; }

            #endregion
        }
        #endregion

        #region SteamChecks.Class.SteamResponse.cs
        public class SteamResponse
        {
            public int? game_count;

            #region Properties and Indexers

            public List<SteamGame> games { get; set; }
            public List<SteamPlayer> players { get; set; }

            #endregion
        }

        public class SteamLevelResponse
        {
            public int player_level { get; set; }
        }

        public class SteamBadgeResponse
        {
            #region Properties and Indexers

            public List<SteamBadge> badges { get; set; }
            public int player_level { get; set; }
            public int player_xp { get; set; }
            public int player_xp_needed_current_level { get; set; }
            public int player_xp_needed_to_level_up { get; set; }

            #endregion
        }
        #endregion

        #region SteamChecks.Discord.cs
        private void SendDiscordMessage(string steamId, string reasonMessage, WebHookThumbnail thumbnail)
        {
            if (string.IsNullOrEmpty(discordWebHookUrl)) return;

            var mentions = "";
            if (discordRolesToMention != null)
                foreach (var roleId in discordRolesToMention)
                {
                    mentions += $"<@&{roleId}> ";
                }

            var message = Lang("DiscordMessage");

            var contentBody = new WebHookContentBody
            {
                Content = $"{mentions}{message}"
            };

            var color = 3092790;
            var player = BasePlayer.FindAwakeOrSleeping(steamId);

            var firstBody = new WebHookEmbedBody
            {
                Embeds = new[]
                {
                    new WebHookEmbed
                    {
                        Color = color,
                        Thumbnail = thumbnail,
                        Description =
                        $"Player{Environment.NewLine}[{player?.displayName??steamId}](https://steamcommunity.com/profiles/{steamId})" +
                        $"{Environment.NewLine}{Environment.NewLine}Steam ID{Environment.NewLine}{steamId}" +
                        $"{Environment.NewLine}{Environment.NewLine}{reasonMessage}"
                    }
                }
            };

            webrequest.Enqueue(discordWebHookUrl,
            JsonConvert.SerializeObject(contentBody,
            new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
            (headerCode, headerResult) =>
            {
                if (headerCode >= 200 && headerCode <= 204)
                {
                    webrequest.Enqueue(discordWebHookUrl,
                    JsonConvert.SerializeObject(firstBody,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }),
                    (firstCode, firstResult) => { }, this, RequestMethod.POST,
                    new Dictionary<string, string> { { "Content-Type", "application/json" } });
                }
            }, this, RequestMethod.POST,
            new Dictionary<string, string> { { "Content-Type", "application/json" } });
        }
        #endregion

        #region SteamChecks.Enum.Badge.cs
        /// <summary>
        /// The badges we reference.
        /// </summary>
        /// <remarks>
        /// Every badge comes with a level, and EXP gained
        /// </remarks>
        private enum Badge
        {
            /// <summary>
            /// Badge for the amount of games owned
            /// </summary>
            /// <remarks>
            /// The level in this badge is exactly to the amount of games owned
            /// E.g. 42 games == level 42 for badge 13
            /// (so not the same as shown on the steam profiles)
            /// </remarks>
            GamesOwned = 13
        }
        #endregion

        #region SteamChecks.Enum.StatusCode.cs
        /// <summary>
        /// HTTP Status Codes (positive) and
        /// custom status codes (negative)
        ///
        /// 200 is successful in all cases
        /// </summary>
        private enum StatusCode
        {
            Success = 200,
            BadRequest = 400,
            Unauthorized = 401,
            Forbidden = 403,
            NotFound = 404,
            MethodNotAllowed = 405,
            TooManyRequests = 429,
            InternalError = 500,
            Unavailable = 503,

            /// <summary>
            /// User has is games and game hours hidden
            /// </summary>
            GameInfoHidden = -100,

            /// <summary>
            /// Invalid steamId
            /// </summary>
            PlayerNotFound = -101,

            /// <summary>
            /// Can also happen, when the SteamAPI returns something unexpected
            /// </summary>
            ParsingFailed = -102
        }
        #endregion

        #region SteamChecks.Enum.SteamRequestType.cs
        /// <summary>
        /// Type of Steam request
        /// </summary>
        private enum SteamRequestType
        {
            /// <summary>
            /// Allows to request only one SteamID
            /// </summary>
            IPlayerService,

            /// <summary>
            /// Allows to request multiple SteamID
            /// But only one used
            /// </summary>
            ISteamUser
        }
        #endregion

        #region SteamChecks.Hooks.cs
        #region Methods (Private)

        /// <summary>
        /// Called by Oxide when plugin starts
        /// </summary>
        private void Init()
        {
            InitializeConfig();

            if (string.IsNullOrEmpty(apiKey))
            {
                LogError(Lang("ErrorAPIConfig"));

                // Unload on next tick
                timer.Once(1f, () => { server.Command("oxide.unload SteamChecks"); });
                return;
            }

            appId = covalence.ClientAppId;

            passedList = new HashSet<string>();
            failedList = new HashSet<string>();

            permission.RegisterPermission(skipPermission, this);
        }

        /// <summary>
        /// Called when a user connects (but before he is spawning)
        /// </summary>
        /// <param name="player"></param>
        private void OnUserConnected(IPlayer player)
        {
            DoChecks(player);
        }


        private void OnUserRespawned(IPlayer player)
        {
            if (!checkPlayersOnRespawn)
            {
                return;
            }
            DoChecks(player);
        }



        #endregion
        #endregion

        #region SteamChecks.Lang.cs
        #region Methods (Private)

        /// <summary>
        /// Abbreviation for printing Language-Strings
        /// </summary>
        /// <param name="key">Language Key</param>
        /// <param name="userId"></param>
        /// <returns></returns>
        private string Lang(string key, string userId = null) => lang.GetMessage(key, this, userId);

        #endregion

        /// <summary>
        /// Load default language messages, for every plugin start
        /// </summary>
        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                ["Console"] = "Kicking {0}... ({1})",

                ["ErrorAPIConfig"] = "The API key you supplied in the config is empty.. register one here https://steamcommunity.com/dev/apikey",
                ["WarningPrivateProfileHours"] = "**** WARNING: Private profile-kick is off. However a option to kick for minimum amount of hours is on.",
                ["WarningPrivateProfileGames"] = "**** WARNING: Private profile-kick is off. However the option to kick for minimum amount of games is on (MinGameCount).",
                ["WarningPrivateProfileCreationTime"] = "**** WARNING: Private profile-kick is off. However the option to kick for account age is on (MinAccountCreationTime).",
                ["WarningPrivateProfileSteamLevel"] = "**** WARNING: Private profile-kick is off. However the option to kick for steam level is on (MinSteamLevel).",

                ["ErrorHttp"] = "Error while contacting the SteamAPI. Error: {0}.",
                ["ErrorPrivateProfile"] = "This player has a private profile, therefore SteamChecks cannot check their hours.",

                ["KickCommunityBan"] = "You have a Steam Community ban on record.",
                ["KickVacBan"] = "You have too many VAC bans on record.",
                ["KickGameBan"] = "You have too many Game bans on record.",
                ["KickTradeBan"] = "You have a Steam Trade ban on record.",
                ["KickPrivateProfile"] = "Your Steam profile state is set to private.",
                ["KickLimitedAccount"] = "Your Steam account is limited.",
                ["KickNoProfile"] = "Set up your Steam community profile first.",
                ["KickMinSteamLevel"] = "Your Steam level is not high enough.",
                ["KickMinRustHoursPlayed"] = "You haven't played enough hours.",
                ["KickMaxRustHoursPlayed"] = "You have played too much Rust.",
                ["KickMinSteamHoursPlayed"] = "You didn't play enough Steam games (hours).",
                ["KickMinNonRustPlayed"] = "You didn't play enough Steam games besides Rust (hours).",
                ["KickHoursPrivate"] = "Your Steam profile is public, but the hours you played is hidden'.",
                ["KickGameCount"] = "You don't have enough Steam games.",
                ["KickMaxAccountCreationTime"] = "Your Steam account is too new.",

                ["KickGeneric"] = "Your Steam account fails our test.",
                ["DiscordMessage"] = "User failed Steam Checks"
            }, this);
        }
        #endregion

        #region SteamChecks.Test.cs
        private const string PluginPrefix = "[SteamChecks] ";

        #region Methods (Private)

        /// <summary>
        /// Command, which checks a steamid64 - with the same method when a user joins
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args">steamid64 to test for</param>
        [Command("steamcheck"), Permission("steamchecks.use")]
        private void SteamCheckCommand(IPlayer player, string command, string[] args)
        {
            if (args.Length != 1)
            {
                TestResult(player, "SteamCheckTests", "You have to provide a SteamID64 as first argument");
                return;
            }

            var steamId = args[0];

            CheckPlayer(steamId, (playerAllowed, reason) =>
            {
                if (playerAllowed)
                    TestResult(player, "CheckPlayer", "The player would pass the checks");
                else
                {
                    webrequest.Enqueue($"https://steamcommunity.com/profiles/{steamId}?xml=1", string.Empty,
                    (code, result) =>
                    {
                        WebHookThumbnail thumbnail = null;
                        if (code >= 200 && code <= 204)
                            thumbnail = new WebHookThumbnail
                            {
                                Url = _steamAvatarRegex.Match(result).Value
                            };
                        SendDiscordMessage(steamId, "The player would not pass the checks. Reason: " + reason, thumbnail);
                    }, this);


                    TestResult(player, "CheckPlayer", "The player would not pass the checks. Reason: " + reason);
                }
            });
        }

        /// <summary>
        /// Unit tests for all Web API functions
        /// Returns detailed results of the queries.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="command"></param>
        /// <param name="args">steamid64 to test for</param>
        [Command("steamcheck.runtests"), Permission("steamchecks.use")]
        private void SteamCheckTests(IPlayer player, string command, string[] args)
        {
            if (args.Length != 1)
            {
                TestResult(player, "SteamCheckTests", "You have to provide a SteamID64 as first argument");
                return;
            }

            var steamId = args[0];

            GetSteamLevel(steamId,
            (statusCode, response) =>
            {
                TestResult(player, "GetSteamLevel",
                $"Status {(StatusCode)statusCode} - Response {response}");
            });

            GetPlaytimeInformation(steamId,
            (statusCode, response) =>
            {
                TestResult(player, "GetPlaytimeInformation",
                $"Status {(StatusCode)statusCode} - Response {response}");
            });

            GetSteamPlayerSummaries(steamId,
            (statusCode, response) =>
            {
                TestResult(player, "GetSteamPlayerSummaries",
                $"Status {(StatusCode)statusCode} - Response {response}");
            });

            GetSteamBadges(steamId, (statusCode, response) =>
            {
                if ((StatusCode)statusCode == StatusCode.Success)
                {
                    var badgeLevel = ParseBadgeLevel(response, Badge.GamesOwned);
                    TestResult(player, "GetSteamBadges - Badge 13, Games owned",
                    $"Status {(StatusCode)statusCode} - Response {badgeLevel}");
                }
                else
                {
                    TestResult(player, "GetSteamBadges",
                    $"Status {(StatusCode)statusCode}");
                }
            });

            GetPlayerBans(steamId,
            (statusCode, response) =>
            {
                TestResult(player, "GetPlayerBans",
                $"Status {(StatusCode)statusCode} - Response {response}");
            });
        }

        private void TestResult(IPlayer player, string function, string result)
        {
            player.Reply(PluginPrefix + $"{function} - {result}");
        }

        #endregion
        #endregion

        #region SteamChecks.WebApi.cs
        /// <summary>
        /// Generic request to the Steam Web API
        /// </summary>
        /// <param name="steamRequestType"></param>
        /// <param name="endpoint">The specific endpoint, e.g. GetSteamLevel/v1</param>
        /// <param name="steamId64"></param>
        /// <param name="callback">Callback returning the HTTP status code <see cref="StatusCode"></see> and a JSON JObject</param>
        /// <param name="additionalArguments">Additional arguments, e.g. &foo=bar</param>
        private void SteamWebRequest(SteamRequestType steamRequestType, string endpoint, string steamId64,
        Action<int, string> callback, string additionalArguments = "")
        {
            var requestUrl = $"{apiURL}/{steamRequestType}/{endpoint}/?key={apiKey}&{(steamRequestType == SteamRequestType.IPlayerService ? "steamid" : "steamids")}={steamId64}{additionalArguments}";

            webrequest.Enqueue(requestUrl, "", (httpCode, response) =>
            {
                callback(httpCode, httpCode == (int)StatusCode.Success ? response : null);
            }, this, Core.Libraries.RequestMethod.GET, null, webTimeout);
        }

        /// <summary>
        /// Get the Steam level of a user
        /// </summary>
        /// <param name="steamId64">The users steamId64</param>
        /// <param name="callback">Callback with the statusCode <see cref="StatusCode"></see> and the steam level</param>
        private void GetSteamLevel(string steamId64, Action<int, int> callback)
        {
            SteamWebRequest(SteamRequestType.IPlayerService, "GetSteamLevel/v1", steamId64,
            (httpCode, response) =>
            {
                if (httpCode == (int)StatusCode.Success)
                {
                    callback(httpCode, JsonConvert.DeserializeObject<SteamLevelApiResponse>(response).Response.player_level);
                }
                else
                {
                    callback(httpCode, -1);
                }
            });
        }

        /// <summary>
        /// Get information about hours played in Steam
        /// </summary>
        /// <param name="steamId64">steamId64 of the user</param>
        /// <param name="callback">Callback with the statusCode <see cref="StatusCode"></see> and the <see cref="GameTimeInformation"></see></param>
        /// <remarks>
        /// Even when the user has his profile public, this can be hidden. This seems to be often the case.
        /// When hidden, the statusCode will be <see cref="StatusCode.GameInfoHidden"></see>
        /// </remarks>
        private void GetPlaytimeInformation(string steamId64, Action<int, GameTimeInformation> callback)
        {
            SteamWebRequest(SteamRequestType.IPlayerService, "GetOwnedGames/v1", steamId64,
            (httpCode, response) =>
            {
                var steamResponse = JsonConvert.DeserializeObject<SteamApiResponse>(response);
                if (httpCode == (int)StatusCode.Success)
                {
                    // We need to check if it is null, because the steam-user can hide game information
                    var gamesCount = steamResponse.Response.game_count;
                    if (gamesCount == null)
                    {
                        callback((int)StatusCode.GameInfoHidden, null);
                        return;
                    }

                    var playtimeRust = steamResponse.Response.games
                    .FirstOrDefault(x => x.appid == 480)?.playtime_forever;

                    if (playtimeRust == null)
                    {
                        callback((int)StatusCode.GameInfoHidden, null);
                        return;
                    }

                    var playtimeAll = steamResponse.Response.games
                    .Sum(x => x.playtime_forever);
                    callback(httpCode, new GameTimeInformation((int)gamesCount, (int)playtimeRust, playtimeAll));
                }
                else
                {
                    callback(httpCode, null);
                }
            }, "&include_appinfo=false"); // We don't need additional appinfos, like images
        }


        /// <summary>
        /// Get Summary information about the player, like if his profile is visible
        /// </summary>
        /// <param name="steamId64">steamId64 of the user</param>
        /// <param name="callback">Callback with the statusCode <see cref="StatusCode"></see> and the <see cref="PlayerSummary"></see></param>
        private void GetSteamPlayerSummaries(string steamId64, Action<int, PlayerSummary> callback)
        {
            SteamWebRequest(SteamRequestType.ISteamUser, "GetPlayerSummaries/v2", steamId64,
            (httpCode, response) =>
            {
                var steamResponse = JsonConvert.DeserializeObject<SteamApiResponse>(response);
                if (httpCode == (int)StatusCode.Success)
                {
                    if (steamResponse.Response.players.Count() != 1)
                    {
                        callback((int)StatusCode.PlayerNotFound, null);
                        return;
                    }

                    var playerInfo = steamResponse.Response.players[0];

                    var summary = new PlayerSummary
                    {
                        Visibility = (PlayerSummary.VisibilityType)playerInfo.communityvisibilitystate,
                        Profileurl = playerInfo.profileurl
                    };

                    // Account creation time can be only fetched, when the profile is public
                    if (summary.Visibility == PlayerSummary.VisibilityType.Public)
                        summary.Timecreated = playerInfo.timecreated;
                    else
                        summary.Timecreated = -1;

                    // We have to do a separate request to the steam community profile to get infos about limited and
                    // if they set up their profile
                    {
                        // Set defaults, which won't get the user kicked
                        summary.NoProfile = false;
                        summary.LimitedAccount = false;

                        webrequest.Enqueue($"https://steamcommunity.com/profiles/{steamId64}/?xml=1", "",
                        (httpCodeCommunity, responseCommunity) =>
                        {
                            if (httpCodeCommunity == (int)StatusCode.Success)
                            {
                                // XML parser is disabled in uMod, so have to use contains

                                // Has not set up their profile?
                                if (responseCommunity.Contains(
                                "This user has not yet set up their Steam Community profile."))
                                    summary.NoProfile = true;

                                if (responseCommunity.Contains("<isLimitedAccount>1</isLimitedAccount>"))
                                    summary.LimitedAccount = true;

                                callback(httpCode, summary);
                            }
                            else
                            {
                                ApiError(steamId64, "GetSteamPlayerSummaries - community xml",
                                httpCodeCommunity);
                                // We will send into the callback success, as the normal GetSteamPlayerSummaries worked in this case
                                // So it's information can be respected
                                callback((int)StatusCode.Success, summary);
                            }
                        }, this, Core.Libraries.RequestMethod.GET, null, webTimeout);
                    }
                }
                else
                {
                    callback(httpCode, null);
                }
            });
        }

        /// <summary>
        /// Utility function for printing a log when a HTTP API Error was encountered
        /// </summary>
        /// <param name="steamId">steamId64 for which user the request was</param>
        /// <param name="function">function name in the plugin</param>
        /// <param name="statusCode">see <see cref="StatusCode"></see></param>
        private void ApiError(string steamId, string function, int statusCode)
        {
            var detailedError = $" SteamID: {steamId} - Function: {function} - ErrorCode: {(StatusCode)statusCode}";
            LogWarning(Lang("ErrorHttp"), detailedError);
        }

        /// <summary>
        /// Get all Steam Badges
        /// </summary>
        /// <param name="steamId64">steamId64 of the user</param>
        /// <param name="callback">Callback with the statusCode <see cref="StatusCode"></see> and the result as JSON</param>
        private void GetSteamBadges(string steamId64, Action<int, string> callback)
        {
            SteamWebRequest(SteamRequestType.IPlayerService, "GetBadges/v1", steamId64, callback);
        }

        /// <summary>
        /// Fetched the level of a given badgeid from a JSON Web API result
        /// </summary>
        /// <param name="response"></param>
        /// <param name="badgeId">ID of the badge, see <see cref="Badge"></see></param>
        /// <returns>level of the badge, or 0 if badge not existing</returns>
        private int ParseBadgeLevel(string response, Badge badgeId)
        {
            var steamResponse = JsonConvert.DeserializeObject<SteamBadgeApiResponse>(response);
            return steamResponse.response.badges.FirstOrDefault(x => x.badgeid == (int)badgeId)?.level ?? 0;
        }

        /// <summary>
        /// Get the information about the bans the player has
        /// </summary>
        /// <param name="steamId64">steamId64 of the user</param>
        /// <param name="callback">Callback with the statusCode <see cref="StatusCode"></see> and the result as <see cref="PlayerBans"></see></param>
        /// <remarks>
        /// Getting the user bans is even possible, if the profile is private
        /// </remarks>
        private void GetPlayerBans(string steamId64, Action<int, PlayerBans> callback)
        {
            SteamWebRequest(SteamRequestType.ISteamUser, "GetPlayerBans/v1", steamId64,
            (httpCode, response) =>
            {
                if (httpCode == (int)StatusCode.Success)
                {
                    var steamResponse = JsonConvert.DeserializeObject<SteamBanApiResponse>(response);
                    if (steamResponse.players.Count() != 1)
                    {
                        callback((int)StatusCode.PlayerNotFound, null);
                        return;
                    }

                    var playerInfo = steamResponse.players[0];

                    var bans = new PlayerBans
                    {
                        CommunityBan = playerInfo.CommunityBanned,
                        VacBan = playerInfo.VACBanned,
                        VacBanCount = playerInfo.NumberOfVACBans,
                        LastBan = playerInfo.DaysSinceLastBan,
                        GameBanCount = playerInfo.NumberOfGameBans,
                        // can be none, probation or banned
                        EconomyBan = playerInfo.EconomyBan != "none"
                    };

                    callback(httpCode, bans);
                }
                else
                {
                    callback(httpCode, null);
                }
            });
        }
        #endregion

    }

}
