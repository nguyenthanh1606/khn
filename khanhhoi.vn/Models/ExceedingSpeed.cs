using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using khanhhoi.vn.ModelExt;
namespace khanhhoi.vn.Models
{
    [Table("tblData", Schema = "dbo")]
    public class ExceedingSpeed 
    {
        public int count { get; set; }
        public string VehicleNumber { get; set; }
       // public string Date { get; set; }
        public DateTime Date { get; set; }
        public DateTime DateSave { get; set; }
        public string TimeStart { get; set; }
        public string SpeedStart { get; set; }
        public string TimeEnd { get; set; }
        public string SpeedEnd { get; set; }
        public string SpeedMax { get; set; }
        public string Duration { get; set; }
        public string Address { get; set; }
        public string AddressEnd { get; set; }
        public string Coordinates { get; set; }
        public string NameDriver { get; set; }
        public string DriverLicense { get; set; }
        public string LoaiHinhHoatDong { get; set; }
        public string TocDoTrungBinh { get; set; }
        public string SpeedLimit { get; set; }
        public string Coordinates_ketthuc { get; set; }
        public string TypeTransportName { get; set; }
        public int TypeTransportID { get; set; }
        public long dataIDStart { get; set; }
        public long dataIDEnd { get; set; }
        public double Distance { get; set; }

    }
}
