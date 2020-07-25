using khanhhoi.vn.Models;
using khanhhoi.vn.Models.Ext;
using khanhhoi.vn.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using khanhhoi.vn.ModelExt;

namespace khanhhoi.vn.Controllers
{
    public class HomeController : Controller
    {
        Services_khndb4 user_sv;
        ReportService report_sv;
        DeviceService dv_sv;
        //UserService user_service;
        public List<ModelLanguage> listLang;
        //  Services_khndb4_backup user_sv_back = new Services_khndb4_backup();
        //  int i_result = 0;
        //  DeviceService dv_sv = new DeviceService();
        // ReportService report_sv = new ReportService();
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        public void setValueLang()
        {
            listLang = new List<ModelLanguage>();
            ModelLanguage lang = new ModelLanguage();
            lang.ID = "vi";
            lang.Name = "Việt Nam";
            listLang.Add(lang);
            lang = new ModelLanguage();
            lang.ID = "en";
            lang.Name = "English";
            listLang.Add(lang);
            lang = new ModelLanguage();
            lang.ID = "km";
            lang.Name = "Cambodia";
            listLang.Add(lang);
        }
        public ActionResult Change(String languageFlag)
        {
            if (languageFlag != null)
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(languageFlag);
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageFlag);
            }
            HttpCookie cookie = Request.Cookies["lang"];
            if (cookie == null)
            {
                cookie = new HttpCookie("lang");
                cookie.Value = languageFlag;
            }
            else
            {
                cookie.Value = languageFlag;
            }
            cookie.Expires = DateTime.UtcNow.AddDays(30);
            Response.Cookies.Add(cookie);

