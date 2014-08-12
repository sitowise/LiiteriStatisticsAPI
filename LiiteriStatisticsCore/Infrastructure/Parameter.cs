using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Infrastructure
{
    public class Parameter
    {
        public Parameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public string Name { get; set; }

        public object Value { get; set; }
    }
}