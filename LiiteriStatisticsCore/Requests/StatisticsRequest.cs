using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Requests
{
    public class StatisticsRequest
    {
        public int[] Years { get; set; }
        public int StatisticsId { get; set; }
        public string Group { get; set; }
        public string Filter { get; set; }
    }
}