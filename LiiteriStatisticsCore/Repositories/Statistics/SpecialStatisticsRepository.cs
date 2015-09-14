using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiiteriStatisticsCore.Queries;

namespace LiiteriStatisticsCore.Repositories
{
    /* This is empty, but still useful since it provides a distinct name
     * in the repository tracer */
    class SpecialStatisticsRepository : NormalStatisticsRepository
    {
        public SpecialStatisticsRepository(
            DbConnection dbConnection,
            IEnumerable<ISqlQuery> queries) : base(dbConnection, queries)
        {
        }
    }
}