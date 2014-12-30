using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Firewall
{
    public class SimpleServiceRule
    {
        public Stack.PacketType Type { get; set; }
        public bool AllowIncoming { get; set; }
        public bool AllowOutgoing { get; set; }
    }
}
