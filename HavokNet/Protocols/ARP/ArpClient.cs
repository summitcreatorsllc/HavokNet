using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Base;
using PcapDotNet.Packets.Arp;

namespace HavokNet.Protocols.ARP
{
    public class ArpClient : OSI.Layer2Protocol
    {
        public const int TIMEOUT = 3;

        public ArpClient(Stack.NetClient client) : base(client)
        {
            ArpCache = new ConcurrentDictionary<string, ArpCacheEntry>();
            ExpirationTime = TimeSpan.FromHours(1);

            // Setup default settings
            AcceptGratuitousReplies = true;
            UpdateUnexpiredEntries = true;
            ExtendExpirationTime = true;
            DisableCache = false;
        }

        

        public ConcurrentDictionary<string, ArpCacheEntry> ArpCache { get; set; }

        #region "Packet Senders/Receives"

        public void SendArpResolutionPacket(Common.IPv4Address ip)
        {
            PcapDotNet.Packets.Arp.ArpLayer arpLayer =
                new PcapDotNet.Packets.Arp.ArpLayer
                {
                    ProtocolType = PcapDotNet.Packets.Ethernet.EthernetType.IpV4,
                    Operation = ArpOperation.Request,
                    SenderHardwareAddress = _client.Configuration.MacAddress.AsBytes.AsReadOnly(),
                    SenderProtocolAddress = _client.Configuration.IpAddress.AsBytes.AsReadOnly(),
                    TargetHardwareAddress = Common.MacAddress.Broadcast.AsBytes.AsReadOnly(),
                    TargetProtocolAddress = ip.AsBytes.AsReadOnly(),
                };

            OSI.Layer2Packet packet = new OSI.Layer2Packet();
            packet.DestinationMac = Common.MacAddress.Broadcast;
            packet.SourceMac = _client.Configuration.MacAddress;
            packet.NextLayers.Add(arpLayer);
            SendPacket(packet);
        }
        private void SendArpReplyPacket(Common.IPv4Address ip)
        {
            PcapDotNet.Packets.Arp.ArpLayer arpLayer =
                new PcapDotNet.Packets.Arp.ArpLayer
                {
                    ProtocolType = PcapDotNet.Packets.Ethernet.EthernetType.IpV4,
                    Operation = ArpOperation.Reply,
                    SenderHardwareAddress = _client.Configuration.MacAddress.AsBytes.AsReadOnly(),
                    SenderProtocolAddress = _client.Configuration.IpAddress.AsBytes.AsReadOnly(),
                    TargetHardwareAddress = ArpCache[ip.AsString].Mac.AsBytes.AsReadOnly(),
                    TargetProtocolAddress = ip.AsBytes.AsReadOnly(),
                };

            OSI.Layer2Packet packet = new OSI.Layer2Packet();
            packet.DestinationMac = ArpCache[ip.AsString].Mac;
            packet.SourceMac = _client.Configuration.MacAddress;
            packet.NextLayers.Add(arpLayer);
            SendPacket(packet);
        }
        public void SendGratuitousArpReply()
        {
            PcapDotNet.Packets.Arp.ArpLayer arpLayer =
                new PcapDotNet.Packets.Arp.ArpLayer
                {
                    ProtocolType = PcapDotNet.Packets.Ethernet.EthernetType.IpV4,
                    Operation = ArpOperation.Reply,
                    SenderHardwareAddress = _client.Configuration.MacAddress.AsBytes.AsReadOnly(),
                    SenderProtocolAddress = _client.Configuration.IpAddress.AsBytes.AsReadOnly(),
                    TargetHardwareAddress = Common.MacAddress.Broadcast.AsBytes.AsReadOnly(),
                    TargetProtocolAddress = _client.Configuration.IpAddress.AsBytes.AsReadOnly(),
                };

            OSI.Layer2Packet packet = new OSI.Layer2Packet();
            packet.DestinationMac = Common.MacAddress.Broadcast;
            packet.SourceMac = _client.Configuration.MacAddress;
            packet.NextLayers.Add(arpLayer);
            SendPacket(packet);
        }
        public void SendGratuitousArpRequest()
        {
            PcapDotNet.Packets.Arp.ArpLayer arpLayer =
                new PcapDotNet.Packets.Arp.ArpLayer
                {
                    ProtocolType = PcapDotNet.Packets.Ethernet.EthernetType.IpV4,
                    Operation = ArpOperation.Request,
                    SenderHardwareAddress = _client.Configuration.MacAddress.AsBytes.AsReadOnly(),
                    SenderProtocolAddress = _client.Configuration.IpAddress.AsBytes.AsReadOnly(),
                    TargetHardwareAddress = Common.MacAddress.Broadcast.AsBytes.AsReadOnly(),
                    TargetProtocolAddress = _client.Configuration.IpAddress.AsBytes.AsReadOnly(),
                };

            OSI.Layer2Packet packet = new OSI.Layer2Packet();
            packet.DestinationMac = Common.MacAddress.Broadcast;
            packet.SourceMac = _client.Configuration.MacAddress;
            packet.NextLayers.Add(arpLayer);
            SendPacket(packet);
        }

