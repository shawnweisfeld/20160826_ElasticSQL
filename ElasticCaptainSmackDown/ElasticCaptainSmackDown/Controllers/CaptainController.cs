using ElasticCaptainSmackDown.Models;
using Microsoft.Azure.SqlDatabase.ElasticScale.Query;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElasticCaptainSmackDown.Controllers
{
    public class CaptainController : Controller
    {
        CaptainContext _captainContext = null;
        RangeShardMap<int> _shardMap = null;

        public CaptainController(CaptainContext captainContext, RangeShardMap<int> shardMap)
        {
            _captainContext = captainContext;
            _shardMap = shardMap;
        }

        // GET: Captain
        public ActionResult Index()
        {
            Dictionary<int, int> votes = new Dictionary<int, int>();

            //Perform a multi shard query to tally all the votes
            // Get the shards to connect to
            var shards = _shardMap.GetShards();

            // Create the multi-shard connection
            using (var conn = new MultiShardConnection(shards, Util.SqlHelper.GetCredentialsConnectionString()))
            {
                // Create a simple command
                using (MultiShardCommand cmd = conn.CreateCommand())
                {
                    // Because this query is grouped by CustomerID, which is sharded,
                    // we will not get duplicate rows.
                    cmd.CommandText = @"SELECT Captain, COUNT(*) AS Votes FROM [dbo].[Votes] GROUP BY [Captain]";

                    // Allow for partial results in case some shards do not respond in time
                    cmd.ExecutionPolicy = MultiShardExecutionPolicy.PartialResults;

                    // Allow the entire command to take up to 30 seconds
                    cmd.CommandTimeout = 30;

                    // Execute the command. 
                    // We do not need to specify retry logic because MultiShardDataReader will internally retry until the CommandTimeout expires.
                    using (MultiShardDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            votes.Add(reader.GetInt32(0), reader.GetInt32(1));
                        }
                    }
                }
            }


            var vm = new ViewModels.Captain.IndexViewModel();

            foreach (var captain in _captainContext.Captains)
            {
                int voteCount = 0;
                votes.TryGetValue(captain.ID, out voteCount);

                vm.Captains.Add(new ViewModels.Captain.IndexViewModelCaptain()
                {
                    ID = captain.ID,
                    Name = captain.Name,
                    Votes = voteCount
                });
            }

            return View(vm);
        }
    }
}