using ElasticCaptainSmackDown.Models;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement;
using Microsoft.Azure.SqlDatabase.ElasticScale.ShardManagement.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElasticCaptainSmackDown.Util
{
    public class ShardHelper
    {
        public static RangeShardMap<int> CreateOrGetRangeShardMap()
        {
            string shardMapName = "CaptainShardMap";
            string shardDbName = string.Format(ConfigHelper.DatabaseName, "ShardMap");

            if (!SqlHelper.DatabaseExists(ConfigHelper.DataSource, shardDbName))
            {
                SqlHelper.CreateDatabase(ConfigHelper.DataSource, shardDbName);
            }

            var shardMapManager = CreateOrGetShardMapManager(SqlHelper.GetConnectionString(ConfigHelper.DataSource, shardDbName));
            RangeShardMap<int> shardMap = CreateOrGetRangeShardMap(shardMapManager, shardMapName);

            return shardMap;
        }

        /// <summary>
        /// Creates a shard map manager in the database specified by the given connection string.
        /// </summary>
        private static ShardMapManager CreateOrGetShardMapManager(string shardMapManagerConnectionString)
        {
            // Get shard map manager database connection string
            // Try to get a reference to the Shard Map Manager in the Shard Map Manager database. If it doesn't already exist, then create it.
            ShardMapManager shardMapManager;
            bool shardMapManagerExists = ShardMapManagerFactory.TryGetSqlShardMapManager(
                shardMapManagerConnectionString,
                ShardMapManagerLoadPolicy.Lazy,
                out shardMapManager);

            if (!shardMapManagerExists)
            {
                // The Shard Map Manager does not exist, so create it
                shardMapManager = ShardMapManagerFactory.CreateSqlShardMapManager(shardMapManagerConnectionString);
            }

            return shardMapManager;
        }

        /// <summary>
        /// Creates a new Range Shard Map with the specified name, or gets the Range Shard Map if it already exists.
        /// </summary>
        private static RangeShardMap<int> CreateOrGetRangeShardMap(ShardMapManager shardMapManager, string shardMapName)
        {
            // Try to get a reference to the Shard Map.
            RangeShardMap<int> shardMap;
            bool shardMapExists = shardMapManager.TryGetRangeShardMap(shardMapName, out shardMap);

            if (!shardMapExists)
            {
                shardMap = shardMapManager.CreateRangeShardMap<int>(shardMapName);

                SchemaInfo schemaInfo = new SchemaInfo();
                schemaInfo.Add(new ShardedTableInfo("Votes", "Captain"));
                shardMapManager.GetSchemaInfoCollection().Add(shardMapName, schemaInfo);

                for (int i = 0; i < 4; i++)
                {
                    int low = i * 25;
                    int high = low + 24;

                    CreateShard(shardMap, new Range<int>(low, high));

                }
            }

            return shardMap;
        }

        private static void CreateShard(RangeShardMap<int> shardMap, Range<int> rangeForNewShard)
        {
            // Create a new shard, or get an existing empty shard (if a previous create partially succeeded).
            Shard shard = CreateOrGetEmptyShard(shardMap);

            // Create a mapping to that shard.
            RangeMapping<int> mappingForNewShard = shardMap.CreateRangeMapping(rangeForNewShard, shard);
        }

        private static Shard CreateOrGetEmptyShard(RangeShardMap<int> shardMap)
        {
            // Get an empty shard if one already exists, otherwise create a new one
            Shard shard = FindEmptyShard(shardMap);
            if (shard == null)
            {
                // No empty shard exists, so create one

                // Choose the shard name
                string databaseName = string.Format(ConfigHelper.DatabaseName, $"VoteShard_{shardMap.GetShards().Count():00}");

                // Only create the database if it doesn't already exist. It might already exist if
                // we tried to create it previously but hit a transient fault.
                if (!SqlHelper.DatabaseExists(ConfigHelper.DataSource, databaseName))
                {
                    SqlHelper.CreateDatabase(ConfigHelper.DataSource, databaseName);
                }

                // Add it to the shard map
                ShardLocation shardLocation = new ShardLocation(ConfigHelper.DataSource, databaseName);
                shard = CreateOrGetShard(shardMap, shardLocation);
            }

            return shard;
        }

        private static Shard CreateOrGetShard(ShardMap shardMap, ShardLocation shardLocation)
        {
            // Try to get a reference to the Shard
            Shard shard;
            bool shardExists = shardMap.TryGetShard(shardLocation, out shard);

            if (!shardExists)
            {
                // The Shard Map does not exist, so create it
                shard = shardMap.CreateShard(shardLocation);

                //force EF to push the schema for the new shard
                var voteContext = new VoteContext(shard.Location.DataSource, shard.Location.Database);
                voteContext.Database.Initialize(true);
            }

            return shard;
        }


        private static Shard FindEmptyShard(RangeShardMap<int> shardMap)
        {
            // Get all shards in the shard map
            IEnumerable<Shard> allShards = shardMap.GetShards();

            // Get all mappings in the shard map
            IEnumerable<RangeMapping<int>> allMappings = shardMap.GetMappings();

            // Determine which shards have mappings
            HashSet<Shard> shardsWithMappings = new HashSet<Shard>(allMappings.Select(m => m.Shard));

            // Get the first shard (ordered by name) that has no mappings, if it exists
            return allShards.OrderBy(s => s.Location.Database).FirstOrDefault(s => !shardsWithMappings.Contains(s));
        }
    }
}