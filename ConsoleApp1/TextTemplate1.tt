               private static void Connect(Connection connection)
        {
            /*if (ConnectionAuth.m_AuthConnection.Contains(connection) == false)
                ConnectionAuth.m_AuthConnection.Add(connection);

            connectionAuth.Approve(connection);*/
            Steamworks.SteamServer.UpdatePlayer(connection.userid, connection.username, 0);
        }
        

        private static List<ulong> SteamIdsLeaved = new List<ulong>();

        /*  [HarmonyPatch(typeof(ConnectionAuth))]
          [HarmonyPatch("Reject")]
          private class ConnectionAuthPatch2
          {
              [HarmonyPrefix]
              private static bool Prefix(Connection connection, string strReason)
              {
                  if (strReason.Contains("Steam Auth Failed"))
                  {
                      connection.authStatus = "ok";
                      Connect(connection);
                      return false;
                  }
                  else
                  {
                      if (SteamIdsLeaved.Contains(connection.userid) == false)
                          SteamIdsLeaved.Add(connection.userid);

                      return true;
                  }
              }
          }*/

       
       
       
       
       
       
       
       
       
       
       
       private static bool outputed = false;

        private static List<int> Packets = new List<int>();

        /*
        //SteamNetworking
     /*   [HarmonyPatch(typeof(Server), "Send", new Type[] { typeof(SendInfo), typeof(MemoryStream), typeof(Connection) })]
        private static class ServerPatch2
        {
            [HarmonyPrefix]
            private static bool Prefix(Connection connection, MemoryStream stream)
            {
                /*if (outputed == false)
                {
                    Console.WriteLine("ServerPatch2");
                    outputed = true;
                }*/
        /*
                string error = string.Empty;
                try
                {
                    error = "SteamIdsLeaved.Contains";
                    if (connection != null && SteamIdsLeaved.Contains(connection.userid))
                    {
                        Interface.GetMod().LogInfo("Fixed Send: " + connection.userid);
                        return false;
                    }
                    error = "ProcessPacket(data)";
                    if (ProcessPacket(stream) == false)
                    {
                        Interface.GetMod().LogInfo("Fixed Send: " + connection.userid + " | packet: " + stream.Length);
                        return false;
                    }
                    error = "Packets.Contains((int)data.Length)";
                    if (Packets.Contains((int)stream.Length) == false)
                    {
                        Interface.GetMod().LogInfo("Server Send: " + connection.userid + " " + stream.Length);
                        Packets.Add((int)stream.Length);
                    }
                }
                catch
                {
                    Interface.GetMod().LogInfo(error);
                    return false;
                }
                return true;
            }
        }*/

        /*[HarmonyPatch(typeof(ProtocolParser), "SkipKey")]
        private static class ProtocolParserPatch3
        {
            [HarmonyPrefix]
            private static bool Prefix(Stream stream, Key key)
            {
                if (outputed == false)
                {
                    Interface.GetMod().LogInfo("ServerPatch2");
                    outputed = true;
                }
                return false;
            }
        }*/

        //SkipKey

        //[HarmonyPatch(typeof(Facepunch.Network.Raknet.Server), "Send", new Type[] { typeof(SendInfo), typeof(MemoryStream), typeof(Connection) })]
        private static class ServerPatch
        {
            [HarmonyPrefix]
            private static bool Prefix(SendInfo sendinfo, MemoryStream data, Connection connection)
            {
                string error = string.Empty;
                try
                {
                    error = "SteamIdsLeaved.Contains";
                    if (connection != null && SteamIdsLeaved.Contains(connection.userid))
                    {
                        Console.WriteLine("Fixed Send: " + connection.userid);
                        return false;
                    }
                    error = "ProcessPacket(data)";
                    if (ProcessPacket(data) == false)
                    {
                        Console.WriteLine("Fixed Send: " + connection.userid + " | packet: " + data.Length);
                        return false;
                    }
                    error = "Packets.Contains((int)data.Length)";
                    if (Packets.Contains((int)data.Length) == false)
                    {
                        Console.WriteLine("Server Send: " + connection.userid + " " + data.Length);
                        Packets.Add((int)data.Length);
                    }
                }
                catch
                {
                    Console.WriteLine(error);
                }

                return true;
            }
        }

        private static bool ProcessPacket(MemoryStream data)
        {
            using (MemoryStream memoryStream = new MemoryStream(data.ToArray()))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    byte b = binaryReader.ReadByte();
                    if (b > 140)
                    {
                        b -= 140;
                        if (b <= 23)
                        {
                            switch (b)
                            {
                                case 5:
                                    return Entities(binaryReader);
                            }
                        }
                    }
                }
            }
            return true;
        }

        private static bool Entities(BinaryReader reader)
        {
            reader.ReadUInt32();
            try
            {
                Deserialize(reader.BaseStream);
                // int num = stream.ReadByte();
                //Console.WriteLine("Decrypted ");
                return true;
            }
            catch (NotImplementedException ex)
            {
                //Interface.GetMod().
                Interface.GetMod().LogInfo("Fixed patch");
                return false;
            }
        }

        public static void Deserialize(Stream stream)
        {
            int num = stream.ReadByte();
            Key key = ProtocolParser.ReadKey((byte)num, stream);
            SkipKey(stream, key);
        }

        public static void SkipKey(Stream stream, Key key)
        {
            switch (key.WireType)
            {
                case Wire.Varint:
                    //ProtocolParser.ReadSkipVarInt(stream);
                    return;

                case Wire.Fixed64:
                    //stream.Seek(8L, SeekOrigin.Current);
                    return;

                case Wire.LengthDelimited:
                    // s//tream.Seek((long)((ulong)ProtocolParser.ReadUInt32(stream)), SeekOrigin.Current);
                    return;

                case Wire.Fixed32:
                    //stream.Seek(4L, SeekOrigin.Current);
                    return;
            }
            throw new NotImplementedException("Unknown wire type: " + key.WireType);
        }