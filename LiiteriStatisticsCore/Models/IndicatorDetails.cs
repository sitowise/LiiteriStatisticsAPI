using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LiiteriStatisticsCore.Models
{
    public class IndicatorDetails : IndicatorBrief, ILiiteriEntity
    {
        //public int? Group { get; set; }
        //public string Unit { get; set; }
        //public int? StatisticId { get; set; }
        //public string ProcessingStage { get; set; }
        //public string TimeSpan { get; set; }
        public int? DecimalCount { get; set; }

        /* unit of measurement */
        [IgnoreDataMember]
        public int DisplayUnitId { get; set; }
        [IgnoreDataMember]
        public int InternalUnitId { get; set; }

        public string Description;
        public string AdditionalInformation;

        //[IgnoreDataMember]
        public int CalculationType { get; set; }

        //public string[] Years;
        //public RegionType[] RegionLayers;

        public IEnumerable<TimePeriod> TimePeriods;
    }
}