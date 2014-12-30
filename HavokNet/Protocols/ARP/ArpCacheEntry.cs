using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Protocols.ARP
{
    public class ArpCacheEntry
    {
        public Common.MacAddress Mac { get; set; }
        public Common.IPv4Address Ip { get; set; }
        public ArpEntryType Type { get; set; }
        public DateTime Expiration { get; set; }
        public string ExpirationString
        {
            get
            {
                if (Type == ArpEntryType.Static || Expiration == DateTime.MaxValue) return "No Expiration";
                else
                {
                    if (Expiration.Date == DateTime.Now.Date)
                    {
                        return Expiration.ToLongTimeString();
                    }
                    else
                    {
                        return Expiration.ToString();
                    }
                }
            }
        }
        public bool IsExpired
        {
            get
            {
                return (Type == ArpEntryType.Dynamic && DateTime.Now > Expiration);

            }
        }
    }
}
