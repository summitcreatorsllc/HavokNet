using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;

namespace HavokNet.OSI
{
    public class Layer2Packet
    {
        public Layer2Packet()
        {
            NextLayers = new List<ILayer>();
        }

        public Common.MacAddress DestinationMac { get; set; }
        public Common.MacAddress SourceMac { get; set; }

        public List<ILayer> NextLayers { get; set; }
    }
}
