using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace khanhhoi.vn.Models
{

    [Table("tbldeviceinformation", Schema = "dbo")]
    public class DeviceInfo 
    {
        /*'Imei', 'Version', 'APNName',
    'APNUser', 'APNPass', 'IPServer', 'PortServer', 'DevicePassword',
    'CompanyName', 'CompanyAddr', 'VIN',
    'VehicleNumber', 'DriverName', 'DriverLicense', 'DayCreateLicense', 'DayExpiredLicense',
        'PhoneDriver1', 'PhoneDriver2', 'PhoneDriver3', 'Speed','DateSave'
    ],
    idProperty: 'InfoID'*/


        public int InfoID { get; set; }
        public string Imei { get; set; }
        public string Version { get; set; }
        public string APNName { get; set; }
        public string APNUser { get; set; }
        public string APNPass { get; set; }
        public string IPServer { get; set; }
        public string PortServer { get; set; }
        public string DevicePassword { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddr { get; set; }
        public string DriverLicense { get; set; }
        public string DayCreateLicense { get; set; }
        public string DayExpiredLicense { get; set; }
        public string PhoneDriver1 { get; set; }
        public string PhoneDriver2 { get; set; }
        public string PhoneDriver3 { get; set; }
        public string Speed { get; set; }
        public string DateSave { get; set; }
        public string SerialNumber { get; set; }
        public string NetworkName { get; set; }
        public int? Buzz { get; set; }
        public string Coordinates { get; set; }
        public int? TimerInfo { get; set; }
        public string SimNumber { get; set; }
        public int? SignalStatus { get; set; }
    }
    public class DeviceInfoExt : DeviceInfo
    {
        public string VehicleNumber { get; set; }
        public int Stt { get; set; }
    }
      
}
