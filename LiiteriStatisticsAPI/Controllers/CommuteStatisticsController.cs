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
    public class CommuteStatisticsController :
        ApiController,
        Core.Controllers.ICommuteStatisticsController
    {
        public class CommuteStatisticsRequest
        {
            public int[] years { get; set; }
            public string type { get; set; }
            public int gender { get; set; }
            public string group { get; set; }
            public string work_filter { get; set; }
            public string home_filter { get; set; }
        }

        private Core.Controllers.ICommuteStatisticsController GetController()
        {
            if (ConfigurationManager.AppSettings["UseWCF"] == "true") {
                ChannelFactory<Core.Controllers.ICommuteStatisticsController> factory =
                    new ChannelFactory<Core.Controllers.ICommuteStatisticsController>(
                        "StatisticsServiceEndpoint");
                return factory.CreateChannel();
            } else {
                return new Core.Controllers.CommuteStatisticsController();
            }
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
            [FromBody] CommuteStatisticsRequest reqobj)
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
        public HttpResponseMessage GetCommuteStatisticsResultsOrDebug(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null,
            bool debug = false)
        {
            if (debug) {
                string debugOutput = this.GetCommuteStatisticsDebugString(
                    statisticsId,
                    years, type,
                    gender,
                    group,
                    work_filter,
                    home_filter);
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    debugOutput,
                    new Formatters.TextPlainFormatter());
            } else {
                return Request.CreateResponse(HttpStatusCode.OK,
                    this.GetCommuteStatistics(
                        statisticsId,
                        years,
                        type,
                        gender,
                        group,
                        work_filter,
                        home_filter).ToList());
            }
        }

        public IEnumerable<Core.Models.StatisticsResult> GetCommuteStatistics(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null)
        {
            return this.GetController().GetCommuteStatistics(
                statisticsId,
                years,
                type,
                gender,
                group,
                work_filter,
                home_filter);
        }

        public string GetCommuteStatisticsDebugString(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null)
        {
            return this.GetController().GetCommuteStatisticsDebugString(
                statisticsId,
                years,
                type,
                gender,
                group,
                work_filter,
                home_filter);
        }
    }
}