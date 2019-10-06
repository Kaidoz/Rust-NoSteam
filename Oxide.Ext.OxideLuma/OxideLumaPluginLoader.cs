using System;
using System.Collections.Generic;
using System.Reflection;
using Oxide.Core;
using Oxide.Core.Logging;
using Oxide.Core.Plugins;

namespace Luma.Oxide
{
	// Token: 0x02000009 RID: 9
	public class OxideLumaPluginLoader : PluginLoader
	{
		// Token: 0x06000026 RID: 38 RVA: 0x000021B5 File Offset: 0x000003B5
		public OxideLumaPluginLoader(OxideLumaExtension extension)
		{
			this.Extension = extension;
			this.logger = Interface.GetMod().RootLogger;
		}

		// Token: 0x06000027 RID: 39 RVA: 0x000021D4 File Offset: 0x000003D4
		public override IEnumerable<string> ScanDirectory(string directory)
		{
			return new string[]
			{
				"Luma"
			};
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00002DF4 File Offset: 0x00000FF4
		public override Plugin Load(string directory, string name)
		{
			if (name == "Luma")
			{
				OxideLuma oxideLuma = new OxideLuma(this.Extension);
				try
				{
					FieldInfo field = base.GetType().GetField("LoadedPlugins", BindingFlags.Instance | BindingFlags.Public);
					if (field != null)
					{
						((Dictionary<string, Plugin>)field.GetValue(this)).Add(name, oxideLuma);
					}
				}
				catch
				{
				}
				return oxideLuma;
			}
			return null;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00002E5C File Offset: 0x0000105C
		public override void Unloading(Plugin plugin)
		{
			OxideLuma oxideLuma = plugin as OxideLuma;
			if (oxideLuma == null || !oxideLuma.installed)
			{
				return;
			}
			oxideLuma.installed = false;
		}

		// Token: 0x04000015 RID: 21
		private OxideLumaExtension Extension;

		// Token: 0x04000016 RID: 22
		private Logger logger;
	}
}
