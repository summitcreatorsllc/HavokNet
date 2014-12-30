using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.DNS
{
    public class MXRecord : DnsEntry
    {
        public MXRecord(string domain, int ttl, List<MXRecordRow> hosts) : base(domain, ttl, DnsRecordType.MX)
        {
            MailHosts = hosts;
        }
        public MXRecord(string domain, int ttl, MXRecordRow host) : base(domain, ttl, DnsRecordType.MX)
        {
            MailHosts = new List<MXRecordRow>();
            MailHosts.Add(host);
        }

        public List<MXRecordRow> MailHosts { get; set; }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            foreach (MXRecordRow row in MailHosts)
            {
                builder.AppendLine("\t" + row.ToString() + "\t" + Ttl.ToString() + "s");
            }
            return builder.ToString();
        }
    }
}
