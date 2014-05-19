using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LiiteriDataAPI.Controllers
{
    public class IndexController : ApiController
    {
        [Route("v0/indicators/")]
        [HttpGet]
        public IEnumerable<Models.StatisticIndexBrief>
            GetIndices(string searchKey = null)
        {
            var finder = new StatisticIndexFinder();
            var result = finder.GetStatisticIndexBriefsByName(searchKey);
            return result;
        }

        [Route("v0/indicators/{id}")]
        [HttpGet]
        public Models.StatisticIndexDetails GetIndex(int id)
        {
            var finder = new StatisticIndexFinder();
            var result = finder.GetStatisticIndexDetailsById(id);
            return result;
        }
    }
}
