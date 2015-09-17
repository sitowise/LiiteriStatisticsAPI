using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Queries
{
    public class IndicatorSubQuery : SqlQuery, ISqlQuery
    {
        private List<string> whereList;

        public enum SubQueryTypes {
            DerivedSummedStatistics,
            DerivedDividedStatistics,
        };

        public SubQueryTypes SubQueryType { get; set; }

        public int? IdIs
        {
            get
            {
                return (int) this.Parameters["IdIs"].Value;
            }
            set
            {
                if (value == null) return;
                this.Parameters.Add("IdIs", value);
            }
        }

        public IndicatorSubQuery() : base()
        {
            this.whereList = new List<string>();
        }

        public override string GetQueryString()
        {
            string queryString = null;
            switch (this.SubQueryType) {
                case SubQueryTypes.DerivedDividedStatistics:
                    queryString = this.GetQueryString_DerivedDividedStatistics();
                    break;
                case SubQueryTypes.DerivedSummedStatistics:
                    queryString = this.GetQueryString_DerivedSummedStatistics();
                    break;
                default:
                    throw new NotImplementedException();
            }
            return queryString;
        }

        private string GetQueryString_DerivedDividedStatistics()
        {
            string sqlString = @"
SELECT
    Nimittaja_Tilasto_ID AS Value
FROM
    DimTilasto_JohdettuTilasto_Jako
WHERE
    Tilasto_ID = @IdIs

UNION ALL

SELECT
    Osoittaja_Tilasto_ID AS Value
FROM
    DimTilasto_JohdettuTilasto_Jako
WHERE
    Tilasto_ID = @IdIs
";
            return sqlString;
        }

        private string GetQueryString_DerivedSummedStatistics()
        {
            string sqlString = @"
SELECT
    Yhteenlaskettava_Tilasto_ID AS Value
FROM
    DimTilasto_JohdettuTilasto_Summa
WHERE
    Tilasto_ID = @IdIs AND
    Ryhma_SEQ = 0
";
            return sqlString;
        }
    }
}