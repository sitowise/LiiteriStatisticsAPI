using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

namespace LiiteriStatisticsCore.Repositories
{
    public class IndicatorDetailsRepository :
        SqlReadWriteRepository<Models.IndicatorDetails>
    {
        public IndicatorDetailsRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public override IEnumerable<Models.IndicatorDetails>
            FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorDetailsFactory());
        }

        public override Models.IndicatorDetails Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorDetailsFactory()).Single();
        }

        public override Models.IndicatorDetails First(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorDetailsFactory()).First();
        }

        public override void Update(Models.IndicatorDetails entity)
        {
            string queryString = @"
MERGE INTO
	[{0}]..[Themes_Statistics] T
USING (
	SELECT
		*
	FROM
		(VALUES (@ThemeId, @StatisticsId)) Dummy(theme_id, statistics_id)) S
ON
	(T.statistics_id = S.statistics_id)
WHEN
	NOT MATCHED THEN
	INSERT VALUES (theme_id, statistics_id)
WHEN
	MATCHED THEN
	UPDATE SET
		T.theme_id = S.theme_id
;
";
            queryString = string.Format(
                queryString,
                ConfigurationManager.AppSettings["DbDataIndex"]);

            using (DbCommand cmd = dbConnection.CreateCommand()) {
                cmd.Parameters.Add(new SqlParameter(
                    "@ThemeId", entity.ThemeId));
                cmd.Parameters.Add(new SqlParameter(
                    "@StatisticsId", entity.Id));
                cmd.CommandText = queryString;
                cmd.ExecuteNonQuery();
            }
        }
    }
}