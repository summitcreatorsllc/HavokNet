using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Common
{
    public class IPv6Address : IPAddress
    {
        public override byte[] AsBytes
        {
            get { throw new NotImplementedException(); }
        }

        public override string AsString
        {
            get { throw new NotImplementedException(); }
        }
    }
}
