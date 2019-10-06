using System;
using System.Collections.Generic;
using Network;

namespace Luma.Oxide
{
	// Token: 0x02000018 RID: 24
	public class Users
	{
		// Token: 0x06000084 RID: 132 RVA: 0x0000261B File Offset: 0x0000081B
		public static UserData Add(UserData userData)
		{
			if (userData != null && userData.UserID != 0UL)
			{
				Users.dictionary[userData.UserID] = userData;
			}
			return userData;
		}

		// Token: 0x06000085 RID: 133 RVA: 0x00005D4C File Offset: 0x00003F4C
		public static UserData Add(BasePlayer player)
		{
			UserData userData = null;
			if (!Users.dictionary.TryGetValue(player.userID, out userData))
			{
				userData = new UserData(player);
				Users.dictionary[player.userID] = userData;
			}
			return userData;
		}

		// Token: 0x06000086 RID: 134 RVA: 0x00005D88 File Offset: 0x00003F88
		public static UserData Add(Connection connection)
		{
			UserData userData = null;
			if (!Users.dictionary.TryGetValue(connection.userid, out userData))
			{
				userData = new UserData(connection.userid)
				{
					UserID = connection.userid,
					Username = connection.username
				};
				Users.dictionary[connection.userid] = userData;
			}
			return userData;
		}

		// Token: 0x06000087 RID: 135 RVA: 0x0000263A File Offset: 0x0000083A
		public static bool Exists(ulong steam_id)
		{
			return steam_id != 0UL && Users.dictionary.ContainsKey(steam_id);
		}

		// Token: 0x06000088 RID: 136 RVA: 0x0000264C File Offset: 0x0000084C
		public static bool Exists(BasePlayer player)
		{
			return !(player == null) && Users.dictionary.ContainsKey(player.userID);
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00002669 File Offset: 0x00000869
		public static bool Exists(Connection connection)
		{
			return connection != null && Users.dictionary.ContainsKey(connection.userid);
		}

		// Token: 0x0600008A RID: 138 RVA: 0x00005DE4 File Offset: 0x00003FE4
		public static UserData Get(ulong steam_id)
		{
			UserData result = null;
			if (!Users.dictionary.TryGetValue(steam_id, out result))
			{
				BasePlayer basePlayer = BasePlayer.FindByID(steam_id);
				if (basePlayer != null)
				{
					return Users.Add(basePlayer);
				}
				result = Users.Add(new UserData(steam_id));
			}
			return result;
		}

		// Token: 0x0600008B RID: 139 RVA: 0x00005E28 File Offset: 0x00004028
		public static UserData Get(BasePlayer player)
		{
			UserData result = null;
			if (!Users.dictionary.TryGetValue(player.userID, out result))
			{
				result = Users.Add(player);
			}
			return result;
		}

		// Token: 0x0600008C RID: 140 RVA: 0x00005BBC File Offset: 0x00003DBC
		public static UserData Get(Connection connection)
		{
			UserData result = null;
			if (!Users.dictionary.TryGetValue(connection.userid, out result))
			{
				result = Users.Add(connection);
			}
			return result;
		}

		// Token: 0x0400006B RID: 107
		public static Dictionary<ulong, UserData> dictionary = new Dictionary<ulong, UserData>();

		// Token: 0x0400006C RID: 108
		public static Dictionary<ulong, BasePlayer> Players = new Dictionary<ulong, BasePlayer>();
	}
}
