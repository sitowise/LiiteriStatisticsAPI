using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Common;
using System.Runtime.Serialization;

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

        public string Description;
        public string AdditionalInformation;

        [IgnoreDataMember]
        public int CalculationType { get; set; }

        public string[] Years;
        //public RegionType[] RegionLayers;
    }
}