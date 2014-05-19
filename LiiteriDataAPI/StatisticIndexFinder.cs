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
    public class StatisticIndexFinder
    {
        private DbConnection GetDbConnection()
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

        public List<Models.StatisticIndexBrief>
            GetStatisticIndexBriefsByName(string searchString)
        {
            string sqlString = @"
SELECT
    *
FROM
    statisticIndex
WHERE
    theme1 LIKE @searchString OR
    theme2 LIKE @searchString OR
    theme3 LIKE @searchString OR
    theme4 LIKE @searchString OR
    theme5 LIKE @searchString OR
    statisticName LIKE @searchString OR
    CONCAT(
        theme1,
        theme2,
        theme3,
        theme4,
        theme5,
        statisticName) LIKE @searchString
ORDER BY
    theme1,
    theme2,
    theme3,
    theme4,
    theme5,
    statisticName
";

            var results = new List<Models.StatisticIndexBrief>();

            using (DbConnection db = this.GetDbConnection()) {
                using (DbCommand cmd = db.CreateCommand()) {
                    DbParameter param;
                    cmd.CommandText = sqlString;

                    param = cmd.CreateParameter();
                    param.DbType = DbType.String;
                    param.ParameterName = "@searchString";
                    param.Value = "%" + searchString + "%";
                    cmd.Parameters.Add(param);

                    using (DbDataReader rdr = cmd.ExecuteReader()) {
                        var factory = new Models.StatisticIndexBriefFactory();
                        while (rdr.Read()) {
                            Models.StatisticIndexBrief result =
                                factory.GetStatisticIndexResult(rdr);
                            results.Add(result);
                        }
                    }
                }
            }

            return results;
        }

        public Models.StatisticIndexDetails
            GetStatisticIndexDetailsById(int id)
        {
            string sqlString1 = @"
USE [LiiteriDataMarts];

SELECT
	*
FROM
	statisticIndex
LEFT JOIN
	DimTilasto ON Tilasto_ID = statisticIndex.statisticId
WHERE
	id = @id
";

            Models.StatisticIndexDetails result;
            Models.StatisticIndexBrief brief;

            using (DbConnection db = this.GetDbConnection()) {
                using (DbCommand cmd = db.CreateCommand()) {
                    DbParameter param;
                    cmd.CommandText = sqlString1;

                    param = cmd.CreateParameter();
                    param.DbType = DbType.Int32;
                    param.ParameterName = "@id";
                    param.Value = id;
                    cmd.Parameters.Add(param);

                    using (DbDataReader rdr = cmd.ExecuteReader()) {
                        if (!rdr.HasRows) return null;
                        rdr.Read();
                        var factory1 = new Models.StatisticIndexBriefFactory();
                        brief = factory1.GetStatisticIndexResult(rdr);
                        var factory2 = new Models.StatisticIndexDetailsFactory();
                        result = factory2.GetStatisticIndexDetails(brief, rdr);
                        return result;
                    }
                }
            }
        }
    }
}