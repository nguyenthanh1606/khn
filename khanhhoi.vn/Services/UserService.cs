using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using khanhhoi.vn.Models;
using Newtonsoft.Json;
using khanhhoi.vn.Interfaces;
using khanhhoi.vn.Repository;
using khanhhoi.vn.Services;
using khanhhoi.vn.ModelExt;

namespace khanhhoi.vn.Services
{
    public class UserService
    {
        private Repository_khndb4 Repository;
        public UserService()
        {
            //String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SeagullDB"].ConnectionString;
            Repository = new Repository_khndb4();
        }
        

    }

}
