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

        public StatisticsResultRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries) :
            base(dbConnection, queries, new Factories.StatisticsResultFactory())
        {
        }

        public override IEnumerable<Models.StatisticsResult> FindAll()
        {
            return base.FindAll();
        }

        public override Models.StatisticsResult Single()
        {
            return this.FindAll().Single();
        }

        public override Models.StatisticsResult First()
        {
            return this.FindAll().First();
        }
    }
}