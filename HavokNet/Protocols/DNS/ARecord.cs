using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HavokNet.Common;

namespace HavokNet.Protocols.DNS
{
    public class ARecord : DnsEntry
    {
        public ARecord(string domain, int ttl,  List<IPv4Address> ips) : base(domain, ttl, DnsRecordType.A)
        {
            Domain = domain;
            IPs = ips;
        }
        public ARecord(string domain, int ttl,  IPv4Address ip) : base(domain, ttl, DnsRecordType.A)
        {
            Domain = domain;
            IPs = new List<IPv4Address>();
            IPs.Add(ip);
        }

        public List<IPv4Address> IPs { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (IPv4Address ip in IPs)
            {
                builder.AppendLine(Domain + "\t\t" + ip.AsString + "\t" + Ttl.ToString());
            }
            return builder.ToString();
        }
    }
}
