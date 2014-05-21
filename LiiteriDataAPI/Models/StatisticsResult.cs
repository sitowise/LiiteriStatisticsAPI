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
        public string Year { get; set; }
        public object value { get; set; }
    }
}