using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.DNS
{
    public abstract class DnsEntry
    {
        public DnsEntry(string domain, int ttl, DnsRecordType type)
        {
            Domain = domain;
            Type = type;
        }

        public DnsRecordType Type { get; set; }
        
        public string Domain { get; set; }
        public int Ttl { get; set; }
    }
}
