using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class StatisticsResult : ILiiteriMarker
    {
        public decimal? Value { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public string AlternativeId { get; set; }
        public int Year { get; set; }
        public bool PrivacyLimitTriggered { get; set; }

        /* If we end up adding something like this, consider
         * using a nullable class instead */
        //public int? AreaPointLat { get; set; }
        //public int? AreaPointLon { get; set; }
    }
}