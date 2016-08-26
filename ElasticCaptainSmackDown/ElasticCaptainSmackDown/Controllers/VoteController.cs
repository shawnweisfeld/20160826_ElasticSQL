using ElasticCaptainSmackDown.App_Start;
using ElasticCaptainSmackDown.Models;
using ElasticCaptainSmackDown.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Practices.Unity;

namespace ElasticCaptainSmackDown.Controllers
{
    public class VoteController : Controller
    {

        // GET: Vote
        [HttpGet]
        public ActionResult Cast(int captain)
        {
            var shardMap = ShardHelper.CreateOrGetRangeShardMap();
            var shardMapping = shardMap.GetMappingForKey(captain);

            var container = UnityConfig.GetConfiguredContainer();

            var voteContext = container.Resolve<VoteContext>(shardMapping.Shard.ToString(),
                new ParameterOverride("server", shardMapping.Shard.Location.DataSource),
                new ParameterOverride("database", shardMapping.Shard.Location.Database));

            voteContext.Votes.Add(new Vote()
            {
                Captain = captain,
                Placed = DateTime.UtcNow
            });
            voteContext.SaveChanges();

            ViewBag.CaptainID = captain;

            return View();
        }
    }
}