using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Configuration;
using System.ServiceModel; // WCF

using Core = LiiteriStatisticsCore;

namespace LiiteriStatisticsAPI.Controllers
{
    [RoutePrefix("v1")]
    public class StatisticController :
        ApiController,
        Core.Controllers.IStatisticsController
    {
        public class StatisticsRequest
        {
            public int[] years { get; set; }
            public string group { get; set; }
            public string filter { get; set; }
        }

        private Core.Controllers.IStatisticsController GetController()
        {
            if (ConfigurationManager.AppSettings["UseWCF"] == "true") {
                ChannelFactory<Core.Controllers.IStatisticsController> factory =
                    new ChannelFactory<Core.Controllers.IStatisticsController>(
                        "StatisticsServiceEndpoint");
                return factory.CreateChannel();
            } else {
                return new Core.Controllers.StatisticsController();
            }
        }

        [Route("statistics/{statisticsId}/")]
        [HttpPost]
        public IEnumerable<Core.Models.StatisticsResult> GetStatistics(
            int statisticsId,
            [FromBody] StatisticsRequest reqobj)
        {
            return this.GetStatistics(
                reqobj.years,
                statisticsId,
                reqobj.group,
                reqobj.filter);
        }

        [Route("statistics/{statisticsId}/")]
        [HttpGet]
        public HttpResponseMessage GetStatisticsResultsOrDebug(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null,
            bool debug = false)
        {
            if (debug) {
                string debugOutput = this.GetStatisticsDebugString(
                    years, statisticsId, group, filter);
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    debugOutput,
                    new Formatters.TextPlainFormatter());
            } else {
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    this.GetStatistics(years, statisticsId, group, filter).ToList());
            }
        }

        public IEnumerable<Core.Models.StatisticsResult> GetStatistics(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null)
        {
            return this.GetController().GetStatistics(
                years, statisticsId, group, filter);
        }

        public string GetStatisticsDebugString(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null)
        {
            return this.GetController().GetStatisticsDebugString(
                years, statisticsId, group, filter);
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