using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    //[Table("tblData", Schema = "dbo")]
    public class PauseStop
    {
        public int count { get; set; }
        public string VehicleNumber { get; set; }
        public string DateTime { get; set; }
        public DateTime DateTimeStart { get; set; }
        public DateTime DateTimeEnd { get; set; }
        public string Duration { get; set; }
        public string Status { get; set; }
        public string Address { get; set; }
        public string Coordinates { get; set; }
        public string NameDriver { get; set; }
        public string DriverLicense { get; set; }
        public DateTime Date { get; set; }
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
    }
    public class GpsDataForPauseStop
    {
        public string Version { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int Speed { get; set; }
        public int Sleep { get; set; }
        public int StatusKey { get; set; }
        public DateTime DateSave { get; set; }
        public string Addr { get; set; }
        public string TheDriver { get; set; }
        public int DeviceID { get; set; }
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }
    }
}
