using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Diagnostics;
using System.Collections;

namespace LiiteriStatisticsCore.Repositories
{
    public class NormalStatisticsRepository :
        SqlReadRepository<Models.StatisticsResult>
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Models.StatisticsRepositoryTracer Tracer;

        public NormalStatisticsRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries) :
            base(dbConnection, queries, new Factories.StatisticsResultFactory())
        {
        }

        public override IEnumerable<Models.StatisticsResult> FindAll()
        {
            if (this.Tracer != null) {
                this.Tracer.QueryDetails = this.queryDetails;
            }
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