using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Queries
{
    public class AreaTypeQuery : SqlQuery, ISqlQuery
    {
        public AreaTypeQuery()
            : base()
        {
        }

        public override string GetQueryString()
        {
            return @"
SELECT
    AT.AlueTaso_ID AS AreaTypeId,
    AT.AlueTasoKuvaus AS Description
FROM
    DimAlueTaso AT
";
        }
    }
}
