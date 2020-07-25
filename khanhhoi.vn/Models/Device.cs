using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace khanhhoi.vn.Models
{

    [Table("tblDevice", Schema = "dbo")]
    public class Device 
    {
        public int DeviceID { get; set; }
        public string Imei { get; set; }
        public int SimID { get; set; }
        public string Version { get; set; }
        public string NameDevice { get; set; }
        public int Status_ { get; set; }
        public string Region { get; set; }
        public int Switch_ { get; set; }
        public int Switch_Door { get; set; }
        public int Switch_Cooler { get; set; }
        public int Key_ { get; set; }
        public int Door { get; set; }
        public DateTime? DateCreate { get; set; }
        public DateTime? DateExpired { get; set; }
        public bool? Active { get; set; }
        public string VehicleNumber { get; set; }
        public int VehicleGroupID { get; set; }
        public int? KHEdit { get; set; }
        public int? FReset { get; set; }
        public int? SpeedLimit { get; set; }
        public int? Cooler_ { get; set; }
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }

    }
}
