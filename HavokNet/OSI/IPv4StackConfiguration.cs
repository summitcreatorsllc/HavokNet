using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HavokNet.Common;

namespace HavokNet.OSI
{
    public class IPv4StackConfiguration
    {
        public MacAddress MacAddress { get; set; }
        public IPv4Address IpAddress { get; set; }
        public IPv4Address SubnetMask { get; set; }
        public IPv4Address DefaultGateway { get; set; }
        public IPv4Address[] DnsServers { get; set; }

        ///////////////////////////
        ///  Calculated Values  ///
        ///////////////////////////

        public IPv4Address BroadcastAddress
        {
            get
            {
                return IPv4Address.GetBroadcastAddress(IpAddress, SubnetMask);
            }
        }
        public IPv4Address NetworkAddress
        {
            get
            {
                return IPv4Address.GetNetworkAddress(IpAddress, SubnetMask);
            }
        }
    }
}
