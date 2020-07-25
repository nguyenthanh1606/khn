using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class tbluserdevice
    {
        public int UDID { get; set; }
        public int UserID { get; set; }
        public int DeviceID { get; set; }
        public DateTime DateCreate { get; set; }
    }
}