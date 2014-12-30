using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Globalization;

namespace HavokNet.Common
{
    public class MacAddress
    {
        public MacAddress(string mac)
        {
            if (mac == "*") { this.data = MacAddress.Random.data; return; }

            try
            {
                mac = mac.Replace(":", "").Replace("-", "");

                data = Enumerable.Range(0, mac.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(mac.Substring(x, 2), 16))
                             .ToArray();

            }
            catch
            {
                throw new Exception("Invalid MAC Address format.");
            }
        }
        public MacAddress(byte[] data)
        {
            this.data = data;
        }


        private byte[] data;

        public byte[] AsBytes
        {
            get
            {
                return data;
            }
        }
        public string AsString
        {
            get
            {
                return BitConverter.ToString(data).Replace('-', ':');
            }
        }

        public override string ToString()
        {
            if (Info != null && Info.VendorID != null && Info.VendorDescription != null && Info.VendorID.Length >= 0 && Info.VendorDescription.Length > 0)
            {
                return AsString + " - " + Info.VendorID + " (" + Info.VendorDescription + ")";
            }
            return AsString;
        }
        private MacInformation info;
        public MacInformation Info
        {
            get
            {
                if (info == null) info = VendorFromMacAddress(this);
                return info;
            }
        }


        public static MacAddress Broadcast
        {
            get
            {
                return new MacAddress("ff:ff:ff:ff:ff:ff");
            }
        }
        public static MacAddress Real
        {
            get
            {
                foreach (System.Net.NetworkInformation.NetworkInterface inter in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
                {
                    if (inter.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up)
                    {
                        return new MacAddress(inter.GetPhysicalAddress().ToString());
                    }
                }
                return null;
            }
        }
        public static MacAddress Random
        {
            get
            {
                Random random = new Random();
                string mac = "";
                int i = 0;
                while (i < 2)
                {
                    mac = mac + random.Next().ToString("X");
                    i++;
                }
                return new MacAddress("7C" + mac.Substring(0, 10).ToLower());
            }
        }



        public static MacInformation VendorFromMacAddress(MacAddress MAC)
        {
            string mac = MAC.AsString;
            foreach (MacInformation info in GetList())
            {
                if (mac.StartsWith(info.MacMask)) return info;
            }
            return null;
        }


        private static string ReadData(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "HavokNet.Common." + file;

            List<string> bytes = new List<string>();
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        private static List<MacInformation> GetList()
        {
            List<MacInformation> macs = new List<MacInformation>();

            byte[] data = Encoding.ASCII.GetBytes(ReadData("MacLookup.txt"));
            using (var ms = new MemoryStream(data))
            using (var reader = new StreamReader(ms))
            {
                while (!reader.EndOfStream)
                {
                    MacInformation info = new MacInformation();
                    string line = reader.ReadLine();
                    string comment = "";
                    if (line.Contains("#"))
                    {
                        comment = line.Split('#')[1].Trim();
                        line = line.Split('#')[0].Trim();
                    }
                    if (line != "" && line.Contains("\t"))
                    {
                        string mask = line.Split('\t')[0];
                        string id = line.Split('\t')[1];
                        info.MacMask = mask;
                        info.VendorDescription = comment;
                        info.VendorID = id;
                        macs.Add(info);
                    }
                }
            }
            return macs;
        }
    }
}
