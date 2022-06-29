using Oxide.Core;
using Oxide.Core.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Oxide.Ext.NoSteam.Language
{
    internal static class MessageService
    {
        private static Lang Lang;

        private static List<Message> MessagesList;

        
        static MessageService()
        {
            InitLocals();
        }

        private static void InitLocals()
        {
            Lang = Interface.Oxide.GetLibrary<Lang>(null);
            MessagesList = new List<Message>();

            ParseMessages();

        }

        internal static string Get(ulong userId, string id)
        {
            string language = Lang.GetLanguage(userId.ToString());

            var message = MessagesList.First(x => x.Id == id);

            return message.Get(language);
        }

        internal static string GetEng(ulong userId, string id)
        {
            var message = MessagesList.First(x => x.Id == id);

            return message.Get("eng");
        }

        private static void ParseMessages()
        {
            MessagesList.Add(new Message(nameof(Messages.AdvertMessage), "Сервер использует NoSteam by Kaidoz. \n" +
                    "Discord: discord.gg/Tn3kzbE\n", 

                    "Server uses NoSteam by Kaidoz. \n" +
                    "Discord: discord.gg/Tn3kzbE\n"));
        }

        internal class Message
        {
            internal string Id { get; set; }

            internal string TextRu { get; set; }

            internal string TextEng { get; set; }

            public Message(string id, string textRu, string textEng)
            {
                Id = id;
                TextRu = textRu;
                TextEng = textEng;
            }

            internal string Get(string language)
            {
                if (language == "ru")
                    return TextRu;

                return TextEng;
            }
        }
    }
}
