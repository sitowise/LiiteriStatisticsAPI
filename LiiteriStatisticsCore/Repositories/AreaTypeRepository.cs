using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public class AreaTypeRepository : SqlReadRepository<Models.AreaType>
    {
        public AreaTypeRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public override IEnumerable<Models.AreaType>
            FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query, new Factories.AreaTypeFactory());
        }

        public override Models.AreaType Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query).Single();
        }

        public override Models.AreaType First(Queries.ISqlQuery query)
        {
            return this.FindAll(query).First();
        }
    }
}