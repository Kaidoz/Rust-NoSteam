// Author:  Kaidoz
// Filename: SteamTicket.cs
// Last update: 2019.10.06 20:41

using System;
using System.IO;
using System.Linq;
using System.Text;
using Network;

namespace ConsoleApp1.Helper
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

            //DebugEx.Log($"SteamTicket {Token.Length}", StackTraceLogType.None);

            FromBytes(authToken);
        }



        private bool IsSteam => SteamId != 0UL && Ticket.SteamID == SteamId && Ticket.Token.UserID == SteamId;

        private bool IsCrack => IsSteam && Ticket.Token.AppID == 480;

        private bool IsLicense => IsSteam && Ticket.Token.AppID == 252490;


        public ClientVersion clientVersion;

        public void GetClientVersion()
        {
            if (IsCrack)
                clientVersion = ClientVersion.NoSteam;

            if(IsLicense)
                clientVersion = ClientVersion.Steam;

            clientVersion = ClientVersion.Unkown;

            //DebugEx.Log($"Not kicking {this.SteamId} " + clientVersion, StackTraceLogType.None);

            //DebugEx.Log($"GetClientVersion {this.IsSteam} {IsCrack} {IsLicense} {Ticket.Token.AppID} {Ticket.Token.UserID}", StackTraceLogType.None);
        }

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
                    binaryWriter.Write((byte)TokenVersion.Major);
                    binaryWriter.Write((byte)TokenVersion.Minor);
                    binaryWriter.Write((byte)TokenVersion.Build);
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
                    File.WriteAllBytes("steamId.txt", bytes);
                    for(int a = 0; a < 100; a++)
                    {
                        Token = binaryReader.ReadBytes(binaryReader.ReadInt64());
                        Ticket = Token.Deserialize<Ticket>();
                    }
                    SteamId = binaryReader.ReadUInt64();
                    Username = binaryReader.ReadString();
                    Token = binaryReader.ReadBytes(binaryReader.ReadInt32());
                    if (Token.Length == 234)
                    {
                        Ticket = Token.Deserialize<Ticket>();
                        return;
                    }

                    if (Token.Length == 240) Ticket = Token.Deserialize<Ticket>();
                }
            }
        }

        static string UnicodeToUTF8(string from)
        {
            var bytes = Encoding.UTF8.GetBytes(from);
            return new string(bytes.Select(b => (char)b).ToArray());
        }

    }
}