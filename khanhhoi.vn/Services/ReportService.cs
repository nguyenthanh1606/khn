using khanhhoi.vn.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using khanhhoi.vn.Models;
using System.Device.Location;
using Newtonsoft.Json;
using System.Globalization;
using System.Text.RegularExpressions;
namespace khanhhoi.vn.Services
{
    public class ReportService
    {
        private Repository_khndb4 Repository;
        //private Services_khndb4_backup Repository_back;
        private Services_khndb4_backupT backupServiceT;
        private IList<tempdatesave> list_dataID = new List<tempdatesave>();
        public ReportService()
        {
            //String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SeagullDB"].ConnectionString;
            Repository = new Repository_khndb4();
            //Repository_back = new Services_khndb4_backup();
            backupServiceT = new Services_khndb4_backupT();

        }
        public UserRole sp_getUserByUserID_profile(Dictionary<String, String> param)
        {


            return Repository.ExecuteStoreProceduce<UserRole>("sp_getUserByUserID_profile",
                                                        param).FirstOrDefault(); ;
        }
        public string ConverteTime(double difference)
        {
            double timetemp = 0;
            double tempMinute = 0;
            double sodu = 0;
            if (difference >= 60)
            {
                sodu = difference % 60;
                difference = difference - sodu;
                timetemp = difference / 60;
                tempMinute = difference % 60;
                return Math.Round(timetemp) + "h" + Math.Round(tempMinute + sodu);
            }
            else
            {
                return Math.Round(difference) + " phút ";
            }
        }

