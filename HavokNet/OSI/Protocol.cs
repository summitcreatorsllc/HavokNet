using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;

namespace HavokNet.OSI
{
    public abstract class Protocol
    {
        public Protocol(Stack.NetClient client)
        {
            _client = client;
        }

        protected Stack.NetClient _client;
        public abstract void OnReceivePacket(Common.PacketData packet);
    }
}
