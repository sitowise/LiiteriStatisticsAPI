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
    public class StatisticsResultFactory : BaseFactory
    {
        public IEnumerable<Models.StatisticsResult> GetStatisticsResults(
            int id,
            string[] years,
            int calctype)
        {
            var results = new List<Models.StatisticsResult>();

            string sqlString = @"
SELECT
	ku.Alue_ID AS regionID,
	ku.Nimi AS municipalityName,
	fta.Jakso_ID AS year,
	{0}
FROM
	FactTilastoArvo fta,
	DimAlue da_alue,
	DimAlue da_kunta,
	DimKunta ku
WHERE
	fta.Tilasto_ID = @id AND
	da_alue.Alue_ID = fta.Alue_ID AND
	da_alue.Kunta_Alue_ID = da_kunta.Alue_ID AND
	ku.Alue_ID = da_kunta.Alue_ID AND
	fta.Jakso_ID IN ('2011', '2012', '2013')
GROUP BY
	ku.Nimi,
	ku.Alue_ID,
	fta.Jakso_ID
ORDER BY
	municipalityName
";

            string aggr = "NULL AS value";
            switch (calctype) {
                case 1:
                    aggr = "SUM(fta.Arvo) AS value";
                    break;
                case 2:
                    aggr = "0 AS value";
                    //throw new NotImplementedException("Not implemented: Aputilasto");
                    break;
                case 3:
                    aggr = "0 AS value";
                    //throw new NotImplementedException("Not implemented: Johdettu tilasto - jakamalla laskettava");
                    break;
                case 4:
                    aggr = "0 AS value";
                    //throw new NotImplementedException("Not implemented: Johdettu tilasto - erikseen laskettava");
                    break;
                case 5:
                    aggr = "0 AS value";
                    //throw new NotImplementedException("Not implemented: Johdettu tilasto - summaamalla laskettava");
                    break;
            }

            sqlString = string.Format(sqlString, aggr);

            Models.StatisticsResult result;
            using (DbConnection db = this.GetDbConnection()) {
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
                            result = this.GetStatisticsResult(rdr);
                            results.Add(result);
                        }
                    }
                }
            }

            return results;
        }

        public Models.StatisticsResult GetStatisticsResult(DbDataReader rdr)
        {
            Models.StatisticsResult result = new Models.StatisticsResult();
            result.RegionID = (int) rdr["regionID"];
            result.MunicipalityName = (string) rdr["municipalityName"].ToString();
            result.Year = (string) rdr["year"].ToString();
            result.value = rdr["value"];
            return result;
        }
    }
}