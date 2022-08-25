using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Oxide.Ext.NoSteam.Utils
{
    internal static class MD5Hash
    {
        public static string Calculate(string input)
        {
            string result;
            using (MD5 hash = MD5.Create())
            {
                result = String.Join
                (
                    "",
                    from ba in hash.ComputeHash
                    (
                        Encoding.UTF8.GetBytes(input)
                    )
                    select ba.ToString("x2")
                );
            }

            return result;
        }
    }
}
