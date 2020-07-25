using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class BaoCaoHanhTrinh_Detail
    {
        public String Driver { get; set; }
        public String VehicleNumber { get; set; }
        public DateTime timeStart { get; set; }
        public DateTime timeEnd { get; set; }
        public String Duran { get; set; }
        public double Distane { get; set; }
        public int SpeedMax { get; set; }
        public String timeDrive { get; set; }
        public int timePause { get; set; }
        public int timeStop { get; set; }
        public String positionStart { get; set; }
        public String positionEnd { get; set; }
    }
}