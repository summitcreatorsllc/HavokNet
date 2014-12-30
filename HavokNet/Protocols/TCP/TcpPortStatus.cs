using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.TCP
{
    public enum TcpPortStatus
    {
        LISTENING,
        ESTABLISHED,
        CLOSE_WAIT,
        TIME_WAIT,
    }
}
