using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Harmony;
using Network;
using Rust;
using Steamworks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            /*var method = typeof(ServerMgr).GetRuntimeMethods().Single(x => x.Name == "UpdateServerInformation");*/

            /* var method2 = typeof(SteamServer).GetMethods().Single(x => x.Name == "BeginAuthSession");

             var methods = typeof(SteamServer).GetMethods().Single(x => x.Name == "get_GameTags");

             var field = typeof(Protocol).GetFields().Single(x => x.Name == "network").GetRawConstantValue();*/

            /*var field = typeof(ServerMgr).GetRuntimeFields().Where(x => x.Name == "_AssemblyHash").First();

            ServerMgr serverMgr = new ServerMgr();

            field.SetValue(serverMgr, "test228");*/
            //Console.WriteLine(method.Name);
            PatchAll();
            Output();
            Console.ReadLine();
        }

        static void PatchAll()
        {
            var harmony = HarmonyInstance.Create("com.github.harmony.rimworld.mod.example");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        

        
        [HarmonyPatch(typeof(Program))]
        [HarmonyPatch("Output")]
        class Patch
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                Console.WriteLine("test2");
                return false;
                //Log.Warning("Window: " + window);
            }
        }

        public static void Output()
        {
            Console.WriteLine("test");
        }
    }
}
