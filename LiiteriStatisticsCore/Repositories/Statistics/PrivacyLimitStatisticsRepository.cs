using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsCore.Repositories
{
    class PrivacyLimitStatisticsRepository :
        IReadRepository<StatisticsResult>
    {
        // Repository which we are applying the privacy limits to
        private IReadRepository<StatisticsResult> MainRepository;

        // Repository which we are comparing against
        private IReadRepository<StatisticsResult> RefRepository;

        private PrivacyLimit Limit;

        public PrivacyLimitStatisticsRepository(
            PrivacyLimit privacyLimit,
            IReadRepository<StatisticsResult> mainRepository,
            IReadRepository<StatisticsResult> refRepository)
        {
            this.Limit = privacyLimit;
            this.MainRepository = mainRepository;
            this.RefRepository = refRepository;
        }

        public IEnumerable<StatisticsResult> FindAll()
        {
            using (IEnumerator<StatisticsResult> mainEnumerator =
                    this.MainRepository.FindAll().GetEnumerator())
            using (IEnumerator<StatisticsResult> refEnumerator =
                    this.RefRepository.FindAll().GetEnumerator()) {
                StatisticsResult refResult;
                StatisticsResult mainResult;
                StatisticsResult ret_r;

                refEnumerator.MoveNext();
                refResult = refEnumerator.Current;
                while (mainEnumerator.MoveNext()) {
                    mainResult = mainEnumerator.Current;
                    ret_r = (StatisticsResult) mainResult.Clone();

                    // ref already at the end of data
                    if (refResult == null) {
                        ret_r.Value = 0;
                        yield return ret_r;
                        continue;
                    }

                    while (refResult.AreaId < mainResult.AreaId) {
                        if (refEnumerator.MoveNext()) {
                            refResult = refEnumerator.Current;
                        } else {
                            refResult = null;
                            break;
                        }
                    }

                    // ref just reached end of data
                    if (refResult == null) {
                        ret_r.Value = 0;
                        yield return ret_r;
                        continue;
                    }

                    if (refResult.AreaId > mainResult.AreaId) {
                        /* ref hopped past main, ret_r is fine as is */
                    } else if (refResult.AreaId == mainResult.AreaId) {
                        /* NOTE: this should be done on values
                         * that have already gone through unit conversion */
                        if (refResult.Value <= this.Limit.GreaterThan) {
                            ret_r.Value = null;
                            ret_r.PrivacyLimitTriggered = true;
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
            throw new NotImplementedException();
        }

        public StatisticsResult Single()
        {
            throw new NotImplementedException();
        }
    }
}