// Author:  Kaidoz
// Filename: Runtime.cs
// Last update: 2019.10.06 4:59

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Network;
using Oxide.Core;
using Steamworks;
using UnityEngine;

namespace Oxide.Ext.NoSteam.Patch
{
    internal class Runtime : MonoBehaviour
    {
        public static void OnNewConnection(ConnectionAuth connectionAuth, Connection connection)
        {
            connection.connected = false;
            if (connection.token == null || connection.token.Length < 32)
            {
                ConnectionAuth.Reject(connection, "Invalid Token");
                return;
            }

            if (connection.userid == 0UL)
            {
                ConnectionAuth.Reject(connection, "Invalid SteamID");
                return;
            }

            if (DeveloperList.Contains(connection.userid))
            {
                ConnectionAuth.Reject(connection, "Developer SteamId");
                return;
            }

            if (ServerUsers.Is(connection.userid, ServerUsers.UserGroup.Banned))
            {
                ConnectionAuth.Reject(connection, "You are banned from this server");
                return;
            }

            if (ServerUsers.Is(connection.userid, ServerUsers.UserGroup.Moderator))
            {
                DebugEx.Log(connection + " has auth level 1");
                connection.authLevel = 1u;
            }

            if (ServerUsers.Is(connection.userid, ServerUsers.UserGroup.Owner))
            {
                DebugEx.Log(connection + " has auth level 2");
                connection.authLevel = 2u;
            }

            if (connectionAuth.IsConnected(connection.userid))
            {
                ConnectionAuth.Reject(connection, "You are already connected!");
                return;
            }
            if (Interface.CallHook("IOnUserApprove", connection) != null) return;

            Core.CheckConnection(connection);
            ConnectionAuth.m_AuthConnection.Add(connection);
            connectionAuth.StartCoroutine(connectionAuth.AuthorisationRoutine(connection));
        }

        public static void DoTick()
        {
            if (SteamServer.IsValid)
            {
                Interface.CallHook("OnTick");
                SteamServer.RunCallbacks();
            }
            Facepunch.RCon.Update();
            for (int i = 0; i < Network.Net.sv.connections.Count; i++)
            {
                Network.Connection connection = Network.Net.sv.connections[i];
                if (!connection.isAuthenticated && connection.GetSecondsConnected() >= (float)ConVar.Server.authtimeout)
                {
                    continue;
                }
            }
        }

        public static void Reject(Connection connection, string strReason)
        {
            if (strReason.Contains("Steam Auth Failed"))
            {
                Interface.CallHook("OnSteamAuthFailed", connection);
                connection.authStatus = "ok";
                Connect(connection);
                return;
            }
            DebugEx.Log(connection.ToString() + " Rejecting connection - " + strReason, StackTraceLogType.None);
            Net.sv.Kick(connection, strReason);
            global::ConnectionAuth.m_AuthConnection.Remove(connection);
        }

        private static void Connect(Connection connection)
        {
            SteamServer.UpdatePlayer(connection.userid, connection.username, 0);
        }
    }
}