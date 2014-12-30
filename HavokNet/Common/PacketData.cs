using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Common
{
    public class PacketData
    {
        public PacketData(PcapDotNet.Packets.Packet packet)
        {
            TimeStamp = DateTime.Now;
            Packet = packet;
            Type = Stack.PacketParser.Parse(Packet);
        }

        public PcapDotNet.Packets.Packet Packet { get; private set; }
        public Stack.PacketType Type { get; private set; }
        public DateTime TimeStamp { get; private set; }
    }
}
