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
        public CommuteStatisticsYearRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public override IEnumerable<int> FindAll(Queries.ISqlQuery query)
        {
            using (DbDataReader rdr = this.GetDbDataReader(query)) {
                while (rdr.Read()) {
                    yield return (int) rdr["Year"];
                }
            }
        }

        public override int Single(Queries.ISqlQuery query)
        {
            throw new NotImplementedException();
        }

        public override int First(Queries.ISqlQuery query)
        {
            throw new NotImplementedException();
        }
    }
}