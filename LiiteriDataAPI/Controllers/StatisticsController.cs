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

        /* V1 features below */

        [Route("v1/statistics/{statisticsId}/")]
        [HttpGet]
        public IEnumerable<StatisticsResult> GetStatisticsV1(
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

                var indicatorQuery = new IndicatorQuery();
                indicatorQuery.IdIs = statisticsId;

                var indicatorDetailsRepository = new IndicatorDetailsRepository(db);
                var details = (IndicatorDetails)
                    indicatorDetailsRepository.Single(indicatorQuery);

                /* Step 2: Create one or more StatisticsQuery objects */
                var queries = new List<StatisticsQuery>();

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

                    queries.Add(statisticsQuery);
                }

                /* Step 3: Fetch StatisticsResult */

                var repository = new StatisticsResultRepository(db);
                return (List<StatisticsResult>) repository.FindAll(queries);
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
                return (List<Area>) repository.FindAll(query);
            }
        }
    }
}