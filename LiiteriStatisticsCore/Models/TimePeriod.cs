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

        /* each time period in an indicator can have multiple annotations
         * from different sources */
        public IEnumerable<Annotation> Annotations;

        /* Some area types are listed in a block list [TilastoKoosteAluetasoEstetty]
         * This member stores the block list, so it can be passed to
         * the query object */
        [IgnoreDataMember]
        public string[] BlockedAreaTypes;
    }
}