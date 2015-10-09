using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsCore.Repositories
{
    public class DividingStatisticsRepository :
        ComparingStatisticsRepository,
        IReadRepository<StatisticsResult>
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DividingStatisticsRepository(
            IReadRepository<StatisticsResult> denominatorRepository,
            IReadRepository<StatisticsResult> numeratorRepository)
        {
            this.Repositories = new List<IReadRepository<StatisticsResult>>() {
                denominatorRepository,
                numeratorRepository,
            };
        }

        protected override StatisticsResult Compare(StatisticsResult[] results)
        {
            if (results.Length != 2) {
                throw new ArgumentException(
                    "Should have 2 values, instead got " +
                    results.Length.ToString());
            }

            StatisticsResult denominator = results[0];
            StatisticsResult numerator = results[1];

            StatisticsResult ret_r = (StatisticsResult) denominator.Clone();

            if (numerator == null) {
                ret_r.Value = 0;
                return ret_r;
            }

            if (denominator.Value == null || denominator.Value == 0) {
                ret_r.Value = 0;
                return ret_r;
            }

            ret_r.Value = numerator.Value / denominator.Value;

            return ret_r;
        }
    }
}