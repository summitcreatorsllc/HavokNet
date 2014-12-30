using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HavokNet.Common
{
    public class MacInformation
    {
        public MacInformation()
        {
            MacMask = "";
            VendorID = "";
            VendorDescription = "";
        }

        public string MacMask { get; set; }
        public string VendorID { get; set; }
        public string VendorDescription { get; set; }

        public override string ToString()
        {
            return VendorID + ", " + VendorDescription;
        }
    }
}
