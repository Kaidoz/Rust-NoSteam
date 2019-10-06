using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Oxide.Core;
using Oxide.Core.Configuration;
using Oxide.Core.Plugins;

namespace Luma.Oxide
{
	// Token: 0x0200000C RID: 12
	public static class Helpers
	{
		// Token: 0x06000045 RID: 69 RVA: 0x00003AD8 File Offset: 0x00001CD8
		public static Dictionary<string, string> GetDirectories()
		{
			return new Dictionary<string, string>
			{
				{
					"plugin",
					Interface.GetMod().PluginDirectory
				},
				{
					"extension",
					Interface.GetMod().ExtensionDirectory
				},
				{
					"config",
					Interface.GetMod().ConfigDirectory
				},
				{
					"data",
					Interface.GetMod().DataDirectory
				},
				{
					"log",
					Interface.GetMod().LogDirectory
				},
				{
					"root",
					Interface.GetMod().RootDirectory
				}
			};
		}

		// Token: 0x06000046 RID: 70 RVA: 0x00003B68 File Offset: 0x00001D68
		public static JArray GetPlugins()
		{
			OxideMod mod = Interface.GetMod();
			PluginManager rootPluginManager = mod.RootPluginManager;
			JArray jarray = new JArray();
			List<string> list = new List<string>();
			foreach (Plugin plugin in rootPluginManager.GetPlugins())
			{
				JObject jobject = new JObject();
				jobject["id"] = plugin.ResourceId;
				jobject["name"] = plugin.Name;
				jobject["title"] = plugin.Title;
				jobject["version"] = plugin.Version.ToString();
				jobject["author"] = plugin.Author;
				try
				{
					string path = (string)plugin.Object.GetType().GetField("Filename", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetField).GetValue(plugin.Object);
					string fileName = Path.GetFileName(path);
					list.Add(fileName);
					jobject["filename"] = fileName;
					try
					{
						jobject["source"] = File.ReadAllText(path, Encoding.UTF8);
					}
					catch
					{
					}
				}
				catch
				{
				}
				if (plugin.HasConfig)
				{
					JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();
					jsonSerializerSettings.Converters.Add(new KeyValuesConverter());
					jobject["config"] = JsonConvert.SerializeObject(plugin.Config, Formatting.Indented, jsonSerializerSettings);
				}
				jarray.Add(jobject);
			}
			foreach (string text in Directory.GetFiles(mod.PluginDirectory))
			{
				if (Helpers.pluginFileRe.IsMatch(text))
				{
					string fileName2 = Path.GetFileName(text);
					if (!list.Contains(fileName2))
					{
						JObject jobject2 = new JObject();
						jobject2["filename"] = fileName2;
						string path2 = Path.Combine(mod.PluginDirectory, fileName2);
						if (File.Exists(path2))
						{
							try
							{
								jobject2["source"] = File.ReadAllText(path2, Encoding.UTF8);
							}
							catch
							{
							}
						}
						string path3 = Path.Combine(mod.ConfigDirectory, Path.GetFileNameWithoutExtension(fileName2) + ".json");
						if (File.Exists(path3))
						{
							try
							{
								jobject2["config"] = File.ReadAllText(path3, Encoding.UTF8);
							}
							catch
							{
							}
						}
						jarray.Add(jobject2);
					}
				}
			}
			return jarray;
		}

		// Token: 0x06000047 RID: 71 RVA: 0x00003E84 File Offset: 0x00002084
		public static bool InstallPlugin(string filename, byte[] source)
		{
			filename = Path.Combine(Interface.GetMod().PluginDirectory, Path.GetFileName(filename));
			bool result;
			try
			{
				File.WriteAllBytes(filename, source);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00003ECC File Offset: 0x000020CC
		public static bool InstallConfig(string filename, string source)
		{
			filename = Path.Combine(Interface.GetMod().ConfigDirectory, Path.GetFileName(filename));
			bool result;
			try
			{
				File.WriteAllText(filename, source, Encoding.UTF8);
				result = true;
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x06000049 RID: 73 RVA: 0x00003F18 File Offset: 0x00002118
		public static bool UninstallPlugin(string filename, bool keepConfig = false)
		{
			filename = Path.Combine(Interface.GetMod().PluginDirectory, Path.GetFileName(filename));
			bool result;
			try
			{
				if (!File.Exists(filename))
				{
					result = false;
				}
				else
				{
					File.Delete(filename);
					if (!keepConfig)
					{
						try
						{
							File.Delete(Path.Combine(Interface.GetMod().ConfigDirectory, Path.GetFileNameWithoutExtension(filename) + ".json"));
						}
						catch
						{
						}
					}
					result = true;
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		// Token: 0x0400001D RID: 29
		private static Regex pluginFileRe = new Regex("\\.(cs|lua|js|py)$");
	}
}
