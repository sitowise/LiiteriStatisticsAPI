using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LiiteriStatisticsCore.Models
{
    public class TimePeriod : ILiiteriEntity
    {
        /* Year */
        public int Id { get; set; }

        /* These area used internally to figure out which DatabaseAreaType
         * to use in the statistics queries */
        [IgnoreDataMember]
        public IEnumerable<DataAreaType> DataAreaTypes;

        /* With these, the client will know what areaTypes can be used
         * to make the queries specific to this TimePeriod */
        public IEnumerable<AreaType> AreaTypes;
    }
}