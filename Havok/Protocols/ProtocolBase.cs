using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havok.Protocols
{
    public abstract class ProtocolBase : System.Windows.Controls.Page
    {
        public ProtocolBase(HavokNet.Stack.NetClient client)
        {
            Client = client;
        }

        public HavokNet.Stack.NetClient Client { get; set; }
    }
}
