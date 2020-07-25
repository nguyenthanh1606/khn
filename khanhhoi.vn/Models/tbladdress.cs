using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Models
{
    public class tbladdress
    {
        //        delimiter $$

        //CREATE TABLE `tbladdress` (
        //  `AddressID` int(11) NOT NULL AUTO_INCREMENT,
        //  `Latitude` decimal(8,4) DEFAULT NULL,
        //  `Longitude` decimal(9,4) DEFAULT NULL,
        //  `Addr` varchar(160) DEFAULT NULL,
        //  `Fix` tinyint(1) DEFAULT NULL,
        //  PRIMARY KEY (`AddressID`),
        //  KEY `LatitudeIn` (`Latitude`) USING BTREE,
        //  KEY `LongitudeIn` (`Longitude`) USING BTREE
        //) ENGINE=InnoDB AUTO_INCREMENT=51163707 DEFAULT CHARSET=utf8$$
        public Int32 AddressID { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public String Addr { get; set; }
        public int state { get; set; }

    }
}