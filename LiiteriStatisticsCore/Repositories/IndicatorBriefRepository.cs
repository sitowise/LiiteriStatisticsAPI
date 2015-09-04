using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public class IndicatorBriefRepository :
        SqlReadRepository<Models.IndicatorBrief>
    {
        public IndicatorBriefRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries) :
            base(dbConnection, queries, new Factories.IndicatorBriefFactory())
        {
        }

        public override IEnumerable<Models.IndicatorBrief> FindAll()
        {
            /* Here we have to do a bit of manual work with the indicators,
             * since we are receiving multiples of the same indicator due
             * to data being joined from other tables */

            var entityList = new List<Models.IndicatorBrief>();
            int prevDetailsId = 0;
            var briefFactory = new Factories.IndicatorBriefFactory();
            Models.IndicatorBrief brief = null;

            using (DbDataReader rdr =
                    this.GetDbDataReader(this.queries.Single())) {
                while (rdr.Read()) {
                    if (prevDetailsId == (prevDetailsId = (int) rdr["Id"])) {
                        continue;
                    }
                    brief = (Models.IndicatorBrief) briefFactory.Create(rdr);
                    yield return brief;
                }
            }
        }

        public override Models.IndicatorBrief Single()
        {
            return this.FindAll().Single();
        }

        public override Models.IndicatorBrief First()
        {
            return this.FindAll().First();
        }
    }
}