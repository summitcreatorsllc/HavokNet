using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet
{
    public static class Toolset
    {
        public static HavokNet.Common.NetworkCard GetDevice(string[] args)
        {
            string arg = GetArg(args, "-dev");

            if (arg != null)
            {
                try
                {
                    int devNumber = int.Parse(arg);
                    return HavokNet.Common.NetworkCard.GetAllDevices()[devNumber];
                }
                catch
                {
                    throw new Exception("No device by that number was found. Please select a different number. Use netinfo to view all available devices.");
                }
            }

            return HavokNet.Common.NetworkCard.GetFirstDevice();
        }
        public static Common.MacAddress GetMac(string[] args)
        {
            string arg = GetArg(args, "-mac");

            if (arg != null)
            {
                try
                {
                    return new Common.MacAddress(arg);
                }
                catch
                {
                    throw new Exception("The Mac Address specified was in an incorrect format.");
                }
            }
            return null;
        }

        public static Common.IPv4Address GetIp(string[] args)
        {
            return GetIp(args, "-ip");
        }
        public static Common.IPv4Address GetIp(string[] args, string argName)
        {
            string arg = GetArg(args, argName);

            if (arg != null)
            {
                try
                {
                    return new Common.IPv4Address(arg);
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public static int GetNumber(string[] args, string argName, int def)
        {
            string arg = GetArg(args, argName);
            if (arg != null)
            {
                try
                {
                    return int.Parse(arg);
                }
                catch
                {

                }
            }
            return def;
        }
        public static string GetArg(string[] args, string argName)
        {
            int i = 0;
            foreach (string s in args)
            {
                if (s.ToLower() == argName)
                {
                    try
                    {
                        return args[i + 1];
                    }
                    catch
                    {
                        throw new Exception("The argument format was incorrect.");
                    }
                }

                i++;
            }

            return null;
        }
        private static bool FindArg(string[] args, string argName)
        {
            foreach (string s in args)
            {
                if (s.ToLower() == "-dev") return true;
            }

            return false;
        }
        public static bool GetBool(string[] args, string argName, bool def)
        {
            string ans = GetArg(args, argName);
            if (ans == null) return def;
            try
            {
                return bool.Parse(argName);
            }
            catch
            {
                return def;
            }
        }

        public static string[] SpeedUnits = new string[6] { "Bps", "Kbps", "Mbps", "Gbps", "Tbps", "Pbps" };
        public static string GetSpeedFromLong(long speed)
        {
            int index = 0;
            while (speed / 1000 > 0)
            {
                speed = speed / 1000;
                index++;
            }

            return speed.ToString() + " " + SpeedUnits[index];
        }
    }
}
