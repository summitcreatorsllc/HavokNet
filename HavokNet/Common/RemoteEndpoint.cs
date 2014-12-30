using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Common
{
    public class RemoteEndpoint
    {
        public IPAddress IP { get; set; }
        public ushort Port { get; set; }

        public override string ToString()
        {
            return IP.AsString + ":" + Port.ToString();
        }
    }
}
