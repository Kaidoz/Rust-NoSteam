using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Ext.NoSteam.Helper
{
    public static class SerializationExtension
    {
        // Token: 0x06000071 RID: 113 RVA: 0x000059C0 File Offset: 0x00003BC0
        public static byte[] Serialize<T>(this T structure) where T : struct
        {
            byte[] array = new byte[Marshal.SizeOf(typeof(T))];
            IntPtr intPtr = Marshal.AllocHGlobal(array.Length);
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
                GCHandle gchandle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                T result = (T)((object)Marshal.PtrToStructure(gchandle.AddrOfPinnedObject(), typeof(T)));
                gchandle.Free();
                return result;
            }
            catch
            {
            }
            return default(T);
        }
    }
}
