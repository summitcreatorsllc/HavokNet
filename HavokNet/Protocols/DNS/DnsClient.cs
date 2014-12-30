using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Packets.Dns;

namespace HavokNet.Protocols.DNS
{
    public class DnsClient : OSI.Layer4Protocol
    {
        public const int TIMEOUT = 5;
        public static Random _random = new Random();

        public DnsClient(Stack.NetClient client) : base(client)
        {
            DnsCache = new ConcurrentDictionary<string, DnsEntry>();
            PendingRequests = new List<string>();
        }

        public ConcurrentDictionary<string, DnsEntry> DnsCache { get; private set; }
        public List<string> PendingRequests { get; private set; }

        public override void OnReceivePacket(Common.PacketData packet)
        {
            var udp = packet.Packet.Ethernet.IpV4.Udp;
            ushort destPort = udp.DestinationPort;
            var dns = udp.Dns;
            string domain = "";
            try
            {
                if (dns.Answers[0].DnsType == DnsType.Ptr)
                {
                    domain = dns.Queries[0].DomainName.ToString();
                }
                else
                {
                    domain = dns.Answers[0].DomainName.ToString();
                }
            }
            catch
            {
                return;
            }
            domain = domain.Trim('.').ToLower();
            
            string hashVal = domain + ":" + destPort.ToString();
            Dictionary<string, DnsEntry> temp = new Dictionary<string, DnsEntry>();
            if (PendingRequests.Contains(hashVal))
            {
                PendingRequests.Remove(hashVal);
                foreach (DnsDataResourceRecord record in dns.Answers)
                {
                    if (record.DnsType == DnsType.Ns)
                    {
                        string nsvalue = (record.Data as DnsResourceDataDomainName).Data.ToString().ToLower().Trim('.');
                        string domainname = record.DomainName.ToString().Trim('.');

                        if (temp.ContainsKey(domainname))
                        {
                            ((NSRecord)temp[domainname]).Nameservers.Add(nsvalue);
                        }
                        else
                        {
                            temp.Add(domainname, new NSRecord(domain, record.Ttl, nsvalue));
                        }
                    }
                    else if (record.DnsType == DnsType.Ptr)
                    {
                        string newdata = (record.Data as DnsResourceDataDomainName).Data.ToString().ToLower().Trim('.');
                        string domainname = record.DomainName.ToString().Trim('.');
                        domainname = domainname.Remove(domainname.Length - 13, 13);
                        domainname = (new Common.IPv4Address(new Common.IPv4Address(domainname).AsReverseString).AsString);
                        if (temp.ContainsKey(domainname))
                        {
                            ((PTRRecord)temp[domainname]).Domain = newdata;
                        }
                        else
                        {
                            temp.Add(domainname, new PTRRecord(newdata, record.Ttl, new Common.IPv4Address(domainname)));
                        }
                    }
                    else if (record.DnsType == DnsType.A)
                    {
                        string ipvalue = (record.Data as DnsResourceDataIpV4).Data.ToString().ToLower().Trim('.');
                        string domainname = record.DomainName.ToString().Trim('.');

                        if (temp.ContainsKey(domainname))
                        {
                            ((ARecord)temp[domainname]).IPs.Add(new Common.IPv4Address(ipvalue));
                        }
                        else
                        {
                            temp.Add(domainname, new ARecord(domain, record.Ttl, new Common.IPv4Address(ipvalue)));
                        }
                    }
                    else if (record.DnsType == DnsType.MailExchange)
                    {
                        var mx = (record.Data as DnsResourceDataMailExchange);
                        string host = mx.MailExchangeHost.ToString();
                        int level = mx.Preference;
                        string domainname = record.DomainName.ToString().Trim('.');

                        if (temp.ContainsKey(domainname))
                        {
                            ((MXRecord)temp[domainname]).MailHosts.Add(new MXRecordRow(domainname, level, record.Ttl));
                        }
                        else
                        {
                            temp.Add(domainname, new MXRecord(record.DomainName.ToString().Trim('.'), record.Ttl, new MXRecordRow(host, level, record.Ttl)));
                        }
                    }
                    else if (record.DnsType == DnsType.CName)
                    {
                        var cnam = (record.Data as DnsResourceDataDomainName).Data.ToString().Trim('.');
                        string cnamdomain = record.DomainName.ToString().Trim('.');

                        if (temp.ContainsKey(cnamdomain))
                        {
                            ((CNAMERecord)temp[cnamdomain]).AliasTarget = cnam;
                        }
                        else
                        {
                            temp.Add(cnamdomain, new CNAMERecord(cnamdomain, record.Ttl, cnam));
                        }
                    }
                }
            }


            foreach (KeyValuePair<string, DnsEntry> pair in temp)
            {
                DnsCache.AddOrUpdate(pair.Key + " " + pair.Value.Type.ToString(), pair.Value, (a, b) => b);
            }
        }

