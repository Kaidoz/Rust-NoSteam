using System;
using System.Linq;
using System.Collections.Generic;
using Oxide.Core;
using Oxide.Core.Plugins;
using UnityEngine;
using Newtonsoft.Json;
using uMod.Libraries.Universal;

namespace Oxide.Plugins
{
    [Info("PluginsTime", "oxide-russia.ru", "1.0.0", ResourceId = 223)]
    internal class PluginsTime : RustPlugin
    {
        #region Hooks

        private void Init()
        {
            //plugins.PluginManager.
            LoadDefaultMessages();
        }

        #endregion Hooks

        #region Commands

        [ConsoleCommand("pluginstime")]
        private void CommandPluginsTime(ConsoleSystem.Arg arg)
        {
            var player = arg?.Player();
            if (player != null && !player.IsAdmin) return;

            string name, time, percent, result = GetLangMessage("INFO.TITLE");
            foreach (var plugin in plugins.PluginManager.GetPlugins().Where(x => !x.IsCorePlugin).OrderByDescending(x => x.TotalHookTime))
            {
                name = plugin.Filename?.Basename(null) ?? (!string.IsNullOrEmpty(plugin.Title) ? $"{plugin.Title.Replace(" ", "")}.dll" : "N|A");
                time = ((double)Math.Round(plugin.TotalHookTime, 2)).ToString("0.###");
                percent = ((double)Math.Round((100f * plugin.TotalHookTime) / UnityEngine.Time.realtimeSinceStartup, 5)).ToString("0.###");
                result += GetLangMessage("INFO.LINE").Replace("{NAME}", name).Replace("{TIME}", time).Replace("{PERCENT}", percent);
            }

            if (player != null)
                PrintToConsole(player, result);
            else
                Puts(result);
        }

        #endregion Commands

        #region Lang

        private void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                {"INFO.TITLE", "\nСписок плагинов с их нагрузкой на сервер:\n"},
                {"INFO.LINE", "{NAME} {TIME}с ({PERCENT}%)\n"}
            }, this);
        }

        private string GetLangMessage(string key, string steamID = null) => lang.GetMessage(key, this, steamID);

        #endregion Lang
    }
}