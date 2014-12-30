using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.ICMP
{
    public enum PingResultType
    {
        Reply,
        TtlExpired,
        DestinationHostUnreachable,
        RequestTimedOut,
    }
}
