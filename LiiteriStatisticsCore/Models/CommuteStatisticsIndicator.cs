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

    public class CommuteStatisticsIndicator
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [IgnoreDataMember]
        public string TableName { get; set; }

        public CommuteStatisticsType[] CommuteStatisticsTypes { get; set; }

        public int[] Years { get; set; }
    }
}