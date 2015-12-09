using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Models
{
    public class CommuteStatisticsYear : ILiiteriMarker
    {
        public int Year { get; set; }

        public List<string> DataSources { get; set; }
    }
}