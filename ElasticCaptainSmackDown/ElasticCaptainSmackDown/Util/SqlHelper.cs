using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;

namespace ElasticCaptainSmackDown.Util
{
    public class SqlHelper
    {
        public static string GetCredentialsConnectionString()
        {

            SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder
            {
                // DDR and MSQ require credentials to be set
                UserID = ConfigHelper.UserName,
                Password = ConfigHelper.Password,
                IntegratedSecurity = ConfigHelper.IntegratedSecurity,

                // DataSource and InitialCatalog cannot be set for DDR and MSQ APIs, because these APIs will
                // determine the DataSource and InitialCatalog for you.
                //
                // DDR also does not support the ConnectRetryCount keyword introduced in .NET 4.5.1, because it
                // would prevent the API from being able to correctly kill connections when mappings are switched
                // offline.
                //
                // Other SqlClient ConnectionString keywords are supported.

                ApplicationName = ConfigHelper.ApplicationName,
                ConnectTimeout = 30
            };

            return connStr.ToString();
        }

        /// <summary>
        /// Returns a connection string that can be used to connect to the specified server and database.
        /// </summary>
        public static string GetConnectionString(string serverName, string database)
        {
            SqlConnectionStringBuilder connStr = new SqlConnectionStringBuilder(GetCredentialsConnectionString());
            connStr.DataSource = serverName;
            connStr.InitialCatalog = database;
            return connStr.ToString();
        }

        public static bool DatabaseExists(string server, string db)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString(server, "master")))
            {
                conn.Open();

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select count(*) from sys.databases where name = @dbname";
                cmd.Parameters.AddWithValue("@dbname", db);
                cmd.CommandTimeout = 60;
                int count = int.Parse(cmd.ExecuteScalar().ToString());

                bool exists = count > 0;
                return exists;
            }
        }

        public static bool DatabaseIsOnline(string server, string db)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString(server, "master")))
            {
                conn.Open();

                SqlCommand cmd = conn.CreateCommand();
                cmd.CommandText = "select count(*) from sys.databases where name = @dbname and state = 0"; // online
                cmd.Parameters.AddWithValue("@dbname", db);
                cmd.CommandTimeout = 60;
                int count = int.Parse(cmd.ExecuteScalar().ToString());

                bool exists = count > 0;
                return exists;
            }
        }


        public static void CreateDatabase(string server, string db)
        {
            using (SqlConnection conn = new SqlConnection(GetConnectionString(server, "master")))
            {
                conn.Open();
                SqlCommand cmd = conn.CreateCommand();

                // Determine if we are connecting to Azure SQL DB
                cmd.CommandText = "SELECT SERVERPROPERTY('EngineEdition')";
                cmd.CommandTimeout = 60;
                int engineEdition = int.Parse(cmd.ExecuteScalar().ToString());

                if (engineEdition == 5)
                {
                    // Azure SQL DB
                    if (!DatabaseExists(server, db))
                    {
                        // Begin creation (which is async for Standard/Premium editions)
                        cmd.CommandText = $"CREATE DATABASE {db} (EDITION = 'basic')";
                        cmd.CommandTimeout = 60;
                        cmd.ExecuteNonQuery();
                    }

                    // Wait for the operation to complete
                    while (!DatabaseIsOnline(server, db))
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }
                else
                {
                    // Other edition of SQL DB
                    cmd.CommandText = $"CREATE DATABASE {db}";
                    cmd.ExecuteNonQuery();
                }
            }
        }

    }
}