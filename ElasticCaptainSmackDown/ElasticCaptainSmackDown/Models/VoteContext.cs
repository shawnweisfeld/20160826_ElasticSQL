using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ElasticCaptainSmackDown.Models
{
    public class VoteContext : DbContext
    {
        public VoteContext(string server, string database) 
            : base (Util.SqlHelper.GetConnectionString(server, database))
        {

        }

        public DbSet<Vote> Votes { get; set; }

    }
}