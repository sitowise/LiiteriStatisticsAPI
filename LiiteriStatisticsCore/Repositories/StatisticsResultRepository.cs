using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Repositories
{
    public class StatisticsResultRepository :
        SqlReadRepository<Models.StatisticsResult>
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public StatisticsResultRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public IEnumerable<Models.StatisticsResult> FindAll(
            IEnumerable<Queries.ISqlQuery> queries)
        {
            foreach (Queries.ISqlQuery query in queries) {
                foreach (Models.StatisticsResult r in this.FindAll(query)) {
                    yield return r;
                }
            }
        }

        public override IEnumerable<Models.StatisticsResult>
            FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.StatisticsResultFactory());
        }

        public override Models.StatisticsResult Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query).Single();
        }

        public override Models.StatisticsResult First(Queries.ISqlQuery query)
        {
            return this.FindAll(query).First();
        }
    }
}