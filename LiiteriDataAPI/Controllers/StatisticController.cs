using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LiiteriDataAPI.Controllers
{
    public class StatisticController : ApiController
    {
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
    }
}
