using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace khanhhoi.vn.Models
{

    [Table("tbldeviceinformation", Schema = "dbo")]
    public class Log 
    {
        /*'Imei', 'Version', 'APNName',
    'APNUser', 'APNPass', 'IPServer', 'PortServer', 'DevicePassword',
    'CompanyName', 'CompanyAddr', 'VIN',
    'VehicleNumber', 'DriverName', 'DriverLicense', 'DayCreateLicense', 'DayExpiredLicense',
        'PhoneDriver1', 'PhoneDriver2', 'PhoneDriver3', 'Speed','DateSave'
    ],
    idProperty: 'InfoID'*/

        public int LogID { get; set; }
        public string UserName { get; set; }
        public string IP { get; set; }
        public string DateSave { get; set; }
    }
}
