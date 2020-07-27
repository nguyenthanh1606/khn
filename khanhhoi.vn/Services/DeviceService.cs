using khanhhoi.vn.ModelExt;
using khanhhoi.vn.Models;
using khanhhoi.vn.Repository;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;

namespace khanhhoi.vn.Services
{
    public class DeviceService
    {
        private Repository_khndb4 Repository;

        public DeviceService()
        {
            Repository = new Repository_khndb4();
        }

        public Device sp_getdevice_by_imei(String imei)
        {
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["imei_"] = imei;

            return Repository.ExecuteStoreProceduce<Device>("sp_getdevice_by_imei", param).FirstOrDefault();
        }

        public IList<Notify> GetDiviceExperedbyUserID(Dictionary<String, String> parameter)
        {
            IEnumerable<Device> dvExperied = Repository.ExecuteStoreProceduce<Device>("sp_GetDeviceExpired_byUserID", parameter);

            IList<Device> listDevice = new List<Device>();
            IList<Notify> listNotify = new List<Notify>();

            if (dvExperied == null) { return listNotify; }
            int count = 0;
            //try {
            //    foreach (var device in dvExperied)
            //    { listDevice.Add(device); }
            //} catch (Exception ex) { }

            foreach (var device in dvExperied)
            { listDevice.Add(device); }

            if (listDevice.Count != 0)
            {
                for (int i = 0; i < listDevice.Count; i++)
                {
                    Notify notify_temp = new Notify();
                    notify_temp.Content = listDevice[i].VehicleNumber;//Biển số
                    notify_temp.Date_temp = listDevice[i].DateCreate.Value.ToString("dd/MM/yyyy");//Ngày đăng ký
                    notify_temp.Title = listDevice[i].DateExpired.Value.ToString("dd/MM/yyyy");//Ngày hết hạn
                    notify_temp.NotifyID = (i + 1);//STT
                    double deltaDate = listDevice[i].DateExpired.Value.Date.Subtract(DateTime.Now.Date).TotalDays;
                    if (deltaDate > 0)
                    {
                        notify_temp.ghichu = "<=" + deltaDate;
                        notify_temp.state = 0;
                    }
                    else
                    {
                        notify_temp.ghichu = "Hết hạn dịch vụ";
                        notify_temp.state = 1;
                        count += 1;
                    }
                    listNotify.Add(notify_temp);
                }
            }

            return listNotify;
        }

        public bool sp_insert_tbldozy(Dictionary<String, String> param)
        {
            return Repository.ExecuteSqlCommand("sp_insert_tbldozy", param);
        }

        //sp_get_tbldozy_by_deviceid_date
        public IList<tbldozy> sp_get_tbldozy_by_deviceid_date(Dictionary<String, String> param)
        {
            return Repository.ExecuteStoreProceduce<tbldozy>("sp_get_tbldozy_by_deviceid_date", param).ToList();
        }

        public IList<Device> getAllDevice_ofUser(Dictionary<string, string> parameter)
        {
            IList<Device> data = Repository.ExecuteStoreProceduce<Device>("sp_getAllDevice_ofUser", parameter).ToList();
            return data;
        }

