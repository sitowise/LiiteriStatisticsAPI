using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.ServiceModel; // WCF

using Core = LiiteriStatisticsCore;

namespace LiiteriStatisticsProxiedAPI.Controllers
{
    [RoutePrefix("v1")]
    public class StatisticsController :
        ApiController,
        Core.Controllers.IStatisticsController
    {
        private Core.Controllers.IStatisticsController GetServiceClient()
        {
            ChannelFactory<Core.Controllers.IStatisticsController> factory =
                new ChannelFactory<Core.Controllers.IStatisticsController>(
                    "StatisticsServiceEndpoint");
            Core.Controllers.IStatisticsController proxy = factory.CreateChannel();
            return proxy;
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
            return this.GetServiceClient().GetStatistics(
                years, statisticsId, group, filter, debug);
        }

        [Route("areaTypes/")]
        [HttpGet]
        public IEnumerable<Core.Models.AreaType> GetAreaTypes()
        {
            return this.GetServiceClient().GetAreaTypes();
        }

        [Route("areaTypes/{areaTypeId}/areas/")]
        [HttpGet]
        public IEnumerable<Core.Models.Area> GetAreas(string areaTypeId)
        {
            return this.GetAreas(areaTypeId);
        }
    }
}