using khanhhoi.vn.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using khanhhoi.vn.Models;
using Newtonsoft.Json;
using System.Globalization;

namespace khanhhoi.vn.Services
{
    public class Services_khndb4
    {
        private Repository_khndb4 Rep;
        public Services_khndb4()
        {
            String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SeagullDB"].ConnectionString;
            Rep = new Repository_khndb4(connectionString);

        }

        public tblkeygoogle sp_get_activeKey_tblkeygoogle() {
            
            return Rep.ExecuteSqlQuery<tblkeygoogle>("sp_get_activeKey_tblkeygoogle").FirstOrDefault(); 
        }
        public tblkeygoogle sp_get_key_orderdate()
        {

            return Rep.ExecuteSqlQuery<tblkeygoogle>("sp_get_key_orderdate").FirstOrDefault();
        }
        public bool sp_active_apikey_tblgoogle(int keyid)
        {
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["keyid"] = keyid.ToString();
            return Rep.ExecuteSqlCommand("sp_active_apikey_tblgoogle",param);
        }
        //sp_set_countview_tblkeygoogle
        public bool sp_set_countview_tblkeygoogle(int keyid)
        {
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["keyid"] = keyid.ToString();
            return Rep.ExecuteSqlCommand("sp_set_countview_tblkeygoogle", param);
        }
        //sp_reset_apikey
        public bool sp_reset_apikey(int keyid)
        {
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["keyid"] = keyid.ToString();
            return Rep.ExecuteSqlCommand("sp_reset_apikey", param);
        }
        public bool UpdateAddress(tbladdress tbladdress)
        {
            Dictionary<string, string> addr = new Dictionary<string, string>
                        {
                            {"_AddressID", tbladdress.AddressID.ToString()},
                            {"_Addr", tbladdress.Addr}
                        };

            //ReportBackupT_Service backupServiceT = new ReportBackupT_Service();
            Services_khndb4_backup sv_back = new Services_khndb4_backup();
            sv_back.UpdateAddress(tbladdress);
            return Rep.ExecuteSqlCommand("sp_UpdateAddr", addr);
        }
        //sp_get_tbladdress_by_addrNull
        public List<tbladdress> GetAddressbyNull()
        {

            return Rep.ExecuteStoreProceduce<tbladdress>("sp_get_tbladdress_by_addrNull").ToList();
        }
        public void closeConnect()
        {
            Rep.Dispose();
        }
        public dynamic ValidateUser(string userName, string passWord)
        {
            if (userName.Contains("'") || passWord.Contains("'"))
            {
                return null;
            }
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("_username", userName);
            parameter.Add("_pass", passWord);

            IEnumerable<UserRole> data = Rep.ExecuteStoreProceduce<UserRole>("sp_LoginT", parameter);
            return data;
        }
        //sp_GetStatus_1Device
        public DeviceStatus sp_GetStatus_1Device(int deviceid_)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter["deviceid_"] = deviceid_.ToString();
            DeviceStatus data =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetStatus_1Device", parameter).FirstOrDefault();
       
             

                data = getStatus_ofDevice_(data);

                    //}                
            
