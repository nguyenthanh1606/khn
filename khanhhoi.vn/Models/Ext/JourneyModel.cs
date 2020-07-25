using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models.Ext
{
    public class JourneyModel
    {
        public DeviceStatus statusDevice { get; set; }
        public List<FileModel> listFile { get; set; }
    }
}