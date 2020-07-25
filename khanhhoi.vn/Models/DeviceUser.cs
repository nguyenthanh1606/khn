using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    public class DeviceUser
    {
        public int? DeviceID { get; set; }
        public int? VehicleGroupID { get; set; }
        public int? U_DID { get; set; }
        public string VehicleNumber { get; set; }
        public string Imei { get; set; }
        public string Status { get; set; }
        public DateTime DateExpired { get; set; }
        public string VehicleGroup { get; set; }
        public string NameDevice { get; set; }
        public int? VehicleCategoryID { get; set; }
        public string Capacity { get; set; }
        public string BarrelNumber { get; set; }
        public int? VehicleID { get; set; }
        public string SimNumber { get; set; }
        public int? KHEdit { get; set; }
        public int SimID { get; set; }
        public int ProvinceID { get; set; }
        public int TypeTransportID { get; set; }
        public string Business { get; set; }
        public string Chassis { get; set; }
        public string Grosston { get; set; }
        public int? ParentGroupID { get; set; }
        public int? VehicleGroupIDChild { get; set; }
        public int? CurrentParentID { get; set; }
        public int? CurrentChildID { get; set; }
        public string Version { get; set; }
        public int? Switch_ { get; set; }
        public int? Switch_Door { get; set; }
        public double ZoneLat { get; set; }
        public double ZoneLng { get; set; }
        public int Radius { get; set; }
        public int? FuelConsumption { get; set; }
        public int? FuelCapacity { get; set; }
    }
}
