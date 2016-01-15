using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LiiteriStatisticsCore.Models
{
    public class CommuteStatisticsType
    {
        public string Id;
        public string Description;
    }

    public class CommuteStatisticsIndicator : ILiiteriEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string AdditionalInformation { get; set; }

        [IgnoreDataMember]
        public string TableName { get; set; }

        public string PrivacyDescription { get; set; }

        public string TimeSpan { get; set; }

        public string TimeSpanDescription { get; set; }

        public string Unit { get; set; }

        public CommuteStatisticsType[] CommuteStatisticsTypes { get; set; }

        public CommuteStatisticsYear[] CommuteStatisticsYears { get; set; }

        [IgnoreDataMember]
        public static Tuple<string, int>[] TableNameIdMapping = new Tuple<string, int>[]
        {
            new Tuple<string, int>("FactTyomatkaTOL2002", -1),
            new Tuple<string, int>("FactTyomatkaTOL2008", -2),
        };
    }
}