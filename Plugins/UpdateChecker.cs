using Newtonsoft.Json;
using Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Oxide.Plugins
{
    [Info("Update Checker", "Kaidoz", "0.0.1 bugsfixed #1")]
    [Description("Описание плагина. Downloaded from Oxide-Russia")]
    internal class UpdateChecker : CovalencePlugin
    {
        #region Variables

        private enum Site
        {
            OxideRussia,
            Umod,
            Unkown
        };

        private readonly Dictionary<string, string> DiscordHeaders = new Dictionary<string, string>
        {
                { "Content-Type", "application/json" }
        };

        private ConfigData configData;

        private Dictionary<string, DataPlugin> OutdatedPlugins = new Dictionary<string, DataPlugin>();

        #endregion Variables

        #region Classes

        #region DataPlugin-Class

        public class DataPlugin
        {
            public string version;

            public string url;

            public DataPlugin(string vers, string url)
            {
                this.version = vers;
                this.url = url;
            }
        }

        #endregion DataPlugin-Class

        #region Config-Class

        public class ConfigData
        {
            [JsonProperty("Time to check updates(mins)")]
            public int timeToCheck { get; set; }

            [JsonProperty("Oxide-Russia Api-Key")]
            public string ApiKey { get; set; }

            [JsonProperty("Messages")]
            public Messages messages { get; set; }

            [JsonProperty("Enabled umods plugins for check")]
            public bool umodEnabled { get; set; }

            public class Messages
            {
                [JsonProperty("Message")]
                public string Msg { get; set; }

                [JsonProperty("Send msg all admins")]
                public bool SendMsg { get; set; }

                [JsonProperty(" Discord")]
                public Discord discord { get; set; }

                [JsonProperty("VK")]
                public Vk vk { get; set; }

                public class Discord
                {
                    [JsonProperty(" Discord")]
                    public bool discordEnabled { get; set; }

                    [JsonProperty("Discord WebHook(FAQ shorturl.at/gCFG0)")]
                    public string discordWebHook { get; set; }

                    [JsonProperty("Discord msg")]
                    public string discordMsg { get; set; }
                }

                public class Vk
                {
                    [JsonProperty(" Vk")]
                    public bool vkEnabled { get; set; }

                    [JsonProperty("Token")]
                    public string vkToken { get; set; }

                    //

                    [JsonProperty("VK msg")]
                    public string vkMsg { get; set; }

                    [JsonProperty("VK users")]
                    public List<string> vkUsers { get; set; }
                }
            }
        }

        #endregion Config-Class

        #region Api-Class

        public class Category
        {
            public bool allow_commercial_external { get; set; }
            public bool allow_external { get; set; }
            public bool allow_fileless { get; set; }
            public bool allow_local { get; set; }
            public bool can_add { get; set; }
            public bool can_upload_images { get; set; }
            public string description { get; set; }
            public int display_order { get; set; }
            public bool enable_support_url { get; set; }
            public bool enable_versioning { get; set; }
            public int last_resource_id { get; set; }
            public string last_resource_title { get; set; }
            public int last_update { get; set; }
            public int min_tags { get; set; }
            public int parent_category_id { get; set; }
            public int resource_category_id { get; set; }
            public int resource_count { get; set; }
            public string title { get; set; }
        }

        public class CustomFields
        {
            [JsonProperty("1")]
            public string One { get; set; }

            public string updates { get; set; }
            public string config { get; set; }

            [JsonProperty("2")]
            public string Two { get; set; }

            [JsonProperty("3")]
            public string Three { get; set; }
        }

        public class DescriptionAttachment
        {
            public int attach_date { get; set; }
            public int attachment_id { get; set; }
            public int content_id { get; set; }
            public string content_type { get; set; }
            public int file_size { get; set; }
            public string filename { get; set; }
            public int height { get; set; }
            public string thumbnail_url { get; set; }
            public int view_count { get; set; }
            public int width { get; set; }
        }

        public class AvatarUrls
        {
            public string o { get; set; }
            public string h { get; set; }
            public string l { get; set; }
            public string m { get; set; }
            public string s { get; set; }
        }

        public class User
        {
            public AvatarUrls avatar_urls { get; set; }
            public bool can_ban { get; set; }
            public bool can_converse { get; set; }
            public bool can_edit { get; set; }
            public bool can_follow { get; set; }
            public bool can_ignore { get; set; }
            public bool can_post_profile { get; set; }
            public bool can_view_profile { get; set; }
            public bool can_view_profile_posts { get; set; }
            public bool can_warn { get; set; }
            public int CMTV_QT_best_answer_count { get; set; }
            public bool is_staff { get; set; }
            public int last_activity { get; set; }
            public string location { get; set; }
            public int message_count { get; set; }
            public int reaction_score { get; set; }
            public int register_date { get; set; }
            public string signature { get; set; }
            public int trophy_points { get; set; }
            public int user_id { get; set; }
            public string user_title { get; set; }
            public string username { get; set; }
        }

        public class Resource
        {
            public Resource()
            {
                Category = new Category();
            }

            public string alt_support_url { get; set; }
            public bool can_download { get; set; }
            public bool can_edit { get; set; }
            public bool can_edit_icon { get; set; }
            public bool can_edit_tags { get; set; }
            public bool can_hard_delete { get; set; }
            public bool can_soft_delete { get; set; }
            public bool can_view_description_attachments { get; set; }
            public Category Category { get; set; }
            public string currency { get; set; }
            public CustomFields custom_fields { get; set; }
            public string description { get; set; }
            public int description_attach_count { get; set; }
            public IList<DescriptionAttachment> DescriptionAttachments { get; set; }
            public int download_count { get; set; }
            public string external_url { get; set; }
            public string icon_url { get; set; }
            public int last_update { get; set; }
            public string prefix { get; set; }
            public int prefix_id { get; set; }
            public string price { get; set; }
            public int rating_avg { get; set; }
            public int rating_count { get; set; }
            public double rating_weighted { get; set; }
            public int reaction_score { get; set; }
            public int resource_category_id { get; set; }
            public int resource_date { get; set; }
            public int resource_id { get; set; }
            public string resource_state { get; set; }
            public string resource_type { get; set; }
            public int review_count { get; set; }
            public string tag_line { get; set; }
            public IList<string> tags { get; set; }
            public string title { get; set; }
            public int update_count { get; set; }
            public User User { get; set; }
            public int user_id { get; set; }
            public string username { get; set; }
            public string version { get; set; }
            public int view_count { get; set; }
        }

        public class ApiResource
        {
            public Resource resource { get; set; }

            public ApiResource()
            {
                resource = new Resource();
            }
        }

        #endregion Api-Class

        #region VkId-Class

        private class VkIdResponse
        {
            public int id { get; set; }
            public string first_name { get; set; }
            public string last_name { get; set; }
        }

        private class VkId
        {
            public List<VkIdResponse> response { get; set; }
        }

        #endregion VkId-Class

        #region Discord-Class

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

        #endregion Discord-Class

        #endregion Classes

        protected void LoadConfigVariables() => configData = Config.ReadObject<ConfigData>();

        protected void SaveConfig(ConfigData config) => Config.WriteObject(config, true);

        protected override void LoadDefaultConfig()
        {
            var config = new ConfigData
            {
                timeToCheck = 60,
                ApiKey = "4VJw3lyCs5ae-UTNGq5azz_mUpTuvRrU",
                umodEnabled = true,
                messages = new ConfigData.Messages()
                {
                    Msg = "Need to update the plugin: {name} \n URL: {url} \n Current version: {old_version} \n New version: {new_version}",
                    SendMsg = true,
                    discord = new ConfigData.Messages.Discord()
                    {
                        discordEnabled = false,
                        discordMsg = "Need to update the plugin: {name} \n URL: {url} \n Current version: {old_version} \n New version: {new_version}",
                        discordWebHook = ""
                    },
                    vk = new ConfigData.Messages.Vk()
                    {
                        vkMsg = "Требуется обновление для плагина: {name} \n URL: {url} \n Текущая версия: {old_version} \n Новая версия: {new_version}",
                        vkEnabled = false,
                        vkToken = "",
                        vkUsers = new List<string>()
                        {
                            "testId"
                        }
                    }
                },
            };
            SaveConfig(config);
        }

        private void OnServerInitialized()
        {
            LoadData();
            LoadConfigVariables();
            StartTimer();
        }

        private void LoadData()
        {
            OutdatedPlugins = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<string, DataPlugin>>("UpdateChecker/Plugins");
        }

        private void SaveData()
        {
            Interface.Oxide.DataFileSystem.WriteObject("UpdateChecker/Plugins", OutdatedPlugins);
        }

        private void StartTimer()
        {
            if (configData.timeToCheck < 30)
                configData.timeToCheck = 30;

            DoCheck();
            timer.Repeat(configData.timeToCheck * 60 * 60, 0, () =>
                {
                    DoCheck();
                }
            );
        }

        private void DoCheck()
        {
            CheckUpdates();
            SaveData();
        }

        private void CheckUpdates()
        {
            Plugin[] _plugins = plugins.GetAll();
            foreach (var pl in _plugins)
            {
                try
                {
                    VersionNumber version = pl.Version;

                    int ResourceId = pl.ResourceId;

                    if (!string.IsNullOrEmpty(pl.Description))
                    {
                        CheckOudatedPlugin(ResourceId, pl);
                    }
                }
                catch (Exception ex)
                {
                    Interface.GetMod().LogError(pl.Name + "\n " + ex.ToString());
                }
            }
        }

        private void DoCheckOR(string result, Plugin plugin)
        {
            ApiResource apiResource;

            if (IsOutdatedORVersion(result, plugin.Version.ToString(), out apiResource))
            {
                AlertOutdatedPlugin(plugin, apiResource, Site.OxideRussia);
                UpdateInfoPlugin(plugin, apiResource);
            }
        }

        private void DoCheckUmod(string result, Plugin plugin, string name)
        {
            string newVersion;

            if (!result.Contains("Oxide.Plugins"))
                return;

            if (IsOutdatedUmodVersion(result, plugin.Version.ToString(), out newVersion))
            {
                ApiResource resource = new ApiResource();
                resource.resource.version = newVersion;
                resource.resource.Category.title = name;
                AlertOutdatedPlugin(plugin, resource, Site.Umod);
                UpdateInfoPlugin(plugin, resource);
            }
        }

        private void UpdateInfoPlugin(Plugin plugin, ApiResource resource)
        {
            if (OutdatedPlugins.ContainsKey(plugin.Name))
            {
                var versionResource = ParseVersion(OutdatedPlugins[plugin.Name].version);
                var versionPlugin = ParseVersion(plugin.Version.ToString());
                if (versionResource > versionPlugin)
                {
                    OutdatedPlugins.Remove(plugin.Name);
                }
            }
            else
            {
                OutdatedPlugins.Add(plugin.Name, new DataPlugin(plugin.Version.ToString(), $"https://oxide-russia.ru/plugins/{resource.resource.resource_id}"));
            }
        }

        private void AlertOutdatedPlugin(Plugin plugin, ApiResource resource, Site site)
        {
            if (OutdatedPlugins.ContainsKey(plugin.Name))
                return;

            string msg = configData.messages.Msg;

            msg = GetReplacedMsg(msg, plugin, resource, site);

            SendMsgToConsole(msg);

            SendMsgAllAdmins(msg);
            SendMsgVk(plugin, resource, site);
            SendMsgDiscord(plugin, resource, site);
        }

        private void SendMsgToConsole(string msg)
        {
            Interface.GetMod().LogWarning($"[{this.Name}] " + msg);
        }

        private void SendMsgAllAdmins(string msg)
        {
            if (!configData.messages.SendMsg)
                return;

            msg += $"[{this.Name}] " + msg;

            foreach (var pl in players.All)
            {
                if (pl.IsAdmin)
                    pl.Message(msg);
            }
        }

        #region Methods Discord

        private void SendMsgDiscord(Plugin pl, ApiResource apiResource, Site site)
        {
            bool disabled = !configData.messages.discord.discordEnabled;

            if (disabled)
                return;

            string msg = GetReplacedMsg(configData.messages.discord.discordMsg, pl, apiResource, site);
            List<string> messages = new List<string>();
            string message = string.Empty;

            for (var i = 0; i < msg.Length; i++)
            {
                var current = msg[i];
                message += current;

                if (i >= 1500)
                {
                    messages.Add(message);
                    message = string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                messages.Add(message);
            }

            foreach (var _msg in messages)
            {
                var contentType = new ContentType(_msg);
                SendMsgDiscord(contentType);
            }
        }

        private void SendMsgDiscord(ContentType contentType)
        {
            webrequest.Enqueue(configData.messages.discord.discordWebHook, JsonConvert.SerializeObject(contentType), (code, response) => { }, this, RequestMethod.POST, DiscordHeaders);
        }

        #endregion Methods Discord

        #region Methods Vk

        private void SendMsgVk(Plugin pl, ApiResource apiResource, Site site)
        {
            bool disabled = !configData.messages.vk.vkEnabled;

            if (disabled)
                return;

            string msg = GetReplacedMsg(configData.messages.vk.vkMsg, pl, apiResource, site);

            foreach (var user in configData.messages.vk.vkUsers)
            {
                DoSendMsgVk(user, msg);
            }
        }

        private void DoSendMsgVk(string id, string msg)
        {
            string resp = $"https://api.vk.com/method/users.get?user_ids={id}&fields=bdate&access_token=7c9d1d437c9d1d437c9d1d438b7cfdce5a77c9d7c9d1d4326f5f8ee31f5739e4d613b8d&v=5.84";

            webrequest.EnqueueGet(resp, (code, response) =>
            {
                try
                {
                    VkId rsp = JsonConvert.DeserializeObject<VkId>(response);

                    id = rsp.response[0].id.ToString();

                    SendVkMsg(msg, id);
                }
                catch { }
            }, this);
        }

        private void SendVkMsg(string msg, string id)
        {
            webrequest.EnqueueGet("https://api.vk.com/method/messages.send?message=" + msg + $"&user_id={id}&access_token=" + configData.messages.vk.vkToken + "&v=5.62", (code, reponse) => { }, this);
        }

        #endregion Methods Vk

        #region Methods Helper

        private void CheckOudatedPlugin(int id, Plugin plugin)
        {
            Site site = GetSite(plugin);
            string url = string.Empty;

            switch (site)
            {
                case Site.OxideRussia:
                    url = $"https://oxide-russia.ru/api/resources/{id}/";
                    SendRequestOxideRussia(url, plugin);
                    break;

                case Site.Umod:
                    string name = GetPluginName(plugin);
                    url = $"https://umod.org/plugins/{name}";
                    SendRequestUmod(url, plugin, name);
                    break;
            }
        }

        private void SendRequestOxideRussia(string url, Plugin plugin)
        {
            Dictionary<string, string> headers = new Dictionary<string, string> { { "XF-Api-Key", configData.ApiKey } };

            webrequest.EnqueueGet(url, (code, response) =>
            {
                try
                {
                    DoCheckOR(response, plugin);
                }
                catch
                {
                    //Puts();
                }
            }, this, headers);
        }

        private void SendRequestUmod(string url, Plugin plugin, string name)
        {
            webrequest.EnqueueGet(url, (code, response) =>
            {
                try
                {
                    DoCheckUmod(response, plugin, name);
                }
                catch
                {
                    //Puts();
                }
            }, this, null);
        }

        private string GetPluginName(Plugin plugin)
        {
            var splits = plugin.Filename.Split('\\');

            return splits[splits.Length - 1];
        }

        private bool IsOutdatedORVersion(string response, string version, out ApiResource apiResource)
        {
            var resource = JsonConvert.DeserializeObject<ApiResource>(response);

            apiResource = resource;

            var versionResource = ParseVersion(resource.resource.version);
            var versionPlugin = ParseVersion(version);
            return versionResource > versionPlugin;

            //return resource.resource.version != version;
        }

        private bool IsOutdatedUmodVersion(string response, string version, out string newVersion)
        {
            newVersion = ParseVersionFromPlugin(response);

            var versionResource = ParseVersion(newVersion);
            var versionPlugin = ParseVersion(version);
            return versionResource > versionPlugin;
        }

        private static Version ParseVersion(string value)
        {
            return System.Version.Parse(value);
        }

        private string ParseVersionFromPlugin(string response)
        {
            var matches = Regex.Match(response, @"\[Info\(.*\)\]", RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            string result1 = matches.Value;

            var matches2 = Regex.Match(result1, @"([0-9]+\.[0-9]+(?:\.[0-9]+)?)");
            string version = matches2.Value;

            return version;
        }

        private string GetReplacedMsg(string msg, Plugin pl, ApiResource resource, Site site)
        {
            string url = string.Empty;
            switch (site)
            {
                case Site.OxideRussia:
                    url = $"https://oxide-russia.ru/plugins/{resource.resource.resource_id}";
                    break;

                case Site.Umod:
                    url = $"https://umod.org/plugins/{resource.resource.Category.title}";
                    break;
            }

            msg = msg.Replace("{name}", pl.Name)
                    .Replace("{url}", url)
                    .Replace("{old_version}", pl.Version.ToString());

            msg = msg.Replace("{new_version}", resource.resource.version + " ");
            // ты задумаешься, нахуя здесь пробел? я отвечу.
            // если убрать пробел, то каким-то магическим способом добавляется дополнительно в конце буква 's'
            // почему? а хуй знает - https://imgur.com/a/rSMgdXI

            return msg;
        }

        private Site GetSite(Plugin plugin)
        {
            if (plugin.Description.IndexOf("oxide-russia", StringComparison.OrdinalIgnoreCase) >= 0)
                return Site.OxideRussia;

            if (configData.umodEnabled)
            {
                return Site.Umod;
            }

            return Site.Unkown;
        }

        #endregion Methods Helper
    }
}