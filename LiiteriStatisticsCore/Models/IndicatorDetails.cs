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
        public string Unit { get; set; }

        // ProcessingStage only exists in the Excel file!
        public string ProcessingStage { get; set; }

        public string TimeSpan { get; set; }
        public string TimeSpanDetails { get; set; }

        public int? DecimalCount { get; set; }

        /* unit of measurement */
        [IgnoreDataMember]
        public int DisplayUnitId { get; set; }
        [IgnoreDataMember]
        public int InternalUnitId { get; set; }

        public string Description;
        public string AdditionalInformation;

#if !DEBUG
        [IgnoreDataMember]
#endif
        public int CalculationType { get; set; }

        public IEnumerable<TimePeriod> TimePeriods;

        //[IgnoreDataMember] // may be useful in the UI?
        public PrivacyLimit PrivacyLimit = null;

        [IgnoreDataMember]
        public int[] DerivedStatistics = null;
    }
}