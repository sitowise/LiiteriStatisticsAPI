using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.ComponentModel;

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
        public int? DisplayUnitId { get; set; }

        [IgnoreDataMember]
        public int? InternalUnitId { get; set; }

        public string Description { get; set; }

        public string AdditionalInformation { get; set; }

#if !DEBUG
        [IgnoreDataMember]
#endif
        public int CalculationType { get; set; }

        [DefaultValue(null)]
        public IEnumerable<TimePeriod> TimePeriods { get; set; }

        [DefaultValue(null)]
        public PrivacyLimit PrivacyLimit { get; set; }

        [IgnoreDataMember]
        [DefaultValue(null)]
        public int[] DerivedStatistics { get; set; }

        [DefaultValue(null)]
        public AccessRight AccessRight { get; set; }
    }
}