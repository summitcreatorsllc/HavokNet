using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Common
{
    public class IPv4Address : IPAddress
    {
        public IPv4Address(string ip)
        {
            int b = 0;
            data = new byte[4];
            foreach (string s in ip.Split('.'))
            {
                data[b] = byte.Parse(s);
                b++;
            }
        }
        public IPv4Address(byte[] ip)
        {
            data = ip;
        }

        

        private byte[] data;

        public override byte[] AsBytes
        {
            get
            {
                return data;
            }
        }

        public override string AsString
        {
            get
            {
                try
                {
                    return data[0].ToString() + "." + data[1].ToString() + "." + data[2].ToString() + "." + data[3].ToString();
                }
                catch { }
                return "0.0.0.0";
            }
        }

        public override string ToString()
        {
            return AsString;
        }
        
        public string AsReverseString
        {
            get
            {
                return new IPv4Address(data.Reverse().ToArray()).AsString;
            }
        }


        public static IPv4Address Localhost
        {
            get
            {
                return new IPv4Address("127.0.0.1");
            }
        }
        public static IPv4Address Any
        {
            get
            {
                return new IPv4Address("0.0.0.0");
            }
        }


       

        public static bool IsInSameSubnet(IPAddress address2, IPAddress address, IPAddress subnetMask)
        {
            IPAddress network1 = GetNetworkAddress(address, subnetMask);
            IPAddress network2 = GetNetworkAddress(address2, subnetMask);

            return network1.AsString == network2.AsString;
        }
        public static IPv4Address GetBroadcastAddress(IPAddress ip, IPAddress subnet)
        {
            byte[] ipAdressBytes = ip.AsBytes;
            byte[] subnetMaskBytes = subnet.AsBytes;

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            }
            return new IPv4Address(broadcastAddress);
        }
        public static IPv4Address GetNetworkAddress(IPAddress ip, IPAddress subnet)
        {
            byte[] ipAdressBytes = ip.AsBytes;
            byte[] subnetMaskBytes = subnet.AsBytes;

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++)
            {
                broadcastAddress[i] = (byte)(ipAdressBytes[i] & (subnetMaskBytes[i]));
            }
            return new IPv4Address(broadcastAddress);
        }
    }
}
