using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiiteriDataAPI.Models
{
    public class StatisticsResult
    {
        public int RegionID { get; set; }
        public string MunicipalityName { get; set; }
        public string MunicipalityId { get; set; }
        public string Year { get; set; }
        public object Value { get; set; }
    }
}