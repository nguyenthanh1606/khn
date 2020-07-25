using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class tbldozy
    {
        public int dozy_id { get; set; }
        public int deviceid { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public int Speed { get; set; }
        public DateTime DateSave { get; set; }
        public String img { get; set; }
    }
}