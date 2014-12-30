using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.DNS
{
    public class MXRecordRow
    {
        public MXRecordRow(string host, int level, int ttl)
        {
            Host = host;
            Level = level;
        }

        public string Host { get; set; }
        public int Level { get; set; }
        public int Ttl { get; set; }

        public override string ToString()
        {
            return Host + "\t\t" + Level;
        }
    }
}
