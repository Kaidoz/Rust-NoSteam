using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Ext.NoSteam.Utils.Steam.Steamworks
{
    public enum AuthResponse
    {
        OK,
        UserNotConnectedToSteam,
        NoLicenseOrExpired,
        VACBanned,
        LoggedInElseWhere,
        VACCheckTimedOut,
        AuthTicketCanceled,
        AuthTicketInvalidAlreadyUsed,
        AuthTicketInvalid,
        PublisherIssuedBan
    }
}
