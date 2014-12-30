using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Base;
using PcapDotNet.Core;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Ethernet;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Transport;
using PcapDotNet.Packets.Http;

namespace HavokNet.Stack
{
    public class NetClient
    {
        #region "Constructors"

        public NetClient(Common.NetworkCard device, OSI.IPv4StackConfiguration config)
        {
            LoadClient(device, config, new Firewall.SimpleFirewall(true, true));
        }
        public NetClient(Common.NetworkCard nic, OSI.IPv4StackConfiguration config, Firewall.FirewallBase firewall)
        {
            LoadClient(nic, config, firewall);
        }

        private void LoadClient(Common.NetworkCard device, OSI.IPv4StackConfiguration config, Firewall.FirewallBase firewall)
        {
            Device = device;
            Communicator = Device.GetCommunicator();


            Configuration = config;
            LoadProtocols();

            PacketParser.Client = this;
        }

        #endregion

        #region "Packet Control"

        public Firewall.FirewallBase NetworkFirewall { get; set; }

        private System.Threading.Thread thread;
        public void Start()
        {
            thread = new System.Threading.Thread(() => Listener());
            thread.Start();
        }

        public void Listener()
        {
            Communicator.ReceivePackets(0, PacketDispatcher);
        }
        public void PacketDispatcher(PcapDotNet.Packets.Packet packet)
        {
            Common.PacketData pdata = new Common.PacketData(packet);
            if (!NetworkFirewall.TestIncoming(pdata)) return;
            Task.Run(() =>
                {
                    switch (pdata.Type)
                    {
                        case PacketType.Arp:
                            Arp.OnReceivePacket(pdata);
                            break;
                        case PacketType.Icmp:
                            Icmp.OnReceivePacket(pdata);
                            break;
                        case PacketType.Dns:
                            Dns.OnReceivePacket(pdata);
                            break;
                        case PacketType.Tcp:
                            Tcp.OnReceivePacket(pdata);
                            break;
                        case PacketType.Http:
                            Tcp.OnReceivePacket(pdata);
                            break;
                    }
                });
        }

        public void Stop()
        {
            try
            {
                thread.Abort();
            }
            catch
            {
                thread = null;
            }
        }

        #endregion

        #region "Protocols"

        private void LoadProtocols()
        {
            Arp = new Protocols.ARP.ArpClient(this);
            Icmp = new Protocols.ICMP.IcmpClient(this);
            Dns = new Protocols.DNS.DnsClient(this);
            Tcp = new Protocols.TCP.TcpClient(this);
            Http = new Protocols.HTTP.HttpClient(this);
        }

        public Protocols.ARP.ArpClient Arp { get; set; }
        public Protocols.ICMP.IcmpClient Icmp { get; set; }
        public Protocols.DNS.DnsClient Dns { get; set; }
        public Protocols.TCP.TcpClient Tcp { get; set; }
        public Protocols.HTTP.HttpClient Http { get; set; }

        #endregion


