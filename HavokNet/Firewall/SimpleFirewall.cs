using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HavokNet.Stack;

namespace HavokNet.Firewall
{
    public class SimpleFirewall : FirewallBase
    {
        public SimpleFirewall(bool defaultIncoming, bool defaultOutgoing)
        {
            ServiceRules = new Dictionary<PacketType, SimpleServiceRule>();

            DefaultResultIncoming = defaultIncoming;
            DefaultResultOutgoing = defaultOutgoing;
        }

        public Dictionary<Stack.PacketType, SimpleServiceRule> ServiceRules { get; private set; }

        public override bool TestIncoming(Common.PacketData pdata)
        {
            if  (ServiceRules.ContainsKey(pdata.Type))
            {
                bool result = ServiceRules[pdata.Type].AllowIncoming;
                LogPacket(result, true, pdata.Type);
                return result;
            }
            else
            {
                LogPacket(DefaultResultIncoming, true, pdata.Type);
                return DefaultResultIncoming;
            }
        }

        public override bool TestOutgoing(Common.PacketData pdata)
        {
            if (ServiceRules.ContainsKey(pdata.Type))
            {
                bool result = ServiceRules[pdata.Type].AllowOutgoing;
                LogPacket(result, false, pdata.Type);
                return result;
            }
            else
            {
                LogPacket(DefaultResultOutgoing, false, pdata.Type);
                return DefaultResultOutgoing;
            }
        }

        public void LogPacket(bool passed, bool incoming, PacketType type)
        {
            if (incoming)
            {
                if (passed)
                {
                    PassedRx.AddOrUpdate(type, 1, (_type, val) => val + 1);
                    PassedRxPacketCount++;
                }
                else
                {
                    DroppedRx.AddOrUpdate(type, 1, (_type, val) => val + 1);
                    DroppedRxPacketCount++;
                }
            }
            else
            {
                if (passed)
                {
                    PassedTx.AddOrUpdate(type, 1, (_type, val) => val + 1);
                    PassedTxPacketCount++;
                }
                else
                {
                    DroppedTx.AddOrUpdate(type, 1, (_type, val) => val + 1);
                    DroppedTxPacketCount++;
                }
            }

        }
    }
}
