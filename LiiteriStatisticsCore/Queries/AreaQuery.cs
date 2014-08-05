using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Queries
{
    public class AreaQuery : SqlQuery, ISqlQuery
    {
        private LiiteriStatisticsCore.Util.AreaTypeMappings
            AreaTypeMappings = new LiiteriStatisticsCore.Util.AreaTypeMappings();

        public AreaQuery() : base()
        {
        }

        public string AreaTypeIdIs { get; set; }

        public override string GetQueryString()
        {
            string queryString = AreaTypeMappings.GetDatabaseListQuery(this.AreaTypeIdIs);

            if (queryString == null || queryString.Length == 0) {
                throw new Exception("No area list available for this area type!");
            }

            return queryString;
        }
    }
}