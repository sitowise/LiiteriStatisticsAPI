using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiiteriStatisticsCore.Repositories
{
    class IndicatorSubRepository : SqlReadRepository<int>
    {
        public IndicatorSubRepository(
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
                    yield return (int) rdr["Value"];
                }
            }
        }

        public override int First()
        {
            throw new NotImplementedException();
        }

        public override int Single()
        {
            throw new NotImplementedException();
        }
    }
}