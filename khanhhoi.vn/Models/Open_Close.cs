using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    [Table("tblData", Schema = "dbo")]
    public class Open_Close 
    {
        public int count { get; set; }
        public string VehicleNumber { get; set; }
        public string TimeOpen { get; set; }
        public string TimeClose { get; set; }
        public string AddressOpen { get; set; }
        public string AddressClose { get; set; }
        public string CoordinatesOpen { get; set; }
        public string CoordinatesClose { get; set; }
        public string NameDriver { get; set; }
        public string DriverLicense { get; set; }
        public string Speed { get; set; }
        public DateTime Date { get; set; }
        public String Duration { get; set; }

    }
  public class GpsDataForOpenClose 
    {
        public string Addr { get; set; }
        public int? StatusDoor { get; set; }
        public DateTime DateSave { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public string TheDriver { get; set; }
        public int DeviceID { get; set; }
        public int Speed { get; set; }
        public int? Cooler { get; set; }
        public string In_Out { get; set; }
        
    }

}
