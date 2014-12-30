using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.ICMP
{
    public class PingSettings
    {
        public byte Ttl { get; set; }
        public ushort Bytes { get; set; }
        public int Repeat { get; set; }
        public int Timeout { get; set; }
        public int Delay { get; set; }

        public static PingSettings FromConfig(byte ttl, ushort bytes, int timeout, int repeat, int delay)
        {
            return new PingSettings()
            {
                Ttl = ttl,
                Bytes = bytes,
                Timeout = timeout,
                Repeat = repeat,
                Delay = delay,
            };
        }
    }
}
