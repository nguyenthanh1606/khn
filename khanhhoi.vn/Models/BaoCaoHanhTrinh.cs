using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khanhhoi.vn.Models;

namespace khanhhoi.vn.Models
{
    public class BaoCaoHanhTrinh:GpsData
    {
        public int? Key_ { get; set; }
        public int? Door_ { get; set; }
        public string VehicleNumber { get; set; }
        public string Addr { get; set; }
        public string Status { get; set; }
        public string Alert { get; set; }
        public string NameDriver { get; set; }
        public string DriverLicense { get; set; }
        public int? SpeedLimit { get; set; }
        public string Distance { get; set; }
        public string Description { get; set; }
    }
}
