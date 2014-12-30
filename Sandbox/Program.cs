using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HavokNet.Common;
using HavokNet.Stack;
using HavokNet.OSI;
using HavokNet.Protocols.HTTP;

namespace Sandbox
{
    class Program
    {
        public static NetClient client = null;
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Havok NetClient...", true);

            var nic = NetworkCard.GetFirstDevice();
            IPv4StackConfiguration config = new IPv4StackConfiguration()
            { 
                IpAddress = new IPv4Address("192.168.1.66"),
                MacAddress = new MacAddress("7C:7A:91:73:3F:25"),
                SubnetMask = new IPv4Address("255.255.255.0"),
                DefaultGateway = new IPv4Address("192.168.1.254"),
                DnsServers = new IPv4Address[1]
                { 
                    new IPv4Address("192.168.1.254")
                }
            };
            client = new NetClient(nic, config);
            client.NetworkFirewall = new HavokNet.Firewall.SimpleFirewall(true, true);
            client.Start();
            Console.WriteLine("Ready to send. Press any key...");
            Console.ReadKey();

            //client.Tcp.Connect("www.google.com", 80);
            //return;

            HttpResponse response = client.Http.Get("http://www.reddit.com");
            client.Stop();

            Console.WriteLine();
            Console.WriteLine("HTTP/" + response.HttpVersion.ToString() + " " + response.Code.ToString());
            foreach (KeyValuePair<string,string[]> pair in response.Headers)
            {
                Console.WriteLine(pair.Key +": " + string.Join(",", pair.Value));
            }

            if (response.Code == HttpResponseCode.Ok)
            {
                string html = response.DataAsString;
                string file = @"C:\Users\john.davis\Desktop\osi.html";
                System.IO.File.WriteAllText(file, html);
                System.Diagnostics.Process.Start(file);
            }

            Console.ReadKey();
        }
    }
}
