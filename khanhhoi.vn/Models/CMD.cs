using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.ModelExt
{
    public class CMD
    {
        public int CMDID { get; set; }
        public string CMDName { get; set; }
        public string CMD_ { get; set; }
    }

    public class CMDDevice
    {
        public int CMDID { get; set; }
        public string CMD { get; set; }
        public DateTime DateCreate { get; set; }
        public int CMDStt { get; set; }

    
    }
}
