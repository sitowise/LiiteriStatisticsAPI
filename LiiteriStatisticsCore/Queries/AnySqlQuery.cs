using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Queries
{
    public class AnySqlQuery : SqlQuery, ISqlQuery
    {
        private string queryString;

        public AnySqlQuery(string queryString) : base()
        {
            this.queryString = queryString;
        }

        public override string GetQueryString()
        {
            return this.queryString;
        }
    }
}
