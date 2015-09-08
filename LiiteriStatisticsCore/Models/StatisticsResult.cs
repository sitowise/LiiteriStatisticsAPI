using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class StatisticsResult : ILiiteriMarker, ICloneable
    {
        public decimal? Value { get; set; }
        public int AreaId { get; set; }
        public string AreaName { get; set; }
        public string AlternativeId { get; set; }
        public int Year { get; set; }
        public bool PrivacyLimitTriggered { get; set; }

        public object Clone()
        {
            return new StatisticsResult() {
                Value = this.Value,
                AreaId = this.AreaId,
                AreaName = this.AreaName,
                AlternativeId = this.AlternativeId,
                Year = this.Year,
                PrivacyLimitTriggered = this.PrivacyLimitTriggered,
            };
        }
    }
}