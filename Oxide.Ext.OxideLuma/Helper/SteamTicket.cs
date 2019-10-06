using System;
using System.IO;
using Network;

namespace Luma.Oxide
{
	// Token: 0x02000013 RID: 19
	public class SteamTicket
	{
		// Token: 0x06000061 RID: 97 RVA: 0x00005270 File Offset: 0x00003470
		public SteamTicket(BinaryReader binary)
		{
			this.SteamID = 0UL;
			this.Ticket = default(STEAM_TICKET);
			this.Username = binary.ReadString();
			int count = binary.ReadInt32();
			this.Token = binary.ReadBytes(count);
			if (this.Token.Length == 234)
			{
				this.Ticket = this.Token.Deserialize<STEAM_TICKET>();
				this.SteamID = this.Ticket.SteamID;
				return;
			}
			if (this.Token.Length == 240)
			{
				this.Ticket = this.Token.Deserialize<STEAM_TICKET>();
				this.SteamID = this.Ticket.SteamID;
			}
		}

		// Token: 0x06000062 RID: 98 RVA: 0x0000531C File Offset: 0x0000351C
		public SteamTicket(Connection connection = null)
		{
			this.SteamID = 0UL;
			this.Username = string.Empty;
			this.Ticket = default(STEAM_TICKET);
			this.Token = new byte[0];
			if (connection != null)
			{
				this.SteamID = connection.userid;
				this.Username = connection.username;
				this.Token = connection.token;
				if (this.Token.Length == 234)
				{
					this.Ticket = this.Token.Deserialize<STEAM_TICKET>();
					return;
				}
				if (this.Token.Length == 240)
				{
					this.Ticket = this.Token.Deserialize<STEAM_TICKET>();
				}
			}
		}

		// Token: 0x1700000C RID: 12
		// (get) Token: 0x06000063 RID: 99 RVA: 0x000023F8 File Offset: 0x000005F8
		public bool IsSteam
		{
			get
			{
				return this.SteamID != 0UL && this.Ticket.SteamID == this.SteamID && this.Ticket.Token.UserID == this.SteamID;
			}
		}

		// Token: 0x1700000D RID: 13
		// (get) Token: 0x06000064 RID: 100 RVA: 0x0000242F File Offset: 0x0000062F
		public bool IsValid
		{
			get
			{
				return this.IsSteam && this.Ticket.Token.AppID == 252490;
			}
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00002452 File Offset: 0x00000652
		public override string ToString()
		{
			return string.Format("[{0}/{1}]", this.SteamID, this.Username);
		}

		// Token: 0x06000066 RID: 102 RVA: 0x000053C4 File Offset: 0x000035C4
		public byte[] ToBytes()
		{
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
				{
					binaryWriter.Write(SteamTicket._TOKEN_HEADER);
					binaryWriter.Write((byte)SteamTicket.TOKEN_VERSION.Major);
					binaryWriter.Write((byte)SteamTicket.TOKEN_VERSION.Minor);
					binaryWriter.Write((byte)SteamTicket.TOKEN_VERSION.Build);
					binaryWriter.Write(this.Username);
					binaryWriter.Write(this.Token.Length);
					binaryWriter.Write(this.Token);
				}
				result = memoryStream.ToArray();
			}
			return result;
		}

		// Token: 0x06000067 RID: 103 RVA: 0x00005480 File Offset: 0x00003680
		public void FromBytes(byte[] bytes)
		{
			using (MemoryStream memoryStream = new MemoryStream(bytes))
			{
				using (BinaryReader binaryReader = new BinaryReader(memoryStream))
				{
					this.SteamID = binaryReader.ReadUInt64();
					this.Username = binaryReader.ReadString();
					this.Token = binaryReader.ReadBytes(binaryReader.ReadInt32());
					if (this.Token.Length == 234)
					{
						this.Ticket = this.Token.Deserialize<STEAM_TICKET>();
					}
				}
			}
		}

		// Token: 0x04000051 RID: 81
		private static readonly byte[] _TOKEN_HEADER = new byte[]
		{
			84,
			79,
			75,
			69,
			78
		};

		// Token: 0x04000052 RID: 82
		private static readonly Version TOKEN_VERSION = new Version(5, 8, 28);

		// Token: 0x04000053 RID: 83
		public ulong SteamID;

		// Token: 0x04000054 RID: 84
		public string Username;

		// Token: 0x04000055 RID: 85
		public STEAM_TICKET Ticket;

		// Token: 0x04000056 RID: 86
		public string Version;

		// Token: 0x04000057 RID: 87
		public byte[] Token;
	}
}
