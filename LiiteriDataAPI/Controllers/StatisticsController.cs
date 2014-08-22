using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using System.Data.Common;

using System.Xml;
using System.Xml.Linq;

using System.Diagnostics;

using System.Configuration;

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

        [Route("v0/indicators/")]
        [HttpGet]
        public IEnumerable<Models.StatisticIndexBrief>
            GetIndices(string searchKey = null)
        {
            var factory = new StatisticIndexBriefFactory();
            var result = factory.GetStatisticIndexBriefsByName(searchKey);
            return result;
        }

        [Route("v0/indicators/{id}")]
        [HttpGet]
        public Models.StatisticIndexDetails GetIndex(int id)
        {
            var factory = new StatisticIndexDetailsFactory();
            var result = factory.GetStatisticIndexDetailsById(id);
            return result;
        }

        [Route("v0/statistics/{id}")]
        [HttpGet]
        public IEnumerable<Models.StatisticsResult> GetStatistics(
            int id,
            string year)
        {
            Models.StatisticIndexDetails details =
                new StatisticIndexDetailsFactory().GetStatisticIndexDetailsById(id);
            var factory = new StatisticsResultFactory();
            return factory.GetStatisticsResults(
                id,
                year,
                details);
        }

        [Route("v0/regions/")]
        [HttpGet]
        public IEnumerable<Models.Region> GetRegions()
        {
            var factory = new RegionFactory();
            return factory.GetRegions();
        }

        private static LiiteriStatisticsCore.Util.AreaTypeMappings
            AreaTypeMappings = new LiiteriStatisticsCore.Util.AreaTypeMappings();

        /* V1 features below */

        [Route("v1/statistics/{statisticsId}/")]
        [HttpGet]
        public IEnumerable<LiiteriStatisticsCore.Models.StatisticsResult> GetStatisticsV1(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null)
        {
            logger.Debug(string.Format("statisticsId={0}", statisticsId));
            logger.Debug(string.Format("years={0}", years));
            logger.Debug(string.Format("group={0}", group));
            logger.Debug(string.Format("filter={0}", filter));

            using (DbConnection db = this.GetDbConnection()) {

                /* Step 1: Fetch IndicatorDetails */

                var indicatorQuery =
                    new LiiteriStatisticsCore.Queries.IndicatorQuery();
                indicatorQuery.IdIs = statisticsId;

                var indicatorDetailsRepository = new LiiteriStatisticsCore.
                    Repositories.IndicatorDetailsRepository(db);
                var details = (LiiteriStatisticsCore.Models.IndicatorDetails)
                    indicatorDetailsRepository.Single(indicatorQuery);

                /* Step 2: Create one or more StatisticsQuery objects */
                var queries =
                    new List<LiiteriStatisticsCore.Queries.StatisticsQuery>();

                /* although StatisticsQuery could implement .YearIn, which 
                 * would accept a list of years, what about if different years
                 * end up having different DatabaseAreaTypes?
                 * For this reason, let's just loop the years and create
                 * multiple queries */
                foreach (int year in years) {
                    LiiteriStatisticsCore.Models.TimePeriod timePeriod = (
                        from p in details.TimePeriods
                        where p.Id == year
                        select p).Single();
                    int[] availableAreaTypes = (
                        from a in timePeriod.DataAreaTypes
                        select a.Id).ToArray();

                    var statisticsQuery = new LiiteriStatisticsCore.Queries
                        .StatisticsQuery(statisticsId);

                    statisticsQuery.CalculationTypeIdIs = details.CalculationType;
                    statisticsQuery.AvailableAreaTypes = availableAreaTypes;

                    if (group == null) group = "finland";
                    statisticsQuery.GroupByAreaTypeIdIs = group;
                    statisticsQuery.YearIs = year;
                    statisticsQuery.AreaFilterQueryString = filter;

                    queries.Add(statisticsQuery);
                }

                /* Step 3: Fetch StatisticsResult */

                var repository = new LiiteriStatisticsCore.Repositories.
                    StatisticsResultRepository(db);
                return (List<LiiteriStatisticsCore.Models.StatisticsResult>)
                    repository.FindAll(queries);
            }
        }

        [Route("v1/areaTypes/")]
        [HttpGet]
        public IEnumerable<LiiteriStatisticsCore.Models.AreaType>
            GetAreaTypesV1()
        {
            return AreaTypeMappings.GetAreaTypes();
        }

        [Route("v1/areaTypes/{areaTypeId}/areas/")]
        [HttpGet]
        public IEnumerable<LiiteriStatisticsCore.Models.Area>
            GetAreasV1(string areaTypeId)
        {
            var query = new LiiteriStatisticsCore.Queries.AreaQuery();
            query.AreaTypeIdIs = areaTypeId;
            using (DbConnection db = this.GetDbConnection()) {
                var repository =
                    new LiiteriStatisticsCore.Repositories.AreaRepository(db);
                return (List<LiiteriStatisticsCore.Models.Area>)
                    repository.FindAll(query);
            }
        }
    }
}