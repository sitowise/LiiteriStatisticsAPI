using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Requests
{
    public class StatisticsRequest : ICloneable
    {
        public int[] Years { get; set; }
        public int StatisticsId { get; set; }
        public string Group { get; set; }
        public string Filter { get; set; }

        // helper property to avoid infinite loop
        [System.ComponentModel.DefaultValue(false)]
        public bool SkipPrivacyLimits { get; set; }

        // helper property to avoid infinite loop
        [System.ComponentModel.DefaultValue(false)]
        public bool SkipUnitConversions { get; set; }

        public object Clone()
        {
            var obj = new StatisticsRequest();
            obj.Years = this.Years;
            obj.StatisticsId = this.StatisticsId;
            obj.Group = this.Group;
            obj.Filter = this.Filter;

            obj.SkipPrivacyLimits = this.SkipPrivacyLimits;
            obj.SkipUnitConversions = this.SkipUnitConversions;

            return obj;
        }
    }
}