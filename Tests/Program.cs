using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HavokNet.Stack;
using HavokNet.Common;

namespace Tests
{
    class Program
    {
        public static NetClient client = null;
        static void Main(string[] args)
        {
            PrintTitle("Starting Havok NetClient Test Suite...", true);

            var nic = NetworkCard.GetFirstDevice();
            client = new NetClient(nic, nic.Config);
            client.NetworkFirewall = new HavokNet.Firewall.SimpleFirewall(true, true);
            client.Start();

            Console.WriteLine("Client started.");
            Console.WriteLine();
            Console.WriteLine("Layer 2 Configuration:");
            Console.WriteLine("\tMAC Address       : " + client.Configuration.MacAddress.AsString);
            Console.WriteLine();
            Console.WriteLine("Layer 3 Configuration: ");
            Console.WriteLine("\tIP Address        : " + client.Configuration.IpAddress.AsString);
            Console.WriteLine("\tSubnet Mask       : " + client.Configuration.SubnetMask.AsString);
            Console.WriteLine("\tDefault Gateway   : " + client.Configuration.DefaultGateway.AsString);
            for (int i = 0; i < client.Configuration.DnsServers.Length;i++)
            {
                Console.WriteLine("\tDNS Server " + (i + 1).ToString() + "      : " + client.Configuration.DnsServers[i].AsString);
            }
            Console.WriteLine("\tNetwork Address   : " + client.Configuration.NetworkAddress.AsString);
            Console.WriteLine("\tBroadcast Address : " + client.Configuration.BroadcastAddress.AsString);
            Console.WriteLine();
            Console.WriteLine();

            Console.WriteLine("Starting tests...");

            // Give us a second to read the text
            System.Threading.Thread.Sleep(1500);

            RunTests();
        }

        static void RunTests()
        {
            TestArp();
            TestIcmp();
            TestDns();

            ShowResults();
        }

        #region "ARP Tests"

        static void TestArp()
        {
            PrintTitle("Starting ARP tests...");
            RunArpTest(client.Configuration.DefaultGateway);
            RunArpTest(client.Configuration.DnsServers[0]);
            RunArpTest(client.Configuration.BroadcastAddress);
            RunArpTest(client.Configuration.IpAddress);
        }
        static void RunArpTest(HavokNet.Common.IPv4Address ip)
        {
            ROT("ARPing " + ip.AsString, () => client.Arp.ResolveIP(client.Configuration.DefaultGateway).AsString);
        }

        #endregion

        #region "DNS"

        static void TestDns()
        {
            PrintTitle("Starting DNS tests...");
            ResolveHost("www.google.com");
            ResolveHost("www.pantego.com");
            ResolveHost("www.reddit.com");


            ReverseDnsLookup(new IPv4Address("8.8.4.4"));
            ReverseDnsLookup(new IPv4Address("8.8.8.8"));
            ReverseDnsLookup(new IPv4Address("209.253.18.18"));
            ReverseDnsLookup(new IPv4Address("209.253.18.17"));

            NameserverLookup("pantego.com");
            NameserverLookup("yahoo.com");
            NameserverLookup("sergiogordon.com");

            MxLookup("pantego.com");
        }

        static void ResolveHost(string host)
        {
            ROT("Resolving " + host, () => client.Dns.ResolveHost(host).ToString());
        }
        static void ReverseDnsLookup(HavokNet.Common.IPv4Address ip)
        {
            ROT("Resolving PTR " + ip.AsString, () => client.Dns.ReverseDnsLookup(ip).ToString());
        }
        static void NameserverLookup(string host)
        {
            ROT("NS lookup of " + host, () => client.Dns.RetrieveNameserverList(host).ToString());
        }
        static void MxLookup(string host)
        {
            ROT("MX lookup of " + host, () => client.Dns.MailLookup(host).ToString());
        }

        #endregion

        #region "ICMP"

        static void TestIcmp()
        {
            PrintTitle("Starting ICMP tests...");

            RunPingTest("www.reddit.com");
            RunPingTest("8.8.8.8");
            RunPingTest(client.Configuration.BroadcastAddress.AsString);
        }

        static void RunPingTest(string iporhost)
        {
            Console.WriteLine("Pinging " + iporhost);
            ConsoleColor col = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Green;
                client.Icmp.Ping(iporhost, HavokNet.Protocols.ICMP.PingSettings.FromConfig(64, 32, 1, 3, 240), (result) =>
                    {
                        Console.WriteLine(result.ToString());
                    });
                success++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                failed++;
            }
            Console.ForegroundColor = col;
            Console.WriteLine();
        }

        #endregion

        #region "Helpers"
        private static int success = 0;
        private static int failed = 0;
        // Run, output, time
        static void ROT(string title, Func<string> run)
        {
            Console.WriteLine(title);
            DateTime now = DateTime.Now;
            ConsoleColor col = Console.ForegroundColor;
            try
            {
                string x = run();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(x + " (took " + (DateTime.Now - now).ToString() + ")");
                success++;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERROR: " + ex.Message);
                failed++;
            }
            Console.ForegroundColor = col;
            Console.WriteLine();
            System.Threading.Thread.Sleep(500);
        }

        static void PrintTitle(string title)
        {
            PrintTitle(title, false);
        }
        static void PrintTitle(string title, bool dontPrintHeader)
        {
            if (!dontPrintHeader) for (int i = 0; i < 5;i++) Console.WriteLine();
            Console.Title = title;
            ConsoleColor col = Console.ForegroundColor;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("****************************************************");

            string titleStr = title;
            while (titleStr.Length < 42)
            {
                if (titleStr.Length % 2 == 1)
                {
                    titleStr = titleStr + " ";
                }
                else
                {
                    titleStr = " " + titleStr;
                }
            }
            titleStr = "**** " + titleStr + " ****";

            Console.WriteLine(titleStr);
            Console.WriteLine("****************************************************");

            Console.ForegroundColor = col;
            Console.WriteLine();
        }

        static void ShowResults()
        {
            PrintTitle("Results:");

            ConsoleColor col = Console.ForegroundColor;
            Console.Write("Successful Tests : ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(success.ToString());
            Console.ForegroundColor = col;

            Console.Write("Failed Tests     : ");
            if (failed == 0)
            {
                Console.ForegroundColor = ConsoleColor.Green;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
            }
            Console.WriteLine(failed.ToString());
            Console.ForegroundColor = col;

            if (failed == 0)
            {
                Console.WriteLine();
                PrintTitle("PERFECT SCORE");
            }
            client.Stop();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        #endregion
    }
}
