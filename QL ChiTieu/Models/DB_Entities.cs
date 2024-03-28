using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace QL_ChiTieu.Models
{
    public class DB_Entities : System.Data.Entity.DbContext
    {
        public DB_Entities() : base("TransactionDB") { }
        public System.Data.Entity.DbSet<User> Users { get; set; }
       
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //Database.SetInitializer<demoEntities>(null);
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            
            base.OnModelCreating(modelBuilder);


        }
    }

}
