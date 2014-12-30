using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PcapDotNet.Core;

namespace HavokNet.Common
{
    public class NetworkCard
    {
        public NetworkCard(LivePacketDevice dev, int index)
        {
            // Save the device so we can open it later!
            device = dev;

            // Get the GUID
            string idStr = dev.Name.Remove(0, dev.Name.IndexOf('{'));


            foreach (string s in idStr.Split('-'))
            {
                if (s.Length == 13) macstr = s.TrimEnd('}');
            }

            // Match to full NetworkCard info
            // from .NET classes
            foreach (var nic in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.Id == idStr)
                {
                    Interface = nic;
                    break;
                }
            }

            // Did we find one?
            if (Interface == null && macstr == null)
            {
                throw new Exception("Invalid network device.");
            }

        }

        private string macstr;
        public System.Net.NetworkInformation.NetworkInterface Interface { get; set; }
        public List<IPv4Address> Addresses { get; set; }

        private LivePacketDevice device;
        public LivePacketDevice Device
        {
            get
            {
                return device;
            }
        }
        public PacketCommunicator GetCommunicator()
        {
            return device.Open(65536, PacketDeviceOpenAttributes.Promiscuous | PacketDeviceOpenAttributes.MaximumResponsiveness | PacketDeviceOpenAttributes.NoCaptureLocal, 1000);
        }


        public string Name
        {
            get
            {
                if (Interface == null) return Device.Name;
                return Interface.Name;
            }
        }
        public string Description
        {
            get
            {
                if (Interface == null) return Device.Description;
                return Interface.Description;
            }
        }

        public string Type
        {
            get
            {
                return Interface.NetworkInterfaceType.ToString();
            }
        }
        public string Speed
        {
            get
            {
                switch (Interface.OperationalStatus)
                {
                    case System.Net.NetworkInformation.OperationalStatus.Up:
                        return Toolset.GetSpeedFromLong(Math.Max(Interface.Speed, 0));
                    case System.Net.NetworkInformation.OperationalStatus.Down:
                        return "Connection Down.";
                    case System.Net.NetworkInformation.OperationalStatus.LowerLayerDown:
                        return "Dependent Connection Down.";
                    case System.Net.NetworkInformation.OperationalStatus.Dormant:
                        return "Connection Dormant.";
                    case System.Net.NetworkInformation.OperationalStatus.NotPresent:
                        return "Interface Missing.";
                    case System.Net.NetworkInformation.OperationalStatus.Testing:
                        return "Testing.";
                    default:
                        return "Status Unknown.";
                }
            }
        }
        public string GUID
        {
            get
            {
                return Interface.Id;
            }
        }
        public string Index { get; set; }

        public System.Net.NetworkInformation.OperationalStatus OperationalStatus
        {
            get
            {
                try
                {
                    return Interface.OperationalStatus;
                }
                catch { }
                return System.Net.NetworkInformation.OperationalStatus.Up;
            }
        }
        public OSI.IPv4StackConfiguration Config
        {
            get
            {
                if (Interface == null)
                {
                    return new OSI.IPv4StackConfiguration() { MacAddress = new MacAddress(macstr) };
                }

                OSI.IPv4StackConfiguration config = new OSI.IPv4StackConfiguration();
                foreach (System.Net.NetworkInformation.UnicastIPAddressInformation uip in Interface.GetIPProperties().UnicastAddresses)
                {
                    string uipstr = uip.Address.ToString();
                    if (uipstr.StartsWith("192") || uipstr.StartsWith("172") || uipstr.StartsWith("10."))
                    {
                        config.IpAddress = new IPv4Address(uipstr);
                        config.SubnetMask = new IPv4Address(uip.IPv4Mask.GetAddressBytes());
                        break;
                    }
                }
                try
                {
                    config.DefaultGateway = new IPv4Address(Interface.GetIPProperties().GatewayAddresses[0].Address.ToString());
                    List<IPv4Address> dnsServers = new List<IPv4Address>();
                    foreach (System.Net.IPAddress dnsIP in Interface.GetIPProperties().DnsAddresses)
                    {
                        dnsServers.Add(new IPv4Address(dnsIP.GetAddressBytes()));
                    }
                    config.DnsServers = dnsServers.ToArray();
                }
                catch { }
                config.MacAddress = new MacAddress(Interface.GetPhysicalAddress().GetAddressBytes());
                return config;
            }
        }

        #region "Helpers"

        public static NetworkCard GetFirstDevice()
        {
            try
            {
                List<NetworkCard> nics = GetAllDevices();
                return nics[0];
            }
            catch
            {
                throw new Exception("There are no available network devices.");
            }
        }
        public static List<NetworkCard> GetAllDevices()
        {
            // Retrieve the device list from the local machine
            IList<LivePacketDevice> allDevices = LivePacketDevice.AllLocalMachine;

            List<NetworkCard> devs = new List<NetworkCard>();

            // Print the list
            for (int i = 0; i != allDevices.Count; ++i)
            {
                try
                {
                    devs.Add(new NetworkCard(allDevices[i] as LivePacketDevice, i));
                }
                catch { }
            }
            return new List<NetworkCard>(from a in devs orderby a.OperationalStatus select a);
        }
        public static List<NetworkCard> GetAllAvailableDevices()
        {
            return new List<NetworkCard>(from a in GetAllDevices() where a.OperationalStatus == System.Net.NetworkInformation.OperationalStatus.Up select a);
        }
            

        #endregion

    }
}