            setValueLang();
            ViewBag.listLang = listLang;
            return RedirectToAction("Login", "Home");
        }
        public ActionResult Login()
        {
            setValueLang();
            ViewBag.listLang = listLang;
            Session["userName"] = Request.Cookies["userName"] == null ? "" : Request.Cookies["userName"].Value;
            Session["pass"] = Request.Cookies["pass"] == null ? "" : Request.Cookies["pass"].Value;
            if (Request.Cookies["userName"] != null && Request.Cookies["pass"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View("Login");
        }
        public ActionResult ValidateUser(FormCollection from)
        {
            if (Request.Cookies["userName"] != null && Request.Cookies["pass"] != null)
            {
                return RedirectToAction("Index", "Home"); 
            }
            String username_ = from["username"];
            String password_ = from["password"];
            String ckRemember = from["ckRemember"];
            if (!String.IsNullOrEmpty(username_) || !String.IsNullOrEmpty(password_))
            {
                user_sv = new Services_khndb4();
                // user_sv = new Services_khndb4();
                IEnumerable<UserRole> IEnumuseritem = (IEnumerable<UserRole>)user_sv.ValidateUser(username_, password_);
                UserRole useritem = IEnumuseritem.FirstOrDefault();
                if (useritem != null && useritem.RoleID != 1 && useritem.RoleID != 2)
                {
                    FormsAuthentication.SetAuthCookie(useritem.LoginName, true);
                    if (!String.IsNullOrEmpty(ckRemember))
                    {
                        addCookie(useritem, true);
                    }
                    else
                    {
                        addCookie(useritem, false);
                    }
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    Session["statusLogin"] = "0";
                    return RedirectToAction("Login");
                }
            }
            else
            {
                Session["statusLogin"] = "0";
                return RedirectToAction("Login");
            }
        }
        public void addCookie(Users firstOrDefault, bool remember)
        {
            Response.Cookies.Add(new HttpCookie("fullName")
            {
                Value = firstOrDefault.FullName.ToString(),
                Expires = DateTime.Now.AddMinutes(90)

            });
            Response.Cookies.Add(new HttpCookie("userName")
            {
                Value = firstOrDefault.LoginName.ToString(),
                Expires = DateTime.Now.AddMinutes(90)

            });
            Response.Cookies.Add(new HttpCookie("pass")
            {
                Value = firstOrDefault.Password_.ToString(),
                Expires = DateTime.Now.AddMinutes(90)

            });
            Response.Cookies.Add(new HttpCookie("userid")
            {
                Value = firstOrDefault.UserID.ToString(),
                Expires = DateTime.Now.AddMinutes(90)

            });
            Response.Cookies.Add(new HttpCookie("userid_provider")
            {
                Value = firstOrDefault.WhoCreateID.Value.ToString(),
                Expires = DateTime.Now.AddMinutes(90)
            });
            if (remember)
            {
                Response.Cookies.Add(new HttpCookie("userName_log")
                {
                    Value = firstOrDefault.LoginName.ToString(),
                    Expires = DateTime.Now.AddDays(30)

                });
                Response.Cookies.Add(new HttpCookie("pass_log")
                {
                    Value = firstOrDefault.Password_.ToString(),
                    Expires = DateTime.Now.AddDays(30)

                });
            }
            else
            {
                if (Request.Cookies["userName_log"] != null)
                {
                    HttpCookie myCookie = new HttpCookie("userName_log");
                    myCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(myCookie);
                }
                if (Request.Cookies["pass_log"] != null)
                {
                    HttpCookie myCookie = new HttpCookie("pass_log");
                    myCookie.Expires = DateTime.Now.AddDays(-1d);
                    Response.Cookies.Add(myCookie);
                }
            }
        }
        public ActionResult GetDiviceExperedbyUserID()
        {
            var jsonResult = Json("0000", JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            if (Request.Cookies["userid"] == null)
            {
                return jsonResult;
            }
            dv_sv = new DeviceService();
            Dictionary<String, String> param_ = new Dictionary<string, string>();
            param_["_UserID"] = (String)Request.Cookies["userid"].Value;
            List<Notify> list = dv_sv.GetDiviceExperedbyUserID(param_).ToList();
            jsonResult = Json(list, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }
        public ActionResult Report(int indexReport, int DeviceID, String dateform, String dateto)
        {
            if (Request.Cookies["userid"] == null)
            {
                return Json("0001", JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (indexReport == 2)//thong tin xe
                {
                    //getAllDevice_ofUser Device
                    Dictionary<String, String> param_ = new Dictionary<string, string>();
                    param_["_UserID"] = (String)Request.Cookies["userid"].Value;
                    IList<Device> listdevicebyUser = (new DeviceService()).getAllDevice_ofUser(param_);
                    return Json(listdevicebyUser, JsonRequestBehavior.AllowGet);
                }
                Dictionary<String, String> param = new Dictionary<string, string>(); //_DeviceID, To, From
                DateTime from_lotrinh = new DateTime();
                DateTime to_lotrinh = new DateTime();
                DateTime from = new DateTime();
                DateTime to = new DateTime();

                if (indexReport == 1)
                {

                    string dateform_Component = dateform.Replace("T", " ");
                    string dateto_Component = dateto.Replace("T", " ");

                    DateTime test_01 = DateTime.ParseExact(dateform_Component, "dd-MM-yyyy HH:mm:ss", null);
                    DateTime test_02 = DateTime.ParseExact(dateto_Component, "dd-MM-yyyy HH:mm:ss", null);

                    //from_lotrinh = DateTime.ParseExact(test, "dd-MM-yyyy HH:mm:ss", null);
                    //to_lotrinh = DateTime.ParseExact(dateto_Component, "dd-MM-yyyy HH:mm:ss", null);
                    from_lotrinh = test_01;
                    to_lotrinh= test_02;

                    //from_lotrinh = DateTime.Parse(_dateform);
                    //to_lotrinh = DateTime.Parse(dateto);
                    param["_DeviceID"] = DeviceID.ToString();
                    param["From"] = from_lotrinh.ToString("yy-MM-dd HH:mm");
                    param["To"] = to_lotrinh.ToString("yy-MM-dd HH:mm");
                }
                else
                {
                    from = DateTime.ParseExact(dateform, "dd/MM/yyyy", null);
                    to = DateTime.ParseExact(dateto, "dd/MM/yyyy", null);
                    param["_DeviceID"] = DeviceID.ToString();
                    param["From"] = from.ToString("yy-MM-dd 00:00");
                    param["To"] = to.ToString("yy-MM-dd 23:59");
                }

                String htmlJson = "";
                LotrinhItem lotrinhItem = new LotrinhItem();

                var jsonResult = Json("", JsonRequestBehavior.AllowGet);
                switch (indexReport)
                {
                    case 1: // lộ trình
                        report_sv = new ReportService();
                        IList<PauseStop> list_PauseStop = report_sv.ReportPause_Stop(param);
                        IList<Distance> listDistane1 = report_sv.ReportDistance(param);
                        user_sv = new Services_khndb4();
                        IList<DeviceStatus> listrs = user_sv.LoTrinh(param);
                        if (listrs != null)
                        {
                            if (listrs.Count > 0)
                            {
                                if (listrs.Count > 2)
                                {
                                    IList<DeviceStatus> listrs_filter = listrs.Where(m => (m.Latitude > 0 || m.Longitude > 0)).ToList().OrderBy(n => n.DataID).ToList();
                                    DateTime dateStart = listrs_filter[0].DateSave.Value;
                                    listrs = new List<DeviceStatus>();
                                    for (int k = 1; k < listrs_filter.Count(); k++)
                                    {
                                        if (listrs_filter[k].DateSave.Value > dateStart)
                                        {
                                            listrs.Add(listrs_filter[k]);
                                            dateStart = listrs_filter[k].DateSave.Value;
                                        }
                                    }
                                }
                                lotrinhItem.Count = listrs.Count();
                                lotrinhItem.VehicleNumber = listrs[0].VehicleNumber;
                                lotrinhItem.DeviceID = listrs[0].DeviceID;
                                lotrinhItem.VehicleCategoryID = listrs[0].VehicleCategoryID; // loại xe => hình xe
                            }
                            else
                            {
                                param = new Dictionary<string, string>();
                                param["_DeviceID"] = DeviceID.ToString(); ;
                                Device deviceItem = (new DeviceService()).GetDeviceByDeviceID(param).FirstOrDefault();
                                lotrinhItem.VehicleNumber = deviceItem.VehicleNumber;
                                lotrinhItem.DeviceID = deviceItem.DeviceID;
                            }

                            lotrinhItem.From = from_lotrinh.ToString("dd-MM-yyyy HH:mm");
                            lotrinhItem.To = to_lotrinh.ToString("dd-MM-yyyy HH:mm");
                            lotrinhItem.Date = from_lotrinh.ToString("dd-MM-yyyy");
                        }

                        jsonResult.MaxJsonLength = Int16.MaxValue;
                        lotrinhItem.listData = listrs != null ? listrs : (listrs = new List<DeviceStatus>());
                        lotrinhItem.Html_str = htmlJson;
                        lotrinhItem.listData_PauseStop = list_PauseStop;
                        lotrinhItem.listDistane1 = listDistane1.ToList();
                        jsonResult = Json(lotrinhItem, JsonRequestBehavior.AllowGet);
                        break;
                    case 3: // chưa dùng
                        report_sv = new ReportService();
                        IList<Distance> listDistane = report_sv.ReportDistance(param);
                        jsonResult = Json(listDistane, JsonRequestBehavior.AllowGet);
                        break;
                    case 5: // chưa dùng
                        report_sv = new ReportService();
                        List<BaoCaoHanhTrinh_Detail> ListBaoCaoHanhTrinh_Detail = report_sv.ReportHanhTrinh_Detail(param);
                        jsonResult = Json(ListBaoCaoHanhTrinh_Detail, JsonRequestBehavior.AllowGet);
                        break;
                    case 7: // chưa dùng
                        report_sv = new ReportService();
                        param["type"] = "on";
                        IList<On_Off> list_On_ = report_sv.ReportOn_Off(param);
                        if (list_On_ != null)
                        {
                            if (list_On_.Count > 0)
                            {
                                jsonResult = Json(list_On_, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                jsonResult = Json("", JsonRequestBehavior.AllowGet);
                            }
                        }
                        break;
                    case 8: // chưa dùng
                        report_sv = new ReportService();
                        param["type"] = "off";
                        IList<On_Off> list_off_ = report_sv.ReportOn_Off(param);
                        if (list_off_ != null)
                        {
                            if (list_off_.Count > 0)
                            {
                                jsonResult = Json(list_off_, JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                jsonResult = Json("", JsonRequestBehavior.AllowGet);
                            }
                        }
                        break;
                    case 9: // chưa dùng
                        report_sv = new ReportService();
                        IList<Fuel> list_Fuel = report_sv.ReportFuel_LineChart(param);
                        if (list_Fuel.Count > 0)
                        {
                            jsonResult = Json(list_Fuel, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            jsonResult = Json("", JsonRequestBehavior.AllowGet);
                        }
                        break;
                    case 11: // báo cáo bgt - dừng đỗ
                        report_sv = new ReportService();
                        IList<PauseStop> listPausStop = report_sv.ReportPause_Stop(param);
                        jsonResult = Json(listPausStop, JsonRequestBehavior.AllowGet);
                        break;
                    case 14: // báo cáo bgt - thời gian lái xe liên tục
                        report_sv = new ReportService();
                        IList<TimeDriver> _list_TimeDriver = report_sv.TimeDriver(param);
                        jsonResult = Json(_list_TimeDriver, JsonRequestBehavior.AllowGet);
                        break;
                    case 12: // báo cáo bgt - tổng hợp theo lái xe
                        report_sv = new ReportService();
                        IList<General> list_Driver = report_sv.ReportGeneralbyDriver(param);
                        jsonResult = Json(list_Driver, JsonRequestBehavior.AllowGet);
                        break;
                    case 101: // báo cáo dn - tổng hợp
                        report_sv = new ReportService();
                        IList<Distance> _listDistane = report_sv.ReportDistance(param);
                        jsonResult = Json(_listDistane, JsonRequestBehavior.AllowGet);
                        break;
                    case 102: // báo cáo dn - chi tiết
                        report_sv = new ReportService();
                        List<BaoCaoHanhTrinh_Detail> _ListBaoCaoHanhTrinh_Detail = report_sv.ReportHanhTrinh_Detail(param);
                        jsonResult = Json(_ListBaoCaoHanhTrinh_Detail, JsonRequestBehavior.AllowGet);
                        break;
                    case 103: // báo cáo dn - tổng hợp km xe hoạt động
                        report_sv = new ReportService();
                        IList<TimeDriver> _list_tonghopkm = report_sv.ReportKm(param);
                        jsonResult = Json(_list_tonghopkm, JsonRequestBehavior.AllowGet);
                        break;
                    case 104: // báo cáo dn - dừng đỗ
                        report_sv = new ReportService();
                        IList<PauseStop> _listPausStop = report_sv.ReportPause_Stop(param);
                        jsonResult = Json(_listPausStop, JsonRequestBehavior.AllowGet);
                        break;
                    case 105: // báo cáo tắt/mở máy lạnh
                        report_sv = new ReportService();
                        IList<Open_Close> _list_ReportCooler = report_sv.ReportCooler(param);
                        jsonResult = Json(_list_ReportCooler, JsonRequestBehavior.AllowGet);
                        break;
                    case 107: // báo cáo dn - tổng hợp tiêu hao nhiên liệu
                        report_sv = new ReportService();
                        IList<Fuel> _list_Fuel = report_sv.ReportFuel_LineChart(param);
                        if (_list_Fuel.Count > 0)
                        {
                            jsonResult = Json(_list_Fuel, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            jsonResult = Json("", JsonRequestBehavior.AllowGet);
                        }
                        break;
                    case 117: // báo cáo dn - tắt/mở khoá điện
                        report_sv = new ReportService();
                        IList<On_Off> list_ReportOpen_Close = report_sv.ReportOn_Off(param);
                        jsonResult = Json(list_ReportOpen_Close, JsonRequestBehavior.AllowGet);
                        //jsonResult = Json(list_, JsonRequestBehavior.AllowGet);
                        break;
                    case 118: // báo cáo dn - tắt/mở mô tô tải
                        report_sv = new ReportService();
                        IList<Open_Close> list_ReportCooler = report_sv.ReportCooler(param);
                        jsonResult = Json(list_ReportCooler, JsonRequestBehavior.AllowGet);
                        break;
                    case 203: // báo cáo bgt - chi tiết vi phạm liên tục 4h
                        report_sv = new ReportService();
                        dv_sv = new DeviceService();
                        IList<TimeDriver> list_TimeDriver_V4 = report_sv.TimeDriverVP4(param);
                        jsonResult = Json(list_TimeDriver_V4, JsonRequestBehavior.AllowGet);
                        break;
                    case 204: // báo cáo bgt - chi tiết vi phạm liên tục 10h
                        report_sv = new ReportService();
                        dv_sv = new DeviceService();
                        IList<TimeDriver> list_TimeDriver_V10 = report_sv.TimeDriverVP10(param);
                        jsonResult = Json(list_TimeDriver_V10, JsonRequestBehavior.AllowGet);
                        break;
                    case 207: // báo cáo bgt - hành trình
                        report_sv = new ReportService();
                        IList<BaoCaoHanhTrinh> list_HT = report_sv.BaoCaoHanhTrinh(param);
                        jsonResult = Json(list_HT, JsonRequestBehavior.AllowGet);
                        break;
                    case 208: // báo cáo bgt - tốc độ xe chạy
                        report_sv = new ReportService();
                        IList<Distance> list_distane = report_sv.ReportDistance(param);
                        jsonResult = Json(list_distane, JsonRequestBehavior.AllowGet);
                        break;
                    case 209: // báo cáo bgt - quá tốc độ giới hạn
                        report_sv = new ReportService();
                        IList<ExceedingSpeed> list_ExceedingSpeed = report_sv.ReportExceedingSpeedDetails(param);
                        jsonResult = Json(list_ExceedingSpeed, JsonRequestBehavior.AllowGet);
                        break;
                    case 210: // báo cáo bgt - thời gian lái xe liên tục
                        report_sv = new ReportService();
                        IList<TimeDriver> list_TimeDriver = report_sv.TimeDriver(param);
                        jsonResult = Json(list_TimeDriver, JsonRequestBehavior.AllowGet);
                        break;

                    default:
                        break;
                }
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;
            }
        }
        [AllowAnonymous]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session["userName"] = null;
            Session["pass"] = null;
            Session["userid"] = null;

            if (Request.Cookies["pass"] != null)
            {
                HttpCookie myCookie = new HttpCookie("pass");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
            if (Request.Cookies["userName"] != null)
            {
                HttpCookie myCookie = new HttpCookie("userName");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
            if (Request.Cookies["userid"] != null)
            {
                HttpCookie myCookie = new HttpCookie("userid");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
            if (Request.Cookies["DeviceID"] != null)
            {
                HttpCookie myCookie = new HttpCookie("DeviceID");
                myCookie.Expires = DateTime.Now.AddDays(-1d);
                Response.Cookies.Add(myCookie);
            }
            return RedirectToAction("Login", "Home");
        }
        public ActionResult LoadStatusDe()
        {
            user_sv = new Services_khndb4();
            Session["userid"] = Request.Cookies["userid"] == null ? "" : Request.Cookies["userid"].Value;
            if (Request.Cookies["userid"] == null)
            {
                return Json(-1, JsonRequestBehavior.AllowGet);
            }
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_RoleID"] = "";
            param["_Node"] = "";
            param["_Lang"] = "";
            param["_UserID"] = (String)Session["userid"];

            List<DeviceStatus> listState = (List<DeviceStatus>)user_sv.getStatus(param);
            if (listState == null)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            if (listState.Count == 0)
            {
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            return Json(listState, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReciveChangeInfo(String checkPass, String pass_old, String password_new, String fullname, String phone, String email, String address)
        {
            var jsonResult = Json("000", JsonRequestBehavior.AllowGet);
            if (Request.Cookies["userid"] == null)
            {
                jsonResult = Json("0001", JsonRequestBehavior.AllowGet);
                return jsonResult;
            }
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["func"] = "";
            param["_UserID"] = (String)Request.Cookies["userid"].Value;
            param["_password"] = "";
            param["_FullName"] = fullname;
            param["_Email"] = email;
            param["_PhoneNumber"] = phone;
            param["_Address"] = address;
            user_sv = new Services_khndb4();
            if (checkPass.Trim().Equals("0"))
            {
                if (String.IsNullOrEmpty(fullname) || String.IsNullOrEmpty(email) || String.IsNullOrEmpty(address))
                {
                    return jsonResult;
                }
                var foo = new EmailAddressAttribute();
                bool checkEmail = foo.IsValid(email);
                if (checkEmail == false)
                {
                    jsonResult = Json("101", JsonRequestBehavior.AllowGet);//err email
                    return jsonResult;
                }
                param["func"] = "0";

                bool result = user_sv.sp_UpdateUser_info_(param);
                jsonResult = Json(result == true ? "1" : "0", JsonRequestBehavior.AllowGet);//insert state
                jsonResult.MaxJsonLength = int.MaxValue;
            }
            else if (checkPass.Trim().Equals("1"))
            {
                String pass_cookie = Request.Cookies["pass"].Value;
                if (!pass_cookie.Equals(pass_old))
                {
                    jsonResult = Json("102", JsonRequestBehavior.AllowGet);//err pass
                }
                else
                {
                    param["func"] = "1";
                    param["_password"] = password_new;
                    bool result = user_sv.sp_UpdateUser_info_(param);
                    jsonResult = Json(result == true ? "2" : "0", JsonRequestBehavior.AllowGet);//insert state
                    jsonResult.MaxJsonLength = int.MaxValue;
                }
            }
            return jsonResult;
        }
        public ActionResult getInfoUser()
        {
            UserRole useritem = null;
            if (Request.Cookies["userName"] != null && Request.Cookies["pass"] != null)
            {
                Dictionary<String, String> param = new Dictionary<string, string>();
                param["_UserID"] = (String)Request.Cookies["userid"].Value;
                useritem = (new ReportService()).sp_getUserByUserID_profile(param);
            }
            if (useritem != null)
            {
                return Json(useritem, JsonRequestBehavior.AllowGet);
            }
            return Json("0001", JsonRequestBehavior.AllowGet);
        }
        public ActionResult Recive_Feedback(String cmt, int danhgia, String chude)
        {
            if (Request.Cookies["userid_provider"] == null)
                return RedirectToAction("Login", "Home");
            if (cmt == null || cmt == "")
                return Json("0", JsonRequestBehavior.AllowGet);

            String provider_id = Request.Cookies["userid_provider"].Value;
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_UserID"] = provider_id;
            param["_username"] = Request.Cookies["userName"].Value;
            param["_chude"] = chude;
            param["_Comment"] = cmt;
            param["_ngay"] = DateTime.Now.ToString();
            param["_danhgia"] = danhgia.ToString();
            bool res = (new Services_khndb4()).sp_update_commentprovider(param);

            return Json(res == true ? "1" : "0", JsonRequestBehavior.AllowGet);
        }
        public ActionResult ViewLastDataBydataid(int deviceid_)
        {
            var jsonResult = Json("", JsonRequestBehavior.AllowGet);
            Dictionary<String, String> param_ = new Dictionary<string, string>();
            param_["deviceid_"] = deviceid_.ToString();
            GpsData listdevicebyUser = (new DeviceService()).sp_get_tbldata_by_deviceid(param_);
            jsonResult = listdevicebyUser != null ? Json(listdevicebyUser, JsonRequestBehavior.AllowGet) : Json("0000", JsonRequestBehavior.AllowGet);
            return jsonResult;
        }
        public ActionResult ViewInfoDevice()
        {

            if (Request.Cookies["userid"] == null)
            {
                return Json("0001", JsonRequestBehavior.AllowGet);
            }
            var jsonResult = Json("", JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = Int16.MaxValue;
            Dictionary<String, String> param_ = new Dictionary<string, string>();
            param_["_UserID"] = (String)Request.Cookies["userid"].Value;
            IList<Device> listdevicebyUser = (new DeviceService()).getAllDevice_ofUser(param_);
            jsonResult = Json(listdevicebyUser, JsonRequestBehavior.AllowGet);

            return jsonResult;
        }
        public ActionResult LoadVideoJourney(String imei, String date)
        {
            Dictionary<String, String> param = new Dictionary<string, string>();
            //param["_DeviceID"] = dvid;
            //Device deviceItem = (new DeviceService()).GetDeviceByDeviceID(param).FirstOrDefault();
            // String imei = deviceItem.Imei;
            //07/11/2019
            //string[] fileArray;
            var jsonResult = Json("0000", JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            // date = "07/11/2019";
            DateTime dateInput = DateTime.ParseExact(date, "dd-MM-yyyy", null);
            String folderName_date = dateInput.ToString("yyyyMMdd");
            //imei = "0x64190621af9";
            string root = AppDomain.CurrentDomain.BaseDirectory;
            //ContentVideos
            root += @"ContentVideos";
            //string path = @"C:\MP_Upload";
            if (Directory.Exists(root))
            {
                root += @"\" + imei;
                if (Directory.Exists(root))
                {
                    root += @"\Video";
                    if (Directory.Exists(root))
                    {

                        root += @"\" + folderName_date;
                        if (Directory.Exists(root))
                        {
                            //fileArray = Directory.GetFiles(root, "*.mp4");
                            DirectoryInfo d = new DirectoryInfo(root);
                            List<FileInfo> File_mp4 = d.GetFiles("*.mp4").ToList();
                            List<FileInfo> Files = new List<FileInfo>();
                            if (File_mp4.Count() > 0)
                            {
                                Files = File_mp4;
                            }
                            DateTime date25 = new DateTime(2019, 12, 25);
                            if (DateTime.Now.Date< date25.Date) {
                                List<FileInfo> File_avi = d.GetFiles("*.avi").ToList();
                                
                                //=File_mp4.Union(d.GetFiles("*.avi")).ToList();
                                for (int j = 0; j < File_avi.Count(); j++)
                                {
                                    FileInfo fileItem = File_mp4.Where(m => m.Name.Replace(".mp4", "").Equals(File_avi[j].Name.Replace(".avi", ""))).FirstOrDefault();
                                    if (fileItem == null)
                                    {
                                        File_mp4.Add(File_avi[j]);
                                    }
                                }
                                if (File_mp4.Count() > 0)
                                {
                                    Files = File_mp4;
                                }
                                else
                                {
                                    Files = File_avi;
                                }
                            }
                           
                            List<FileModel> list_fileName = new List<FileModel>();

                            foreach (var item in Files)
                            {
                                FileModel item_ = new FileModel();
                                item_.fileName = item.Name;
                                item_.Group = 0;
                                String dateStr = folderName_date + " " + item.Name.Replace(".mp4", "").Replace(".avi", "");
                                item_.Folderdate = folderName_date;
                                if (dateStr.Length > 15)
                                {
                                    String[] dateStr_arr = dateStr.Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                                    dateStr = dateStr_arr[0];
                                    item_.Group = Int16.Parse(dateStr_arr[1]);
                                }
                                DateTime dateParse = DateTime.ParseExact(dateStr, "yyyyMMdd HHmmss", null);
                                item_.dateSave = dateParse;
                                item_.date_ = dateParse.ToString("dd-MM-yyyy HH:mm:ss");
                                item_.dateSave_str = dateParse.ToString("dd-MM-yyyy");
                                item_.IMEI = imei;
                                list_fileName.Add(item_);
                            }
                            dv_sv = new DeviceService();
                            Device dv_item = dv_sv.sp_getdevice_by_imei(imei);
                            user_sv = new Services_khndb4();
                            Dictionary<String, String> param_ = new Dictionary<string, string>();
                            param_["deviceid_"] = dv_item.DeviceID.ToString();
                            
                            DeviceStatus device_status_item = user_sv.sp_get_status_last_time_by_deviceid(param_);
                            //root += @"\" + folderName_date;
                            list_fileName = list_fileName.OrderBy(m => m.dateSave).ToList();
                            JourneyModel journey = new JourneyModel();
                            journey.listFile = list_fileName;
                            journey.statusDevice = device_status_item;
                           
                            //list_fileName = list_fileName.OrderBy(m => m.Group).ToList();
                            jsonResult = Json(journey, JsonRequestBehavior.AllowGet);
                            //String a = "";

                        }
                    }

                }

            }
            //20191107
            return jsonResult;
        }
        public ActionResult TestgetAjaxVideo() {
            var jsonResult = Json("0000", JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            //H:\Visual Studio 2012\Projects\Project_NEW_KHANHHOI.VN\241219\khanhhoi.vn\khanhhoi.vn\ContentVideos\0x900eb30b5e51\Video\20191108\191755_1.mp4
            String videoTest = @"H:\Visual Studio 2012\Projects\Project_NEW_KHANHHOI.VN\241219\khanhhoi.vn\khanhhoi.vn\ContentVideos\0x900eb30b5e51\Video\20191108\191755_1.mp4";
            jsonResult = Json(videoTest, JsonRequestBehavior.AllowGet);
            return jsonResult;
        }
        public ActionResult PlayBackJourney(String Cur, String im, String d)
        {
            //var convert = new NReco.VideoConverter.FFMpegConverter();
            //String input = @"H:\Visual Studio 2012\Projects\Project_NEW_KHANHHOI.VN\131119_2\131119\khanhhoi.vn\khanhhoi.vn\ContentVideos\0x64190621af9\Video\20191120\111000.avi";
            //String output = @"H:\Visual Studio 2012\Projects\Project_NEW_KHANHHOI.VN\131119_2\131119\khanhhoi.vn\khanhhoi.vn\ContentVideos\0x64190621af9\Video\20191120\test.mp4";
            //convert.ConvertMedia(input, output, NReco.VideoConverter.Format.mp4);

            if (Request.Cookies["userid"] == null)
            {
                return RedirectToAction("Logout", "Home");
            }
            var jsonResult = Json("", JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = Int16.MaxValue;
            Dictionary<String, String> param_ = new Dictionary<string, string>();
            param_["_UserID"] = (String)Request.Cookies["userid"].Value;
            IList<Device> listdevicebyUser = (new DeviceService()).getAllDevice_ofUser(param_);
            String path = "";
            String folderName_date = "";
            ViewBag.date_str = "";
            if (!String.IsNullOrEmpty(Cur))
            {
                DateTime dateInput = DateTime.ParseExact(d, "dd-MM-yyyy", null);
                folderName_date = dateInput.ToString("yyyyMMdd");
                if (Cur.Contains("avi"))
                {
                    string root = AppDomain.CurrentDomain.BaseDirectory;
                    root += @"ContentVideos";
                    root += @"\" + im;
                    root += @"\Video";
                    root += @"\" + folderName_date;
                    String inputFile = root + @"\" + Cur;
                    if (System.IO.File.Exists(inputFile))
                    {
                        String outputFile = root + @"\" + Cur.Replace("avi", "mp4");
                        var convert = new NReco.VideoConverter.FFMpegConverter();
                        convert.ConvertMedia(inputFile, outputFile, NReco.VideoConverter.Format.mp4);

                        try
                        {
                            Cur = Cur.Replace("avi", "mp4");
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(err.Message);
                        }
                        System.IO.File.Delete(inputFile);
                    }
                    else
                    {
                        Cur = Cur.Replace("avi", "mp4");
                    }

                    // String a = "";
                }


                //~/ContentVideos/0x64190621af9/Video/20191107/125856.mp4
                // path = "~/ContentVideos/";
                // path += im + "/Video/" + folderName_date + "/" + Cur;
                String dateStr = folderName_date + " " + Cur.Replace(".mp4", "").Replace(".avi", "");

                if (dateStr.Length > 15)
                {
                    String[] dateStr_arr = dateStr.Split(new Char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
                    dateStr = dateStr_arr[0];
                    String group = dateStr_arr[1];
                }
                DateTime dateParse = DateTime.ParseExact(dateStr, "yyyyMMdd HHmmss", null);
                ViewBag.date_str = dateParse.ToString("dd-MM-yyyy HH:mm:ss");
            }

            ViewBag.listdevicebyUser = listdevicebyUser;
            ViewBag.imei = im;
            ViewBag.folderName_date = folderName_date;
            ViewBag.date_ = d;
            ViewBag.Cur = Cur;
            return View();
        }
        public ActionResult Convert_avi_to_mp4(String imei__, String dateform_, String filename_) {
            string root = AppDomain.CurrentDomain.BaseDirectory;
            root += @"ContentVideos";
            root += @"\" + imei__;
            root += @"\Video";
            root += @"\" + dateform_;
            String inputFile = root + @"\" + filename_;
            if (System.IO.File.Exists(inputFile))
            {
                String outputFile = root + @"\" + filename_.Replace("avi", "mp4");
                var convert = new NReco.VideoConverter.FFMpegConverter();
                convert.ConvertMedia(inputFile, outputFile, NReco.VideoConverter.Format.mp4);
                filename_ = filename_.Replace("avi", "mp4");
 
                System.IO.File.Delete(inputFile);
                return Json(filename_, JsonRequestBehavior.AllowGet);
            }

            return Json("Err", JsonRequestBehavior.AllowGet);
        }
        public ActionResult sp_get_tbldozy_by_deviceid_date(String dvID, String d)
        {
            var jsonResult = Json("0000", JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            if (Request.Cookies["userid"] == null)
            {
                return Json(jsonResult, JsonRequestBehavior.AllowGet);
            }
            DateTime dateInput = DateTime.ParseExact(d, "dd-MM-yyyy", null);
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["deviceid_"] = dvID;
            param["date_"] = dateInput.ToString("yyyy-MM-dd 00:00:00");
            dv_sv = new DeviceService();
            List<tbldozy> listDozy = dv_sv.sp_get_tbldozy_by_deviceid_date(param).ToList();

            return Json(listDozy, JsonRequestBehavior.AllowGet);
        }
        public ActionResult sp_GetUser_ByID()
        {
            user_sv = new Services_khndb4();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_UserID"] = (String)Request.Cookies["userid"].Value;
            IEnumerable<Users> info_user = user_sv.GetUser_ByID(param);
            return Json(info_user, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetUser_ByWhoCreateID()
        {
            user_sv = new Services_khndb4();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_UserID"] = (String)Request.Cookies["userid"].Value;
            IEnumerable<Users> list_user = user_sv.GetUser_ByWhoCreateID(param);
            return Json(list_user, JsonRequestBehavior.AllowGet);
        }
        public ActionResult sp_GetDevice_byUserID(String UserID)
        {
            dv_sv = new DeviceService();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_UserID"] = UserID;
            IEnumerable<DeviceUser> _dsxe = dv_sv.GetDeviceby_UserID(param);
            return Json(_dsxe, JsonRequestBehavior.AllowGet);
        }

        public ActionResult sp_GetDevice_byUserIDT()
        {
            dv_sv = new DeviceService();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_UserID"] = (String)Request.Cookies["userid"].Value;
            IEnumerable<DeviceUser> list_thietbi = dv_sv.GetDeviceby_UserID(param);
            return Json(list_thietbi, JsonRequestBehavior.AllowGet);
        }
        public ActionResult sp_GetDriver_byDeviceID(int _DeviceID)
        {
            dv_sv = new DeviceService();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_DeviceID"] = _DeviceID.ToString();
            IEnumerable<Driver> list_taixe = dv_sv.GetDriver_byDeviceID(param);
            return Json(list_taixe, JsonRequestBehavior.AllowGet);
        }
        public ActionResult InsertAcc_byUserLogin(String list_deviceid, String username, String pass, String fullname, String email, String phone, String address, String note)
        {
            //user_sv = new Services_khndb4();
            dv_sv = new DeviceService();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_LoginName"] = username;
            param["_Password_"] = pass;
            param["_FullName"] = fullname;
            param["_Email"] = email;
            param["_PhoneNumber"] = phone;
            param["_Address"] = address;
            param["_Detail"] = note;
            param["_WhoCreateID"] = (String)Request.Cookies["userid"].Value;
            param["_ListDeviceID"] = list_deviceid;
            //bool res = user_sv.sp_InsertAcc_byUserLogin(param);
            bool res = dv_sv.AddUser_Device(param);
            return Json(res == true ? "1" : "0", JsonRequestBehavior.AllowGet);
        }
        public ActionResult EditAcc_byUserLogin(String userid, String password, String fullname, String email, String phone, String address, String note,
            String Selected, String Unselect)
        {
            //user_sv = new Services_khndb4();
            dv_sv = new DeviceService();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_UserID"] = userid;
            param["_Password_"] = password;
            param["_FullName"] = fullname;
            param["_Email"] = email;
            param["_PhoneNumber"] = phone;
            param["_Address"] = address;
            param["_Detail"] = note;
            param["_WhoCreateID"] = (String)Request.Cookies["userid"].Value;
            param["_Selected"] = Selected;
            param["_Unselect"] = Unselect;
            //bool res = user_sv.sp_InsertAcc_byUserLogin(param);
            bool res = dv_sv.EditUser_Device(param);
            return Json(res == true ? "1" : "0", JsonRequestBehavior.AllowGet);
        }

        public ActionResult DeleteAcc_byUserLogin(String list_userid)
        {
            dv_sv = new DeviceService();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_UserID"] = list_userid;
            bool res = dv_sv.DeleteUser_Device(param);
            return Json(res == true ? "1" : "0", JsonRequestBehavior.AllowGet);
        }

        public ActionResult sp_Create_DriverT(String DayExpiredLicense, String DayCreateLicense, String NameDriver, String PhoneDriver, String IdCard,
            String DriverLicense, String Rank, String Details, String DeviceID, String RegPlace)
        {
            user_sv = new Services_khndb4();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["DayExpiredLicense"] = DayExpiredLicense;
            param["DayCreateLicense"] = DayCreateLicense;
            param["NameDriver"] = NameDriver;
            param["PhoneDriver"] = PhoneDriver;
            param["IdCard"] = IdCard;
            param["DriverLicense"] = DriverLicense;
            param["Rank"] = Rank;
            param["Details"] = Details;
            param["DeviceID"] = DeviceID;
            param["RegPlace"] = RegPlace;

            bool _result = user_sv.CreateDriver(param);
            return Json(_result == true ? "1" : "0", JsonRequestBehavior.AllowGet);
        }
        public ActionResult sp_Update_DriverT(String DayExpiredLicense, String DayCreateLicense, String NameDriver, String PhoneDriver, String IdCard,
            String DriverLicense, String Rank, String Details, String DeviceID, String DriverID, String RegPlace, String DDID)
        {
            user_sv = new Services_khndb4();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["DayExpiredLicense"] = DayExpiredLicense;
            param["DayCreateLicense"] = DayCreateLicense;
            param["NameDriver"] = NameDriver;
            param["PhoneDriver"] = PhoneDriver;
            param["IdCard"] = IdCard;
            param["DriverLicense"] = DriverLicense;
            param["Rank"] = Rank;
            param["Details"] = Details;
            param["DeviceID"] = DeviceID;
            param["DriverID"] = DriverID;
            param["RegPlace"] = RegPlace;
            param["DDID"] = DDID;

            bool _result = user_sv.UpdateDriver(param);
            return Json(_result == true ? "1" : "0", JsonRequestBehavior.AllowGet);
        }
        public ActionResult sp_Delete_Driver(String DriverID)
        {
            user_sv = new Services_khndb4();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_DriverID"] = DriverID;
            bool _result = user_sv.DeleteDriver(param);
            return Json(_result == true ? "1" : "0", JsonRequestBehavior.AllowGet);
        }

        public ActionResult sp_Update_DeviceGroupUserT(String DeviceID, String VehicleID, String VehicleGroupIDChild, String VehicleNumber,
            String VehicleCategoryID, String Capacity, String BarrelNumber, String SimNumber, String SimID, String _ProvinceID, String _TypeTransportID, String Business, String Chassis, String Grosston, String ParentGroupID, String _CurrentChildID, String _Switch_, String _Switch_Door,
            String FuelConsumption, String FuelCapacity)
        {
            user_sv = new Services_khndb4();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["DeviceID"] = DeviceID;
            param["VehicleID"] = VehicleID;
            param["VehicleGroupIDChild"] = VehicleGroupIDChild;
            param["VehicleNumber"] = VehicleNumber;
            param["VehicleCategoryID"] = VehicleCategoryID;
            param["Capacity"] = Capacity;
            param["BarrelNumber"] = BarrelNumber;
            param["SimNumber"] = SimNumber;
            param["SimID"] = SimID;
            param["_ProvinceID"] = _ProvinceID;
            param["_TypeTransportID"] = _TypeTransportID;
            param["Business"] = Business;
            param["Chassis"] = Chassis;
            param["Grosston"] = Grosston;
            param["ParentGroupID"] = ParentGroupID;
            param["_CurrentChildID"] = _CurrentChildID;
            param["_Switch_"] = _Switch_;
            param["_Switch_Door"] = _Switch_Door;
            param["FuelConsumption"] = FuelConsumption;
            param["FuelCapacity"] = FuelCapacity;

            bool _result = user_sv.Update_Device(param);
            return Json(_result == true ? "1" : "0", JsonRequestBehavior.AllowGet);
        }

        public ActionResult sp_GetAll_VehicleCategory()
        {
            dv_sv = new DeviceService();
            IEnumerable<VehicleCategorys> _result = dv_sv.GetAll_VehicleCategory();
            return Json(_result, JsonRequestBehavior.AllowGet);

        }
        public ActionResult sp_GetAll_Province()
        {
            dv_sv = new DeviceService();
            IEnumerable<Province> _result = dv_sv.GetAll_Province();
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult sp_GetAll_TypeTranport()
        {
            dv_sv = new DeviceService();
            IEnumerable<TypeTransport> _result = dv_sv.GetAll_TypeTransport();
            return Json(_result, JsonRequestBehavior.AllowGet);
        }
        public ActionResult sp_GetAll_VehicleGroup_byUserIDT()
        {
            dv_sv = new DeviceService();
            Dictionary<String, String> param = new Dictionary<string, string>();
            param["_UserID"] = (String)Request.Cookies["userid"].Value;
            IEnumerable<VehicleGroups> _result = dv_sv.GetAllVehicleGroup_byUserID(param);
            return Json(_result, JsonRequestBehavior.AllowGet);
        }


    }
}