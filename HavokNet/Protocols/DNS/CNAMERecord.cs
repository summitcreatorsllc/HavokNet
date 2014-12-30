using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.DNS
{
    public class CNAMERecord : DnsEntry
    {
        public CNAMERecord(string domain, int ttl, string aliasTarget) : base(domain, ttl, DnsRecordType.CNAME)
        {
            AliasTarget = aliasTarget;
        }

        public string AliasTarget { get; set; }
    }
}
