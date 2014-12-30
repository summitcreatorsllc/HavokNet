using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.OSI
{
    public abstract class Layer4Protocol : Protocol
    {
        public Layer4Protocol(Stack.NetClient client) : base(client) { }

        public void SendPacket(Layer4Packet packet, Stack.NetClient.PacketSentHandler callback)
        {
            _client.SendLayer4Packet(packet, callback);
        }
        public void SendPacket(Layer4Packet packet)
        {
            _client.SendLayer4Packet(packet, (() => { }));
        }

        public abstract override void OnReceivePacket(Common.PacketData packet);
    }
}
