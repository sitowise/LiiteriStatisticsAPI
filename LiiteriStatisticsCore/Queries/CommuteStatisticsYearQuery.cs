using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Queries
{
    public class CommuteStatisticsYearQuery : SqlQuery, ISqlQuery
    {
        public string statisticsTable { get; set; }

        // private, since it's a required constructor argument
        private string TableNameIs
        {
            get
            {
                return (string) this.Parameters["TableNameIs"].Value;
            }
            set
            {
                this.Parameters.Add("TableNameIs", value);
            }
        }

        public CommuteStatisticsYearQuery(string tableName)
        {
            this.TableNameIs = tableName;
        }

        public override string GetQueryString()
        {
            string sqlString = @"
SELECT
    TJ.Jakso_ID AS Year,
    TL.Tietolahde AS DataSource
FROM
    Apu_TyomatkaTOLTallennusJakso TJ

    INNER JOIN FactTyomatkaAnalyysiTietolahde TL ON
        TJ.Jakso_ID >= TL.Alkaen_Jakso_ID
WHERE
    TJ.Taulu = @TableNameIs
";
            return sqlString;
        }
    }
}