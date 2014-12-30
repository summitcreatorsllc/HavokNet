using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.ICMP
{
    public class PingRequest
    {
        public PingRequest(IcmpClient.PingResultHandler callback)
        {
            Callback = callback;
            TimeStamp = DateTime.Now;
        }

        private IcmpClient.PingResultHandler callback;
        public IcmpClient.PingResultHandler Callback
        {
            get
            {
                return (result) =>
                    {
                        ReplyReceived = true;
                        callback(result);
                    };
            }
            set
            {
                callback = value;
            }
        }
        public bool ReplyReceived { get; set; }
        public DateTime TimeStamp { get; set; }
        public ushort Bytes { get; set; }
    }
}
