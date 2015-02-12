using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Core = LiiteriStatisticsCore;

namespace LiiteriStatisticsDirectAPI.Controllers
{
    [RoutePrefix("v1")]
    public class CommuteStatisticsController :
        ApiController,
        Core.Controllers.ICommuteStatisticsController
    {
        private Core.Controllers.ICommuteStatisticsController GetController()
        {
            return new Core.Controllers.CommuteStatisticsController();
        }

        [Route("commuteStatistics/")]
        [HttpGet]
        public IEnumerable<Core.Models.CommuteStatisticsIndicator>
            GetCommuteStatisticsIndicators()
        {
            return this.GetController().GetCommuteStatisticsIndicators();
        }

        [Route("commuteStatistics/{statisticsId}/")]
        [HttpPost]
        public IEnumerable<Core.Models.StatisticsResult> GetCommuteStatistics(
            int statisticsId,
            [FromBody] Core.Controllers.CommuteStatisticsRequest reqobj)
        {
            return this.GetCommuteStatistics(
                statisticsId,
                reqobj.years,
                reqobj.type,
                reqobj.gender,
                reqobj.group,
                reqobj.work_filter,
                reqobj.home_filter);
        }

        [Route("commuteStatistics/{statisticsId}/")]
        [HttpGet]
        public IEnumerable<Core.Models.StatisticsResult> GetCommuteStatistics(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null,
            bool debug = false)
        {
            return this.GetController().GetCommuteStatistics(
                statisticsId,
                years,
                type,
                gender,
                group,
                work_filter,
                home_filter,
                debug);
        }
    }
}