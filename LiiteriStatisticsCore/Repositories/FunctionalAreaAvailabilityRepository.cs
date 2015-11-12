using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Repositories
{
    public class FunctionalAreaAvailabilityRepository :
        SqlReadRepository<Models.FunctionalAreaAvailability>
    {
        private static Util.AreaTypeMappings
            AreaTypeMappings = new Util.AreaTypeMappings();

        public FunctionalAreaAvailabilityRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries) :
            base(dbConnection, queries,
                new Factories.FunctionalAreaAvailabilityFactory())
        {
        }

        public override IEnumerable<Models.FunctionalAreaAvailability> FindAll()
        {
            var query = this.queries.Single();
            var availFactory = new Factories.FunctionalAreaAvailabilityFactory();
            using (DbDataReader rdr = this.GetDbDataReader(query)) {
                while (rdr.Read()) {
                    var avail = (Models.FunctionalAreaAvailability)
                        availFactory.Create(rdr);
                    yield return avail;
                }
            }
        }

        public override Models.FunctionalAreaAvailability First()
        {
            throw new NotImplementedException();
        }

        public override Models.FunctionalAreaAvailability Single()
        {
            throw new NotImplementedException();
        }
    }
}