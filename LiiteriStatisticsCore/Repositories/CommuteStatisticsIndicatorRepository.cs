using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;

using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsCore.Repositories
{
    public class CommuteStatisticsIndicatorRepository :
        SqlReadRepository<CommuteStatisticsIndicator>
    {
        public CommuteStatisticsIndicatorRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries) :
            base(dbConnection, queries, new Factories.CommuteStatisticsIndicatorFactory())
        {
        }

        private IEnumerable<CommuteStatisticsYear> GetYears(string tableName)
        {
            var query = new Queries.CommuteStatisticsYearQuery(tableName);

            var repo = new CommuteStatisticsYearRepository(
                this.dbConnection,
                new Queries.ISqlQuery[] { query });

            return repo.FindAll().ToArray();
        }

        public override IEnumerable<CommuteStatisticsIndicator> FindAll()
        {
            using (DbDataReader rdr =
                    this.GetDbDataReader(this.queries.Single())) {
                while (rdr.Read()) {
                    var indicator = (CommuteStatisticsIndicator)
                        this.factory.Create(rdr);
                    indicator.CommuteStatisticsYears = this.GetYears(indicator.TableName).ToArray();
                    yield return indicator;
                }
            }
        }

        public override CommuteStatisticsIndicator Single()
        {
            return this.FindAll().Single();
        }

        public override CommuteStatisticsIndicator First()
        {
            return this.FindAll().First();
        }
    }
}