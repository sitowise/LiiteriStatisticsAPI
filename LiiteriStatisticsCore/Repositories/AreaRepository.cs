using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public class AreaRepository : SqlReadRepository<Models.Area>
    {
        public AreaRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public override IEnumerable<Models.Area>
            FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query, new Factories.AreaFactory());
        }

        public override Models.Area Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query).Single();
        }

        public override Models.Area First(Queries.ISqlQuery query)
        {
            return this.FindAll(query).First();
        }
    }
}