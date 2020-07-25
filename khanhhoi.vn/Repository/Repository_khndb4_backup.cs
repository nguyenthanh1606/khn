using khanhhoi.vn.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using khanhhoi.vn.Conenction;

namespace khanhhoi.vn.Repository
{
    public class Repository_khndb4_backup : IRepository
    {
        public khndb4_backupContext TNCctx { get; set; }
        public Database Db { get; set; }
        String connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SeagullDB_Backup"].ConnectionString;
        public Repository_khndb4_backup()
        {
            MySqlConnection connection = new MySqlConnection(connectionString);

            
            // MySqlTransaction transaction = connection.BeginTransaction();
            try
            {
                connection.Open();
                this.TNCctx = new khndb4_backupContext(connection, false);

                // Interception/SQL logging
                this.TNCctx.Database.Log = (string message) => { Console.WriteLine(message); };

                this.Db = this.TNCctx.Database;
                //IEnumerable<GpsData> result = Db.SqlQuery<GpsData>("Select * from tbldata order by dataid desc limit 20");
                // List<GpsData> list = result.ToList();
                // Db.Connection.Close();


                //transaction.Commit();
            }
            catch
            {
                return;
                
                //transaction.Rollback();
                //throw;
            }




        }
        public IEnumerable<T> ExecuteSqlQuery<T>(string query)
        {
            try
            {
                IEnumerable<T> result = Db.SqlQuery<T>(query);
                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }

        }

        public IEnumerable<T> ExecuteStoreProceduce<T>(string storeProceduce, Dictionary<string, string> parameters)
        {
            string query = "call " + storeProceduce + "(";
            if (parameters != null)
            {
                //    query = parameters.Aggregate(query, (current, parameter) => current + ("@" + parameter.Key + " = '" + parameter.Value+"'"));
                foreach (KeyValuePair<string, string> par in parameters)
                {
                    if (parameters.LastOrDefault().Key == par.Key)
                        query += "@" + par.Key + " := '" + par.Value + "'";
                    else
                        query += "@" + par.Key + " := '" + par.Value + "',";
                }
            }
            //query = parameters.Aggregate(query, (current, parameter) => current + ("@" + parameter.Key + "=" + parameter.Value));
            query += ")";
            IEnumerable<T> result = ExecuteSqlQuery<T>(query);
            return result;
        }

        public IEnumerable<T> ExecuteStoreProceduce<T>(string storeProceduce)
        {
            string query = "call " + storeProceduce;
            IEnumerable<T> result = ExecuteSqlQuery<T>(query);
            return result;
        }

        public bool ExecuteSqlCommand(string storeProceduce, Dictionary<string, string> parameters)
        {
            string query = "call " + storeProceduce + " (";
            if (parameters != null)
            {
                //    query = parameters.Aggregate(query, (current, parameter) => current + ("@" + parameter.Key + " = '" + parameter.Value+"'"));
                foreach (KeyValuePair<string, string> par in parameters)
                {
                    if (parameters.LastOrDefault().Key == par.Key)
                        query += "'" + par.Value + "'";
                    else
                        query += "'" + par.Value + "',";
                }
            }
            query += ")";
            int result = Db.ExecuteSqlCommand(query);
            return result >= 1;
        }

        public void Dispose()
        {
            try
            {
                Db.Connection.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}