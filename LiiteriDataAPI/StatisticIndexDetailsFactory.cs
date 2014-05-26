using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Data;

namespace LiiteriDataAPI
{
    public class StatisticIndexDetailsFactory : BaseFactory
    {
        public RegionType[] GetRegionLayers(DbConnection db, int id)
        {
            var result = new List<RegionType>();
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
                        var rl = new RegionType();
                        rl.Id = (int) rdr["AlueTaso_ID"];
                        rl.Name = rdr["AlueTasoKuvaus"].ToString();
                        result.Add(rl);
                    }
                }
            }
            return result.ToArray();
        }

        public RegionType[] GetRegionLayers(int id)
        {
            using (DbConnection db = this.GetDbConnection()) {
                return this.GetRegionLayers(db, id);
            }
        }

        public Models.StatisticIndexDetails
            GetStatisticIndexDetailsById(int id)
        {
            string sqlString1 = @"
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
                        var factory2 = new StatisticIndexDetailsFactory();
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

        public string[] GetYears(DbConnection db, int id)
        {
            var result = new List<string>();
            string sqlString = @"
SELECT
	DISTINCT Jakso_ID
FROM
	Apu_TilastoTallennusJakso J
WHERE
	J.Tilasto_ID = 8003 AND
	J.AlueTaso_ID = 2;
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

        public Models.StatisticIndexDetails GetStatisticIndexDetails(
            Models.StatisticIndexBrief brief,
            DbDataReader rdr,
            string[] years,
            RegionType[] regionLayers) /* reader for metadata */
        {
            Models.StatisticIndexDetails result = new Models.StatisticIndexDetails();
            result.Id = brief.Id;
            result.Name = brief.Name;

            result.Group = rdr["statisticGroup"] as int? ?? default(int);
            result.Unit = (string) rdr["unit"].ToString();
            //result.StatisticId = rdr["statisticId"] as int? ?? default(int);
            result.ProcessingStage = (string) rdr["processingStage"].ToString();
            result.TimeSpan = (string) rdr["timeSpan"].ToString();
            result.DecimalCount = rdr["EsitysDesimaaliTarkkuus"] as int? ?? default(int);

            result.DisplayUnitID = (int) rdr["MittayksikkoEsitys_Mittayksikko_ID"];
            result.InternalUnitID = (int) rdr["MittayksikkoTallennus_Mittayksikko_ID"];

            result.Themes = brief.Themes;

            result.Description = (string) rdr["Kuvaus"].ToString();
            result.AdditionalInformation = (string) rdr["Lisatieto"].ToString();

            result.CalculationType = (int) rdr["TilastoLaskentaTyyppi_ID"];

            result.Years = years;

            //result.RegionLayers = regionLayers;

            return result;
        }
    }
}