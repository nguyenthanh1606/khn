using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class Driver
    {
        public int DriverID { get; set; }
        public string Imei { get; set; }
        public string NameDriver { get; set; }
        public string PhoneDriver { get; set; }
        public string IdCard { get; set; }
        public string DriverLicense { get; set; }
        public string Rank { get; set; }
        public DateTime DayCreateLicense { get; set; }
        public DateTime DayExpiredLicense { get; set; }
        public string RegPlace { get; set; }
        public DateTime DateDriver { get; set; }
        //public string hethan { get; set; }
        public string Details { get; set; }
        public int ExceedingSpeed { get; set; }
        public string ContinuousDriving { get; set; }
        public double ContinuousDrivingTemp { get; set; }
        public string DrivingInDay { get; set; }
        public int ContinuousDrivingViolations { get; set; }
        public int OpenDoor { get; set; }
        public int Switch_Door { get; set; }
        public string Warning { get; set; }
        public int? DeviceID { get; set; }
        public int? DDID { get; set; }
        public string Distace { get; set; }//'Distace',
        public string Door { get; set; }
        public string DungDo { get; set; }
    }
    public class DriverC
    {
        public string NameDriver { get; set; }
        public string DriverDefault { get; set; }
        public string DriverLicense { get; set; }
    }
}