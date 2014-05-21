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
            string year,
            int calctype)
        {
            var results = new List<Models.StatisticsResult>();

            string sqlString = null;

            string sqlString_normal = @"
DECLARE @AlueTaso_ID INT = 2
SELECT
	K.Alue_ID AS regionID,
	K.Nimi AS municipalityName,
	K.Nro AS municipalityID,
	T.Jakso_ID AS year,
	SUM(T.Arvo) AS value
FROM
	FactTilastoArvo T
		INNER JOIN DimAlue A ON
			A.Alue_ID = T.Alue_ID AND
			@year BETWEEN A.Alkaen_Jakso_ID AND A.Asti_JAKSO_ID
		INNER JOIN DimKunta K ON
			K.Alue_ID = A.Kunta_Alue_ID
WHERE
	T.Tilasto_ID = @id AND
	T.Jakso_ID = @year
GROUP BY
	K.Nimi,
	K.Nro,
	K.Alue_ID,
	T.Jakso_ID
ORDER BY
	municipalityName;
";

            string sqlString_derived_summed = @"
DECLARE @AlueTaso_ID INT = 2
SELECT
	K.Alue_ID AS regionID,
	K.Nimi AS municipalityName,
	K.Nro AS municipalityID,
	T.Jakso_ID AS year,
	SUM(T.Arvo) AS value
FROM
	DimTilasto_JohdettuTilasto_Summa Tjts
		INNER JOIN FactTilastoarvo T ON
			T.Tilasto_ID = Tjts.Yhteenlaskettava_Tilasto_ID AND
			T.Jakso_ID = @year
		INNER JOIN DimAlue A ON
			A.Alue_ID = T.Alue_ID AND
			@year BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID AND
			A.AlueTaso_ID = @AlueTaso_ID
		INNER JOIN DimKunta K ON
			K.Alue_ID = A.Kunta_Alue_ID
WHERE
	Tjts.Tilasto_ID = @id
GROUP BY
	K.Nimi,
	K.Nro,
	K.Alue_ID,
	T.Jakso_ID
ORDER BY
	municipalityName;
";

            switch (calctype) {
                case 5: /* Johdettu tilasto - summaamalla laskettava */
                    sqlString = sqlString_derived_summed;
                    break;
                case 1:
                    sqlString = sqlString_normal;
                    break;
                default:
                    throw new Exception("Calculation type not implemented!");
            }


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

                    param = cmd.CreateParameter();
                    param.DbType = DbType.String;
                    param.ParameterName = "@year";
                    param.Value = year;
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
            result.MunicipalityId = (string) rdr["municipalityID"].ToString();
            result.Year = (string) rdr["year"].ToString();
            result.Value = rdr["value"];
            return result;
        }
    }
}