using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElasticCaptainSmackDown.Models
{
    public class Vote
    {
        public int ID { get; set; }
        public int Captain { get; set; }
        public DateTime Placed { get; set; }
    }
}