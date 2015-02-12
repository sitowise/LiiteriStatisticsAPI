using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;

using Core = LiiteriStatisticsCore;

namespace LiiteriStatisticsDirectAPI.Controllers
{
    public class IndicatorController :
        ApiController,
        Core.Controllers.IIndicatorController
    {
        private Core.Controllers.IIndicatorController GetController()
        {
            return new Core.Controllers.IndicatorController();
        }

        [Route("v1/indicators/")]
        [HttpGet]
        public IEnumerable<Core.Models.IndicatorBrief> GetIndicators(
            string name = null)
        {
            return this.GetController().GetIndicators(name);
        }

        [Route("v1/indicators/{id}")]
        [HttpGet]
        public Core.Models.IndicatorDetails GetIndicatorDetails(int id)
        {
            return this.GetController().GetIndicatorDetails(id);
        }
    }
}
