// Author:  Kaidoz
// Filename: SteamTokendata.cs
// Last update: 2019.10.06 20:41

using System.Runtime.InteropServices;

namespace Oxide.Ext.NoSteam.Utils.Steam.Structures
{
    // Token: 0x02000010 RID: 16
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SteamTokendata
    {
        // Token: 0x0400002F RID: 47
        public int Length;

        // Token: 0x04000030 RID: 48
        public int Unknown0x38;

        // Token: 0x04000031 RID: 49
        public int Unknown0x3C;

        // Token: 0x04000032 RID: 50
        public ulong UserID;

        // Token: 0x04000033 RID: 51
        public int AppID;

        // Token: 0x04000034 RID: 52
        public int Unknown0x4C;

        // Token: 0x04000035 RID: 53
        public byte Unknown0x50;

        // Token: 0x04000036 RID: 54
        public byte Unknown0x51;

        // Token: 0x04000037 RID: 55
        public byte Unknown0x52;

        // Token: 0x04000038 RID: 56
        public byte Unknown0x53;

        // Token: 0x04000039 RID: 57
        public uint Unknown0x54;

        // Token: 0x0400003A RID: 58
        public uint StartTime;

        // Token: 0x0400003B RID: 59
        public uint EndedTime;

        // Token: 0x0400003C RID: 60
        public byte Unknown0x60;

        // Token: 0x0400003D RID: 61
        public byte Unknown0x61;

        // Token: 0x0400003E RID: 62
        public byte Unknown0x62;

        // Token: 0x0400003F RID: 63
        public byte Unknown0x63;

        // Token: 0x04000040 RID: 64
        public short Unknown0x64;

        // Token: 0x04000041 RID: 65
        public short Unknown0x66;

        // Token: 0x04000042 RID: 66
        public short Unknown0x68;

        // Token: 0x04000043 RID: 67
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] SHA128;
    }
}