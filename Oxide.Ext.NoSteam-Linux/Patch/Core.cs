using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Facepunch;
using Facepunch.Math;
using Ionic.Crc;
using Network;
using Oxide.Core;
using Oxide.Core.Libraries.Covalence;
using Oxide.Ext.NoSteam_Linux.Core;
using Oxide.Ext.NoSteam_Linux.Helper;
using Rust;
using Steamworks;
using UnityEngine;
using HarmonyInstance = Harmony.HarmonyInstance;
using LogType = Oxide.Core.Logging.LogType;

namespace Oxide.Ext.NoSteam_Linux.Patch
{
    class Core
    {
        private static MethodInfo OnAuthenticatedLocal;

        private static MethodInfo OnAuthenticatedRemote;

        public static void Do()
        {
            ParseMethods();
            DoPatch();
        }

        public static void DoPatch()
        {
            var harmony = HarmonyInstance.Create("com.github.harmony.rust.exp");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        private static void ParseMethods()
        {
            OnAuthenticatedLocal = typeof(EACServer).GetMethod("OnAuthenticatedLocal", BindingFlags.Static | BindingFlags.NonPublic);
            OnAuthenticatedRemote = typeof(EACServer).GetMethod("OnAuthenticatedRemote", BindingFlags.Static | BindingFlags.NonPublic);
        }

        [Harmony.HarmonyPatch(typeof(ConnectionAuth))]
        [Harmony.HarmonyPatch("OnNewConnection")]
        class ConnectionAuthPatch
        {
            [Harmony.HarmonyPrefix]
            static bool Prefix(Connection connection)
            {
                if (DeveloperList.Contains(connection.userid))
                {
                    ConnectionAuth.Reject(connection, "Developer SteamId");
                    return false;
                }

                var message = Interface.CallHook("CanNewConnection", connection, !ShouldIgnore(connection));
                if (message != null)
                {
                    ConnectionAuth.Reject(connection, message.ToString());
                    return false;
                }

                return true;
            }
        }

        [Harmony.HarmonyPatch(typeof(SteamInventory), "UpdateSteamInventory")]
        class SteamInventoryPatch
        {
            [Harmony.HarmonyPrefix]
            public static bool Prefix(BaseEntity.RPCMessage msg)
            {
                if (ShouldIgnore(msg.connection))
                    return false;

                return true;
            }
        }

        [Harmony.HarmonyPatch(typeof(EACServer))]
        [Harmony.HarmonyPatch("OnJoinGame")]
        class Patch01
        {
            static bool Prefix(Connection connection)
            {
                if (ShouldIgnore(connection))
                {
                    OnAuthenticatedLocal.Invoke(null, new object[] { connection });
                    OnAuthenticatedRemote.Invoke(null, new object[] { connection });
                    return false;
                }
                return true;
            }
        }

        [Harmony.HarmonyPatch(typeof(ConnectionAuth))]
        [Harmony.HarmonyPatch("Reject")]
        class ConnectionAuthPatch2
        {
            [Harmony.HarmonyPrefix]
            static bool Prefix(Connection connection, string strReason)
            {
                if (strReason.Contains("Steam Auth Failed"))
                {
                    if (Interface.CallHook("OnSteamAuthFailed", connection) == null)
                    {
                        connection.authStatus = "ok";
                        Connect(connection);
                        return false;
                    }
                }

                return true;
            }
        }

        [Harmony.HarmonyPatch(typeof(ConVar.Server))]
        [Harmony.HarmonyPatch("cheatreport")]
        class ConvarPatch
        {
            [Harmony.HarmonyPrefix]
            static bool Prefix(ConsoleSystem.Arg arg)
            {
                BasePlayer basePlayer = arg.Player();
                if (basePlayer == null)
                    return false;

                ulong @uint = arg.GetUInt64(0, 0UL);
                var player = BasePlayer.FindByID(@uint);

                if (player == null)
                    return false;

                if (ShouldIgnore(player.Connection) || ShouldIgnore(basePlayer.Connection))
                    return false;

                return true;
            }
        }

        [Harmony.HarmonyPatch(typeof(ServerMgr))]
        [Harmony.HarmonyPatch("get_AvailableSlots")]
        class ServerPatch3
        {
            [Harmony.HarmonyPrefix]
            static bool Prefix(ref int __result)
            {
                __result = ConVar.Server.maxplayers - CountSteamPlayer();

                return false;
            }
        }

        [Harmony.HarmonyPatch(typeof(ServerMgr))]
        [Harmony.HarmonyPatch("UpdateServerInformation")]
        class ServerMgrPatch
        {
            [Harmony.HarmonyPrefix]
            static bool Prefix()
            {
                if (!SteamServer.IsValid)
                {
                    return false;
                }
                using (TimeWarning.New("UpdateServerInformation", 0))
                {
                    SteamServer.ServerName = ConVar.Server.hostname;
                    SteamServer.MaxPlayers = ConVar.Server.maxplayers;
                    SteamServer.Passworded = false;
                    SteamServer.MapName = global::World.Name;
                    string text = "stok";
                    string text2 = string.Format("born{0}", Epoch.FromDateTime(global::SaveRestore.SaveCreatedTime));
                    string text3 = string.Format("gm{0}", global::ServerMgr.GamemodeName());
                    SteamServer.GameTags = string.Format("mp{0},cp{1},qp{5},v{2}{3},h{4},{6},{7},{8}", new object[]
                    {
                        ConVar.Server.maxplayers,
                        Core.CountSteamPlayer(),
                        typeof(Protocol).GetFields().Single(x => x.Name == "network").GetRawConstantValue(),
                        ConVar.Server.pve ? ",pve" : string.Empty,
                        AssemblyHash,
                        SingletonComponent<global::ServerMgr>.Instance.connectionQueue.Queued,
                        text,
                        text2,
                        text3
                    });
                    object result = Interface.CallHook("OnGameTags", SteamServer.GameTags, Core.CountSteamPlayer());
                    if (result != null)
                    {
                        SteamServer.GameTags = result.ToString();
                    }
                    Interface.CallHook("IOnUpdateServerInformation");
                    if (ConVar.Server.description != null && ConVar.Server.description.Length > 100)
                    {
                        string[] array = ConVar.Server.description.SplitToChunks(100).ToArray<string>();
                        Interface.CallHook("IOnUpdateServerDescription");
                        for (int i = 0; i < 16; i++)
                        {
                            if (i < array.Length)
                            {
                                SteamServer.SetKey(string.Format("description_{0:00}", i), array[i]);
                            }
                            else
                            {
                                SteamServer.SetKey(string.Format("description_{0:00}", i), string.Empty);
                            }
                        }
                    }
                    else
                    {
                        SteamServer.SetKey("description_0", ConVar.Server.description);
                        for (int j = 1; j < 16; j++)
                        {
                            SteamServer.SetKey(string.Format("description_{0:00}", j), string.Empty);
                        }
                    }
                    SteamServer.SetKey("hash", AssemblyHash);
                    SteamServer.SetKey("world.seed", global::World.Seed.ToString());
                    SteamServer.SetKey("world.size", global::World.Size.ToString());
                    SteamServer.SetKey("pve", ConVar.Server.pve.ToString());
                    SteamServer.SetKey("headerimage", ConVar.Server.headerimage);
                    SteamServer.SetKey("url", ConVar.Server.url);
                    SteamServer.SetKey("gmn", global::ServerMgr.GamemodeName());
                    SteamServer.SetKey("gmt", global::ServerMgr.GamemodeTitle());
                    SteamServer.SetKey("gmd", global::ServerMgr.GamemodeDesc());
                    SteamServer.SetKey("gmu", global::ServerMgr.GamemodeUrl());
                    SteamServer.SetKey("uptime", ((int)UnityEngine.Time.realtimeSinceStartup).ToString());
                    SteamServer.SetKey("gc_mb", global::Performance.report.memoryAllocations.ToString());
                    SteamServer.SetKey("gc_cl", global::Performance.report.memoryCollections.ToString());
                    SteamServer.SetKey("fps", global::Performance.report.frameRate.ToString());
                    SteamServer.SetKey("fps_avg", global::Performance.report.frameRateAverage.ToString("0.00"));
                    SteamServer.SetKey("ent_cnt", global::BaseNetworkable.serverEntities.Count.ToString());
                    SteamServer.SetKey("build", BuildInfo.Current.Scm.ChangeId);
                }
                return false;
            }
        }

        private static string _assemblyHash = null;

        private static string AssemblyHash
        {
            get
            {
                if (_assemblyHash == null)
                {
                    string location = typeof(global::ServerMgr).Assembly.Location;
                    if (!string.IsNullOrEmpty(location))
                    {
                        byte[] array = File.ReadAllBytes(location);
                        CRC32 crc = new CRC32();
                        crc.SlurpBlock(array, 0, array.Length);
                        _assemblyHash = crc.Crc32Result.ToString("x");
                    }
                    else
                    {
                        _assemblyHash = "il2cpp";
                    }
                }
                return _assemblyHash;
            }
        }

        private static int CountSteamPlayer()
        {
            int count = 0;
            foreach (var player in BasePlayer.activePlayerList)
            {
                SteamTicket steamTicket = new SteamTicket(player.Connection);
                bool check = steamTicket.SteamId != 0 && steamTicket.SteamId == player.userID;
                if (check && steamTicket.Ticket.Token.AppID == 252490)
                {
                    count++;
                }
            }
            return count;
        }

        private static void Connect(Connection connection)
        {
            SteamServer.UpdatePlayer(connection.userid, connection.username, 0);
        }

        public static bool ShouldIgnore(Connection connection)
        {
            var steamTicket = new SteamTicket(connection);
            if (steamTicket.Ticket.Token.AppID == 480)
            {
                return true;
            }
            return false;
        }
    }
}
