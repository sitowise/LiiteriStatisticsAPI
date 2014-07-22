using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class TimePeriod : ILiiteriEntity
    {
        public int Id { get; set; }
        public IEnumerable<AreaType> AreaTypes;
    }
}