using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    class CommuteStatisticsYearRepository : SqlReadRepository<int>
    {
        public CommuteStatisticsYearRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries) :
            base(dbConnection, queries)
        {
        }

        public override IEnumerable<int> FindAll()
        {
            using (DbDataReader rdr =
                    this.GetDbDataReader(this.queries.Single())) {
                while (rdr.Read()) {
                    yield return (int) rdr["Year"];
                }
            }
        }

        public override int Single()
        {
            throw new NotImplementedException();
        }

        public override int First()
        {
            throw new NotImplementedException();
        }
    }
}