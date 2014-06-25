using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/* Only used by V0 API, to be removed */

namespace LiiteriDataAPI.Models
{
    public class StatisticsResult
    {
        public int RegionID { get; set; }
        public string MunicipalityName { get; set; }
        public string MunicipalityId { get; set; }
        public string Year { get; set; }
        public decimal Value { get; set; }
    }
}