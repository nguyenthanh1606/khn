using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models.Ext
{
    public class LoadModel
    {
        public decimal percentLoad { get; set; }
        public List<DeviceStatus> listDeviceStatus { get; set; }
    }
}