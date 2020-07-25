using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class GpsData
    {
        public long? DataID { get; set; }
        public int DeviceID { get; set; }
        public string Version { get; set; }
        public string Imei { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? Speed { get; set; }
        // public int? SpeedLimit { get; set; }
        public int? StatusKey { get; set; }
        public int? StatusDoor { get; set; }
        public int? Sleep { get; set; }
        public DateTime? DateSave { get; set; }
        public int Switch_ { get; set; }
        public int Switch_Door { get; set; }
        public string TheDriver { get; set; }


    }
    public class GpsDataExt
    {
        public long? DataID { get; set; }
        public string Imei { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int? Speed { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime DateSave { get; set; }
        public string Address { get; set; }
        public string Addr { get; set; }
        public string TheDriver { get; set; }
        public int DeviceID { get; set; }
        public int? SpeedLimit { get; set; }
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }
        public int? FuelConsumption { get; set; }
    }
    public class GpsDataExtForGeneral : GpsData
    {
        public string VehicleNumber { get; set; }
        public string Address { get; set; }
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }

    }
    public class GpsdataExtForOil : GpsData
    {
        public int? Oilvalue { get; set; }
    }
}