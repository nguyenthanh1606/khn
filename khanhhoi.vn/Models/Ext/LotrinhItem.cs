using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models.Ext
{
    public class LotrinhItem
    {
        public String Html_str { get; set; }
        public int Count { get; set; }
        public String VehicleNumber { get; set; }
        public int DeviceID { get; set; }
        public String Date { get; set; }
        public String From { get; set; }
        public String To { get; set; }
        public IList<DeviceStatus> listData { get; set; }
        public IList<Fuel> listData_Fuel { get; set; }

        public IList<PauseStop> listData_PauseStop { get; set; }

        public IList<Distance> listDistane1 { get; set; }
        public int VehicleCategoryID { get; set; }
    }
}