using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace ElasticCaptainSmackDown.Util
{
    public class ConfigHelper
    {
        public static string UserName
        {
            get
            {
                return GetString(nameof(UserName));
            }
        }

        public static string Password
        {
            get
            {
                return GetString(nameof(Password));
            }
        }

        public static bool IntegratedSecurity
        {
            get
            {
                return GetBool(nameof(IntegratedSecurity));
            }
        }

        public static string ApplicationName
        {
            get
            {
                return GetString(nameof(ApplicationName));
            }
        }

        public static string DataSource
        {
            get
            {
                return GetString(nameof(DataSource));
            }
        }
        
        public static string DatabaseName
        {
            get
            {
                return GetString(nameof(DatabaseName));
            }
        }

        private static bool GetBool(string key)
        {
            var tmp = ConfigurationManager.AppSettings[key];
            return tmp != null && bool.Parse(tmp);
        }

        private static string GetString(string key)
        {
            return ConfigurationManager.AppSettings[key] ?? string.Empty;
        }
    }
}