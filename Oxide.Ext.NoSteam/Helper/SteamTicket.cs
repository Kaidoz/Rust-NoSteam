// Author:  Kaidoz
// Filename: SteamTicket.cs
// Last update: 2019.10.06 4:59

using System;
using System.IO;
using Network;

namespace Oxide.Ext.NoSteam.Helper
{
    public class SteamTicket
    {
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


        public STEAM_TICKET Ticket;


        public byte[] Token;


        public string Username;


        public string Version;

        public SteamTicket(Connection connection = null)
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
                    Ticket = Token.Deserialize<STEAM_TICKET>();
                    return;
                }

                if (Token.Length == 240) Ticket = Token.Deserialize<STEAM_TICKET>();
            }
        }


        public bool IsSteam => SteamId != 0UL && Ticket.SteamID == SteamId && Ticket.Token.UserID == SteamId;


        public bool IsValid => IsSteam && Ticket.Token.AppID == 252490;


        public override string ToString()
        {
            return string.Format("[{0}/{1}]", SteamId, Username);
        }


        public byte[] ToBytes()
        {
            byte[] result;
            using (var memoryStream = new MemoryStream())
            {
                using (var binaryWriter = new BinaryWriter(memoryStream))
                {
                    binaryWriter.Write(TokenHeader);
                    binaryWriter.Write((byte) TokenVersion.Major);
                    binaryWriter.Write((byte) TokenVersion.Minor);
                    binaryWriter.Write((byte) TokenVersion.Build);
                    binaryWriter.Write(Username);
                    binaryWriter.Write(Token.Length);
                    binaryWriter.Write(Token);
                }

                result = memoryStream.ToArray();
            }

            return result;
        }


        public void FromBytes(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                using (var binaryReader = new BinaryReader(memoryStream))
                {
                    SteamId = binaryReader.ReadUInt64();
                    Username = binaryReader.ReadString();
                    Token = binaryReader.ReadBytes(binaryReader.ReadInt32());
                    if (Token.Length == 234) Ticket = Token.Deserialize<STEAM_TICKET>();
                }
            }
        }
    }
}