using MySql.Data.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace khanhhoi.vn.Conenction
{
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class khndb4_backupContext : DbContext
    {
        public khndb4_backupContext() : base() { }
        public ObjectContext ObjectContext()
        {
            return ((IObjectContextAdapter)this).ObjectContext;
        }
        public khndb4_backupContext(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {

        }
    }
}