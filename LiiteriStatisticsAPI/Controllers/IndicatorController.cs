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
    public class IndicatorController :
        ApiController,
        Core.Controllers.IIndicatorController
    {
        private Core.Controllers.IIndicatorController GetController()
        {
            if (ConfigurationManager.AppSettings["UseWCF"] == "true") {
                ChannelFactory<Core.Controllers.IIndicatorController> factory =
                    new ChannelFactory<Core.Controllers.IIndicatorController>(
                        "StatisticsServiceEndpoint");
                return factory.CreateChannel();
            } else {
                return new Core.Controllers.IndicatorController();
            }
        }

        [Route("v1/indicators/")]
        [HttpGet]
        public IEnumerable<Core.Models.IndicatorBrief> GetIndicators(
            string name = null,
            int? accessRight = null)
        {
            return this.GetController().GetIndicators(name, accessRight);
        }

        [Route("v1/indicators/{id}")]
        [HttpGet]
        public Core.Models.IndicatorDetails GetIndicatorDetails(int id)
        {
            return this.GetController().GetIndicatorDetails(id);
        }
    }
}