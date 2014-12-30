using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havok.Protocols
{
    public class FirewallProtocolReport
    {
        public HavokNet.Stack.PacketType Protocol { get; set; }
        public int DroppedRx { get; set; }
        public int DroppedTx { get; set; }
        public int DroppedTotal
        {
            get
            {
                return DroppedRx + DroppedTx;
            }
        }
        public int PassedRx { get; set; }
        public int PassedTx { get; set; }
        public int PassedTotal
        {
            get
            {
                return PassedRx + PassedTx;
            }
        }


        public int Total
        {
            get
            {
                return PassedTotal + DroppedTotal;
            }
        }
    }
}
