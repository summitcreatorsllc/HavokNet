using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Common
{
    public abstract class IPAddress
    {
        public abstract string AsString { get; }
        public abstract byte[] AsBytes  { get; }

    }
}
