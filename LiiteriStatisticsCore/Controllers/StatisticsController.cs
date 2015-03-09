using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

using System.Runtime.Serialization; // DataContract / DataMember
using System.ServiceModel; // WCF

namespace LiiteriStatisticsCore.Controllers
{
    [ServiceContract]
    public interface IStatisticsController
    {
        [OperationContract]
        IEnumerable<Models.StatisticsResult> GetStatistics(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null);

        [OperationContract]
        string GetStatisticsDebugString(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null);

        [OperationContract]
        IEnumerable<Models.AreaType> GetAreaTypes();

        [OperationContract]
        IEnumerable<Models.Area> GetAreas(string areaTypeId);
    }

    public class StatisticsController : IStatisticsController
    {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Util.AreaTypeMappings
            AreaTypeMappings = new Util.AreaTypeMappings();

        private class StatisticsResultContainer
        {
            public string DebugString;
            public IEnumerable<Models.StatisticsResult> Results;
        }

        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        /* be prepared to return either a debug string, or the actual
         * results */
        private StatisticsResultContainer GetStatisticsResultContainer(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null,
            bool debug = false)
        {
            using (DbConnection db = this.GetDbConnection()) {

                /* Step 1: Fetch IndicatorDetails */

                var indicatorQuery = new Queries.IndicatorQuery();
                indicatorQuery.IdIs = statisticsId;

                var indicatorDetailsRepository =
                    new Repositories.IndicatorDetailsRepository(db);
                var details = (Models.IndicatorDetails)
                    indicatorDetailsRepository.Single(indicatorQuery);

                /* Step 2: Create one or more StatisticsQuery objects */
                var queries = new List<Queries.StatisticsQuery>();

                /* For privacy limits, we do some extra stuff */
                Queries.IndicatorQuery refIndicatorQuery;
                Models.IndicatorDetails refDetails = null;

                IList<Tuple<Queries.ISqlQuery, Queries.ISqlQuery>>
                    querypairs = null;

                if (details.PrivacyLimit != null) {
                    refIndicatorQuery = new Queries.IndicatorQuery();
                    refIndicatorQuery.IdIs = details.PrivacyLimit.RefId;
                    refDetails = (Models.IndicatorDetails)
                        indicatorDetailsRepository.Single(refIndicatorQuery);
                    querypairs =
                        new List<Tuple<Queries.ISqlQuery, Queries.ISqlQuery>>();
                }

                /* although StatisticsQuery could implement .YearIn, which 
                 * would accept a list of years, what about if different years
                 * end up having different DatabaseAreaTypes?
                 * For this reason, let's just loop the years and create
                 * multiple queries */
                foreach (int year in years) {
                    Models.TimePeriod timePeriod = (
                        from p in details.TimePeriods
                        where p.Id == year
                        select p).Single();
                    int[] availableAreaTypes = (
                        from a in timePeriod.DataAreaTypes
                        select a.Id).ToArray();

                    var statisticsQuery = new Queries.StatisticsQuery(statisticsId);

                    statisticsQuery.CalculationTypeIdIs = details.CalculationType;
                    statisticsQuery.AvailableAreaTypes = availableAreaTypes;

                    if (group == null) group = "finland";
                    statisticsQuery.GroupByAreaTypeIdIs = group;
                    statisticsQuery.YearIs = year;

                    if (filter != null && filter.Length == 0) {
                        filter = null;
                    }
                    statisticsQuery.AreaFilterQueryString = filter;

                    if (details.PrivacyLimit == null) {
                        queries.Add(statisticsQuery);
                    } else {
                        /* For privacy limits, we need to do a parallel
                         * query and compare the results side-by-side
                         * in the repository */
                        var refQuery = new Queries.StatisticsQuery(
                            details.PrivacyLimit.RefId);
                        /* TODO: implement cloning?
                         * (StatisticsQuery state may be too uncertain, though)
                         */
                        refQuery.CalculationTypeIdIs =
                            refDetails.CalculationType;

                        Models.TimePeriod refTimePeriod = (
                            from p in refDetails.TimePeriods
                            where p.Id == year
                            select p).Single();
                        int[] refAvailableAreaTypes = (
                            from a in refTimePeriod.DataAreaTypes
                            select a.Id).ToArray();

                        refQuery.AvailableAreaTypes = refAvailableAreaTypes;

                        refQuery.GroupByAreaTypeIdIs =
                            statisticsQuery.GroupByAreaTypeIdIs;
                        refQuery.YearIs = statisticsQuery.YearIs;
                        refQuery.AreaFilterQueryString =
                            statisticsQuery.AreaFilterQueryString;
                        querypairs.Add(
                            new Tuple<Queries.ISqlQuery, Queries.ISqlQuery>(
                                statisticsQuery, refQuery));
                    }
                }

                if (debug) {
                    Util.DebugOutput debugOutput;
                    if (queries.Count > 0) {
                        debugOutput = new Util.DebugOutput(queries);
                    } else if (querypairs.Count > 0) {
                        debugOutput = new Util.DebugOutput(querypairs);
                    } else {
                        throw new Exception("No statistics queries specified!");
                    }
                    return new StatisticsResultContainer() {
                        DebugString = debugOutput.ToString(),
                        Results = null
                    };
                    /* We could continue here and fill the actual results,
                     * but if there's an error we might rather just want
                     * to see the query */
                }

                /* Step 3: Fetch StatisticsResult */

                var repository = new Repositories.StatisticsResultRepository(db);
                /* when IndictorDetails is passed to the repository, it will
                 * know how to do unit conversions */
                repository.Indicator = details;

                IEnumerable<Models.StatisticsResult> results;
                if (queries.Count > 0) {
                    results = repository.FindAll(queries);
                } else if (querypairs.Count > 0) {
                    results = repository.FindAll(querypairs);
                } else {
                    throw new Exception("No statistics queries specified!");
                }

                /* Note: we are iterating the generator here, could be
                 * memory-inefficient */
                return new StatisticsResultContainer() {
                    DebugString = null,
                    Results = results.ToList()
                };
            }
        }

        public virtual IEnumerable<Models.StatisticsResult> GetStatistics(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null)
        {
            return this.GetStatisticsResultContainer(
                years,
                statisticsId,
                group,
                filter,
                false).Results;
        }

        public string GetStatisticsDebugString(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null)
        {
            return this.GetStatisticsResultContainer(
                years,
                statisticsId,
                group,
                filter,
                true).DebugString;
        }

        public virtual IEnumerable<Models.AreaType> GetAreaTypes()
        {
            return AreaTypeMappings.GetAreaTypes();
        }

        public virtual IEnumerable<Models.Area> GetAreas(string areaTypeId)
        {
            var query = new Queries.AreaQuery();
            query.AreaTypeIdIs = areaTypeId;
            using (DbConnection db = this.GetDbConnection()) {
                var repository = new Repositories.AreaRepository(db);
                foreach (Models.Area r in repository.FindAll(query)) {
                    yield return r;
                }
            }
        }
    }
}