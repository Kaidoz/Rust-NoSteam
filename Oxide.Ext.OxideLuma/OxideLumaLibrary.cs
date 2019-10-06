using System;
using Oxide.Core.Libraries;

namespace Luma.Oxide
{
	// Token: 0x0200000B RID: 11
	public class OxideLumaLibrary : Library
	{
		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000042 RID: 66 RVA: 0x000022D4 File Offset: 0x000004D4
		public override bool IsGlobal
		{
			get
			{
				return false;
			}
		}

		// Token: 0x06000043 RID: 67 RVA: 0x000022D7 File Offset: 0x000004D7
		internal OxideLumaLibrary(OxideLumaExtension extension)
		{
			this.extension = extension;
		}

		// Token: 0x06000044 RID: 68 RVA: 0x000022E6 File Offset: 0x000004E6
		[LibraryFunction("IsInstalled")]
		public bool IsInstalled()
		{
			return Module.Instance != null;
		}

		// Token: 0x0400001C RID: 28
		private OxideLumaExtension extension;
	}
}