        public static double CalculateDistanceForDriver(IList<DataForDriver> list, string thedriver)
        {
            double result2 = 0;
            int start = 0;
            //  int end = 0;
            if (list != null)
            {
                //      GpsDataExt tmp = list.FirstOrDefault(m => m.Latitude > 0 && m.Longitude > 0);
                double sLatitude = 0.0;
                double sLongitude = 0.0;

                sLatitude = Convert.ToDouble(list[start].Latitude);
                sLongitude = Convert.ToDouble(list[start].Longitude);

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].TheDriver == thedriver)
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
                    else
                    {
                        sLatitude = Convert.ToDouble(list[i].Latitude.Value);
                        sLongitude = Convert.ToDouble(list[i].Longitude.Value);
                    }
                }
            }

            return Math.Round(result2 / 1000);
        }

        public static double CalculateDistanceForZone(DeviceStatus DeviceStatus)
        {
            var sCood = new GeoCoordinate(Convert.ToDouble(DeviceStatus.Latitude), Convert.ToDouble(DeviceStatus.Longitude));
            var eCood = new GeoCoordinate(Convert.ToDouble(DeviceStatus.ZoneLat), Convert.ToDouble(DeviceStatus.ZoneLng));
            return sCood.GetDistanceTo(eCood);
        }

        public dynamic GetDriver_byDeviceID(Dictionary<string, string> parameter)
        {
            IEnumerable<Driver> data = Repository.ExecuteStoreProceduce<Driver>("sp_GetDriver_byDeviceID", parameter);
            return data;
        }

        public dynamic GetDeviceby_UserID(Dictionary<string, string> parameter) // private -> public
        {
            IEnumerable<DeviceUser> data = Repository.ExecuteStoreProceduce<DeviceUser>("sp_GetDevice_byUserIDT", parameter);
            return data;
        }

        public dynamic GetDeviceby_UserIDT(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceUser> data = Repository.ExecuteStoreProceduce<DeviceUser>("sp_GetDevice_byUserIDT", parameter);
            IList<DeviceUser> datars = new List<DeviceUser>();
            foreach (var device in data)
            {
                if (device.ParentGroupID == null || device.ParentGroupID == 0)
                {
                    device.ParentGroupID = device.VehicleGroupID;
                    device.VehicleGroupIDChild = 0;
                }
                else
                {
                    device.VehicleGroupIDChild = device.VehicleGroupID;
                }
                device.CurrentParentID = device.ParentGroupID;
                device.CurrentChildID = device.VehicleGroupIDChild;
                datars.Add(device);
            }

            return datars;
        }

        private dynamic GetDeviceIssueby_UserID(Dictionary<string, string> parameter)
        {
            //_UserID
            IEnumerable<Issue> data = Repository.ExecuteStoreProceduce<Issue>("sp_GetDeviceIssue_byUserID", parameter);
            return data;
        }

        private dynamic GetDriverby_UserIDT(Dictionary<string, string> parameter)
        {
            //_UserID
            IEnumerable<Driver> data = Repository.ExecuteStoreProceduce<Driver>("sp_getDriver_byUserID", parameter);
            IList<Driver> datars = new List<Driver>();
            foreach (var device in data)
            {
                datars.Add(device);
            }

            return datars;
        }

        private IList<DeviceStatus> GetDeviceLastTimeUser(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceStatus> data =
                Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetDeviceLastTimeUser_byGroup", parameter);
            if (data.FirstOrDefault() == null)
            {
                return null;
            }
            IList<DeviceStatus> a = data.ToList();
            return data.ToList();
        }

        private IList<DeviceStatus> GetAll_DeviceLastTimeUser(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceStatus> data =
                Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetDeviceLastTimeUser22", parameter);
            if (data.FirstOrDefault() == null)
            {
                return null;
            }
            return data.ToList();
        }

        public IList<DeviceStatus> GetDeviceLastTimeByID(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceStatus> data =
                Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetDeviceLastTimeByDeviceID", parameter);
            if (data.FirstOrDefault() == null)
            {
                return null;
            }
            return data.ToList();
        }

        public dynamic getDataBySwitch(string variable)
        {
            Dictionary<string, string> parameter = null;
            if (variable != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable);
            }
            IEnumerable<DeviceStatus> data = Repository.ExecuteStoreProceduce<DeviceStatus>("sp_getDataBySwitch", parameter);
            return data;
        }

        public dynamic getDataFirst(string variable)
        {
            Dictionary<string, string> parameter = null;
            if (variable != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable);
            }
            IEnumerable<DeviceStatus> data = Repository.ExecuteStoreProceduce<DeviceStatus>("sp_getDataFirst", parameter);
            if (data.FirstOrDefault() != null)
            {
                return data;
            }
            return null;
        }

        public DeviceStatus getDataFirstT(Dictionary<string, string> parameter)
        {
            IEnumerable<DeviceStatus> data = Repository.ExecuteStoreProceduce<DeviceStatus>("sp_getDataFirstT", parameter);

            if (data.FirstOrDefault() != null)
            {
                return data.FirstOrDefault();
            }

            return null;
        }

        //sp_get_tbldata_by_deviceid //GpsData
        public GpsData sp_get_tbldata_by_deviceid(Dictionary<string, string> parameter)
        {
            GpsData data = Repository.ExecuteStoreProceduce<GpsData>("sp_get_tbldata_by_deviceid", parameter).FirstOrDefault();
            return data;
        }

        public dynamic getData_truocdo(string variable)
        {
            Dictionary<string, string> parameter = null;
            if (variable != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable);
            }
            DeviceStatus data = Repository.ExecuteStoreProceduce<DeviceStatus>("getDevicebyImeiLasttime_truocdo",
                                                                               parameter).FirstOrDefault();
            return data;
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
                    //foreach (var var in countVehicle)
                    //{
                    //    item.SL += var.SL.Value;
                    //}

                    item.id = node + "/" + item.VehicleGroupID;
                    item.text = item.VehicleGroup + " (" + item.SL + " xe)";
                    if (lang == "en")
                    {
                        item.text = item.VehicleGroup + " (" + item.SL + " vehicle)";
                    }
                    // item.leaf = "false";
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
                //   param.Add("_GroupID", groupID);

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
                foreach (var s in datatemp)
                {
                    s.leaf = "true";
                    data.Add(s);
                }
            }

            if (data != null)
            {
                double subtract_Result = 0;
                int count = 0;
                for (int i = 0; i < data.Count; i++)
                {
                    if (data[i].leaf == "false")
                    {
                        data[i].STT = "";
                        continue;
                    }

                    data[i] = getStatus_ofDevice(data[i], roleID, node);
                    //if (userid.Equals("25106")) {
                    //    if (data[i].color.Equals("red")) {
                    //        data[i].color = "black";
                    //    }
                    //}
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

                    //if (data[i].Radius < CalculateDistanceForZone(data[i]))
                    //{
                    //    data[i].Status = data[i].Status + " + Ra khỏi khoanh vùng";
                    //}

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
                }
            }
            //     data[0].VehicleNumber = (DateTime.Now.Ticks - c) / 100000 + "ms";
            return data;
        }

        public DeviceStatus getValueOil_Current(DeviceStatus data_model)
        {
            // sp_tblinforcaloil_by_vehiclenumber _vehiclenumber
            //sp_getdata_for_Oil _deviceID
            DeviceStatus obj_ = new DeviceStatus();

            obj_.OilValue = 0;
            obj_.FuelCapacity = 0;
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("_vehiclenumber", data_model.VehicleNumber);
            OilInfomation OilInfomation_ =
                Repository.ExecuteStoreProceduce<OilInfomation>("sp_tblinforcaloil_by_vehiclenumber", parameter).FirstOrDefault();
            if (OilInfomation_ != null)
            {
                parameter = new Dictionary<string, string>();
                parameter["_deviceID"] = data_model.DeviceID.ToString(); ;
                IList<GpsdataExtForOil> List_GpsdataExtForOil_ =
                Repository.ExecuteStoreProceduce<GpsdataExtForOil>("sp_getdata_for_Oil", parameter).ToList();
                if (List_GpsdataExtForOil_.Count > 0)
                {
                    List_GpsdataExtForOil_ = List_GpsdataExtForOil_.OrderByDescending(m => m.DateSave).Where(m => m.Oilvalue <= OilInfomation_.VoltMax).ToList();
                    for (int i = 0; i < List_GpsdataExtForOil_.Count; i++)
                    {
                        if (OilInfomation_.method_name == "Volt")
                        {
                            if (List_GpsdataExtForOil_[i].Oilvalue.Value <= OilInfomation_.VoltMax.Value)
                            {
                                if (data_model.Version.Equals("GT"))
                                {
                                    obj_.OilValue = (List_GpsdataExtForOil_[i].Oilvalue.Value * OilInfomation_.VolumeOilBarrel.Value) / 100;
                                }
                                else
                                {
                                    obj_.OilValue = (List_GpsdataExtForOil_[i].Oilvalue.Value * OilInfomation_.VolumeOilBarrel.Value) / OilInfomation_.VoltMax.Value;
                                }

                                break;
                            }
                        }
                        else if (OilInfomation_.method_name == "Litter")
                        {
                            //OilInfomation_.VolumeOilBarrel.Value
                            if ((List_GpsdataExtForOil_[i].Oilvalue.Value / 10) <= OilInfomation_.VolumeOilBarrel.Value)
                            {
                                obj_.OilValue = List_GpsdataExtForOil_[i].Oilvalue.Value / 10;
                                break;
                            }
                        }
                    }

                    obj_.FuelCapacity = OilInfomation_.VolumeOilBarrel;
                }
            }
            return obj_;
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
                    }
                    else
                    {
                        data.KeyShow = data.StatusKey == 1 ? Resources.Language.lbmo : Resources.Language.lbtat;
                    }
                }
                else
                {
                    data.KeyShow = Resources.Language.lbmo;
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
                    data.DoorShow = "Đóng";
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
                                    data.color = "yellow";
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
                                temp += " (" + Resources.Language.lbquatocdo + ")";
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

                                data.Status = Resources.Language.lbnoidung +
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

                                    data.Status = Resources.Language.lbdung + ConverteTime(timeTemp);
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
                                        temp = temp + Resources.Language.lbcuamo;
                                        data.color = "yellow";
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

                                    data.Status = Resources.Language.lbdung + ConverteTime(timeTemp);
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

                                        data.Status = Resources.Language.lbdung + ConverteTime(timeTemp);
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
                                data.Status = Resources.Language.lbmatll_ +
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
                                    data.Status = Resources.Language.lbdung +
                                                  ConverteTime(timep);
                                    data.color = "black";
                                    if (timep > 15)
                                    {
                                        data.Status = data.Status.Replace(Resources.Language.lbdung, Resources.Language.lbdau);
                                    }
                                }
                                else
                                {
                                    data.Status = Resources.Language.lbmatgps +
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
                                data.color = "red";
                                data.status_id = 3;
                                if (timep > 2880)
                                    Add_Fail(data.DeviceID, data.VehicleNumber, data.Status, data.stringStatus);
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
                    data.status_id = 5;
                    data.Addr = "";
                    data.color = "red";
                }
            }

            return data;
        }

        public dynamic getStatus_1Device(Dictionary<string, string> param)
        {
            //string deviceID = param.FirstOrDefault(m => m.Key == "_DeviceID").Value;
            string roleID = param.FirstOrDefault(m => m.Key == "_RoleID").Value;
            Dictionary<string, string> parameter = new Dictionary<string, string>();
            parameter.Add("_UserID", param.FirstOrDefault(m => m.Key == "_UserID").Value);
            parameter.Add("_DeviceID", param.FirstOrDefault(m => m.Key == "_DeviceID").Value);
            IList<DeviceStatus> device = GetDeviceLastTimeByID(parameter);

            if (device.FirstOrDefault() != null)
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
                    st.DayExpiredLicense = driver.DayExpiredLicense;
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

                return st;
            }
            return null;
        }

        private IList<DataForDriver> getGetDeviceByImeiOnDay(string variable)
        {
            Dictionary<string, string> parameter = null;
            if (variable != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>(variable);
            }
            IList<DataForDriver> list = Repository.ExecuteStoreProceduce<DataForDriver>("sp_GetDeviceByImeiOnDay",
                                                                                        parameter).ToList();
            return list;
        }

        private string getLastTheDriverByImei(string deviceID)
        {
            Dictionary<string, string> parameter = null;
            if (deviceID != null)
            {
                parameter = JsonConvert.DeserializeObject<Dictionary<string, string>>("{'_DeviceID':'" + deviceID + "'}");
            }
            string number =
                Repository.ExecuteStoreProceduce<String>("sp_getLastDriver_byID", parameter).FirstOrDefault();
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
                Repository.ExecuteStoreProceduce<Driver>("sp_getDriverByPhoneDriver", parameter).FirstOrDefault();
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
                Repository.ExecuteStoreProceduce<Driver>("sp_getDriverFirstByDeviceID", parameter).FirstOrDefault();
            return driver;
        }

        public Device getDevivebyDeviceIDT(String _DeviceID)
        {
            Dictionary<String, String> parameter = new Dictionary<string, string>();
            parameter.Add("_DeviceID", _DeviceID);
            Device device = Repository.ExecuteStoreProceduce<Device>("sp_GetDevice_byDeviceIDT",
                                                                     parameter).First();
            return device;
        }

        public List<Device> GetDeviceByDeviceID(Dictionary<string, string> parameter)
        {
            IEnumerable<Device> data = Repository.ExecuteStoreProceduce<Device>("sp_GetDevice_ByDevice", parameter);
            return data.ToList();
        }

        //private dynamic getDriver(Dictionary<string, string> param) //param: arrray imei
        //{
        //    string variable = param["_IDs"];
        //    string[] listDeviceID = variable.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
        //    IList<Driver> listDriver = new List<Driver>();

        //    for (int i = 0; i < listDeviceID.Length; i++)
        //    {
        //        ReportService reportService = new ReportService();
        //        string number = "";
        //        number = getLastTheDriverByImei(listDeviceID[i]);
        //        Driver driver =
        //            getDriverByPhoneDriver("{'_DeviceID':'" + listDeviceID[i] + "','_PhoneDriver':'" + number + "'}") ??
        //            getDriverFirst("{'_DeviceID':'" + listDeviceID[i] + "'}");
        //        Dictionary<String,String> parameter=new Dictionary<string, string>();
        //        parameter.Add("IDs", listDeviceID[i]);
        //        parameter.Add("From", DateTime.Now.ToString("yy-MM-dd")+" 00:00");
        //        parameter.Add("To", DateTime.Now.ToString("yy-MM-dd"+" 23:59"));
        //        parameter.Add("Type", "TimeDriverGen");
        //        IList<TimeDriver> listTimeDriver=new List<TimeDriver>();
        //        if (!String.IsNullOrEmpty(number))
        //        {
        //            IList<TimeDriver> temp =reportService.TimeDriver(parameter).ToList();
        //            if (temp.Count > 0 && temp.Count<=3)
        //            {
        //                for (int j = 0; j < temp.Count; j++)
        //                {
        //                    listTimeDriver.Add(temp[j]);
        //                }

        //            }
        //            else if (temp.Count >= 0 && temp.Count > 3)
        //            {
        //                listTimeDriver = temp;
        //            }
        //        }
        //        else
        //        {
        //            listTimeDriver = reportService.TimeDriver(parameter).ToList();
        //        }

        //        IList<Open_Close> listOpenClose = reportService.ReportOpen_Close(parameter);
        //        IList<ExceedingSpeed> listQuaVt = reportService.ReportExceedingSpeed(parameter);
        //        IList<Distance> listDistance= reportService.ReportDistance(parameter);
        //        Dictionary<String, String> parameter_ = new Dictionary<string, string>();
        //        parameter_.Add("_DeviceID", listDeviceID[i]);
        //        double tg_lxtrongngay = 0, distane = 0;
        //        if (listDistance.Count > 0 && !String.IsNullOrEmpty(listDistance[0].Distances))
        //        {
        //            double distane_temp = double.Parse(listDistance[0].Distances.Replace("Km", ""));
        //            distane = distane_temp;
        //        }
        //        int speed = 0, speedlimit = 0, open_door = 0, slvuottoc=0;

        //        int solanlxlt = 0;
        //        DeviceStatus data = getDataFirstT(parameter_);
        //        speed= data==null ?0 : data.Speed.Value;
        //        speedlimit = data == null ? 0 : data.SpeedLimit;
        //        String warning = "";
        //        String ContinuousDriving = "0";
        //        //speed = data.Speed.Value;
        //        if (listTimeDriver != null&&listTimeDriver.Count>0 )
        //        {
        //            for (int j = 0; j < listTimeDriver.Count; j++)
        //            {
        //                tg_lxtrongngay += listTimeDriver[j].stimedriver;
        //                //distane += listTimeDriver[j].sDistance;
        //                if((listTimeDriver[j].stimedriver)>240)
        //                {
        //                    solanlxlt += 1;
        //                }
        //            }
        //           if(listTimeDriver[listTimeDriver.Count-1].stimedriver>240)
        //           {
        //               warning = "Lái xe liên tục quá 4h";
        //           }
        //           else if (tg_lxtrongngay > 600)
        //           {
        //               warning = "Lái xe quá 10h trong ngày";
        //           }
        //           else if (speed >= speedlimit)
        //           {
        //               warning = "Vượt vận tốc quy định";
        //           }
        //           else
        //           {
        //               warning = "Ổn định";
        //           }
        //            ContinuousDriving = listTimeDriver[listTimeDriver.Count - 1].TimeDriver_;
        //            for (int j = 0; j < listOpenClose.Count; j++)
        //            {
        //                if(!String.IsNullOrEmpty(listOpenClose[j].CoordinatesOpen))
        //                {
        //                    open_door += 1;
        //                }
        //            }
        //            for (int j = 0; j < listQuaVt.Count; j++)
        //            {
        //                if (!String.IsNullOrEmpty(listQuaVt[j].Coordinates))
        //                {
        //                    slvuottoc += 1;
        //                }
        //            }
        //        }
        //        if(driver==null)
        //        {
        //            driver = new Driver();
        //        }
        //        driver.ExceedingSpeed = slvuottoc;
        //        //driver.
        //        driver.OpenDoor = open_door;
        //        driver.Warning = warning;
        //        driver.Distace = Math.Round(distane) + " km";
        //        driver.ContinuousDrivingViolations = solanlxlt;
        //        driver.ContinuousDriving = ContinuousDriving;
        //        driver.DrivingInDay = ConverteTime(tg_lxtrongngay);
        //        listDriver.Add(driver);
        //        //}
        //    }
        //    return listDriver;
        //}

        public dynamic getDriver(Dictionary<string, string> param) //param: arrray imei//Nguyên bản
        {
            string variable = param["_IDs"];
            string[] listDeviceID = variable.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string, string> pramDungDo = new Dictionary<string, string>();
            DateTime now_ = new DateTime();
            now_ = DateTime.Now;
            string from = now_.ToString("yy-MM-dd 00:00");
            string to = now_.ToString("yy-MM-dd 23:59");
            pramDungDo["IDs"] = param["_IDs"];
            pramDungDo["From"] = from;
            pramDungDo["To"] = to;
            ReportService report = new ReportService();
            IList<PauseStop> list_dungdo = report.ReportPause_Stop(pramDungDo);
            IList<Driver> listDriver = new List<Driver>();

            for (int i = 0; i < listDeviceID.Length; i++)
            {
                string number = "";
                number = getLastTheDriverByImei(listDeviceID[i]);
                //if (number != "")
                //{
                Driver driver =
                    getDriverByPhoneDriver("{'_DeviceID':'" + listDeviceID[i] + "','_PhoneDriver':'" + number + "'}") ??
                    getDriverFirst("{'_DeviceID':'" + listDeviceID[i] + "'}");

                IList<DataForDriver> listData = getGetDeviceByImeiOnDay("{'_DeviceID':'" + listDeviceID[i] + "'}");

                if (listData != null)
                {
                    listData[0].Speed = 0;
                    if (driver != null)
                    {
                        foreach (var dataForDriver in listData)
                        {
                            if (dataForDriver.TheDriver == "" || dataForDriver.TheDriver == "000" || dataForDriver.TheDriver == number)
                            {
                                dataForDriver.TheDriver = driver.PhoneDriver;
                            }
                        }
                    }
                    else
                    {
                        driver = new Driver();
                        driver.PhoneDriver = number;
                        driver.Switch_Door = 1;
                    }
                    driver.ContinuousDriving = "";
                    driver.Warning = "";
                    driver.DeviceID = Int16.Parse(listDeviceID[i]);
                    int switch_door_open = driver.Switch_Door == 1 ? 0 : 1;
                    //TaiXe obj = new TaiXe();
                    DateTime start = DateTime.Now;
                    DateTime ketthucTruocdo = DateTime.Now;
                    DateTime end = DateTime.Now;

                    int slQuavt = 0;
                    int slmocua = 0;
                    int sllaixeLT = 0;
                    double tg_lxtrongngay = 0;
                    int vp_lxlientuc = 0;
                    bool flag = true;
                    bool flag2 = false;
                    bool f = false;
                    bool f_door = false;
                    bool f_laixe = false;
                    bool f_lxtl = false;
                    string thedriver = driver.PhoneDriver;
                    double Distance = 0;
                    int SpeedLimit = listData.FirstOrDefault().SpeedLimit.Value;
                    DateTime tempstar = new DateTime();
                    for (int j = 0; j < listData.Count; j++)
                    {
                        if (!listData[j].TheDriver.Equals(thedriver))
                        {
                            continue;
                        }
                        if (listData[j].Speed > SpeedLimit)
                        {
                            if (flag)
                            {
                                tempstar = listData[j].DateSave.Value;

                                flag = false;
                                flag2 = false;
                            }
                        }
                        else if (listData[j].Speed <= SpeedLimit)
                        {
                            if (!flag)
                            {
                                if (listData[j].DateSave.Value.Subtract(tempstar).TotalSeconds > 60)
                                {
                                    slQuavt += 1;
                                    flag2 = true;
                                }

                                flag = true;
                            }
                        }

                        if (!f_door)
                        {
                            if (listData[j].StatusDoor.Equals(switch_door_open))
                            {
                                f_door = true;
                            }
                        }
                        else
                        {
                            if (!listData[j].StatusDoor.Equals(switch_door_open))
                            {
                                if (listData[j].Speed < 30)
                                {
                                    slmocua += 1;
                                }
                                f_door = false;
                            }
                        }

                        if (!f_laixe)
                        {
                            if (listData[j].Speed > 0 && listData[j].DateSave.Value.Subtract(listData[j - 1].DateSave.Value).TotalMinutes > 15)
                            {
                                f_laixe = true;
                            }
                            if (listData[j].Speed > 0)
                            {
                                //    thedriver = listData[j].TheDriver;
                                end = listData[j].DateSave.Value;
                                f_laixe = true;
                                //f_lxtl = false;
                            }
                        }
                        else
                        {
                            if (listData[j].TheDriver.Equals(thedriver) || string.IsNullOrEmpty(thedriver))
                            {
                                if (listData[j].Speed == 0 || j == listData.Count - 1)
                                {
                                    start = listData[j - 1].DateSave.Value;
                                    double timetemp = start.Subtract(end).TotalMinutes;

                                    if (driver.ContinuousDrivingTemp > 0 && end.Subtract(ketthucTruocdo).TotalMinutes <= 15)
                                    {
                                        driver.ContinuousDrivingTemp += timetemp;
                                        driver.ContinuousDriving = ConverteTime(driver.ContinuousDrivingTemp);
                                    }
                                    else
                                    {
                                        driver.ContinuousDriving = ConverteTime(timetemp);
                                        driver.ContinuousDrivingTemp = timetemp;
                                    }

                                    //if (!f_lxtl)
                                    //{
                                    driver.Warning = timetemp > 240 ? "Lái xe liên tục quá 4h!" : "Ổn định";
                                    //    f_lxtl = true;
                                    //}

                                    tg_lxtrongngay += timetemp;
                                    if (driver.ContinuousDrivingTemp > 240)
                                    {
                                        vp_lxlientuc += 1;
                                        driver.Warning = "Lái xe liên tục quá 4h!";
                                        driver.ContinuousDrivingTemp = 0;
                                    }
                                    f_laixe = false;
                                    for (int k = j; k < listData.Count; k++)
                                    {
                                        if (listData[k].Speed > 0)
                                        {
                                            double timeTemp = listData[k].DateSave.Value.Subtract(listData[j].DateSave.Value).TotalMinutes;
                                            if (timeTemp <= 15)
                                            {
                                                tg_lxtrongngay += timeTemp;
                                            }
                                            break;
                                        }
                                    }
                                    ketthucTruocdo = listData[j].DateSave.Value;
                                }
                            }
                        }
                    }
                    if (tg_lxtrongngay > 600)
                    {
                        driver.Warning = "Lái xe quá 10h trong ngày!";
                    }

                    //driver.Warning
                    driver.Distace = CalculateDistanceForDriver(listData, thedriver) + " km";

                    driver.ContinuousDrivingViolations = vp_lxlientuc;
                    driver.OpenDoor = slmocua;
                    driver.ExceedingSpeed = slQuavt;
                    driver.DrivingInDay = ConverteTime(tg_lxtrongngay);
                    driver.DungDo = list_dungdo.Count.ToString() + " lần";
                    Device dvtemp = getDevivebyDeviceIDT(listDeviceID[i]);
                    {
                        if (dvtemp != null && dvtemp.Door != 1)
                        {
                            driver.OpenDoor = 0;
                        }
                    }

                    listDriver.Add(driver);
                }
                //  }
            }
            return listDriver;
        }

        public DeviceStatus GetExpired(Dictionary<string, string> param)
        {
            //DateTime DateExpired;
            //string timestemp="";
            DeviceStatus timestemp =
                Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetDateExpired_byDeviceID", param).FirstOrDefault();
            return timestemp;
        }

        public VehicleCategorys getVehicle_by_DeviceID(Dictionary<string, string> param)
        {
            //DateTime DateExpired;
            //string timestemp="";
            VehicleCategorys vehicle =
                Repository.ExecuteStoreProceduce<VehicleCategorys>("sp_GetVehicleG_by_DeviceID", param).FirstOrDefault();
            return vehicle;
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
                parameter2.Add("_DeviceID", param.FirstOrDefault(p => p.Key == "_DeviceID").Value);
            }

            //DateTime DateExpired;
            //string timestemp="";
            IEnumerable<DeviceStatus> timestemp =
                Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetDateExpired_byDeviceID", parameter2);
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
                                list = Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetLotrinh_FromTo",
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
                                    Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetLotrinh_FromTo",
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

        private IList<DeviceStatus> checkPoint(IList<DeviceStatus> list)
        {
            IList<DeviceStatus> list_delete = new List<DeviceStatus>();
            IList<DeviceStatus> list_result = new List<DeviceStatus>();
            if (list.Count > 0)
            {
                for (int i = 0; i < list.Count - 1; i++)
                {
                    if (list[i].Speed.Value > 0)
                    {
                        if (list[i + 1].Speed.Value > 0)
                        {
                            var sCood = new GeoCoordinate(double.Parse(list[i].Latitude.Value.ToString()), double.Parse(list[i].Longitude.Value.ToString()));
                            var eCood = new GeoCoordinate(double.Parse(list[i + 1].Latitude.Value.ToString()), double.Parse(list[i + 1].Longitude.Value.ToString()));

                            double distanebyCoor = sCood.GetDistanceTo(eCood);
                            double deltaT = (list[i + 1].DateSave.Value.Subtract(list[i].DateSave.Value).TotalSeconds) / 3600;
                            double deltaV = ((list[i].Speed.Value + list[i].Speed.Value) / 2) * 1000;
                            double distanebyTime = deltaT * deltaV;
                            double resultCompare = (distanebyCoor - distanebyTime) < 0 ? (distanebyCoor - distanebyTime) * -1 : (distanebyCoor - distanebyTime);
                            if (resultCompare > 100)
                            {
                                list_delete.Add(list[i + 1]);
                            }
                        }
                    }
                }
                foreach (var deviceStatuse in list_delete)
                {
                    list.Remove(deviceStatuse);
                }
                list_result = list;
            }
            return list_result;
        }

        private dynamic GetVehicleGroup(Dictionary<string, string> param)
        {
            IList<VehicleGroups> list =
                Repository.ExecuteStoreProceduce<VehicleGroups>("sp_GetbyUserID_DeviceGroupT", param).ToList();
            return list;
        }

        private dynamic GetVehicleGroupForRp(Dictionary<string, string> param)
        {
            IList<VehicleGroups> list =
                Repository.ExecuteStoreProceduce<VehicleGroups>("sp_GetbyUserID_DeviceGroup", param).ToList();
            return list;
        }

        private dynamic GetVehicleGroupParent(Dictionary<string, string> param)
        {
            IList<DeviceStatus> list =
                Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetbyUserID_DeviceGroupChild", param).ToList();
            return list;
        }

        private dynamic GetAllVehicleGroup_byGroupParID(Dictionary<string, string> param)
        {
            IList<VehicleGroups> list =
                Repository.ExecuteStoreProceduce<VehicleGroups>("sp_GetAll_VehicleGroup_byGroupParID", param).ToList();
            return list;
        }

        private dynamic GetDevicebyGroup(Dictionary<string, string> param)
        {
            IList<Device> list =
                Repository.ExecuteStoreProceduce<Device>("sp_getDevice_byGroupID2", param).ToList();
            return list;
        }

        private Users GetUser_byLoginName(string loginname)
        {
            Dictionary<string, string> param =
                new Dictionary<string, string>
                    {
                        {"_LoginName", loginname}
                    };
            return Repository.ExecuteStoreProceduce<Users>("sp_GetUserByLoginName", param).FirstOrDefault();
        }

        public bool AddUser_Device(Dictionary<string, string> param)
        {
            string lstDeviceID = param.SingleOrDefault(p => p.Key == "_ListDeviceID").Value;

            Dictionary<string, string> user =
                new Dictionary<string, string>
                    {
                        {"_LoginName", param["_LoginName"]},
                        {"_Password_", param["_Password_"]},
                        {"_FullName", param["_FullName"]},
                        {"_Email", param["_Email"]},
                        {"_PhoneNumber", param["_PhoneNumber"]},
                        {"_Address", param["_Address"]},
                        {"_CreateDate", DateTime.Now.ToString("yyyy/MM/dd")},
                        {"_EndDate", DateTime.Now.AddYears(1).ToString("yyyy/MM/dd")},
                        {"_IsActive", "1"},
                        {"_Details", param["_Detail"]},
                        {"_WhoCreateID", param["_WhoCreateID"]},
                        {"_RoleID", "4"},
                    };
            if (Repository.ExecuteSqlCommand("sp_CreateUser", user))
            {
                int userID = GetUser_byLoginName(param["_LoginName"]).UserID;
                string[] deviceId = lstDeviceID.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < deviceId.Length; i++)
                {
                    Dictionary<string, string> user_device =
                        new Dictionary<string, string>
                            {
                                {"_UserID", userID.ToString()},
                                {"_DeviceID", deviceId[i]}
                            };
                    if (!Repository.ExecuteSqlCommand("sp_Create_UserDevice", user_device))
                        return false;
                }
                return true;
            }
            return false;
        }

        public bool EditUser_Device(Dictionary<string, string> param)
        {
            string userID = param["_UserID"];
            Dictionary<string, string> user =
                new Dictionary<string, string>
                    {
                        {"UserID",userID},
                        {"_RoleID", "4"},
                        {"_Password_", param["_Password_"]},
                        {"_FullName", param["_FullName"]},
                        {"_Email", param["_Email"]},
                        {"_PhoneNumber", param["_PhoneNumber"]},
                        {"_Address", param["_Address"]},
                        {"_EndDate", DateTime.Now.AddYears(1).ToString("yyyy/MM/dd")},
                        {"_IsActive", "1"},
                        {"_Details", param["_Detail"]},
                        {"_WhoCreateID", param["_WhoCreateID"]},
                    };
            if (Repository.ExecuteSqlCommand("sp_Update_User_main", user))
            {
                try
                {
                    string[] selected = param["_Selected"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < selected.Length; i++)
                    {
                        Dictionary<string, string> user_device = new Dictionary<string, string>{
                            {"_UserID", userID},
                            {"_DeviceID", selected[i]}
                        };
                        Repository.ExecuteSqlCommand("sp_Update_Selected_UD", user_device);
                    }
                }
                catch (Exception ex) { }
                try
                {
                    string[] unselect = param["_Unselect"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < unselect.Length; j++)
                    {
                        Dictionary<string, string> user_device = new Dictionary<string, string>
                        {
                            {"_UserID", userID},
                            {"_DeviceID", unselect[j]}
                        };
                        Repository.ExecuteSqlCommand("sp_Update_Unselect_UD", user_device);
                    }
                }
                catch (Exception ex) { }

                return true;
            }
            return false;
        }

        public bool DeleteUser_Device(Dictionary<string, string> param)
        {
            return Repository.ExecuteSqlCommand("sp_DeleteUser_Device", param);
        }

        private bool UpdateAddress(Dictionary<string, string> param)
        {
            Dictionary<string, string> addr = new Dictionary<string, string>
                        {
                            {"_AddressID",  param["_AddressID"]},
                            {"_Addr", param["_Addr"].Replace("'","")}
                        };
            return Repository.ExecuteSqlCommand("sp_UpdateAddr", addr);
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
            return Repository.ExecuteSqlCommand("sp_Add_Fail", parameter);
        }

        public dynamic GetAllVehicleGroup_byUserID(Dictionary<string, string> param)
        {
            IList<VehicleGroups> list = Repository.ExecuteStoreProceduce<VehicleGroups>("sp_GetAll_VehicleGroup_byUserIDT", param).ToList();
            return list;
        }

        public dynamic GetAll_VehicleCategory()
        {
            IEnumerable<VehicleCategorys> data = Repository.ExecuteStoreProceduce<VehicleCategorys>("sp_GetAll_VehicleCategory");
            return data;
        }

        public dynamic GetAll_Province()
        {
            IEnumerable<Province> data = Repository.ExecuteStoreProceduce<Province>("sp_GetAll_Province");
            return data;
        }

        public dynamic GetAll_TypeTransport()
        {
            IEnumerable<TypeTransport> data = Repository.ExecuteStoreProceduce<TypeTransport>("sp_GetAll_TypeTranport");
            return data;
        }
    }
}