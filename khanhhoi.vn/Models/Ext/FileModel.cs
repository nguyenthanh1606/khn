using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models.Ext
{
    public class FileModel
    {
        public String date_ { get; set; }
        public String fileName { get; set; }
        public String IMEI { get; set; }
        public String dateSave_str { get; set; }
        public DateTime dateSave { get; set; }
        public int Group { get; set; }
        public String Folderdate { get; set; }
    }
}