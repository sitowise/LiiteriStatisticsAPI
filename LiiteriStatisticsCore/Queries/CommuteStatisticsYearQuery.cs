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
    Jakso_ID AS Year
FROM
    Apu_TyomatkaTOLTallennusJakso
WHERE
    Taulu = @TableNameIs
";
            return sqlString;
        }
    }
}