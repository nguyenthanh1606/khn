using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class VehicleGroups
    {
        public int VehicleGroupID { get; set; }
        public string VehicleGroup { get; set; }
        public int UserID { get; set; }
        public string id { get; set; }
        public string text { get; set; }
        public bool @checked { get; set; }
        public int? SL { get; set; }
        public int? ParentGroupID { get; set; }
    }
}