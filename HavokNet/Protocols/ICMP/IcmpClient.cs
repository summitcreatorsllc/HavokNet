using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;
using PcapDotNet.Packets.IpV4;
using PcapDotNet.Packets.Icmp;

namespace HavokNet.Protocols.ICMP
{
    public class IcmpClient : OSI.Layer3Protocol
    {
        public static Random _random = new Random();

        public IcmpClient(Stack.NetClient client) : base(client)
        {
            CurrentPings = new Dictionary<ushort, PingRequest>();
        }

        public Dictionary<ushort, PingRequest> CurrentPings { get; set; }

        public override void OnReceivePacket(Common.PacketData pdata)
        {
            var packet = pdata.Packet;
            var fromIP = new Common.IPv4Address(packet.Ethernet.IpV4.Source.ToString());
            var toIP = new Common.IPv4Address(packet.Ethernet.IpV4.Destination.ToString());

            var ipv4 = packet.Ethernet.IpV4;
            var icmp = ipv4.Icmp;
            // Is this an echo request to our IP?
            if (icmp.MessageType == PcapDotNet.Packets.Icmp.IcmpMessageType.Echo && toIP.AsString == _client.Configuration.IpAddress.AsString)
            {
                SendIcmpReplyPacket(packet);
            }
            else if (icmp.MessageType == IcmpMessageType.EchoReply && CurrentPings.ContainsKey((packet.Ethernet.IpV4.Icmp as IcmpEchoReplyDatagram).Identifier))
            {
                var reply = packet.Ethernet.IpV4.Icmp as IcmpEchoReplyDatagram;
                var request = CurrentPings[reply.Identifier];
                PingResult result = new PingResult(packet, pdata.TimeStamp, request);
                result.Bytes = request.Bytes;
                request.Callback(result);
            }
            else if (icmp.MessageType == IcmpMessageType.TimeExceeded)
            {
                var reply = packet.Ethernet.IpV4.Icmp as IcmpTimeExceededDatagram;
                var replyEcho = reply.IpV4.Icmp as IcmpEchoDatagram;
                var request = CurrentPings[replyEcho.Identifier];
                PingResult result = new PingResult(PingResultType.TtlExpired);
                result.RespondingHost = fromIP;
                result.Response = (int)(DateTime.Now - request.TimeStamp).TotalMilliseconds;
                request.Callback(result);
            }
            else if (icmp.MessageType == IcmpMessageType.DestinationUnreachable)
            {
                var reply = packet.Ethernet.IpV4.Icmp as IcmpDestinationUnreachableDatagram;
                var replyEcho = reply.IpV4.Icmp as IcmpDestinationUnreachableDatagram;
                //var request = CurrentPings[replyEcho.Identifier];
                //PingResult result = new PingResult(PingResultType.DestinationHostUnreachable);
                //result.RespondingHost = fromIP;
                //result.Response = (int)(DateTime.Now - request.TimeStamp).TotalMilliseconds;
                //request.Callback(result);
            }
        }

        private void SendIcmpReplyPacket(Packet request)
        {
            var icmp = request.Ethernet.IpV4.Icmp as IcmpEchoDatagram;
            OSI.Layer3Packet packet = new OSI.Layer3Packet();
            packet.Destination = request.Ethernet.IpV4.Source.ToString();
            packet.SourceIP = _client.Configuration.IpAddress;
            packet.Ttl = (byte)(request.Ethernet.IpV4.Ttl / 2);
            var data = icmp.ToArray();
            data = data.Subsegment(8, data.Length - 8).ToArray();

            PcapDotNet.Packets.Icmp.IcmpEchoReplyLayer icmpLayer =
                new PcapDotNet.Packets.Icmp.IcmpEchoReplyLayer
                {
                    Checksum = null,
                    Identifier = icmp.Identifier,
                    SequenceNumber = (ushort)(icmp.SequenceNumber),

                };
            PcapDotNet.Packets.PayloadLayer extra = new PcapDotNet.Packets.PayloadLayer() { Data = new PcapDotNet.Packets.Datagram(data) };

            packet.NextLayers.Add(icmpLayer);
            packet.NextLayers.Add(extra);

            SendPacket(packet);
        }
        private void SendIcmpRequestPacket(Common.IPv4Address ip, byte ttl, ushort bytes, int seqNum, int timeout, PingResultHandler callback)
        {
            OSI.Layer3Packet packet = new OSI.Layer3Packet();
            packet.Destination = ip.AsString;
            packet.SourceIP = _client.Configuration.IpAddress;
            packet.Ttl = ttl;

            var data = new byte[bytes];
            _random.NextBytes(data);

            var shor = new byte[2];
            _random.NextBytes(shor);
            ushort id = BitConverter.ToUInt16(shor, 0);

                        // Save ping
            if (CurrentPings.ContainsKey(id)) CurrentPings[id] = new PingRequest(callback);
            else
            {
                PingRequest req = new PingRequest(callback);
                req.Bytes = bytes;
                CurrentPings.Add(id, req);
            }

            PcapDotNet.Packets.Icmp.IcmpEchoLayer icmpLayer =
                new PcapDotNet.Packets.Icmp.IcmpEchoLayer
                {
                    Checksum = null,
                    Identifier = id,
                    SequenceNumber = (ushort)(seqNum),
                };
            PcapDotNet.Packets.PayloadLayer extra = new PcapDotNet.Packets.PayloadLayer() { Data = new PcapDotNet.Packets.Datagram(data) };

            packet.NextLayers.Add(icmpLayer);
            packet.NextLayers.Add(extra);



            SendPacket(packet, () =>
                {
                    CurrentPings[id].TimeStamp = DateTime.Now;
                });

            DateTime now = DateTime.Now;
            while ((DateTime.Now - now).TotalMilliseconds < timeout)
            {
                if (CurrentPings[id].ReplyReceived) break;
                System.Threading.Thread.Sleep(100);
            }

            if (!CurrentPings[id].ReplyReceived)
            {
                CurrentPings[id].Callback(new PingResult(PingResultType.RequestTimedOut));
            }
            CurrentPings.Remove(id);
        }
        
        public void Ping(Common.IPv4Address ip, PingResultHandler callback)
        {
            SendIcmpRequestPacket(ip, 225, 32, 123, 3, callback);
        }
        public void Ping(Common.IPv4Address ip, PingSettings settings, PingResultHandler callback)
        {
            for (int i = 0; i < (int)Math.Max(settings.Repeat,1);i++)
            {
                try
                {
                    SendIcmpRequestPacket(ip, settings.Ttl, settings.Bytes, 456, settings.Timeout, callback);
                }
                catch (ARP.IpResolutionFailed arp)
                {
                    callback(new PingResult(PingResultType.DestinationHostUnreachable) { RespondingHost = _client.Configuration.IpAddress });
                }
                System.Threading.Thread.Sleep(settings.Delay);
            }
        }
        public void Ping(string host, PingResultHandler callback)
        {
            Common.IPv4Address ip = _client.Dns.ResolveHost(host).IPs[0];
            Ping(ip, callback);
        }
        public void Ping(string host, PingSettings settings, PingResultHandler callback)
        {
            Common.IPv4Address ip = _client.Dns.ResolveHost(host).IPs[0];
            Ping(ip, settings, callback);
        }

        public delegate void PingResultHandler(PingResult result);
    }
}
