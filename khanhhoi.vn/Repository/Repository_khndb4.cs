using khanhhoi.vn.Conenction;
using khanhhoi.vn.Interfaces;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace khanhhoi.vn.Repository
{
    public class Repository_khndb4 : IRepository
    {
        public khndb4Context TNCctx { get; set; }
        public Database Db { get; set; }
        static string connectionString = null;
        public string conn1 = System.Configuration.ConfigurationManager.ConnectionStrings["SeagullDB"].ConnectionString;
        public string conn2 = System.Configuration.ConfigurationManager.ConnectionStrings["SeagullDB2"].ConnectionString;
        public string conn3 = System.Configuration.ConfigurationManager.ConnectionStrings["SeagullDB3"].ConnectionString;

        public Repository_khndb4()
        {
            if(connectionString == null)
            {
                connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["SeagullDB"].ConnectionString;
            }

            
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                this.TNCctx = new khndb4Context(connection, false);

                this.Db = this.TNCctx.Database;
            }
            catch
            {
                connectionString = ChangeConnectString(connectionString);
                connection.Close();
                new Repository_khndb4();                
            }
        }
        public Repository_khndb4(string connectionString)
        {
            MySqlConnection connection = new MySqlConnection(connectionString);
            try
            {
                connection.Open();
                this.TNCctx = new khndb4Context(connection, false);

                this.Db = this.TNCctx.Database;
            }
            catch
            {
                return;
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
            try
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
            catch (Exception)
            {
                throw;
            }
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
        public string ChangeConnectString(string con)
        {
            if (con == conn1) return conn2;
            else if (con == conn2) return conn3;
            else return conn1;
        }
    }
}