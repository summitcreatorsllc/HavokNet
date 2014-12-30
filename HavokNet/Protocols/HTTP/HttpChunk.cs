using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace HavokNet.Protocols.HTTP
{
    public class HttpChunk
    {
        public HttpChunk(byte[] data)
        {
            Payload = data;
        }

        public byte[] Payload { get; set; }
    }
}
