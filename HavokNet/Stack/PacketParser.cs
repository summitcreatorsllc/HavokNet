using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Icmp;
using PcapDotNet.Packets.Arp;

namespace HavokNet.Stack
{
    public static class PacketParser
    {
        public static NetClient Client { get; set; }

        public static PacketType Parse(Packet pack)
        {
            var eth = pack.Ethernet;

            byte[] typeData = new byte[2] { eth[12], eth[13] };
            ushort ethType = BitConverter.ToUInt16(typeData, 0);
            if (eth.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.Arp)
            {
                return PacketType.Arp;
            }
            else if (eth.EtherType == PcapDotNet.Packets.Ethernet.EthernetType.IpV4)
            {
                var ipv4 = eth.IpV4;
                if (ipv4.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.Tcp)
                {
                    var tcp = ipv4.Tcp;

                    // Check HTTP
                    if (tcp.Http.Body != null)
                    {
                        return PacketType.Http;
                    }

                    return PacketType.Tcp;
                }
                if (ipv4.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.Udp)
                {
                    var udp = ipv4.Udp;
                    if (udp.DestinationPort == 53 || udp.SourcePort == 53)
                    {
                        try
                        {
                            int x = udp.Dns.Answers.Count;
                            return PacketType.Dns;
                        }
                        catch { }
                    }
                    if (udp.SourcePort == 67 || udp.SourcePort == 68)
                    {
                        return PacketType.Dhcp;
                    }
                }
                if (ipv4.Protocol == PcapDotNet.Packets.IpV4.IpV4Protocol.InternetControlMessageProtocol)
                {
                    return PacketType.Icmp;
                }
            }
            return PacketType.Dhcp;
        }

        private static bool IsValidIpPacket(Packet pack)
        {
            if (pack.Ethernet.IpV4 == null) return false;
            bool m1 = Common.IPv4Address.IsInSameSubnet(new Common.IPv4Address(pack.Ethernet.IpV4.Source.ToString()), Client.Configuration.IpAddress, Client.Configuration.SubnetMask);
            bool m2 = Common.IPv4Address.IsInSameSubnet(new Common.IPv4Address(pack.Ethernet.IpV4.CurrentDestination.ToString()), Client.Configuration.IpAddress, Client.Configuration.SubnetMask);

            return (m1 || m2);
        }
    }
}
