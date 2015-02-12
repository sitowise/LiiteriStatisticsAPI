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
    public class IndicatorController :
        ApiController,
        Core.Controllers.IIndicatorController
    {
        private Core.Controllers.IIndicatorController GetServiceClient()
        {
            ChannelFactory<Core.Controllers.IIndicatorController> factory =
                new ChannelFactory<Core.Controllers.IIndicatorController>(
                    "StatisticsServiceEndpoint");
            Core.Controllers.IIndicatorController proxy = factory.CreateChannel();
            return proxy;
        }

        [Route("v1/indicators/")]
        [HttpGet]
        public IEnumerable<Core.Models.IndicatorBrief>
            GetIndicators(string name = null)
        {
            return this.GetServiceClient().GetIndicators(name);
        }

        [Route("v1/indicators/{id}")]
        [HttpGet]
        public Core.Models.IndicatorDetails GetIndicatorDetails(int id)
        {
            return this.GetServiceClient().GetIndicatorDetails(id);
        }
    }
}