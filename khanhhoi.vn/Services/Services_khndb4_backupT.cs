using khanhhoi.vn.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using khanhhoi.vn.Models;

namespace khanhhoi.vn.Services
{
    public class Services_khndb4_backupT
    {
        private Repository_khndb4_backup Repository;
        public Services_khndb4_backupT()
        {
            Repository = new Repository_khndb4_backup();

        }
        public Dictionary<string, string> paramOld(Dictionary<string, string> param)
        {
            Dictionary<string, string> dateTo = new Dictionary<string, string>
                                                    {
                                                        {"_to", param.FirstOrDefault(pair => pair.Key == "To").Value}
                                                    };
            Int64 toID = Repository.ExecuteStoreProceduce<Int64>("sp_DataID_toDay", dateTo).First();

            Dictionary<string, string> dateFrom = new Dictionary<string, string>
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

            Dictionary<string, string> parameter =
                new Dictionary<string, string>
                    {
                        {"_DeviceID", ""},
                        {"_from", fromID.ToString()},
                        {"_to", toID.ToString()}
                    };
            return parameter;
        }
        public IList<tempdatesave> getDataID(Dictionary<string, string> param)
        {
            //sp_DataID_from_to
            IList<tempdatesave> list =
                       Repository.ExecuteStoreProceduce<tempdatesave>("sp_DataID_from_to", param).ToList();
            return list;
        }
        public GpsData checkLastRecord(Dictionary<string, string> param)
        {
            //sp_DataID_from_to
            GpsData gpsdata =
                       Repository.ExecuteStoreProceduce<GpsData>("chuacodulieu_check", param).ToList().FirstOrDefault();
            return gpsdata;
        }
        public IList<GpsDataExtForGeneral> DataForReportGeneral(Dictionary<string, string> param)
        {
            IList<GpsDataExtForGeneral> list =
                        Repository.ExecuteStoreProceduce<GpsDataExtForGeneral>("getDataForGeneral_byIdT",
                                                                               param).OrderBy(item => item.DateSave).ToList();
            return list;
        }
        public IList<Report_General> DataForReportGeneral_(Dictionary<string, string> param)
        {
            IList<Report_General> list =
                        Repository.ExecuteStoreProceduce<Report_General>("sp_get_tblreportgeneralbyvehicle_byID_From_to",
                                                                               param).OrderBy(item => item.Date_data).ToList();
            return list;
        }

        public IList<DeviceStatus> DataForLoTrinh(Dictionary<string, string> param) //  Imei, from, to
        {

            IList<DeviceStatus> ListLoTrinh =
                Repository.ExecuteStoreProceduce<DeviceStatus>("sp_GetLotrinh_FromTo", param).OrderBy(item => item.DateSave).ToList();
            return ListLoTrinh;
        }

        public IList<GpsDataExt> DataForDistance(Dictionary<string, string> param)//Distance
        {
            IList<GpsDataExt> list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_GetDataDistance_byID",
                                                                                          param).OrderBy(item => item.DateSave).ToList();
            return list;
        }

        public IList<BaoCaoHanhTrinh> DataForHanhTrinh(Dictionary<string, string> param)
        {
            IList<BaoCaoHanhTrinh> hanhTrinhsAll =
                    Repository.ExecuteStoreProceduce<BaoCaoHanhTrinh>("sp_getData_HanhTrinh_byIDT", param).OrderBy(item => item.DateSave).ToList();
            return hanhTrinhsAll;
        }

        public IList<GpsDataExt> DataForExceedingSpeed(Dictionary<string, string> param)
        {
            IList<GpsDataExt> list = Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byIDT",
                                                                                          param).OrderBy(item => item.DateSave).ToList();
            return list;
        }


        public IList<GpsDataForOn_Off> DataForReportOn_Off(Dictionary<string, string> param)//type: on or off
        {
            IList<GpsDataForOn_Off> list = Repository.ExecuteStoreProceduce<GpsDataForOn_Off>("sp_GetDataOn_Off_byID",
                                                                                          param).OrderBy(item => item.DateSave).ToList();
            return list;
        }



        public IList<GpsDataExt> DataForTimeDriver(Dictionary<string, string> param)
        {
            IList<GpsDataExt> list =
                            Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byIDT",
                                                                         param).ToList();
            return list;
        }

        public IList<GpsDataExt> DataForTimeDriverVP10(Dictionary<string, string> param)
        {
            IList<GpsDataExt> list =
                            Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byID",
                                                                         param).ToList();
            return list;
        }
        public IList<GpsDataExt> DataForTimeDriverVP4(Dictionary<string, string> param)
        {
            IList<GpsDataExt> list =
                            Repository.ExecuteStoreProceduce<GpsDataExt>("sp_getData_QuaVanToc_byID",
                                                                         param).ToList();
            return list;
        }


        public IList<GpsDataForOpenClose> DataForReportOpen_Close(Dictionary<string, string> param)
        {
            IList<GpsDataForOpenClose> list =
                        Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataOpenClose_byID",
                                                                              param).OrderBy(item => item.DateSave).ToList();
            return list;
        }

        public IList<GpsDataForOpenClose> DataForReportCooler(Dictionary<string, string> param)
        {
            IList<GpsDataForOpenClose> list =
                        Repository.ExecuteStoreProceduce<GpsDataForOpenClose>("sp_GetDataCooler_byID",
                                                                              param).ToList();
            return list;
        }
        public IList<Fuel> DataForReportFuel(Dictionary<string, string> param)
        {
            IList<Fuel> list =
                        Repository.ExecuteStoreProceduce<Fuel>("sp_GetDataFuel_byID",
                                                                              param).ToList();
            return list;
        }

        public IList<GpsDataForPauseStop> DataForReportPause_Stop(Dictionary<string, string> param)
        {
            IList<GpsDataForPauseStop> list =
                        Repository.ExecuteStoreProceduce<GpsDataForPauseStop>("sp_GetData_PauseStop_byIDT",
                                                                              param).ToList();
            return list;
        }


        //admin
        public bool EditDevice(Dictionary<string, string> parameter)
        {
            return Repository.ExecuteSqlCommand("sp_Update_DeviceT", parameter);
        }
        public bool EditDeviceDL(Dictionary<string, string> parameter)
        {
            return Repository.ExecuteSqlCommand("sp_Update_DeviceDLTT", parameter);
        }

    }
        
}