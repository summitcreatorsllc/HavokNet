using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.TCP
{
    public class TcpClient : OSI.Layer4Protocol
    {
        public Random _rnd;

        public TcpClient(Stack.NetClient client) : base(client)
        {
            _rnd = new Random();
            Sessions = new List<TcpSession>();
            for (ushort x = 0; x < ushort.MaxValue; x++)
            {
                Sessions.Add(new TcpSession(this, client.Configuration.IpAddress, x));
            }
        }

        public List<TcpSession> Sessions { get; set; }

        public override void OnReceivePacket(Common.PacketData pdata)
        {
            var ip = pdata.Packet.Ethernet.IpV4;
            var tcp = ip.Tcp;
            var destPort = tcp.DestinationPort;
            var destIp = new Common.IPv4Address(ip.Destination.ToString());

            Sessions[destPort].AddPacketToQueue(pdata);
        }
        

        public TcpSession Connect(string destination, ushort port)
        {
            ushort destPort = port;
            ushort sPort = (ushort)_rnd.Next(0,short.MaxValue);


            var available = new List<TcpSession>(from a in Sessions where a.State == TcpState.LISTEN select a);
            TcpSession session = available[_rnd.Next(0,available.Count)];
            session.Connect(destination, port);
            return session;
            
        }

    }
}
