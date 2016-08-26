using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ElasticCaptainSmackDown.Models
{
    public class CaptainContext : DbContext
    {
        public CaptainContext() 
            : base (Util.SqlHelper.GetConnectionString(Util.ConfigHelper.DataSource, string.Format(Util.ConfigHelper.DatabaseName, "Captain")))
        {
            
        }

        public DbSet<Captain> Captains { get; set; }

    }

    public class CaptainInitalizer : CreateDatabaseIfNotExists<CaptainContext>
    {
        protected override void Seed(CaptainContext context)
        {
            base.Seed(context);

            for (int i = 0; i < 100; i++)
            {
                context.Captains.Add(new Captain()
                {
                    Name = $"Captain {i:N0}"
                });
            }

            context.SaveChanges();
        }
    }
}