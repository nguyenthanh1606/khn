using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace khanhhoi.vn.Models
{

    [Table("tblFile", Schema = "dbo")]
    public class File
    {
        public int FileID { get; set; }
        public string FileName { get; set; }
        public string Description { get; set; }
        public string URL { get; set; }
        public DateTime? DateUpload { get; set; }
    }
}