        public string ConverteDateTime(DateTime datetime)
        {
            // return "Ngày " + datetime.Day + ", tháng " + datetime.Month + ", năm " + datetime.Year + ", lúc " + datetime.TimeOfDay;
            return datetime.TimeOfDay + " " + datetime.Day + "-" + datetime.Month + "-" + datetime.Year;
        }
        public List<BaoCaoHanhTrinh_Detail> ReportHanhTrinh_Detail(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            _listID = _listID + ",";
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<PauseStop> ListPauseStop = new List<PauseStop>();
            List<BaoCaoHanhTrinh_Detail> list_BaoCaoHanhTrinh_Detail = new List<BaoCaoHanhTrinh_Detail>();

            //string[] list_arr = arrImei.Split(',');
            bool flag = true;
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            int count = 0;
            string VehicleNumber = "";
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataForPauseStop> list = new List<GpsDataForPauseStop>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForReportPause_Stop(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list =
                                    Repository.ExecuteStoreProceduce<GpsDataForPauseStop>("sp_GetData_PauseStop_byIDT",
                                                                                          parameter).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<GpsDataForPauseStop> list_backup = backupServiceT.DataForReportPause_Stop(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<GpsDataForPauseStop> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataForPauseStop>("sp_GetData_PauseStop_byIDT",
                                                                                          parameter).ToList();
                                list = list_backup.Union(listNew).ToList();
                                break;
                            }
                    }

                    var parameter2 = new Dictionary<string, string>();
                    parameter2.Add("_DeviceID", _deviceID);
                    Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicle",
                                                                             parameter2).First();
                    if (device != null)
                    {
                        VehicleNumber = device.VehicleNumber;
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list = list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0).ToList();
                    }
                    if (list.FirstOrDefault() != null)
                    {
                        IList<PauseStop> ListPauseStopSingle = new List<PauseStop>();
                        bool fSpeed = false;
                        var tempstar = new DateTime();
                        string diachi = "";
                        string toado = "";
                        string thedriver = "";
                        thedriver = list[0].TheDriver;
                        DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
                                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                        GpsDataExtForGeneral dateGPS = new GpsDataExtForGeneral();
                        for (int i = 0; i < list.Count(); i++)
                        {
                            // neu chua co diem bat dau va xe dang dung
                            if (!fSpeed && list[i].Speed > 0)
                            {
                                tempstar = list[i].DateSave;
                                toado = list[i].Latitude + "," + list[i].Longitude;
                                diachi = !string.IsNullOrEmpty(list[i].Addr) ? list[i].Addr : "Undefined";
                                fSpeed = true;
                            }
                            // neu da co diem bat dau va xe bat dau chay
                            //hoac toan bo dong phia sau deu co van toc  bang 0(i == list.Count - 1)
                            else if (fSpeed && (list[i].Speed == 0 || i == list.Count - 1))
                            {
                                fSpeed = false;
                                // tinh khoang thoi gian dung
                                double difference = list[i].DateSave.Subtract(tempstar).TotalMinutes;
                                // neu thoi gian nho hon 1 phut thi quay lai vong lap
                                if (difference <= 1)
                                {
                                    continue;
                                }
                                // nguoc lai kiem tra dung hoac do
                                //neu thoi gian nghi > 15 phut la do nguoc lai la dung
                                else
                                {
                                    count++;
                                    var dungdotemp = new PauseStop();
                                    // kiem tra co thay doi tai xe khong
                                    if (list[i].TheDriver != thedriver)
                                    {
                                        drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
                                                     getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                        thedriver = list[i].TheDriver;
                                    }
                                    if (drivertemp != null)
                                    {
                                        dungdotemp.DriverLicense = drivertemp.DriverLicense;
                                        dungdotemp.NameDriver = drivertemp.NameDriver;
                                    }
                                    else
                                    {
                                        dungdotemp.NameDriver = "";
                                        dungdotemp.DriverLicense = "";
                                    }
                                    // lay thong tin
                                    // kiem tra la dung hoac do
                                    dungdotemp.Status = difference > 15 ? "Đỗ" : "Dừng";
                                    dungdotemp.DateTimeEnd = list[i].DateSave;
                                    dungdotemp.VehicleNumber = VehicleNumber;
                                    dungdotemp.count = count;
                                    dungdotemp.DateTime = ConverteDateTime(tempstar);
                                    dungdotemp.DateTimeStart = tempstar;
                                    dungdotemp.Date = tempstar;
                                    dungdotemp.Duration = ConverteTime(difference);
                                    dungdotemp.Address = diachi;
                                    dungdotemp.Coordinates = toado;
                                    dungdotemp.TypeTransportName = list[0].TypeTransportName;
                                    // add vao danh sach
                                    ListPauseStopSingle.Add(dungdotemp);
                                    diachi = "";
                                    toado = "";
                                }
                            }
                        } //end list

                        try
                        {
                            string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                            DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                    "yy-MM-dd",
                                                                                    CultureInfo.InvariantCulture));
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                        "yy-MM-dd", CultureInfo.InvariantCulture));

                            for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                            {
                                // logic here
                                bool fAdd = true;
                                foreach (PauseStop var in ListPauseStopSingle)
                                {
                                    if (DateTime.Compare(var.Date.Date, date) == 0)
                                    {
                                        fAdd = false;
                                    }
                                }
                                if (fAdd)
                                {
                                    var dungdotemp = new PauseStop();
                                    DriverC drivertt =
                                        getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                                    if (drivertt != null)
                                    {
                                        dungdotemp.NameDriver = drivertt.NameDriver;
                                        dungdotemp.DriverLicense = drivertt.DriverLicense;
                                    }
                                    else
                                    {
                                        dungdotemp.NameDriver = "";
                                        dungdotemp.DriverLicense = "";
                                    }
                                    //dungdotemp.NameDriver = "không xác định";
                                    //dungdotemp.DriverLicense = "không xác định";


                                    dungdotemp.Status = "";
                                    dungdotemp.VehicleNumber = VehicleNumber;
                                    dungdotemp.count = count;
                                    dungdotemp.DateTime = ConverteDateTime(date);
                                    dungdotemp.Date = date;
                                    dungdotemp.Duration = "0p";
                                    dungdotemp.Address = "";
                                    dungdotemp.Coordinates = "";
                                    dungdotemp.TypeTransportName = getDevivebyDeviceIDT(_deviceID).TypeTransportName;
                                    ListPauseStopSingle.Add(dungdotemp);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.GetBaseException();
                        }
                        ListPauseStopSingle = ListPauseStopSingle.OrderBy(m => m.Date).ToList();

                        foreach (PauseStop vars in ListPauseStopSingle)
                        {
                            ListPauseStop.Add(vars);
                        }
                        //CalculateDistanceForGPSDataG
                        //double CalculateDistanceForGPSDataG(IList<GpsDataExtForGeneral> list, int start, int end)

                        for (int i = 0; i < ListPauseStop.Count; i++)
                        {
                            BaoCaoHanhTrinh_Detail BaoCaoHanhTrinh_Item = new BaoCaoHanhTrinh_Detail();
                            BaoCaoHanhTrinh_Item.Driver = ListPauseStop[i].NameDriver;
                            BaoCaoHanhTrinh_Item.timeStart = ListPauseStop[i].DateTimeStart;
                            BaoCaoHanhTrinh_Item.timeEnd = ListPauseStop[i].DateTimeEnd;
                            BaoCaoHanhTrinh_Item.Duran = ListPauseStop[i].Duration;
                            BaoCaoHanhTrinh_Item.timeDrive = ListPauseStop[i].Duration;
                            List<GpsDataForPauseStop> listTemp = list.Where(m => m.DateSave >= ListPauseStop[i].DateTimeStart && m.DateSave <= ListPauseStop[i].DateTimeEnd).ToList();
                            // !string.IsNullOrEmpty(list[i].Addr) ? list[i].Addr : "Undefined";
                            if (listTemp.Count > 0)
                            {
                                BaoCaoHanhTrinh_Item.positionStart = !string.IsNullOrEmpty(listTemp[0].Addr) ? listTemp[0].Addr : "Undefined";
                                BaoCaoHanhTrinh_Item.positionEnd = !string.IsNullOrEmpty(listTemp[listTemp.Count - 1].Addr) ? listTemp[listTemp.Count - 1].Addr : "Undefined";
                                BaoCaoHanhTrinh_Item.SpeedMax = (listTemp.OrderByDescending(m => m.Speed).ToList())[0].Speed;
                                BaoCaoHanhTrinh_Item.Distane = Math.Round(CalculateDistanceForGPSDataPausestop(listTemp, 0, listTemp.Count), 2);
                                BaoCaoHanhTrinh_Item.VehicleNumber = ListPauseStop[i].VehicleNumber;
                                list_BaoCaoHanhTrinh_Detail.Add(BaoCaoHanhTrinh_Item);
                            }

                        }
                    }
                }
            }
            return list_BaoCaoHanhTrinh_Detail;
        }
        public static double CalculateDistanceForGPSDataPausestop(List<GpsDataForPauseStop> list, int start, int end)
        {
            double result2 = 0;
            if (list != null && list.Count > 0)
            {
                //      GpsDataExt tmp = list.FirstOrDefault(m => m.Latitude > 0 && m.Longitude > 0);
                double sLatitude = 0.0;
                double sLongitude = 0.0;

                sLatitude = Convert.ToDouble(list[start].Latitude);
                sLongitude = Convert.ToDouble(list[start].Longitude);


                for (int i = start; i < end; i++)
                {
                    double eLatitude = Convert.ToDouble(list[i].Latitude.Value);
                    double eLongitude = Convert.ToDouble(list[i].Longitude.Value);

                    if (sLatitude < 1 || sLongitude < 1)
                    {
                        if (eLatitude > 1 && eLongitude > 1)
                        {
                            sLatitude = eLatitude;
                            sLongitude = eLongitude;
                        }
                        continue;
                    }
                    else if (eLatitude < 1 || eLongitude < 1)
                    {
                        continue;
                    }
                    else
                    {
                        var sCood = new GeoCoordinate(sLatitude, sLongitude);
                        var eCood = new GeoCoordinate(eLatitude, eLongitude);
                        result2 += sCood.GetDistanceTo(eCood);
                        sLatitude = Convert.ToDouble(list[i].Latitude.Value);
                        sLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    }
                }
            }

            return result2 / 1000;
        }
        public bool checkDate(Dictionary<string, string> param)
        {
            string totemp = param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(3, 2);
            // string fromtemp = param.FirstOrDefault(pair => pair.Key == "From").Value;

            if ((totemp != param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(3, 2)) ||
                totemp != DateTime.Now.Month.ToString())
            {
                return false;
            }
            return true;
        }

        public int checkDateInt(Dictionary<string, string> param)
        {
            string totemp = param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(3, 2);
            string fromtemp = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(3, 2);

            // if(int.Parse(fromtemp)-1==DateTime.Now.Month-1||int.Parse(fromtemp)==0)
            string monthnow = DateTime.Now.Month.ToString();
            if (int.Parse(fromtemp) == DateTime.Now.AddMonths(-1).Month || int.Parse(fromtemp) == 0)
            {
                if (DateTime.Now.Day < 5)
                {
                    return 2;
                }
                else
                {
                    //if((totemp=="12")&&(fromtemp=="12"))
                    //{
                    //    return 2;
                    //}
                    if (monthnow.Length == 1)
                        monthnow = "0" + monthnow;

                    if ((fromtemp != totemp) && (totemp == monthnow))
                    {
                        return 3;
                    }
                    else if ((fromtemp == totemp) && (totemp == monthnow))
                    {
                        return 2;
                    }
                }
            }
            //else if (monthnow == "1" && int.Parse(fromtemp) == 12)
            //{
            //    if (DateTime.Now.Day < 5)
            //    {
            //        return 2;
            //    }
            //    else
            //    {


            //        //if((totemp=="12")&&(fromtemp=="12"))
            //        //{
            //        //    return 2;
            //        //}
            //        if (monthnow.Length == 1)
            //            monthnow = "0" + monthnow;

            //        if ((fromtemp != totemp) && (totemp == monthnow))
            //        {
            //            return 3;
            //        }
            //        else if ((fromtemp == totemp) && (totemp == monthnow))
            //        {
            //            return 2;
            //        }
            //    }
            //}
            else
            {
                //if((totemp=="12")&&(fromtemp=="12"))
                //{
                //    return 2;
                //}
                if (monthnow.Length == 1)
                    monthnow = "0" + monthnow;

                if ((fromtemp != totemp) && ((totemp == monthnow || totemp == "12")))
                {
                    return 3;
                }
                else if ((fromtemp == totemp) && (totemp == monthnow))
                {
                    return 2;
                }
            }
            return 1; // 1 la db cu; 2 la db moi; 3 la ca 2 db
        }

        public Dictionary<string, string> getParamNew(Dictionary<string, string> param)
        {
            var dateTo = new Dictionary<string, string>
                {
                    {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                };
            Int64 toID = Repository.ExecuteStoreProceduce<Int64>("sp_DataID_toDay", dateTo).First();

            var dateFrom = new Dictionary<string, string>
                {
                    {
                        "_from",
                        param.FirstOrDefault(pair => pair.Key == "From").Value
                    },
                    {
                        "IdTempTo",
                        toID.ToString()
                    },
                };
            Int64 fromID = Repository.ExecuteStoreProceduce<Int64>("sp_DataID_fromDayT", dateFrom).FirstOrDefault();

            var paramNew =
                new Dictionary<string, string>
                    {
                        {"_DeviceID", ""},
                        {"_from", fromID.ToString()},
                        {"_to", toID.ToString()}
                    };
            return paramNew;
        }

        public DriverC getDriverbyPhone(int _deviceID, string phonenumber)
        {
            Dictionary<string, string> paramdriver = null;
            paramdriver =
                JsonConvert.DeserializeObject<Dictionary<string, string>>("{'_DeviceID':'" +
                                                                          _deviceID +
                                                                          "','_PhoneDriver':'" +
                                                                          phonenumber +
                                                                          "'}");
            DriverC driverc =
                Repository.ExecuteStoreProceduce<DriverC>("sp_getDriverByPhoneDriver",
                                                          paramdriver).FirstOrDefault();
            return driverc;
        }


        public static double CalculateDistance(IList<GpsDataExt> list, int start, int end)
        {
            double result2 = 0;
            if (list != null && list.Count > 0)
            {
                //      GpsDataExt tmp = list.FirstOrDefault(m => m.Latitude > 0 && m.Longitude > 0);
                double sLatitude = 0.0;
                double sLongitude = 0.0;

                sLatitude = Convert.ToDouble(list[start].Latitude);
                sLongitude = Convert.ToDouble(list[start].Longitude);


                for (int i = start; i < end; i++)
                {
                    double eLatitude = Convert.ToDouble(list[i].Latitude.Value);
                    double eLongitude = Convert.ToDouble(list[i].Longitude.Value);

                    if (sLatitude < 1 || sLongitude < 1)
                    {
                        if (eLatitude > 1 && eLongitude > 1)
                        {
                            sLatitude = eLatitude;
                            sLongitude = eLongitude;
                        }
                        continue;
                    }
                    else if (eLatitude < 1 || eLongitude < 1)
                    {
                        continue;
                    }
                    else
                    {
                        var sCood = new GeoCoordinate(sLatitude, sLongitude);
                        var eCood = new GeoCoordinate(eLatitude, eLongitude);
                        result2 += sCood.GetDistanceTo(eCood);
                        sLatitude = Convert.ToDouble(list[i].Latitude.Value);
                        sLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    }
                }
            }


            return result2 / 1000.0;
        }
        public static double CalculateDistance_V2(IList<GpsDataExt> list, int start, int end)
        {
            double result2 = 0;
            if (list != null && list.Count > 0)
            {
                //      GpsDataExt tmp = list.FirstOrDefault(m => m.Latitude > 0 && m.Longitude > 0);
                double sLatitude = 0.0;
                double sLongitude = 0.0;
                int day = 0;
                day = list[start].DateSave.Day;
                sLatitude = Convert.ToDouble(list[start].Latitude);
                sLongitude = Convert.ToDouble(list[start].Longitude);


                for (int i = start; i < end; i++)
                {
                    double eLatitude = Convert.ToDouble(list[i].Latitude.Value);
                    double eLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    if (list[i].DateSave.Day != day)
                    {
                        if (eLatitude > 1 && eLongitude > 1)
                        {
                            sLatitude = eLatitude;
                            sLongitude = eLongitude;
                        }
                        day = list[i].DateSave.Day;
                        continue;
                    }
                    day = list[i].DateSave.Day; day = list[i].DateSave.Day;
                    if (sLatitude < 1 || sLongitude < 1)
                    {
                        if (eLatitude > 1 && eLongitude > 1)
                        {
                            sLatitude = eLatitude;
                            sLongitude = eLongitude;
                        }
                        continue;
                    }
                    else if (eLatitude < 1 || eLongitude < 1)
                    {
                        continue;
                    }
                    else
                    {
                        var sCood = new GeoCoordinate(sLatitude, sLongitude);
                        var eCood = new GeoCoordinate(eLatitude, eLongitude);
                        result2 += sCood.GetDistanceTo(eCood);
                        sLatitude = Convert.ToDouble(list[i].Latitude.Value);
                        sLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    }
                }
            }


            return result2 / 1000.0;
        }

        public static double CalculateDistance_V2_forOil(IList<Fuel> list, int start, int end)
        {
            double result2 = 0;
            if (list != null && list.Count > 0)
            {
                //      GpsDataExt tmp = list.FirstOrDefault(m => m.Latitude > 0 && m.Longitude > 0);
                double sLatitude = 0.0;
                double sLongitude = 0.0;
                int day = 0;
                day = list[start].DateSave.Day;
                sLatitude = Convert.ToDouble(list[start].Latitude);
                sLongitude = Convert.ToDouble(list[start].Longitude);


                for (int i = start; i < end; i++)
                {
                    double eLatitude = Convert.ToDouble(list[i].Latitude.Value);
                    double eLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    if (list[i].DateSave.Day != day)
                    {
                        if (eLatitude > 1 && eLongitude > 1)
                        {
                            sLatitude = eLatitude;
                            sLongitude = eLongitude;
                        }
                        day = list[i].DateSave.Day;
                        continue;
                    }
                    day = list[i].DateSave.Day; day = list[i].DateSave.Day;
                    if (sLatitude < 1 || sLongitude < 1)
                    {
                        if (eLatitude > 1 && eLongitude > 1)
                        {
                            sLatitude = eLatitude;
                            sLongitude = eLongitude;
                        }
                        continue;
                    }
                    else if (eLatitude < 1 || eLongitude < 1)
                    {
                        continue;
                    }
                    else
                    {
                        var sCood = new GeoCoordinate(sLatitude, sLongitude);
                        var eCood = new GeoCoordinate(eLatitude, eLongitude);
                        result2 += sCood.GetDistanceTo(eCood);
                        sLatitude = Convert.ToDouble(list[i].Latitude.Value);
                        sLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    }
                }
            }


            return result2 / 1000.0;
        }

        public static double CalculateDistance_(String start, String end)
        {
            double result2 = 0;
            var sCoor = new string[2];
            var eCoor = new string[2];
            if (!String.IsNullOrEmpty(start) && !String.IsNullOrEmpty(end))
            {
                int spoint = start.IndexOf(",");
                int epoint = end.IndexOf(",");
                sCoor[0] = start.Substring(0, spoint);
                sCoor[1] = start.Substring((spoint), start.Length - spoint).Replace(",", "");
                eCoor[0] = end.Substring(0, epoint);
                eCoor[1] = end.Substring(epoint, end.Length - epoint).Replace(",", "");
                var _sCood = new GeoCoordinate(double.Parse(sCoor[0]), double.Parse(sCoor[1]));
                var _eCood = new GeoCoordinate(double.Parse(eCoor[0]), double.Parse(eCoor[1]));
                result2 = _sCood.GetDistanceTo(_eCood);
            }

            return result2 / 1000;
        }

        public static double CalculateDistanceForGPSDataG(IList<GpsDataExtForGeneral> list, int start, int end)
        {
            double result2 = 0;
            if (list != null && list.Count > 0)
            {
                //      GpsDataExt tmp = list.FirstOrDefault(m => m.Latitude > 0 && m.Longitude > 0);
                double sLatitude = 0.0;
                double sLongitude = 0.0;

                sLatitude = Convert.ToDouble(list[start].Latitude);
                sLongitude = Convert.ToDouble(list[start].Longitude);


                for (int i = start; i < end; i++)
                {
                    double eLatitude = Convert.ToDouble(list[i].Latitude.Value);
                    double eLongitude = Convert.ToDouble(list[i].Longitude.Value);

                    if (sLatitude < 1 || sLongitude < 1)
                    {
                        if (eLatitude > 1 && eLongitude > 1)
                        {
                            sLatitude = eLatitude;
                            sLongitude = eLongitude;
                        }
                        continue;
                    }
                    else if (eLatitude < 1 || eLongitude < 1)
                    {
                        continue;
                    }
                    else
                    {
                        var sCood = new GeoCoordinate(sLatitude, sLongitude);
                        var eCood = new GeoCoordinate(eLatitude, eLongitude);
                        result2 += sCood.GetDistanceTo(eCood);
                        sLatitude = Convert.ToDouble(list[i].Latitude.Value);
                        sLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    }
                }
            }

            return result2 / 1000;
        }

        public static double CalculateDistanceForGPSHanhTrinh(IList<BaoCaoHanhTrinh> list, int start, int end)
        {
            double result2 = 0;
            if (list != null)
            {
                //      GpsDataExt tmp = list.FirstOrDefault(m => m.Latitude > 0 && m.Longitude > 0);
                double sLatitude = 0.0;
                double sLongitude = 0.0;

                sLatitude = Convert.ToDouble(list[start].Latitude);
                sLongitude = Convert.ToDouble(list[start].Longitude);


                for (int i = start; i <= end; i++)
                {
                    double eLatitude = Convert.ToDouble(list[i].Latitude.Value);
                    double eLongitude = Convert.ToDouble(list[i].Longitude.Value);

                    if (sLatitude < 1 || sLongitude < 1)
                    {
                        if (eLatitude > 1 && eLongitude > 1)
                        {
                            sLatitude = eLatitude;
                            sLongitude = eLongitude;
                        }
                        continue;
                    }
                    else if (eLatitude < 1 || eLongitude < 1)
                    {
                        continue;
                    }
                    else
                    {
                        var sCood = new GeoCoordinate(sLatitude, sLongitude);
                        var eCood = new GeoCoordinate(eLatitude, eLongitude);
                        result2 += sCood.GetDistanceTo(eCood);
                        sLatitude = Convert.ToDouble(list[i].Latitude.Value);
                        sLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    }
                }
            }

            return Math.Round(result2 / 1000);
        }

        public IList<Distance> ReportDistance(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            _listID = _listID + ",";
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<Distance> lisqd = new List<Distance>();

            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataExt> list = new List<GpsDataExt>();

                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForDistance(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_GetDataDistance_byID",
                                                                                    parameter).OrderBy(
                                                                                        item => item.DateSave).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<GpsDataExt> list_backup = backupServiceT.DataForDistance(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<GpsDataExt> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataExt>("sp_GetDataDistance_byID",
                                                                                 parameter).OrderBy(
                                                                                     item => item.DateSave).ToList();
                                list = list_backup.Union(listNew).ToList();
                                break;
                            }
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list =
                            list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0)
                                .ToList();
                    }
                    if (list.FirstOrDefault() != null)
                    {
                        bool flag = true;
                        DateTime startday = list[0].DateSave.Date;
                        DateTime endday = list[list.Count - 1].DateSave.Date;
                        IList<GpsDataExt> list_temp = null;
                        double quangduong = 0;
                        int v_max = 0;
                        double v_all = 0;
                        double v_tb = 0;
                        String vehicleNumber = "";
                        int sumCount = 0;

                        //list_temp= new List<GpsDataExt>();


                        vehicleNumber = list[0].VehicleNumber;
                        // quangduong += CalculateDistance_V2(list, 0, list.Count);
                        if (param["From"].Contains("00:00") && param["To"].Contains("23:59"))
                        {
                            IList<General> listTemp = ReportGeneral(param);
                            foreach (General temp in listTemp)
                            {
                                quangduong += Int16.Parse(temp.Distance);
                            }
                        }
                        else
                        {
                            quangduong += CalculateDistance_V2(list, 0, list.Count);
                        }
                        v_max = list[0].Speed.Value;
                        for (int k = 0; k < list.Count(); k++)
                        {
                            int hientai = list[k].Speed.Value;
                            if (hientai > 0)
                            {
                                sumCount++;
                                v_all += hientai;
                            }
                            if (hientai > v_max)
                            {
                                v_max = hientai;
                            }
                        }

                        if (v_all > 0)
                        {
                            v_tb = Math.Round(v_all / sumCount);
                        }
                        var dist = new Distance();
                        dist.VehicleNumber = vehicleNumber;
                        dist.SpeedMax = v_max + " km/h";
                        dist.Distances = Math.Round(quangduong).ToString();
                        dist.SpeedAVG = v_tb + " km/h";
                        dist.TotalFuel = "0";
                        dist.Date_from = list[0].DateSave.ToString("dd-MM-yyy hh:mm:ss");
                        dist.Date_to = list[list.Count - 1].DateSave.ToString("dd-MM-yyy hh:mm:ss"); ;
                        lisqd.Add(dist);
                    }
                    else
                    {
                        var dist = new Distance();

                        dist.VehicleNumber = getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;

                        dist.SpeedMax = "0 km/h";
                        dist.Distances = "0";
                        dist.SpeedAVG = "0 km/h";
                        dist.TotalFuel = "0";
                        dist.Date_from = dateFrom.ToString("dd-MM-yyy hh:mm:ss"); ;
                        dist.Date_to = dateTo.ToString("dd-MM-yyy hh:mm:ss"); ;
                        lisqd.Add(dist);
                    }
                }
            }

            return lisqd;
        }
        public IList<TimeDriver> ReportKm(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<TimeDriver> listKM = new List<TimeDriver>();

            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<TimeDriver> listKMSingle = new List<TimeDriver>();
                    IList<GpsDataExt> list = new List<GpsDataExt>();
                    IList<GpsDataExt> list_tong = new List<GpsDataExt>();

                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
                                paramOld["_DeviceID"] = _deviceID;
                                list = backupServiceT.DataForDistance(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = getParamNew(param);
                                parameter["_DeviceID"] = _deviceID;
                                list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_GetDataDistance_byID",parameter)
                                    .OrderBy(item => item.DateSave).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
                                paramOld["_DeviceID"] = _deviceID;
                                IList<GpsDataExt> list_backup = backupServiceT.DataForDistance(paramOld);
                                Dictionary<string, string> parameter = getParamNew(param);
                                parameter["_DeviceID"] = _deviceID;
                                IList<GpsDataExt> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataExt>("sp_GetDataDistance_byID", parameter).OrderBy(item => item.DateSave).ToList();
                                list = list_backup.Union(listNew).ToList();
                                break;
                            }
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list_tong = list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0).ToList();
                    }
                    if (list_tong.FirstOrDefault() != null)
                    {
                        for (DateTime vb = dateFrom; vb <= dateTo; vb = vb.AddDays(1))
                        {
                            list = list_tong.Where(m => m.DateSave.Date.CompareTo(vb) == 0).ToList();
                            if (list.FirstOrDefault() != null && list.Count > 1)
                            {
                                bool flag = true;
                                DateTime startday = list[0].DateSave.Date;
                                DateTime endday = list[list.Count - 1].DateSave.Date;
                                IList<GpsDataExt> list_temp = null;
                                double quangduong = 0;
                                int v_max = 0;
                                double v_all = 0;
                                double v_tb = 0;
                                String vehicleNumber = "";
                                int sumCount = 0;
                                vehicleNumber = list[0].VehicleNumber;
                                v_max = list[0].Speed.Value;
                                int dayCurrent = list[0].DateSave.Day;
                                int kCurrent = 0;
                                for (int k = 0; k < list.Count(); k++)
                                {//dist.Distances = Math.Round(quangduong).ToString();
                                    if (dayCurrent != list[k].DateSave.Day || k == list.Count - 1)
                                    {
                                        // quangduong = CalculateDistance_V2(list, kCurrent, k);
                                        if (dateTo.Date.Subtract(dateFrom.Date).TotalDays == 0)
                                        {
                                            if (param["From"].Contains("00:00") && param["To"].Contains("23:59"))
                                            {
                                                IList<General> listTemp = ReportGeneral(param);
                                                foreach (General temp in listTemp)
                                                {
                                                    quangduong += Int16.Parse(temp.Distance);
                                                }
                                            }
                                            else
                                            {
                                                quangduong = CalculateDistance_V2(list, kCurrent, k + 1);
                                            }
                                        }
                                        else
                                        {
                                            if (param["From"].Contains("00:00") && param["To"].Contains("23:59"))
                                            {
                                                Dictionary<String, String> paramTemp = new Dictionary<string, string>();
                                                paramTemp["_DeviceID"] = _deviceID;
                                                paramTemp["From"] = vb.ToString("yy-MM-dd 00:00");
                                                paramTemp["To"] = vb.ToString("yy-MM-dd 23:59");
                                                IList<General> listTemp = ReportGeneral(paramTemp);
                                                foreach (General temp in listTemp)
                                                {
                                                    quangduong += Int16.Parse(temp.Distance);
                                                }
                                            }
                                            else
                                            {
                                                //if (param["From"].Contains("00:00") && param["To"].Contains("23:59"))
                                                if ((vb.Date.Subtract(dateFrom.Date).TotalDays) == 0 && !param["From"].Contains("00:00"))
                                                {
                                                    quangduong = CalculateDistance_V2(list, kCurrent, k + 1);
                                                }
                                                else if ((vb.Date.Subtract(dateTo.Date).TotalDays) == 0 && !param["To"].Contains("23:59"))
                                                {
                                                    quangduong = CalculateDistance_V2(list, kCurrent, k + 1);
                                                }
                                                else
                                                {
                                                    Dictionary<String, String> paramTemp = new Dictionary<string, string>();
                                                    paramTemp["IDs"] = _deviceID;
                                                    paramTemp["From"] = vb.ToString("yy-MM-dd 00:00");
                                                    paramTemp["To"] = vb.ToString("yy-MM-dd 23:59");
                                                    IList<General> listTemp = ReportGeneral(paramTemp);
                                                    foreach (General temp in listTemp)
                                                    {
                                                        quangduong += Int16.Parse(temp.Distance);
                                                    }
                                                }

                                            }
                                        }
                                        var laixetemp = new TimeDriver();
                                        laixetemp.Date = list[k].DateSave.ToShortDateString();
                                        laixetemp.date2 = DateTime.Parse(list[k].DateSave.ToShortDateString());
                                        laixetemp.VehicleNumber = vehicleNumber;
                                        laixetemp.TimeStart = list[kCurrent].DateSave.TimeOfDay.ToString();
                                        laixetemp.TimeEnd = list[k].DateSave.TimeOfDay.ToString();
                                        laixetemp.AddressStart = list[kCurrent].Address;
                                        laixetemp.AddressEnd = list[k].Address;
                                        laixetemp.Distance = Math.Round(quangduong).ToString() + " km";
                                        //laixetemp.NameDriver = drivertemp.NameDriver;
                                        //laixetemp.theDriver = bThedriver ? list[i - 1].TheDriver : list[i].TheDriver;
                                        //laixetemp.DriverLicense = drivertemp.DriverLicense;
                                        listKMSingle.Add(laixetemp);
                                        quangduong = 0;
                                        dayCurrent = list[k].DateSave.Day;
                                        kCurrent = k;
                                    }
                                }
                            }
                            else
                            {
                                #region "Truong hop khong co data"





















                                try
                                {











                                    // logic here


                                    var laixetemp = new TimeDriver();
                                    laixetemp.TimeDriver_ = "0p";
                                    // laixetemp.count = count;
                                    laixetemp.Date = vb.Date.ToString("dd-MM-yyyy");
                                    laixetemp.date2 = vb.Date;
                                    laixetemp.VehicleNumber =
                                        getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;
                                    laixetemp.TimeStart = "";
                                    laixetemp.TimeEnd = "";
                                    laixetemp.AddressStart = "";
                                    laixetemp.CoordinatesStart = "";
                                    laixetemp.AddressEnd = "";
                                    laixetemp.CoordinatesEnd = "";
                                    laixetemp.SpeedAVG = "0 km/h";
                                    laixetemp.SpeedMax = "0 km/h";
                                    laixetemp.Distance = "0 km";
                                    laixetemp.stimedriver = 0;
                                    laixetemp.sDistance = 0;
                                    laixetemp.TypeTransportName = getDevivebyDeviceIDT(_deviceID).TypeTransportName;
                                    //DriverC drivertt =
                                    //    getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                                    //if (drivertt != null)
                                    //{
                                    //    laixetemp.NameDriver = drivertt.NameDriver;
                                    //    laixetemp.DriverLicense = drivertt.DriverLicense;
                                    //}
                                    //else
                                    //{
                                    //    laixetemp.NameDriver = "";
                                    //    laixetemp.DriverLicense = "";
                                    //}

                                    listKMSingle.Add(laixetemp);



                                }
                                catch (Exception ex)
                                {
                                    ex.GetBaseException();
                                }

                                #endregion
                            }
                        }
                    }
                    //Add data vao danh sach
                    listKMSingle = listKMSingle.OrderBy(m => m.date2).ToList();
                    foreach (TimeDriver vars in listKMSingle)
                    {
                        listKM.Add(vars);
                    }
                }
            }
            return listKM;
        }

        private IList<Distance> ReportDistancebyday(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<Distance> lisqd = new List<Distance>();
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataExt> list = new List<GpsDataExt>();

                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
                                paramOld["_DeviceID"] = _deviceID;
                                list = backupServiceT.DataForDistance(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = getParamNew(param);
                                parameter["_DeviceID"] = _deviceID;
                                list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_GetDataDistance_byID",
                                                                                    parameter).OrderBy(
                                                                                        item => item.DateSave).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
                                paramOld["_DeviceID"] = _deviceID;
                                list = backupServiceT.DataForDistance(paramOld);
                                if (list != null && list.Count > 0)
                                {
                                    string monthNow = list.LastOrDefault().DateSave.Month.ToString();
                                    if (monthNow.Length == 1)
                                    {
                                        monthNow = "0" + monthNow;
                                    }

                                    param["From"] = param["To"].Substring(0, 2) + "-" + monthNow + "-" + "01 " +
                                                    param["From"].Substring(9);
                                }
                                else
                                {
                                    string monthNow = DateTime.Now.Month.ToString();
                                    if (monthNow.Length == 1)
                                    {
                                        monthNow = "0" + monthNow;
                                    }
                                    param["From"] = param["To"].Substring(0, 2) + "-" + param["To"].Substring(3, 2) +
                                                    "-" + "01 " +
                                                    param["From"].Substring(9);
                                }
                                Dictionary<string, string> parameter = getParamNew(param);
                                parameter["_DeviceID"] = _deviceID;
                                IList<GpsDataExt> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataExt>("sp_GetDataDistance_byID",
                                                                                 parameter).OrderBy(
                                                                                     item => item.DateSave).ToList();
                                foreach (GpsDataExt gpsDataExt in listNew)
                                {
                                    list.Add(gpsDataExt);
                                }
                                break;
                            }
                    }

                    if (list.FirstOrDefault() != null)
                    {
                        DateTime startDay = list[0].DateSave.Date;
                        DateTime endDay = list[list.Count - 1].DateSave.Date;

                        for (DateTime i = startDay; i <= endDay; i.AddDays(1))
                        {
                            IList<GpsDataExt> listtemp = list.Where(m => m.DateSave.Date == i).ToList();
                            for (int k = 0; k < listtemp.Count; k++)
                            {
                            }
                        }
                        double quangduong = CalculateDistance(list, 0, list.Count);
                    }
                    else
                    {
                        var dist = new Distance();
                        dist.VehicleNumber = getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;
                        dist.Distances = "0";
                        lisqd.Add(dist);
                    }
                }
            }
            return lisqd;
        }

        public DriverC getDriverFirst(string param)
        {
            Dictionary<string, string> parameter = null;
            if (param != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(param);
            }
            DriverC driver =
                Repository.ExecuteStoreProceduce<DriverC>("sp_getDriverFirstByDeviceID", parameter).FirstOrDefault();
            return driver;
        }

        public Device getVehicleByDeviceID(string param)
        {
            Dictionary<string, string> parameter = null;
            if (param != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(param);
            }
            Device device =
                Repository.ExecuteStoreProceduce<Device>("sp_getVehicleByDeviceID", parameter).FirstOrDefault();
            return device;
        }

        public IList<ExceedingSpeed> CalExceedingSpeed(IList<GpsDataExtForGeneral> list, int speedLimit)
        {

            IList<ExceedingSpeed> listExc = new List<ExceedingSpeed>();
            int SpeedLimit = 0;
            int _deviceID = list[0].DeviceID;
            if (list.FirstOrDefault() != null)
            {
                IList<ExceedingSpeed> listExcSingle = new List<ExceedingSpeed>();
                int count = 0;
                bool flag = true;
                bool flag2 = false;
                long dataIDStart = 0, dataIDEnd = 0;
                int temp1 = 0;
                int temp2 = 0;
                var ngay = new DateTime();
                string vantocbatdau = "";
                string diadiem = "";
                string toado = "", toado_ketthuc = "";
                string vantocketthuc = "";
                var tempstar = new DateTime();
                var tempend = new DateTime();
                //    DateTime dateStartSpeed = new DateTime();
                string thedriver = "";


                SpeedLimit = speedLimit;

                var startDay = new DateTime();
                var endday = new DateTime();
                startDay = list[0].DateSave.Value.Date;
                endday = list[list.Count - 1].DateSave.Value.Date;
                for (DateTime k = startDay; k <= endday; k += TimeSpan.FromDays(1))
                {
                    IList<GpsDataExtForGeneral> list_ =
                        list.Where(
                            m =>
                            m.DateSave.Value.Date.Year == k.Year && m.DateSave.Value.Date.Month == k.Month &&
                            m.DateSave.Value.Date.Day == k.Day).ToList();
                    list_ = list_.OrderBy(m => m.DateSave).ToList();

                    int countV = 0, Vtrungbinh = 0, Vtong = 0;

                    for (int i = 0; i < list_.Count; i++)
                    {
                        if (list_[i].Speed.Value > SpeedLimit)
                        {
                            if (flag)
                            {
                                if (!string.IsNullOrEmpty(list_[i].Address))
                                {
                                    //if (list[i].Address.Contains("Tp.HCM - Trung Lương"))
                                    //{
                                    //    diadiem = "";
                                    //    continue;
                                    //}
                                    diadiem = list_[i].Address;
                                }
                                else
                                {
                                    diadiem = "Undefined";
                                }

                                temp1 = i;
                                count += 1;
                                ngay = list_[i].DateSave.Value;
                                tempstar = list_[i].DateSave.Value;
                                vantocbatdau = list_[i].Speed.Value.ToString();
                                dataIDStart = list_[i].DataID.Value;
                                if (countV != 0)
                                {
                                    Vtrungbinh = Vtong / countV;

                                    countV = 0;
                                    Vtong = 0;
                                }
                                else
                                {
                                    Vtrungbinh = 0;
                                }

                                toado = list_[i].Latitude + "," + list_[i].Longitude;
                                flag = false;
                                flag2 = false;
                            }
                        }
                        else if (list_[i].Speed.Value <= SpeedLimit)
                        {
                            countV += 1;
                            Vtong += list_[i].Speed.Value;

                            if (!flag)
                            {
                                if (list_[i].DateSave.Value.Subtract(tempstar).TotalSeconds > 60)
                                {
                                    if (list_[i].DateSave.Value.Subtract(list_[i - 1].DateSave.Value).TotalMinutes < 10)
                                    {
                                        temp2 = i;
                                        tempend = DateTime.Parse(list_[i].DateSave.ToString());
                                        vantocketthuc = list_[i].Speed.Value.ToString() + " km/h";
                                        toado_ketthuc = list_[i].Latitude + "," + list_[i].Longitude;
                                        dataIDEnd = list_[i].DataID.Value;
                                        // countV = 0;
                                        //Vtong = 0;
                                    }
                                    else
                                    {
                                        temp2 = i;
                                        tempend = DateTime.Parse(list_[i - 1].DateSave.ToString());
                                        vantocketthuc = list_[i - 1].Speed.Value.ToString() + " km/h";
                                        toado_ketthuc = list_[i - 1].Latitude + "," + list_[i - 1].Longitude;
                                        dataIDEnd = list_[i - 1].DataID.Value;
                                        countV = 0;
                                        Vtong = 0;
                                    }
                                    flag2 = true;
                                }

                                flag = true;
                            }
                        }
                        if (flag2)
                        {
                            var vuottoctemp = new ExceedingSpeed();
                            string thoiluong = null;
                            int max = list_[temp1].Speed.Value;
                            for (int K = temp1 + 1; K < temp2 + 1; K++)
                            {
                                if (max < list_[K].Speed.Value)
                                {
                                    max = list_[K].Speed.Value;
                                }
                            }
                            vuottoctemp.count = count;
                            vuottoctemp.VehicleNumber = list_[0].VehicleNumber;
                            vuottoctemp.Date = ngay;
                            vuottoctemp.TocDoTrungBinh = Vtrungbinh.ToString();
                            vuottoctemp.SpeedStart = vantocbatdau;
                            vuottoctemp.Address = diadiem;
                            vuottoctemp.Coordinates = toado;
                            vuottoctemp.SpeedEnd = vantocketthuc;
                            vuottoctemp.SpeedMax = max.ToString();
                            vuottoctemp.Duration = ConverteTime(tempend.Subtract(tempstar).TotalMinutes);
                            vuottoctemp.TimeStart = tempstar.TimeOfDay.ToString();
                            vuottoctemp.TimeEnd = tempend.TimeOfDay.ToString();
                            vuottoctemp.SpeedLimit = SpeedLimit.ToString();
                            vuottoctemp.TypeTransportName = list_[0].TypeTransportName;
                            vuottoctemp.Coordinates_ketthuc = toado_ketthuc;
                            vuottoctemp.dataIDStart = dataIDStart;
                            vuottoctemp.dataIDEnd = dataIDEnd;
                            listExcSingle.Add(vuottoctemp);
                            diadiem = "";
                            flag = true;
                            flag2 = false;
                        }
                    }
                }

                listExcSingle = listExcSingle.OrderBy(m => m.Date).ToList();

                foreach (ExceedingSpeed var in listExcSingle)
                {
                    listExc.Add(var);
                }
            }


            return listExc;
        }

        public IList<ExceedingSpeed> ReportExceedingSpeed(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            int SpeedLimit = 80;
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<ExceedingSpeed> listExc = new List<ExceedingSpeed>();

            //string[] list_arr = arrDevices.Split(',');
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataExt> list = new List<GpsDataExt>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForExceedingSpeed(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byIDT",
                                                                                    parameter).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<GpsDataExt> list_backup = backupServiceT.DataForExceedingSpeed(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<GpsDataExt> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byIDT",
                                                                                 parameter).ToList();
                                // foreach (GpsDataExt gpsDataExt in listNew)
                                //{
                                list = list_backup.Union(listNew).ToList();
                                //}
                                break;
                            }
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list =
                            list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0)
                                .ToList();
                    }
                    if (list.FirstOrDefault() != null)
                    {
                        IList<ExceedingSpeed> listExcSingle = new List<ExceedingSpeed>();

                        var ngay = new DateTime();
                        string vantocbatdau = "";
                        string diadiem = "";
                        string toado = "", toado_ketthuc = "";
                        string vantocketthuc = "";
                        var tempstar = new DateTime();
                        var tempend = new DateTime();
                        //    DateTime dateStartSpeed = new DateTime();
                        string thedriver = "";
                        DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
                                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                        SpeedLimit = list.FirstOrDefault().SpeedLimit.Value;

                        DateTime startDay = list[0].DateSave.Date;
                        DateTime endday = list[list.Count - 1].DateSave.Date;
                        for (DateTime k = startDay; k <= endday; k = k.AddDays(1))
                        {
                            int count = 0;
                            bool flag = true;
                            bool flag2 = false;
                            //Dictionary<string, long> data_id = getDataIDByDate(k);
                            //IList<GpsDataExt> list_ =
                            //         list.Where(m => m.DataID.Value >= data_id["from"] && m.DataID.Value <= data_id["to"]).
                            //             ToList();
                            IList<GpsDataExt> list_ =
                                list.Where(m => m.DateSave.Date.CompareTo(k.Date) == 0).ToList();
                            if (list_.Count > 0)
                            {
                                List<GpsDataExt> deleteItem = list.ToList();
                                deleteItem.RemoveAll(item => item.DateSave.Date.CompareTo(list_[0].DateSave.Date) == 0);
                                list = deleteItem;
                                list_ = list_.OrderBy(m => m.DateSave).ToList();
                                //list.Remove
                                int countV = 0, Vtrungbinh = 0, Vtong = 0;

                                for (int i = 0; i < list_.Count; i++)
                                {

                                    if (list_[i].Speed.Value > SpeedLimit)
                                    {
                                        countV += 1;
                                        Vtong += list_[i].Speed.Value;
                                        if (flag)
                                        {
                                            if (!string.IsNullOrEmpty(list_[i].Address))
                                            {
                                                //if (list[i].Address.Contains("Tp.HCM - Trung Lương"))
                                                //{
                                                //    diadiem = "";
                                                //    continue;
                                                //}
                                                diadiem = list_[i].Address;
                                            }
                                            else
                                            {
                                                diadiem = "Undefined";
                                            }

                                            // temp1 = i;
                                            count += 1;
                                            ngay = list_[i].DateSave;
                                            tempstar = list_[i].DateSave;
                                            vantocbatdau = list_[i].Speed.Value.ToString() + " km/h";


                                            toado = list_[i].Latitude + "," + list_[i].Longitude;
                                            flag = false;
                                            flag2 = false;
                                        }
                                    }
                                    else if (list_[i].Speed.Value <= SpeedLimit)
                                    {
                                        if (!flag)
                                        {
                                            if (list_[i].DateSave.Subtract(tempstar).TotalSeconds > 60)
                                            {
                                                if (list_[i].DateSave.Subtract(list_[i - 1].DateSave).TotalMinutes < 10)
                                                {

                                                    tempend = DateTime.Parse(list_[i].DateSave.ToString());
                                                    vantocketthuc = list_[i].Speed.Value.ToString() + " km/h";
                                                    toado_ketthuc = list_[i].Latitude + "," + list_[i].Longitude;
                                                    // countV = 0;
                                                    //Vtong = 0;
                                                }
                                                else
                                                {

                                                    tempend = DateTime.Parse(list_[i - 1].DateSave.ToString());
                                                    vantocketthuc = list_[i - 1].Speed.Value.ToString() + " km/h";
                                                    toado_ketthuc = list_[i - 1].Latitude + "," + list_[i - 1].Longitude;

                                                }
                                                flag2 = true;
                                            }

                                            flag = true;
                                            if (countV != 0)
                                            {
                                                Vtrungbinh = Vtong / countV;
                                                countV = 0;
                                                Vtong = 0;
                                            }
                                            else
                                            {
                                                Vtrungbinh = 0;
                                            }
                                        }
                                    }
                                    if (flag2)
                                    {
                                        var vuottoctemp = new ExceedingSpeed();
                                        string thoiluong = null;


                                        if (list_[0].TheDriver != thedriver)
                                        {
                                            vuottoctemp.NameDriver = "";
                                            vuottoctemp.DriverLicense = "";
                                            drivertemp = getDriverbyPhone(list_[i].DeviceID, list_[i].TheDriver);
                                            if (drivertemp == null)
                                            {
                                                drivertemp = getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                            }
                                            if (drivertemp != null)
                                            {
                                                vuottoctemp.NameDriver = drivertemp.NameDriver;
                                                vuottoctemp.DriverLicense = drivertemp.DriverLicense;
                                            }
                                        }

                                        vuottoctemp.count = count;
                                        vuottoctemp.VehicleNumber = list_[0].VehicleNumber;
                                        vuottoctemp.Date = ngay;
                                        vuottoctemp.TocDoTrungBinh = Vtrungbinh.ToString() + " km/h";
                                        vuottoctemp.SpeedStart = vantocbatdau;
                                        vuottoctemp.Address = diadiem;
                                        vuottoctemp.Coordinates = toado;
                                        vuottoctemp.SpeedEnd = vantocketthuc;
                                        //vuottoctemp.SpeedMax = max + " km/h";
                                        vuottoctemp.Duration = ConverteTime(tempend.Subtract(tempstar).TotalMinutes);
                                        vuottoctemp.TimeStart = tempstar.TimeOfDay.ToString();
                                        vuottoctemp.TimeEnd = tempend.TimeOfDay.ToString();
                                        vuottoctemp.SpeedLimit = SpeedLimit.ToString();
                                        vuottoctemp.TypeTransportName = list_[0].TypeTransportName;
                                        vuottoctemp.Coordinates_ketthuc = toado_ketthuc;
                                        listExcSingle.Add(vuottoctemp);
                                        Vtrungbinh = 0;
                                        diadiem = "";
                                        flag = true;
                                        flag2 = false;
                                    }
                                }
                            }

                        }
                        try
                        {
                            string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                            DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                    "yy-MM-dd",
                                                                                    CultureInfo.InvariantCulture));
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                        "yy-MM-dd", CultureInfo.InvariantCulture));

                            for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                            {
                                // logic here
                                bool fAdd = true;
                                foreach (ExceedingSpeed var in listExcSingle)
                                {
                                    if (DateTime.Compare(var.Date.Date, date) == 0)
                                    {
                                        fAdd = false;
                                    }
                                }
                                if (fAdd)
                                {
                                    drivertemp = getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");


                                    var vuottoctemp = new ExceedingSpeed();
                                    if (drivertemp != null)
                                    {
                                        vuottoctemp.NameDriver = drivertemp.NameDriver;
                                        vuottoctemp.DriverLicense = drivertemp.DriverLicense;
                                    }
                                    else
                                    {
                                        vuottoctemp.NameDriver = "";
                                        vuottoctemp.DriverLicense = "";
                                    }
                                    vuottoctemp.VehicleNumber = list[0].VehicleNumber;
                                    vuottoctemp.Date = date;
                                    vuottoctemp.SpeedStart = "0 km/h";
                                    vuottoctemp.Address = "";
                                    vuottoctemp.Coordinates = "";
                                    vuottoctemp.Coordinates_ketthuc = toado_ketthuc;
                                    vuottoctemp.SpeedEnd = "0 km/h";
                                    vuottoctemp.SpeedMax = "0 km/h";
                                    vuottoctemp.Duration = "0p";
                                    vuottoctemp.TimeStart = "";
                                    vuottoctemp.TocDoTrungBinh = "0 km/h";
                                    vuottoctemp.TimeEnd = "";
                                    vuottoctemp.SpeedLimit = SpeedLimit.ToString();
                                    vuottoctemp.TypeTransportName = getDevivebyDeviceIDT(_deviceID).TypeTransportName;
                                    listExcSingle.Add(vuottoctemp);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.GetBaseException();
                        }
                        listExcSingle = listExcSingle.OrderBy(m => m.Date).ToList();

                        foreach (ExceedingSpeed var in listExcSingle)
                        {
                            listExc.Add(var);
                        }
                    }
                }
            }
            return listExc;
        }

        public IList<ExceedingSpeed> ReportExceedingSpeedDetails(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            int SpeedLimit = 80;
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<ExceedingSpeed> listExc = new List<ExceedingSpeed>();

            //string[] list_arr = arrDevices.Split(',');
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataExt> list = new List<GpsDataExt>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForExceedingSpeed(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byIDT",
                                                                                    parameter).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<GpsDataExt> list_backup = backupServiceT.DataForExceedingSpeed(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<GpsDataExt> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byIDT",
                                                                                 parameter).ToList();
                                // foreach (GpsDataExt gpsDataExt in listNew)
                                //{
                                list = list_backup.Union(listNew).ToList();
                                //}
                                break;
                            }
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list =
                            list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0)
                                .ToList();
                    }
                    if (list.FirstOrDefault() != null)
                    {
                        IList<ExceedingSpeed> listExcSingle = new List<ExceedingSpeed>();

                        var ngay = new DateTime();
                        string vantocbatdau = "";
                        string diadiem = "";
                        string diadiemketthuc = "";
                        string toado = "", toado_ketthuc = "";
                        string vantocketthuc = "";
                        var tempstar = new DateTime();
                        var tempend = new DateTime();
                        //    DateTime dateStartSpeed = new DateTime();
                        string thedriver = "";
                        DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
                                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                        SpeedLimit = list.FirstOrDefault().SpeedLimit.Value;

                        DateTime startDay = list[0].DateSave.Date;
                        DateTime endday = list[list.Count - 1].DateSave.Date;
                        for (DateTime k = startDay; k <= endday; k = k.AddDays(1))
                        {
                            int count = 0;
                            bool flag = true;
                            bool flag2 = false;
                            //Dictionary<string, long> data_id = getDataIDByDate(k);
                            //IList<GpsDataExt> list_ =
                            //         list.Where(m => m.DataID.Value >= data_id["from"] && m.DataID.Value <= data_id["to"]).
                            //             ToList();
                            IList<GpsDataExt> list_ =
                                list.Where(m => m.DateSave.Date.CompareTo(k.Date) == 0).ToList();
                            if (list_.Count > 0)
                            {
                                List<GpsDataExt> deleteItem = list.ToList();
                                deleteItem.RemoveAll(item => item.DateSave.Date.CompareTo(list_[0].DateSave.Date) == 0);
                                list = deleteItem;
                                list_ = list_.OrderBy(m => m.DateSave).ToList();
                                //list.Remove
                                int countV = 0, Vtrungbinh = 0, Vtong = 0;
                                int iStart = 0, iEnd = 0;
                                for (int i = 0; i < list_.Count; i++)
                                {

                                    if (list_[i].Speed.Value > SpeedLimit)
                                    {
                                        countV += 1;
                                        Vtong += list_[i].Speed.Value;
                                        if (flag)
                                        {
                                            if (!string.IsNullOrEmpty(list_[i].Address))
                                            {
                                                //if (list[i].Address.Contains("Tp.HCM - Trung Lương"))
                                                //{
                                                //    diadiem = "";
                                                //    continue;
                                                //}
                                                diadiem = list_[i].Address;
                                            }
                                            else
                                            {
                                                diadiem = "Undefined";
                                            }

                                            // temp1 = i;
                                            count += 1;
                                            ngay = list_[i].DateSave;
                                            tempstar = list_[i].DateSave;
                                            iStart = i;
                                            vantocbatdau = list_[i].Speed.Value.ToString() + " km/h";


                                            toado = list_[i].Latitude + "," + list_[i].Longitude;
                                            flag = false;
                                            flag2 = false;
                                        }
                                    }
                                    else if (list_[i].Speed.Value <= SpeedLimit)
                                    {
                                        if (!flag)
                                        {
                                            if (list_[i].DateSave.Subtract(tempstar).TotalSeconds > 60)
                                            {
                                                if (list_[i].DateSave.Subtract(list_[i - 1].DateSave).TotalMinutes < 10)
                                                {

                                                    tempend = DateTime.Parse(list_[i].DateSave.ToString());
                                                    vantocketthuc = list_[i].Speed.Value.ToString() + " km/h";
                                                    toado_ketthuc = list_[i].Latitude + "," + list_[i].Longitude;
                                                    diadiemketthuc = list_[i].Address;
                                                    iEnd = i;
                                                    // countV = 0;
                                                    //Vtong = 0;
                                                }
                                                else
                                                {

                                                    tempend = DateTime.Parse(list_[i - 1].DateSave.ToString());
                                                    vantocketthuc = list_[i - 1].Speed.Value.ToString() + " km/h";
                                                    toado_ketthuc = list_[i - 1].Latitude + "," + list_[i - 1].Longitude;
                                                    diadiemketthuc = list_[i].Address;
                                                    iEnd = i - 1;
                                                }
                                                flag2 = true;
                                            }

                                            flag = true;
                                            if (countV != 0)
                                            {
                                                Vtrungbinh = Vtong / countV;
                                                countV = 0;
                                                Vtong = 0;
                                            }
                                            else
                                            {
                                                Vtrungbinh = 0;
                                            }
                                        }
                                    }
                                    if (flag2)
                                    {
                                        var vuottoctemp = new ExceedingSpeed();
                                        string thoiluong = null;


                                        //if (list_[0].TheDriver != thedriver)
                                        //{
                                        //    vuottoctemp.NameDriver = "";
                                        //    vuottoctemp.DriverLicense = "";
                                        //    drivertemp = getDriverbyPhone(list_[i].DeviceID, list_[i].TheDriver);
                                        //    if (drivertemp == null)
                                        //    {
                                        //        drivertemp = getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                        //    }
                                        //    if (drivertemp != null)
                                        //    {
                                        //        vuottoctemp.NameDriver = drivertemp.NameDriver;
                                        //        vuottoctemp.DriverLicense = drivertemp.DriverLicense;
                                        //    }
                                        //}

                                        vuottoctemp.count = count;
                                        vuottoctemp.VehicleNumber = list_[0].VehicleNumber;
                                        vuottoctemp.Date = ngay;
                                        vuottoctemp.TocDoTrungBinh = Vtrungbinh.ToString() + " km/h";
                                        vuottoctemp.SpeedStart = vantocbatdau;
                                        vuottoctemp.Address = diadiem;
                                        vuottoctemp.AddressEnd = diadiemketthuc;
                                        vuottoctemp.Coordinates = toado;
                                        vuottoctemp.SpeedEnd = vantocketthuc;
                                        //vuottoctemp.SpeedMax = max + " km/h";
                                        vuottoctemp.Duration = ConverteTime(tempend.Subtract(tempstar).TotalMinutes);
                                        vuottoctemp.Distance = Math.Round(CalculateDistance(list_, iStart, iEnd));
                                        vuottoctemp.TimeStart = tempstar.TimeOfDay.ToString();
                                        vuottoctemp.TimeEnd = tempend.TimeOfDay.ToString();
                                        vuottoctemp.SpeedLimit = SpeedLimit.ToString();
                                        vuottoctemp.TypeTransportName = list_[0].TypeTransportName;
                                        vuottoctemp.Coordinates_ketthuc = toado_ketthuc;
                                        listExcSingle.Add(vuottoctemp);
                                        Vtrungbinh = 0;
                                        diadiem = "";
                                        flag = true;
                                        flag2 = false;
                                    }
                                }
                            }

                        }
                        try
                        {
                            string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                            DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                    "yy-MM-dd",
                                                                                    CultureInfo.InvariantCulture));
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                        "yy-MM-dd", CultureInfo.InvariantCulture));

                            for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                            {
                                // logic here
                                bool fAdd = true;
                                foreach (ExceedingSpeed var in listExcSingle)
                                {
                                    if (DateTime.Compare(var.Date.Date, date) == 0)
                                    {
                                        fAdd = false;
                                    }
                                }
                                if (fAdd)
                                {
                                    drivertemp = getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");


                                    var vuottoctemp = new ExceedingSpeed();
                                    if (drivertemp != null)
                                    {
                                        vuottoctemp.NameDriver = drivertemp.NameDriver;
                                        vuottoctemp.DriverLicense = drivertemp.DriverLicense;
                                    }
                                    else
                                    {
                                        vuottoctemp.NameDriver = "";
                                        vuottoctemp.DriverLicense = "";
                                    }
                                    vuottoctemp.VehicleNumber = list[0].VehicleNumber;
                                    vuottoctemp.Date = date;
                                    vuottoctemp.SpeedStart = "0 km/h";
                                    vuottoctemp.Address = "";
                                    vuottoctemp.Coordinates = "";
                                    vuottoctemp.Coordinates_ketthuc = toado_ketthuc;
                                    vuottoctemp.SpeedEnd = "0 km/h";
                                    vuottoctemp.SpeedMax = "0 km/h";
                                    vuottoctemp.Duration = "0p";
                                    vuottoctemp.TimeStart = "";
                                    vuottoctemp.TocDoTrungBinh = "0 km/h";
                                    vuottoctemp.TimeEnd = "";
                                    vuottoctemp.SpeedLimit = SpeedLimit.ToString();
                                    vuottoctemp.TypeTransportName = getDevivebyDeviceIDT(_deviceID).TypeTransportName;
                                    listExcSingle.Add(vuottoctemp);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.GetBaseException();
                        }
                        listExcSingle = listExcSingle.OrderBy(m => m.Date).ToList();

                        foreach (ExceedingSpeed var in listExcSingle)
                        {
                            listExc.Add(var);
                        }
                    }
                }
            }
            return listExc;
        }

        public dynamic ReportVanTocVanHanh(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<BaoCaoHanhTrinh> list_hanhtrinh = new List<BaoCaoHanhTrinh>();
            DeviceService dvService = new DeviceService();
            //dvService.LoTrinh();
            // Key = "_DeviceID"
            param.Add("_DeviceID", "");
            for (int t = 0; t < _arrIDs.Length; t++)
            {
                try
                {
                    string _DeviceID = _arrIDs[t];
                    param["_DeviceID"] = _DeviceID;
                    IList<DeviceStatus> listrs = new List<DeviceStatus>();
                    listrs = dvService.LoTrinh(param);
                    //listrs = dvService.LoTrinh(param);
                    //IList<BaoCaoHanhTrinh> hanhTrinhs = new List<BaoCaoHanhTrinh>();
                    for (int i = 0; i < listrs.Count; i++)
                    {
                        BaoCaoHanhTrinh temBaoCaoHanhTrinh = new BaoCaoHanhTrinh();
                        temBaoCaoHanhTrinh.VehicleNumber = listrs[i].VehicleNumber;
                        temBaoCaoHanhTrinh.DateSave = listrs[i].DateSave;
                        temBaoCaoHanhTrinh.Speed = listrs[i].Speed;
                        temBaoCaoHanhTrinh.Addr = listrs[i].Addr;
                        if (listrs[i].Speed == 0)
                        {
                            var sttkey = listrs[i].StatusKey;
                            var switch_ = listrs[i].Switch_;
                            if (switch_ == 1)
                                sttkey = sttkey == 0 ? 1 : 0;
                            if (sttkey == 0)
                                temBaoCaoHanhTrinh.Description = "Đỗ";
                            else
                                temBaoCaoHanhTrinh.Description = "Dừng";
                        }
                        list_hanhtrinh.Add(temBaoCaoHanhTrinh);

                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Sai kiểu dữ liệu: " + e.Message);
                }

                //           header: 'Biển số',
                //    sortable: true,
                //    dataIndex: 'VehicleNumber'
                //}, {
                //    header: 'Thời điểm',
                //    sortable: true,
                //    dataIndex: 'DateSave',
                //}, {
                //    header: 'Các tốc độ (km/h)',
                //    sortable: true,
                //    dataIndex: 'Speed',
            }
            return list_hanhtrinh;
        }

        // báo cáo tắt/mở khoá điện
        

        private On_Off AddItemOn_Off(IList<GpsDataForOn_Off> list, DateTime end, DateTime start, ref int count,
                                     string bienso, int i, ref string thedriver, ref DriverC drivertemp,
                                     string _deviceID)
        {
            count++;
            var on_offtemp = new On_Off();

            if (list[i].TheDriver != thedriver)
            {
                drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                thedriver = list[i].TheDriver;
            }
            if (drivertemp != null)
            {
                on_offtemp.DriverLicense = drivertemp.DriverLicense;
                on_offtemp.NameDriver = drivertemp.NameDriver;
            }
            else
            {
                on_offtemp.NameDriver = "";
                on_offtemp.DriverLicense = "";
            }

            on_offtemp.count = count;
            on_offtemp.VehicleNumber = bienso;
            on_offtemp.DateTime = ConverteDateTime(start);
            on_offtemp.Date = start;
            on_offtemp.Duration = ConverteTime(end.Subtract(start).TotalMinutes);
            if (end.Subtract(start).TotalMinutes == 0)
                on_offtemp.Duration = ConverteTime(end.Subtract(start).TotalSeconds);


            return on_offtemp;
        }

        public IList<Open_Close> ReportOpen_Close(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;  //IDs
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<Open_Close> ListOpen_Close = new List<Open_Close>();

            int count = 0;
            string VehicleNumber = "";
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    Device dvtemp = getDevivebyDeviceIDT(_deviceID);
                    {
                        if (dvtemp != null && dvtemp.Door != 1)
                        {
                            continue;
                        }
                    }

                    IList<GpsDataForOpenClose> list = new List<GpsDataForOpenClose>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForReportOpen_Close(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list = Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataOpenClose_byID",
                                                                                             parameter).OrderBy(
                                                                                                 item => item.DateSave).
                                                  ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<GpsDataForOpenClose> list_backup = backupServiceT.DataForReportOpen_Close(paramOld);

                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<GpsDataForOpenClose> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataOpenClose_byID",
                                                                                          parameter).OrderBy(
                                                                                              item => item.DateSave).
                                               ToList();
                                list = list_backup.Union(listNew).ToList();
                                break;
                            }
                    }
                    var parameter2 = new Dictionary<string, string>();
                    parameter2.Add("_DeviceID", _deviceID);
                    Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicle",
                                                                             parameter2).FirstOrDefault();
                    int switch_dong = 0;
                    int switch_mo = 1;
                    if (device != null)
                    {
                        VehicleNumber = device.VehicleNumber;
                        if (device.Switch_Door == 1)
                        {
                            switch_dong = 1;
                            switch_mo = 0;
                        }
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list =
                            list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0)
                                .ToList();
                    }
                    if (list.FirstOrDefault() != null)
                    {
                        IList<Open_Close> ListOpen_CloseSingle = new List<Open_Close>();
                        string TimeOpen = "";
                        string TimeClose = "";
                        string AddressOpen = "";
                        string AddressClose = "";
                        string CoordinatesOpen = "";
                        string CoordinatesClose = "";
                        bool flag1 = true;
                        var Date = new DateTime();
                        //   bool flag2 = false;
                        //         int m = 0;
                        int speedtemp = 0;

                        string thedriver = list[0].TheDriver;
                        DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
                                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");


                        for (int i = 0; i < list.Count(); i++)
                        {
                            if (flag1)
                            {
                                if (list[i].StatusDoor.Equals(switch_mo))
                                {
                                    speedtemp = list[i].Speed;
                                    if (speedtemp >= 30)
                                    {
                                        continue;
                                    }
                                    TimeOpen = ConverteDateTime(list[i].DateSave);
                                    Date = list[i].DateSave;
                                    CoordinatesOpen = list[i].Latitude + "," + list[i].Longitude;
                                    AddressOpen = !string.IsNullOrEmpty(list[i].Addr) ? list[i].Addr : "Undefined";

                                    flag1 = false;
                                    //  flag2 = true;
                                    continue;
                                }
                            }
                            else
                            {
                                if (list[i].StatusDoor.Equals(switch_dong))
                                {
                                    TimeClose = ConverteDateTime(list[i].DateSave);
                                    CoordinatesClose = list[i].Latitude + "," + list[i].Longitude;
                                    if (!string.IsNullOrEmpty(list[i].Addr))
                                        AddressClose = list[i].Addr;
                                    else
                                        AddressClose = "Undefined";

                                    if (TimeOpen != "" && TimeClose != "")
                                    {
                                        count++;

                                        if (list[i].TheDriver != thedriver)
                                        {
                                            drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
                                                         getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                            thedriver = list[i].TheDriver;
                                        }


                                        var DongMoTemp = new Open_Close();
                                        if (drivertemp != null)
                                        {
                                            DongMoTemp.NameDriver = drivertemp.NameDriver;
                                            DongMoTemp.DriverLicense = drivertemp.DriverLicense;
                                        }
                                        else
                                        {
                                            DongMoTemp.NameDriver = "";
                                            DongMoTemp.DriverLicense = "";
                                        }

                                        DongMoTemp.VehicleNumber = VehicleNumber;
                                        DongMoTemp.TimeOpen = TimeOpen;
                                        DongMoTemp.Speed = speedtemp + " km/h";
                                        DongMoTemp.TimeClose = TimeClose;
                                        DongMoTemp.AddressOpen = AddressOpen;
                                        DongMoTemp.AddressClose = AddressClose;
                                        DongMoTemp.CoordinatesOpen = CoordinatesOpen;
                                        DongMoTemp.CoordinatesClose = CoordinatesClose;
                                        DongMoTemp.count = count;
                                        DongMoTemp.Date = Date;
                                        ListOpen_CloseSingle.Add(DongMoTemp);

                                        AddressOpen = "";
                                        AddressClose = "";
                                        speedtemp = 0;
                                    }
                                    flag1 = true;
                                }
                            }
                        } //end list
                        try
                        {
                            string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                            DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                    "yy-MM-dd",
                                                                                    CultureInfo.InvariantCulture));
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                        "yy-MM-dd", CultureInfo.InvariantCulture));


                            for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                            {
                                // logic here
                                bool fAdd = true;
                                foreach (Open_Close var in ListOpen_CloseSingle)
                                {
                                    if (DateTime.Compare(var.Date.Date, date) == 0)
                                    {
                                        fAdd = false;
                                    }
                                }
                                if (fAdd)
                                {
                                    var DongMoTemp = new Open_Close();

                                    DriverC drivertt =
                                        getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                                    if (drivertt != null)
                                    {
                                        DongMoTemp.NameDriver = drivertt.NameDriver;
                                        DongMoTemp.DriverLicense = drivertt.DriverLicense;
                                    }
                                    else
                                    {
                                        DongMoTemp.NameDriver = "";
                                        DongMoTemp.DriverLicense = "";
                                    }
                                    DongMoTemp.VehicleNumber = VehicleNumber;
                                    DongMoTemp.TimeOpen = ConverteDateTime(date);
                                    DongMoTemp.Speed = "0 km/h";
                                    DongMoTemp.TimeClose = "";
                                    DongMoTemp.AddressOpen = "";
                                    DongMoTemp.AddressClose = "";
                                    DongMoTemp.CoordinatesOpen = "";
                                    DongMoTemp.CoordinatesClose = "";
                                    DongMoTemp.count = count;
                                    DongMoTemp.Date = date;
                                    ListOpen_CloseSingle.Add(DongMoTemp);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.GetBaseException();
                        }
                        ListOpen_CloseSingle = ListOpen_CloseSingle.OrderBy(m => m.Date).ToList();

                        foreach (Open_Close vars in ListOpen_CloseSingle)
                        {
                            ListOpen_Close.Add(vars);
                        }
                    }
                }
            }
            return ListOpen_Close;
        }

        public IList<On_Off> ReportOn_Off(Dictionary<string, string> param) //gộp 2 báo cáo
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<On_Off> ListOn_Off = new List<On_Off>();
            //string type = param.FirstOrDefault(pair => pair.Key == "type").Value; // dùng để tách 2 báo cáo
            bool flag = true;
            int count = 0;
            //int m = -1;

            string bienso = "";
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                DateTime start = DateTime.Now;
                DateTime end = DateTime.Now;

                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataForOn_Off> list = new List<GpsDataForOn_Off>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForReportOn_Off(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list = Repository.ExecuteStoreProceduce<GpsDataForOn_Off>("sp_GetDataOn_Off_byID",
                                                                                          parameter).OrderBy(
                                                                                              item => item.DateSave).
                                                  ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<GpsDataForOn_Off> list_backup = backupServiceT.DataForReportOn_Off(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<GpsDataForOn_Off> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataForOn_Off>("sp_GetDataOn_Off_byID",
                                                                                       parameter).OrderBy(
                                                                                           item => item.DateSave).ToList
                                        ();
                                list = list_backup.Union(listNew).ToList();
                                break;
                            }
                    }

                    var parameter2 = new Dictionary<string, string>();
                    parameter2.Add("_DeviceID", _deviceID);
                    Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicle",
                                                                             parameter2).FirstOrDefault();
                    int switch_tat = 0;
                    int switch_mo = 1;
                    if (device != null)
                    {
                        bienso = device.VehicleNumber;
                        if (device.Switch_ == 1)
                        {
                            switch_tat = 1;
                            switch_mo = 0;
                        }
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list =
                            list.Where(n => n.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(n.DateSave) >= 0)
                                .ToList();
                    }
                    if (list.FirstOrDefault() != null)
                    {
                        start = list.FirstOrDefault().DateSave;

                        DateTime dto2 =
                            Convert.ToDateTime(
                                DateTime.ParseExact(param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8)
                                                    ,
                                                    "yy-MM-dd", CultureInfo.InvariantCulture));
                        // loai bo dong cuoi neu khong thoa ngay(To)
                        if (list[list.Count - 1].DateSave.Date > dto2.Date)
                            list.RemoveAt(list.Count - 1);

                        IList<On_Off> ListOn_OffSingle = new List<On_Off>();
                        string TimeOpen = "";
                        string TimeClose = "";
                        var Date = new DateTime();

                        string thedriver = "";
                        thedriver = list[0].TheDriver;
                        DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
                                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                        DateTime timeOpen_ = new DateTime(); // start
                        DateTime timeClose_ = new DateTime(); // end
                        double totalTime = 0;
                        double totalTime_seconds = 0;
                        for (int i = 0; i < list.Count(); i++)
                        {
                            if (flag)
                            {
                                if (list[i].StatusKey.Equals(switch_mo))
                                {
                                    TimeOpen = ConverteDateTime(list[i].DateSave);
                                    start = list[i].DateSave;
                                    Date = list[i].DateSave;
                                    timeOpen_ = list[i].DateSave;
                                    flag = false;
                                    continue;
                                }
                            }
                            else
                            {
                                if (list[i].StatusKey.Equals(switch_tat))
                                {
                                    TimeClose = ConverteDateTime(list[i].DateSave);
                                    timeClose_ = list[i].DateSave;

                                    if (TimeOpen != "" && TimeClose != "")
                                    {
                                        count++;
                                        if (list[i].TheDriver != thedriver)
                                        {
                                            drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
                                                         getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                            thedriver = list[i].TheDriver;
                                        }

                                        var _on_off = new On_Off();
                                        if (drivertemp != null)
                                        {
                                            _on_off.NameDriver = drivertemp.NameDriver;
                                            _on_off.DriverLicense = drivertemp.DriverLicense;
                                        }
                                        else
                                        {
                                            _on_off.NameDriver = "";
                                            _on_off.DriverLicense = "";
                                        }
                                        _on_off.VehicleNumber = bienso;
                                        _on_off.TimeOpen = TimeOpen;
                                        _on_off.TimeClose = TimeClose;
                                        _on_off.count = count;
                                        _on_off.Date = Date;
                                        _on_off.Duration = "";
                                        double totalMinute = timeClose_.Subtract(timeOpen_).TotalMinutes;
                                        //totalTime += timeClose_.Subtract(timeOpen_).TotalSeconds;
                                        totalTime += Math.Round(totalMinute);
                                        _on_off.Duration = Math.Round(totalMinute) + " phút";

                                        //if (totalMinute == 0)
                                        //{
                                        //    _on_off.Duration = timeClose_.Subtract(timeOpen_).TotalSeconds + " giây";
                                        //}
                                        if (totalMinute <= 0.5){
                                            _on_off.Duration = Math.Round(totalMinute * 60) + " giây";
                                            totalTime_seconds += Math.Round(totalMinute * 60);
                                        }

                                        ListOn_OffSingle.Add(_on_off);
                                    }
                                    flag = true;
                                }
                            }
                        }

                        try
                        {
                            string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                            DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                    "yy-MM-dd",
                                                                                    CultureInfo.InvariantCulture));
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                        "yy-MM-dd", CultureInfo.InvariantCulture));

                            for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                            {
                                // logic here
                                bool fAdd = true;
                                foreach (On_Off var in ListOn_OffSingle)
                                {
                                    if (DateTime.Compare(var.Date.Date, date) == 0)
                                    {
                                        fAdd = false;
                                    }
                                }
                                if (fAdd)
                                {
                                    DriverC drivertt =
                                        getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                    var on_offtemp = new On_Off();
                                    on_offtemp.count = count;
                                    if (drivertt != null)
                                    {
                                        on_offtemp.NameDriver = drivertt.NameDriver;
                                        on_offtemp.DriverLicense = drivertt.DriverLicense;
                                    }
                                    else
                                    {
                                        on_offtemp.NameDriver = "";
                                        on_offtemp.DriverLicense = "";
                                    }
                                    on_offtemp.VehicleNumber = bienso;
                                    on_offtemp.DateTime = ConverteDateTime(date);
                                    on_offtemp.Duration = "0p";
                                    on_offtemp.Date = date;
                                    on_offtemp.TimeOpen = ConverteDateTime(date);
                                    on_offtemp.TimeClose = "";
                                    on_offtemp.count = count;
                                    on_offtemp.Date = Date;
                                    on_offtemp.Duration = "";
                                    //ListOn_OffSingle.Add(on_offtemp); // thêm những ngày ko có dữ liệu (chủ nhật hoặc ko làm việc)
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.GetBaseException();
                        }
                        ListOn_OffSingle = ListOn_OffSingle.OrderBy(p => p.Date).ToList();

                        foreach (On_Off var in ListOn_OffSingle)
                        {
                            ListOn_Off.Add(var);
                        }
                        On_Off itemTong = new On_Off();
                        itemTong.VehicleNumber = "Tổng";
                        //itemTong.Duration = totalTime.ToString() + " phút"; totalTime_test
                        itemTong.Duration = ConverteTime2(totalTime, totalTime_seconds); //danhdau01

                        ListOn_Off.Add(itemTong);
                    }
                }
            }

            return ListOn_Off;
        }
        //public IList<On_Off> ReportOn_Off(Dictionary<string, string> param) //type: on or off
        //{
        //    int iDate = checkDateInt(param);
        //    Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
        //    string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
        //    _listID = _listID + ",";
        //    string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //    IList<On_Off> ListOn_OffSingle = new List<On_Off>();

        //    string type = param.FirstOrDefault(pair => pair.Key == "type").Value;

        //    // string[] list_arr = arrDeviceID.Split(',');
        //    bool flag = true;

        //    int count = 0;
        //    int m = -1;

        //    string bienso = "";
        //    for (int j = 0; j < _arrIDs.Length; j++)
        //    {
        //        DateTime start = DateTime.Now;
        //        DateTime end = DateTime.Now;

        //        string _deviceID = _arrIDs[j];
        //        if (!String.IsNullOrEmpty(_deviceID))
        //        {
        //            IList<GpsDataForOn_Off> list = new List<GpsDataForOn_Off>();
        //            switch (iDate)
        //            {
        //                case 1:
        //                    {
        //                        Dictionary<string, string> paramOld = new Dictionary<string, string>();
        //                        paramOld["_DeviceID"] = _deviceID;
        //                        paramOld["_from"] = dataid["_from"];
        //                        paramOld["_to"] = dataid["_to"];
        //                        list = backupServiceT.DataForReportOn_Off(paramOld);
        //                        break;
        //                    }
        //                case 2:
        //                    {
        //                        Dictionary<string, string> parameter = new Dictionary<string, string>();
        //                        parameter["_DeviceID"] = _deviceID;
        //                        parameter["_from"] = dataid["_from"];
        //                        parameter["_to"] = dataid["_to"];
        //                        list = Repository.ExecuteStoreProceduce<GpsDataForOn_Off>("sp_GetDataOn_Off_byID",
        //                                                                                  parameter).OrderBy(
        //                                                                                      item => item.DateSave).
        //                                          ToList();
        //                        break;
        //                    }
        //                case 3:
        //                    {
        //                        Dictionary<string, string> paramOld = new Dictionary<string, string>();
        //                        paramOld["_DeviceID"] = _deviceID;
        //                        paramOld["_from"] = dataid["_from_old"];
        //                        paramOld["_to"] = dataid["_to_old"];
        //                        IList<GpsDataForOn_Off> list_backup = backupServiceT.DataForReportOn_Off(paramOld);
        //                        Dictionary<string, string> parameter = new Dictionary<string, string>();
        //                        parameter["_DeviceID"] = _deviceID;
        //                        parameter["_from"] = dataid["_from_new"];
        //                        parameter["_to"] = dataid["_to_new"];
        //                        IList<GpsDataForOn_Off> listNew =
        //                            Repository.ExecuteStoreProceduce<GpsDataForOn_Off>("sp_GetDataOn_Off_byID",
        //                                                                               parameter).OrderBy(
        //                                                                                   item => item.DateSave).ToList
        //                                ();
        //                        list = list_backup.Union(listNew).ToList();
        //                        break;
        //                    }
        //            }
        //            var parameter2 = new Dictionary<string, string>();
        //            parameter2.Add("_DeviceID", _deviceID);
        //            Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicle",
        //                                                                     parameter2).FirstOrDefault();
        //            int switch_tat = 0;
        //            int switch_mo = 1;
        //            if (device != null)
        //            {
        //                bienso = device.VehicleNumber;
        //                if (device.Switch_ == 1)
        //                {
        //                    switch_tat = 1;
        //                    switch_mo = 0;
        //                }
        //            }
        //            String from = "20" + param["From"];
        //            String to = "20" + param["To"];
        //            DateTime dateFrom = DateTime.Parse(from);
        //            DateTime dateTo = DateTime.Parse(to);
        //            if (list.FirstOrDefault() != null)
        //            {
        //                list =
        //                    list.Where(n => n.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(n.DateSave) >= 0)
        //                        .ToList();
        //            }
        //            if (list.FirstOrDefault() != null)
        //            {
        //                start = list.FirstOrDefault().DateSave;

        //                DateTime dto2 =
        //                    Convert.ToDateTime(
        //                        DateTime.ParseExact(param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8)
        //                                            ,
        //                                            "yy-MM-dd", CultureInfo.InvariantCulture));
        //                // loai bo dong cuoi neu khong thoa ngay(To)
        //                if (list[list.Count - 1].DateSave.Date > dto2.Date)
        //                    list.RemoveAt(list.Count - 1);
        //                string thedriver = "";
        //                thedriver = list[0].TheDriver;
        //                DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
        //                                     getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

        //                // neu mo may
        //                if (type.Equals("on"))
        //                {
        //                    flag = false;
        //                    for (int i = 0; i < list.Count(); i++)
        //                    {
        //                        //ListOn_OffSingle
        //                        // ListOn_OffSingle.Add(AddItemOn_Off(list, end, start, ref count, bienso, i,
        //                        // ref thedriver, ref drivertemp, //_deviceID));
        //                        if (list[i].StatusKey.Equals(switch_mo) && flag == false)
        //                        {
        //                            flag = true;
        //                            start = list[i].DateSave;
        //                        }
        //                        if ((flag == true && list[i].StatusKey.Equals(switch_tat)) || (list[i].StatusKey.Equals(switch_mo) && flag == true && i == list.Count - 1))
        //                        {
        //                            end = list[i].DateSave;
        //                            var on_offtemp = new On_Off();
        //                            // on_offtemp.count = count;
        //                            on_offtemp.VehicleNumber = bienso;
        //                            on_offtemp.DateTime = ConverteDateTime(start);
        //                            on_offtemp.Date = start;
        //                            on_offtemp.Duration = ConverteTime(end.Subtract(start).TotalMinutes);
        //                            if (end.Subtract(start).TotalMinutes == 0)
        //                            {
        //                                on_offtemp.Duration = ConverteTime(end.Subtract(start).TotalSeconds);
        //                            }
        //                            ListOn_OffSingle.Add(on_offtemp);
        //                            flag = false;
        //                        }



        //                    }
        //                }
        //                else if (type.Equals("off"))
        //                {
        //                    flag = false;
        //                    for (int i = 0; i < list.Count(); i++)
        //                    {
        //                        //ListOn_OffSingle
        //                        // ListOn_OffSingle.Add(AddItemOn_Off(list, end, start, ref count, bienso, i,
        //                        // ref thedriver, ref drivertemp, //_deviceID));
        //                        if (list[i].StatusKey.Equals(switch_tat) && flag == false)
        //                        {
        //                            flag = true;
        //                            start = list[i].DateSave;
        //                        }
        //                        if ((flag == true && list[i].StatusKey.Equals(switch_mo)) || (list[i].StatusKey.Equals(switch_tat) && flag == true && i == list.Count - 1))
        //                        {
        //                            end = list[i].DateSave;
        //                            var on_offtemp = new On_Off();
        //                            // on_offtemp.count = count;
        //                            on_offtemp.VehicleNumber = bienso;
        //                            on_offtemp.DateTime = ConverteDateTime(start);
        //                            on_offtemp.Date = start;
        //                            on_offtemp.Duration = ConverteTime(end.Subtract(start).TotalMinutes);
        //                            if (end.Subtract(start).TotalMinutes == 0)
        //                            {
        //                                on_offtemp.Duration = ConverteTime(end.Subtract(start).TotalSeconds);
        //                            }
        //                            ListOn_OffSingle.Add(on_offtemp);
        //                            flag = false;
        //                        }



        //                    }
        //                }
        //                //end list
        //                try
        //                {

        //                }
        //                catch (Exception ex)
        //                {
        //                    ex.GetBaseException();
        //                }

        //            }
        //        }
        //    }
        //    return ListOn_OffSingle;
        //}

        public IList<Open_Close> ReportCooler(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<Open_Close> ListOpen_Close = new List<Open_Close>();
            
            int count = 0;
            string VehicleNumber = "";
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataForOpenClose> list = new List<GpsDataForOpenClose>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForReportCooler(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list = Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataCooler_byID",
                                                                                             parameter).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<GpsDataForOpenClose> list_backup = backupServiceT.DataForReportCooler(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<GpsDataForOpenClose> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataCooler_byID",
                                                                                          parameter).ToList();
                                list = list_backup.Union(listNew).ToList();
                                break;
                            }
                    }
                    var parameter2 = new Dictionary<string, string>();
                    parameter2.Add("_DeviceID", _deviceID);
                    Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicleT", parameter2).FirstOrDefault();
                    int switch_dong = 0;
                    int switch_mo = 1;
                    if (device != null)
                    {
                        if (device.Switch_Cooler == 1)
                        {
                            switch_dong = 1;
                            switch_mo = 0;
                        }
                    }
                    if (device != null)
                    {
                        VehicleNumber = device.VehicleNumber;
                        if (device.Cooler_ != 1)
                        {
                            return ListOpen_Close;
                        }
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list = list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0).ToList();
                    }
                    if (list.FirstOrDefault() != null)
                    {
                        IList<Open_Close> ListOpen_CloseSingle = new List<Open_Close>();
                        string TimeOpen = "";
                        string TimeClose = "";
                        string AddressOpen = "";
                        string AddressClose = "";
                        string CoordinatesOpen = "";
                        string CoordinatesClose = "";
                        bool flag1 = true;
                        var Date = new DateTime();
                        int speedtemp = 0;
                        string thedriver = list[0].TheDriver;
                        DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ?? getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                        DateTime timeOpen_ = new DateTime();
                        DateTime timeClose_ = new DateTime();
                        double totalTime = 0;
                        double totalTime_seconds = 0;
                        for (int i = 0; i < list.Count(); i++)
                        {
                            if (flag1)
                            {
                                if (list[i].Cooler.Value.Equals(switch_mo))
                                {
                                    speedtemp = list[i].Speed;
                                    TimeOpen = ConverteDateTime(list[i].DateSave);
                                    timeOpen_ = list[i].DateSave;
                                    Date = list[i].DateSave;
                                    CoordinatesOpen = list[i].Latitude + "," + list[i].Longitude;
                                    AddressOpen = !string.IsNullOrEmpty(list[i].Addr) ? list[i].Addr : "chưa xác định";
                                    flag1 = false;
                                    continue;
                                }
                            }
                            else
                            {
                                if (list[i].Cooler.Value.Equals(switch_dong))
                                {
                                    TimeClose = ConverteDateTime(list[i].DateSave);
                                    timeClose_ = list[i].DateSave;
                                    CoordinatesClose = list[i].Latitude + "," + list[i].Longitude;
                                    if (!string.IsNullOrEmpty(list[i].Addr))
                                        AddressClose = list[i].Addr;
                                    else
                                        AddressClose = "chưa xác định";

                                    if (TimeOpen != "" && TimeClose != "")
                                    {
                                        count++;
                                        if (list[i].TheDriver != thedriver)
                                        {
                                            drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
                                                         getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                            thedriver = list[i].TheDriver;
                                        }
                                        var DongMoTemp = new Open_Close();
                                        if (drivertemp != null)
                                        {
                                            DongMoTemp.NameDriver = drivertemp.NameDriver;
                                            DongMoTemp.DriverLicense = drivertemp.DriverLicense;
                                        }
                                        else
                                        {
                                            DongMoTemp.NameDriver = "";
                                            DongMoTemp.DriverLicense = "";
                                        }

                                        DongMoTemp.VehicleNumber = VehicleNumber;
                                        DongMoTemp.TimeOpen = TimeOpen;
                                        DongMoTemp.Speed = speedtemp + " km/h";
                                        DongMoTemp.TimeClose = TimeClose;
                                        DongMoTemp.AddressOpen = AddressOpen;
                                        DongMoTemp.AddressClose = AddressClose;
                                        DongMoTemp.CoordinatesOpen = CoordinatesOpen;
                                        DongMoTemp.CoordinatesClose = CoordinatesClose;
                                        DongMoTemp.count = count;
                                        DongMoTemp.Date = Date;
                                        DongMoTemp.Duration = "";
                                        double totalMinute = timeClose_.Subtract(timeOpen_).TotalMinutes;
                                        //totalTime += timeClose_.Subtract(timeOpen_).TotalSeconds;
                                        totalTime += Math.Round(totalMinute);
                                        DongMoTemp.Duration = Math.Round(totalMinute) + " phút";
                                        //if (totalMinute == 0)
                                        //{
                                        //    DongMoTemp.Duration = timeClose_.Subtract(timeOpen_).TotalSeconds + " giây";
                                        //}

                                        if (totalMinute <= 0.5) {
                                            DongMoTemp.Duration = Math.Round(totalMinute * 60) + " giây";
                                            totalTime_seconds += Math.Round(totalMinute * 60);
                                        }

                                        ListOpen_CloseSingle.Add(DongMoTemp);
                                        AddressOpen = "";
                                        AddressClose = "";
                                        speedtemp = 0;
                                    }
                                    flag1 = true;
                                }
                            }
                        } //end list
                        try
                        {
                            string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                            DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                    "yy-MM-dd",
                                                                                    CultureInfo.InvariantCulture));
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                        "yy-MM-dd", CultureInfo.InvariantCulture));

                            for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                            {
                                // logic here
                                bool fAdd = true;
                                foreach (Open_Close var in ListOpen_CloseSingle)
                                {
                                    if (DateTime.Compare(var.Date.Date, date) == 0)
                                    {
                                        fAdd = false;
                                    }
                                }
                                if (fAdd)
                                {
                                    var DongMoTemp = new Open_Close();

                                    DriverC drivertt =
                                        getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                                    if (drivertt != null)
                                    {
                                        DongMoTemp.NameDriver = drivertt.NameDriver;
                                        DongMoTemp.DriverLicense = drivertt.DriverLicense;
                                    }
                                    else
                                    {
                                        DongMoTemp.NameDriver = "";
                                        DongMoTemp.DriverLicense = "";
                                    }
                                    DongMoTemp.VehicleNumber = VehicleNumber;
                                    DongMoTemp.TimeOpen = ConverteDateTime(date);
                                    DongMoTemp.Speed = "0 km/h";
                                    DongMoTemp.TimeClose = "";
                                    DongMoTemp.AddressOpen = "";
                                    DongMoTemp.AddressClose = "";
                                    DongMoTemp.CoordinatesOpen = "";
                                    DongMoTemp.CoordinatesClose = "";
                                    DongMoTemp.count = count;
                                    DongMoTemp.Date = Date;
                                    DongMoTemp.Duration = "";
                                    //ListOpen_CloseSingle.Add(DongMoTemp); // thêm những ngày ko có dữ liệu (chủ nhật hoặc ko làm việc)
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.GetBaseException();
                        }
                        ListOpen_CloseSingle = ListOpen_CloseSingle.OrderBy(m => m.Date).ToList();
                        //totalTime = Math.Round(totalTime / 60);
                        foreach (Open_Close vars in ListOpen_CloseSingle)
                        {
                            ListOpen_Close.Add(vars);
                        }
                        Open_Close dongmoItem = new Open_Close();
                        dongmoItem.VehicleNumber = "Tổng";
                        //dongmoItem.Duration = totalTime.ToString()+" phút";
                        dongmoItem.Duration = ConverteTime2(totalTime, totalTime_seconds); //danhdau01
                        ListOpen_Close.Add(dongmoItem);
                    }
                }
            }
            return ListOpen_Close;
        }
        //public IList<Open_Close> ReportCooler(Dictionary<string, string> param)
        //{
        //    int iDate = checkDateInt(param);
        //    Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
        //    string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value; // IDs
        //    string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //    IList<Open_Close> ListOpen_Close = new List<Open_Close>();

        //    //bool flag = true;
        //    //DateTime start = DateTime.Now;
        //    //DateTime end = DateTime.Now;
        //    int count = 0;
        //    string VehicleNumber = "";
        //    for (int j = 0; j < _arrIDs.Length; j++)
        //    {
        //        string _deviceID = _arrIDs[j];
        //        if (!String.IsNullOrEmpty(_deviceID))
        //        {
        //            IList<GpsDataForOpenClose> list = new List<GpsDataForOpenClose>();
        //            switch (iDate)
        //            {
        //                case 1:
        //                    {
        //                        Dictionary<string, string> paramOld = new Dictionary<string, string>();
        //                        paramOld["_DeviceID"] = _deviceID;
        //                        paramOld["_from"] = dataid["_from"];
        //                        paramOld["_to"] = dataid["_to"];
        //                        list = backupServiceT.DataForReportCooler(paramOld);
        //                        break;
        //                    }
        //                case 2:
        //                    {
        //                        Dictionary<string, string> parameter = new Dictionary<string, string>();
        //                        parameter["_DeviceID"] = _deviceID;
        //                        parameter["_from"] = dataid["_from"];
        //                        parameter["_to"] = dataid["_to"];
        //                        list = Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataCooler_byID",
        //                                                                                     parameter).ToList();
        //                        break;
        //                    }
        //                case 3:
        //                    {
        //                        Dictionary<string, string> paramOld = new Dictionary<string, string>();
        //                        paramOld["_DeviceID"] = _deviceID;
        //                        paramOld["_from"] = dataid["_from_old"];
        //                        paramOld["_to"] = dataid["_to_old"];
        //                        IList<GpsDataForOpenClose> list_backup = backupServiceT.DataForReportCooler(paramOld);
        //                        Dictionary<string, string> parameter = new Dictionary<string, string>();
        //                        parameter["_DeviceID"] = _deviceID;
        //                        parameter["_from"] = dataid["_from_new"];
        //                        parameter["_to"] = dataid["_to_new"];
        //                        IList<GpsDataForOpenClose> listNew =
        //                            Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataCooler_byID",
        //                                                                                  parameter).ToList();
        //                        list = list_backup.Union(listNew).ToList();
        //                        break;
        //                    }
        //            }
        //            var parameter2 = new Dictionary<string, string>();
        //            parameter2.Add("_DeviceID", _deviceID);
        //            Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicleT",
        //                                                                     parameter2).FirstOrDefault();

        //            int switch_dong = 0;
        //            int switch_mo = 1;
        //            if (device != null)
        //            {
        //                VehicleNumber = device.VehicleNumber;
        //                if (device.Cooler_ != 1)
        //                {
        //                    return ListOpen_Close;
        //                }
        //            }
        //            String from = "20" + param["From"];
        //            String to = "20" + param["To"];
        //            DateTime dateFrom = DateTime.Parse(from);
        //            DateTime dateTo = DateTime.Parse(to);
        //            if (list.FirstOrDefault() != null)
        //            {
        //                list =
        //                    list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0)
        //                        .ToList();
        //            }
        //            if (list.FirstOrDefault() != null)
        //            {
        //                IList<Open_Close> ListOpen_CloseSingle = new List<Open_Close>();
        //                string TimeOpen = "";
        //                string TimeClose = "";
        //                string AddressOpen = "";
        //                string AddressClose = "";
        //                string CoordinatesOpen = "";
        //                string CoordinatesClose = "";
        //                bool flag1 = true;
        //                var Date = new DateTime();
        //                //   bool flag2 = false;
        //                //         int m = 0;
        //                int speedtemp = 0;

        //                string thedriver = list[0].TheDriver;
        //                DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
        //                                     getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");


        //                for (int i = 0; i < list.Count(); i++)
        //                {
        //                    if (flag1)
        //                    {
        //                        if (list[i].Cooler.Value.Equals(switch_mo))
        //                        {
        //                            speedtemp = list[i].Speed;
        //                            //if (speedtemp >= 30)
        //                            //{
        //                            //    continue;
        //                            //}
        //                            TimeOpen = ConverteDateTime(list[i].DateSave);
        //                            Date = list[i].DateSave;
        //                            CoordinatesOpen = list[i].Latitude + "," + list[i].Longitude;
        //                            AddressOpen = !string.IsNullOrEmpty(list[i].Addr) ? list[i].Addr : "Undefined";
        //                            flag1 = false;
        //                            //  flag2 = true;
        //                            continue;
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (list[i].Cooler.Value.Equals(switch_dong))
        //                        {
        //                            TimeClose = ConverteDateTime(list[i].DateSave);
        //                            CoordinatesClose = list[i].Latitude + "," + list[i].Longitude;
        //                            if (!string.IsNullOrEmpty(list[i].Addr))
        //                                AddressClose = list[i].Addr;
        //                            else
        //                                AddressClose = "Undefined";

        //                            if (TimeOpen != "" && TimeClose != "")
        //                            {
        //                                count++;

        //                                if (list[i].TheDriver != thedriver)
        //                                {
        //                                    drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
        //                                                 getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
        //                                    thedriver = list[i].TheDriver;
        //                                }


        //                                var DongMoTemp = new Open_Close();
        //                                if (drivertemp != null)
        //                                {
        //                                    DongMoTemp.NameDriver = drivertemp.NameDriver;
        //                                    DongMoTemp.DriverLicense = drivertemp.DriverLicense;
        //                                }
        //                                else
        //                                {
        //                                    DongMoTemp.NameDriver = "";
        //                                    DongMoTemp.DriverLicense = "";
        //                                }

        //                                DongMoTemp.VehicleNumber = VehicleNumber;
        //                                DongMoTemp.TimeOpen = TimeOpen;
        //                                DongMoTemp.Speed = speedtemp + " km/h";
        //                                DongMoTemp.TimeClose = TimeClose;
        //                                DongMoTemp.AddressOpen = AddressOpen;
        //                                DongMoTemp.AddressClose = AddressClose;
        //                                DongMoTemp.CoordinatesOpen = CoordinatesOpen;
        //                                DongMoTemp.CoordinatesClose = CoordinatesClose;
        //                                DongMoTemp.count = count;
        //                                DongMoTemp.Date = Date;
        //                                ListOpen_CloseSingle.Add(DongMoTemp);

        //                                AddressOpen = "";
        //                                AddressClose = "";
        //                                speedtemp = 0;
        //                            }
        //                            flag1 = true;
        //                        }
        //                    }
        //                } //end list
        //                try
        //                {
        //                    string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
        //                    DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
        //                                                                            "yy-MM-dd",
        //                                                                            CultureInfo.InvariantCulture));
        //                    DateTime dto =
        //                        Convert.ToDateTime(
        //                            DateTime.ParseExact(
        //                                param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
        //                                "yy-MM-dd", CultureInfo.InvariantCulture));


        //                    for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
        //                    {
        //                        // logic here
        //                        bool fAdd = true;
        //                        foreach (Open_Close var in ListOpen_CloseSingle)
        //                        {
        //                            if (DateTime.Compare(var.Date.Date, date) == 0)
        //                            {
        //                                fAdd = false;
        //                            }
        //                        }
        //                        if (fAdd)
        //                        {
        //                            var DongMoTemp = new Open_Close();

        //                            DriverC drivertt =
        //                                getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

        //                            if (drivertt != null)
        //                            {
        //                                DongMoTemp.NameDriver = drivertt.NameDriver;
        //                                DongMoTemp.DriverLicense = drivertt.DriverLicense;
        //                            }
        //                            else
        //                            {
        //                                DongMoTemp.NameDriver = "";
        //                                DongMoTemp.DriverLicense = "";
        //                            }
        //                            DongMoTemp.VehicleNumber = VehicleNumber;
        //                            DongMoTemp.TimeOpen = ConverteDateTime(date);
        //                            DongMoTemp.Speed = "0 km/h";
        //                            DongMoTemp.TimeClose = "";
        //                            DongMoTemp.AddressOpen = "";
        //                            DongMoTemp.AddressClose = "";
        //                            DongMoTemp.CoordinatesOpen = "";
        //                            DongMoTemp.CoordinatesClose = "";
        //                            DongMoTemp.count = count;
        //                            DongMoTemp.Date = Date;
        //                            ListOpen_CloseSingle.Add(DongMoTemp);
        //                        }
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    ex.GetBaseException();
        //                }
        //                ListOpen_CloseSingle = ListOpen_CloseSingle.OrderBy(m => m.Date).ToList();

        //                foreach (Open_Close vars in ListOpen_CloseSingle)
        //                {
        //                    ListOpen_Close.Add(vars);
        //                }
        //            }
        //        }
        //    }
        //    return ListOpen_Close;
        //}
        public string ConverteTime2(double difference, double _seconds) // làm tròn số dư, <5 -> 0 và >=5 -> 1
        {
            double timetemp = 0;
            double tempMinute = 0;
            double sodu = 0;
            if (difference >= 60) // phút > 60 => tổng phút => giờ + (phút + (tổng giây => phút))
            {
                sodu = difference % 60;
                difference = difference - sodu;
                timetemp = difference / 60;
                tempMinute = difference % 60;
                sodu = sodu + Math.Round(_seconds / 60); // cộng thêm tổng số giây
                if (tempMinute == 0)
                    return timetemp + " giờ " + sodu + " phút";
                else
                    return timetemp + " giờ" + tempMinute + sodu + " phút";
            }
            else // phút < 60 => phút + tổng giây => phút
            {
                if (_seconds >= 60)
                {
                    sodu = sodu + Math.Round(_seconds / 60);
                }
                else if (_seconds > 30 && _seconds < 60)
                {
                    sodu = sodu + 1;
                }
                return difference + sodu + " phút ";
            }
        }
        
        public IList<reportFule> ReportFuel(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<Fuel> ListFuel = new List<Fuel>();
            IList<reportFule> ListFuel_temp = new List<reportFule>();
            String from = "20" + param["From"];
            String to = "20" + param["To"];
            DateTime dateFrom = DateTime.Parse(from);
            DateTime dateTo = DateTime.Parse(to);
            //bool flag = true;
            //DateTime start = DateTime.Now;
            //DateTime end = DateTime.Now;

            // Dictionary<string, string> parameter = new Dictionary<string, string>();
            // parameter.Add("_vehiclenumber", data_model.VehicleNumber);
            // OilInfomation OilInfomation_ =
            //  Repository.ExecuteStoreProceduce<OilInfomation>("sp_tblinforcaloil_by_vehiclenumber", parameter).FirstOrDefault();

            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                Dictionary<string, string> para_DeviceID = new Dictionary<string, string>();
                para_DeviceID.Add("_DeviceID", _deviceID);
                Device _Device = (new DeviceService()).GetDeviceByDeviceID(para_DeviceID).FirstOrDefault();
                Dictionary<string, string> parameter_oil = new Dictionary<string, string>();
                parameter_oil.Add("_vehiclenumber", _Device.VehicleNumber);
                OilInfomation OilInfomation_ =
                 Repository.ExecuteStoreProceduce<OilInfomation>("sp_tblinforcaloil_by_vehiclenumber", parameter_oil).FirstOrDefault();
                if (OilInfomation_ != null)
                {
                    IList<Fuel> list = new List<Fuel>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForReportFuel(paramOld).OrderBy(m => m.DateSave).ToList();
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list = Repository.ExecuteStoreProceduce<Fuel>("sp_GetDataFuel_byID",
                                                                              parameter).OrderBy(m => m.DateSave).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<Fuel> list_backup = backupServiceT.DataForReportFuel(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<Fuel> listNew =
                                    Repository.ExecuteStoreProceduce<Fuel>("sp_GetDataFuel_byID",
                                                                           parameter).ToList();
                                list = list_backup.Union(listNew).OrderBy(m => m.DateSave).ToList();
                                break;
                            }

                    }

                    if (list.Count > 1)
                    {
                        list = list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0 && m.Oilvalue <= OilInfomation_.VoltMax && m.Oilvalue > 0).ToList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].Oilvalue_real = Math.Round(Double.Parse(((list[i].Oilvalue * OilInfomation_.VolumeOilBarrel.Value) / OilInfomation_.VoltMax.Value).ToString()));
                        }
                        list = FilterOil(list, 100);
                        Dictionary<int, IList<Fuel>> list_dic_fuel = new Dictionary<int, IList<Fuel>>();
                        int index_dic = 0;
                        double oil_value_real_0 = list[0].Oilvalue_real;
                        IList<Fuel> list_fuel_temp = new List<Fuel>();
                        int count_ = 0;
                        for (int k = 1; k < list.Count; k++)
                        {

                            if (list[k].Oilvalue_real <= oil_value_real_0)
                            {
                                list_fuel_temp.Add(list[k]);
                                count_ = 0;
                            }
                            else
                            {
                                count_ += 1;
                            }

                            if (count_ >= 2)
                            {
                                if (count_ == 2)
                                {
                                    list_dic_fuel.Add(index_dic, list_fuel_temp);
                                    index_dic += 1;

                                    list_fuel_temp = new List<Fuel>();
                                }
                                oil_value_real_0 = list[k].Oilvalue_real;

                            }
                            if (k == list.Count - 1)
                            {
                                list_dic_fuel.Add(index_dic, list_fuel_temp);
                            }
                        }

                        for (int h = 0; h < list_dic_fuel.Count; h++)
                        {
                            list_fuel_temp = list_dic_fuel[h].OrderBy(m => m.DateSave).ToList();
                            double result2 = 0;
                            if (list_fuel_temp != null && list_fuel_temp.Count > 0)
                            {
                                result2 = CalculateDistance_V2_forOil(list_fuel_temp, 0, list_fuel_temp.Count - 1);
                                result2 = Math.Round(result2);
                                reportFule a = new reportFule();
                                a.Distane = result2;
                                a.Oilvalue_start = list_fuel_temp[0].Oilvalue_real.ToString() + "/" + OilInfomation_.VolumeOilBarrel;
                                a.DateSave_start = list_fuel_temp[0].DateSave;
                                a.Address_start = list_fuel_temp[0].Address;

                                a.Oilvalue_end = list_fuel_temp[list_fuel_temp.Count - 1].Oilvalue_real.ToString() + "/" + OilInfomation_.VolumeOilBarrel;
                                a.DateSave_end = list_fuel_temp[list_fuel_temp.Count - 1].DateSave;
                                a.Address_end = list_fuel_temp[list_fuel_temp.Count - 1].Address;
                                a.Oilvalue_Result = list_fuel_temp[0].Oilvalue_real - list_fuel_temp[list_fuel_temp.Count - 1].Oilvalue_real;
                                a.VehicleNumber = list_fuel_temp[0].VehicleNumber;
                                ListFuel_temp.Add(a);
                            }

                        }


                    }
                }
            }
            return ListFuel_temp;
        }

        public IList<Fuel> ReportFuel_LineChart(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            _listID = _listID + ",";
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<Fuel> ListFuel = new List<Fuel>();
            IList<reportFule> ListFuel_temp = new List<reportFule>();
            String from = "20" + param["From"];
            String to = "20" + param["To"];
            DateTime dateFrom = DateTime.Parse(from);
            DateTime dateTo = DateTime.Parse(to);
            //bool flag = true;
            //DateTime start = DateTime.Now;
            //DateTime end = DateTime.Now;

            // Dictionary<string, string> parameter = new Dictionary<string, string>();
            // parameter.Add("_vehiclenumber", data_model.VehicleNumber);
            // OilInfomation OilInfomation_ =
            //  Repository.ExecuteStoreProceduce<OilInfomation>("sp_tblinforcaloil_by_vehiclenumber", parameter).FirstOrDefault();
            IList<Fuel> list = new List<Fuel>();
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                Dictionary<string, string> para_DeviceID = new Dictionary<string, string>();
                para_DeviceID.Add("_DeviceID", _deviceID);
                Device _Device = (new DeviceService()).GetDeviceByDeviceID(para_DeviceID).FirstOrDefault();
                Dictionary<string, string> parameter_oil = new Dictionary<string, string>();
                parameter_oil.Add("_vehiclenumber", _Device.VehicleNumber);
                OilInfomation OilInfomation_ =
                 Repository.ExecuteStoreProceduce<OilInfomation>("sp_tblinforcaloil_by_vehiclenumber", parameter_oil).FirstOrDefault();
                list = new List<Fuel>();
                if (OilInfomation_ != null)
                {

                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForReportFuel(paramOld).OrderBy(m => m.DateSave).ToList();
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list = Repository.ExecuteStoreProceduce<Fuel>("sp_GetDataFuel_byID",
                                                                              parameter).OrderBy(m => m.DateSave).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<Fuel> list_backup = backupServiceT.DataForReportFuel(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<Fuel> listNew =
                                    Repository.ExecuteStoreProceduce<Fuel>("sp_GetDataFuel_byID",
                                                                           parameter).ToList();
                                list = list_backup.Union(listNew).OrderBy(m => m.DateSave).ToList();
                                break;
                            }

                    }

                    if (list.Count > 1)
                    {
                        list = list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0 && m.Oilvalue <= OilInfomation_.VoltMax && m.Oilvalue > 0).ToList();
                        for (int i = 0; i < list.Count; i++)
                        {
                            list[i].Oilvalue_real = Math.Round(Double.Parse(((list[i].Oilvalue * OilInfomation_.VolumeOilBarrel.Value) / OilInfomation_.VoltMax.Value).ToString()));
                        }

                        list = FilterOil(list, 100);
                        list = FilterOil(list, 10);
                    }

                    if (list.Count > 100)
                    {
                        IList<Fuel> list_ = list;
                        list = new List<Fuel>();
                        for (int i = 0; i < list_.Count; i += 4)
                        {
                            list.Add(list_[i]);
                        }
                    }
                }
            }
            return list;
        }


        private IList<Fuel> FilterOil(IList<Fuel> list, int val_)
        {
            for (int i = 0; i < list.Count; i++)
            {
                list[i].val = (int)(list[i].Oilvalue_real / val_);
            }
            IList<Fuel> listFilter = new List<Fuel>();
            Fuel valtemp = list[0];
            int valmem = 100;
            for (int i = 1; i < list.Count; i++)
            {
                valmem = 100;
                if (valtemp.val != list[i].val && valtemp.val != list[i + 1].val)
                {
                    if (i >= 3)
                    {
                        if (valtemp.val != list[i - 2].val && valtemp.val != list[i - 3].val)
                        {
                            listFilter.Add(valtemp);
                        }
                    }
                    else
                    {
                        listFilter.Add(valtemp);
                    }

                }
                else if (((valtemp.val != list[i].val && valtemp.val == list[i + 1].val) || (valtemp.val == list[i].val && valtemp.val != list[i + 1].val)) && (valtemp.val != valmem))
                {
                    listFilter.Add(valtemp);
                }
                else
                {
                    valmem = valtemp.val;
                }
                if ((i + 1) == list.Count - 1)
                {
                    break;
                }
                valtemp = list[i];
            }

            if (listFilter.Count > 0)
            {
                for (int i = 0; i < listFilter.Count; i++)
                {
                    list = list.Where(m => m.DateSave != listFilter[i].DateSave).ToList();
                }
            }



            return list;
        }

        public IList<PauseStop> ReportPause_Stop(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            _listID = _listID + ",";

            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<PauseStop> ListPauseStop = new List<PauseStop>();


            //string[] list_arr = arrImei.Split(',');
            bool flag = true;
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            int count = 0;
            string VehicleNumber = "";
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataForPauseStop> list = new List<GpsDataForPauseStop>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from"];
                                paramOld["_to"] = dataid["_to"];
                                list = backupServiceT.DataForReportPause_Stop(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from"];
                                parameter["_to"] = dataid["_to"];
                                list =
                                    Repository.ExecuteStoreProceduce<GpsDataForPauseStop>("sp_GetData_PauseStop_byIDT",
                                                                                          parameter).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                paramOld["_from"] = dataid["_from_old"];
                                paramOld["_to"] = dataid["_to_old"];
                                IList<GpsDataForPauseStop> list_backup = backupServiceT.DataForReportPause_Stop(paramOld);
                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                parameter["_DeviceID"] = _deviceID;
                                parameter["_from"] = dataid["_from_new"];
                                parameter["_to"] = dataid["_to_new"];
                                IList<GpsDataForPauseStop> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataForPauseStop>("sp_GetData_PauseStop_byIDT",
                                                                                          parameter).ToList();
                                list = list_backup.Union(listNew).ToList();
                                break;
                            }
                    }

                    var parameter2 = new Dictionary<string, string>();
                    parameter2.Add("_DeviceID", _deviceID);
                    Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicle",
                                                                             parameter2).First();
                    if (device != null)
                    {
                        VehicleNumber = device.VehicleNumber;
                    }
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    if (list.FirstOrDefault() != null)
                    {
                        list = list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0).ToList();
                    }
                    if (list.FirstOrDefault() != null)
                    {
                        IList<PauseStop> ListPauseStopSingle = new List<PauseStop>();
                        bool fSpeed = false;
                        var tempstar = new DateTime();
                        string diachi = "";
                        string toado = "";
                        string thedriver = "";
                        thedriver = list[0].TheDriver;
                        DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
                                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                        for (int i = 0; i < list.Count(); i++)
                        {
                            // neu chua co diem bat dau va xe dang dung
                            if (!fSpeed && list[i].Speed == 0)
                            {
                                tempstar = list[i].DateSave;
                                toado = list[i].Latitude + "," + list[i].Longitude;
                                diachi = !string.IsNullOrEmpty(list[i].Addr) ? list[i].Addr : "Undefined";
                                fSpeed = true;
                            }
                            // neu da co diem bat dau va xe bat dau chay
                            //hoac toan bo dong phia sau deu co van toc  bang 0(i == list.Count - 1)
                            else if (fSpeed && (list[i].Speed > 0 || i == list.Count - 1))
                            {
                                fSpeed = false;
                                // tinh khoang thoi gian dung
                                double difference = list[i].DateSave.Subtract(tempstar).TotalMinutes;
                                // neu thoi gian nho hon 1 phut thi quay lai vong lap
                                if (difference <= 1)
                                {
                                    continue;
                                }
                                // nguoc lai kiem tra dung hoac do
                                //neu thoi gian nghi > 15 phut la do nguoc lai la dung
                                else
                                {
                                    count++;
                                    var dungdotemp = new PauseStop();
                                    // kiem tra co thay doi tai xe khong
                                    if (list[i].TheDriver != thedriver)
                                    {
                                        drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
                                                     getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                        thedriver = list[i].TheDriver;
                                    }
                                    if (drivertemp != null)
                                    {
                                        dungdotemp.DriverLicense = drivertemp.DriverLicense;
                                        dungdotemp.NameDriver = drivertemp.NameDriver;
                                    }
                                    else
                                    {
                                        dungdotemp.NameDriver = "";
                                        dungdotemp.DriverLicense = "";
                                    }
                                    // lay thong tin
                                    // kiem tra la dung hoac do
                                    dungdotemp.Status = difference > 15 ? "Đỗ" : "Dừng";
                                    dungdotemp.VehicleNumber = VehicleNumber;
                                    dungdotemp.count = count;
                                    dungdotemp.DateTime = ConverteDateTime(tempstar);
                                    dungdotemp.Date = tempstar;
                                    dungdotemp.Duration = ConverteTime(difference);
                                    dungdotemp.Address = diachi;
                                    dungdotemp.Coordinates = toado;
                                    dungdotemp.Latitude = list[i].Latitude;
                                    dungdotemp.Longitude = list[i].Longitude;
                                    dungdotemp.TypeTransportName = list[0].TypeTransportName;
                                    // add vao danh sach
                                    ListPauseStopSingle.Add(dungdotemp);
                                    diachi = "";
                                    toado = "";
                                }
                            }
                        } //end list

                        try
                        {
                            string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                            DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                    "yy-MM-dd",
                                                                                    CultureInfo.InvariantCulture));
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                        "yy-MM-dd", CultureInfo.InvariantCulture));

                            for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                            {
                                // logic here
                                bool fAdd = true;
                                foreach (PauseStop var in ListPauseStopSingle)
                                {
                                    if (DateTime.Compare(var.Date.Date, date) == 0)
                                    {
                                        fAdd = false;
                                    }
                                }
                                if (fAdd)
                                {
                                    var dungdotemp = new PauseStop();
                                    DriverC drivertt =
                                        getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                                    if (drivertt != null)
                                    {
                                        dungdotemp.NameDriver = drivertt.NameDriver;
                                        dungdotemp.DriverLicense = drivertt.DriverLicense;
                                    }
                                    else
                                    {
                                        dungdotemp.NameDriver = "";
                                        dungdotemp.DriverLicense = "";
                                    }
                                    //dungdotemp.NameDriver = "không xác định";
                                    //dungdotemp.DriverLicense = "không xác định";


                                    dungdotemp.Status = "";
                                    dungdotemp.VehicleNumber = VehicleNumber;
                                    dungdotemp.count = count;
                                    dungdotemp.DateTime = ConverteDateTime(date);
                                    dungdotemp.Date = date;
                                    dungdotemp.Duration = "0p";
                                    dungdotemp.Address = "";
                                    dungdotemp.Coordinates = "";
                                    dungdotemp.TypeTransportName = getDevivebyDeviceIDT(_deviceID).TypeTransportName;
                                    ListPauseStopSingle.Add(dungdotemp);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.GetBaseException();
                        }
                        ListPauseStopSingle = ListPauseStopSingle.OrderBy(m => m.Date).ToList();

                        foreach (PauseStop vars in ListPauseStopSingle)
                        {
                            ListPauseStop.Add(vars);
                        }
                    }
                }
            }
            return ListPauseStop;
        }
        public Device getDevivebyDeviceIDT(String _DeviceID)
        {
            Dictionary<String, String> parameter = new Dictionary<string, string>();
            parameter.Add("_DeviceID", _DeviceID);
            Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetDevice_byDeviceIDT",
                                                                     parameter).First();
            return device;
        }

        public IList<TimeDriver> TimeDriver(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            int count = 0;
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value; // pair.Key == "IDs"
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            IList<TimeDriver> listTimeDriver = new List<TimeDriver>();
            try
            {
                for (int j = 0; j < _arrIDs.Length; j++)
                {
                    string _deviceID = _arrIDs[j];
                    if (!String.IsNullOrEmpty(_deviceID))
                    {
                        IList<GpsDataExt> list = new List<GpsDataExt>();
                        switch (iDate)
                        {
                            case 1:
                                {
                                    Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                    paramOld["_DeviceID"] = _deviceID;
                                    paramOld["_from"] = dataid["_from"];
                                    paramOld["_to"] = dataid["_to"];
                                    list = backupServiceT.DataForTimeDriver(paramOld);
                                    break;
                                }
                            case 2:
                                {
                                    Dictionary<string, string> parameter = new Dictionary<string, string>();
                                    parameter["_DeviceID"] = _deviceID;
                                    parameter["_from"] = dataid["_from"];
                                    parameter["_to"] = dataid["_to"];
                                    list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byIDT",
                                                                                        parameter).ToList();
                                    break;
                                }
                            case 3:
                                {
                                    Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                    paramOld["_DeviceID"] = _deviceID;
                                    paramOld["_from"] = dataid["_from_old"];
                                    paramOld["_to"] = dataid["_to_old"];
                                    IList<GpsDataExt> list_backup = backupServiceT.DataForTimeDriver(paramOld);
                                    Dictionary<string, string> parameter = new Dictionary<string, string>();
                                    parameter["_DeviceID"] = _deviceID;
                                    parameter["_from"] = dataid["_from_new"];
                                    parameter["_to"] = dataid["_to_new"];
                                    IList<GpsDataExt> listNew =
                                        Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byIDT",
                                                                                     parameter).ToList();
                                    list = list_backup.Union(listNew).ToList();
                                    break;
                                }
                        }
                        String from = "20" + param["From"];
                        String to = "20" + param["To"];
                        DateTime dateFrom = DateTime.Parse(from);
                        DateTime dateTo = DateTime.Parse(to);
                        if (list.FirstOrDefault() != null)
                        {
                            list = list.Where(m => m.DateSave.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave) >= 0).OrderBy(m => m.DateSave).ToList();
                        }
                        if (list.FirstOrDefault() != null)
                        {
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8)
                                        ,
                                        "yy-MM-dd", CultureInfo.InvariantCulture));

                            if (list[list.Count - 1].DateSave.Date > dto.Date)
                                list.RemoveAt(list.Count - 1);
                            IList<TimeDriver> listTimeDriverSingle = new List<TimeDriver>();
                            string bienso = "";
                            bool flag = true;
                            bool flag2 = false;
                            int temp1 = 0;
                            int temp2 = 0;
                            int lstStart = 0;
                            int lstEnd = 0;
                            string ngay = "";
                            string diadiembatdau = "";
                            string toadobatdau = "";
                            string DiaDiemKetThuc = "";
                            string ToaDoKetThuc = "";
                            bool bThedriver = false;
                            var tempstar = new DateTime();
                            var tempend = new DateTime();
                            string tempDriver = list.FirstOrDefault().TheDriver;
                            DriverC drivertemp = getDriverbyPhone(list.FirstOrDefault().DeviceID, tempDriver) ??
                                                 getDriverFirst("{'_DeviceID':'" + list.FirstOrDefault().DeviceID + "'}");
                            if (drivertemp == null)
                            {
                                drivertemp = new DriverC();
                                drivertemp.NameDriver = "";
                                drivertemp.DriverLicense = "";
                            }
                            string day = list.FirstOrDefault().DateSave.ToShortDateString();
                            int countEnd = 0;
                            int speed0 = 0;
                            decimal log = 0, lat = 0;
                            DateTime dateSave0;
                            for (int i = 0; i < list.Count; i++)
                            {
                                bool newRow = false;
                                double distemp = 0.0;
                                // nếu xe chạy 
                                if (list[i].Speed > 0)
                                {
                                    // nếu chưa có điểm bắt đầu, tuc flag = true
                                    // ta sẽ lấy thông tin điểm bắt đầu                                   
                                    if (flag)
                                    {
                                        count++;
                                        countEnd = 1;
                                        tempstar = list[i].DateSave;
                                        toadobatdau = list[i].Latitude + "," + list[i].Longitude;
                                        bienso = list[i].VehicleNumber;
                                        diadiembatdau = !string.IsNullOrEmpty(list[i].Address)
                                                            ? list[i].Address
                                                            : "Undefined";
                                        lstStart = i;
                                        // bat co len da co diem bat dau roi
                                        flag = false;
                                    }
                                    else
                                    {
                                        countEnd++;
                                        // neu toan bo danh sach co van toc > 0
                                        // ta se lay diem ket thuc chinh la cuoi danh sach
                                        if (countEnd == list.Count)
                                        {
                                            tempend = list[i].DateSave;
                                            ToaDoKetThuc = list[i].Latitude + "," + list[i].Longitude;
                                            DiaDiemKetThuc = !string.IsNullOrEmpty(list[i].Address)
                                                                 ? list[i].Address
                                                                 : "Undefined";
                                            lstEnd = i;
                                            distemp = Math.Round(CalculateDistance(list, lstStart, lstEnd));
                                            if (distemp == 0) // khong cho phep add neu khoang cach =0
                                            {
                                                flag = true;
                                                flag2 = false;
                                            }
                                            else
                                                flag2 = true;
                                        }
                                        // neu khac ngay ta lay diem ket thuc chinh la gan sat diem khac ngay
                                        else if (list[i].DateSave.ToShortDateString() != day ||
                                                 tempDriver != list[i].TheDriver)
                                        {
                                            tempend = list[i - 1].DateSave;
                                            ToaDoKetThuc = list[i - 1].Latitude + "," + list[i - 1].Longitude;
                                            DiaDiemKetThuc = !string.IsNullOrEmpty(list[i - 1].Address)
                                                                 ? list[i - 1].Address
                                                                 : "Undefined";
                                            lstEnd = i - 1;
                                            // cap nhat ngay moi hoac tai xe moi
                                            if (tempDriver != list[i].TheDriver)
                                                tempDriver = list[i].TheDriver;
                                            else day = list[i].DateSave.ToShortDateString();

                                            distemp = Math.Round(CalculateDistance(list, lstStart, lstEnd));
                                            if (distemp == 0) // khong cho phep add neu khoang cach =0
                                            {
                                                flag = true;
                                                flag2 = false;
                                            }
                                            else
                                                flag2 = true;
                                        }
                                    }
                                }
                                // nếu xe dừng 
                                else if (list[i].Speed == 0 || i == list.Count - 1)
                                {
                                    countEnd = 0;
                                    // nếu điểm bắt đầu trước đó đã có
                                    // ta sẽ lấy thông tin điểm kết thúc                                  
                                    if (!flag)
                                    {
                                        tempend = list[i].DateSave;
                                        if (i > 0 && tempend.Subtract(list[i - 1].DateSave).TotalMinutes > 15)
                                        {
                                            tempend = list[i - 1].DateSave;
                                        }
                                        ToaDoKetThuc = list[i].Latitude + "," + list[i].Longitude;
                                        DiaDiemKetThuc = !string.IsNullOrEmpty(list[i].Address)
                                                             ? list[i].Address
                                                             : "Undefined";
                                        lstEnd = i;
                                        processData(ref i, list, listTimeDriverSingle,
                                                    ref lstEnd, ref tempend, ref ToaDoKetThuc, ref DiaDiemKetThuc,
                                                    ref lstStart, ref tempstar, ref toadobatdau, ref diadiembatdau,
                                                    ref bThedriver, ref day,
                                                    ref tempDriver, ref bienso, true, ref newRow);
                                        //*************************************
                                        // tính khoảng cách.Nếu khoảng cách bằng 0
                                        // thì không cho phép thêm vào
                                        distemp = Math.Round(CalculateDistance(list, lstStart, lstEnd));
                                        if (distemp == 0)
                                        {
                                            flag = true;
                                            flag2 = false;
                                        }
                                        else
                                            flag2 = true;
                                    } //end 

                                    // neu khong co diem bat dau
                                    else
                                    {
                                        tempend = list[i].DateSave;
                                        if (i > 0 && tempend.Subtract(list[i - 1].DateSave).TotalMinutes > 15)
                                        {
                                            tempend = list[i - 1].DateSave;
                                        }
                                        processData(ref i, list, listTimeDriverSingle,
                                                    ref lstEnd, ref tempend, ref ToaDoKetThuc, ref DiaDiemKetThuc,
                                                    ref lstStart, ref tempstar, ref toadobatdau, ref diadiembatdau,
                                                    ref bThedriver, ref day,
                                                    ref tempDriver, ref bienso, false, ref newRow);
                                        distemp = Math.Round(CalculateDistance(list, lstStart, lstEnd));
                                        if (distemp == 0)
                                        {
                                            flag = true;
                                            flag2 = false;
                                        }
                                        else
                                            flag2 = true;
                                    }
                                }

                                // cho phép thêm vào
                                if (flag2)
                                {
                                    flag = true;
                                    flag2 = false;
                                    var laixetemp = new TimeDriver();
                                    double difference = tempend.Subtract(tempstar).TotalMinutes;
                                    laixetemp.TimeDriver_ = ConverteTime(difference);
                                    laixetemp.Date = tempstar.ToShortDateString();
                                    laixetemp.date2 = DateTime.Parse(tempstar.ToShortDateString());
                                    laixetemp.TypeTransportName = list[0].TypeTransportName;
                                    laixetemp.VehicleNumber = bienso;

                                    laixetemp.Start = tempstar;
                                    laixetemp.End = tempend;
                                    laixetemp.TimeStart = tempstar.TimeOfDay.ToString();
                                    laixetemp.TimeEnd = tempend.TimeOfDay.ToString();
                                    laixetemp.stimedriver = difference;
                                    laixetemp.AddressStart = diadiembatdau;
                                    laixetemp.CoordinatesStart = toadobatdau;
                                    laixetemp.AddressEnd = DiaDiemKetThuc;
                                    laixetemp.CoordinatesEnd = ToaDoKetThuc;
                                    laixetemp.sDistance = distemp;
                                    laixetemp.Distance = laixetemp.sDistance + " km";

                                    laixetemp.NameDriver = drivertemp.NameDriver;
                                    laixetemp.theDriver = bThedriver ? list[i - 1].TheDriver : list[i].TheDriver;

                                    laixetemp.DriverLicense = drivertemp.DriverLicense;

                                    double avg = 0.0;
                                    int max = 0;
                                    CaculatorSpeedMax_AVG(list, lstStart, lstEnd, ref avg, ref max);
                                    if (max == 0)
                                        continue;
                                    laixetemp.SpeedMax = max + " km/h";
                                    laixetemp.SpeedAVG = Math.Round(avg) + " km/h";
                                    if (difference > 1380)
                                    {
                                        difference = 1380;
                                        laixetemp.TimeDriver_ = ConverteTime(difference);
                                    }
                                    listTimeDriverSingle.Add(laixetemp);

                                    if (i != list.Count - 1 && newRow == false)
                                        i--;

                                    lstStart = lstEnd;
                                }
                                // neu khac tai xe thi cap nhat lai thong tin tai xe                               
                                if (bThedriver)
                                {
                                    drivertemp = getDriverbyPhone(list[i].DeviceID, tempDriver) ??
                                                 getDriverFirst("{'_DeviceID':'" + list[i].DeviceID + "'}");
                                    if (drivertemp == null)
                                    {
                                        drivertemp = new DriverC();
                                        drivertemp.NameDriver = "";
                                        drivertemp.DriverLicense = "";
                                    }
                                    bThedriver = false;
                                }
                            } //end list

                            #region "Truong hop khong co data"

                            try
                            {
                                string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                                DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                        "yy-MM-dd",
                                                                                        CultureInfo.InvariantCulture));
                                DateTime dto2 =
                                    Convert.ToDateTime(
                                        DateTime.ParseExact(
                                            param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                            "yy-MM-dd", CultureInfo.InvariantCulture));

                                for (DateTime date = dfrom.Date; date <= dto2.Date; date = date.AddDays(1))
                                {
                                    // logic here
                                    bool fAdd = true;
                                    foreach (TimeDriver var in listTimeDriverSingle)
                                    {
                                        if (DateTime.Compare(var.date2.Date, date) == 0)
                                        {
                                            fAdd = false;
                                        }
                                    }
                                    if (fAdd)
                                    {
                                        var laixetemp = new TimeDriver();
                                        laixetemp.TimeDriver_ = "0p";
                                        laixetemp.count = count;
                                        laixetemp.Date = date.ToShortDateString();
                                        laixetemp.date2 = date;
                                        laixetemp.VehicleNumber =
                                            getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;
                                        laixetemp.TimeStart = "";
                                        laixetemp.TimeEnd = "";
                                        laixetemp.AddressStart = "";
                                        laixetemp.CoordinatesStart = "";
                                        laixetemp.AddressEnd = "";
                                        laixetemp.CoordinatesEnd = "";
                                        laixetemp.SpeedAVG = "0 km/h";
                                        laixetemp.SpeedMax = "0 km/h";
                                        laixetemp.Distance = "0 km";
                                        laixetemp.stimedriver = 0;
                                        laixetemp.sDistance = 0;
                                        laixetemp.TypeTransportName = getDevivebyDeviceIDT(_deviceID).TypeTransportName;
                                        DriverC drivertt =
                                            getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                                        if (drivertt != null)
                                        {
                                            laixetemp.NameDriver = drivertt.NameDriver;
                                            laixetemp.DriverLicense = drivertt.DriverLicense;
                                        }
                                        else
                                        {
                                            laixetemp.NameDriver = "";
                                            laixetemp.DriverLicense = "";
                                        }
                                        listTimeDriverSingle.Add(laixetemp);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.GetBaseException();
                            }

                            #endregion

                            //Add data vao danh sach
                            listTimeDriverSingle = listTimeDriverSingle.OrderBy(m => m.date2).ToList();
                            foreach (TimeDriver vars in listTimeDriverSingle)
                            {
                                listTimeDriver.Add(vars);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return listTimeDriver;
        }

        private void CaculatorSpeedMax_AVG(IList<GpsDataExt> list, int temp1, int temp2, ref double avg, ref int max)
        {
            max = list[temp1].Speed.Value;
            int sum = 0;
            int tempCount = temp2 - temp1;
            for (int k = temp1; k < temp2; k++)
            {
                sum += list[k].Speed.Value;
                if (max < list[k].Speed)
                    max = list[k].Speed.Value;
            }
            avg = sum / tempCount;
        }


        private int kiemTraXeNghi(ref Boolean fAdvance, IList<GpsDataExt> list, int i, DateTime tempend,
                                  ref string tempDriver, ref int cut, ref string day, ref Boolean bThedriver)
        {
            int t = -1, k;
            fAdvance = false;
            for (k = i; k < list.Count; k++)
            {
                if (list[k].Speed > 0)
                {
                    t = k;
                    break;
                }
                //neu khac tai xe hoac khac ngay
                if (tempDriver != list[k].TheDriver ||
                    !day.Equals(list[k].DateSave.ToShortDateString()))
                {
                    //neu khac tai xe
                    if (tempDriver != list[k].TheDriver)
                    {
                        cut = -2;
                        tempDriver = list[k].TheDriver;
                        bThedriver = true;
                    }
                    //neu khac ngay
                    else if (!day.Equals(list[k].DateSave.ToShortDateString()))
                    {
                        day = list[k].DateSave.ToShortDateString();
                        cut = -3;
                    }
                    return k - 1;
                }
                if (list[k].DateSave.Subtract(tempend).TotalMinutes > 15)
                {
                    cut = -1;
                    int j;
                    //kiem tra xem cac phan tu con lai van toc co =0 hay ko
                    for (j = k; j < list.Count; j++)
                        if (list[j].Speed > 0)
                            break;
                    if (j == list.Count) //neu co
                        cut = -4;
                    return k - 1;
                }
            }
            if (t != -1)
            {
                fAdvance = true;
                //tinh khoang thoi gian nghi
                double subtime = list[t].DateSave.Subtract(tempend).TotalMinutes;
                //neu giong tai xe va giong ngay va thoi gian nghi < 15 phut
                if (tempDriver == list[t].TheDriver && subtime < 15 && day == list[t].DateSave.ToShortDateString())
                {
                    cut = 1;
                    return t;
                }
                //neu khac tai xe
                if (tempDriver != list[t].TheDriver)
                {
                    //cap nhat tai xe moi
                    cut = -2;
                    tempDriver = list[t].TheDriver;
                    bThedriver = true;
                    return t - 1;
                }
                //neu khac ngay
                if (!day.Equals(list[t].DateSave.ToShortDateString()))
                {
                    //cap nhat ngay moi
                    day = list[t].DateSave.ToShortDateString();
                    cut = -3;
                    return t - 1;
                }
            }
            if (k == list.Count) cut = -4; //kiem tra cac phan tu con lai co = 0 hay ko
            return t;
        }

        private void processData(ref int i, IList<GpsDataExt> list, IList<TimeDriver> listTimeDriverSingle,
                                 ref int lstEnd, ref DateTime tempend, ref string ToaDoKetThuc,
                                 ref string DiaDiemKetThuc,
                                 ref int lstStart, ref DateTime tempstart, ref string ToaDoBatDau,
                                 ref string DiaDiemBatDau,
                                 ref bool bThedriver, ref string day,
                                 ref string tempDriver, ref string bienso, bool isEnd, ref bool newRow)
        {
            //*************************************
            //Điểm bắt đầu là A
            // tìm đến điểm có tốc độ khác 0 để tính khoảng cách thời gian nghĩ có nhỏ hơn 15 phút hay không
            // Nếu nhỏ hơn(thực hiện cộng gộp lại khoảng cách và thời gian) ta tìm tiếp điểm có vận tốc = 0 phía sau(B) để lấy khoản cách từ A  đến B.
            // Nếu lớn hơn (thực hiện Add thêm một dòng mới) ta tìm đến vận tốc bằng 0 tiếp theo để tính khoảng cách

            int t;
            bool fAdvance = false;
            int cut = 0;
            int position = 0;
            if (isEnd == false && listTimeDriverSingle.Count <= 0)
                tempend = list[0].DateSave;
            t = kiemTraXeNghi(ref fAdvance, list, i, tempend, ref tempDriver, ref cut, ref day, ref bThedriver);
            //van toc luc sau toan =0
            if (cut == -4)
            {
                lstStart = i;
                lstEnd = i;
                i = list.Count - 1;
                return;
            }

            // truong hop khong co diem bat dau 
            // ta tim diem bat dau chinh la luc co van toc luc sau
            if (isEnd == false)
            {
                if (!fAdvance) //van toc =0
                {
                    do
                    {
                        cut = 0;
                        //tim diem bat dau co van toc >0
                        t = kiemTraXeNghi(ref fAdvance, list, t + 1, tempend, ref tempDriver, ref cut, ref day,
                                          ref bThedriver);
                        tempend = !fAdvance ? list[t + 1].DateSave : list[t].DateSave;
                        if (fAdvance || cut == -4) break;
                    } while (true); //tim van toc > 0

                    if (cut == -4) //toan bo van toc luc sau deu =0
                    {
                        lstStart = i;
                        lstEnd = i;
                        i = list.Count - 1;
                        return;
                    }
                }
                //lay thoi gian bat dau luc co van toc >0
                if (fAdvance && (cut == -2 || cut == -3))
                    t = t + 1;
                tempstart = list[t].DateSave;
                //neu duoi 15phut ta lay diem bat dau tai diem truoc do
                // nguoc lai ta lay diem dat van toc > 15phut
                lstStart = cut == 1 ? i : t;
                ToaDoBatDau = list[t].Latitude + "," + list[t].Longitude;
                bienso = list[t].VehicleNumber;
                DiaDiemBatDau = !string.IsNullOrEmpty(list[t].Address)
                                    ? list[t].Address
                                    : "Undefined";
            } //end if

            int t2 = t;
            int indexEnd = i;
            //nếu nghĩ dưới 15 phút và có vận tốc lúc sau thì
            // ta tiếp tục tìm các đoạn có thời gian nghĩ dưới 15 phút để lấy thông tin điểm cuối cùng
            if ((fAdvance && cut == 1) || !isEnd)
            {
                int cout = -1;
                bool f2 = false; //kiem tra toan bo van toc luc sau deu > 0
                //thực hiện  tìm khoảng thời gian nghĩ  dưới 15 phút
                for (int z = t2; z < list.Count; z++)
                {
                    //nếu vận tốc bằng 0 ta tìm đến lúc xe chạy và xét thời gian nghĩ
                    if (list[z].Speed == 0)
                    {
                        f2 = true;
                        bool f0 = false;
                        tempend = list[z].DateSave;
                        int t3 = -1;
                        int cut2 = 0;

                        t3 = kiemTraXeNghi(ref f0, list, z, tempend, ref tempDriver, ref cut2, ref day, ref bThedriver);
                        if (f0 && cut2 == 1) //dieu kien < 15phut
                        {
                            indexEnd = z;
                            z = t3 - 1;
                            cout = list.Count - z - 1;
                        }
                        else
                        {
                            // neu > 15 phut hoac khac tai xe hoac khac ngay: ta tach ra
                            // lay vi tri ket thuc gan xac diem khac tai xe hoac ngay
                            if (cut2 == -2 || cut2 == -3) // khac tai xe hoac khac ngay  
                            {
                                newRow = true;
                                position = t3 - 1;
                            }
                            else if (cut2 == -4) //neu cac van toc luc sau deu =0 den cuoi
                            {
                                indexEnd = z;
                                position = list.Count - 2;
                                break;
                            }
                            else position = t3; //lay tai vi tri xac gioi han
                            //lay tai diem co van toc 0 dau tien
                            indexEnd = z;
                            break;
                        }
                    } //end speed = 0

                    else if (list[z].Speed > 0) //xe dang chay
                    {
                        cout--;
                        // kiem tra xem tac ca phan tu con lai co > 0 hay khong
                        if (cout == 0)
                        {
                            indexEnd = z;
                            position = z - 1;
                            break;
                        }
                        // kiem tra khac ngay hoac khac tai xe
                        if (!day.Equals(list[z].DateSave.ToShortDateString()) || !tempDriver.Equals(list[z].TheDriver))
                        {
                            if (!day.Equals(list[z].DateSave.ToShortDateString()))
                                day = list[z].DateSave.ToShortDateString();
                            else
                            {
                                tempDriver = list[z].TheDriver;
                                bThedriver = true;
                            }
                            //lay xac vitri gioi han
                            indexEnd = z - 1;
                            position = indexEnd;
                            newRow = true;
                            break;
                        }
                        if (z < list.Count - 2)
                        {
                            //neu van toc ke khac ngay ta ngat 
                            double subtime = list[z + 1].DateSave.Subtract(list[z].DateSave).TotalMinutes;
                            if (!list[z].DateSave.ToShortDateString().Equals(list[z + 1].DateSave.ToShortDateString()))
                            {
                                day = list[z + 1].DateSave.ToShortDateString();
                                indexEnd = z;
                                position = z - 1;
                                newRow = true;
                                break;
                            }

                            //truong hop GPS bi mat khoang thoi gian
                            if (subtime > 15 && list[z + 1].Speed == 0)
                            {
                                indexEnd = z;
                                position = z + 1;
                                break;
                            }
                        }
                    }
                } //end for
                // truong hop cac phan tu phia sau khong co van toc = 0. chi co van toc > 0
                //if (f2 == false && !newRow)
                //{
                //    indexEnd = list.Count - 1;
                //    position = indexEnd - 1;
                //}
                i = position + 1;
                lstEnd = indexEnd;
                tempend = list[indexEnd].DateSave;
                ToaDoKetThuc = list[indexEnd].Latitude + "," + list[indexEnd].Longitude;
                DiaDiemKetThuc = !string.IsNullOrEmpty(list[indexEnd].Address)
                                     ? list[indexEnd].Address
                                     : "Undefined";
            } //end if < 15

            // truong hop > 15 phut
            // ta Add vao danh sach
            else
            {
                if (cut == -2 || cut == -3) // khac tai xe hoac khac ngay
                {
                    newRow = true;
                    i = t;
                    //neu van toc >0 ta lay diem gan xac khac tai xe hoac ngay

                    //if (fAdvance)
                    //{
                    //    lstEnd = t;
                    //    tempend = list[t].DateSave;
                    //    ToaDoKetThuc = list[t].Latitude + "," + list[t].Longitude;
                    //    DiaDiemKetThuc = !string.IsNullOrEmpty(list[t].Address)
                    //                         ? list[i].Address
                    //                         : "Undefined";
                    //}//else ta lay diem cuoi ban dau
                } //else ta lay diem cuoi ban dau
                else i = t + 1;
            }
        }

        public int GetNumber(string text)
        {
            var exp = new Regex(@"(\d+)"); // find a sequence of digits could be \d+
            MatchCollection matches = exp.Matches(text);

            if (matches.Count == 1) // if there's one number return that
            {
                int number = int.Parse(matches[0].Value);
                return number;
            }
            else if (matches.Count > 1)
                throw new InvalidOperationException("only one number allowed");
            else
                return 0;
        }

        public IList<TimeDriver> TimeDriverVP10(Dictionary<string, string> param)
        {
            IList<TimeDriver> result = TimeDriver(param);
            IList<TimeDriver> response = new List<TimeDriver>();
            IList<TimeDriver> device = new List<TimeDriver>();
            string vehicleNumber = result.Count > 0 ? result[0].VehicleNumber : "";
            foreach (TimeDriver item in result)
            {
                //neu giong bien so
                if (vehicleNumber == item.VehicleNumber)
                {
                    if (item.sDistance == 0) continue;
                    if (device.Count == 0)
                        device.Add(item);
                    else
                    {
                        //neu giong ngay thi cap nhat lai thong tin(cong gop them khoang cach, thoi gian lai xe)               
                        if (item.date2.Day == device[device.Count - 1].date2.Day &&
                            item.theDriver == device[device.Count - 1].theDriver)
                        {
                            device[device.Count - 1].sDistance =
                                Math.Round(device[device.Count - 1].sDistance + item.sDistance);
                            device[device.Count - 1].Distance = device[device.Count - 1].sDistance + " km";

                            device[device.Count - 1].stimedriver += item.stimedriver;
                            device[device.Count - 1].TimeDriver_ = ConverteTime(device[device.Count - 1].stimedriver);

                            device[device.Count - 1].End = item.End;
                            device[device.Count - 1].CoordinatesEnd = item.CoordinatesEnd;
                            device[device.Count - 1].TimeEnd = item.TimeEnd;
                            device[device.Count - 1].AddressEnd = item.AddressEnd;


                            device[device.Count - 1].SpeedMax = GetNumber(device[device.Count - 1].SpeedMax) >
                                                                GetNumber(item.SpeedMax)
                                                                    ? device[device.Count - 1].SpeedMax
                                                                    : item.SpeedMax;
                            device[device.Count - 1].SpeedAVG =
                                Math.Round(
                                    (double)(GetNumber(device[device.Count - 1].SpeedAVG) + GetNumber(item.SpeedAVG) / 2)) +
                                " km/h";
                        }
                        //khac ngay thi them moi
                        else device.Add(item);
                    }
                } //end if out
                //khac bien so
                else
                {
                    //kiem tra dieu kien lai xe qua 10gio trong ngay
                    //neu thoa thi add vao danh sach
                    foreach (TimeDriver timeDriver in device)
                        if (timeDriver.stimedriver > 600)
                            response.Add(timeDriver);
                    vehicleNumber = item.VehicleNumber;
                    device = new List<TimeDriver>();
                }
            } //end for out
            if (device.Count > 0)
            {
                //lay device cuoi cung
                foreach (TimeDriver timeDriver in device)
                    if (timeDriver.stimedriver > 600)
                        response.Add(timeDriver);
            }
            return response;
        }

        public IList<TimeDriver> TimeDriverVP4(Dictionary<string, string> param)
        {
            IList<TimeDriver> result = TimeDriver(param);
            return result.Where(vars => vars.stimedriver > 240).ToList();
        }

        public int tongsolanvuottoc = 0;
        private Dictionary<int, string> CalSpeedPercent(IList<GpsDataExtForGeneral> list,
                                                        double distane_,
                                                        int speedLimit)
        {
            //_DeviceID
            // List<Device> GetDeviceByDeviceID(Dictionary<string, string> parameter)
            var ListVuotToc = new Dictionary<int, IList<GpsDataExtForGeneral>>();
            IList<ExceedingSpeed> Coor1 = new List<ExceedingSpeed>();
            IList<ExceedingSpeed> Coor2 = new List<ExceedingSpeed>();
            IList<ExceedingSpeed> Coor3 = new List<ExceedingSpeed>();
            IList<ExceedingSpeed> Coor4 = new List<ExceedingSpeed>();
            bool flag = true;
            bool flag2 = false;
            int t = 0;
            IList<GpsDataExtForGeneral> list_temp = new List<GpsDataExtForGeneral>();
            var result = new Dictionary<int, string>();
            if (list != null || list.Count > 0)
            {
                IList<ExceedingSpeed> list_vuottoc = CalExceedingSpeed(list, speedLimit);
                if (list_vuottoc != null)
                    tongsolanvuottoc = list_vuottoc.Count;

                for (int i = 0; i < list_vuottoc.Count; i++)
                {
                    if (!string.IsNullOrEmpty(list_vuottoc[i].Coordinates))
                    {
                        //list_temp = new List<GpsDataExtForGeneral>();
                        list_temp = list.Where(m => m.DataID >= list_vuottoc[i].dataIDStart && m.DataID <= list_vuottoc[i].dataIDEnd).ToList();
                        //if(list_temp.FirstOrDefault(m=>m.DataID==list_vuottoc[i].dataIDEnd).Speed<speedLimit)
                        //{
                        //    list_temp.Remove(list_temp.FirstOrDefault(m => m.DataID == list_vuottoc[i].dataIDEnd));
                        //}
                        if (list_temp.Count > 0)
                        {
                            ListVuotToc.Add(t, list_temp);
                            t += 1;
                        }

                    }

                }
                ExceedingSpeed exTemp = null;
                for (int i = 0; i < ListVuotToc.Keys.Count; i++)
                {
                    KeyValuePair<int, IList<GpsDataExtForGeneral>> list_ =
                        ListVuotToc.Where(m => m.Key == i).FirstOrDefault();
                    if (list_.Value.Count > 0)
                    {
                        exTemp = new ExceedingSpeed();
                        int vTrungBinh = 0;
                        int vTong = 0;
                        int vCount = 0;
                        exTemp.Coordinates = list_.Value[0].Latitude.Value + "," + list_.Value[0].Longitude.Value;
                        for (int j = 0; j < list_.Value.Count; j++)
                        {
                            //switch (checkCondition(int.Parse(list_.Value[j].Speed.ToString()), speedLimit))
                            if (list_.Value[j].Speed > speedLimit)
                            {
                                vCount += 1;
                                vTong += list_.Value[j].Speed.Value;
                            }
                            if (j == list_.Value.Count - 1)
                            {
                                exTemp.Coordinates_ketthuc = list_.Value[j].Latitude.Value + "," +
                                                                   list_.Value[j].Longitude.Value;
                            }
                        }
                        if (vCount > 0 && vTong > 0)
                        {
                            vTrungBinh = vTong / vCount;
                        }
                        else
                        {

                        }


                        switch (checkCondition(vTrungBinh, speedLimit))
                        {
                            case 1:
                                Coor1.Add(exTemp);
                                break;
                            case 2:
                                Coor2.Add(exTemp);
                                break;
                            case 3:
                                Coor3.Add(exTemp);
                                break;
                            case 4:
                                Coor4.Add(exTemp);
                                break;
                        }
                    }

                }
                var my_dic = new Dictionary<int, IList<ExceedingSpeed>>();
                my_dic.Add(0, Coor1);
                my_dic.Add(1, Coor2);
                my_dic.Add(2, Coor3);
                my_dic.Add(3, Coor4);

                for (int i = 0; i < my_dic.Keys.Count; i++)
                {
                    KeyValuePair<int, IList<ExceedingSpeed>> list_tempt = my_dic.Where(m => m.Key == i).FirstOrDefault();
                    String phantram = Calpercent(distane_, list_tempt.Value);
                    result.Add(i, phantram + "," + list_tempt.Value.Count.ToString());
                }
            }
            return result;
        }

        private String Calpercent(double distanne, IList<ExceedingSpeed> value)
        {
            double distane_temp = 0;
            String result = "0.00";
            if (value != null)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    distane_temp += CalculateDistance_(value[i].Coordinates, value[i].Coordinates_ketthuc);
                }
                if (distane_temp > distanne)
                {
                    distane_temp = distanne / 30;
                }
                result = float.Parse(((distane_temp / distanne) * 100).ToString()).ToString("0.00");
                if (result.Equals("NaN"))
                {
                    result = "0.00";
                }
            }

            return result;
        }

        private int checkCondition(int value, int speedLimit)
        {
            int i = 0;
            if (speedLimit + 5 <= value && value < speedLimit + 10)
            {
                i = 1;
            }
            else if (speedLimit + 10 <= value && value < speedLimit + 20)
            {
                i = 2;
            }
            else if (speedLimit + 20 <= value && value < speedLimit + 35)
            {
                i = 3;
            }
            else if (value > speedLimit + 35)
            {
                i = 4;
            }
            return i;
        }
        public Dictionary<string, string> getAllDataID_From_To(Dictionary<String, String> param, int checkDate)
        {
            //string totemp = param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(3, 2);
            //string fromtemp = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(3, 2);
            Dictionary<string, string> result = new Dictionary<string, string>();
            try
            {
                string totemp = param.FirstOrDefault(pair => pair.Key == "To").Value;
                string fromtemp = param.FirstOrDefault(pair => pair.Key == "From").Value;
                Dictionary<string, string> parameter = new Dictionary<string, string>();
                parameter["_from"] = fromtemp.Substring(0, 8) + " 00:00:00";
                parameter["_to"] = totemp.Substring(0, 8) + " 23:59:59";
                list_dataID = Repository.ExecuteStoreProceduce<tempdatesave>("sp_DataID_from_to", parameter).ToList();
                IList<tempdatesave> DataIDOld = list_dataID.Where(m => m.DateSave.Value.Date.Month != DateTime.Now.Date.Month).OrderBy(m => m.DataID).ToList();
                IList<tempdatesave> DataIDNew =
                    list_dataID.Where(
                        m =>
                        m.DateSave.Value.Date.Month == DateTime.Now.Date.Month).OrderBy(m => m.DataID).ToList();

                if (DateTime.Now.Date.Day < 5 && DataIDOld.Count > 0)
                {
                    int DataIDOldCount = DataIDOld.Count, monthNow = DateTime.Now.Month;
                    if (DataIDOld[DataIDOldCount - 1].DateSave.Value.Date.Month == DateTime.Now.AddMonths(-1).Month)
                    {
                        IList<tempdatesave> DataIDOld_temp = new List<tempdatesave>();
                        DataIDNew = DataIDNew.Union
                        (DataIDOld.Where(n => (n.DateSave.Value.Date.Month == DateTime.Now.AddMonths(-1).Month)).ToList())
                        .ToList().OrderBy(m => m.DataID).ToList();
                        //DataIDOld.ToList().RemoveAll(m=>(monthNow-m.DateSave.Value.Date.Month)==1);

                        for (int i = 0; i < DataIDOldCount; i++)
                        {
                            if (DataIDOld[i].DateSave.Value.Month != DateTime.Now.AddMonths(-1).Month)
                            {
                                DataIDOld_temp.Add(DataIDOld[i]);
                            }
                        }
                        DataIDOld = DataIDOld_temp;
                    }
                }
                result = new Dictionary<string, string>();
                if (DataIDNew.Count > 0 && DataIDNew[DataIDNew.Count - 1].DateSave.Value.Date.Day == DateTime.Now.Date.Day)
                {
                    tempdatesave recordLast = Repository.ExecuteStoreProceduce<tempdatesave>("sp_DataID_Lastest").FirstOrDefault();
                    DataIDNew.Add(recordLast);
                    list_dataID.Add(recordLast);
                    DataIDNew.OrderBy(m => m.DataID);
                }
                switch (checkDate)
                {
                    case 1:
                        result["_from"] = DataIDOld[0].DataID.ToString();
                        result["_to"] = DataIDOld[DataIDOld.Count - 1].DataID.ToString();
                        break;
                    case 2:
                        result["_from"] = DataIDNew[0].DataID.ToString();
                        result["_to"] = DataIDNew[DataIDNew.Count - 1].DataID.ToString();
                        break;
                    case 3:
                        result["_from_old"] = DataIDOld[0].DataID.ToString();
                        result["_to_old"] = DataIDOld[DataIDOld.Count - 1].DataID.ToString();
                        result["_from_new"] = DataIDNew[0].DataID.ToString();
                        result["_to_new"] = DataIDNew[DataIDNew.Count - 1].DataID.ToString();
                        break;
                }
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
                return result;
            }

        }
        private Dictionary<string, long> getDataIDByDate(DateTime dateIn)
        {
            Dictionary<string, long> result = new Dictionary<string, long>();
            IList<tempdatesave> listTemp = list_dataID.Where(m => m.DateSave.Value.Date.Equals(dateIn.Date)).OrderBy(m => m.DataID).ToList();
            result["from"] = listTemp.Count > 0 ? listTemp[0].DataID.Value : 0;
            result["to"] = listTemp.Count > 0 ? listTemp[listTemp.Count - 1].DataID.Value : 0;
            return result;
        }


        private IList<General> ReportGeneralOld(Dictionary<string, string> param)
        {
            int count = 0;
            int iDate = checkDateInt(param);
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            IList<General> listGeneral = new List<General>();
            for (int t = 0; t < _arrIDs.Length; t++)
            {
                string VehicleNumber = "";
                string _deviceID = _arrIDs[t];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    IList<GpsDataExtForGeneral> list = new List<GpsDataExtForGeneral>();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
                                paramOld["_DeviceID"] = _deviceID;
                                list = backupServiceT.DataForReportGeneral(paramOld);
                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = getParamNew(param);
                                parameter["_DeviceID"] = _deviceID;
                                list = Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
                                                                                    parameter).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
                                paramOld["_DeviceID"] = _deviceID;
                                list = backupServiceT.DataForReportGeneral(paramOld);
                                string monthNow = DateTime.Now.Month.ToString();
                                if (monthNow.Length == 1)
                                {
                                    monthNow = "0" + monthNow;
                                }
                                param["From"] = param["To"].Substring(0, 2) + "-" + monthNow + "-" + "01 " +
                                                param["From"].Substring(9);
                                Dictionary<string, string> parameter = getParamNew(param);
                                parameter["_DeviceID"] = _deviceID;
                                IList<GpsDataExtForGeneral> listNew =
                                    Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
                                                                                 parameter).ToList();
                                foreach (var gpsDataExt in listNew)
                                {
                                    list.Add(gpsDataExt);
                                }
                                break;
                            }
                    }
                    //parameter["_DeviceID"] = _deviceID;
                    //IList<GpsDataExtForGeneral> list =
                    //    Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
                    //                                                           parameter).ToList();
                    Device device = null;
                    IList<General> listGeneralSigle = new List<General>();
                    if (list.FirstOrDefault() != null)
                    {
                        Dictionary<string, string> parameter2 = new Dictionary<string, string>();
                        parameter2.Add("_DeviceID", _deviceID);
                        device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicleT",
                                                                                 parameter2).First();
                        int switch_tat = 0;
                        int switch_mo = 1;
                        int switch_door_dong = 0;
                        int switch_door_mo = 1;
                        int SpeedLimit = 80;
                        if (device != null)
                        {
                            SpeedLimit = device.SpeedLimit.Value;
                            VehicleNumber = device.VehicleNumber;
                            if (device.Switch_ == 1)
                            {

                                switch_tat = 1;
                                switch_mo = 0;
                            }
                            if (device.Switch_Door == 1)
                            {
                                switch_door_dong = 1;
                                switch_door_mo = 0;
                            }
                        }


                        // double soGio = DateTime.Parse(day_end + " 11:59:59 PM").Subtract(DateTime.Parse(day + " 12:00:00 AM")).TotalMinutes;

                        int solanvuottoc = 0;
                        int solandongmocua = 0;
                        int solandungdo = 0;
                        int STimeVP4 = 0;
                        double laixelientuc = 0;
                        double laixelientucOld = 0;
                        double thoigiandung = 0;

                        string thoigianlaixe = "";
                        string thoigiandungtemp = "";
                        string namedriver = "";
                        string gplx = "";
                        double vantocTB = 0;
                        bool flag = false;
                        bool flag2 = false;
                        bool flag3 = false;
                        bool flag4 = false;
                        bool flag5 = false;
                        bool f = false;
                        DateTime datestart = new DateTime();
                        DateTime dateend = new DateTime();
                        DateTime dateStartSpeed = new DateTime();
                        DateTime dateRS = new DateTime();
                        int countTB = 0;
                        string tempdate = "";
                        int batdau = 0;
                        int ketthuc = 0;
                        int j = 0; //batdau
                        int k = 0; //ketthuc
                        //IList<Start_end> list_start = On_Off("0", imei, day,day_end);
                        //for (int j = 0; j < list_start.Count; j++) {
                        //    laixelientuc += Math.Round(double.Parse(list_start[j].total));
                        //}

                        double max_v = 0;
                        bool lxf = true;
                        bool lxf2 = false;
                        double qd = 0;
                        int i_lx = 0;
                        string thedriver = list[0].TheDriver;
                        DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
                                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                        if (drivertemp != null)
                        {
                            namedriver = drivertemp.NameDriver;
                            gplx = drivertemp.DriverLicense;
                        }
                        else
                        {
                            namedriver = "không xác định";
                            gplx = "không xác định";
                        }
                        STimeVP4 = 0;
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (!f)
                            {
                                tempdate = list[i].DateSave.Value.ToShortDateString();
                                dateRS = list[i].DateSave.Value;
                                j = i;
                                f = true;
                                STimeVP4 = 0;
                                laixelientucOld = 0;
                            }
                            else // if (f)
                            {
                                string tempdate2 = list[i].DateSave.Value.ToShortDateString();
                                if (!tempdate2.Equals(tempdate))
                                {

                                    k = i;
                                    i = k - 1;
                                    i_lx++;
                                    f = false;
                                }
                                else // if (tempdate2.Equals(tempdate))
                                {
                                    if (i == list.Count - 1)
                                    {
                                        k = i;
                                    }
                                }
                                if (k > j)
                                {

                                    if (thedriver != list[i].TheDriver)
                                    {
                                        drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
                                                     getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
                                        if (drivertemp != null)
                                        {
                                            namedriver = drivertemp.NameDriver;
                                            gplx = drivertemp.DriverLicense;
                                            thedriver = list[i].TheDriver;
                                        }
                                        else
                                        {
                                            namedriver = "không xác định";
                                            gplx = "không xác định";
                                        }
                                    }

                                    max_v = list[j].Speed.Value;
                                    batdau = j;
                                    ketthuc = k;
                                    qd = CalculateDistanceForGPSDataG(list, j, k);
                                    laixelientuc = 0;

                                    STimeVP4 = 0;
                                    for (int m = j; m < k; m++)
                                    {



                                        if (max_v < list[m].Speed)
                                        {
                                            max_v = list[m].Speed.Value;
                                        }
                                        if (list[m].Speed >= SpeedLimit)
                                        {
                                            //if (!flag && !list[m].Address.Contains("Tp.HCM - Trung Lương"))
                                            //{
                                            //    dateStartSpeed = list[m].DateSave.Value;
                                            //    flag = true;
                                            //}
                                        }
                                        else if (list[m].Speed < SpeedLimit)
                                        {
                                            if (flag)
                                            {
                                                if (list[m].DateSave.Value.Subtract(dateStartSpeed).TotalSeconds > 120)
                                                {
                                                    solanvuottoc++;

                                                }
                                                flag = false;
                                            }
                                        }
                                        if (list[m].StatusDoor.Equals(switch_door_mo))
                                        {
                                            if (!flag2)
                                            {
                                                flag2 = true;
                                            }
                                        }
                                        else
                                        {
                                            if (flag2)
                                            {
                                                if (list[m].Speed < 30)
                                                {
                                                    solandongmocua++;
                                                }
                                                flag2 = false;
                                            }
                                        }


                                        if (list[m].Speed == 0)
                                        {
                                            if (!flag4)
                                            {
                                                datestart = list[m].DateSave.Value;
                                                flag4 = true;
                                            }
                                            if (lxf2)
                                            {
                                                double timeStemp =
                                                    list[m].DateSave.Value.Subtract(list[i_lx].DateSave.Value).
                                                        TotalMinutes;
                                                laixelientuc += timeStemp;
                                                laixelientucOld += timeStemp;
                                                bool f_LxLienTuc = true;
                                                for (int l = m; l < k; l++)
                                                {
                                                    if (list[l].Speed > 0)
                                                    {
                                                        double timeTemp2 =
                                                            list[l].DateSave.Value.Subtract(list[m].DateSave.Value).
                                                                TotalMinutes;
                                                        if (timeTemp2 <= 15 && list[l].DateSave.Value.Day == list[m].DateSave.Value.Day)
                                                        {
                                                            laixelientuc += timeTemp2;
                                                            laixelientucOld += timeTemp2;
                                                            f_LxLienTuc = true;
                                                        }
                                                        else if (timeTemp2 > 15)
                                                        {
                                                            if (timeStemp <= 240)
                                                            {
                                                                laixelientucOld = 0;
                                                            }

                                                        }


                                                        break;
                                                    }

                                                }
                                                if (laixelientucOld > 240)
                                                {
                                                    if (f_LxLienTuc || m == k - 1)
                                                    {
                                                        STimeVP4++;
                                                        laixelientucOld = 0;
                                                        f_LxLienTuc = false;
                                                    }

                                                }




                                                lxf2 = false;
                                                lxf = true;
                                            }
                                        }
                                        else if (list[m].Speed > 0)
                                        {
                                            if (flag4)
                                            {
                                                dateend = list[m].DateSave.Value;
                                                double timeStemp = dateend.Subtract(datestart).TotalMinutes;
                                                thoigiandung += timeStemp;
                                                if (timeStemp > 1)
                                                    solandungdo++;
                                                flag4 = false;
                                            }
                                            vantocTB += list[m].Speed.Value;
                                            countTB += 1;
                                            if (lxf)
                                            {
                                                i_lx = m;
                                                lxf = false;
                                                lxf2 = true;
                                            }
                                        }
                                    }
                                    i_lx = i;
                                    flag5 = true;
                                }
                                if (flag5)
                                {
                                    if (vantocTB > countTB)
                                    {
                                        vantocTB = Math.Round(vantocTB / countTB);
                                    }
                                    count++;
                                    General th = new General();
                                    th.count = count;
                                    th.Date = dateRS;
                                    th.SOpen_Close = solandongmocua;
                                    th.SExceedingSpeed = solanvuottoc.ToString();
                                    th.SPause_Stop = solandungdo;

                                    th.SStop = ConverteTime(thoigiandung);

                                    th.TimeDriver_ = ConverteTime(laixelientuc);

                                    th.STimeVP4 = STimeVP4;
                                    STimeVP4 = 0;

                                    th.Distance = qd + " km";
                                    if (qd > 0)
                                    {
                                        th.SpeedAVG = vantocTB + " km/h";
                                        th.SpeedMax = max_v + " km/h";
                                    }
                                    th.VehicleNumber = VehicleNumber;
                                    th.SPause_Stop = solandungdo;
                                    th.NameDriver = namedriver;
                                    th.DriverLicense = gplx;
                                    listGeneralSigle.Add(th);

                                    laixelientucOld = 0;
                                    STimeVP4 = 0;
                                    qd = 0;
                                    solanvuottoc = 0;
                                    solandongmocua = 0;
                                    solandungdo = 0;
                                    laixelientuc = 0;
                                    thoigiandung = 0;
                                    max_v = 0;
                                    thoigianlaixe = "";
                                    thoigiandungtemp = "";
                                    vantocTB = 0;
                                    flag = false;
                                    flag2 = false;
                                    flag3 = false;
                                    flag4 = false;
                                    flag5 = false;
                                    f = false;
                                    datestart = new DateTime();
                                    dateend = new DateTime();




                                    dateStartSpeed = new DateTime();
                                    tempdate = "";
                                    batdau = 0;
                                    ketthuc = 0;

                                    countTB = 0;
                                    batdau = 0;
                                    ketthuc = 0;
                                    j = 0; //batdau
                                    k = 0;
                                    i_lx++;
                                }
                            }
                        }

                    } //end if data null
                    try
                    {
                        string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                        DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                "yy-MM-dd", CultureInfo.InvariantCulture));
                        DateTime dto =
                            Convert.ToDateTime(
                                DateTime.ParseExact(
                                    param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                    "yy-MM-dd", CultureInfo.InvariantCulture));






                        for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                        {

                            // logic here
                            bool fAdd = true;
                            foreach (var var in listGeneralSigle)
                            {
                                if (DateTime.Compare(var.Date.Date, date) == 0)
                                {
                                    fAdd = false;
                                }
                                if (var.Distance.Equals("0 km"))
                                {
                                    var.SpeedAVG = "0 km";
                                    var.SpeedMax = "0 km";
                                }
                            }
                            if (fAdd)
                            {
                                General general = new General();
                                general.SpeedAVG = "0 km";
                                general.SpeedMax = "0 km";
                                general.Distance = "0 km";
                                general.DriverLicense = "không xác định";
                                general.NameDriver = "không xác định";
                                general.SExceedingSpeed = "0 lần";
                                general.SOpen_Close = 0;
                                general.SStop = "0p";
                                general.Date = date;
                                if (device != null)
                                {
                                    general.VehicleNumber = device.VehicleNumber;

                                }

                                general.Date = date;
                                listGeneralSigle.Add(general);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        ex.GetBaseException();
                    }
                    listGeneralSigle = listGeneralSigle.OrderBy(m => m.Date).ToList();

                    foreach (var general in listGeneralSigle)
                    {
                        if (general.Distance.Equals("0 km"))
                        {
                            general.SpeedAVG = "0 km";
                            general.SpeedMax = "0 km";
                        }
                        listGeneral.Add(general);
                    }
                }
            }




            return listGeneral;
        }


        public IList<General> ReportGeneral(Dictionary<string, string> param)
        {
            //IList<ExceedingSpeed> Count_ExceedingSpeed = ReportExceedingSpeed(param);

            int count = 0;
            //  IList<tempdatesave> list_temp = getAllDataID_From_To(param);
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<General> listGeneralSigle = new List<General>();
            IList<General> listGeneral = new List<General>();
            IList<PauseStop> Count_Pause_Stop = new List<PauseStop>();
            IList<Report_General> list_Report = new List<Report_General>();
            for (int t = 0; t < _arrIDs.Length; t++)
            {
                string _deviceID = _arrIDs[t];

                if (!String.IsNullOrEmpty(_deviceID))
                {
                    var param_ = new Dictionary<string, string>();
                    param_["From"] = param["From"];
                    param_["To"] = param["To"];
                    param_["_DeviceID"] = _deviceID;

                    IList<GpsDataExtForGeneral> list = new List<GpsDataExtForGeneral>();

                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                //paramOld["_from"] = dataid["_from"];
                                //paramOld["_to"] = dataid["_to"];
                                //list = backupServiceT.DataForReportGeneral(paramOld);
                                paramOld["_from"] = param["From"];
                                paramOld["_to"] = param["To"];
                                list_Report = backupServiceT.DataForReportGeneral_(paramOld);
                                break;
                            }
                        case 2:
                            {

                                string st_to = param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8);
                                DateTime date_to = Convert.ToDateTime(DateTime.ParseExact(st_to,
                                                                                        "yy-MM-dd",
                                                                                        CultureInfo.InvariantCulture));

                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                if (date_to.Date == DateTime.Now.Date)
                                {
                                    Dictionary<string, string> param_temp = new Dictionary<string, string>(){
                                    { "_DeviceID", _deviceID },
                                    { "From", date_to.ToString("yy-MM-dd 00:00:00") },
                                    { "To",  date_to.ToString("yy-MM-dd 23:59:59") } };
                                    parameter = getParamNew(param_temp);
                                    parameter["_DeviceID"] = _deviceID;
                                    Count_Pause_Stop = ReportPause_Stop(param_temp);
                                    list = Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byIdT",
                                                                                                  parameter).OrderBy(
                                                                                                      item => item.DateSave)
                                        .ToList();
                                }
                                parameter["_DeviceID"] = _deviceID;

                                //parameter["_from"] = dataid["_from"];
                                //parameter["_to"] = dataid["_to"];
                                parameter["_from"] = param["From"];
                                parameter["_to"] = param["To"];

                                list_Report = Repository.ExecuteStoreProceduce<Report_General>("sp_get_tblreportgeneralbyvehicle_byID_From_to",
                                                                                           parameter).OrderBy(
                                                                                               item => item.Date_data)
                                 .ToList();

                                break;
                            }
                        case 3:
                            {
                                //Count_Pause_Stop = ReportPause_Stop(param_);
                                //Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                //paramOld["_DeviceID"] = _deviceID;
                                //paramOld["_from"] = dataid["_from_old"];
                                //paramOld["_to"] = dataid["_to_old"];
                                //IList<GpsDataExtForGeneral> list_backup = backupServiceT.DataForReportGeneral(paramOld);

                                //Dictionary<string, string> parameter = new Dictionary<string, string>();
                                //parameter["_DeviceID"] = _deviceID;
                                //parameter["_from"] = dataid["_from_new"];
                                //parameter["_to"] = dataid["_to_new"];

                                //IList<GpsDataExtForGeneral> listNew =
                                //    Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byIdT",
                                //                                                           parameter).OrderBy(
                                //                                                               item => item.DateSave).
                                //        ToList();

                                Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                paramOld["_DeviceID"] = _deviceID;
                                //paramOld["_from"] = dataid["_from"];
                                //paramOld["_to"] = dataid["_to"];
                                //list = backupServiceT.DataForReportGeneral(paramOld);
                                paramOld["_from"] = param["From"];
                                paramOld["_to"] = param["To"];
                                IList<Report_General> list_Report_backup = backupServiceT.DataForReportGeneral_(paramOld);

                                string st_to = param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8);
                                DateTime date_to = Convert.ToDateTime(DateTime.ParseExact(st_to,
                                                                                        "yy-MM-dd",
                                                                                        CultureInfo.InvariantCulture));

                                Dictionary<string, string> parameter = new Dictionary<string, string>();
                                if (date_to.Date == DateTime.Now.Date)
                                {
                                    Dictionary<string, string> param_temp = new Dictionary<string, string>(){
                                    { "IDs", _deviceID },
                                    { "From", date_to.ToString("yy-MM-dd 00:00:00") },
                                    { "To",  date_to.ToString("yy-MM-dd 23:59:59") } };
                                    parameter = getParamNew(param_temp);
                                    parameter["_DeviceID"] = _deviceID;
                                    Count_Pause_Stop = ReportPause_Stop(param_temp);
                                    list = Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byIdT",
                                                                                                  parameter).OrderBy(
                                                                                                      item => item.DateSave)
                                        .ToList();
                                }
                                parameter["_DeviceID"] = _deviceID;

                                //parameter["_from"] = dataid["_from"];
                                //parameter["_to"] = dataid["_to"];
                                parameter["_from"] = param["From"];
                                parameter["_to"] = param["To"];

                                list_Report = Repository.ExecuteStoreProceduce<Report_General>("sp_get_tblreportgeneralbyvehicle_byID_From_to",
                                                                                           parameter).OrderBy(
                                                                                               item => item.Date_data)
                                 .ToList();
                                if (list_Report_backup.Count > 0)
                                {
                                    foreach (Report_General object_ in list_Report_backup)
                                    {
                                        if ((list_Report.Where(n => n.DeviceID == object_.DeviceID)) != null)
                                        {
                                            continue;
                                        }
                                        list_Report.Add(object_);
                                    }
                                }

                                break;
                            }
                    }
                    //parameter["_DeviceID"] = _deviceID;
                    //IList<GpsDataExtForGeneral> list =
                    //    Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
                    //                                                           parameter).ToList();
                    Device device = null;
                    var parameter2 = new Dictionary<string, string>();
                    parameter2.Add("_DeviceID", _deviceID);
                    device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicleT",
                                                                      parameter2).First();
                    General GenTemp = null;
                    String from = "20" + param["From"];
                    String to = "20" + param["To"];
                    DateTime dateFrom = DateTime.Parse(from);
                    DateTime dateTo = DateTime.Parse(to);
                    try
                    {
                        if (list.FirstOrDefault() != null)
                        {
                            list = list.Where(m => m.DateSave.Value.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave.Value) >= 0).ToList();
                            //DateTime startDayList = list[0].DateSave.Value.Date;
                            DateTime StartDay = list[0].DateSave.Value;
                            DateTime EndDay = DateTime.Parse("20" + param.FirstOrDefault(m => m.Key == "To").Value);
                            //foreach (DateTime day in EachDay(StartDay,EndDay))
                            //{

                            //}
                            for (DateTime i = StartDay.Date; i <= EndDay.Date; i = i.AddDays(1))
                            {
                                double distane_ = 0;
                                GenTemp = new General();
                                //Dictionary<string, long> data_id = getDataIDByDate(i);
                                //IList<GpsDataExtForGeneral> listTemp =
                                //    list.Where(m => m.DataID.Value >= data_id["from"] && m.DataID.Value <= data_id["to"]).ToList();
                                IList<GpsDataExtForGeneral> listTemp =
                                    list.Where(m => m.DateSave.Value.Date.CompareTo(i.Date) == 0).ToList();
                                if (listTemp.Count > 0)
                                {
                                    //List<GpsDataExtForGeneral> deleteItem = list.ToList();
                                    //deleteItem.RemoveAll(item => item.DateSave.Value.Date == listTemp[0].DateSave.Value.Date);
                                    ////list = deleteItem;
                                    IList<PauseStop> pause_stop_temp =
                                        Count_Pause_Stop.Where(
                                            n => n.Date.DayOfYear.CompareTo(i.Date.DayOfYear) == 0).ToList
                                            ();
                                    distane_ += CalculateDistanceForGPSDataG(listTemp, 0, listTemp.Count);
                                    //IList<ExceedingSpeed> Count_ExceedingSpeed_temp =
                                    //    Count_ExceedingSpeed.Where(
                                    //        n => n.Date.Day == i.Day && n.Date.Month == i.Month && n.Date.Year == i.Year).ToList
                                    //        ();

                                    Dictionary<int, string> abc = CalSpeedPercent(listTemp, distane_,
                                                                                  (new DeviceService())
                                                                                      .getVehicle_by_DeviceID(parameter2)
                                                                                      .SpeedLimit);
                                    if (abc.Keys.Count > 0)
                                    {

                                        for (int j = 0; j < abc.Keys.Count; j++)
                                        {

                                            KeyValuePair<int, string> result_ =
                                                abc.Where(m => m.Key == j).FirstOrDefault();

                                            int z = result_.Value.IndexOf(",");
                                            string phantram = result_.Value.Substring(0, z);
                                            String solan = result_.Value.Substring(z, result_.Value.Length - z).Replace(
                                                ",", "");
                                            switch (j)
                                            {
                                                case 0:
                                                    GenTemp.tyle1 = phantram;
                                                    GenTemp.solan1 = solan;

                                                    break;
                                                case 1:
                                                    GenTemp.tyle2 = phantram;
                                                    GenTemp.solan2 = solan;

                                                    break;
                                                case 2:
                                                    GenTemp.tyle3 = phantram;
                                                    GenTemp.solan3 = solan;

                                                    break;
                                                case 3:
                                                    GenTemp.tyle4 = phantram;
                                                    GenTemp.solan4 = solan;

                                                    break;
                                            }
                                            //try
                                            //{
                                            //    if (int.Parse(phantram) > 0)
                                            //    {
                                            //        tongsolanvuottoc++;

                                            //    }
                                            //}
                                            //catch (Exception)
                                            //{

                                            //}


                                        }
                                        GenTemp.SExceedingSpeed = tongsolanvuottoc.ToString();
                                        GenTemp.SExceedingSpeed1000 = tongsolanvuottoc.ToString();
                                        tongsolanvuottoc = 0;
                                    }
                                    else
                                    {
                                        GenTemp.tyle1 = "0.00";
                                        GenTemp.solan1 = "0";
                                        GenTemp.tyle2 = "0.00";
                                        GenTemp.solan2 = "0";
                                        GenTemp.tyle3 = "0.00";
                                        GenTemp.solan3 = "0";
                                        GenTemp.tyle4 = "0.00";
                                        GenTemp.solan4 = "0";
                                        GenTemp.SExceedingSpeed = "0";
                                        GenTemp.SExceedingSpeed1000 = "0";
                                    }
                                    GenTemp.Distance = Math.Round(distane_).ToString();
                                    GenTemp.Date = i;
                                    GenTemp.VehicleNumber = device.VehicleNumber;
                                    GenTemp.SPause_Stop = pause_stop_temp.Count;
                                    GenTemp.TypeTransportName = listTemp[0].TypeTransportName;

                                }
                                else
                                {
                                    GenTemp.tyle1 = "0.00";
                                    GenTemp.solan1 = "0";
                                    GenTemp.tyle2 = "0.00";
                                    GenTemp.solan2 = "0";
                                    GenTemp.tyle3 = "0.00";
                                    GenTemp.solan3 = "0";
                                    GenTemp.tyle4 = "0.00";
                                    GenTemp.solan4 = "0";
                                    GenTemp.SPause_Stop = 0;
                                    GenTemp.VehicleNumber =
                                        getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;
                                    //GenTemp.Date = i;
                                    if (device != null)
                                    {
                                        GenTemp.VehicleNumber = device.VehicleNumber;
                                    }
                                    GenTemp.Distance = "0";
                                    GenTemp.Date = i;
                                    GenTemp.SExceedingSpeed = "0";
                                    GenTemp.SExceedingSpeed1000 = "0";
                                    GenTemp.TypeTransportName = getDevivebyDeviceIDT(_deviceID).TypeTransportName;

                                }
                                listGeneral.Add(GenTemp);
                                // distane_ = 0;

                            }
                        }
                        if (list_Report.Count > 0)
                        {
                            foreach (Report_General obj in list_Report)
                            {
                                GenTemp = new General();
                                GenTemp.tyle1 = obj.Type1.ToString("0.00");
                                GenTemp.solan1 = obj.OverSpeed1.ToString();
                                GenTemp.tyle2 = obj.Type2.ToString("0.00");
                                GenTemp.solan2 = obj.OverSpeed2.ToString();
                                GenTemp.tyle3 = obj.Type3.ToString("0.00");
                                GenTemp.solan3 = obj.OverSpeed3.ToString();
                                GenTemp.tyle4 = obj.Type4.ToString("0.00");
                                GenTemp.solan4 = obj.OverSpeed4.ToString();
                                GenTemp.SExceedingSpeed = obj.TotalOverSpeed.ToString();
                                GenTemp.SExceedingSpeed1000 = obj.TotalOverSpeed1000.ToString();
                                GenTemp.SPause_Stop = obj.TotalPauseStop;
                                GenTemp.VehicleNumber = obj.VehicleNumber.ToString();
                                GenTemp.Distance = obj.Distance.ToString();
                                GenTemp.Date = obj.Date_data.Date;
                                GenTemp.TypeTransportName = obj.TypeTransportName.ToString();
                                listGeneral.Add(GenTemp);
                            }

                        }

                    }
                    catch (Exception ee)
                    {
                        //throw ee;
                        continue;
                    }

                }
            }
            listGeneral = listGeneral.OrderBy(m => m.Date).Where(m => !m.Distance.Equals("0 Km")).ToList();
            return listGeneral;
        }
        public IList<Driver> getDriverbyDriverID(Dictionary<string, string> param)
        {
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> parameter = null;
            IList<Driver> listDriver = new List<Driver>();
            for (int i = 0; i < _arrIDs.Length; i++)
            {
                String _DriverID = _arrIDs[i];
                parameter = new Dictionary<string, string>();
                parameter.Add("_DriverID", _DriverID);
                Driver driver_ =
                Repository.ExecuteStoreProceduce<Driver>("sp_getDriverbyDriverID", parameter).FirstOrDefault();
                listDriver.Add(driver_);
            }

            return listDriver;
        }


        public IList<General> ReportGeneralbyDriver(Dictionary<string, string> param)
        {
            IList<Driver> Listdriver = getDriverbyDriverID(param).OrderBy(m => m.DeviceID).ToList();
            IList<ExceedingSpeed> Count_ExceedingSpeed = null;
            int count = 0;
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<General> listGeneralSigle = new List<General>();
            IList<General> listGeneral = new List<General>();
            if (Listdriver.Count > 0)
            {
                for (int x = 0; x < Listdriver.Count; x++)
                {
                    Dictionary<String, String> param_temp = param;
                    param_temp["IDs"] = Listdriver[x].DeviceID.ToString();
                    // Count_ExceedingSpeed = ReportExceedingSpeed(param_temp);
                    // param["From"] = Listdriver[x].DateDriver.ToString();
                    string _deviceID = Listdriver[x].DeviceID.ToString();
                    //if(x<Listdriver.Count)
                    //{
                    //    if (_deviceID.Equals(Listdriver[x+1].DeviceID.ToString()))
                    //    {

                    //    }
                    //}

                    if (!String.IsNullOrEmpty(_deviceID))
                    {
                        var param_ = new Dictionary<string, string>();
                        param_ = param;
                        param_["IDs"] = _deviceID;
                        IList<PauseStop> Count_Pause_Stop = ReportPause_Stop(param_);
                        IList<GpsDataExtForGeneral> list = new List<GpsDataExtForGeneral>();
                        switch (iDate)
                        {
                            case 1:
                                {
                                    Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                    paramOld["_DeviceID"] = _deviceID;
                                    paramOld["_from"] = dataid["_from"];
                                    paramOld["_to"] = dataid["_to"];
                                    list = backupServiceT.DataForReportGeneral(paramOld);
                                    break;
                                }
                            case 2:
                                {
                                    Dictionary<string, string> parameter = new Dictionary<string, string>();
                                    parameter["_DeviceID"] = _deviceID;
                                    parameter["_from"] = dataid["_from"];
                                    parameter["_to"] = dataid["_to"];
                                    list = Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
                                                                                                  parameter).OrderBy(
                                                                                                      item => item.DateSave)
                                        .ToList();
                                    break;
                                }
                            case 3:
                                {
                                    Dictionary<string, string> paramOld = new Dictionary<string, string>();
                                    paramOld["_DeviceID"] = _deviceID;
                                    paramOld["_from"] = dataid["_from_old"];
                                    paramOld["_to"] = dataid["_to_old"];
                                    IList<GpsDataExtForGeneral> list_backup = backupServiceT.DataForReportGeneral(paramOld);

                                    Dictionary<string, string> parameter = new Dictionary<string, string>();
                                    parameter["_DeviceID"] = _deviceID;
                                    parameter["_from"] = dataid["_from_new"];
                                    parameter["_to"] = dataid["_to_new"];
                                    IList<GpsDataExtForGeneral> listNew =
                                        Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
                                                                                               parameter).OrderBy(
                                                                                                   item => item.DateSave).
                                            ToList();
                                    list = list_backup.Union(listNew).ToList();
                                    break;
                                }
                        }

                        Device device = null;
                        var parameter2 = new Dictionary<string, string>();
                        parameter2.Add("_DeviceID", _deviceID);
                        device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicleT",
                                                                          parameter2).First();
                        General GenTemp = null;
                        String from = "20" + param["From"];
                        String to = "20" + param["To"];
                        DateTime dateFrom = DateTime.Parse(from);
                        DateTime dateTo = DateTime.Parse(to);
                        if (list.FirstOrDefault() != null)
                        {
                            list = list.Where(m => m.DateSave.Value.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave.Value) >= 0).ToList();
                        }
                        if (list.FirstOrDefault() != null)
                        {
                            DateTime startDayList = list[0].DateSave.Value.Date;
                            double distane_ = 0;

                            DateTime StartDay = list[0].DateSave.Value;
                            DateTime EndDay = DateTime.Parse("20" + param.FirstOrDefault(m => m.Key == "To").Value);
                            //foreach (DateTime day in EachDay(StartDay,EndDay))
                            //{

                            //}
                            //for (DateTime date = dfrom.Date; date <= dto2.Date; date = date.AddDays(1))
                            for (DateTime i = StartDay.Date; i <= EndDay.Date; i = i.AddDays(1))
                            {
                                GenTemp = new General();
                                IList<GpsDataExtForGeneral> listTemp =
                                    list.Where(
                                        m =>
                                        m.DateSave.Value.Date.CompareTo(i.Date) == 0 && m.TheDriver.Equals(Listdriver[x].PhoneDriver)).ToList();
                                if (listTemp.Count > 0)
                                {
                                    IList<PauseStop> pause_stop_temp =
                                   Count_Pause_Stop.Where(n => n.Date.DayOfYear.CompareTo(i.Date.DayOfYear) == 0).ToList();

                                    distane_ += CalculateDistanceForGPSDataG(listTemp, 0, listTemp.Count);
                                    //IList<ExceedingSpeed> Count_ExceedingSpeed_temp =
                                    //    Count_ExceedingSpeed.Where(
                                    //        n => n.Date.Day == i.Day && n.Date.Month == i.Month && n.Date.Year == i.Year).ToList
                                    //        ();

                                    Dictionary<int, string> abc = CalSpeedPercent(listTemp, distane_, (new DeviceService()).getVehicle_by_DeviceID(parameter2).SpeedLimit);
                                    if (abc.Keys.Count > 0)
                                    {
                                        for (int j = 0; j < abc.Keys.Count; j++)
                                        {
                                            KeyValuePair<int, string> result_ =
                                                abc.Where(m => m.Key == j).FirstOrDefault();

                                            int z = result_.Value.IndexOf(",");
                                            string phantram = result_.Value.Substring(0, z);
                                            String solan = result_.Value.Substring(z, result_.Value.Length - z).Replace(
                                                ",", "");
                                            switch (j)
                                            {
                                                case 0:
                                                    GenTemp.tyle1 = phantram + " %";
                                                    GenTemp.solan1 = solan + " lần";
                                                    break;
                                                case 1:
                                                    GenTemp.tyle2 = phantram + " %";
                                                    GenTemp.solan2 = solan + " lần";
                                                    break;
                                                case 2:
                                                    GenTemp.tyle3 = phantram + " %";
                                                    GenTemp.solan3 = solan + " lần";
                                                    break;
                                                case 3:
                                                    GenTemp.tyle4 = phantram + " %";
                                                    GenTemp.solan4 = solan + " lần";
                                                    break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        GenTemp.tyle1 = "0.00 %";
                                        GenTemp.solan1 = "0 lần";
                                        GenTemp.tyle2 = "0.00 %";
                                        GenTemp.solan2 = "0 lần";
                                        GenTemp.tyle3 = "0.00 %";
                                        GenTemp.solan3 = "0 lần";
                                        GenTemp.tyle4 = "0.00 % ";
                                        GenTemp.solan4 = "0 lần";
                                    }
                                    GenTemp.Distance = Math.Round(distane_) + " Km";
                                    GenTemp.Date = i;
                                    GenTemp.VehicleNumber = device.VehicleNumber;
                                    GenTemp.SPause_Stop = pause_stop_temp.Count;
                                    GenTemp.NameDriver = Listdriver[x].NameDriver;
                                    GenTemp.DriverLicense = Listdriver[x].DriverLicense;

                                }
                                else
                                {
                                    GenTemp.tyle1 = "0.00 %";
                                    GenTemp.solan1 = "0 lần";
                                    GenTemp.tyle2 = "0.00 %";
                                    GenTemp.solan2 = "0 lần";
                                    GenTemp.tyle3 = "0.00 %";
                                    GenTemp.solan3 = "0 lần";
                                    GenTemp.tyle4 = "0.00 % ";
                                    GenTemp.solan4 = "0 lần";
                                    GenTemp.SPause_Stop = 0;
                                    // xem lại chỗ này
                                    GenTemp.VehicleNumber =
                                        getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;

                                    if (device != null)
                                    {
                                        GenTemp.VehicleNumber = device.VehicleNumber;
                                    }
                                    GenTemp.Distance = "0 Km";
                                    GenTemp.Date = i;
                                    GenTemp.NameDriver = Listdriver[x].NameDriver;
                                    GenTemp.DriverLicense = Listdriver[x].DriverLicense;
                                }
                                listGeneral.Add(GenTemp);
                                distane_ = 0;
                            }
                        }

                        else
                        {
                            try
                            {
                                string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                                DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                        "yy-MM-dd",
                                                                                        CultureInfo.InvariantCulture));
                                DateTime dto =
                                    Convert.ToDateTime(
                                        DateTime.ParseExact(
                                            param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                            "yy-MM-dd", CultureInfo.InvariantCulture));


                                for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                                {
                                    // logic here
                                    bool fAdd = true;
                                    foreach (General var in listGeneralSigle)
                                    {
                                        if (DateTime.Compare(var.Date.Date, date) == 0)
                                        {
                                            fAdd = false;
                                        }
                                    }
                                    if (fAdd)
                                    {
                                        GenTemp = new General();
                                        //    GenTemp = new General();
                                        GenTemp.tyle1 = "0.00 %";
                                        GenTemp.solan1 = "0 lần";
                                        GenTemp.tyle2 = "0.00 %";
                                        GenTemp.solan2 = "0 lần";
                                        GenTemp.tyle3 = "0.00 %";
                                        GenTemp.solan3 = "0 lần";
                                        GenTemp.tyle4 = "0.00 % ";
                                        GenTemp.solan4 = "0 lần";
                                        GenTemp.SPause_Stop = 0;
                                        // xem lại chỗ này
                                        GenTemp.VehicleNumber =
                                            getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;
                                        GenTemp.Date = date;
                                        if (device != null)
                                        {
                                            GenTemp.VehicleNumber = device.VehicleNumber;
                                        }
                                        GenTemp.Distance = "0 Km";
                                        GenTemp.Date = date;
                                        GenTemp.NameDriver = Listdriver[x].NameDriver;
                                        GenTemp.DriverLicense = Listdriver[x].DriverLicense;
                                        listGeneral.Add(GenTemp);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.GetBaseException();
                            }
                        }
                        try
                        {
                            string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
                            DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
                                                                                    "yy-MM-dd",
                                                                                    CultureInfo.InvariantCulture));
                            DateTime dto =
                                Convert.ToDateTime(
                                    DateTime.ParseExact(
                                        param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
                                        "yy-MM-dd", CultureInfo.InvariantCulture));


                            for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
                            {
                                // logic here
                                bool fAdd = true;
                                foreach (General var in listGeneralSigle)
                                {
                                    if (DateTime.Compare(var.Date.Date, date) == 0)
                                    {
                                        fAdd = false;
                                    }
                                }
                                if (fAdd)
                                {
                                    GenTemp = new General();
                                    //    GenTemp = new General();
                                    GenTemp.tyle1 = "0.00 %";
                                    GenTemp.solan1 = "0 lần";
                                    GenTemp.tyle2 = "0.00 %";
                                    GenTemp.solan2 = "0 lần";
                                    GenTemp.tyle3 = "0.00 %";
                                    GenTemp.solan3 = "0 lần";
                                    GenTemp.tyle4 = "0.00 % ";
                                    GenTemp.solan4 = "0 lần";
                                    GenTemp.SPause_Stop = 0;
                                    // xem lại chỗ này
                                    GenTemp.VehicleNumber =
                                        getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;
                                    GenTemp.Date = date;
                                    if (device != null)
                                    {
                                        GenTemp.VehicleNumber = device.VehicleNumber;
                                    }
                                    GenTemp.Distance = "0 Km";
                                    GenTemp.Date = date;
                                    GenTemp.NameDriver = Listdriver[x].NameDriver;
                                    GenTemp.DriverLicense = Listdriver[x].DriverLicense;
                                    listGeneral.Add(GenTemp);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.GetBaseException();
                        }
                    }

                }
            }

            listGeneral = listGeneral.OrderBy(m => m.Date).ToList();
            return listGeneral;
        }

        //private IList<General> ReportGeneral(Dictionary<string, string> param)
        //{
        //    int count = 0;
        //    int iDate = checkDateInt(param);
        //    string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
        //    string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);


        //    IList<General> listGeneral = new List<General>();
        //    for (int t = 0; t < _arrIDs.Length; t++)
        //    {

        //        string VehicleNumber = "";
        //        string _deviceID = _arrIDs[t];


        //        if (!String.IsNullOrEmpty(_deviceID))
        //        {
        //            IList<GpsDataExtForGeneral> list = new List<GpsDataExtForGeneral>();
        //            switch (iDate)
        //            {
        //                case 1:
        //                    {

        //                        Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
        //                        paramOld["_DeviceID"] = _deviceID;
        //                        list = backupServiceT.DataForReportGeneral(paramOld);
        //                        break;
        //                    }
        //                case 2:
        //                    {
        //                        Dictionary<string, string> parameter = getParamNew(param);
        //                        parameter["_DeviceID"] = _deviceID;
        //                        list = Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
        //                                                                            parameter).OrderBy(item => item.DateSave).ToList();
        //                        break;
        //                    }
        //                case 3:
        //                    {

        //                        Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
        //                        paramOld["_DeviceID"] = _deviceID;
        //                        list = backupServiceT.DataForReportGeneral(paramOld);

        //                        if (list != null && list.Count > 0)
        //                        {
        //                            string monthNow = list.LastOrDefault().DateSave.Value.Month.ToString();
        //                            if (monthNow.Length == 1)
        //                            {
        //                                monthNow = "0" + monthNow;
        //                            }

        //                            param["From"] = param["To"].Substring(0, 2) + "-" + monthNow + "-" + "01 " +
        //                                            param["From"].Substring(9);
        //                        }
        //                        else
        //                        {

        //                            string monthNow = DateTime.Now.Month.ToString();
        //                            if (monthNow.Length == 1)
        //                            {
        //                                monthNow = "0" + monthNow;
        //                            }
        //                            param["From"] = param["To"].Substring(0, 2) + "-" + param["To"].Substring(3, 2) +
        //                                            "-" + "01 " +
        //                                            param["From"].Substring(9);
        //                        }
        //                        Dictionary<string, string> parameter = getParamNew(param);
        //                        parameter["_DeviceID"] = _deviceID;
        //                        IList<GpsDataExtForGeneral> listNew =
        //                            Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
        //                                                                         parameter).OrderBy(item => item.DateSave).ToList();
        //                        foreach (var gpsDataExt in listNew)
        //                        {
        //                            list.Add(gpsDataExt);
        //                        }
        //                        break;
        //                    }
        //            }
        //            //parameter["_DeviceID"] = _deviceID;
        //            //IList<GpsDataExtForGeneral> list =
        //            //    Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
        //            //                                                           parameter).ToList();
        //            Device device = null;
        //            IList<General> listGeneralSigle = new List<General>();
        //            if (list.FirstOrDefault() != null)
        //            {
        //                Dictionary<string, string> parameter2 = new Dictionary<string, string>();
        //                parameter2.Add("_DeviceID", _deviceID);
        //                device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicleT",
        //                                                                         parameter2).First();
        //                int switch_tat = 0;
        //                int switch_mo = 1;
        //                int switch_door_dong = 0;
        //                int switch_door_mo = 1;
        //                int SpeedLimit = int.Parse(device.SpeedLimit.ToString());
        //                if (device != null)
        //                {
        //                    SpeedLimit = device.SpeedLimit.Value;
        //                    VehicleNumber = device.VehicleNumber;
        //                    if (device.Switch_ == 1)
        //                    {

        //                        switch_tat = 1;
        //                        switch_mo = 0;
        //                    }
        //                    if (device.Switch_Door == 1)
        //                    {
        //                        switch_door_dong = 1;
        //                        switch_door_mo = 0;
        //                    }
        //                }


        //                // double soGio = DateTime.Parse(day_end + " 11:59:59 PM").Subtract(DateTime.Parse(day + " 12:00:00 AM")).TotalMinutes;

        //                int solanvuottoc = 0;
        //                int solandongmocua = 0;
        //                int solandungdo = 0;
        //                int STimeVP4 = 0;
        //                double laixelientuc = 0;
        //                double laixelientucOld = 0;
        //                double thoigiandung = 0;

        //                string thoigianlaixe = "";
        //                string thoigiandungtemp = "";
        //                string namedriver = "";
        //                string gplx = "";
        //                double vantocTB = 0;
        //                bool flag = false;
        //                bool flag2 = false;
        //                bool flag3 = false;
        //                bool flag4 = false;
        //                bool flag5 = false;
        //                bool f = false;
        //                DateTime datestart = new DateTime();
        //                DateTime dateend = new DateTime();
        //                DateTime dateStartSpeed = new DateTime();
        //                DateTime dateRS = new DateTime();
        //                int countTB = 0;
        //                string tempdate = "";
        //                int batdau = 0;
        //                int ketthuc = 0;
        //                int j = 0; //batdau
        //                int k = 0; //ketthuc
        //                //IList<Start_end> list_start = On_Off("0", imei, day,day_end);
        //                //for (int j = 0; j < list_start.Count; j++) {
        //                //    laixelientuc += Math.Round(double.Parse(list_start[j].total));
        //                //}

        //                double max_v = 0;
        //                bool lxf = true;
        //                bool lxf2 = false;
        //                double qd = 0;
        //                int i_lx = 0;
        //                string thedriver = list[0].TheDriver;
        //                DriverC drivertemp = getDriverbyPhone(list[0].DeviceID, list[0].TheDriver) ??
        //                                     getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
        //                if (drivertemp != null)
        //                {
        //                    namedriver = drivertemp.NameDriver;
        //                    gplx = drivertemp.DriverLicense;
        //                }
        //                else
        //                {
        //                    namedriver = "";
        //                    gplx = "";
        //                }
        //                STimeVP4 = 0;
        //                for (int i = 0; i < list.Count; i++)
        //                {
        //                    if (!f)
        //                    {
        //                        tempdate = list[i].DateSave.Value.ToShortDateString();
        //                        dateRS = list[i].DateSave.Value;
        //                        j = i;
        //                        f = true;
        //                        STimeVP4 = 0;
        //                        laixelientucOld = 0;
        //                    }
        //                    else // if (f)
        //                    {
        //                        string tempdate2 = list[i].DateSave.Value.ToShortDateString();
        //                        if (!tempdate2.Equals(tempdate))
        //                        {

        //                            k = i;
        //                            i = k - 1;
        //                            i_lx++;
        //                            f = false;
        //                        }
        //                        else // if (tempdate2.Equals(tempdate))
        //                        {
        //                            if (i == list.Count - 1)
        //                            {
        //                                k = i;
        //                            }
        //                        }
        //                        if (k > j)
        //                        {

        //                            if (thedriver != list[i].TheDriver)
        //                            {
        //                                drivertemp = getDriverbyPhone(list[i].DeviceID, list[i].TheDriver) ??
        //                                             getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");
        //                                if (drivertemp != null)
        //                                {
        //                                    namedriver = drivertemp.NameDriver;
        //                                    gplx = drivertemp.DriverLicense;
        //                                    thedriver = list[i].TheDriver;
        //                                }
        //                                else
        //                                {
        //                                    namedriver = "";
        //                                    gplx = "";
        //                                }
        //                            }

        //                            max_v = list[j].Speed.Value;
        //                            batdau = j;
        //                            ketthuc = k;
        //                            qd = CalculateDistanceForGPSDataG(list, j, k);
        //                            laixelientuc = 0;

        //                            STimeVP4 = 0;
        //                            for (int m = j; m < k; m++)
        //                            {


        //                                if (max_v < list[m].Speed)
        //                                {
        //                                    max_v = list[m].Speed.Value;
        //                                }
        //                                if (list[m].Speed >= SpeedLimit)
        //                                {
        //                                    //if (!flag && !list[m].Address.Contains("Tp.HCM - Trung Lương"))
        //                                    //{
        //                                    //    dateStartSpeed = list[m].DateSave.Value;
        //                                    //    flag = true;
        //                                    //}
        //                                    if (!flag)
        //                                    {
        //                                        dateStartSpeed = list[m].DateSave.Value;
        //                                        flag = true;
        //                                    }
        //                                }
        //                                else if (list[m].Speed < SpeedLimit)
        //                                {
        //                                    if (flag)
        //                                    {
        //                                        if (list[m].DateSave.Value.Subtract(dateStartSpeed).TotalSeconds > 120)
        //                                        {
        //                                            solanvuottoc++;

        //                                        }
        //                                        flag = false;
        //                                    }
        //                                }
        //                                if (list[m].StatusDoor.Equals(switch_door_mo))
        //                                {
        //                                    if (!flag2)
        //                                    {
        //                                        flag2 = true;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    if (flag2)
        //                                    {
        //                                        if (list[m].Speed < 30)
        //                                        {
        //                                            solandongmocua++;
        //                                        }
        //                                        flag2 = false;
        //                                    }
        //                                }


        //                                if (list[m].Speed == 0)
        //                                {
        //                                    // if (m > 0) m --;
        //                                    if (!flag4)
        //                                    {
        //                                        datestart = list[m].DateSave.Value;
        //                                        flag4 = true;
        //                                    }
        //                                    if (lxf2)
        //                                    {
        //                                        double timeStemp =
        //                                            list[m].DateSave.Value.Subtract(list[i_lx].DateSave.Value).
        //                                                TotalMinutes;
        //                                        if (timeStemp < 0) timeStemp = 0;
        //                                        laixelientuc += timeStemp;
        //                                        laixelientucOld += timeStemp;
        //                                        bool f_LxLienTuc = true;
        //                                        for (int l = m; l < k; l++)
        //                                        {
        //                                            if (list[l].Speed > 0)
        //                                            {
        //                                                double timeTemp2 =
        //                                                    list[l].DateSave.Value.Subtract(list[m].DateSave.Value).
        //                                                        TotalMinutes;

        //                                                if (timeTemp2 <= 15 && list[l].DateSave.Value.Day == list[m].DateSave.Value.Day)
        //                                                {
        //                                                    laixelientuc += timeTemp2;
        //                                                    laixelientucOld += timeTemp2;
        //                                                    f_LxLienTuc = true;
        //                                                }
        //                                                else if (timeTemp2 > 15)
        //                                                {
        //                                                    if (timeStemp <= 240)
        //                                                    {
        //                                                        laixelientucOld = 0;
        //                                                    }

        //                                                }


        //                                                break;
        //                                            }

        //                                        }
        //                                        if (laixelientucOld > 240)
        //                                        {
        //                                            if (f_LxLienTuc || m == k - 1)
        //                                            {
        //                                                STimeVP4++;
        //                                                laixelientucOld = 0;
        //                                                f_LxLienTuc = false;
        //                                            }

        //                                        }


        //                                        lxf2 = false;
        //                                        lxf = true;
        //                                    }
        //                                    // m++;
        //                                }
        //                                else if (list[m].Speed > 0)
        //                                {
        //                                    if (flag4)
        //                                    {
        //                                        dateend = list[m].DateSave.Value;
        //                                        double timeStemp = dateend.Subtract(datestart).TotalMinutes;
        //                                        thoigiandung += timeStemp;
        //                                        if (timeStemp > 1)
        //                                            solandungdo++;
        //                                        flag4 = false;
        //                                    }
        //                                    vantocTB += list[m].Speed.Value;
        //                                    countTB += 1;
        //                                    if (lxf)
        //                                    {
        //                                        i_lx = m;
        //                                        lxf = false;
        //                                        lxf2 = true;
        //                                    }
        //                                }
        //                            }
        //                            i_lx = i;
        //                            flag5 = true;
        //                        }
        //                        if (flag5)
        //                        {
        //                            if (vantocTB > countTB)
        //                            {
        //                                vantocTB = Math.Round(vantocTB / countTB);
        //                            }
        //                            count++;
        //                            General th = new General();
        //                            th.count = count;
        //                            th.Date = dateRS;
        //                            th.SOpen_Close = solandongmocua;
        //                            th.SExceedingSpeed = solanvuottoc.ToString();
        //                            th.SPause_Stop = solandungdo;

        //                            th.SStop = ConverteTime(thoigiandung);

        //                            th.TimeDriver_ = ConverteTime(laixelientuc);

        //                            th.STimeVP4 = STimeVP4;
        //                            STimeVP4 = 0;

        //                            if (laixelientuc <= 0)
        //                            {
        //                                qd = 0;
        //                            }
        //                            th.Distance = qd + " km";
        //                            if (qd > 0)
        //                            {
        //                                th.SpeedAVG = vantocTB + " km/h";
        //                                th.SpeedMax = max_v + " km/h";
        //                            }
        //                            th.VehicleNumber = VehicleNumber;
        //                            th.SPause_Stop = solandungdo;
        //                            th.NameDriver = namedriver;
        //                            th.DriverLicense = gplx;
        //                            listGeneralSigle.Add(th);

        //                            laixelientucOld = 0;
        //                            STimeVP4 = 0;
        //                            qd = 0;
        //                            solanvuottoc = 0;
        //                            solandongmocua = 0;
        //                            solandungdo = 0;
        //                            laixelientuc = 0;
        //                            thoigiandung = 0;
        //                            max_v = 0;
        //                            thoigianlaixe = "";
        //                            thoigiandungtemp = "";
        //                            vantocTB = 0;
        //                            flag = false;
        //                            flag2 = false;
        //                            flag3 = false;
        //                            flag4 = false;
        //                            flag5 = false;
        //                            f = false;
        //                            datestart = new DateTime();
        //                            dateend = new DateTime();


        //                            dateStartSpeed = new DateTime();
        //                            tempdate = "";
        //                            batdau = 0;
        //                            ketthuc = 0;

        //                            countTB = 0;
        //                            batdau = 0;
        //                            ketthuc = 0;
        //                            j = 0; //batdau
        //                            k = 0;
        //                            i_lx++;
        //                        }
        //                    }
        //                }

        //            } //end if data null
        //            try
        //            {
        //                string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
        //                DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
        //                                                                        "yy-MM-dd", CultureInfo.InvariantCulture));
        //                DateTime dto =
        //                    Convert.ToDateTime(
        //                        DateTime.ParseExact(
        //                            param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
        //                            "yy-MM-dd", CultureInfo.InvariantCulture));


        //                for (DateTime date = dfrom.Date; date <= dto.Date; date = date.AddDays(1))
        //                {

        //                    // logic here
        //                    bool fAdd = true;
        //                    foreach (var var in listGeneralSigle)
        //                    {
        //                        if (DateTime.Compare(var.Date.Date, date) == 0)
        //                        {
        //                            fAdd = false;
        //                        }
        //                        if (var.Distance.Equals("0 km"))
        //                        {
        //                            var.SpeedAVG = "0 km";
        //                            var.SpeedMax = "0 km";
        //                        }
        //                    }
        //                    if (fAdd)
        //                    {
        //                        General general = new General();
        //                        general.SpeedAVG = "0 km";
        //                        general.SpeedMax = "0 km";

        //                        general.Distance = "0 km";
        //                        general.VehicleNumber = getVehicleByDeviceID("{'_DeviceID':'" + _deviceID + "'}").VehicleNumber;
        //                        DriverC drivertt = getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

        //                        if (drivertt != null)
        //                        {
        //                            general.NameDriver = drivertt.NameDriver;
        //                            general.DriverLicense = drivertt.DriverLicense;
        //                        }
        //                        else
        //                        {
        //                            general.NameDriver = "";
        //                            general.DriverLicense = "";
        //                        }
        //                        general.SExceedingSpeed = "0";
        //                        general.SOpen_Close = 0;
        //                        general.SStop = "0p";
        //                        general.Date = date;
        //                        if (device != null)
        //                        {
        //                            general.VehicleNumber = device.VehicleNumber;

        //                        }

        //                        general.Date = date;
        //                        listGeneralSigle.Add(general);
        //                    }

        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                ex.GetBaseException();
        //            }
        //            listGeneralSigle = listGeneralSigle.OrderBy(m => m.Date).ToList();

        //            foreach (var general in listGeneralSigle)
        //            {
        //                if (general.Distance.Equals("0 km"))
        //                {
        //                    general.SpeedAVG = "0 km";
        //                    general.SpeedMax = "0 km";
        //                }
        //                listGeneral.Add(general);
        //            }
        //        }
        //    }


        //    return listGeneral;
        //}


        public dynamic BaoCaoHanhTrinh(Dictionary<string, string> param)
        {
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            string _listID = param.FirstOrDefault(pair => pair.Key == "_DeviceID").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<BaoCaoHanhTrinh> list_hanhtrinh = new List<BaoCaoHanhTrinh>();
            String DVDK2017 = "11121,11169,11167";
            //11121
            //11169
            //11167
            for (int t = 0; t < _arrIDs.Length; t++)
            {
                string _deviceID = _arrIDs[t];
                IList<BaoCaoHanhTrinh> hanhTrinhs = new List<BaoCaoHanhTrinh>();
                switch (iDate)
                {
                    case 1:
                        {
                            Dictionary<string, string> paramOld = new Dictionary<string, string>();
                            paramOld["_DeviceID"] = _deviceID;
                            paramOld["_from"] = dataid["_from"];
                            paramOld["_to"] = dataid["_to"];
                            hanhTrinhs = backupServiceT.DataForHanhTrinh(paramOld);
                            break;
                        }
                    case 2:
                        {
                            Dictionary<string, string> parameter = new Dictionary<string, string>();
                            parameter["_DeviceID"] = _deviceID;
                            parameter["_from"] = dataid["_from"];
                            parameter["_to"] = dataid["_to"];
                            hanhTrinhs =
                                Repository.ExecuteStoreProceduce<BaoCaoHanhTrinh>("sp_getData_HanhTrinh_byIDT",
                                                                                  parameter).OrderBy(
                                                                                      item => item.DateSave).ToList();
                            break;
                        }
                    case 3:
                        {
                            Dictionary<string, string> paramOld = new Dictionary<string, string>();
                            paramOld["_DeviceID"] = _deviceID;
                            paramOld["_from"] = dataid["_from_old"];
                            paramOld["_to"] = dataid["_to_old"];
                            IList<BaoCaoHanhTrinh> list_backup = backupServiceT.DataForHanhTrinh(paramOld);

                            Dictionary<string, string> parameter = new Dictionary<string, string>();
                            parameter["_DeviceID"] = _deviceID;
                            parameter["_from"] = dataid["_from_new"];
                            parameter["_to"] = dataid["_to_new"];
                            IList<BaoCaoHanhTrinh> listNew =
                                Repository.ExecuteStoreProceduce<BaoCaoHanhTrinh>("sp_getData_HanhTrinh_byIDT",
                                                                                  parameter).OrderBy(
                                                                                      item => item.DateSave).ToList();

                            hanhTrinhs = list_backup.Union(listNew).ToList();
                            break;
                        }
                }
                //parameter["_DeviceID"] = _arrID[i];
                //IEnumerable<BaoCaoHanhTrinh> hanhTrinhsAll =
                //    Repository.ExecuteStoreProceduce<BaoCaoHanhTrinh>("sp_getData_HanhTrinh_byID", parameter);
                //  List<BaoCaoHanhTrinh> hanhTrinhs = hanhTrinhsAll.ToList();
                BaoCaoHanhTrinh temp = null;
                //foreach (var ht in hanhTrinhs)
                //{
                //    if (ht.Key_ == 1)
                //    {
                //        if (ht.Switch_ == 1)
                //            ht.StatusKey = (ht.StatusKey == 0 ? 1 : 0);
                //    }
                //    else
                //    {
                //        ht.StatusKey = 1;
                //    }
                //    if (ht.Door_ == 1)
                //    {
                //        if (ht.Switch_Door == 1)
                //            ht.StatusDoor = (ht.StatusDoor == 0 ? 1 : 0);
                //    }
                //    else
                //    {
                //        ht.Door_ = 0;
                //    }
                //    if (temp != null)
                //    {
                //        if (ht.Speed == 0 && temp.Speed == 0 && ht.StatusKey == temp.StatusKey && ht.StatusDoor == temp.StatusDoor)
                //            continue;
                //    }
                //    ht.Alert = "Ổn định";
                //    if (ht.Speed > 0)
                //    {
                //        ht.Status = ht.Speed + "km/h";
                //        if (ht.Speed >= 80) ht.Alert = "Vượt tốc";
                //    }
                //    else if (ht.Speed == 0)
                //    {
                //        ht.Status = ht.StatusKey == 0 ? "Đỗ" : "Dừng";
                //    }
                //    if (string.IsNullOrEmpty(ht.Addr))
                //        ht.Addr = "Undefined";
                //    temp = ht;
                //    list_hanhtrinh.Add(ht);
                //}

                String from = "20" + param["From"];
                String to = "20" + param["To"];
                DateTime dateFrom = DateTime.Parse(from);
                DateTime dateTo = DateTime.Parse(to);
                if (hanhTrinhs.FirstOrDefault() != null)
                {
                    hanhTrinhs = hanhTrinhs.Where(m => m.DateSave.Value.CompareTo(dateFrom) >= 0 && dateTo.CompareTo(m.DateSave.Value) >= 0).ToList();
                }
                if (hanhTrinhs != null && hanhTrinhs.Count != 0)
                {
                    string thedriver = hanhTrinhs.FirstOrDefault().TheDriver;
                    DriverC drivertemp = getDriverbyPhone(hanhTrinhs[0].DeviceID, hanhTrinhs[0].TheDriver) ??
                                         getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                    int SpeedLimit = hanhTrinhs.FirstOrDefault().SpeedLimit.Value;

                    bool fPause = false;

                    for (int j = 0; j < hanhTrinhs.Count(); j++)
                    {
                        if (thedriver != hanhTrinhs[j].TheDriver)
                        {
                            drivertemp = getDriverbyPhone(hanhTrinhs[j].DeviceID, hanhTrinhs[j].TheDriver) ??
                                         getDriverFirst("{'_DeviceID':'" + _deviceID + "'}");

                            thedriver = hanhTrinhs[j].TheDriver;
                        }

                        if (drivertemp != null)
                        {
                            hanhTrinhs[j].NameDriver = drivertemp.NameDriver;
                            hanhTrinhs[j].DriverLicense = drivertemp.DriverLicense;
                        }
                        else
                        {
                            hanhTrinhs[j].NameDriver = "";
                            hanhTrinhs[j].DriverLicense = "";
                        }
                        if (j == hanhTrinhs.Count() - 1)
                        {
                            hanhTrinhs[j] = hanhTrinhs[j];
                        }
                        if (hanhTrinhs[j].Key_ == 1)
                        {
                            if (hanhTrinhs[j].Switch_ == 1)
                                hanhTrinhs[j].StatusKey = (hanhTrinhs[j].StatusKey == 0 ? 1 : 0);
                        }
                        else
                        {
                            hanhTrinhs[j].StatusKey = 1;
                        }
                        if (hanhTrinhs[j].Door_ == 1)
                        {
                            if (hanhTrinhs[j].Switch_Door == 1)
                                hanhTrinhs[j].StatusDoor = (hanhTrinhs[j].StatusDoor == 0 ? 1 : 0);
                        }
                        else
                        {
                            hanhTrinhs[j].StatusDoor = 0;
                        }
                        if (temp != null)
                        {
                            if (!DVDK2017.Contains(_deviceID))
                            {
                                if (hanhTrinhs[j].Sleep == 0 && hanhTrinhs[j].Speed == 0 && temp.Speed == 0 &&
                             hanhTrinhs[j].StatusKey == temp.StatusKey &&
                             hanhTrinhs[j].StatusDoor == temp.StatusDoor && j < hanhTrinhs.Count - 1)
                                    continue;
                            }


                        }
                        hanhTrinhs[j].Alert = "Ổn định";
                        if (hanhTrinhs[j].Speed > 0)
                        {
                            fPause = true;
                            hanhTrinhs[j].Status = hanhTrinhs[j].Speed + "km/h";
                            if (hanhTrinhs[j].Speed >= SpeedLimit) hanhTrinhs[j].Alert = "Vượt tốc";
                        }
                        //11121
                        //11169
                        //11167

                        else if (hanhTrinhs[j].Speed == 0)
                        {
                            if (DVDK2017.Contains(_deviceID))
                            {
                                hanhTrinhs[j].Status = hanhTrinhs[j].Speed + " km/h";
                            }
                            else
                            {
                                hanhTrinhs[j].Status = "Dừng";
                                if (j - 1 >= 0)
                                {
                                    bool fSpeed = false;
                                    for (int k = j; k >= 0; k--)
                                    {
                                        if (hanhTrinhs[k].Speed > 0 || k == 0)
                                        {
                                            if (!fSpeed)
                                            {
                                                double durtemp =
                                                    hanhTrinhs[j].DateSave.Value.Subtract(hanhTrinhs[k].DateSave.Value)
                                                        .TotalMinutes;

                                                if (durtemp > 15)
                                                {
                                                    hanhTrinhs[j].Status = "Đỗ " + ConverteTime(durtemp);
                                                }
                                                else
                                                {
                                                    hanhTrinhs[j].Status = "Dừng " + ConverteTime(durtemp);
                                                }
                                                fSpeed = true;
                                            }
                                        }
                                        //else if (hanhTrinhs[k].Speed > 0)
                                        //{
                                        //    fSpeed = true;
                                        //    continue;
                                        //}
                                        if (hanhTrinhs[k].Speed == 0 && fSpeed && fPause)
                                        {
                                            hanhTrinhs[j].Distance = CalculateDistanceForGPSHanhTrinh(hanhTrinhs, k, j) +
                                                                     " km";
                                            break;
                                        }
                                    }
                                }
                                fPause = false;
                            }


                        }
                        if (string.IsNullOrEmpty(hanhTrinhs[j].Addr))
                            hanhTrinhs[j].Addr = "Undefined";
                        temp = hanhTrinhs[j];
                        list_hanhtrinh.Add(hanhTrinhs[j]);
                    }
                }
            }
            //xuat file truc tiep
            //ExportService export=new ExportService();
            //export.bchta(list_hanhtrinh, "ht");
            return list_hanhtrinh;
        }
        private dynamic GetIssueby_DeviceID(Dictionary<string, string> parameter)
        {
            //_UserID
            IEnumerable<Issue> data = Repository.ExecuteStoreProceduce<Issue>("sp_GetIssue_byDeviceID", parameter);
            return data;
        }


        public dynamic BaoCaoVanTocTungGiay(Dictionary<string, string> param)
        {
            //string vfrom = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(0, 8);
            //DateTime dfrom = Convert.ToDateTime(DateTime.ParseExact(vfrom,
            //                                                        "yy-MM-dd 00:00:00",
            //                                                        CultureInfo.InvariantCulture));
            //DateTime dto =
            //    Convert.ToDateTime(
            //        DateTime.ParseExact(
            //            param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(0, 8),
            //            "yy-MM-dd 23:59:00", CultureInfo.InvariantCulture));
            int iDate = checkDateInt(param);
            Dictionary<String, String> dataid = getAllDataID_From_To(param, iDate);
            Services_khndb4_backupT backupServiceT = new Services_khndb4_backupT();
            ReportService rpService = new ReportService();
            DateTime dfrom = DateTime.Parse("20" + param["From"].ToString());
            DateTime dto = DateTime.Parse("20" + param["To"].ToString());
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<VantocTunggiay> list_tunggiay = new List<VantocTunggiay>();
            IList<VantocTunggiay> list_tunggiay_final = new List<VantocTunggiay>();
            VantocTunggiay tunggiay_temp;
            for (int t = 0; t < _arrIDs.Length; t++)
            {
                string _deviceID = _arrIDs[t];
                IList<BaoCaoHanhTrinh> hanhTrinhs = new List<BaoCaoHanhTrinh>();
                list_tunggiay = new List<VantocTunggiay>();
                switch (iDate)
                {
                    case 1:
                        {
                            Dictionary<string, string> paramOld = backupServiceT.paramOld(param);

                            paramOld["_DeviceID"] = _deviceID;

                            hanhTrinhs = backupServiceT.DataForHanhTrinh(paramOld).Where(m => m.DateSave > dfrom && m.DateSave < dto).OrderBy(item => item.DateSave).ToList();
                            break;
                        }
                    case 2:
                        {
                            Dictionary<string, string> parameter = rpService.getParamNew(param);
                            parameter["_DeviceID"] = _deviceID;

                            hanhTrinhs =
                                Repository.ExecuteStoreProceduce<BaoCaoHanhTrinh>("sp_getData_HanhTrinh_byIDT",
                                                                                  parameter).Where(m => m.DateSave > dfrom && m.DateSave < dto).OrderBy(item => item.DateSave).ToList();
                            break;
                        }
                    case 3:
                        {
                            Dictionary<string, string> paramOld = backupServiceT.paramOld(param);

                            paramOld["_DeviceID"] = _deviceID;

                            IList<BaoCaoHanhTrinh> list_backup = backupServiceT.DataForHanhTrinh(paramOld).Where(m => m.DateSave > dfrom && m.DateSave < dto).OrderBy(item => item.DateSave).ToList();
                            string monthNow = DateTime.Now.Month.ToString();
                            if (monthNow.Length == 1)
                            {
                                monthNow = "0" + monthNow;
                            }
                            param["From"] = param["To"].Substring(0, 2) + "-" + param["To"].Substring(3, 2) + "-" + "01 " +
                                            param["From"].Substring(9);
                            Dictionary<string, string> parameter = rpService.getParamNew(param);
                            parameter["_DeviceID"] = _deviceID;

                            IList<BaoCaoHanhTrinh> listNew =
                                Repository.ExecuteStoreProceduce<BaoCaoHanhTrinh>("sp_getData_HanhTrinh_byIDT",
                                                                                  parameter).Where(m => m.DateSave > dfrom && m.DateSave < dto).OrderBy(item => item.DateSave).ToList();

                            hanhTrinhs = list_backup.Union(listNew).ToList();
                            break;
                        }
                }
                BaoCaoHanhTrinh hanhtrinh_temp;
                Dictionary<string, string> parameter_ = new Dictionary<string, string>();
                parameter_["_DeviceID"] = _deviceID;
                GpsData sttdv = null;
                sttdv = Repository.ExecuteStoreProceduce<GpsData>("chuacodulieu_check", parameter_).ToList().FirstOrDefault();
                if (sttdv == null)
                {
                    sttdv = backupServiceT.checkLastRecord(parameter_);
                }
                if (hanhTrinhs.Count == 0 && dfrom <= DateTime.Now && dfrom <= sttdv.DateSave)
                {
                    //getDevivebyDeviceIDT() {
                    //getDevivebyDeviceIDT(_deviceID)

                    Device dv = getDevivebyDeviceIDT(_deviceID);
                    hanhtrinh_temp = new BaoCaoHanhTrinh();
                    hanhtrinh_temp.VehicleNumber = dv.VehicleNumber;
                    hanhtrinh_temp.Speed = 0;
                    hanhtrinh_temp.DateSave = dfrom;
                    hanhTrinhs.Add(hanhtrinh_temp);
                    hanhtrinh_temp = new BaoCaoHanhTrinh();
                    hanhtrinh_temp.VehicleNumber = dv.VehicleNumber;
                    hanhtrinh_temp.Speed = 0;
                    hanhtrinh_temp.DateSave = dto;
                    if (dto > DateTime.Now)
                    {
                        hanhtrinh_temp.DateSave = DateTime.Now;
                    }
                    if (dto > sttdv.DateSave)
                    {
                        hanhtrinh_temp.DateSave = sttdv.DateSave;
                    }

                    hanhTrinhs.Add(hanhtrinh_temp);
                }

                if (hanhTrinhs.Count > 0)
                {
                    if (hanhTrinhs.Count == 1)
                    {
                        hanhTrinhs = new List<BaoCaoHanhTrinh>();
                        Device dv = getDevivebyDeviceIDT(_deviceID);
                        hanhtrinh_temp = new BaoCaoHanhTrinh();
                        hanhtrinh_temp.VehicleNumber = dv.VehicleNumber;
                        hanhtrinh_temp.Speed = 0;
                        hanhtrinh_temp.DateSave = dfrom;
                        hanhTrinhs.Add(hanhtrinh_temp);
                        hanhtrinh_temp = new BaoCaoHanhTrinh();
                        hanhtrinh_temp.VehicleNumber = dv.VehicleNumber;
                        hanhtrinh_temp.Speed = 0;
                        hanhtrinh_temp.DateSave = dto;

                        if (dto > sttdv.DateSave)
                        {
                            hanhtrinh_temp.DateSave = sttdv.DateSave;
                        }

                        hanhTrinhs.Add(hanhtrinh_temp);
                    }


                    hanhtrinh_temp = new BaoCaoHanhTrinh();
                    hanhtrinh_temp = hanhTrinhs[0];
                    for (int i = 0; i < hanhTrinhs.Count; i++)
                    {

                        if (hanhTrinhs[i].DateSave != hanhtrinh_temp.DateSave)
                        {
                            double delta = (hanhTrinhs[i].DateSave.Value - hanhtrinh_temp.DateSave.Value).TotalSeconds;
                            for (double j = 0; j <= delta; j++)
                            {
                                tunggiay_temp = new VantocTunggiay();
                                tunggiay_temp.speed = hanhtrinh_temp.Speed.Value;
                                tunggiay_temp.DateSave = hanhtrinh_temp.DateSave.Value.AddSeconds(j);
                                tunggiay_temp.BS = hanhtrinh_temp.VehicleNumber;
                                if (j == delta)
                                {
                                    tunggiay_temp.speed = hanhTrinhs[i].Speed.Value;
                                    tunggiay_temp.DateSave = hanhTrinhs[i].DateSave.Value.AddSeconds(j);
                                    tunggiay_temp.BS = hanhtrinh_temp.VehicleNumber;
                                }
                                list_tunggiay.Add(tunggiay_temp);
                            }


                            hanhtrinh_temp = hanhTrinhs[i];
                        }

                    }
                    //list_tunggiay_final
                    int index = 0;
                    tunggiay_temp = list_tunggiay[0];
                    tunggiay_temp.tunggiay = list_tunggiay[0].speed.ToString();
                    for (int k = 0; k < list_tunggiay.Count; k++)
                    {
                        if (index == 30 || k == list_tunggiay.Count - 1)
                        {
                            index = 0;
                            list_tunggiay_final.Add(tunggiay_temp);
                            tunggiay_temp = list_tunggiay[k];
                            tunggiay_temp.tunggiay = list_tunggiay[k].speed.ToString();
                        }
                        tunggiay_temp.tunggiay += "," + list_tunggiay[k].speed.ToString();
                        index += 1;

                    }

                }








            }

            //xuat file truc tiep
            //ExportService export=new ExportService();
            //export.bchta(list_hanhtrinh, "ht");
            return list_tunggiay_final;
        }

    }

}
