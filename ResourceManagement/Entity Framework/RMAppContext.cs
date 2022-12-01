using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using ResourceManagement.Models;

namespace ResourceManagement.Entity_Framework
{
    public class RMAppContext : DbContext
    {
        public RMAppContext() : base("name=TimesheetDBEntities")
        {
            Database.SetInitializer<RMAppContext>(new CreateDatabaseIfNotExists<RMAppContext>());
        
        }

        public virtual DbSet<TimeSheet> ambctaskcapture { get; set; }

        public virtual DbSet<EmployeeModel> emp_info { get; set; }
    }
}