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

        public RegionLayer[] GetRegionLayers(DbConnection db, int id)
        {
            var result = new List<RegionLayer>();
            string sqlString = @"
SELECT
	fta.AlueTaso_ID,
	dat.AlueTasoKuvaus
FROM
	FactTilastoarvo fta,
	DimAlueTaso dat
WHERE
	dat.AlueTaso_ID = fta.AlueTaso_ID AND
	fta.Tilasto_ID = @id
GROUP BY
	fta.AlueTaso_ID,
	dat.AlueTasoKuvaus
";
            using (DbCommand cmd = db.CreateCommand()) {
                DbParameter param;
                cmd.CommandText = sqlString;

                param = cmd.CreateParameter();
                param.DbType = DbType.Int32;
                param.ParameterName = "@id";
                param.Value = id;
                cmd.Parameters.Add(param);

                using (DbDataReader rdr = cmd.ExecuteReader()) {
                    while (rdr.Read()) {
                        var rl = new RegionLayer();
                        rl.Id = (int) rdr["AlueTaso_ID"];
                        rl.Name = rdr["AlueTasoKuvaus"].ToString();
                        result.Add(rl);
                    }
                }
            }
            return result.ToArray();
        }

        public RegionLayer[] GetRegionLayers(int id)
        {
            using (DbConnection db = this.GetDbConnection()) {
                return this.GetRegionLayers(db, id);
            }
        }

        public string[] GetYears(DbConnection db, int id)
        {
            var result = new List<string>();
            string sqlString = @"
SELECT
    DISTINCT Jakso_ID
FROM
    FactTilastoArvo 
WHERE
    Tilasto_ID = @id
";

            using (DbCommand cmd = db.CreateCommand()) {
                DbParameter param;
                cmd.CommandText = sqlString;

                param = cmd.CreateParameter();
                param.DbType = DbType.Int32;
                param.ParameterName = "@id";
                param.Value = id;
                cmd.Parameters.Add(param);

                using (DbDataReader rdr = cmd.ExecuteReader()) {
                    while (rdr.Read()) {
                        result.Add(rdr["Jakso_ID"].ToString());
                    }
                }
            }
            return result.ToArray();
        }

        public string[] GetYears(int id)
        {
            using (DbConnection db = this.GetDbConnection()) {
                return this.GetYears(db, id);
            }
        }

        public List<Models.StatisticIndexBrief>
            GetStatisticIndexBriefsByName(string searchString)
        {
            string sqlString = @"
SELECT
    *
FROM
    statisticIndex idx,
    DimTilasto tilasto
WHERE
    idx.statisticID = tilasto.Tilasto_ID AND
    (theme1 LIKE @searchString OR
    theme2 LIKE @searchString OR
    theme3 LIKE @searchString OR
    theme4 LIKE @searchString OR
    theme5 LIKE @searchString OR
    statisticName LIKE @searchString OR
    (theme1 + theme2 + theme3 + theme4 + theme5 + statisticName) LIKE @searchString)
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
	statisticIndex idx,
	DimTilasto tilasto
WHERE
    idx.statisticID = tilasto.Tilasto_ID AND
	idx.statisticID = @id
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
                        result = factory2.GetStatisticIndexDetails(
                            brief,
                            rdr,
                            this.GetYears(db, brief.Id),
                            this.GetRegionLayers(db, brief.Id));
                        return result;
                    }
                }
            }
        }
    }
}