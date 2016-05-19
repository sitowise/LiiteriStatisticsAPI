using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsCore.Repositories
{
    public class SummingStatisticsRepository :
        ComparingStatisticsRepository,
        IReadRepository<StatisticsResult>
    {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public SummingStatisticsRepository(
            IEnumerable<IReadRepository<StatisticsResult>> repositories)
        {
            this.Repositories = repositories;
        }

        protected override StatisticsResult Compare(StatisticsResult[] results)
        {
            // values excluding nulls
            decimal[] values = results
                .Where(x => x != null && x.Value != null)
                .Select(x => (decimal) x.Value)
                .ToArray();

            var ret_r = (StatisticsResult) results.First().Clone();
            ret_r.Value = values.Sum();
            return ret_r;
        }
    }
}