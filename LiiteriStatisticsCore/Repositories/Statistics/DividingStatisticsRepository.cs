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
        IReadRepository<StatisticsResult>
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IReadRepository<StatisticsResult> DenominatorRepository;
        private IReadRepository<StatisticsResult> NumeratorRepository;

        public DividingStatisticsRepository(
            IReadRepository<StatisticsResult> denominatorRepository,
            IReadRepository<StatisticsResult> numeratorRepository)
        {
            this.DenominatorRepository = denominatorRepository;
            this.NumeratorRepository = numeratorRepository;
        }

        public IEnumerable<StatisticsResult> FindAll()
        {
            /* Denominator == Nimittäjä
             * Numerator == Osoittaja */
            using (IEnumerator<StatisticsResult> denomEnumerator =
                    this.DenominatorRepository.FindAll().GetEnumerator())
            using (IEnumerator<StatisticsResult> numerEnumerator =
                    this.NumeratorRepository.FindAll().GetEnumerator()) {
                StatisticsResult numerResult;
                StatisticsResult denomResult;
                StatisticsResult ret_r;

                numerEnumerator.MoveNext();
                numerResult = numerEnumerator.Current;
                while (denomEnumerator.MoveNext()) {
                    denomResult = denomEnumerator.Current;
                    ret_r = (StatisticsResult) denomResult.Clone();

                    // numerator already at the end of data
                    if (numerResult == null) {
                        ret_r.Value = 0;
                        yield return ret_r;
                        continue;
                    }

                    while (numerResult.AreaId < denomResult.AreaId) {
                        if (numerEnumerator.MoveNext()) {
                            numerResult = numerEnumerator.Current;
                        } else {
                            numerResult = null;
                            break;
                        }
                    }

                    // numerator just reached end of data
                    if (numerResult == null) {
                        ret_r.Value = 0;
                        yield return ret_r;
                        continue;
                    }

                    if (numerResult.AreaId > denomResult.AreaId) {
                        /* we hopped past denom, assume numerator is 0 */
                        Debug.WriteLine(
                            "numerator > denominator, assume numerator == 0");
                        ret_r.Value = 0;
                    } else if (numerResult.AreaId == denomResult.AreaId) {
                        if (denomResult.Value == 0) {
                            ret_r.Value = 0;
                        } else {
                            ret_r.Value = numerResult.Value / denomResult.Value;
                        }
                    } else {
                        throw new Exception("Unexpected state");
                    }

                    yield return ret_r;
                }
            }
        }

        public StatisticsResult First()
        {
            return this.FindAll().First();
        }

        public StatisticsResult Single()
        {
            return this.FindAll().Single();
        }
    }
}