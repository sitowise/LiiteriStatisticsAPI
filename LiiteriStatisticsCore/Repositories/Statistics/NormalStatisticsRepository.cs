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
        SqlReadRepository<Models.StatisticsResult>,
        IStatisticsRepository
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Util.AreaTypeMappings
            AreaTypeMappings = new Util.AreaTypeMappings();

        public Models.StatisticsRepositoryTracer Tracer;

        public bool MaySkipPrivacyLimits
        {
            get
            {
                foreach (var query in this.queries) {
                    if (!(query is Queries.StatisticsQuery)) {
                        logger.Warn(
                            "NormalStatisticsRepository contains a query that is not StatisticsQuery: " +
                            query.GetType().ToString());
                        continue;
                    }

                    /* SUPPORT-14527 / YM-654
                     * if any of the queries use a functional
                     * databaseAreaType (such as a grid), then privacy limits
                     * should not be skipped */
                    int id = ((Queries.StatisticsQuery) query).GetDatabaseAreaTypeId();
                    var category = AreaTypeMappings.GetAreaTypeCategory(id);
                    if (category != Util.AreaTypeMappings.AreaTypeCategory.AdministrativeArea) {
                        return false;
                    }
                }

                return true;
            }
        }

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