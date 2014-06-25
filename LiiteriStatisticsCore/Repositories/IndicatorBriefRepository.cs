using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public class IndicatorBriefRepository :
        SqlReadRepository<Models.IndicatorBrief>
    {
        public IndicatorBriefRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public override IEnumerable<Models.IndicatorBrief>
            FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorBriefFactory());
        }

        public override Models.IndicatorBrief Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorBriefFactory()).Single();
        }

        public override Models.IndicatorBrief First(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorBriefFactory()).First();
        }
    }
}