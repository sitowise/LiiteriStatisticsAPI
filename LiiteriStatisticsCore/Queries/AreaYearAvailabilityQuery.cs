using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Queries
{
    public class AreaYearAvailabilityQuery : SqlQuery, ISqlQuery
    {
        private static Util.AreaTypeMappings
            AreaTypeMappings = new Util.AreaTypeMappings();

        public AreaYearAvailabilityQuery(string areaTypeId) : base()
        {
            this.AreaTypeId = areaTypeId;
        }

        private string AreaTypeId { get; set; }

        public override string GetQueryString()
        {
            var schema = AreaTypeMappings.GetDatabaseSchema(this.AreaTypeId);
            if (schema["AreaYearAvailabilityField"] == null) {
                throw new Exception("Specified AreaType has no AreaYearAvailabilityField");
            }

            string sqlString = @";
SELECT
    Jakso_ID AS Year
FROM
    Apu_AlueTallennusJakso ATJ
WHERE
    {0} = 1
";
            sqlString = string.Format(
                sqlString,
                string.Format(schema["AreaYearAvailabilityField"], "ATJ"));

            return sqlString;
        }
    }
}