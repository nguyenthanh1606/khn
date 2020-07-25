using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Interfaces
{
    public interface IRepository
    {
        IEnumerable<T> ExecuteSqlQuery<T>(string query);
        IEnumerable<T> ExecuteStoreProceduce<T>(string storeProceduce, Dictionary<string, string> parameters);
        IEnumerable<T> ExecuteStoreProceduce<T>(string storeProceduce);
        bool ExecuteSqlCommand(string query, Dictionary<string, string> parameters);
        void Dispose();
    }
}