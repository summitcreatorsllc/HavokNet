using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Firewall
{
    public abstract class FirewallBase
    {
        public FirewallBase()
        {
            PassedRx = new ConcurrentDictionary<Stack.PacketType, int>();
            PassedTx = new ConcurrentDictionary<Stack.PacketType, int>();
            DroppedRx = new ConcurrentDictionary<Stack.PacketType, int>();
            DroppedTx = new ConcurrentDictionary<Stack.PacketType, int>();
        }

        public abstract bool TestIncoming(Common.PacketData pdata);
        public abstract bool TestOutgoing(Common.PacketData pdata);

        public bool DefaultResultIncoming { get; set; }
        public bool DefaultResultOutgoing { get; set; }

        public ConcurrentDictionary<Stack.PacketType, int> PassedRx { get; set; }
        public ConcurrentDictionary<Stack.PacketType, int> PassedTx { get; set; }
        public ConcurrentDictionary<Stack.PacketType, int> DroppedRx { get; set; }
        public ConcurrentDictionary<Stack.PacketType, int> DroppedTx { get; set; }

        public int PassedRxPacketCount {get; set;}
        public int DroppedRxPacketCount { get; set; }
        public int PassedTxPacketCount { get; set; }
        public int DroppedTxPacketCount { get; set; }

        public int TotalDroppedPackets
        {
            get
            {
                return DroppedRxPacketCount + DroppedTxPacketCount;
            }
        }
    }
}
