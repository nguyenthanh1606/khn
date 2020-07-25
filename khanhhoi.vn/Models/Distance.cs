using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    [Table("tblData", Schema = "dbo")]
    public class Distance 
    {
        public string VehicleNumber { get; set; }
        public string Distances { get; set; }
        public string SpeedAVG { get; set; }
        public string  SpeedMax { get; set; }
        public DateTime Date { get; set; }
        public string TotalFuel { get; set; }
        public string Date_from { get; set; }
        public string Date_to { get; set; }
    }
}
