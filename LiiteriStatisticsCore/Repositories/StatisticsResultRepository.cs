using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public class StatisticsResultRepository :
        SqlReadRepository<Models.StatisticsResult>
    {
        public StatisticsResultRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        public IEnumerable<Models.StatisticsResult> FindAll(
            IEnumerable<Queries.ISqlQuery> queries)
        {
            var entityList = new List<Models.StatisticsResult>();
            foreach (Queries.ISqlQuery query in queries) {
                entityList.AddRange(this.FindAll(query));
            }
            return entityList;
        }

        public override IEnumerable<Models.StatisticsResult>
            FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.StatisticsResultFactory());
        }

        public override Models.StatisticsResult Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query).Single();
        }

        public override Models.StatisticsResult First(Queries.ISqlQuery query)
        {
            return this.FindAll(query).First();
        }
    }
}
