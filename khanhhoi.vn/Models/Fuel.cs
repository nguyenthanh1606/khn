using System;

namespace khanhhoi.vn.Models
{
    public class Fuel
    {
        public string VehicleNumber { get; set; }
        public string Address { get; set; }
        public string Speed { get; set; }
        public DateTime DateSave { get; set; }
        public int Oilvalue { get; set; }
        public double Oilvalue_real { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public int DeviceID { get; set; }
        public int val { get; set; }
        //public double Fuel_ { get; set; }
        //StatusKey
        //public int StatusKey { get; set; }
        //public int FuelCapacity { get; set; }
        //public double FuelAmount { get; set; }
        //public double Distane { get; set; }
        //public double tieuthu { get; set; }
    }
    public class reportFule {
        public string VehicleNumber { get; set; }
        public string Address_start { get; set; }
        public DateTime DateSave_start { get; set; }
        public string Oilvalue_start { get; set; }
        public string Address_end { get; set; }
        public DateTime DateSave_end { get; set; }
        public string Oilvalue_end { get; set; }
        public double Oilvalue_Result { get; set; }
        public double Distane { get; set; }

    }
}
