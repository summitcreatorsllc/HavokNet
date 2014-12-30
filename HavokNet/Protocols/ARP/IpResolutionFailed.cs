using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using HavokNet.Common;

namespace HavokNet.Protocols.ARP
{
    public class IpResolutionFailed : ApplicationException, ISerializable
    {
        public IpResolutionFailed(IPv4Address ip) : base("ARP resolution of the IP \"" + ip + "\" to its MAC address has failed.")
        {
            IP = ip;
        }
        public IpResolutionFailed(string message, IPv4Address ip) : base(message)
        {
            IP = ip;
        }

        public IPv4Address IP { get; set; }
    }
}
