using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Common;

namespace LiiteriDataAPI
{
    public class StatisticIndexBriefFactory : BaseFactory
    {
        public List<Models.StatisticIndexBrief>
            GetStatisticIndexBriefsByName(string searchString)
        {
            string sqlString = @"
SELECT
    tilasto.Tilasto_ID,
	theme1,
	theme2,
	theme3,
	theme4,
	theme5,
	idx.statisticName,
	COUNT(ja.Jakso_ID) jakso_count
FROM
    statisticIndex idx,
    DimTilasto tilasto,
	Apu_TilastoTallennusJakso ja
WHERE
    idx.statisticID = tilasto.Tilasto_ID AND
	ja.Tilasto_ID = tilasto.Tilasto_ID AND
	ja.AlueTaso_ID = 2 AND
    tilasto.TilastoLaskentatyyppi_ID <> 2 AND
    (theme1 LIKE @searchString OR
    theme2 LIKE @searchString OR
    theme3 LIKE @searchString OR
    theme4 LIKE @searchString OR
    theme5 LIKE @searchString OR
    statisticName LIKE @searchString OR
    (theme1 + theme2 + theme3 + theme4 + theme5 + statisticName) LIKE @searchString)
GROUP BY
	tilasto.Tilasto_ID,
	theme1,
	theme2,
	theme3,
	theme4,
	theme5,
	idx.statisticName
ORDER BY
    theme1,
    theme2,
    theme3,
    theme4,
    theme5,
    idx.statisticName
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
                            if ((int) rdr["jakso_count"] == 0) {
                                continue;
                            }
                            Models.StatisticIndexBrief result =
                                factory.GetStatisticIndexResult(rdr);
                            results.Add(result);
                        }
                    }
                }
            }

            return results;
        }
    }
}