using System;

namespace khanhhoi.vn.Models
{
    public class Issue
    {
        public string VehicleNumber { get; set; }
        public string TypeIS { get; set; }
        public int Speed { get; set; }
        public DateTime DateIS { get; set; }
        public DateTime DateSave { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int DeviceID { get; set; }
    }
}
