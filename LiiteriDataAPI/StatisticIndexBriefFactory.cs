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
    }
}