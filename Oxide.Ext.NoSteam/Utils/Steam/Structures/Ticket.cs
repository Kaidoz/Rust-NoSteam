// Author:  Kaidoz
// Filename: Ticket.cs
// Last update: 2019.10.06 20:41

using System.Runtime.InteropServices;

namespace Oxide.Ext.NoSteam.Utils.Steam.Structures
{
    // Token: 0x02000012 RID: 18
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 234)]
    public struct Ticket
    {
        // Token: 0x0400004B RID: 75
        public uint Length;

        // Token: 0x0400004C RID: 76
        public ulong ID;

        // Token: 0x0400004D RID: 77
        public ulong SteamID;

        // Token: 0x0400004E RID: 78
        public uint ConnectionTime;

        // Token: 0x0400004F RID: 79
        public SteamSession Session;

        // Token: 0x04000050 RID: 80
        public SteamTokendata Token;
    }
}