        public override void OnReceivePacket(Common.PacketData pdata)
        {
            var packet = pdata.Packet;
            var arp = packet.Ethernet.Arp;
            var senderMac = new Common.MacAddress(arp.SenderHardwareAddress.ToArray());
            var destMac = new Common.MacAddress(arp.TargetHardwareAddress.ToArray());
            var senderIp = new Common.IPv4Address(arp.SenderProtocolAddress.ToArray());
            var destIp = new Common.IPv4Address(arp.TargetProtocolAddress.ToArray());

            // We can cache their MAC and IP
                if (AcceptGratuitousReplies || destMac.AsString != Common.MacAddress.Broadcast.AsString)
                {
                    ArpCache.AddOrUpdate(senderIp.AsString, new ArpCacheEntry()
                        {
                            Mac = senderMac,
                            Ip = senderIp,
                            Type = ArpEntryType.Dynamic,
                            Expiration = GetNextExpirationTime(),
                        }, (str, myarp) => myarp);

                    if (arp.Operation == ArpOperation.Reply)
                    {
                        TryFireArpReplyReceied(senderIp, senderMac);
                    }

                    // Fire the Cache Changed event
                    TryFireArpCacheChanged();
                }

            // If it is a request for our IP, respond
            if (arp.Operation == ArpOperation.Request && destIp.AsString == _client.Configuration.IpAddress.AsString)
            {
                // Send Reply Packet
                SendArpReplyPacket(senderIp);
            }
        }

        #endregion

        #region "User Functions"

        public Common.MacAddress ResolveIP(Common.IPv4Address ip)
        {
            // Do we already know?
            if (ArpCache.ContainsKey(ip.AsString) && (ArpCache[ip.AsString].Expiration > DateTime.Now || ArpCache[ip.AsString].Type == ArpEntryType.Static))
            {
                Common.MacAddress mac = ArpCache[ip.AsString].Mac;
                if (DisableCache)
                {
                    ArpCache.Clear();
                }
                return mac;
            }

            // Is this the broadcast IP for our network?
            if (ip.AsString == _client.Configuration.BroadcastAddress.AsString) return Common.MacAddress.Broadcast;

            // Is this me?
            if (ip.AsString == _client.Configuration.IpAddress.AsString) return _client.Configuration.MacAddress;

            // Is this IP in our subnet?
            if (!Common.IPv4Address.IsInSameSubnet(ip, _client.Configuration.IpAddress, _client.Configuration.SubnetMask))
            {
                return ResolveIP(_client.Configuration.DefaultGateway);
            }

            // We don't know. Send packet and wait for response.
            SendArpResolutionPacket(ip);

            DateTime now = DateTime.Now;
            while ((DateTime.Now-now).TotalSeconds < TIMEOUT)
            {
                if (ArpCache.ContainsKey(ip.AsString) && (ArpCache[ip.AsString].Expiration > DateTime.Now || ArpCache[ip.AsString].Type == ArpEntryType.Static))
                {
                    Common.MacAddress mac = ArpCache[ip.AsString].Mac;
                    if (DisableCache)
                    {
                        ArpCache.Clear();
                    }
                }
                System.Threading.Thread.Sleep(100);
            }
            if (ArpCache.ContainsKey(ip.AsString)) return ArpCache[ip.AsString].Mac;
            throw new IpResolutionFailed(ip);
        }
        public void AddStaticEntry(Common.IPv4Address ip, Common.MacAddress mac)
        {
            ArpCacheEntry entry = new ArpCacheEntry()
            {
                Ip = ip,
                Mac = mac,
                Expiration = DateTime.Now,
                Type = ArpEntryType.Static,
            };
            if (ArpCache.ContainsKey(ip.AsString))
            {
                ArpCache[ip.AsString] = entry;
            }
            else
            {
                ArpCache.AddOrUpdate(ip.AsString, entry, null);
            }
            TryFireArpCacheChanged();
        }

        #endregion

        #region "Settings"

        private DateTime GetNextExpirationTime()
        {
            return DateTime.Now + ExpirationTime;
        }
        public TimeSpan ExpirationTime { get; set; }

        private bool disableCache;
        public bool DisableCache
        {
            get
            {
                return disableCache;
            }
            set
            {
                if (value)
                {
                    ArpCache.Clear();
                    TryFireArpCacheChanged();
                }
                disableCache = value;
            }
        }
        public bool AcceptGratuitousReplies { get; set; }
        public bool UpdateUnexpiredEntries { get; set; }
        public bool ExtendExpirationTime { get; set; }

        #endregion

        #region "Event and Delegate Stuff"

        public delegate void ArpReceivedHandler(Common.IPv4Address ip, Common.MacAddress mac);
        public event ArpReceivedHandler ArpReplyReceived;
        private void TryFireArpReplyReceied(Common.IPv4Address ip, Common.MacAddress mac)
        {
            try
            {
                ArpReplyReceived(ip, mac);
            }
            catch
            {

            }
        }

        public delegate void CacheModifiedHandler(List<ArpCacheEntry> cache);
        public event CacheModifiedHandler ArpCacheChanged;
        private void TryFireArpCacheChanged()
        {
            if (DisableCache) return;
            try
            {
                ArpCacheChanged(new List<ArpCacheEntry>(from a in ArpCache.Values where !a.IsExpired orderby a.Ip.AsString select a));
            }
            catch { }
        }

        #endregion
    }
}
