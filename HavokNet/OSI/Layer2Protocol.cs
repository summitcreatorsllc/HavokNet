using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.OSI
{
    public abstract class Layer2Protocol : Protocol
    {
        public Layer2Protocol(Stack.NetClient client) : base(client) { }

        public void SendPacket(Layer2Packet packet, Stack.NetClient.PacketSentHandler callback)
        {
            _client.SendLayer2Packet(packet, callback);
        }
        public void SendPacket(Layer2Packet packet)
        {
            _client.SendLayer2Packet(packet, (() => { }));
        }


        public abstract override void OnReceivePacket(Common.PacketData pdata);
    }
}
