using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;

namespace HavokNet.OSI
{
    public class Layer3Packet
    {
        public Layer3Packet()
        {
            Ttl = 255;
            NextLayers = new List<ILayer>();
        }

        public Common.IPv4Address SourceIP { get; set; }
        public string Destination { get; set; }
        public byte Ttl { get; set; }
        public List<ILayer> NextLayers { get; set; }
    }
}
