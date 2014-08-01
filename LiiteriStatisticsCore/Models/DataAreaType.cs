using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    /* Used within TimePeriods within IndicatorDetails */
    public class DataAreaType : ILiiteriEntity
    {
        public int Id { get; set; }
        public string DataSource { get; set; }
    }
}