using ConsoleApp1.Helper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var steamTicket = new SteamTicket(File.ReadAllBytes(@"D:\Games\Rust\Server\rustds\steamId.txt"));

            steamTicket.GetClientVersion();

            Console.ReadLine();
        }

        public static class DD
        {
            internal static List<int> waitingList = new List<int>()
            {
                0,
                1,
                2,
                3
            };
        }
    }
}