using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.OSI
{
    public class Layer4TcpPacket : Layer4Packet
    {
        public Layer4TcpPacket()
        {
            Protocol = TransportProtocol.TCP;
        }

        public bool SYN { get; set; }
        public bool ACK { get; set; }
        public bool FIN { get; set; }
        public bool PSH { get; set; }
        public bool RST { get; set; }
        public uint SequenceNumber { get; set; }
        public uint AcknowledgementNumber { get; set; }
    }
}
