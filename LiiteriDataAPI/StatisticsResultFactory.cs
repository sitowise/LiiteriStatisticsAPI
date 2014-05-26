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
            Models.StatisticIndexDetails details)
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
			@year BETWEEN A.Alkaen_Jakso_ID AND A.Asti_JAKSO_ID AND
			A.AlueTaso_ID = @AlueTaso_ID
		INNER JOIN DimKunta K ON
			K.Alue_ID = A.Kunta_Alue_ID
WHERE
	T.Tilasto_ID = @id AND
	T.Jakso_ID = @year AND
	T.Arvo IS NOT NULL
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
	Tjts.Tilasto_ID = @id AND
    Tjtjs.Ryhma_SEQ = 0
GROUP BY
	K.Nimi,
	K.Nro,
	K.Alue_ID,
	T.Jakso_ID
ORDER BY
	municipalityName;
";

            string sqlString_derived_divided = @"
DECLARE @AlueTaso_ID INT = 2
SELECT
	(SUM(T1.Arvo) / SUM(T2.Arvo)) AS Value,
	K.Nimi AS municipalityName,
	K.Alue_ID AS regionID,
	K.Nro AS municipalityId,
	T1.Jakso_ID AS year
FROM
	DimTilasto_JohdettuTilasto_Jako J
		LEFT JOIN FactTilastoarvo T1 ON
			T1.Tilasto_ID = J.Osoittaja_Tilasto_ID AND
			T1.Jakso_ID = @year AND
			T1.AlueTaso_ID = @AlueTaso_ID
		LEFT JOIN FactTilastoarvo T2 ON
			T2.Tilasto_ID = J.Nimittaja_Tilasto_ID AND
			T2.Jakso_ID = @year AND
			T2.AlueTaso_ID = @AlueTaso_ID

		INNER JOIN DimAlue A ON
			A.Alue_ID = T1.Alue_ID AND
			A.Alue_ID = T2.Alue_ID AND
			@year BETWEEN A.Alkaen_Jakso_ID AND A.Asti_Jakso_ID AND
			A.AlueTaso_ID = @AlueTaso_ID
		INNER JOIN DimKunta K ON
			K.Alue_ID = A.Kunta_Alue_ID
WHERE
	J.Tilasto_ID = @id
GROUP BY
	K.Nimi,
	K.Nro,
	K.Alue_ID,
	T1.Jakso_ID
ORDER BY
	K.Nimi
";

            switch (details.CalculationType) {
                case 3: /* Johdettu tilasto - jakamalla laskettava */
                    sqlString = sqlString_derived_divided;
                    break;
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
                            this.ConvertStatisticValue(result, details);
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

        public void ConvertStatisticValue(
            Models.StatisticsResult statResult,
            Models.StatisticIndexDetails details)
        {
            /* we appear to have the following cases in db:
             * 10, 14 (m2 -> ha)
             * 12, 1 (osuus -> %)
             * 6,7 (kpl -> lkm)
             * 15,18 (e -> unknown)
             */
            switch (details.InternalUnitID) {
                case 12: // osuus
                    switch (details.DisplayUnitID) {
                        case 1: // %
                            statResult.Value = (double) statResult.Value * 100;
                            break;
                    }
                    break;
                case 10: // m2
                    switch (details.DisplayUnitID) {
                        case 14: // ha
                            statResult.Value = (double) statResult.Value / 100;
                            break;
                    }
                    break;
            }
        }
    }
}