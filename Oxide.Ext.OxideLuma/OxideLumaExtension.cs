using System;
using Oxide.Core;
using Oxide.Core.Extensions;
using Oxide.Core.Logging;

namespace Luma.Oxide
{
	// Token: 0x02000008 RID: 8
	public class OxideLumaExtension : Extension
	{
		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000018 RID: 24 RVA: 0x0000214A File Offset: 0x0000034A
		public override string Name
		{
			get
			{
				return "Luma";
			}
		}

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002151 File Offset: 0x00000351
		public override VersionNumber Version
		{
			get
			{
				return EXTENSION.Version;
			}
		}

		// Token: 0x17000008 RID: 8
		// (get) Token: 0x0600001A RID: 26 RVA: 0x00002158 File Offset: 0x00000358
		public override string Author
		{
			get
			{
				return "Мизантроп";
			}
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x0600001B RID: 27 RVA: 0x0000215F File Offset: 0x0000035F
		// (set) Token: 0x0600001C RID: 28 RVA: 0x00002166 File Offset: 0x00000366
		public static OxideLumaExtension Instance { get; private set; }

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600001D RID: 29 RVA: 0x0000216E File Offset: 0x0000036E
		// (set) Token: 0x0600001E RID: 30 RVA: 0x00002176 File Offset: 0x00000376
		public OxideLumaLibrary Library { get; private set; }

		// Token: 0x0600001F RID: 31 RVA: 0x0000217F File Offset: 0x0000037F
		public OxideLumaExtension(ExtensionManager manager) : base(manager)
		{
			this.logger = Interface.GetMod().RootLogger;
			OxideLumaExtension.Instance = this;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x0000219E File Offset: 0x0000039E
		public void Log(string message)
		{
			this.logger.Write(LogType.Chat, message, new object[0]);
		}

		// Token: 0x06000021 RID: 33 RVA: 0x00002D70 File Offset: 0x00000F70
		public override void Load()
		{
			base.Manager.RegisterPluginLoader(new OxideLumaPluginLoader(this));
			base.Manager.RegisterLibrary("Luma", this.Library = new OxideLumaLibrary(this));
		}

		// Token: 0x06000022 RID: 34 RVA: 0x00002DB0 File Offset: 0x00000FB0
		internal bool Call(string method, params object[] args)
		{
			Module instance = Module.Instance;
			return instance != null && instance.Call(method, args);
		}

		// Token: 0x06000023 RID: 35 RVA: 0x00002DD0 File Offset: 0x00000FD0
		internal bool Call(string method, out object returnValue, params object[] args)
		{
			returnValue = null;
			Module instance = Module.Instance;
			return instance != null && instance.Call(method, out returnValue, args);
		}

		// Token: 0x06000024 RID: 36 RVA: 0x000021B3 File Offset: 0x000003B3
		public override void LoadPluginWatchers(string s)
		{
		}

		// Token: 0x06000025 RID: 37 RVA: 0x000021B3 File Offset: 0x000003B3
		public override void OnModLoad()
		{
		}

		// Token: 0x04000012 RID: 18
		private CompoundLogger logger;
	}
}
