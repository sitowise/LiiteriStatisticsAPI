using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace LiiteriStatisticsCore.Requests
{
    [DataContract]
    public class StatisticsRequest : ICloneable
    {
        [DataMember]
        public int[] Years { get; set; }

        [DataMember]
        public int StatisticsId { get; set; }

        [DataMember]
        public string Group { get; set; }

        [DataMember]
        public string Filter { get; set; }

        [DataMember]
        public int? AreaYear { get; set; }

        // helper property to avoid infinite loop
        [System.ComponentModel.DefaultValue(false)]
        public bool SkipPrivacyLimits { get; set; }

        // helper property to avoid infinite loop
        [System.ComponentModel.DefaultValue(false)]
        public bool SkipUnitConversions { get; set; }

        // helper property to avoid infinite loop
        [System.ComponentModel.DefaultValue(0)]
        public int RecursionDepth { get; set; }

        public object Clone()
        {
            var obj = new StatisticsRequest();
            obj.Years = this.Years;
            obj.AreaYear = this.AreaYear;
            obj.StatisticsId = this.StatisticsId;
            obj.Group = this.Group;
            obj.Filter = this.Filter;

            obj.SkipPrivacyLimits = this.SkipPrivacyLimits;
            obj.SkipUnitConversions = this.SkipUnitConversions;
            obj.RecursionDepth = this.RecursionDepth;

            return obj;
        }
    }
}