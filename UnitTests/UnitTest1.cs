using System.Reflection;

namespace UnitTests
{
    public class UnitTest1
    {

        [Fact]
        public void Test1()
        {
            var assembly = Assembly.UnsafeLoadFrom(@"D:\Sources\C#\Oxide.NoSteam\Source\Oxide.Ext.NoSteam-Linux\bin\Release\Facepunch.Steamworks.Win64.dll");

        }
    }
}