using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    [Table("tblData", Schema = "dbo")]
    class DistaneModel
    {
        public String sCoor { get; set; }
        public String eCoor { get; set; }
    }
}
