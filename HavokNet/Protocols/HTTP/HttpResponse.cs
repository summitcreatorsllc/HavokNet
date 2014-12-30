using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace HavokNet.Protocols.HTTP
{
    public class HttpResponse
    {
        public HttpResponse(byte[] data)
        {
            LoadFromBytes(data);
        }
        public HttpResponse(List<HttpChunk> chunks)
        {
            byte[] alldata = new byte[0];
            foreach (HttpChunk chunk in chunks)
            {
                alldata = alldata.Concat(chunk.Payload).ToArray();
            }
            LoadFromBytes(alldata);
        }

        private void LoadFromBytes(byte[] data)
        {
            using (var bs = new MemoryStream())
            using (var bwrite = new BinaryWriter(bs))
            using (var ms = new MemoryStream(data))
            using (var reader = new StreamReader(ms))
            {
                // First we have HTTP headers
                StringBuilder headers = new StringBuilder();
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line != "")
                    {
                        headers.AppendLine(line);
                    }
                    else
                    {
                        break;
                    }
                }
                LoadHeaders(headers.ToString());
                StringBuilder builder = new StringBuilder();
                while (!reader.EndOfStream)
                {
                    builder.AppendLine(reader.ReadLine());
                }
                Data = Encoding.UTF8.GetBytes(builder.ToString());
            }

        }

        private void LoadHeaders(string headers)
        {
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(headers)))
            using (var reader = new StreamReader(ms))
            {
                string first = reader.ReadLine();
                Code = (HttpResponseCode)int.Parse(first.Split(' ')[1]);
                HttpVersion = float.Parse(first.Split(' ')[0].Split('/')[1]);


                Headers = new Dictionary<string, string[]>();
                // Got the first line out of the way, now parse the rest
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    string name = line.Split(':')[0];
                    string val = line.Split(':')[1].Trim();
                    if (!Headers.ContainsKey(name)) Headers.Add(name, new string[1] { val });
                    else Headers[name] = Headers[name].ToList().Concat(new string[1] { val }).ToArray();
                }
            }
        }

        public float HttpVersion { get; set; }
        public Dictionary<string, string[]> Headers { get; set; }
        public HttpResponseCode Code { get; set; }
        public byte[] Data { get; set; }
        public string DataAsString
        {
            get
            {
                return Encoding.UTF8.GetString(Data);
            }
        }
    }
}
