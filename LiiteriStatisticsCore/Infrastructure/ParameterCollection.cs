using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LiiteriStatisticsCore.Infrastructure
{
    public class ParameterCollection :
        KeyedCollection<string, Parameter>
    {
        private string defaultPrefix = "Param";

        public ParameterCollection(string defaultPrefix = null)
        {
            if (defaultPrefix != null) {
                this.defaultPrefix = defaultPrefix;
            }
        }

        public void Add(string name, object value)
        {
            Parameter param = new Parameter(name, value);
            this.Add(param);
        }

        public void Add(ParameterCollection parameters)
        {
            foreach (Parameter p in parameters) {
                this.Add(p);
            }
        }

        public string AddValue(object value)
        {
            int num = this.Count + 1;
            string name = string.Format("{0}_{1}",
                this.defaultPrefix,
                num.ToString());
            this.Add(name, value);
            return name;
        }

        protected override string GetKeyForItem(Parameter item)
        {
            return item.Name;
        }
    }
}