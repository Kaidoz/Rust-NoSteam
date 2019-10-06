using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Steamworks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var method = typeof(ConnectionAuth).GetMethods().Single(x => x.Name == "OnNewConnection");

            var method2 = typeof(SteamServer).GetMethods().Single(x => x.Name == "BeginAuthSession");

            var methods = typeof(ServerMgr).GetRuntimeMethods().Single(x => x.Name == "DoTick");

            Console.ReadLine();
        }
    }
}