        public ARecord ResolveHost(string dns)
        {
            dns = dns.ToLower();
            string lookup = dns + " A";
            string clookup = dns + " CNAME";

            // Is this an IP?
            HavokNet.Common.IPv4Address ip = null;
            try
            {
                ip = new Common.IPv4Address(dns);
                return new ARecord(dns, 0, ip);
            }
            catch { }

            // Do we already know?
            if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as ARecord);
            if (DnsCache.ContainsKey(clookup))
            {
                return ResolveHost((DnsCache[clookup] as CNAMERecord).AliasTarget);
            }
            
            // We don't know. Send packet and wait for response.
            SendRecordRequest(dns, DnsType.A);

            DateTime now = DateTime.Now;
            while ((DateTime.Now - now).TotalSeconds < TIMEOUT)
            {
                if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as ARecord);
                if (DnsCache.ContainsKey(clookup))
                {
                    return ResolveHost((DnsCache[clookup] as CNAMERecord).AliasTarget);
                }
                System.Threading.Thread.Sleep(100);
            }

            throw new Exception("Host not found.");
        }
        public PTRRecord ReverseDnsLookup(Common.IPv4Address ip)
        {

            string lookup = ip.AsString + " PTR";
            string ipStr = ip.AsReverseString + ".in-addr.arpa".ToLower();

            // Do we already know?
            if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as PTRRecord);
            
            // We don't know. Send packet and wait for response.
            SendRecordRequest(ipStr, DnsType.Ptr);

            DateTime now = DateTime.Now;
            while ((DateTime.Now - now).TotalSeconds < TIMEOUT)
            {
                if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as PTRRecord);
                System.Threading.Thread.Sleep(100);
            }

            if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as PTRRecord);
            throw new Exception("Host not found.");
        }
        public NSRecord RetrieveNameserverList(string host)
        {
            string lookup = host + " NS";
            // Do we already know?
            if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as NSRecord);

            // We don't know. Send packet and wait for response.
            SendRecordRequest(host, DnsType.Ns);

            DateTime now = DateTime.Now;
            while ((DateTime.Now - now).TotalSeconds < TIMEOUT)
            {
                if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as NSRecord);
                System.Threading.Thread.Sleep(100);
            }

            throw new Exception("Host not found.");
        }
        public MXRecord MailLookup(string domain)
        {
            domain = domain.ToLower();

            string lookup = domain + " MX";

            // Do we already know?
            if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as MXRecord);

            // We don't know. Send packet and wait for response.
            SendRecordRequest(domain, DnsType.MailExchange);

            DateTime now = DateTime.Now;
            while ((DateTime.Now - now).TotalSeconds < TIMEOUT)
            {
                if (DnsCache.ContainsKey(lookup)) return (DnsCache[lookup] as MXRecord);
                System.Threading.Thread.Sleep(100);
            }

            throw new Exception("MX Records for host \"" + domain + "\" not found.");
        }
        private Common.IPv4Address[] SplitIps(string ips)
        {
            string[] stuff = ips.Split(',');
            Common.IPv4Address[] ip = new Common.IPv4Address[stuff.Length];
            for (int i = 0; i < stuff.Length;i++)
            {
                ip[i] = new Common.IPv4Address(stuff[i]);
            }
            return ip;
        }

        private void SendRecordRequest(string host, DnsType type)
        {
            DnsLayer dnsLayer =
                new DnsLayer
                {
                    Id = 100,
                    IsResponse = false,
                    OpCode = DnsOpCode.Query,
                    IsAuthoritativeAnswer = false,
                    IsTruncated = false,
                    IsRecursionDesired = true,
                    IsRecursionAvailable = false,
                    FutureUse = false,
                    IsAuthenticData = false,
                    IsCheckingDisabled = false,
                    ResponseCode = DnsResponseCode.NoError,
                    Queries = new[]
                                      {
                                          new DnsQueryResourceRecord(new DnsDomainName(host),
                                                                     type, DnsClass.Internet),
                                      },
                    Answers = null,
                    Authorities = null,
                    Additionals = null,
                    DomainNameCompressionMode = DnsDomainNameCompressionMode.All,
                };

            ushort port = (ushort)(4050 + _random.Next(0, 1000));
            string hashVal = host.ToLower() + ":" + port;
            PendingRequests.Add(hashVal);

            OSI.Layer4UdpPacket udp = new OSI.Layer4UdpPacket();
            udp.Destination = _client.Configuration.DnsServers[0].AsString;
            udp.LocalPort = port;
            udp.RemotePort = 53;
            udp.NextLayers.Add(dnsLayer);
            SendPacket(udp);
        }
    }
}
