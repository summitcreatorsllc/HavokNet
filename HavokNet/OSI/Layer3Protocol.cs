using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.OSI
{
    public abstract class Layer3Protocol : Protocol
    {
        public Layer3Protocol(Stack.NetClient client) : base(client) { }

        public void SendPacket(Layer3Packet packet, Stack.NetClient.PacketSentHandler callback)
        {
            _client.SendLayer3Packet(packet, callback);
        }
        public void SendPacket(Layer3Packet packet)
        {
            _client.SendLayer3Packet(packet, (() => { }));
        }

        public abstract override void OnReceivePacket(Common.PacketData pdata);
    }
}
