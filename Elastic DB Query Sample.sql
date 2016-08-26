/*

CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'my password!';

CREATE DATABASE SCOPED CREDENTIAL ElasticDBQueryCred
WITH IDENTITY = 'shawn',
SECRET = 'my password';

--bring in all the shards (horizontally partitioned)
CREATE EXTERNAL DATA SOURCE capsmkdwn_DataSrc WITH
  (TYPE = SHARD_MAP_MANAGER,
  LOCATION = 'houstonsqlsat.database.windows.net',
  DATABASE_NAME = 'capsmkdwn_ShardMap',
  CREDENTIAL = ElasticDBQueryCred,
  SHARD_MAP_NAME = 'CaptainShardMap'
) ;

CREATE EXTERNAL TABLE [dbo].[Votes](
	[ID] [int] NOT NULL,
	[Captain] [int] NOT NULL,
	[Placed] [datetime] NOT NULL)
WITH
( DATA_SOURCE = capsmkdwn_DataSrc,
  DISTRIBUTION = SHARDED([Captain])
) ;

-- bring in the captain meta data info (vertical partitioning) 
CREATE EXTERNAL DATA SOURCE capsmkdwn_DataSrcCap WITH 
    (TYPE = RDBMS, 
    LOCATION = 'houstonsqlsat.database.windows.net', 
    DATABASE_NAME = 'capsmkdwn_Captain', 
    CREDENTIAL = ElasticDBQueryCred, 
) ;

CREATE EXTERNAL TABLE [dbo].[Captains](
	[ID] [int] NOT NULL,
	[Name] [nvarchar](max) NULL) 
WITH 
( DATA_SOURCE = capsmkdwn_DataSrcCap) 

*/
--execute a query
SELECT C.Name, COUNT(*) AS Votes
FROM [dbo].[Votes] AS V
LEFT OUTER JOIN [dbo].[Captains] AS C
  ON V.Captain = C.ID
GROUP BY C.Name

SELECT C.Name, V.Votes
FROM (SELECT Captain, COUNT(*) AS Votes
	FROM dbo.Votes
	GROUP BY Captain) AS V
LEFT OUTER JOIN [dbo].[Captains] AS C
  ON V.Captain = C.ID
