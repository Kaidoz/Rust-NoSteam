﻿using System;
using System.Runtime.InteropServices;

namespace Luma.Oxide
{
	// Token: 0x02000012 RID: 18
	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 234)]
	public struct STEAM_TICKET
	{
		// Token: 0x0400004B RID: 75
		public uint Length;

		// Token: 0x0400004C RID: 76
		public ulong ID;

		// Token: 0x0400004D RID: 77
		public ulong SteamID;

		// Token: 0x0400004E RID: 78
		public uint ConnectionTime;

		// Token: 0x0400004F RID: 79
		public STEAM_SESSION Session;

		// Token: 0x04000050 RID: 80
		public STEAM_TOKENDATA Token;
	}
}
