using ElasticCaptainSmackDown.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElasticCaptainSmackDown.ViewModels.Captain
{
    public class IndexViewModel
    {
        public IndexViewModel()
        {
            Captains = new List<IndexViewModelCaptain>();
        }
        public List<IndexViewModelCaptain> Captains { get; set; }
    }

    public class IndexViewModelCaptain
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int Votes { get; set; }
    }
}