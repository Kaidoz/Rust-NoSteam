// Author:  Kaidoz
// Filename: Serilization.cs
// Last update: 2019.10.06 20:41

using System.Runtime.InteropServices;

namespace Oxide.Ext.NoSteam.Utils.Steam
{
    public static class Convertor
    {
        public static byte[] Serialize<T>(this T structure) where T : struct
        {
            var array = new byte[Marshal.SizeOf(typeof(T))];
            var intPtr = Marshal.AllocHGlobal(array.Length);
            Marshal.StructureToPtr(structure, intPtr, true);
            Marshal.Copy(intPtr, array, 0, array.Length);
            Marshal.FreeHGlobal(intPtr);
            return array;
        }

        // Token: 0x06000072 RID: 114 RVA: 0x00005A0C File Offset: 0x00003C0C
        public static T Deserialize<T>(this byte[] bytes) where T : struct
        {
            try
            {
                var gchandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                var result = (T)Marshal.PtrToStructure(gchandle.AddrOfPinnedObject(), typeof(T));
                gchandle.Free();
                return result;
            }
            catch
            {
            }

            return default;
        }
    }
}