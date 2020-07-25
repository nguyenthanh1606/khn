using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class DeviceStatus : GpsData
    {
        public int? VehicleGroupID { get; set; }
        public int? SL { get; set; }
        public string VehicleNumber { get; set; }
        public string Addr { get; set; }
        public Int64? AddressID { get; set; }
        public DateTime? DateExpired { get; set; }
        public string Status { get; set; }

        public int? Key_ { get; set; }
        public int? Door { get; set; }
        //    public DateTime DateSave { get; set; }
        public string color { get; set; }
        public string VehicleGroup { get; set; }

        public string KeyShow { get; set; }
        public string DoorShow { get; set; }
        public string stringStatus { get; set; }
        public int VehicleCategoryID { get; set; }
        public string id { get; set; }
        public string leaf { get; set; }
        public string text { get; set; }
        public bool @checked { get; set; }
        public string in_out { get; set; }
        public string STT { get; set; }

        public decimal? OldLat { get; set; }
        public decimal? OldLng { get; set; }

        public decimal? DLat { get; set; }
        public decimal? DLng { get; set; }
        public string NameDriver { get; set; }
        public string PhoneDriver { get; set; }
        public string DriverLicense { get; set; }
        public DateTime? DayCreateLicense { get; set; }
        public DateTime? DayExpiredLicense { get; set; }
        public string Rank { get; set; }
        public string RegPlace { get; set; }
        public int? TypeTransportID { get; set; }//ma loai hinh van tai
        public string TypeTransportName { get; set; }
        public string ProvinceName { get; set; }
        public string Business { get; set; }
        public int SpeedLimit { get; set; }
        public string Chassis { get; set; }
        public int? Cooler { get; set; }
        public string Grosston { get; set; }
        public int? Cooler_ { get; set; }
        public decimal ZoneLat { get; set; }
        public decimal ZoneLng { get; set; }
        public int Radius { get; set; }
        public int? QCVN { get; set; }
        public int? OilValue { get; set; }
        public int? FuelCapacity { get; set; }
        //public string DungDo { get; set; }
        public decimal? OldLat_ { get; set; }
        public decimal? OldLng_ { get; set; }
        public String loginname { get; set; }
        public int status_id { get; set; }
        //   public string Time { get; set; }
        public string ioelement { get; set; }
    }
}