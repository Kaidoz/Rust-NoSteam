using System;
using System.Linq;
using System.Text;
using Network;

namespace Luma.Oxide
{
	// Token: 0x02000014 RID: 20
	public class NoSteamTicket
	{
		// Token: 0x06000069 RID: 105 RVA: 0x00005518 File Offset: 0x00003718
		public NoSteamTicket(Connection connection = null)
		{
			this.SteamID = 0UL;
			this.Username = string.Empty;
			this.EmuName = string.Empty;
			this.Version = string.Empty;
			this.Token = new byte[0];
			this.Ticket = default(STEAM_TICKET);
			if (connection != null)
			{
				this.SteamID = connection.userid;
				this.Username = connection.username;
				this.Token = connection.token.ToArray<byte>();
				if (this.Token.Length == 234)
				{
					this.Ticket = this.Token.Deserialize<STEAM_TICKET>();
				}
				if (connection.token.Length.ToString() == "556" && this.SteamID == BitConverter.ToUInt64(this.Token, 8))
				{
					int num = BitConverter.ToInt32(this.Token, 280);
					if (num > 0 && num < 16 && BitConverter.ToInt32(this.Token, 544) == 3)
					{
						string text = Encoding.ASCII.GetString(this.Token, 284, num).Remove(5);
						if (!string.IsNullOrEmpty(text))
						{
							this.Version = text;
						}
						Version version = new Version(text);
						if (version.Major >= 1 && version.Minor >= 4)
						{
							this.EmuName = "LumaEmu";
							return;
						}
					}
				}
				else if (connection.token.Length.ToString() == "596" && this.SteamID == BitConverter.ToUInt64(this.Token, 8))
				{
					int num2 = BitConverter.ToInt32(this.Token, 280);
					if (num2 > 0 && num2 < 16 && BitConverter.ToInt32(this.Token, 544) == 3)
					{
						string text2 = Encoding.ASCII.GetString(this.Token, 284, num2).Remove(5);
						if (!string.IsNullOrEmpty(text2))
						{
							this.Version = text2;
						}
						Version version2 = new Version(text2);
						if (version2.Major >= 1 && version2.Minor >= 4)
						{
							this.EmuName = "LumaEmu";
							return;
						}
					}
				}
				else if (connection.token.Length.ToString() == "304" && this.SteamID == BitConverter.ToUInt64(this.Token, 8))
				{
					int num3 = BitConverter.ToInt32(this.Token, 180);
					if (num3 > 0 && num3 < 56)
					{
						string text3 = Encoding.ASCII.GetString(this.Token, 176, num3).Remove(5);
						if (!string.IsNullOrEmpty(text3))
						{
							this.Version = text3;
						}
						Version version3 = new Version(text3);
						if (version3.Major >= 1 && version3.Minor >= 4)
						{
							this.EmuName = "LumaEmu";
							return;
						}
					}
				}
				else if (connection.token.Length.ToString() == "240" && this.SteamID == BitConverter.ToUInt64(this.Token, 8))
				{
					int num4 = BitConverter.ToInt32(this.Token, 180);
					if (num4 > 0 && num4 < 56)
					{
						string text4 = Encoding.ASCII.GetString(this.Token, 176, num4).Remove(5);
						if (!string.IsNullOrEmpty(text4))
						{
							this.Version = text4;
						}
						Version version4 = new Version(text4);
						if (version4.Major >= 1 && version4.Minor >= 4)
						{
							this.EmuName = "LumaEmu";
							return;
						}
					}
				}
				else if (connection.token.Length == 548 && this.SteamID == BitConverter.ToUInt64(this.Token, 8))
				{
					int num5 = BitConverter.ToInt32(this.Token, 280);
					if (num5 > 0 && num5 < 16 && BitConverter.ToInt32(this.Token, 544) == 3)
					{
						string text5 = Encoding.ASCII.GetString(this.Token, 284, num5).Remove(5);
						if (!string.IsNullOrEmpty(text5))
						{
							this.Version = text5;
						}
						Version version5 = new Version(text5);
						if (version5.Major >= 1 && version5.Minor >= 4)
						{
							this.EmuName = "LumaEmu";
						}
					}
				}
			}
		}

		// Token: 0x04000058 RID: 88
		public ulong SteamID;

		// Token: 0x04000059 RID: 89
		public string Username;

		// Token: 0x0400005A RID: 90
		public string EmuName;

		// Token: 0x0400005B RID: 91
		public string Version;

		// Token: 0x0400005C RID: 92
		public STEAM_TICKET Ticket;

		// Token: 0x0400005D RID: 93
		public byte[] Token;
	}
}