            return data;
        }
        private IList<DeviceStatus> GetAll_DeviceLastTimeUser_limit(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceStatus> data =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetDeviceLastTimeUser22_", parameter);
            if (data.FirstOrDefault() == null)
            {
                return null;
            }
            return data.ToList();
        }
        public List<tbluserdevice> sp_getcountdeviceByuserid(int userid)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("userid_", userid.ToString());
            return Rep.ExecuteStoreProceduce<tbluserdevice>("sp_getcountdeviceByuserid", parameter).ToList();
        }

        public List<DeviceStatus> GetAll_DeviceLastTimeUser(int userid)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter["_UserID"] = userid.ToString();
            List<DeviceStatus> data =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetDeviceLastTimeUser22", parameter).ToList();
            if (data.FirstOrDefault() == null)
            {
                return null;
            }
            else {
                for (int i = 0; i < data.Count; i++)
                {

                    data[i] = getStatus_ofDevice_(data[i]);

                    //}


                }
            }
            return data.ToList();
        }
        public string ConverteTime(double difference)
        {
            if (difference < 0)
            {
                double a = difference;
                difference = a * -1;
            }

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
            else if (difference >= 1)
            {
                return Math.Round(difference) + " phút ";
            }
            else
            {
                return Math.Round(difference * 10) + " giây";
            }
        }
        public dynamic getDataBySwitch(string variable)
        {
            Dictionary<string, string> parameter = null;
            if (variable != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable);
            }
            IEnumerable<DeviceStatus> data = Rep.ExecuteStoreProceduce<DeviceStatus>("sp_getDataBySwitch", parameter);
            return data;
        }
        public dynamic getDataFirst(string variable)
        {
            Dictionary<string, string> parameter = null;
            if (variable != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable);
            }
            IEnumerable<DeviceStatus> data = Rep.ExecuteStoreProceduce<DeviceStatus>("sp_getDataFirst", parameter);
            if (data.FirstOrDefault() != null)
            {
                return data;
            }
            return null;
        }
        public dynamic getData_truocdo(string variable)
        {
            Dictionary<string, string> parameter = null;
            if (variable != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable);
            }
            DeviceStatus data = Rep.ExecuteStoreProceduce<DeviceStatus>("getDevicebyImeiLasttime_truocdo",
                                                                               parameter).FirstOrDefault();
            return data;
        }
        public DeviceStatus getStatus_ofDevice_(DeviceStatus data)
        {

            double subtract_Result = 0;
            string imei = data.Imei;

            //data.OldLat = 0;
            //data.OldLng = 0;

            if (imei != null || imei != "")
            {
                if (data.Addr == null || data.Addr == "")
                    data.Addr = Resources.Language.lbnhapvaoxem;


                data.leaf = "true";
                data.@checked = false;
                data.text = data.VehicleNumber;
                int switchtemp = data.Switch_;
                int switch_tat = 0;
                int switch_mo = 1;
                if (switchtemp == 1)
                {
                    switch_tat = 1;
                    switch_mo = 0;
                }
                int switch_doortemp = data.Switch_Door;
                int switch_door_tat = 0;
                int switch_door_mo = 1;
                if (switch_doortemp == 1)
                {
                    switch_door_tat = 1;
                    switch_door_mo = 0;
                }

                if (data.Key_ == 1)
                {
                    if (data.Switch_ == 1)
                    {
                        data.KeyShow = data.StatusKey == 0 ? Resources.Language.lbmo : Resources.Language.lbtat;
                    }
                    else
                    {
                        data.KeyShow = data.StatusKey == 1 ? Resources.Language.lbmo : Resources.Language.lbtat;
                    }
                }
                else
                {
                    data.KeyShow = "Mở";
                }

                if (data.Door == 1)
                {
                    if (data.Switch_Door == 1)
                    {
                        data.DoorShow = data.StatusDoor == 0 ? Resources.Language.lbmo : Resources.Language.lbdong;
                    }
                    else
                    {
                        data.DoorShow = data.StatusDoor == 1 ? Resources.Language.lbmo : Resources.Language.lbdong;
                    }
                }
                else
                {
                    data.DoorShow = Resources.Language.lbdong;
                }

                if (DateTime.Compare(data.DateExpired.Value.Date, DateTime.Now.Date) < 0)
                {
                    data.Status = Resources.Language.lbhethandichvu;
                    data.color = "red";
                    data.Addr = "";
                    data.stringStatus = "CanhBao";
                    data.status_id = 6;
                    return data;
                }

                if (data.Speed != null)
                {

                    if (data.Latitude > 0)
                    {

                        DateTime date = data.DateSave.Value;
                        subtract_Result = DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes;
                        if (subtract_Result > 10)
                        {
                            //if (data.Version.Equals("2.0") || data.Version.Equals("3.0") || data.Version.Equals("3.11"))
                            //{
                            //    if (data.Sleep != null)
                            //    {
                            //        if (data.Sleep == 0)
                            //        {
                            //            data.Status = "Mất LL " +
                            //                          ConverteTime(
                            //                              DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                            //            data.color = "red";
                            //            data.stringStatus = "MatLL";
                            //            return data;
                            //        }

                            //    }
                            //}
                            //else
                            //{
                            //    data.Status = "Mất LL " +
                            //                  ConverteTime(DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                            //    data.color = "red";
                            //    data.stringStatus = "MatLL";
                            //    return data;
                            //} cuuuuuuuuuuuuuuuuuuu
                            double totalmi = DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes;
                            data.Status = Resources.Language.lbmatll_ +
                                              ConverteTime(totalmi);
                            data.color = "red";
                            data.stringStatus = "MatLL";
                            data.status_id = 1;
                            return data;
                        }
                        else if (data.Speed > 0)
                        {
                            string temp = "";
                            if (data.StatusDoor.Equals(switch_door_mo) && data.Door == 1)
                            {
                                if (!data.Version.Equals("3.0"))
                                {
                                    temp = temp + Resources.Language.lbcuamo;
                                    data.color = "#FFC125";
                                    data.stringStatus = "CanhBao";
                                }
                            }
                            else
                            {
                                data.color = "green";
                                //data.color = "blue";
                            }
                            if (data.Speed > data.SpeedLimit)
                            {
                                temp += " ("+Resources.Language.lbquatocdo+")";
                                data.color = "red";
                                data.stringStatus = "VuotToc";
                            }
                            data.Status = data.Speed.ToString() + "km/h" + temp;
                            return data;
                        }
                        //if (data.Sleep != null)
                        //{
                        //    if (data.Version.Equals("2.0") && data.Sleep == 1)
                        //    {
                        //        data.Status = "Đỗ " +
                        //                      ConverteTime(DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                        //        data.color = "black";
                        //        return data;
                        //    }
                        //    else if (data.Version.Equals("3.11") && data.Sleep == 1)
                        //    {
                        //        data.Status = "Đỗ " +
                        //                      ConverteTime(DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                        //        data.color = "black";
                        //        return data;
                        //    }

                        //}
                        //cu  if (data.StatusKey.Equals(switch_mo) && data.Speed == 0)
                        if (data.Speed == 0)
                        {
                            IEnumerable<DeviceStatus> data2 =
                                getDataBySwitch("{'_DeviceID':'" + data.DeviceID + "','_Switch_tat':'" +
                                                switch_tat.ToString() + "'}");
                            if (data2.FirstOrDefault() != null)
                            {
                                double timeTemp =
                                    DateTime.Now.Subtract(data2.FirstOrDefault().DateSave.Value).TotalMinutes;

                                data.Status = Resources.Language.lbdung+" " +
                                              ConverteTime(timeTemp);
                                if (timeTemp > 15)
                                {
                                    data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                }
                                data.color = "black";
                            }
                            else
                            {
                                IEnumerable<DeviceStatus> data3 =
                                    getDataFirst("{'_DeviceID':'" + data.DeviceID + "'}");
                                if (data3 != null)
                                {
                                    double timeTemp =
                                        DateTime.Now.Subtract(data3.FirstOrDefault().DateSave.Value).TotalMinutes;

                                    data.Status = Resources.Language.lbdung+" " + ConverteTime(timeTemp);
                                    if (timeTemp > 15)
                                    {
                                        data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                    }
                                    data.color = "black";
                                }
                            }

                        }
                        if (data.StatusKey.Equals(switch_tat) &&
                            !(data.Version.Equals("2.0") && data.Version.Equals("3.11")))
                        {
                            //if (
                            //    !(data.Version.Equals("2.0") && data.Version.Equals("3.11")))
                            //{
                            if (data.Speed > 0)
                            {
                                string temp = "";
                                if (data.StatusDoor.Equals(switch_door_mo) && data.Door == 1)
                                {
                                    if (!data.Version.Equals("3.0"))
                                    {
                                        temp = temp +" "+ Resources.Language.lbcuamo;
                                        data.color = "#FFC125";
                                        data.stringStatus = "CanhBao";
                                    }
                                }
                                else
                                {
                                    data.color = "green";
                                    //  data.color = "blue";
                                }
                                if (data.Speed > data.SpeedLimit)
                                {
                                    //temp += " (quá tốc độ)";
                                    data.color = "red";
                                    data.stringStatus = "VuotToc";
                                }
                                data.Status = data.Speed + "km/h" + temp;
                                return data;
                            }
                            else
                            {
                                IEnumerable<DeviceStatus> data2 =
                                    getDataBySwitch("{'_DeviceID':'" + data.DeviceID + "','_Switch_tat':'" +
                                                    switch_mo.ToString() + "'}");
                                if (data2.FirstOrDefault() != null)
                                {
                                    double timeTemp =
                                       DateTime.Now.Subtract(data2.FirstOrDefault().DateSave.Value).TotalMinutes;

                                    data.Status = Resources.Language.lbdung+" " + ConverteTime(timeTemp);
                                    if (timeTemp > 15)
                                    {
                                        data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                    }
                                    data.color = "black";
                                }
                                else
                                {
                                    IEnumerable<DeviceStatus> data3 =
                                        getDataFirst("{'_DeviceID':'" + data.DeviceID + "'}");
                                    if (data3 != null)
                                    {
                                        double timeTemp =
                                            DateTime.Now.Subtract(data3.FirstOrDefault().DateSave.Value).TotalMinutes;

                                        data.Status = Resources.Language.lbdung+" " + ConverteTime(timeTemp);
                                        if (timeTemp > 15)
                                        {
                                            data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                        }
                                        data.color = "black";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        DateTime datesavetemp = data.DateSave.Value;
                        DeviceStatus data_truocdo = getData_truocdo("{'_DeviceID':'" + data.DeviceID + "'}");
                        if (data_truocdo != null)
                        {
                            data.Latitude = data_truocdo.Latitude;
                            data.Longitude = data_truocdo.Longitude;
                            data.Addr = data_truocdo.Addr;
                            data.AddressID = data_truocdo.AddressID;

                            //  data.DateSave = data_truocdo.DateSave;


                            subtract_Result = DateTime.Now.Subtract(datesavetemp).TotalMinutes;
                            if (subtract_Result > 10)
                            {
                                data.DateSave = datesavetemp;
                                data.Status = Resources.Language.lbmatll_+" " +
                                              ConverteTime(DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                                data.color = "red";
                                data.stringStatus = "MatLL";
                                data.status_id = 1;
                                //if ((data.Version.Equals("2.0") || data.Version.Equals("3.0") || data.Version.Equals("3.11")) &&
                                //    data.Sleep == 1)
                                //{
                                //    IEnumerable<DeviceStatus> data2 =
                                //        getDataBySwitch("{'_DeviceID':'" + data.DeviceID + "','_Switch_tat':'" +
                                //                        switch_tat.ToString() + "'}");
                                //    data.color = "red";
                                //    if (data2.FirstOrDefault() != null)
                                //    {
                                //        DateTime dt = data2.FirstOrDefault().DateSave.Value;
                                //        data.Status = "Đỗ " + ConverteTime(DateTime.Now.Subtract(dt).TotalMinutes);
                                //        data.color = "black";
                                //    }
                                //    else
                                //    {
                                //        IEnumerable<DeviceStatus> data3 =
                                //            getDataFirst("{'_DeviceID':'" + data.DeviceID + "'}");
                                //        if (data3 != null)
                                //        {
                                //            DateTime dt = data3.FirstOrDefault().DateSave.Value;
                                //            data.Status = "Đỗ " + ConverteTime(DateTime.Now.Subtract(dt).TotalMinutes);
                                //            data.color = "black";
                                //        }
                                //    }
                                //}
                            }
                            else
                            {

                                double timep = DateTime.Now.Subtract(data_truocdo.DateSave.Value).TotalMinutes;
                                if (timep < 5)
                                {
                                    data.Status = Resources.Language.lbdung+" " +
                                                  ConverteTime(timep);
                                    data.color = "black";
                                    if (timep > 15)
                                    {
                                        data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                    }
                                }
                                else
                                {
                                    data.Status = Resources.Language.lbmatgps+" " +
                                                  ConverteTime(timep);
                                    data.color = "black";
                                    data.stringStatus = "MatGPS";
                                    data.status_id = 2;
                                }
                            }
                        }
                        else
                        {
                            double timep = DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes;
                            if (timep > 15)
                            {
                                data.Status = Resources.Language.lbmatllvachuacogps;
                                data.color = "red";
                                data.status_id = 3;
                            }
                            else
                            {
                                data.Status = Resources.Language.lbchuacogps;
                                data.color = "black";
                                data.status_id = 4;
                            }
                            data.Addr = "";

                            return data;
                        }

                    }
                    if (String.IsNullOrEmpty(data.Addr))
                        data.Addr = Resources.Language.lbnhapvaoxem;
                }
                else
                {
                    data.Status = Resources.Language.tbchuacodulieu;
                    data.Addr = "";
                    data.color = "red";
                    data.status_id = 5;
                }

            }

            return data;
        }
        /*--------------------------------*/
        public dynamic getStatus(Dictionary<string, string> param)
        {
            long c = DateTime.Now.Ticks;
            string roleID = param.FirstOrDefault(m => m.Key == "_RoleID").Value;
            string node = param.FirstOrDefault(m => m.Key == "_Node").Value;
            string lang = param.FirstOrDefault(m => m.Key == "_Lang").Value;
            string userid = param.FirstOrDefault(m => m.Key == "_UserID").Value;
            string groupID = "";
            param.Remove("_RoleID");
            param.Remove("_Node");
            param.Remove("_Lang");
            if (node == "myTree")
            {
                IList<VehicleGroups> listGroup = new List<VehicleGroups>();
                listGroup = GetVehicleGroup(param);
                foreach (var item in listGroup)
                {
                    param.Add("_GroupID", item.VehicleGroupID.ToString());
                    IList<DeviceStatus> countVehicle = GetVehicleGroupParent(param);
                    param.Remove("_GroupID");
                    item.id = node + "/" + item.VehicleGroupID;
                    item.text = item.VehicleGroup + " (" + item.SL + " xe)";
                    if (lang == "en")
                    {
                        item.text = item.VehicleGroup + " (" + item.SL + " vehicle)";
                    }
                    item.@checked = false;
                }
                return listGroup;
            }
            IList<DeviceStatus> data = new List<DeviceStatus>();
            if (!String.IsNullOrEmpty(node))
            {
                string[] temp = node.Split('/');
                if (temp.Length >= 2)
                {
                    switch (temp.Length)
                    {
                        case 2:
                            {
                                groupID = temp[1];
                                param.Add("_GroupID", groupID);
                                data = GetVehicleGroupParent(param);
                                foreach (var item in data)
                                {
                                    item.id = node + "/" + item.VehicleGroupID;
                                    item.text = item.VehicleGroup + " (" + item.SL + " xe)";
                                    item.leaf = "false";
                                    item.@checked = false;
                                }
                                break;
                            }
                        case 3:
                            {
                                param.Add("_GroupID", temp[2]);
                                break;
                            }
                    }
                }
                IList<DeviceStatus> datatemp = GetDeviceLastTimeUser(param);
                if (datatemp != null)
                {
                    foreach (var s in datatemp)
                    {
                        s.leaf = "true";
                        data.Add(s);
                    }
                }
            }
            else // If on mobile web | node = null
            {
                IList<DeviceStatus> datatemp = GetAll_DeviceLastTimeUser(param);
                //try {
                //    foreach (var s in datatemp)
                //    {
                //        s.leaf = "true";
                //        data.Add(s);
                //    }
                //} catch (Exception ex) { }
                foreach (var s in datatemp)
                {
                    s.leaf = "true";
                    data.Add(s);
                }
            }

            if (data != null) //danhdau01
            {
                //double subtract_Result = 0;
                int count = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].leaf == "false")
                    {
                        data[i].STT = "";
                        continue;
                    }

                    data[i] = getStatus_ofDevice(data[i], roleID, node);
                    data[i].STT = (++count).ToString();

                    if (data[i].QCVN.Value == 1)
                    {
                        if (String.IsNullOrEmpty(data[i].NameDriver))
                        {
                            Driver driver = getDriverFirst("{'_DeviceID':'" + data[i].DeviceID + "'}");
                            if (driver != null)
                            {
                                data[i].NameDriver = driver.NameDriver;
                                data[i].PhoneDriver = driver.PhoneDriver;
                                data[i].DriverLicense = driver.DriverLicense;
                                data[i].DayCreateLicense = driver.DayCreateLicense;
                                data[i].DayExpiredLicense = driver.DayExpiredLicense;
                            }
                            else
                            {
                                data[i].NameDriver = Resources.Language.lbchuacothongtinlaixe;
                                data[i].PhoneDriver = "";
                                data[i].DriverLicense = "";
                            }
                        }
                    }

                    if (lang == "en")
                    {
                        string stts = data[i].Status;
                        if (stts == "Chưa có dữ liệu")
                            stts = "No data";
                        else if (stts == "Chưa có GPS")
                            stts = "No GPS data";
                        else if (stts == "Mất LL + Chưa có GPS")
                            stts = "Lost connect + No GPS";

                        if (!string.IsNullOrEmpty(stts))
                            stts = stts.Replace("Dừng", "Pause").Replace("Đỗ", "Stop").Replace("quá tốc độ", "over speed").
                                Replace("Hết hạn dịch vụ", "Expires service").Replace("cửa mở", "door open").
                                Replace("Mất LL", "Lost connect").Replace("phút", "min").Replace("giây", "sec");
                        data[i].Status = stts;
                    }
                    DeviceStatus obj_oil = getValueOil_Current(data[i]);
                    if (obj_oil != null)
                    {
                        data[i].OilValue = obj_oil.OilValue;
                        data[i].FuelCapacity = obj_oil.FuelCapacity;
                    }
                    List<GpsData> list_old = getOld_Coord(data[i]);
                    if (list_old.Count == 2)
                    {
                        data[i].OldLat_ = list_old.Count == 2 ? list_old[1].Latitude : list_old[0].Latitude;
                        data[i].OldLng_ = list_old.Count == 2 ? list_old[1].Longitude : list_old[0].Longitude;
                    }
                }
            }
            return data;
        }
        private dynamic GetVehicleGroup(Dictionary<string, string> param)
        {
            IList<VehicleGroups> list =
                Rep.ExecuteStoreProceduce<VehicleGroups>("sp_GetbyUserID_DeviceGroupT", param).ToList();
            return list;
        }
        private dynamic GetVehicleGroupParent(Dictionary<string, string> param)
        {
            IList<DeviceStatus> list =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetbyUserID_DeviceGroupChild", param).ToList();
            return list;
        }
        //sp_get_status_last_time_by_deviceid
        public DeviceStatus sp_get_status_last_time_by_deviceid(Dictionary<string, string> parameter)
        {
            DeviceStatus data =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_get_status_last_time_by_deviceid", parameter).FirstOrDefault();
            //try {
            //    if (data.FirstOrDefault() == null)
            //    { return null; }
            //} catch (Exception ex) { return null; }
            if (data == null)
            { return null; }

            return data;
        }
        private IList<DeviceStatus> GetAll_DeviceLastTimeUser(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceStatus> data =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetDeviceLastTimeUser22", parameter);
            //try {
            //    if (data.FirstOrDefault() == null)
            //    { return null; }
            //} catch (Exception ex) { return null; }
            if (data.FirstOrDefault() == null)
            { return null; }

            return data.ToList();
        }
        private IList<DeviceStatus> GetDeviceLastTimeUser(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceStatus> data =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetDeviceLastTimeUser_byGroup", parameter);
            if (data.FirstOrDefault() == null)
            {
                return null;
            }
            IList<DeviceStatus> a = data.ToList();
            return data.ToList();
        }
        public dynamic getStatus_1Device(Dictionary<string, string> param)
        {
            //string deviceID = param.FirstOrDefault(m => m.Key == "_DeviceID").Value;
            string roleID = param.FirstOrDefault(m => m.Key == "_RoleID").Value;
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("_UserID", param.FirstOrDefault(m => m.Key == "_UserID").Value);
            parameter.Add("_DeviceID", param.FirstOrDefault(m => m.Key == "_DeviceID").Value);
            IList<DeviceStatus> device = GetDeviceLastTimeByID(parameter);



            if (device != null)
            {
                DeviceStatus st = getStatus_ofDevice(device[0], roleID, "");
                //if (parameter["_UserID"].Equals("25106"))
                //{
                //    if (st.color.Equals("red"))
                //    {
                //        st.color = "black";
                //    }
                //}
                string number = "";
                number = getLastTheDriverByImei(device[0].DeviceID.ToString());
                //if (number != "")
                //{
                Driver driver =
                    getDriverByPhoneDriver("{'_DeviceID':'" + device[0].DeviceID.ToString() + "','_PhoneDriver':'" + number + "'}") ??
                    getDriverFirst("{'_DeviceID':'" + device[0].DeviceID.ToString() + "'}");
                if (driver != null)
                {
                    st.NameDriver = driver.NameDriver;
                    st.PhoneDriver = driver.PhoneDriver;
                    st.DriverLicense = driver.DriverLicense;
                    st.DayCreateLicense = driver.DayCreateLicense;
                    st.DayExpiredLicense= driver.DayExpiredLicense;
                    st.Rank = !String.IsNullOrEmpty(driver.Rank)? driver.Rank:"";
                    st.RegPlace = !String.IsNullOrEmpty(driver.RegPlace) ? driver.RegPlace : ""; 
                }
                else
                {
                    st.NameDriver = Resources.Language.lbchuacothongtinlaixe;
                    st.PhoneDriver = "";
                    st.DriverLicense = "";
                }

                DeviceStatus obj_oil = getValueOil_Current(st);
                if (obj_oil != null)
                {
                    st.OilValue = obj_oil.OilValue;
                    st.FuelCapacity = obj_oil.FuelCapacity;
                }
                List<GpsData> list_old = getOld_Coord(st);
                if (list_old.Count == 2)
                {
                    st.OldLat_ = list_old.Count == 2 ? list_old[1].Latitude : list_old[0].Latitude;
                    st.OldLng_ = list_old.Count == 2 ? list_old[1].Longitude : list_old[0].Longitude;
                }
                return st;
            }
            return null;
        }
        public DeviceStatus getValueOil_Current(DeviceStatus data_model)
        {
            // sp_tblinforcaloil_by_vehiclenumber _vehiclenumber
            //sp_getdata_for_Oil _deviceID
            DeviceStatus obj_ = new DeviceStatus();

            try
            {
                obj_.OilValue = 0;
                obj_.FuelCapacity = 0;
                Dictionary<string, string> parameter = new Dictionary<string, string>();
                parameter.Add("_vehiclenumber", data_model.VehicleNumber);
                OilInfomation OilInfomation_ = Rep.ExecuteStoreProceduce<OilInfomation>("sp_tblinforcaloil_by_vehiclenumber", parameter).FirstOrDefault();
                if (OilInfomation_ != null)
                {

                    parameter = new Dictionary<string, string>();
                    parameter["_deviceID"] = data_model.DeviceID.ToString(); ;
                    IList<GpsdataExtForOil> List_GpsdataExtForOil_ =
                    Rep.ExecuteStoreProceduce<GpsdataExtForOil>("sp_getdata_for_Oil", parameter).ToList();
                    if (List_GpsdataExtForOil_.Count > 0)
                    {
                        //List_GpsdataExtForOil_ = List_GpsdataExtForOil_.OrderByDescending(m => m.DateSave).Where(m => m.Oilvalue <= OilInfomation_.VoltMax).ToList();
                        List_GpsdataExtForOil_ = List_GpsdataExtForOil_.OrderByDescending(m => m.DateSave).ToList();
                        obj_.OilValue = (List_GpsdataExtForOil_[0].Oilvalue.Value * OilInfomation_.VolumeOilBarrel.Value) / OilInfomation_.VoltMax.Value;

                        obj_.FuelCapacity = OilInfomation_.VolumeOilBarrel;
                    }
                }
            }
            catch
            {
                throw;
            }
            return obj_;
        }
        public List<GpsData> getOld_Coord(DeviceStatus data_model)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("_deviceID", data_model.DeviceID.ToString());
            //sp_get_OldCoord
            List<GpsData> list_Gpsdata =
               Rep.ExecuteStoreProceduce<GpsData>("sp_get_OldCoord", parameter).ToList();
            return list_Gpsdata;
        }
        private string getLastTheDriverByImei(string deviceID)
        {

            Dictionary<string, string> parameter = null;
            if (deviceID != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>("{'_DeviceID':'" + deviceID + "'}");
            }
            string number =
                Rep.ExecuteStoreProceduce<String>("sp_getLastDriver_byID", parameter).FirstOrDefault();
            return number;
        }
        public Driver getDriverByPhoneDriver(string param)
        {
            Dictionary<string, string> parameter = null;
            if (param != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(param);
            }
            Driver driver =
                Rep.ExecuteStoreProceduce<Driver>("sp_getDriverByPhoneDriver", parameter).FirstOrDefault();
            return driver;
        }
        public Driver getDriverFirst(string param)
        {
            Dictionary<string, string> parameter = null;
            if (param != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(param);
            }
            Driver driver =
                Rep.ExecuteStoreProceduce<Driver>("sp_getDriverFirstByDeviceID", parameter).FirstOrDefault();
            return driver;
        }
        private DeviceStatus getStatus_ofDevice(DeviceStatus data, string roleID, string node)
        {

            double subtract_Result = 0;
            string imei = data.Imei;
            data.DLat = data.Latitude - data.OldLat;
            data.DLng = data.Longitude - data.OldLng;
            //data.OldLat = 0;
            //data.OldLng = 0;

            if (imei != null || imei != "")
            {
                if (data.Addr == null || data.Addr == "")
                    data.Addr = Resources.Language.lbnhapvaoxem;

                data.id = node + "/" + data.DeviceID;
                data.leaf = "true";
                data.@checked = false;
                data.text = data.VehicleNumber;
                int switchtemp = data.Switch_;
                int switch_tat = 0;
                int switch_mo = 1;
                if (switchtemp == 1)
                {
                    switch_tat = 1;
                    switch_mo = 0;
                }
                int switch_doortemp = data.Switch_Door;
                int switch_door_tat = 0;
                int switch_door_mo = 1;
                if (switch_doortemp == 1)
                {
                    switch_door_tat = 1;
                    switch_door_mo = 0;
                }

                if (data.Key_ == 1)
                {
                    if (data.Switch_ == 1)
                    {
                        data.KeyShow = data.StatusKey == 0 ? Resources.Language.lbmo : Resources.Language.lbtat;
                        if (data.StatusKey == 0)
                        {
                            data.status_id = 14;//khoaMo
                        }
                        else
                        {
                            data.status_id = 15;//khoaTat
                        }
                    }
                    else
                    {
                        data.KeyShow = data.StatusKey == 1 ? Resources.Language.lbmo : Resources.Language.lbtat;
                        if (data.StatusKey == 1)
                        {
                            data.status_id = 14;//khoaMo
                        }
                        else {
                            data.status_id = 15;//khoaTat
                        }

                    }
                }
                else
                {
                    data.KeyShow = Resources.Language.lbmo;
                    data.status_id = 14;//khoaMo
                }

                if (data.Door == 1)
                {
                    if (data.Switch_Door == 1)
                    {
                        data.DoorShow = data.StatusDoor == 0 ? Resources.Language.lbmo : Resources.Language.lbdong;
                    }
                    else
                    {
                        data.DoorShow = data.StatusDoor == 1 ? Resources.Language.lbmo : Resources.Language.lbdong;
                    }
                }
                else
                {
                    data.DoorShow = Resources.Language.lbdong;
                }

                if (DateTime.Compare(data.DateExpired.Value.Date, DateTime.Now.Date) < 0)
                {
                    data.Status = Resources.Language.lbhethandichvu;
                    data.color = "red";
                    data.Addr = "";
                    data.stringStatus = "CanhBao";
                    data.status_id = 6;
                    return data;
                }

                if (data.Speed != null)
                {

                    if (data.Latitude > 0)
                    {

                        DateTime date = data.DateSave.Value;
                        subtract_Result = DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes;
                        if (subtract_Result > 25)
                        {
                            //if (data.Version.Equals("2.0") || data.Version.Equals("3.0") || data.Version.Equals("3.11"))
                            //{
                            //    if (data.Sleep != null)
                            //    {
                            //        if (data.Sleep == 0)
                            //        {
                            //            data.Status = "Mất LL " +
                            //                          ConverteTime(
                            //                              DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                            //            data.color = "red";
                            //            data.stringStatus = "MatLL";
                            //            return data;
                            //        }

                            //    }
                            //}
                            //else
                            //{
                            //    data.Status = "Mất LL " +
                            //                  ConverteTime(DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                            //    data.color = "red";
                            //    data.stringStatus = "MatLL";
                            //    return data;
                            //} cuuuuuuuuuuuuuuuuuuu
                            double totalmi = DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes;
                            data.Status = Resources.Language.lbmatll_+" " +
                                              ConverteTime(totalmi);
                            data.color = "red";
                            data.stringStatus = "MatLL";
                            data.status_id = 1;
                            if (totalmi >= 2880)
                            {
                                Add_Fail(data.DeviceID, data.VehicleNumber, data.Status, data.stringStatus);
                            }
                            return data;
                        }
                        else if (data.Speed > 0)
                        {
                            string temp = "";
                            if (data.StatusDoor.Equals(switch_door_mo) && data.Door == 1)
                            {
                                if (!data.Version.Equals("3.0"))
                                {
                                    temp = temp + Resources.Language.lbcuamo;
                                    data.color = "#FFC125";
                                    data.stringStatus = "CanhBao";
                                }
                            }
                            else
                            {
                                data.color = "green";
                                //data.color = "blue";
                            }
                            if (data.Speed > data.SpeedLimit)
                            {
                                temp += " ("+ Resources.Language.lbquatocdo+ ")";
                                data.color = "red";
                                data.stringStatus = "VuotToc";
                            }
                            data.Status = data.Speed.ToString() + "km/h" + temp;
                            return data;
                        }
                        //if (data.Sleep != null)
                        //{
                        //    if (data.Version.Equals("2.0") && data.Sleep == 1)
                        //    {
                        //        data.Status = "Đỗ " +
                        //                      ConverteTime(DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                        //        data.color = "black";
                        //        return data;
                        //    }
                        //    else if (data.Version.Equals("3.11") && data.Sleep == 1)
                        //    {
                        //        data.Status = "Đỗ " +
                        //                      ConverteTime(DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                        //        data.color = "black";
                        //        return data;
                        //    }

                        //}
                        //cu  if (data.StatusKey.Equals(switch_mo) && data.Speed == 0)
                        if (data.Speed == 0)
                        {
                            IEnumerable<DeviceStatus> data2 =
                                getDataBySwitch("{'_DeviceID':'" + data.DeviceID + "','_Switch_tat':'" +
                                                switch_tat.ToString() + "'}");
                            if (data2.FirstOrDefault() != null)
                            {
                                double timeTemp =
                                    DateTime.Now.Subtract(data2.FirstOrDefault().DateSave.Value).TotalMinutes;

                                data.Status = Resources.Language.lbdung+" " +
                                              ConverteTime(timeTemp);
                                data.status_id = 11;//Dung
                                if (timeTemp > 15)
                                {
                                    data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                    data.status_id = 12;//Do
                                }
                                data.color = "black";
                                
                            }
                            else
                            {
                                IEnumerable<DeviceStatus> data3 =
                                    getDataFirst("{'_DeviceID':'" + data.DeviceID + "'}");
                                if (data3 != null)
                                {
                                    double timeTemp =
                                        DateTime.Now.Subtract(data3.FirstOrDefault().DateSave.Value).TotalMinutes;

                                    data.Status = Resources.Language.lbdung+" " + ConverteTime(timeTemp);
                                    data.status_id = 11;//Dung
                                    if (timeTemp > 15)
                                    {
                                        data.status_id = 12;//Dung
                                        data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                    }
                                    data.color = "black";
                                }
                            }

                        }
                        if (data.StatusKey.Equals(switch_tat) &&
                            !(data.Version.Equals("2.0") && data.Version.Equals("3.11")))
                        {
                            //if (
                            //    !(data.Version.Equals("2.0") && data.Version.Equals("3.11")))
                            //{
                            if (data.Speed > 0)
                            {
                                string temp = "";
                                if (data.StatusDoor.Equals(switch_door_mo) && data.Door == 1)
                                {
                                    if (!data.Version.Equals("3.0"))
                                    {
                                        temp = temp + Resources.Language.lbcuamo;
                                        data.color = "#FFC125";
                                        data.stringStatus = "CanhBao";
                                    }
                                }
                                else
                                {
                                    data.color = "green";
                                    //  data.color = "blue";
                                }
                                if (data.Speed > data.SpeedLimit)
                                {
                                    //temp += " (quá tốc độ)";
                                    data.color = "red";
                                    data.stringStatus = "VuotToc";
                                }
                                data.Status = data.Speed + "km/h" + temp;
                                data.status_id = 13;//chay
                                return data;
                            }
                            else
                            {
                                IEnumerable<DeviceStatus> data2 =
                                    getDataBySwitch("{'_DeviceID':'" + data.DeviceID + "','_Switch_tat':'" +
                                                    switch_mo.ToString() + "'}");
                                if (data2.FirstOrDefault() != null)
                                {
                                    double timeTemp =
                                       DateTime.Now.Subtract(data2.FirstOrDefault().DateSave.Value).TotalMinutes;
                                    data.status_id = 11;//Dung
                                    data.Status = Resources.Language.lbdung+" " + ConverteTime(timeTemp);
                                    if (timeTemp > 15)
                                    {
                                        data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                        data.status_id = 12;//Do
                                    }
                                    data.color = "black";
                                }
                                else
                                {
                                    IEnumerable<DeviceStatus> data3 =
                                        getDataFirst("{'_DeviceID':'" + data.DeviceID + "'}");
                                    if (data3 != null)
                                    {
                                        double timeTemp =
                                            DateTime.Now.Subtract(data3.FirstOrDefault().DateSave.Value).TotalMinutes;

                                        data.Status = Resources.Language.lbdung+" " + ConverteTime(timeTemp);
                                        data.status_id = 11;//Dung
                                        if (timeTemp > 15)
                                        {
                                            data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                            data.status_id = 12;//Do
                                        }
                                        data.color = "black";
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        DateTime datesavetemp = data.DateSave.Value; //danhdau01
                        DeviceStatus data_truocdo = getData_truocdo("{'_DeviceID':'" + data.DeviceID + "'}");
                        if (data_truocdo != null)
                        {
                            data.Latitude = data_truocdo.Latitude;
                            data.Longitude = data_truocdo.Longitude;
                            data.Addr = data_truocdo.Addr;
                            data.AddressID = data_truocdo.AddressID;

                            //  data.DateSave = data_truocdo.DateSave;


                            subtract_Result = DateTime.Now.Subtract(datesavetemp).TotalMinutes;
                            if (subtract_Result > 25)
                            {
                                data.DateSave = datesavetemp;
                                data.Status = Resources.Language.lbmatll_ +" " +
                                              ConverteTime(DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes);
                                data.color = "red";
                                data.stringStatus = "MatLL";
                                data.status_id = 1;
                                if (subtract_Result >= 2880)
                                {

                                    Add_Fail(data.DeviceID, data.VehicleNumber, data.Status, data.stringStatus);

                                }
                                //if ((data.Version.Equals("2.0") || data.Version.Equals("3.0") || data.Version.Equals("3.11")) &&
                                //    data.Sleep == 1)
                                //{
                                //    IEnumerable<DeviceStatus> data2 =
                                //        getDataBySwitch("{'_DeviceID':'" + data.DeviceID + "','_Switch_tat':'" +
                                //                        switch_tat.ToString() + "'}");
                                //    data.color = "red";
                                //    if (data2.FirstOrDefault() != null)
                                //    {
                                //        DateTime dt = data2.FirstOrDefault().DateSave.Value;
                                //        data.Status = "Đỗ " + ConverteTime(DateTime.Now.Subtract(dt).TotalMinutes);
                                //        data.color = "black";
                                //    }
                                //    else
                                //    {
                                //        IEnumerable<DeviceStatus> data3 =
                                //            getDataFirst("{'_DeviceID':'" + data.DeviceID + "'}");
                                //        if (data3 != null)
                                //        {
                                //            DateTime dt = data3.FirstOrDefault().DateSave.Value;
                                //            data.Status = "Đỗ " + ConverteTime(DateTime.Now.Subtract(dt).TotalMinutes);
                                //            data.color = "black";
                                //        }
                                //    }
                                //}
                            }
                            else
                            {

                                double timep = DateTime.Now.Subtract(data_truocdo.DateSave.Value).TotalMinutes;
                                if (timep < 5)
                                {
                                    data.Status = Resources.Language.lbdung+" " +
                                                  ConverteTime(timep);
                                    data.color = "black";
                                    data.status_id = 11;//Dung
                                    if (timep > 15)
                                    {
                                        data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                        data.status_id = 12;//Do
                                    }
                                }
                                else if (data.Version == "LK")
                                {
                                    data.Status = Resources.Language.lbdung+" " +
                                            ConverteTime(timep);
                                    data.color = "black";
                                    data.status_id = 11;//Dung
                                    if (timep > 15)
                                    {
                                        data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                        data.status_id = 12;//Do
                                    }
                                }
                                else
                                {
                                    data.Status = Resources.Language.lbmatgps+" " +
                                                  ConverteTime(timep);
                                    data.color = "black";
                                    data.stringStatus = "MatGPS";
                                    data.status_id = 2;
                                    if (timep > 720)
                                    {
                                        Add_Fail(data.DeviceID, data.VehicleNumber, data.Status, data.stringStatus);
                                    }
                                }
                            }
                        }
                        else
                        {
                            double timep = DateTime.Now.Subtract(data.DateSave.Value).TotalMinutes;
                            if (timep > 15)
                            {
                                data.Status = Resources.Language.lbmatllvachuacogps;
                                data.status_id = 3;
                                data.color = "red";
                                if (timep > 2880)
                                    Add_Fail(data.DeviceID, data.VehicleNumber, data.Status, data.stringStatus);
                            }
                            else
                            {
                                data.Status = Resources.Language.lbchuacogps;
                                data.status_id = 4;
                                data.color = "black";
                            }
                            data.Addr = "";

                            return data;
                        }

                    }
                    if (String.IsNullOrEmpty(data.Addr))
                        data.Addr = Resources.Language.lbnhapvaoxem;
                   
                }
                else
                {
                    data.Status = Resources.Language.tbchuacodulieu;
                    data.Addr = "";
                    data.color = "red";
                    data.status_id = 5;
                }

            }

            return data;
        }
        public bool Add_Fail(int DeviceID, string vehicle, string Content, string status)
        {
            Dictionary<string, string> parameter = new Dictionary<string, string>
                        {
                            {"_DeviceID", DeviceID.ToString()},
                            {"_FVehicle", vehicle},
                            {"_FContent", Content},
                            {"_FStatus", status}

                        };
            return Rep.ExecuteSqlCommand("sp_Add_Fail", parameter);
        }
        public IList<DeviceStatus> GetDeviceLastTimeByID(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceStatus> data =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetDeviceLastTimeByDeviceID", parameter);
            if (data.FirstOrDefault() == null)
            {
                return null;
            }
            return data.ToList();
        }
        public dynamic LoTrinh(Dictionary<string, string> param) //  Imei, from, to
        {
            IList<DeviceStatus> listrs = new List<DeviceStatus>();
            int iDate = 0;

            string totemp = param.FirstOrDefault(pair => pair.Key == "To").Value.Substring(3, 2);
            string fromtemp = param.FirstOrDefault(pair => pair.Key == "From").Value.Substring(3, 2);

            DateTime dfrom = DateTime.Parse("20" + param["From"].ToString());
            DateTime dto = DateTime.Parse("20" + param["To"].ToString());
            //if (int.Parse(fromtemp) - 1 == DateTime.Now.Month - 1 || int.Parse(fromtemp) == 0)
            string monthnow = DateTime.Now.Month.ToString();
            if (int.Parse(fromtemp) == DateTime.Now.AddMonths(-1).Month || int.Parse(fromtemp) == 0)
            {
                if (DateTime.Now.Day < 5)
                {
                    iDate = 2;
                }
                else
                {
                    if (monthnow.Length == 1)
                        monthnow = "0" + monthnow;
                    if ((fromtemp != totemp) && (totemp == monthnow))
                    {
                        iDate = 3;
                    }
                    else if ((fromtemp == totemp) && (totemp == monthnow))
                    {
                        iDate = 2;
                    }
                    else
                    {
                        iDate = 1; // 1 la db cu; 2 la db moi; 3 la ca 2 db
                    }
                    //if ((totemp == "12") && (fromtemp == "12"))
                    //{
                    //    iDate = 2;
                    //}
                }
            }
            //else if (monthnow == "1" && int.Parse(fromtemp) == 12)
            //{
            //    if (DateTime.Now.Day < 5)
            //    {
            //        iDate = 2;
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
            //            iDate = 2;
            //        }
            //        else if ((fromtemp == totemp) && (totemp == monthnow))
            //        {
            //            iDate = 2;
            //        }
            //    }
            //}

            else
            {
                if (monthnow.Length == 1)
                    monthnow = "0" + monthnow;
                if ((fromtemp != totemp) && ((totemp == monthnow || totemp == "12")))
                {
                    iDate = 3;
                }
                else if ((fromtemp == totemp) && (totemp == monthnow))
                {
                    iDate = 2;
                }
                else
                {
                    iDate = 1; // 1 la db cu; 2 la db moi; 3 la ca 2 db
                }
                //if ((totemp == "12") && (fromtemp == "12"))
                //{
                //    iDate = 2;
                //}
            }

            Dictionary<string, string> parameter2 = new Dictionary<string, string>();
            if (param != null)
            {
                parameter2.Add("_DeviceID", param.FirstOrDefault(p => p.Key == "_DeviceID").Value.Replace(",",""));
            }

            //DateTime DateExpired;
            //string timestemp="";
            IEnumerable<DeviceStatus> timestemp =
                Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetDateExpired_byDeviceID", parameter2);
            if (timestemp != null)
            {
                //DateExpired = ;
                if (DateTime.Compare(timestemp.FirstOrDefault().DateExpired.Value, DateTime.Now.Date) < 0)
                {
                    return new { isActive = "false" };
                }
                else
                {

                    IList<DeviceStatus> list = new List<DeviceStatus>();
                    Services_khndb4_backupT backupServiceT = new Services_khndb4_backupT();
                    ReportService rpService = new ReportService();
                    switch (iDate)
                    {
                        case 1:
                            {
                                Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
                                paramOld["_DeviceID"] = param["_DeviceID"];
                                list = backupServiceT.DataForLoTrinh(paramOld).Where(m => m.DateSave > dfrom && m.DateSave < dto).OrderBy(item => item.DateSave).ToList();

                                break;
                            }
                        case 2:
                            {
                                Dictionary<string, string> parameter = rpService.getParamNew(param);
                                parameter["_DeviceID"] = param["_DeviceID"];
                                list = Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetLotrinh_FromTo",
                                                                                    parameter).Where(m => m.DateSave > dfrom && m.DateSave < dto).OrderBy(item => item.DateSave).ToList();
                                break;
                            }
                        case 3:
                            {
                                Dictionary<string, string> paramOld = backupServiceT.paramOld(param);
                                paramOld["_DeviceID"] = param["_DeviceID"];
                                list = backupServiceT.DataForLoTrinh(paramOld).Where(m => m.DateSave > dfrom && m.DateSave < dto).OrderBy(item => item.DateSave).ToList();
                                //param["_from"] = param["_from"].Substring(0, 4) + "-" + DateTime.Now.Month + "-" + "01 " +
                                //                param["_from"].Substring(11);

                                string monthNow = DateTime.Now.Month.ToString();
                                if (monthNow.Length == 1)
                                {
                                    monthNow = "0" + monthNow;
                                }
                                param["From"] = param["To"].Substring(0, 2) + "-" + param["To"].Substring(3, 2) + "-" + "01 " +
                                                param["From"].Substring(9);
                                Dictionary<string, string> parameter = rpService.getParamNew(param);
                                parameter["_DeviceID"] = param["_DeviceID"];
                                IList<DeviceStatus> listNew =
                                    Rep.ExecuteStoreProceduce<DeviceStatus>("sp_GetLotrinh_FromTo",
                                                                                 parameter).OrderBy(item => item.DateSave).ToList();
                                foreach (var gpsDataExt in listNew)
                                {
                                    list.Add(gpsDataExt);
                                }
                                break;
                            }
                    }


                    //       IEnumerable<DeviceStatus> ListLoTrinh = Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetLotrinh_FromTo", param);
                    DeviceStatus temp = null;
                    //foreach (var lt in list)
                    //{

                    //    if (temp != null)
                    //    {
                    //        if (lt.Speed == 0 && temp.Speed == 0 && lt.StatusKey == temp.StatusKey)
                    //            continue;
                    //    }
                    //    if (lt.Addr == "" || lt.Addr == null)
                    //        lt.Addr = "chưa xác định";
                    //    temp = lt;

                    //    lt.DLat = lt.Latitude - lt.OldLat;
                    //    lt.DLng = lt.Longitude - lt.OldLng;
                    //    lt.OldLat = 0;
                    //    lt.OldLng = 0;
                    //    listrs.Add(lt);
                    //}

                    if (list != null && list.Count > 0)
                    {
                        string tempPhoneDriver = list[0].TheDriver;
                        Driver drivertemp = getDriverByPhoneDriver("{'_DeviceID':'" + list[0].DeviceID + "','_PhoneDriver':'" + tempPhoneDriver + "'}") ??
                        getDriverFirst("{'_DeviceID':'" + list[0].DeviceID + "'}");
                        if (drivertemp == null)
                        {
                            drivertemp = new Driver();
                            drivertemp.NameDriver = "không xác định";
                            drivertemp.DriverLicense = "không xác định";
                            drivertemp.PhoneDriver = "không xác định";
                        }

                        for (int i = 0; i < list.Count; i++)
                        {
                            if (temp != null)
                            {
                                if (list[i].Speed == 0 && temp.Speed == 0 && list[i].StatusKey == temp.StatusKey && i < list.Count - 1)
                                    continue;
                            }
                            if (string.IsNullOrEmpty(list[i].Addr))
                                list[i].Addr = "chưa xác định";
                            temp = list[i];

                            if (i > 0)
                            {
                                list[i].DLat = list[i].Latitude - list[i - 1].Latitude;
                                list[i].DLng = list[i].Longitude - list[i - 1].Longitude;
                            }
                            if (list[i].TheDriver != tempPhoneDriver)
                            {
                                tempPhoneDriver = list[i].TheDriver;
                                drivertemp = getDriverByPhoneDriver("{'_DeviceID':'" + list[0].DeviceID + "','_PhoneDriver':'" + tempPhoneDriver + "'}") ??
                                    getDriverFirst("{'_DeviceID':'" + list[0].DeviceID + "'}");
                                if (drivertemp == null)
                                {
                                    drivertemp = new Driver();
                                    drivertemp.NameDriver = "không xác định";
                                    drivertemp.DriverLicense = "không xác định";
                                    drivertemp.PhoneDriver = "không xác định";

                                }

                            }
                            list[i].NameDriver = drivertemp.NameDriver;
                            list[i].DriverLicense = drivertemp.DriverLicense;
                            list[i].DayCreateLicense = drivertemp.DayCreateLicense;
                            list[i].DayExpiredLicense = drivertemp.DayExpiredLicense;
                            list[i].PhoneDriver = drivertemp.PhoneDriver;
                            listrs.Add(list[i]);
                        }


                        if (listrs.Count > 3000)
                        {
                            IList<DeviceStatus> listrs2 = new List<DeviceStatus>();
                            for (int ii = 0; ii < listrs.Count; ii += 4)
                            {
                                listrs2.Add(listrs[ii]);
                            }
                            return listrs2;

                        }
                    }

                    return listrs;
                }
            }
            return null;
        }
        public List<Device> sp_get_all_tbldevice() {

            List<Device> ListDV =
               Rep.ExecuteStoreProceduce<Device>("sp_get_all_tbldevice").ToList();

            return ListDV;
        }

        public bool sp_update_nameDevice(Dictionary<String, String> param)
        {
            return Rep.ExecuteSqlCommand("sp_update_nameDevice", param) ;
        }
        public bool sp_Update_Password(String _userName, String _passWord)
        {
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_userName"] = _userName;
            param["_passWord"] = _passWord;
            return Rep.ExecuteSqlCommand("sp_Update_Password", param);
        }
        public Boolean sp_UpdateUser_info_(Dictionary<String, String> parameter)
        {
            return Rep.ExecuteSqlCommand("sp_UpdateUser_info_", parameter);
        }
        public Boolean sp_update_commentprovider(Dictionary<String, String> parameter)
        {
            return Rep.ExecuteSqlCommand("sp_update_commentprovider", parameter);
        }
        public dynamic GetUser_ByID(Dictionary<string, string> parameter)
        {
            IEnumerable<Users> data = Rep.ExecuteStoreProceduce<Users>("sp_GetUser_ByID", parameter);
            return data;
        }

        //public Boolean sp_InsertAcc_byUserLogin(Dictionary<String, String> parameter)
        //{
        //    return Rep.ExecuteSqlCommand("sp_InsertAcc_byUserLogin", parameter);
        //}

        public dynamic GetUser_ByWhoCreateID(Dictionary<string, string> parameter)
        {
            IEnumerable<Users> data = Rep.ExecuteStoreProceduce<Users>("sp_GetUser_byWhoCreateIDT", parameter);
            return data;
        }

        // Thiết bị - tài xế
        public bool Update_Device(Dictionary<string, string> parameter) // 20
        {
            int dvCu = Get_DeviceByVehicleNumber(parameter["VehicleNumber"]);
            if (dvCu == int.Parse(parameter["DeviceID"]) || dvCu == 0)
            {
                Dictionary<string, string> param =
                    new Dictionary<string, string>
                        {
                            {"_DeviceID", parameter["DeviceID"]},
                            {"_VehicleID", parameter["VehicleID"]},
                            {"_VehicleGroupID", string.IsNullOrEmpty(parameter["VehicleGroupIDChild"])?"0":parameter["VehicleGroupIDChild"]},
                            {"_VehicleNumber", parameter["VehicleNumber"]},
                            {"_VehicleCategoryID", parameter["VehicleCategoryID"]},
                            {"_Capacity", parameter["Capacity"]},
                            {"_BarrelNumber", parameter["BarrelNumber"]},
                            {"_SimNumber", parameter["SimNumber"]},
                            {"_SimID", parameter["SimID"]},
                            {"_ProvinceID", parameter["_ProvinceID"]},
                            {"_TypeTransportID", parameter["_TypeTransportID"]},
                            {"_Business", parameter["Business"]},
                            {"_Chassis", parameter["Chassis"]},
                             {"_Grosston", parameter["Grosston"]},
                            {"_ParentID", parameter["ParentGroupID"]},
                            {"_CurrentChildID", parameter["_CurrentChildID"]},
                             {"_Switch_", parameter.FirstOrDefault(p => p.Key == "_Switch_").Value != null ? "1" : "0"},
                            {"_Switch_Door", parameter.FirstOrDefault(p => p.Key == "_Switch_Door").Value != null ? "1" : "0"},
                            {"_FuelConsumption", parameter["FuelConsumption"]},
                             {"_FuelCapacity", parameter["FuelCapacity"]},
                        };
                return Rep.ExecuteSqlCommand("sp_Update_DeviceGroupUserT", param);
            }
            return false;
        }
        public int Get_DeviceByVehicleNumber(string VehicleNumber)
        {
            Dictionary<string, string> param =
                new Dictionary<string, string>
                    {
                        {"_VehicleNumber", VehicleNumber},
                    };
            Device dv = Rep.ExecuteStoreProceduce<Device>("sp_GetDevice_byVehicleNumber", param).FirstOrDefault();
            if (dv != null)
                return dv.DeviceID;
            return 0;
        }
        public bool CreateDriver(Dictionary<string, string> parameter)
        {
            string dtexpired = Convert.ToDateTime(DateTime.ParseExact(parameter["DayExpiredLicense"],
                               "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToString("yyyy/MM/dd");
            string dtcreate = Convert.ToDateTime(DateTime.ParseExact(parameter["DayCreateLicense"],
                                "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToString("yyyy/MM/dd");
            Dictionary<string, string> driver =
                new Dictionary<string, string>
                    {
                         {"_NameDriver", parameter["NameDriver"]},
                         {"_PhoneDriver", parameter["PhoneDriver"]},
                         {"_IdCard", parameter["IdCard"]},
                         {"_DriverLicense", parameter["DriverLicense"]},
                         {"_Rank", parameter["Rank"]},
                         {"_DayCreateLicense", dtcreate},
                         {"_DayExpiredLicense",dtexpired},
                         {"_Details", parameter["Details"]},
                         {"_DeviceID", parameter["DeviceID"]},
                         {"_RegPlace",parameter["RegPlace"]},
                    };
            return Rep.ExecuteSqlCommand("sp_Create_DriverT", driver);
        }
        public bool UpdateDriver(Dictionary<string, string> parameter)
        {
            string dtexpired = Convert.ToDateTime(DateTime.ParseExact(parameter["DayExpiredLicense"], "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToString("yyyy/MM/dd");
            string dtcreate = Convert.ToDateTime(DateTime.ParseExact(parameter["DayCreateLicense"],
                                "dd/MM/yyyy", CultureInfo.InvariantCulture)).ToString("yyyy/MM/dd");
            Dictionary<string, string> driver =
                new Dictionary<string, string>
                    {
                         {"_NameDriver", parameter["NameDriver"]},
                         {"_PhoneDriver", parameter["PhoneDriver"]},
                         {"_IdCard", parameter["IdCard"]},
                         {"_DriverLicense", parameter["DriverLicense"]},
                         {"_Rank", parameter["Rank"]},
                         {"_DayExpiredLicense", dtexpired},
                         {"_Details", parameter["Details"]},
                         {"_DeviceID", parameter["DeviceID"]},
                         {"_DriverID", parameter["DriverID"]},
                         {"_DDID", parameter["DDID"]},
                         {"_DayCreateLicense",dtcreate},
                         {"_RegPlace",parameter["RegPlace"]},

                    };
            return Rep.ExecuteSqlCommand("sp_Update_DriverT", driver);
        }
        public bool DeleteDriver(Dictionary<string, string> parameter)
        {
            return Rep.ExecuteSqlCommand("sp_Delete_Driver", parameter);
        }

    }
}