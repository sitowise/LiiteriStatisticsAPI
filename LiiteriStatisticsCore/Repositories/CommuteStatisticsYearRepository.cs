using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public class CommuteStatisticsYearRepository :
        SqlReadRepository<Models.CommuteStatisticsYear>
    {
        public CommuteStatisticsYearRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries) :
            base(dbConnection, queries)
        {
        }

        public override IEnumerable<Models.CommuteStatisticsYear> FindAll()
        {
            var years = new List<Models.CommuteStatisticsYear>();

            using (DbDataReader rdr =
                    this.GetDbDataReader(this.queries.Single())) {
                int? prev_year = null;
                Models.CommuteStatisticsYear obj = null;
                List<string> dataSources = new List<string>(); ;
                while (rdr.Read()) {
                    int year = rdr.GetInt32(rdr.GetOrdinal("Year"));
                    if (prev_year != (prev_year = year)) {
                        obj = new Models.CommuteStatisticsYear();
                        obj.Year = year;
                        dataSources = new List<string>();
                        obj.DataSources = dataSources;
                        years.Add(obj);
                    }
                    dataSources.Add(rdr["DataSource"].ToString());
                }
            }
            return years;
        }

        public override Models.CommuteStatisticsYear Single()
        {
            throw new NotImplementedException();
        }

        public override Models.CommuteStatisticsYear First()
        {
            throw new NotImplementedException();
        }
    }
}