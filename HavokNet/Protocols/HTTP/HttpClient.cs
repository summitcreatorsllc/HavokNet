using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Packets.Http;
using HavokNet.Stack;
using HavokNet.Protocols.TCP;
using System.IO;

namespace HavokNet.Protocols.HTTP
{
    public class HttpClient
    {
        public HttpClient(Stack.NetClient client)
        {
            Client = client;
        }

        private Stack.NetClient Client { get; set; }

        public HttpResponse Get(string url, ushort port)
        {
            // Get TCP session
            if (url.StartsWith("http"))
            {
                url = url.Split(':')[1].Remove(0, 2);
            }
            if (url.Split('/').Length == 1) url = url + "/";


            bool keepWaiting = true;
            
            long length = 0;
            long read = 0;
            HttpResponseDatagram response = null;
            ConcurrentDictionary<uint, byte[]> datagrams = new ConcurrentDictionary<uint, byte[]>();
            TCP.TcpSession session = Client.Tcp.Connect(url.Split('/')[0], port);
            session.PacketReceived += (packet) =>
                {
                    if (packet.Type == PacketType.Http && false)
                    {
                        response = packet.Packet.Ethernet.IpV4.Tcp.Http as HttpResponseDatagram;
                        keepWaiting = false;
                        return;
                    }

                    var tcp = packet.Packet.Ethernet.IpV4.Tcp;
                    if (tcp == null) return;
                    if (datagrams.ContainsKey(tcp.SequenceNumber)) return;
                    if (tcp.Payload.ToArray() == null) return;
                    datagrams.AddOrUpdate(tcp.SequenceNumber, tcp.Payload.ToArray(), (a,b) => b);
                    string str = Encoding.UTF8.GetString(tcp.Payload.ToArray());
                    if (str.Contains("Content-Length: "))
                    {
                        using (MemoryStream ms = new MemoryStream(tcp.Payload.ToArray()))
                        using (StreamReader reader = new StreamReader(ms))
                        {
                            int pos = 0;
                            while (!reader.EndOfStream)
                            {
                                string line = reader.ReadLine();
                                pos +=  line.Length + 2;
                                if (line == "")
                                {
                                    //read -= pos;
                                    break;
                                }
                            }
                        }
                        length = long.Parse(System.Text.RegularExpressions.Regex.Split(str, "Content-Length: ")[1].Split('\r')[0]);
                    }
                    read += tcp.Payload.ToArray().Length;

                    if (read >= length && length != 0)
                    {
                        keepWaiting = false;
                    }
                };
            session.CloseConnection += () =>
                {
                    keepWaiting = false;
                };

            TcpPacket tcpPacket = new TcpPacket();
            
            using (var ms = new MemoryStream())
            using (var writer = new StreamWriter(ms))
            {
                writer.WriteLine("GET /" + url.Split('/').Last() + " HTTP/1.0");
                writer.WriteLine("Host: " + url.Split('/')[0]);
                writer.WriteLine("Connection: close");
                writer.WriteLine();
                writer.Flush();
                tcpPacket.Layers.Add(new PayloadLayer() { Data = new Datagram(ms.ToArray()) });
            }



            tcpPacket.Destination = url.Split('/')[0];
            tcpPacket.DestPort = port;

            session.SendPacket(tcpPacket);



            while (keepWaiting) ;

            session.Disconnect();


            if (response == null)
            {
                byte[] data = new byte[0];

                foreach (KeyValuePair<uint, byte[]> dg in (from a in datagrams orderby a.Key ascending select a))
                {
                    data = data.Concat(dg.Value).ToArray();
                }
                return new HttpResponse(data);
            }
            else
            {
                return new HttpResponse(response.ToArray());
            }
        }
        public HttpResponse Get(string url)
        {
            return Get(url, 80);
        }
    }
}
