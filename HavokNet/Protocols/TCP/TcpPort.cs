using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.TCP
{
    public class TcpPort
    {
        public TcpPort(int port)
        {
            PortNumber = port;
        }

        public TcpPortStatus Status { get; set; }
        public int PortNumber { get; set; }
        public List<Common.RemoteEndpoint> Connection { get; set; }
    }
}
