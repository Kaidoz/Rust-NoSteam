using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Network;

namespace Luma.Oxide
{
	// Token: 0x02000017 RID: 23
	public class UserData : IEnumerable<KeyValuePair<string, object>>, IEnumerable
	{
		// Token: 0x06000073 RID: 115 RVA: 0x00005A64 File Offset: 0x00003C64
		public UserData(ulong steam_id)
		{
			if (steam_id == 0UL)
			{
				this.UserID = 0UL;
				this.Username = "SERVER";
				return;
			}
			this.UserID = steam_id;
			this.player = BasePlayer.FindByID(steam_id);
			if (this.player != null)
			{
				this.Username = this.player.displayName;
			}
			this.GetEntityID();
		}

		// Token: 0x06000074 RID: 116 RVA: 0x00005AE0 File Offset: 0x00003CE0
		public UserData(BasePlayer player)
		{
			if (player == null)
			{
				this.UserID = 0UL;
				this.Username = "SERVER";
				return;
			}
			this.player = player;
			this.UserID = player.userID;
			this.Username = player.displayName;
			this.GetEntityID();
		}

		// Token: 0x06000075 RID: 117 RVA: 0x00005B4C File Offset: 0x00003D4C
		public UserData(Connection connection)
		{
			if (connection == null)
			{
				this.UserID = 0UL;
				this.Username = "SERVER";
				return;
			}
			this.player = (connection.player as BasePlayer);
			this.UserID = connection.userid;
			this.Username = connection.username;
			this.GetEntityID();
		}

		// Token: 0x1700000F RID: 15
		// (get) Token: 0x06000076 RID: 118 RVA: 0x00002514 File Offset: 0x00000714
		public bool IsOnline
		{
			get
			{
				return this.Player != null && this.Player.IsConnected;
			}
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00005BBC File Offset: 0x00003DBC
		public static UserData Get(Connection connection)
		{
			UserData result = null;
			if (!Users.dictionary.TryGetValue(connection.userid, out result))
			{
				result = Users.Add(connection);
			}
			return result;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00002531 File Offset: 0x00000731
		public uint GetEntityID()
		{
			if (this.Player != null)
			{
				this.EntityID = this.player.net.ID;
			}
			return this.EntityID;
		}

		// Token: 0x17000010 RID: 16
		// (get) Token: 0x06000079 RID: 121 RVA: 0x0000255D File Offset: 0x0000075D
		public string[] Params
		{
			get
			{
				return this.Extra.Keys.ToArray<string>();
			}
		}

		// Token: 0x0600007A RID: 122 RVA: 0x0000256F File Offset: 0x0000076F
		public void Set(string key, string value)
		{
			if (key != null)
			{
				if (!string.IsNullOrEmpty(value))
				{
					this.Extra[key] = value;
					return;
				}
				if (this.Extra.ContainsKey(key))
				{
					this.Extra.Remove(key);
				}
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x00005BE8 File Offset: 0x00003DE8
		public string GetExtra()
		{
			string text = string.Empty;
			foreach (string text2 in this.Extra.Keys)
			{
				text = string.Concat(new string[]
				{
					text,
					",",
					text2,
					"=",
					this.Extra[text2].ToString()
				});
			}
			return text.TrimStart(new char[]
			{
				','
			});
		}

		// Token: 0x0600007C RID: 124 RVA: 0x00005C88 File Offset: 0x00003E88
		public void ParseExtra(string parse)
		{
			string[] array = parse.Split(new char[]
			{
				','
			});
			for (int i = 0; i < array.Length; i++)
			{
				string[] array2 = array[i].Split(new char[]
				{
					'='
				}, 2);
				if (array2.Length > 1 && !string.IsNullOrEmpty(array2[0]))
				{
					this.Extra[array2[0]] = array2[1];
				}
			}
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000025A5 File Offset: 0x000007A5
		public void ClearExtra()
		{
			this.Extra.Clear();
		}

		// Token: 0x17000011 RID: 17
		// (get) Token: 0x0600007E RID: 126 RVA: 0x000025B2 File Offset: 0x000007B2
		// (set) Token: 0x0600007F RID: 127 RVA: 0x000025D9 File Offset: 0x000007D9
		public BasePlayer Player
		{
			get
			{
				if (this.player == null)
				{
					this.player = BasePlayer.FindByID(this.UserID);
				}
				return this.player;
			}
			set
			{
				this.player = value;
			}
		}

		// Token: 0x06000080 RID: 128 RVA: 0x00005CEC File Offset: 0x00003EEC
		public void Update()
		{
			if (this.Player != null && this.player.net.connection != null)
			{
				this.Username = this.player.net.connection.username;
				this.EntityID = this.player.net.ID;
			}
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000025E2 File Offset: 0x000007E2
		public override string ToString()
		{
			if (this.UserID == 0UL)
			{
				return this.Username;
			}
			return string.Format("{0}/{1}", this.UserID, this.Username);
		}

		// Token: 0x06000082 RID: 130 RVA: 0x0000260E File Offset: 0x0000080E
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, object>>)this.Extra).GetEnumerator();
		}

		// Token: 0x06000083 RID: 131 RVA: 0x0000260E File Offset: 0x0000080E
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<KeyValuePair<string, object>>)this.Extra).GetEnumerator();
		}

		// Token: 0x04000064 RID: 100
		private Dictionary<string, object> Extra = new Dictionary<string, object>();

		// Token: 0x04000065 RID: 101
		private BasePlayer player;

		// Token: 0x04000066 RID: 102
		public ulong UserID;

		// Token: 0x04000067 RID: 103
		public uint EntityID;

		// Token: 0x04000068 RID: 104
		public string Username = string.Empty;

		// Token: 0x04000069 RID: 105
		[NonSerialized]
		public bool IsSteam;

		// Token: 0x0400006A RID: 106
		[NonSerialized]
		public NoSteamTicket NoSteam;
	}
}
