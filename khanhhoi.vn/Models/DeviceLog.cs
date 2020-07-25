using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace khanhhoi.vn.Models
{

    [Table("tbldeviceinformation", Schema = "dbo")]
    public class DeviceLog 
    {
        /*'Imei', 'Version', 'APNName',
    'APNUser', 'APNPass', 'IPServer', 'PortServer', 'DevicePassword',
    'CompanyName', 'CompanyAddr', 'VIN',
    'VehicleNumber', 'DriverName', 'DriverLicense', 'DayCreateLicense', 'DayExpiredLicense',
        'PhoneDriver1', 'PhoneDriver2', 'PhoneDriver3', 'Speed','DateSave'
    ],
    idProperty: 'InfoID'*/

        public int dvLogID { get; set; }
        public string dvLogContent { get; set; }
        public string dvNote { get; set; }
        public DateTime DateLog { get; set; }
    }
}
