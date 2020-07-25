using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace khanhhoi.vn.Models
{

    [Table("tblFile", Schema = "dbo")]
    public class Fail
    {
        public int FID { get; set; }
        public string FVehicle { get; set; }
        public DateTime FDate { get; set; }
        public string FContent { get; set; }
    }
}
