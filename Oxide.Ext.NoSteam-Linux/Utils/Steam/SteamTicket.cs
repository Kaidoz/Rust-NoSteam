// Author:  Kaidoz
// Filename: SteamTicket.cs
// Last update: 2019.10.06 20:41

using System;
using System.IO;
using Network;
using UnityEngine;

namespace Oxide.Ext.NoSteam.Helper
{
    public class SteamTicket
    {

        public enum ClientVersion
        {
            NoSteam,
            Steam,
            Unkown
        }

        private static readonly byte[] TokenHeader =
        {
            84,
            79,
            75,
            69,
            78
        };

        private static readonly Version TokenVersion = new Version(5, 8, 28);

        public ulong SteamId;

        public Ticket Ticket;

        public byte[] Token;

        public string Username;

        public string Version;

        public SteamTicket(Connection connection)
        {
            SteamId = 0UL;
            Username = string.Empty;
            Ticket = default;
            Token = new byte[0];

            if (connection != null)
            {
                SteamId = connection.userid;
                Username = connection.username;
                Token = connection.token;

                if (Token.Length == 234)
                {
                    Ticket = Token.Deserialize<Ticket>();
                    return;
                }

                if (Token.Length == 240) Ticket = Token.Deserialize<Ticket>();
            }
        }

        public SteamTicket(byte[] authToken)
        {
            SteamId = 0UL;
            Username = string.Empty;
            Ticket = default;
            Token = authToken;
        }
        private bool IsCrack => Token.Length == 234;

        private bool IsLicense => Token.Length == 240;


        public ClientVersion clientVersion;

        public void GetClientVersion()
        {
            if (IsCrack)
                clientVersion = ClientVersion.NoSteam;
            else if (IsLicense)
                clientVersion = ClientVersion.Steam;
            else
                clientVersion = ClientVersion.Unkown;
        }

        public override string ToString()
        {
            return string.Format("[{0}/{1}]", SteamId, Username);
        }
    }
}