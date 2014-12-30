using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.OSI
{
    public class Layer4UdpPacket : Layer4Packet
    {
        public Layer4UdpPacket()
        {
            Protocol = TransportProtocol.UDP;
        }
    }
}
