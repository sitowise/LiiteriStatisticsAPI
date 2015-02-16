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
            string filter = null,
            bool debug = false);

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

        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        public virtual IEnumerable<Models.StatisticsResult> GetStatistics(
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
                        refQuery.AvailableAreaTypes =
                            statisticsQuery.AvailableAreaTypes;
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

                /* this debug output is the reason we've declared the
                 * entire controller as HttpResponseMessage */
                /*
                TODO: HOWTODO
                if (debug) {
                    DebugOutput debugOutput;
                    if (queries.Count > 0) {
                        debugOutput = new DebugOutput(queries);
                    } else if (querypairs.Count > 0) {
                        debugOutput = new DebugOutput(querypairs);
                    } else {
                        throw new Exception("No statistics queries specified!");
                    }
                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        debugOutput.ToString(),
                        new Formatters.TextPlainFormatter());
                }
                */

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
                return results.ToList();
                /*
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    results.ToList());
                */
            }
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