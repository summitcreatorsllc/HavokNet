using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;

namespace HavokNet.Protocols.TCP
{
    public class TcpPacket
    {
        public TcpPacket()
        {
            Layers = new List<ILayer>();
        }

        public List<ILayer> Layers { get; set; }
        public string Destination { get; set; }
        public ushort DestPort { get; set; }
    }
}
