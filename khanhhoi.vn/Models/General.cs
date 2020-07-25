using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    [Table("tblData", Schema = "dbo")]
    public class General 
    {
        public int count { get; set; }
        public string VehicleNumber { get; set; }
        public DateTime Date { get; set; }
        public string Distance { get; set; }
        public string SExceedingSpeed { get; set; }
        public string SpeedAVG { get; set; }
        public string SpeedMax { get; set; }
        public int SOpen_Close { get; set; }
        public int SPause_Stop { get; set; }
        public string SStop { get; set; }
        public string TimeDriver_ { get; set; }
        public string NameDriver { get; set; }
        public string DriverLicense { get; set; }
        public int STimeVP4 { get; set; }
        public string tyle1 { get; set; }//Tỷ lệ quá vận tốc từ 5 đến dưới 10 km/h
        public string tyle2 { get; set; }//Tỷ lệ quá vận tốc từ 10 đến dưới 20 km/h
        public string tyle3 { get; set; }//Tỷ lệ quá vận tốc từ 20 đến dưới 35 km/h
        public string tyle4 { get; set; }//Tỷ lệ quá vận tốc trên 35 km/h
        public string solan1 { get; set; }//Số lần quá tốc độ từ 5 đến dưới 10km/h
        public string solan2 { get; set; }//Số lần quá tốc độ từ 10 đến dưới 20km/h
        public string solan3 { get; set; }//Số lần quá tốc độ từ 20 đến dưới 35km/h
        public string solan4 { get; set; }//Số lần quá tốc độ trên 35km/h
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }
        public string SExceedingSpeed1000 { get; set; }
    }
    public class GeneralV2
    {
        public int count { get; set; }
        public string VehicleNumber { get; set; }
        public int DeviceID { get; set; }
        public DateTime Date { get; set; }
        public double Distance { get; set; }
        public int SExceedingSpeed { get; set; }
        public int SpeedAVG { get; set; }
        public int SpeedMax { get; set; }
        public int SOpen_Close { get; set; }
        public int SPause_Stop { get; set; }
        public int SStop { get; set; }
        public int TimeDriver_ { get; set; }
        public string NameDriver { get; set; }
        public string DriverLicense { get; set; }
        public int STimeVP4 { get; set; }
        public double tyle1 { get; set; }//Tỷ lệ quá vận tốc từ 5 đến dưới 10 km/h
        public double tyle2 { get; set; }//Tỷ lệ quá vận tốc từ 10 đến dưới 20 km/h
        public double tyle3 { get; set; }//Tỷ lệ quá vận tốc từ 20 đến dưới 35 km/h
        public double tyle4 { get; set; }//Tỷ lệ quá vận tốc trên 35 km/h
        public int solan1 { get; set; }//Số lần quá tốc độ từ 5 đến dưới 10km/h
        public int solan2 { get; set; }//Số lần quá tốc độ từ 10 đến dưới 20km/h
        public int solan3 { get; set; }//Số lần quá tốc độ từ 20 đến dưới 35km/h
        public int solan4 { get; set; }//Số lần quá tốc độ trên 35km/h
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }
        public int SExceedingSpeed1000 { get; set; }
    }

    public class Report_General
    {
        public int count { get; set; }
        
        public int DeviceID { get; set; }
        public string VehicleNumber { get; set; }
        public string TypeTransportName { get; set; }
        public double Distance { get; set; }
        public DateTime Date_data { get; set; }

        public double Type1 { get; set; }//Tỷ lệ quá vận tốc từ 5 đến dưới 10 km/h
        public double Type2 { get; set; }//Tỷ lệ quá vận tốc từ 10 đến dưới 20 km/h
        public double Type3 { get; set; }//Tỷ lệ quá vận tốc từ 20 đến dưới 35 km/h
        public double Type4 { get; set; }//Tỷ lệ quá vận tốc trên 35 km/h

        public int OverSpeed1 { get; set; }//Số lần quá tốc độ từ 5 đến dưới 10km/h
        public int OverSpeed2 { get; set; }//Số lần quá tốc độ từ 10 đến dưới 20km/h
        public int OverSpeed3 { get; set; }//Số lần quá tốc độ từ 20 đến dưới 35km/h
        public int OverSpeed4 { get; set; }//Số lần quá tốc độ trên 35km/h

        public int TotalOverSpeed { get; set; }
        public int TotalOverSpeed1000 { get; set; }
        public int TotalPauseStop { get; set; }
        public DateTime DateTimerRpt { get; set; }

    }
}
