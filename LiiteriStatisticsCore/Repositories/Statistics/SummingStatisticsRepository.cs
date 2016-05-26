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
            // exclude nulls from the results
            StatisticsResult[] results2 = results
                .Where(x => x != null && x.Value != null)
                .ToArray();

            // grab the decimal values for summing
            decimal[] values = results2
                .Select(x => (decimal) x.Value)
                .ToArray();

            // pick the first item to use as a template
            var ret_r = (StatisticsResult) results2.First().Clone();

            // .. and override the value with the summed value
            ret_r.Value = values.Sum();

            return ret_r;
        }
    }
}