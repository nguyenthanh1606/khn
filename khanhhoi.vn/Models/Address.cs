using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace khanhhoi.vn.Models
{
    public class Address
    {
        public string Addr { get; set; }
    }
    public class AddressFull:Address
    {
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public Int32 AddressID { get; set; }
    }
}
