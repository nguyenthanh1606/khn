using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khanhhoi.vn.Models;
namespace khanhhoi.vn.Models
{
    public class DeviceFull:Device
    {
        public int? VehicleID { get; set; }
        public int? VehicleCategoryID { get; set; }
        public string Capacity { get; set; }
        public string BarrelNumber { get; set; }
        public int SimCategoryID { get; set; }
        public string SimNumber { get; set; }
        public int? KhID { get; set; }
        public int? DaiLyID { get; set; }
        public string SimCategory { get; set; }
        public string VehicleCategory { get; set; }
        public string UD_DLID { get; set; }
        public string UD_KHID { get; set; }
        public string KH { get; set; }
        public string DL { get; set; }
        public string US { get; set; }
        public int? QCVN { get; set; }
        public string DriverDefault { get; set; }
        public int ProvinceID { get; set; }
        public int TypeTransportID { get; set; }
        public int? KHEdit { get; set; }
        public int? FReset { get; set; }
        public string Business { get; set; }
        public int? SpeedLimit { get; set; }
        public int? SpeedBGT { get; set; }
        public string Chassis { get; set; }
        public string Grosston { get; set; }
        public int? Cooler { get; set; }
        public string IP { get; set; }
        public int? Port { get; set; }

    }
}
