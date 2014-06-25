using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Runtime.Serialization;

/* Only used by V0 API, to be removed */

namespace LiiteriDataAPI.Models
{
    public class StatisticIndexDetails : StatisticIndexBrief
    {
        public int? Group { get; set; }
        public string Unit { get; set; }
        //public int? StatisticId { get; set; }
        public string ProcessingStage { get; set; }
        public string TimeSpan { get; set; }
        public int? DecimalCount { get; set; }

        /* unit of measurement */
        [IgnoreDataMember]
        public int DisplayUnitID { get; set; }
        [IgnoreDataMember]
        public int InternalUnitID { get; set; }

        public string Description;
        public string AdditionalInformation;

        //[IgnoreDataMember]
        public int CalculationType { get; set; }

        public string[] Years;
        //public RegionType[] RegionLayers;
    }
}