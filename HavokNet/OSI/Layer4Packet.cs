using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;

namespace HavokNet.OSI
{
    public abstract class Layer4Packet
    {
        public Layer4Packet()
        {
            NextLayers = new List<ILayer>();
        }

        public string Destination { get; set; }
        public ushort LocalPort { get; set; }
        public ushort RemotePort { get; set; }

        public TransportProtocol Protocol { get; protected set; }
        public List<ILayer> NextLayers { get; set; }
    }

    public enum TransportProtocol
    {
        TCP,
        UDP,
    }
}
