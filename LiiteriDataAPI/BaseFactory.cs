using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;
using System.Data;
using System.Configuration;

namespace LiiteriDataAPI
{
    public class BaseFactory
    {
        public DbConnection GetDbConnection()
        {
            string connStr = ConfigurationManager.
                ConnectionStrings["statisticDataDB"].ToString();
            DbConnection conn = new SqlConnection
            {
                ConnectionString = connStr,
            };
            conn.Open();

            return conn;
        }
    }
}