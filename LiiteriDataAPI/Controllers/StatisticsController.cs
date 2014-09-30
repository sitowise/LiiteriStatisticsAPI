using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using System.Data.Common;

using System.Diagnostics;
using System.Configuration;

using LiiteriStatisticsCore.Util;
using LiiteriStatisticsCore.Models;
using LiiteriStatisticsCore.Queries;
using LiiteriStatisticsCore.Repositories;

/* These are for V0 only, and will be replaced */

namespace LiiteriDataAPI.Controllers
{
    public class StatisticController : ApiController
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        private static AreaTypeMappings
            AreaTypeMappings = new AreaTypeMappings();

        [Route("v1/statistics/{statisticsId}/")]
        [HttpGet]
        public HttpResponseMessage GetStatisticsV1(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null,
            bool debug = false)
        {
            using (DbConnection db = this.GetDbConnection()) {

                /* Step 1: Fetch IndicatorDetails */

                var indicatorQuery = new IndicatorQuery();
                indicatorQuery.IdIs = statisticsId;

                var indicatorDetailsRepository = new IndicatorDetailsRepository(db);
                var details = (IndicatorDetails)
                    indicatorDetailsRepository.Single(indicatorQuery);

                /* Step 2: Create one or more StatisticsQuery objects */
                var queries = new List<StatisticsQuery>();

                /* For privacy limits, we do some extra stuff */
                IndicatorQuery refIndicatorQuery;
                IndicatorDetails refDetails = null;
                IList<Tuple<ISqlQuery, ISqlQuery>> querypairs = null;

                if (details.PrivacyLimit != null) {
                    refIndicatorQuery = new IndicatorQuery();
                    refIndicatorQuery.IdIs = details.PrivacyLimit.RefId;
                    refDetails = (IndicatorDetails)
                        indicatorDetailsRepository.Single(refIndicatorQuery);
                    querypairs = new List<Tuple<ISqlQuery, ISqlQuery>>();
                }

                /* although StatisticsQuery could implement .YearIn, which 
                 * would accept a list of years, what about if different years
                 * end up having different DatabaseAreaTypes?
                 * For this reason, let's just loop the years and create
                 * multiple queries */
                foreach (int year in years) {
                    TimePeriod timePeriod = (
                        from p in details.TimePeriods
                        where p.Id == year
                        select p).Single();
                    int[] availableAreaTypes = (
                        from a in timePeriod.DataAreaTypes
                        select a.Id).ToArray();

                    var statisticsQuery = new StatisticsQuery(statisticsId);

                    statisticsQuery.CalculationTypeIdIs = details.CalculationType;
                    statisticsQuery.AvailableAreaTypes = availableAreaTypes;

                    if (group == null) group = "finland";
                    statisticsQuery.GroupByAreaTypeIdIs = group;
                    statisticsQuery.YearIs = year;
                    statisticsQuery.AreaFilterQueryString = filter;

                    if (details.PrivacyLimit == null) {
                        queries.Add(statisticsQuery);
                    } else {
                        /* For privacy limits, we need to do a parallel
                         * query and compare the results side-by-side
                         * in the repository */
                        StatisticsQuery refQuery = new StatisticsQuery(
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
                        querypairs.Add(new Tuple<ISqlQuery, ISqlQuery>(
                            statisticsQuery, refQuery));
                    }
                }

                /* this debug output is the reason we've declared the
                 * entire controller as HttpResponseMessage */
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

                /* Step 3: Fetch StatisticsResult */

                var repository = new StatisticsResultRepository(db);
                /* when IndictorDetails is passed to the repository, it will
                 * know how to do unit conversions */
                repository.Indicator = details;

                IEnumerable<StatisticsResult> results;
                if (queries.Count > 0) {
                    results = repository.FindAll(queries);
                } else if (querypairs.Count > 0) {
                    results = repository.FindAll(querypairs);
                } else {
                    throw new Exception("No statistics queries specified!");
                }

                /* Note: we are iterating the generator here, could be
                 * memory-inefficient */
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    results.ToList());
            }
        }

        [Route("v1/areaTypes/")]
        [HttpGet]
        public IEnumerable<AreaType> GetAreaTypesV1()
        {
            return AreaTypeMappings.GetAreaTypes();
        }

        [Route("v1/areaTypes/{areaTypeId}/areas/")]
        [HttpGet]
        public IEnumerable<Area> GetAreasV1(string areaTypeId)
        {
            var query = new AreaQuery();
            query.AreaTypeIdIs = areaTypeId;
            using (DbConnection db = this.GetDbConnection()) {
                var repository = new AreaRepository(db);
                foreach (Area r in repository.FindAll(query)) {
                    yield return r;
                }
            }
        }
    }
}