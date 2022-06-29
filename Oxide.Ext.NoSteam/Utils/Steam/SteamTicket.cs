using Network;
using Oxide.Ext.NoSteam.Utils.Steam.Structures;

namespace Oxide.Ext.NoSteam.Utils.Steam
{
    public class SteamTicket
    {
        public enum ClientVersion
        {
            NoSteam,
            Steam,
            Unkown
        }

        public ulong SteamId;

        public Ticket Ticket;

        public byte[] Token;

        public string Username;

        public string Version;

        public SteamTicket(Connection connection)
        {
            Token = connection.token;

            if (connection != null)
            {
                SteamId = connection.userid;
                Username = connection.username;
                Token = connection.token;

                Ticket = Token.Deserialize<Ticket>();

                GetClientVersion();
            }
        }

        public SteamTicket(byte[] authToken)
        {
            Token = authToken;

            Ticket = Token.Deserialize<Ticket>();

            SteamId = Ticket.Token.UserID;
            Username = Ticket.Token.UserID.ToString();

            GetClientVersion();
        }
        private bool IsCrack => Ticket.Token.AppID == 480;

        private bool IsLicense => Ticket.Token.AppID == 252490;


        public ClientVersion clientVersion;

        private void GetClientVersion()
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