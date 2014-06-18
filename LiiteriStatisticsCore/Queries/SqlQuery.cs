using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiiteriStatisticsCore.Queries
{
    public interface ISqlQuery
    {
        IEnumerable<KeyValuePair<string, object>> Parameters { get; }
        string GetQueryString();
    }

    public abstract class SqlQuery : ISqlQuery
    {
        public Dictionary<string, object> Parameters {get; set;}

        IEnumerable<KeyValuePair<string, object>> ISqlQuery.Parameters
        {
            get { return Parameters;  }
        }

        public SqlQuery()
        {
            this.Parameters = new Dictionary<string, object>();
        }

        public void AddParameter(string key, object value)
        {
            this.Parameters[key] = value;
        }

        public object GetParameter(string key)
        {
            if (!this.Parameters.ContainsKey(key)) {
                return null;
            }
            return this.Parameters[key];
        }

        public abstract string GetQueryString();
    }
}