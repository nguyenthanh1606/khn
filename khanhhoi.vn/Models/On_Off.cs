using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    [Table("tblDevice", Schema = "dbo")]
    public class On_Off 
    {
        public int count { get; set; }
        public string VehicleNumber { get; set; }
        public string TimeOpen { get; set; }
        public string TimeClose { get; set; }
        public string Duration { get; set; }
        public string DateTime { get; set; }
        public string Warning { get; set; }
        public string NameDriver { get; set; }
        public string DriverLicense { get; set; }
        public DateTime Date { get; set; }
        public string AddressOpen { get; set; }
        public string AddressClose { get; set; }
        public string CoordinatesOpen { get; set; }
        public string CoordinatesClose { get; set; }

    }
    public class GpsDataForOn_Off
    {
        public int? StatusKey { get; set; }
        public DateTime DateSave { get; set; }
        
        public string TheDriver { get; set; }
        public int DeviceID { get; set; }
        
    }
}
