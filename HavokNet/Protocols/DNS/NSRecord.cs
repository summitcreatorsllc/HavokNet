using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.DNS
{
    public class NSRecord : DnsEntry
    {
        public NSRecord(string domain, int ttl, List<string> nameservers) : base(domain, ttl, DnsRecordType.NS)
        {
            Nameservers = nameservers;
        }
        public NSRecord(string domain, int ttl, string ns) : base(domain, ttl, DnsRecordType.NS)
        {
            Nameservers = new List<string>();
            Nameservers.Add(ns);
        }

        public List<string> Nameservers { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (string nameserver in Nameservers)
            {
                builder.AppendLine(Domain + "\t" + nameserver + "\t" + Ttl.ToString());
            }
            return builder.ToString();
        }
    }
}
