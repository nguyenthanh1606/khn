using khanhhoi.vn.Repository;
using System;
using System.Device.Location;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using khanhhoi.vn.Models;

namespace khanhhoi.vn.Services
{
    public class Services_khndb4_backup
    {
        private Repository_khndb4_backup Repository;
        public Services_khndb4_backup()
        {
            Repository = new Repository_khndb4_backup();

        }

        public bool UpdateAddress(tbladdress tbladdress)
        {
            Dictionary<string, string> addr = new Dictionary<string, string>
                        {
                            {"_AddressID", tbladdress.AddressID.ToString()},
                            {"_Addr", tbladdress.Addr}
                        };

            //ReportBackupT_Service backupServiceT = new ReportBackupT_Service();
            return Repository.ExecuteSqlCommand("sp_UpdateAddr", addr);
        }
        public IList<General> ReportGeneral(Dictionary<string, string> param)
        {
            int count = 0;
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<General> listGeneral = new List<General>();

            Dictionary<string, string> dateTo = new Dictionary<string, string>{
                    {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>{
                    {"_from",param.FirstOrDefault(pair => pair.Key == "From").Value},
                };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();
            Dictionary<string, string> parameter =
                        new Dictionary<string, string>{
                    {"_DeviceID", ""},
                    {"_from",fromID.ToString()},
                    {"_to", toID.ToString()}
                };

            for (int t = 0; t < _arrIDs.Length; t++)
            {
                string VehicleNumber = "";
                string _deviceID = _arrIDs[t];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    parameter["_DeviceID"] = _deviceID;
                    IList<GpsDataExtForGeneral> list =
                        Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byId",
                                                                               parameter).ToList();
                    if (list.FirstOrDefault() != null)
                    {
                        Dictionary<string, string> parameter2 = new Dictionary<string, string>();
                        parameter2.Add("_DeviceID", _deviceID);
                        Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicle",
                                                                                 parameter2).First();
                        int switch_tat = 0;
                        int switch_mo = 1;
                        int switch_door_dong = 0;
                        int switch_door_mo = 1;
                        if (device != null)
                        {
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
                        double laixelientuc = 0;
                        double thoigiandung = 0;
                        string thoigianlaixe = "";
                        string thoigiandungtemp = "";
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
                        if (list.FirstOrDefault() != null)
                        {
                            double max_v = 0;
                            bool lxf = true;
                            bool lxf2 = false;
                            int i_lx = 0;
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (!f)
                                {
                                    tempdate = list[i].DateSave.Value.ToShortDateString();
                                    dateRS = list[i].DateSave.Value;
                                    j = i;
                                    f = true;
                                }
                                else // if (f)
                                {
                                    string tempdate2 = list[i].DateSave.Value.ToShortDateString();
                                    if (!tempdate2.Equals(tempdate))
                                    {
                                        k = i;
                                        i = k - 1;
                                        f = false;
                                    }
                                    else// if (tempdate2.Equals(tempdate))
                                    {
                                        if (i == list.Count - 1)
                                        {
                                            k = i;
                                        }
                                    }
                                    if (k > j)
                                    {
                                        max_v = list[j].Speed.Value;
                                        batdau = j;
                                        ketthuc = k;
                                        for (int m = j; m < k; m++)
                                        {
                                            if (max_v < list[m].Speed)
                                            {
                                                max_v = list[m].Speed.Value;
                                            }
                                            if (list[m].Speed >= 80)
                                            {
                                                if (!flag)
                                                {
                                                    dateStartSpeed = list[m].DateSave.Value;
                                                    flag = true;
                                                }
                                            }
                                            else if (list[m].Speed < 80)
                                            {
                                                if (flag)
                                                {
                                                    if (list[m].DateSave.Value.Subtract(dateStartSpeed).TotalSeconds > 30)
                                                    {
                                                        solanvuottoc++;
                                                        flag = false;
                                                    }
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
                                                    solandongmocua++;
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
                                                    double timeStemp = list[m].DateSave.Value.Subtract(list[i_lx].DateSave.Value).TotalMinutes;
                                                    laixelientuc += timeStemp;
                                                    for (int l = m; l < k; l++)
                                                    {
                                                        if (list[l].Speed > 0)
                                                        {
                                                            double timeTemp2 =
                                                                list[l].DateSave.Value.Subtract(list[m].DateSave.Value).
                                                                    TotalMinutes;
                                                            if (timeTemp2 <= 15)
                                                            {
                                                                laixelientuc += timeTemp2;
                                                            }
                                                            break;
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

                                            if (m == k - 1)
                                            {
                                                flag5 = true;
                                            }
                                        }
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

                                        double qd = CalculateDistanceForGPSDataG(list, batdau, ketthuc);
                                        th.Distance = qd + " km";
                                        if (qd > 0)
                                        {
                                            th.SpeedAVG = vantocTB + " km/h";
                                            th.SpeedMax = max_v + " km/h";
                                        }
                                        th.VehicleNumber = VehicleNumber;
                                        th.SPause_Stop = solandungdo;
                                        listGeneral.Add(th);

                                        solanvuottoc = 0;
                                        solandongmocua = 0;
                                        solandungdo = 0;
                                        laixelientuc = 0;
                                        thoigiandung = 0;
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
                                        countTB = 0;
                                        tempdate = "";
                                        batdau = 0;
                                        ketthuc = 0;
                                        j = 0; //batdau
                                        k = 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return listGeneral;
        }

        public dynamic LoTrinh(Dictionary<string, string> param) //  Imei, from, to
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            if (param != null)
            {
                parameter.Add("_DeviceID", param.FirstOrDefault(p => p.Key == "_DeviceID").Value);
            }
            //DateTime DateExpired;
            //string timestemp="";
            // IEnumerable<DeviceStatus> timestemp =
            //    Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetDateExpired_byDeviceID", parameter);

            DeviceService dv = new DeviceService();
            DeviceStatus timestemp = dv.GetExpired(parameter);
            if (timestemp != null)
            {
                //DateExpired = ;
                if (DateTime.Compare(timestemp.DateExpired.Value, DateTime.Now.Date) < 0)
                {
                    return new { isActive = "false" };
                }
                else
                {
                    IEnumerable<DeviceStatus> ListLoTrinh = Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetLotrinh_FromTo", param);
                    IList<DeviceStatus> list_lotrinh = new List<DeviceStatus>();
                    DeviceStatus temp = null;
                    foreach (var lt in ListLoTrinh)
                    {
                        if (temp != null)
                        {
                            if (lt.Speed == 0 && temp.Speed == 0 && lt.StatusKey == temp.StatusKey)
                                continue;
                        }
                        if (lt.Addr == "" || lt.Addr == null)
                            lt.Addr = "chưa xác định";
                        temp = lt;
                        list_lotrinh.Add(lt);
                    }
                    return list_lotrinh;
                }
            }
            return null;
        }

        public IList<Distance> ReportDistance(Dictionary<string, string> param)
        {

            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<Distance> lisqd = new List<Distance>();
            Dictionary<string, string> dateTo = new Dictionary<string, string>{
                    {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>{
                    {"_from",param.FirstOrDefault(pair => pair.Key == "From").Value},

                };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();

            Dictionary<string, string> parameter =
                new Dictionary<string, string>{
                    {"_DeviceID", ""},
                    {"_from",fromID.ToString()},
                    {"_to", toID.ToString()}
                };

            int sumCount = 0;
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    parameter["_DeviceID"] = _deviceID;
                    IList<GpsDataExt> list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_GetDataDistance_byID",
                                                                                          parameter).ToList();
                    if (list.FirstOrDefault() != null)
                    {
                        double quangduong = CalculateDistance(list, 0, list.Count);
                        int v_max = 0;
                        double v_all = 0;
                        double v_tb = 0;

                        v_max = list[0].Speed.Value;

                        for (int i = 0; i < list.Count(); i++)
                        {
                            int hientai = list[i].Speed.Value;
                            if (hientai > 0)
                            {
                                sumCount++;
                            }
                            if (hientai > v_max)
                            {
                                v_max = hientai;
                            }
                            v_all += hientai;


                        }
                        v_tb = Math.Round(v_all / sumCount);

                        Distance dist = new Distance();
                        dist.VehicleNumber = list[0].VehicleNumber;
                        dist.SpeedMax = v_max + " Km/h";
                        dist.Distances = quangduong + " Km";
                        dist.SpeedAVG = v_tb + " Km/h";
                        lisqd.Add(dist);
                    }
                }
            }
            return lisqd;
        }

        public dynamic BaoCaoHanhTrinh(Dictionary<string, string> param)
        {
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrID = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<BaoCaoHanhTrinh> list_hanhtrinh = new List<BaoCaoHanhTrinh>();
            Dictionary<string, string> dateTo = new Dictionary<string, string>
                                                    {
                                                        {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                                                    };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "_from",
                                                              param.FirstOrDefault(pair => pair.Key == "From").Value
                                                          },

                                                      };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).FirstOrDefault();

            Dictionary<string, string> parameter =
                new Dictionary<string, string>
                    {
                        {"_DeviceID", ""},
                        {"_from", fromID.ToString()},
                        {"_to", toID.ToString()}
                    };

            for (int i = 0; i < _arrID.Length; i++)
            {
                parameter["_DeviceID"] = _arrID[i];
                IEnumerable<BaoCaoHanhTrinh> hanhTrinhsAll =
                    Repository.ExecuteStoreProceduce<BaoCaoHanhTrinh>("sp_getData_HanhTrinh_byID", parameter);
                List<BaoCaoHanhTrinh> hanhTrinhs = hanhTrinhsAll.ToList();
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
                //        ht.Addr = "chưa xác định";
                //    temp = ht;
                //    list_hanhtrinh.Add(ht);
                //}


                for (int j = 0; j < hanhTrinhs.Count(); j++)
                {
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
                        hanhTrinhs[j].Door_ = 0;
                    }
                    if (temp != null)
                    {
                        if (hanhTrinhs[j].Sleep == 0 && hanhTrinhs[j].Speed == 0 && temp.Speed == 0 && hanhTrinhs[j].StatusKey == temp.StatusKey &&
                            hanhTrinhs[j].StatusDoor == temp.StatusDoor && j < hanhTrinhs.Count - 1)
                            continue;
                    }
                    hanhTrinhs[j].Alert = "Ổn định";
                    if (hanhTrinhs[j].Speed > 0)
                    {
                        hanhTrinhs[j].Status = hanhTrinhs[j].Speed + "km/h";
                        if (hanhTrinhs[j].Speed >= 80) hanhTrinhs[j].Alert = "Vượt tốc";
                    }
                    else if (hanhTrinhs[j].Speed == 0)
                    {
                        hanhTrinhs[j].Status = "Dừng";
                        if (j - 1 >= 0)
                        {
                            for (int k = j; k >= 0; k--)
                            {
                                if (hanhTrinhs[k].Speed > 0 || k == 0)
                                {
                                    if (hanhTrinhs[j].DateSave.Value.Subtract(hanhTrinhs[k].DateSave.Value).TotalMinutes > 15)
                                    {
                                        hanhTrinhs[j].Status = "Đỗ";
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(hanhTrinhs[j].Addr))
                        hanhTrinhs[j].Addr = "chưa xác định";
                    temp = hanhTrinhs[j];
                    list_hanhtrinh.Add(hanhTrinhs[j]);
                }
            }
            return list_hanhtrinh;
        }


        public IList<ExceedingSpeed> ReportExceedingSpeed(Dictionary<string, string> param)
        {
            int SpeedMax = 80;
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<ExceedingSpeed> listExc = new List<ExceedingSpeed>();
            Dictionary<string, string> dateTo = new Dictionary<string, string>{
                    {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>{
                    {"_from",param.FirstOrDefault(pair => pair.Key == "From").Value},

                };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();
            Dictionary<string, string> parameter =
                new Dictionary<string, string>{
                    {"_DeviceID", ""},
                    {"_from",fromID.ToString()},
                    {"_to", toID.ToString()}
                };

            //string[] list_arr = arrDevices.Split(',');
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    parameter["_DeviceID"] = _deviceID;
                    IList<GpsDataExt> list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byID",
                                                                                          parameter).ToList();
                    if (list.FirstOrDefault() != null)
                    {
                        int count = 0;
                        bool flag = true;
                        bool flag2 = false;
                        int temp1 = 0;
                        int temp2 = 0;
                        DateTime ngay = new DateTime();
                        string vantocbatdau = "";
                        string diadiem = "";
                        string toado = "";
                        string vantocketthuc = "";
                        DateTime tempstar = new DateTime();
                        DateTime tempend = new DateTime();
                        for (int i = 0; i < list.Count; i++)
                        {

                            if (list[i].Speed.Value >= SpeedMax)
                            {
                                if (flag)
                                {
                                    temp1 = i;
                                    count += 1;
                                    ngay = list[i].DateSave;
                                    tempstar = list[i].DateSave;
                                    vantocbatdau = list[i].Speed.Value.ToString() + " km/h";
                                    if (list[i].Address != null && list[i].Address != "")
                                        diadiem = list[i].Address;
                                    else
                                    {
                                        diadiem = "chưa xác định";
                                    }

                                    toado = list[i].Latitude + "," + list[i].Longitude;
                                    flag = false;
                                    flag2 = false;
                                }
                            }
                            else if (list[i].Speed.Value < SpeedMax)
                            {
                                if (!flag)
                                {
                                    temp2 = i;
                                    tempend = DateTime.Parse(list[i].DateSave.ToString());
                                    vantocketthuc = list[i].Speed.Value.ToString() + " km/h";
                                    flag = true;
                                    flag2 = true;
                                }
                            }
                            if (flag2)
                            {
                                ExceedingSpeed vuottoctemp = new ExceedingSpeed();
                                string thoiluong = null;
                                int max = list[temp1].Speed.Value;
                                for (int K = temp1 + 1; K < temp2 + 1; K++)
                                {
                                    if (max < list[K].Speed.Value)
                                    {
                                        max = list[K].Speed.Value;
                                    }
                                }
                                vuottoctemp.count = count;
                                vuottoctemp.VehicleNumber = list[0].VehicleNumber;
                                vuottoctemp.Date = ngay;
                                vuottoctemp.SpeedStart = vantocbatdau;
                                vuottoctemp.Address = diadiem;
                                vuottoctemp.Coordinates = toado;
                                vuottoctemp.SpeedEnd = vantocketthuc;
                                vuottoctemp.SpeedMax = max + " km/h";
                                vuottoctemp.Duration = ConverteTime(tempend.Subtract(tempstar).TotalMinutes);
                                vuottoctemp.TimeStart = tempstar.TimeOfDay.ToString();
                                vuottoctemp.TimeEnd = tempend.TimeOfDay.ToString();
                                listExc.Add(vuottoctemp);
                                diadiem = "";
                                flag = true;
                                flag2 = false;

                            }
                        }
                    }
                }
            }
            return listExc;
        }


        public IList<On_Off> ReportOn_Off(Dictionary<string, string> param)//type: on or off
        {
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<On_Off> ListOn_Off = new List<On_Off>();

            Dictionary<string, string> dateTo = new Dictionary<string, string>{
                    {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>{
                    {"_from",param.FirstOrDefault(pair => pair.Key == "From").Value},

                };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();
            Dictionary<string, string> parameter =
                new Dictionary<string, string>{
                    {"_DeviceID", ""},
                    {"_from",fromID.ToString()},
                    {"_to", toID.ToString()}
                };
            string type = param.FirstOrDefault(pair => pair.Key == "type").Value;

            // string[] list_arr = arrDeviceID.Split(',');
            bool flag = true;
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            int count = 0;
            int m = 0;
            string bienso = "";
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    parameter["_DeviceID"] = _deviceID;

                    IList<GpsDataForOn_Off> list = Repository.ExecuteStoreProceduce<GpsDataForOn_Off>("sp_GetDataOn_Off_byID",
                                                                                          parameter).ToList();
                    Dictionary<string, string> parameter2 = new Dictionary<string, string>();
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
                    if (list.FirstOrDefault() != null)
                    {

                        if (type.Equals("on"))
                        {


                            for (int i = 0; i < list.Count(); i++)
                            {
                                if (flag == true)
                                {
                                    if (list[i].StatusKey.Equals(switch_mo))
                                    {
                                        start = list[i].DateSave;
                                        flag = false;
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (list[i].StatusKey.Equals(switch_tat))
                                    {
                                        m = i - 1;
                                    }
                                    else if (i == list.Count - 1)
                                    {
                                        m = i;
                                    }
                                    if (m != 0)
                                    {
                                        end = list[m].DateSave;
                                        flag = true;
                                        count++;
                                        On_Off on_offtemp = new On_Off();
                                        on_offtemp.count = count;
                                        on_offtemp.VehicleNumber = bienso;
                                        on_offtemp.DateTime = ConverteDateTime(start);
                                        on_offtemp.Duration = ConverteTime(end.Subtract(start).TotalMinutes);
                                        m = 0;
                                        ListOn_Off.Add(on_offtemp);
                                        continue;
                                    }
                                }
                            }
                        }
                        else if (type.Equals("off"))
                        {


                            for (int i = 0; i < list.Count(); i++)
                            {
                                if (flag == true)
                                {
                                    if (list[i].StatusKey.Equals(switch_tat))
                                    {
                                        start = list[i].DateSave;
                                        flag = false;
                                        continue;
                                    }
                                }
                                else
                                {
                                    if (list[i].StatusKey.Equals(switch_mo))
                                    {
                                        m = i - 1;
                                    }
                                    else if (i == list.Count - 1)
                                    {
                                        m = i;
                                    }
                                    if (m != 0)
                                    {
                                        end = list[m].DateSave;
                                        flag = true;
                                        count++;
                                        On_Off on_offtemp = new On_Off();
                                        on_offtemp.count = count;
                                        on_offtemp.VehicleNumber = bienso;
                                        on_offtemp.DateTime = ConverteDateTime(start);
                                        on_offtemp.Duration = ConverteTime(end.Subtract(start).TotalMinutes);
                                        m = 0;
                                        ListOn_Off.Add(on_offtemp);
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return ListOn_Off;
        }


        public string ConverteDateTime(DateTime datetime)
        {
            return "Ngày " + datetime.Day + ", tháng " + datetime.Month + ", năm " + datetime.Year + ", lúc " + datetime.TimeOfDay;
        }


        public IList<TimeDriver> TimeDriver(Dictionary<string, string> param)
        {
            int count = 0;
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<TimeDriver> listTimeDriver = new List<TimeDriver>();

            Dictionary<string, string> dateTo = new Dictionary<string, string>
                                                    {
                                                        {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                                                    };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "_from",
                                                              param.FirstOrDefault(pair => pair.Key == "From").Value
                                                          },

                                                      };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();
            Dictionary<string, string> parameter =
                new Dictionary<string, string>
                    {
                        {"_DeviceID", ""},
                        {"_from", fromID.ToString()},
                        {"_to", toID.ToString()}
                    };
            string type = param.FirstOrDefault(pair => pair.Key == "Type").Value;


            //string[] list_arr = arrDevices.Split(',');
            double sumtempdriver = 0;
            //gia tri truoc
            //      double TimeDriverOld = 0;
            double SpeedAVGOld = 0;
            int SpeedMaxOld = 0;
            double DistanceOld = 0;

            try
            {
                for (int j = 0; j < _arrIDs.Length; j++)
                {
                    string _deviceID = _arrIDs[j];
                    if (!String.IsNullOrEmpty(_deviceID))
                    {
                        parameter["_DeviceID"] = _deviceID;
                        IList<GpsDataExt> list =
                            Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byID",
                                                                         parameter).ToList();
                        if (list.FirstOrDefault() != null)
                        {
                            string bienso = "";
                            bool flag = true;
                            bool flag2 = false;
                            bool flag3 = false;
                            int temp1 = 0;
                            int temp2 = 0;
                            int lstStart = 0;
                            int lstEnd = 0;
                            string ngay = "";
                            string diadiembatdau = "";
                            string toadobatdau = "";
                            string DiaDiemKetThuc = "";
                            string ToaDoKetThuc = "";
                            DateTime tempstar = new DateTime();
                            DateTime tempend = new DateTime();
                            string tempDriver = list.FirstOrDefault().TheDriver;
                            string day = list.FirstOrDefault().DateSave.ToShortDateString();
                            for (int i = 0; i < list.Count; i++)
                            {

                                if (!tempDriver.Equals(list[i].TheDriver) ||
                                    !day.Equals(list[i].DateSave.ToShortDateString()))
                                {
                                    tempDriver = list[i].TheDriver;
                                    day = list[i].DateSave.ToShortDateString();
                                    flag2 = true;
                                    flag3 = true;
                                    flag = false;
                                    i--;

                                }
                                else if (list[i].Speed > 0)
                                {
                                    if (flag)
                                    {
                                        temp1 = i;
                                        count++;
                                        ngay = list[i].DateSave.ToShortDateString();
                                        tempstar = list[i].DateSave;
                                        toadobatdau = list[i].Latitude + "," + list[i].Longitude;
                                        bienso = list[i].VehicleNumber;
                                        diadiembatdau = !string.IsNullOrEmpty(list[i].Address)
                                                            ? list[i].Address
                                                            : "chưa xác định";
                                        lstStart = i;
                                        flag = false;
                                        flag3 = false;
                                    }
                                }
                                else if (list[i].Speed == 0 || flag3)
                                {
                                    if (!flag)
                                    {
                                        temp2 = i;
                                        tempend = list[i].DateSave;
                                        ToaDoKetThuc = list[i].Latitude + "," + list[i].Longitude;
                                        DiaDiemKetThuc = !string.IsNullOrEmpty(list[i].Address)
                                                             ? list[i].Address
                                                             : "chưa xác định";
                                        lstEnd = i;
                                        flag2 = true;
                                    }
                                    else
                                    {
                                        temp1 = i;
                                        count++;
                                        ngay = list[i].DateSave.ToShortDateString();
                                        tempstar = list[i].DateSave;
                                        toadobatdau = list[i].Latitude + "," + list[i].Longitude;
                                        bienso = list[i].VehicleNumber;
                                        diadiembatdau = !string.IsNullOrEmpty(list[i].Address)
                                                            ? list[i].Address
                                                            : "chưa xác định";
                                        lstStart = i;
                                        flag = false;
                                    }
                                }
                                if (i == list.Count - 1)
                                    flag2 = true;
                                if (flag2)
                                {
                                    flag = true;
                                    flag2 = false;
                                    TimeDriver laixetemp = new TimeDriver();
                                    //            string thoiluong = null;
                                    int max = list[temp1].Speed.Value;
                                    int tempCount = temp2 - temp1;

                                    double difference = tempend.Subtract(tempstar).TotalMinutes;
                                    laixetemp.TimeDriver_ = ConverteTime(difference);
                                    laixetemp.count = count;
                                    laixetemp.Date = ngay;
                                    laixetemp.VehicleNumber = bienso;
                                    laixetemp.TimeStart = tempstar.TimeOfDay.ToString();
                                    laixetemp.TimeEnd = tempend.TimeOfDay.ToString();
                                    laixetemp.AddressStart = diadiembatdau;
                                    laixetemp.CoordinatesStart = toadobatdau;
                                    laixetemp.AddressEnd = DiaDiemKetThuc;
                                    laixetemp.CoordinatesEnd = ToaDoKetThuc;

                                    double distemp = CalculateDistance(list, lstStart, lstEnd);
                                    if (distemp == 0)
                                    {
                                        if (flag3)
                                            i += 2;
                                        continue;
                                    }

                                    laixetemp.Distance = distemp + " km";
                                    lstStart = lstEnd;
                                    Dictionary<string, string> paramdriver = null;
                                    paramdriver =
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>("{'_DeviceID':'" +
                                                                                                  _deviceID +
                                                                                                  "','_PhoneDriver':'" +
                                                                                                  list[i].TheDriver +
                                                                                                  "'}");
                                    IEnumerable<DriverC> driverc =
                                        Repository.ExecuteStoreProceduce<DriverC>("sp_getDriverByPhoneDriver",
                                                                                  paramdriver);
                                    laixetemp.NameDriver = driverc.FirstOrDefault() != null
                                                               ? driverc.FirstOrDefault().NameDriver
                                                               : "không xác định";



                                    sumtempdriver += difference;


                                    int sum = 0;
                                    double avg = 0;
                                    for (int k = temp1; k < temp2; k++)
                                    {
                                        sum += list[k].Speed.Value;
                                        if (max < list[k].Speed)
                                        {
                                            max = list[k].Speed.Value;
                                        }

                                    }
                                    avg = sum / tempCount;
                                    if (sum == 0)
                                    {
                                        if (flag3)
                                            i += 2;
                                        continue;
                                    }
                                    if (listTimeDriver.Count > 0)
                                    {
                                        int idtemp = listTimeDriver.Count - 1;
                                        double tegsdg =
                                            tempstar.Subtract(listTimeDriver[idtemp].End).TotalMinutes;
                                        if (tegsdg < 15 && tempDriver == list[i].TheDriver)
                                        {
                                            if (max < SpeedMaxOld)
                                                max = SpeedMaxOld;

                                            avg = (SpeedAVGOld + avg) / 2;
                                            listTimeDriver[idtemp].End = tempend;
                                            listTimeDriver[idtemp].SpeedMax = max + " km/h";
                                            listTimeDriver[idtemp].SpeedAVG = Math.Round(avg) + " km/h";
                                            listTimeDriver[idtemp].TimeDriver_ =
                                                ConverteTime(tempend.Subtract(listTimeDriver[idtemp].Start).TotalMinutes);
                                            listTimeDriver[idtemp].Distance =
                                                (DistanceOld + distemp).ToString() +
                                                " km";
                                            DistanceOld = (distemp + DistanceOld);
                                            SpeedAVGOld = avg;
                                            SpeedMaxOld = max;
                                            listTimeDriver[idtemp].TimeEnd =
                                                list[i].DateSave.TimeOfDay.ToString();
                                            listTimeDriver[idtemp].AddressEnd = DiaDiemKetThuc;

                                        }
                                        else
                                        {

                                            laixetemp.theDriver = list[i].TheDriver;
                                            laixetemp.Start = tempstar;
                                            laixetemp.End = tempend;
                                            laixetemp.SpeedMax = max + " km/h";
                                            laixetemp.SpeedAVG = Math.Round(avg) + " km/h";
                                            listTimeDriver.Add(laixetemp);

                                            DistanceOld = (distemp);
                                            SpeedAVGOld = avg;
                                            SpeedMaxOld = max;

                                            diadiembatdau = "";
                                            DiaDiemKetThuc = "";


                                        }
                                    }
                                    else
                                    {

                                        laixetemp.theDriver = list[i].TheDriver;
                                        laixetemp.Start = tempstar;
                                        laixetemp.End = tempend;
                                        laixetemp.SpeedMax = max + " km/h";
                                        laixetemp.SpeedAVG = Math.Round(avg) + " km/h";
                                        listTimeDriver.Add(laixetemp);

                                        DistanceOld = (distemp);
                                        SpeedAVGOld = avg;
                                        SpeedMaxOld = max;

                                        diadiembatdau = "";
                                        DiaDiemKetThuc = "";
                                    }
                                }
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

        public IList<TimeDriver> TimeDriverVP10(Dictionary<string, string> param)
        {
            int count = 0;
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<TimeDriver> listTimeDriver = new List<TimeDriver>();
            IList<TimeDriver> listTimeDriverRS = new List<TimeDriver>();

            Dictionary<string, string> dateTo = new Dictionary<string, string>
                                                    {
                                                        {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                                                    };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "_from",
                                                              param.FirstOrDefault(pair => pair.Key == "From").Value
                                                          },

                                                      };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();
            Dictionary<string, string> parameter =
                new Dictionary<string, string>
                    {
                        {"_DeviceID", ""},
                        {"_from", fromID.ToString()},
                        {"_to", toID.ToString()}
                    };
            try
            {
                for (int j = 0; j < _arrIDs.Length; j++)
                {
                    string _deviceID = _arrIDs[j];
                    if (!String.IsNullOrEmpty(_deviceID))
                    {
                        parameter["_DeviceID"] = _deviceID;
                        IList<GpsDataExt> list =
                            Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byID",
                                                                         parameter).ToList();
                        if (list.FirstOrDefault() != null)
                        {
                            string bienso = "";
                            bool flag = true;
                            bool flag2 = false;
                            bool flag3 = false;
                            int temp1 = 0;
                            int temp2 = 0;
                            int lstStart = 0;
                            int lstEnd = 0;
                            string ngay = "";
                            string diadiembatdau = "";
                            string toadobatdau = "";
                            string DiaDiemKetThuc = "";
                            string ToaDoKetThuc = "";
                            DateTime tempstar = new DateTime();
                            DateTime tempend = new DateTime();
                            string tempDriver = list.FirstOrDefault().TheDriver;
                            string day = list.FirstOrDefault().DateSave.ToShortDateString();
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (!tempDriver.Equals(list[i].TheDriver) ||
                                    !day.Equals(list[i].DateSave.ToShortDateString()))
                                {
                                    tempDriver = list[i].TheDriver;
                                    day = list[i].DateSave.ToShortDateString();
                                    flag2 = true;
                                    flag3 = true;
                                    flag = false;
                                    i--;

                                }
                                else if (list[i].Speed > 0)
                                {
                                    if (flag)
                                    {
                                        temp1 = i;
                                        count++;
                                        ngay = list[i].DateSave.ToShortDateString();
                                        tempstar = list[i].DateSave;
                                        toadobatdau = list[i].Latitude + "," + list[i].Longitude;
                                        bienso = list[i].VehicleNumber;
                                        diadiembatdau = !string.IsNullOrEmpty(list[i].Address)
                                                            ? list[i].Address
                                                            : "chưa xác định";
                                        lstStart = i;
                                        flag = false;
                                        flag3 = false;
                                    }
                                }
                                else if (list[i].Speed == 0 || flag3)
                                {
                                    if (!flag)
                                    {
                                        temp2 = i;
                                        tempend = list[i].DateSave;
                                        ToaDoKetThuc = list[i].Latitude + "," + list[i].Longitude;
                                        DiaDiemKetThuc = !string.IsNullOrEmpty(list[i].Address)
                                                             ? list[i].Address
                                                             : "chưa xác định";
                                        lstEnd = i;
                                        flag2 = true;
                                    }
                                    else
                                    {
                                        temp1 = i;
                                        count++;
                                        ngay = list[i].DateSave.ToShortDateString();
                                        tempstar = list[i].DateSave;
                                        toadobatdau = list[i].Latitude + "," + list[i].Longitude;
                                        bienso = list[i].VehicleNumber;
                                        diadiembatdau = !string.IsNullOrEmpty(list[i].Address)
                                                            ? list[i].Address
                                                            : "chưa xác định";
                                        lstStart = i;
                                        flag = false;
                                    }
                                }
                                if (i == list.Count - 1)
                                    flag2 = true;
                                if (flag2)
                                {
                                    flag = true;
                                    flag2 = false;
                                    TimeDriver laixetemp = new TimeDriver();
                                    //            string thoiluong = null;
                                    int max = list[temp1].Speed.Value;
                                    int tempCount = temp2 - temp1;

                                    double difference = tempend.Subtract(tempstar).TotalMinutes;
                                    laixetemp.TimeDriver_ = ConverteTime(difference);
                                    laixetemp.count = count;
                                    laixetemp.Date = ngay;
                                    laixetemp.VehicleNumber = bienso;
                                    laixetemp.TimeStart = tempstar.TimeOfDay.ToString();
                                    laixetemp.TimeEnd = tempend.TimeOfDay.ToString();
                                    laixetemp.AddressStart = diadiembatdau;
                                    laixetemp.CoordinatesStart = toadobatdau;
                                    laixetemp.AddressEnd = DiaDiemKetThuc;
                                    laixetemp.CoordinatesEnd = ToaDoKetThuc;

                                    double distemp = CalculateDistance(list, lstStart, lstEnd);
                                    if (distemp == 0)
                                    {
                                        if (flag3)
                                            i += 2;
                                        continue;
                                    }

                                    laixetemp.Distance = distemp + " km";
                                    lstStart = lstEnd;
                                    Dictionary<string, string> paramdriver = null;
                                    paramdriver =
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>("{'_DeviceID':'" +
                                                                                                  _deviceID +
                                                                                                  "','_PhoneDriver':'" +
                                                                                                  list[i].TheDriver +
                                                                                                  "'}");
                                    IEnumerable<DriverC> driverc =
                                        Repository.ExecuteStoreProceduce<DriverC>("sp_getDriverByPhoneDriver",
                                                                                  paramdriver);
                                    laixetemp.NameDriver = driverc.FirstOrDefault() != null
                                                               ? driverc.FirstOrDefault().NameDriver
                                                               : "không xác định";
                                    int sum = 0;
                                    for (int k = temp1; k < temp2; k++)
                                    {
                                        sum += list[k].Speed.Value;
                                    }

                                    if (sum == 0)
                                    {
                                        if (flag3)
                                            i += 2;
                                        continue;
                                    }

                                    TimeDriver driverExist =
                                        listTimeDriver.FirstOrDefault(m => m.theDriver == list[i].TheDriver);
                                    if (driverExist != null)
                                    {
                                        //   int idtemp = listTimeDriver.Count - 1;
                                        driverExist.End = tempend;
                                        driverExist.TimeDriver_ =
                                            ConverteTime(driverExist.stimedriver + difference);
                                        driverExist.stimedriver += difference;
                                        driverExist.Distance =
                                            (driverExist.sDistance + distemp).ToString() + " km";
                                        driverExist.sDistance += distemp;
                                        driverExist.TimeEnd =
                                            list[i].DateSave.TimeOfDay.ToString();
                                        driverExist.AddressEnd = DiaDiemKetThuc;
                                    }
                                    else
                                    {

                                        laixetemp.theDriver = list[i].TheDriver;
                                        laixetemp.Start = tempstar;
                                        laixetemp.End = tempend;
                                        laixetemp.sDistance = distemp;
                                        laixetemp.stimedriver = difference;
                                        listTimeDriver.Add(laixetemp);

                                    }
                                    diadiembatdau = "";
                                    DiaDiemKetThuc = "";
                                }
                            }
                            if (listTimeDriver.Count > 0)
                            {
                                foreach (var timeDriver in listTimeDriver)
                                {
                                    if (timeDriver.stimedriver > 600)
                                    {
                                        listTimeDriverRS.Add(timeDriver);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return listTimeDriverRS;
        }
        public IList<TimeDriver> TimeDriverVP4(Dictionary<string, string> param)
        {
            int count = 0;
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<TimeDriver> listTimeDriver = new List<TimeDriver>();
            IList<TimeDriver> listTimeDriverRS = new List<TimeDriver>();

            Dictionary<string, string> dateTo = new Dictionary<string, string>
                                                    {
                                                        {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                                                    };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "_from",
                                                              param.FirstOrDefault(pair => pair.Key == "From").Value
                                                          },

                                                      };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();
            Dictionary<string, string> parameter =
                new Dictionary<string, string>
                    {
                        {"_DeviceID", ""},
                        {"_from", fromID.ToString()},
                        {"_to", toID.ToString()}
                    };
            string type = param.FirstOrDefault(pair => pair.Key == "Type").Value;


            //string[] list_arr = arrDevices.Split(',');
            double sumtempdriver = 0;
            //gia tri truoc
            //      double TimeDriverOld = 0;
            double SpeedAVGOld = 0;
            int SpeedMaxOld = 0;
            double DistanceOld = 0;

            try
            {
                for (int j = 0; j < _arrIDs.Length; j++)
                {
                    string _deviceID = _arrIDs[j];
                    if (!String.IsNullOrEmpty(_deviceID))
                    {
                        parameter["_DeviceID"] = _deviceID;
                        IList<GpsDataExt> list =
                            Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byID",
                                                                         parameter).ToList();
                        if (list.FirstOrDefault() != null)
                        {
                            string bienso = "";
                            bool flag = true;
                            bool flag2 = false;
                            bool flag3 = false;
                            int temp1 = 0;
                            int temp2 = 0;
                            int lstStart = 0;
                            int lstEnd = 0;
                            string ngay = "";
                            string diadiembatdau = "";
                            string toadobatdau = "";
                            string DiaDiemKetThuc = "";
                            string ToaDoKetThuc = "";
                            DateTime tempstar = new DateTime();
                            DateTime tempend = new DateTime();
                            string tempDriver = list.FirstOrDefault().TheDriver;
                            string day = list.FirstOrDefault().DateSave.ToShortDateString();
                            for (int i = 0; i < list.Count; i++)
                            {
                                if (!tempDriver.Equals(list[i].TheDriver) ||
                                    !day.Equals(list[i].DateSave.ToShortDateString()))
                                {
                                    tempDriver = list[i].TheDriver;
                                    day = list[i].DateSave.ToShortDateString();
                                    flag2 = true;
                                    flag3 = true;
                                    flag = false;
                                    i--;

                                }
                                else if (list[i].Speed > 0)
                                {
                                    if (flag)
                                    {
                                        temp1 = i;
                                        count++;
                                        ngay = list[i].DateSave.ToShortDateString();
                                        tempstar = list[i].DateSave;
                                        toadobatdau = list[i].Latitude + "," + list[i].Longitude;
                                        bienso = list[i].VehicleNumber;
                                        diadiembatdau = !string.IsNullOrEmpty(list[i].Address)
                                                            ? list[i].Address
                                                            : "chưa xác định";
                                        lstStart = i;
                                        flag = false;
                                        flag3 = false;
                                    }
                                }
                                else if (list[i].Speed == 0 || flag3)
                                {
                                    if (!flag)
                                    {
                                        temp2 = i;
                                        tempend = list[i].DateSave;
                                        ToaDoKetThuc = list[i].Latitude + "," + list[i].Longitude;
                                        DiaDiemKetThuc = !string.IsNullOrEmpty(list[i].Address)
                                                             ? list[i].Address
                                                             : "chưa xác định";
                                        lstEnd = i;
                                        flag2 = true;
                                    }
                                    else
                                    {
                                        temp1 = i;
                                        count++;
                                        ngay = list[i].DateSave.ToShortDateString();
                                        tempstar = list[i].DateSave;
                                        toadobatdau = list[i].Latitude + "," + list[i].Longitude;
                                        bienso = list[i].VehicleNumber;
                                        diadiembatdau = !string.IsNullOrEmpty(list[i].Address)
                                                            ? list[i].Address
                                                            : "chưa xác định";
                                        lstStart = i;
                                        flag = false;
                                    }
                                }
                                if (i == list.Count - 1)
                                    flag2 = true;
                                if (flag2)
                                {
                                    flag = true;
                                    flag2 = false;
                                    TimeDriver laixetemp = new TimeDriver();
                                    //            string thoiluong = null;
                                    int max = list[temp1].Speed.Value;
                                    int tempCount = temp2 - temp1;

                                    double difference = tempend.Subtract(tempstar).TotalMinutes;
                                    laixetemp.TimeDriver_ = ConverteTime(difference);
                                    laixetemp.count = count;
                                    laixetemp.Date = ngay;
                                    laixetemp.VehicleNumber = bienso;
                                    laixetemp.TimeStart = tempstar.TimeOfDay.ToString();
                                    laixetemp.TimeEnd = tempend.TimeOfDay.ToString();
                                    laixetemp.AddressStart = diadiembatdau;
                                    laixetemp.CoordinatesStart = toadobatdau;
                                    laixetemp.AddressEnd = DiaDiemKetThuc;
                                    laixetemp.CoordinatesEnd = ToaDoKetThuc;

                                    double distemp = CalculateDistance(list, lstStart, lstEnd);
                                    if (distemp == 0)
                                    {
                                        flag = true;
                                        flag2 = false;
                                        if (flag3)
                                            i += 2;
                                        continue;
                                    }

                                    laixetemp.Distance = distemp + " km";
                                    lstStart = lstEnd;
                                    Dictionary<string, string> paramdriver = null;
                                    paramdriver =
                                        JsonConvert.DeserializeObject<Dictionary<string, string>>("{'_DeviceID':'" +
                                                                                                  _deviceID +
                                                                                                  "','_PhoneDriver':'" +
                                                                                                  list[i].TheDriver +
                                                                                                  "'}");
                                    IEnumerable<DriverC> driverc =
                                        Repository.ExecuteStoreProceduce<DriverC>("sp_getDriverByPhoneDriver",
                                                                                  paramdriver);
                                    laixetemp.NameDriver = driverc.FirstOrDefault() != null
                                                               ? driverc.FirstOrDefault().NameDriver
                                                               : "không xác định";



                                    sumtempdriver += difference;


                                    int sum = 0;
                                    double avg = 0;
                                    for (int k = temp1; k < temp2; k++)
                                    {
                                        sum += list[k].Speed.Value;
                                        if (max < list[k].Speed)
                                        {
                                            max = list[k].Speed.Value;
                                        }

                                    }
                                    avg = sum / tempCount;
                                    if (sum == 0)
                                    {
                                        if (flag3)
                                            i += 2;
                                        continue;
                                    }
                                    if (listTimeDriver.Count > 0)
                                    {
                                        int idtemp = listTimeDriver.Count - 1;
                                        double tegsdg =
                                            tempstar.Subtract(listTimeDriver[idtemp].End).TotalMinutes;
                                        if (tegsdg < 15 && tempDriver == list[i].TheDriver)
                                        {
                                            if (max < SpeedMaxOld)
                                                max = SpeedMaxOld;

                                            avg = (SpeedAVGOld + avg) / 2;
                                            listTimeDriver[idtemp].End = tempend;
                                            listTimeDriver[idtemp].SpeedMax = max + " km/h";
                                            listTimeDriver[idtemp].SpeedAVG = Math.Round(avg) + " km/h";
                                            listTimeDriver[idtemp].TimeDriver_ =
                                                ConverteTime(tempend.Subtract(listTimeDriver[idtemp].Start).TotalMinutes);
                                            listTimeDriver[idtemp].Distance =
                                                (DistanceOld + distemp).ToString() +
                                                " km";
                                            DistanceOld = (distemp + DistanceOld);
                                            SpeedAVGOld = avg;
                                            SpeedMaxOld = max;
                                            listTimeDriver[idtemp].TimeEnd =
                                                list[i].DateSave.TimeOfDay.ToString();

                                        }
                                        else
                                        {

                                            laixetemp.theDriver = list[i].TheDriver;
                                            laixetemp.Start = tempstar;
                                            laixetemp.End = tempend;
                                            laixetemp.SpeedMax = max + " km/h";
                                            laixetemp.SpeedAVG = Math.Round(avg) + " km/h";
                                            listTimeDriver.Add(laixetemp);

                                            DistanceOld = (distemp);
                                            SpeedAVGOld = avg;
                                            SpeedMaxOld = max;

                                            diadiembatdau = "";
                                            DiaDiemKetThuc = "";

                                        }
                                    }
                                    else
                                    {

                                        laixetemp.theDriver = list[i].TheDriver;
                                        laixetemp.Start = tempstar;
                                        laixetemp.End = tempend;
                                        laixetemp.SpeedMax = max + " km/h";
                                        laixetemp.SpeedAVG = Math.Round(avg) + " km/h";
                                        listTimeDriver.Add(laixetemp);

                                        DistanceOld = (distemp);
                                        SpeedAVGOld = avg;
                                        SpeedMaxOld = max;

                                        diadiembatdau = "";
                                        DiaDiemKetThuc = "";
                                    }


                                }
                            }
                            if (listTimeDriver.Count > 0)
                            {
                                foreach (var timeDriver in listTimeDriver)
                                {
                                    if (timeDriver.End.Subtract(timeDriver.Start).TotalMinutes > 240)
                                    {
                                        listTimeDriverRS.Add(timeDriver);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
            return listTimeDriverRS;
        }


        public IList<Open_Close> ReportOpen_Close(Dictionary<string, string> param)
        {
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<Open_Close> ListOpen_Close = new List<Open_Close>();
            Dictionary<string, string> dateTo = new Dictionary<string, string>{
                    {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>{
                    {"_from",param.FirstOrDefault(pair => pair.Key == "From").Value},

                };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();
            Dictionary<string, string> parameter =
                new Dictionary<string, string>{
                    {"_DeviceID", ""},
                    {"_from",fromID.ToString()},
                    {"_to", toID.ToString()}
                };


            //bool flag = true;
            //DateTime start = DateTime.Now;
            //DateTime end = DateTime.Now;
            int count = 0;
            string VehicleNumber = "";
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string _deviceID = _arrIDs[j];
                if (!String.IsNullOrEmpty(_deviceID))
                {
                    parameter["_DeviceID"] = _deviceID;

                    IList<GpsDataForOpenClose> list =
                        Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataOpenClose_byID",
                                                                              parameter).ToList();
                    Dictionary<string, string> parameter2 = new Dictionary<string, string>();
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

                    if (list.FirstOrDefault() != null)
                    {
                        string TimeOpen = "";
                        string TimeClose = "";
                        string AddressOpen = "";
                        string AddressClose = "";
                        string CoordinatesOpen = "";
                        string CoordinatesClose = "";
                        bool flag1 = true;
                        bool flag2 = false;
                        int m = 0;

                        for (int i = 0; i < list.Count(); i++)
                        {
                            if (flag1 == true)
                            {
                                if (list[i].StatusDoor.Equals(switch_mo))
                                {
                                    TimeOpen = ConverteDateTime(list[i].DateSave);
                                    CoordinatesOpen = list[i].Latitude + "," + list[i].Longitude;
                                    if (list[i].Addr != null && list[i].Addr != "")
                                        AddressOpen = list[i].Addr;
                                    else
                                        AddressOpen = "chưa xác định";

                                    flag1 = false;
                                    flag2 = true;
                                    continue;
                                }
                            }
                            else if (flag2)
                            {
                                if (list[i].StatusDoor.Equals(switch_dong))
                                {
                                    m = i;
                                }
                                else if (i == list.Count - 1)
                                {
                                    m = i;
                                }

                                if (m != 0)
                                {
                                    TimeClose = ConverteDateTime(list[m].DateSave);
                                    CoordinatesClose = list[m].Latitude + "," + list[m].Longitude;
                                    if (!string.IsNullOrEmpty(list[m].Addr))
                                        AddressClose = list[i].Addr;
                                    else
                                        AddressClose = "chưa xác định";

                                    if (TimeOpen != "" && TimeClose != "")
                                    {
                                        count++;
                                        Open_Close DongMoTemp = new Open_Close();
                                        DongMoTemp.VehicleNumber = VehicleNumber;
                                        DongMoTemp.TimeOpen = TimeOpen;
                                        DongMoTemp.TimeClose = TimeClose;
                                        DongMoTemp.AddressOpen = AddressOpen;
                                        DongMoTemp.AddressClose = AddressClose;
                                        DongMoTemp.CoordinatesOpen = CoordinatesOpen;
                                        DongMoTemp.CoordinatesClose = CoordinatesClose;
                                        DongMoTemp.count = count;
                                        ListOpen_Close.Add(DongMoTemp);
                                        AddressOpen = "";
                                        AddressClose = "";
                                    }
                                    flag1 = true;
                                    flag2 = false;
                                    continue;
                                }
                            }
                        }
                    }

                }
            }
            return ListOpen_Close;
        }


        public IList<PauseStop> ReportPause_Stop(Dictionary<string, string> param)
        {
            string _listID = param.FirstOrDefault(pair => pair.Key == "IDs").Value;
            string[] _arrIDs = _listID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            IList<PauseStop> ListPauseStop = new List<PauseStop>();

            Dictionary<string, string> dateTo = new Dictionary<string, string>
                                                    {
                                                        {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                                                    };
            int toID = Repository.ExecuteStoreProceduce<int>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>
                                                      {
                                                          {
                                                              "_from",
                                                              param.FirstOrDefault(pair => pair.Key == "From").Value
                                                          },

                                                      };
            int fromID = Repository.ExecuteStoreProceduce<int>("sp_DataID_fromDayT", dateFrom).First();
            Dictionary<string, string> parameter =
                new Dictionary<string, string>
                    {
                        {"_DeviceID", ""},
                        {"_from", fromID.ToString()},
                        {"_to", toID.ToString()}
                    };

            //string[] list_arr = arrImei.Split(',');
            bool flag = true;
            DateTime start = DateTime.Now;
            DateTime end = DateTime.Now;
            int count = 0;
            string VehicleNumber = "";
            for (int j = 0; j < _arrIDs.Length; j++)
            {
                string deviceId_ = _arrIDs[j];
                if (!String.IsNullOrEmpty(deviceId_))
                {
                    parameter["_DeviceID"] = deviceId_;
                    IList<GpsDataForPauseStop> list =
                        Repository.ExecuteStoreProceduce<GpsDataForPauseStop>("sp_GetData_PauseStop_byID",
                                                                              parameter).ToList();
                    Dictionary<string, string> parameter2 = new Dictionary<string, string>();
                    parameter2.Add("_DeviceID", deviceId_);
                    Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetSwitchAndNumVehicle",
                                                                             parameter2).First();
                    if (device != null)
                    {
                        VehicleNumber = device.VehicleNumber;
                    }

                    if (list.FirstOrDefault() != null)
                    {
                        bool fSpeed = false;
                        DateTime tempstar = new DateTime();
                        string diachi = "";
                        string toado = "";
                        for (int i = 0; i < list.Count(); i++)
                        {
                            if (!fSpeed && list[i].Speed == 0)
                            {
                                tempstar = list[i].DateSave;
                                toado = list[i].Latitude + "," + list[i].Longitude;
                                diachi = !string.IsNullOrEmpty(list[i].Addr) ? list[i].Addr : "chưa xác định";
                                fSpeed = true;
                            }
                            else if (fSpeed && (list[i].Speed > 0 || i == list.Count - 1))
                            {
                                fSpeed = false;
                                double difference = list[i].DateSave.Subtract(tempstar).TotalMinutes;
                                if (difference <= 1)
                                {
                                    continue;
                                }
                                else
                                {
                                    count++;
                                    PauseStop dungdotemp = new PauseStop();
                                    dungdotemp.Status = difference > 15 ? "Đỗ" : "Dừng";
                                    dungdotemp.VehicleNumber = VehicleNumber;
                                    dungdotemp.count = count;
                                    dungdotemp.DateTime = ConverteDateTime(tempstar);
                                    dungdotemp.Duration = ConverteTime(difference);
                                    dungdotemp.Address = diachi;
                                    dungdotemp.Coordinates = toado;
                                    ListPauseStop.Add(dungdotemp);
                                    diachi = "";
                                    toado = "";
                                }
                            }
                        }
                    }
                }
            }
            return ListPauseStop;
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
        public static double CalculateDistance(IList<GpsDataExt> list, int start, int end)
        {
            double result2 = 0;
            if (list != null)
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

            return Math.Round(result2 / 1000);
        }
        public static double CalculateDistanceForGPSDataG(IList<GpsDataExtForGeneral> list, int start, int end)
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
        public List<Device> sp_get_all_tbldevice()
        {

            List<Device> ListDV =
               Repository.ExecuteStoreProceduce<Device>("sp_get_all_tbldevice").ToList();
            return ListDV;
        }
    }
}
