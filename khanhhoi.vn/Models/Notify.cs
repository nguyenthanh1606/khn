using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    public class Notify
    {
        public int? NotifyID { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DateSave { get; set; }
        public String Date_temp { get; set; }
        public string ghichu { get; set; }
        public int Count { get; set; }

        public int state { get; set; }
        public String messSupport { get; set; }
    }
}
