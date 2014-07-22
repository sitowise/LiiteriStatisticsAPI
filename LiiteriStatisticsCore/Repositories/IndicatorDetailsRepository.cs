using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

namespace LiiteriStatisticsCore.Repositories
{
    public class IndicatorDetailsRepository :
        SqlReadRepository<Models.IndicatorDetails>
    {
        public IndicatorDetailsRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public override IEnumerable<Models.IndicatorDetails>
            FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorDetailsFactory());
        }

        public override Models.IndicatorDetails Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorDetailsFactory()).Single();
        }

        public override Models.IndicatorDetails First(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.IndicatorDetailsFactory()).First();
        }
    }
}