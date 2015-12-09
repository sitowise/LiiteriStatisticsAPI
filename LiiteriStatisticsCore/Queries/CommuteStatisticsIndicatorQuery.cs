using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Queries
{
    public class CommuteStatisticsIndicatorQuery : SqlQuery, ISqlQuery
    {
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

        private int? _StatisticsId = null;
        public int? StatisticsId
        {
            get
            {
                return this._StatisticsId;
            }
            set
            {
                this._StatisticsId = value;
                this.TableNameIs = Models.CommuteStatisticsIndicator
                    .TableNameIdMapping.Single(a => a.Item2 == value).Item1;
            }
        }

        private List<string> whereList = new List<string>();

        public CommuteStatisticsIndicatorQuery() : base()
        {
        }

        public override string GetQueryString()
        {
            string whereString = "";
            if (this.whereList.Count > 0) {
                whereString = string.Format(
                    "WHERE\n    {0}",
                    string.Join(" AND ", this.whereList));
            }

            string sqlString = @"
SELECT
    K.Taulu AS TableName,
    K.Nimi AS Name,
    K.Lisatieto AS Description,
    K.TietosuojaSelite AS PrivacyDescription,
    AV.AjallinenVaiheLyhenne AS TimeSpan,
    AV.AjallinenVaiheKuvaus AS TimeSpanDescription,
    Y.MittayksikkoLyhenne AS Unit
FROM
    DimTyomatkaAnalyysiKuvaus K
    INNER JOIN DimAjallinenVaihe AV ON
        AV.AjallinenVaihe_ID = K.AjallinenVaihe_ID
    INNER JOIN DimMittayksikko Y ON
        Y.Mittayksikko_ID = K.Mittayksikko_ID
{0}
";
            sqlString = string.Format(sqlString, whereString);

            return sqlString;
        }
    }
}