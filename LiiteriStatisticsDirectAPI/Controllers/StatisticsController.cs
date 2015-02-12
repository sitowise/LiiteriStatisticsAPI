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
    public class StatisticController :
        ApiController,
        Core.Controllers.IStatisticsController
    {
        private Core.Controllers.IStatisticsController GetController()
        {
            return new Core.Controllers.StatisticsController();
        }

        [Route("statistics/{statisticsId}/")]
        [HttpPost]
        public IEnumerable<Core.Models.StatisticsResult> GetStatistics(
            int statisticsId,
            [FromBody] Core.Controllers.StatisticsRequest reqobj)
        {
            return this.GetStatistics(
                reqobj.years,
                statisticsId,
                reqobj.group,
                reqobj.filter);
        }

        [Route("statistics/{statisticsId}/")]
        [HttpGet]
        public IEnumerable<Core.Models.StatisticsResult> GetStatistics(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null,
            bool debug = false)
        {
            return this.GetController().GetStatistics(
                years, statisticsId, group, filter, debug);
        }

        [Route("areaTypes/")]
        [HttpGet]
        public IEnumerable<Core.Models.AreaType> GetAreaTypes()
        {
            return this.GetController().GetAreaTypes();
        }

        [Route("areaTypes/{areaTypeId}/areas/")]
        [HttpGet]
        public IEnumerable<Core.Models.Area> GetAreas(string areaTypeId)
        {
            return this.GetController().GetAreas(areaTypeId);
        }
    }
}