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
            int groupAreaTypeId,
            int? filterAreaTypeId = null,
            int? filterAreaId = null)
        {
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

                /* although StatisticsQuery implements .YearIn, which 
                 * accepts a list of years, what about if different years
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

                    /* So we want to group by groupAreaType, and we have to search
                     * by using one of availableAreaTypes */

                    /* We have to decide which one of availableAreaTypes
                     * (databaseAreaType) will be used */

                    var statisticsQuery = new LiiteriStatisticsCore.Queries
                        .StatisticsQuery(statisticsId);
                    statisticsQuery.CalculationTypeIdIs = details.CalculationType;

                    //statisticsQuery.DatabaseAreaTypeIdIs = availableAreaTypes[0];

                    statisticsQuery.DatabaseAreaTypeIdIs =
                        (int) Controllers.StatisticController.AreaTypeMappings.GetDatabaseAreaType(
                            groupAreaTypeId,
                            availableAreaTypes);

                    Debug.WriteLine(
                        "We were asked to group by groupAreaTypeId {0}, " +
                        "statistics data is available in areaTypes {1}, " +
                        "finally we ended up searching the database by areaType {2}",
                        groupAreaTypeId,
                        string.Join(", ", availableAreaTypes),
                        statisticsQuery.DatabaseAreaTypeIdIs);

                    statisticsQuery.GroupByAreaTypeIdIs = groupAreaTypeId;

                    statisticsQuery.YearIs = year;

                    statisticsQuery.FilterAreaIdIs = filterAreaId;
                    statisticsQuery.FilterAreaTypeIdIs = filterAreaTypeId;

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
            var query = new LiiteriStatisticsCore.Queries.AreaTypeQuery();
            using (DbConnection db = this.GetDbConnection()) {
                var repository =
                    new LiiteriStatisticsCore.Repositories.AreaTypeRepository(db);
                return (List<LiiteriStatisticsCore.Models.AreaType>)
                    repository.FindAll(query);
            }
        }

        [Route("v1/areaTypes/{areaTypeId}/areas/")]
        [HttpGet]
        public IEnumerable<LiiteriStatisticsCore.Models.Area>
            GetAreasV1(int areaTypeId)
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