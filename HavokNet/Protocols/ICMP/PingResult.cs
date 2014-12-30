using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets;
using PcapDotNet.Packets.Icmp;

namespace HavokNet.Protocols.ICMP
{
    public class PingResult
    {
        public PingResult(Packet packet, DateTime ts, PingRequest req)
        {
            Result = PingResultType.Reply;
            var reply = packet.Ethernet.IpV4.Icmp as IcmpEchoReplyDatagram;
            Ttl = packet.Ethernet.IpV4.Ttl;
            RespondingHost = new Common.IPv4Address(packet.Ethernet.IpV4.Source.ToString());
            Response = (int)(ts - req.TimeStamp).TotalMilliseconds;
        }
        public PingResult(PingResultType type)
        {
            Result = type;
        }

        public Common.IPv4Address RespondingHost { get; set; }
        public byte Ttl { get; set; }
        public ushort Bytes { get; set; }
        public int Response { get; set; }
        public PingResultType Result { get; set; }

        public override string ToString()
        {
            switch (Result)
            {
                case PingResultType.Reply:
                    return "Reply from " + RespondingHost + ":\tbytes=" + Bytes + "\ttime=" + Response + "ms\tTTL=" + Ttl;
                case PingResultType.TtlExpired:
                    return "TTL Exceeded: " + RespondingHost + ":\ttime=" + Response + "ms\tTTL=" + Ttl;
                case PingResultType.DestinationHostUnreachable:
                    return RespondingHost + ": Destination Host Unreachable";
                case PingResultType.RequestTimedOut:
                    return "Request Timed Out...";
            }
            return "Unknown Response. How Did You See This?";
        }
    }
}
