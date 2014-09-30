using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

namespace LiiteriStatisticsCore.Util
{
    public class DebugOutput
    {
        IEnumerable<Queries.ISqlQuery> SqlQueries;

        public DebugOutput(Queries.ISqlQuery query)
        {
            this.SqlQueries = (new List<Queries.ISqlQuery>()
                { query, }).ToArray();
        }

        public DebugOutput(IEnumerable<Queries.ISqlQuery> queries)
        {
            this.SqlQueries = queries;
        }

        public DebugOutput(
            IEnumerable<Tuple<Queries.ISqlQuery, Queries.ISqlQuery>> querypairs)
        {
            var queries = new List<Queries.ISqlQuery>();
            foreach (Tuple<Queries.ISqlQuery, Queries.ISqlQuery> pair in
                    querypairs) {
                queries.Add(pair.Item1);
                queries.Add(pair.Item2);
            }
            this.SqlQueries = queries;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Queries.ISqlQuery query in this.SqlQueries) {
                string queryString = query.GetQueryString();
                foreach (Infrastructure.Parameter param in query.Parameters) {
                    string type = "INT";
                    string value = param.Value.ToString();
                    switch (param.Value.GetType().ToString()) {
                        case "System.String":
                            type = "VARCHAR(MAX)";
                            value = string.Format("'{0}'", value);
                            break;
                    }
                    sb.Append(string.Format(
                        "DECLARE @{0} {1} = {2}\n",
                        param.Name,
                        type,
                        value));
                }
                sb.Append(queryString);
                sb.Append("\n");
            }

            return sb.ToString();
        }
    }
}