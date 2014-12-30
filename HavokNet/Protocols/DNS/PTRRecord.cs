using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HavokNet.Common;

namespace HavokNet.Protocols.DNS
{
    public class PTRRecord : DnsEntry
    {
        public PTRRecord(string domain, int ttl, IPv4Address ip) : base(domain, ttl, DnsRecordType.PTR)
        {
            IP = ip;
        }

        public IPv4Address IP { get; set; }

        public override string ToString()
        {
            return IP.AsString + " points to " + Domain;
        }
    }
}
