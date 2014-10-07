using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace LiiteriStatisticsCore.Models
{
    public class AreaType : ILiiteriMarker
    {
        // id is a string, such as "municipality" or "finland"
        public string Id { get; set; }
        public string Description { get; set; }

        /* Only used when exposing areaTypes via indicator
         * NOTE: DataSource only makes sense if connected
         * to a year (TimePeriod) */
        public string DataSource { get; set; }
    }
}