        public OSI.IPv4StackConfiguration Configuration { get; set; }
        public Common.NetworkCard Device { get; set; }
        private PacketCommunicator Communicator { get; set; }
        
        
        public delegate void PacketSentHandler();
        public void SendLayer2Packet(OSI.Layer2Packet packet, PacketSentHandler callback)
        {
            EthernetLayer ethernetLayer =
                new EthernetLayer
                {
                    Source = new MacAddress(packet.SourceMac.AsString),
                    Destination = new MacAddress(packet.DestinationMac.AsString),
                    EtherType = EthernetType.None, // Will be filled automatically.
                };
            List<ILayer> layers = new List<ILayer>(packet.NextLayers);
            layers.Insert(0, ethernetLayer);
            Packet pack = PacketBuilder.Build(DateTime.Now, layers.ToArray());
            SendPacket(pack, callback);
        }
        public void SendLayer3Packet(OSI.Layer3Packet packet, PacketSentHandler callback)
        {
            IpV4Layer ipV4Layer = new IpV4Layer
            {
                Source = new IpV4Address(packet.SourceIP.AsString),
                Ttl = packet.Ttl,
                // The rest of the important parameters will be set for each packet
            };

            Common.IPv4Address ip = null;

            try
            {
                System.Net.IPAddress.Parse(packet.Destination);
                ip = new Common.IPv4Address(packet.Destination);
            }
            catch
            {
                ip = Dns.ResolveHost(packet.Destination).IPs[0];
            }

            ipV4Layer.CurrentDestination = new IpV4Address(ip.AsString);

            OSI.Layer2Packet l2 = new OSI.Layer2Packet();
            l2.SourceMac = Configuration.MacAddress;
            l2.DestinationMac = Arp.ResolveIP(ip);

            foreach (ILayer layer in packet.NextLayers)
            {
                l2.NextLayers.Add(layer);
            }
            l2.NextLayers.Insert(0, ipV4Layer);

            SendLayer2Packet(l2, callback);
        }
        public void SendLayer4Packet(OSI.Layer4Packet packet, PacketSentHandler callback)
        {
            switch (packet.Protocol)
            {
                case OSI.TransportProtocol.TCP:
                    SendTcpPacket(packet as OSI.Layer4TcpPacket, callback);
                    break;
                case OSI.TransportProtocol.UDP:
                    SendUdpPacket(packet as OSI.Layer4UdpPacket, callback);
                    break;
            }
        }
        private void SendTcpPacket(OSI.Layer4TcpPacket packet, PacketSentHandler callback)
        {
            TcpLayer tcpLayer =
                new TcpLayer
                {
                    SourcePort = packet.LocalPort,
                    DestinationPort = packet.RemotePort,
                    Checksum = null, // Will be filled automatically.
                    SequenceNumber = packet.SequenceNumber,
                    AcknowledgmentNumber = packet.AcknowledgementNumber,
                    Window = 64512,
                };

            if (packet.ACK) tcpLayer.ControlBits = tcpLayer.ControlBits | TcpControlBits.Acknowledgment;
            if (packet.SYN) tcpLayer.ControlBits = tcpLayer.ControlBits | TcpControlBits.Synchronize;
            if (packet.RST) tcpLayer.ControlBits = tcpLayer.ControlBits | TcpControlBits.Reset;
            if (packet.PSH) tcpLayer.ControlBits = tcpLayer.ControlBits | TcpControlBits.Push;
            if (packet.FIN) tcpLayer.ControlBits = tcpLayer.ControlBits | TcpControlBits.Fin;

            OSI.Layer3Packet l3 = new OSI.Layer3Packet();
            l3.Ttl = 255;
            l3.SourceIP = Configuration.IpAddress;
            l3.Destination = packet.Destination;

            foreach (ILayer layer in packet.NextLayers)
            {
                l3.NextLayers.Add(layer);
            }
            l3.NextLayers.Insert(0, tcpLayer);
            SendLayer3Packet(l3, callback);
        }
        private void SendUdpPacket(OSI.Layer4UdpPacket packet, PacketSentHandler callback)
        {
            UdpLayer udpLayer =
                new UdpLayer
                {
                    SourcePort = packet.LocalPort,
                    DestinationPort = packet.RemotePort,
                    Checksum = null, // Will be filled automatically.
                    CalculateChecksumValue = true,
                };

            OSI.Layer3Packet l3 = new OSI.Layer3Packet();
            l3.Ttl = 255;
            l3.SourceIP = Configuration.IpAddress;
            l3.Destination = packet.Destination;
            
            foreach (ILayer layer in packet.NextLayers)
            {
                l3.NextLayers.Add(layer);
            }
            l3.NextLayers.Insert(0, udpLayer);
            SendLayer3Packet(l3, callback);
        }
        private void SendPacket(Packet pack, PacketSentHandler callback)
        {
            if (NetworkFirewall.TestOutgoing(new Common.PacketData(pack)))
            {
                Communicator.SendPacket(pack);
                callback();
            }
        }
    }
}
