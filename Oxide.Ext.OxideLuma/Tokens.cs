using System;
using System.Collections.Generic;
using Network;

namespace Luma.Oxide
{
	// Token: 0x02000015 RID: 21
	public class Tokens
	{
		// Token: 0x1700000E RID: 14
		// (get) Token: 0x0600006A RID: 106 RVA: 0x00002495 File Offset: 0x00000695
		// (set) Token: 0x0600006B RID: 107 RVA: 0x0000249C File Offset: 0x0000069C
		public static string SavePath { get; private set; }

		// Token: 0x0600006C RID: 108 RVA: 0x000024A4 File Offset: 0x000006A4
		public static bool OnConnected(Connection connection)
		{
			if (Tokens.Available.Contains(connection.userid))
			{
				Tokens.Available.Remove(connection.userid);
			}
			return true;
		}

		// Token: 0x0600006D RID: 109 RVA: 0x000024CA File Offset: 0x000006CA
		public static bool OnDisconnected(Connection connection)
		{
			if (!Tokens.Available.Contains(connection.userid))
			{
				Tokens.Available.Add(connection.userid);
			}
			return true;
		}

		// Token: 0x0600006E RID: 110 RVA: 0x000024EF File Offset: 0x000006EF
		public static bool OnDisconnected(BasePlayer player)
		{
			if (!Tokens.Available.Contains(player.userID))
			{
				Tokens.Available.Add(player.userID);
			}
			return true;
		}

		// Token: 0x0400005E RID: 94
		private static readonly byte[] _DB_HEADER = new byte[]
		{
			68,
			66,
			55,
			122
		};

		// Token: 0x0400005F RID: 95
		private static readonly Version _DB_MINVERSION = new Version(5, 9, 2);

		// Token: 0x04000060 RID: 96
		public static Dictionary<ulong, SteamTicket> Dictionary = new Dictionary<ulong, SteamTicket>();

		// Token: 0x04000061 RID: 97
		public static List<ulong> Available = new List<ulong>();

		// Token: 0x04000062 RID: 98
		public static List<ulong> FakeID = new List<ulong>();
	}
}
