using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Requests
{
    public class StatisticsRequest : ICloneable
    {
        public int[] Years { get; set; }
        public int StatisticsId { get; set; }
        public string Group { get; set; }
        public string Filter { get; set; }

        public object Clone()
        {
            var obj = new StatisticsRequest();
            obj.Years = this.Years;
            obj.StatisticsId = this.StatisticsId;
            obj.Group = this.Group;
            obj.Filter = this.Filter;
            return obj;
        }
    }
}