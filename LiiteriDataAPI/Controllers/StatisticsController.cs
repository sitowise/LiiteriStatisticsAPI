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

        private static IDictionary<int, int[]> AreaTypeMappings =
            ReadAreaTypeMappings();

        private static IDictionary<int, int[]> ReadAreaTypeMappings()
        {
            var data = new Dictionary<int, int[]>();

            string XmlFile = System.IO.Path.Combine(
                AppDomain.CurrentDomain.GetData("DataDirectory").ToString(),
                "AreaTypeMappings.xml");
            Debug.WriteLine(string.Format("Reading xmlFile from {0}", XmlFile));
            XmlDocument xmldoc = new XmlDocument();
            XDocument xdoc = XDocument.Load(XmlFile);
            foreach (var selectionAreaType in
                    xdoc.Root.Descendants("SelectionAreaType")) {
                IList<int> l = new List<int>();
                if (selectionAreaType.Descendants("DatabaseAreaTypes")
                        .Count() == 0) {
                    continue;
                }
                foreach (var databaseAreaType in selectionAreaType.
                        Descendants("DatabaseAreaTypes").Single().
                        Descendants("DatabaseAreaType")) {
                    l.Add(Convert.ToInt32(
                        databaseAreaType.Attribute("id").Value.ToString()));
                }
                data[Convert.ToInt32(
                    selectionAreaType.Attribute("id").Value.ToString())] =
                    l.ToArray();
            }
            return data;
        }

        /*
         * selectionAreaType = user wants to search by this areaType
         * availableAreaTypes = the statistics has data for these areaTypes
         * returns = suggestion on what areaType the datbase search should be done with
         */
        private int GetDatabaseAreaType(
            int selectionAreaType,
            int[] availableAreaTypes)
        {
            if (!AreaTypeMappings.Keys.Contains(selectionAreaType)) {
                throw new Exception(string.Format(
                    "No areaType mappings found for id: {0}",
                    selectionAreaType));
            }
            foreach (int id in
                    AreaTypeMappings[selectionAreaType]) {
                if (availableAreaTypes.Contains(id)) {
                    return id;
                }
            }
            throw new Exception("No areaType mapping found!");
        }

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

            int year = years[0]; /* should probably do several queries
                                  * in a loop for different years, because
                                  * they in theory could have different
                                  * databaseAreaTypes */

            using (DbConnection db = this.GetDbConnection()) {

                /* Step 1: Fetch IndicatorDetails */

                var indicatorQuery =
                    new LiiteriStatisticsCore.Queries.IndicatorQuery();
                indicatorQuery.IdIs = statisticsId;

                var indicatorDetailsRepository = new LiiteriStatisticsCore.
                    Repositories.IndicatorDetailsRepository(db);
                var details = (LiiteriStatisticsCore.Models.IndicatorDetails)
                    indicatorDetailsRepository.Single(indicatorQuery);

                /* Step 2: Create StatisticsQuery */

                LiiteriStatisticsCore.Models.TimePeriod timePeriod = (
                    from p in details.TimePeriods
                    where p.Id == year
                    select p).Single();
                int[] availableAreaTypes = (
                    from a in timePeriod.AreaTypes
                    select a.Id).ToArray();

                /* So we want to group by groupAreaType, and we have to search
                 * by using one of availableAreaTypes */

                /* We have to decide which one of availableAreaTypes
                 * (databaseAreaType) will be used */

                var statisticsQuery = new LiiteriStatisticsCore.Queries.
                    StatisticsQuery(statisticsId);
                statisticsQuery.CalculationTypeIdIs = details.CalculationType;

                //statisticsQuery.DatabaseAreaTypeIdIs = availableAreaTypes[0];
                statisticsQuery.DatabaseAreaTypeIdIs =
                    this.GetDatabaseAreaType(groupAreaTypeId, availableAreaTypes);
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

                /* Step 3: Fetch StatisticsResult */

                var repository = new LiiteriStatisticsCore.Repositories.
                    StatisticsResultRepository(db);
                return (List<LiiteriStatisticsCore.Models.StatisticsResult>)
                    repository.FindAll(statisticsQuery);
            }
        }
    }
}