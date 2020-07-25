using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class OilInfomation
    {
        public int InfoCalID { get; set; }
        public string VehicleNumber { get; set; }
        public string ModelVehicle { get; set; }
        public int? VoltMax { get; set; }
        public int? VoltMin { get; set; }
        public int? VolumeOilBarrel { get; set; }
        public int method_id { get; set; }
        public string method_name { get; set; }
        public int? VoltOld { get; set; }
        public DateTime? DatesaveOld { get; set; }

    }
}