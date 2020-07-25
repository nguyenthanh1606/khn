using MySql.Data.EntityFramework;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace khanhhoi.vn.Conenction
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class khndb4Context : DbContext
    {
        public khndb4Context() : base() { }
        public ObjectContext ObjectContext()
        {
            return ((IObjectContextAdapter)this).ObjectContext;
        }
        public khndb4Context(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {

        }
    }
}