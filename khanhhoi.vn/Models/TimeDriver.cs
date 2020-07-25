using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    [Table("tblData", Schema = "dbo")]
    public class TimeDriver 
    {
        public int count { get; set; }
        public string VehicleNumber { get; set; }
        public string Date { get; set; }
        public DateTime date2 { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public string TimeDriver_ { get; set; }
        public string AddressStart { get; set; }
        public string CoordinatesStart { get; set; }
        public string AddressEnd { get; set; }
        public string CoordinatesEnd { get; set; }
        public string SpeedAVG { get; set; }
        public string SpeedMax { get; set; }
        public string Distance { get; set; }
        public string Warning { get; set; }
        public string NameDriver { get; set; }
        public string DriverLicense { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string theDriver { get; set; }
        public double sDistance { get; set; }
        public double stimedriver { get; set; }
        public string phoneOld { get; set; }
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }

    }
   
}
