// Author:  Kaidoz
// Filename: SteamSession.cs
// Last update: 2019.10.06 20:41

using System.Runtime.InteropServices;

namespace Oxide.Ext.NoSteam.Utils.Steam.Structures
{
    // Token: 0x02000011 RID: 17
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SteamSession
    {
        // Token: 0x04000044 RID: 68
        public uint Length;

        // Token: 0x04000045 RID: 69
        public uint Unknown0x1C;

        // Token: 0x04000046 RID: 70
        public uint Unknown0x20;

        // Token: 0x04000047 RID: 71
        public uint Unknown0x24;

        // Token: 0x04000048 RID: 72
        public uint Unknown0x28;

        // Token: 0x04000049 RID: 73
        public uint SessionID;

        // Token: 0x0400004A RID: 74
        public uint ConnectNumber;
    }
}