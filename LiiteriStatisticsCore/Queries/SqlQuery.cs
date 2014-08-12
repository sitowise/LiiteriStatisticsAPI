using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LiiteriStatisticsCore.Queries
{
    public interface ISqlQuery
    {
        Infrastructure.ParameterCollection Parameters { get; set; }
        string GetQueryString();
    }

    public abstract class SqlQuery : ISqlQuery
    {
        public Infrastructure.ParameterCollection Parameters { get; set; }

        public SqlQuery()
        {
            this.Parameters = new Infrastructure.ParameterCollection();
        }

        public abstract string GetQueryString();
    }
}