using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            ServerMgr serverMgr = new ServerMgr();
            var methods = typeof(ServerMgr).GetRuntimeMethods();
            string result = methods.Where(x => x.Name.Contains("get_AssemblyHash")).First()
                .Invoke(serverMgr, null).ToString();
            Console.WriteLine(result);
            Console.ReadLine();
        }
    }
}
