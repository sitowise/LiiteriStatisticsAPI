using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiiteriStatisticsCore.Models;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Repositories
{
    class PrivacyLimitStatisticsRepository :
        ComparingStatisticsRepository,
        IStatisticsRepository
    {
        private PrivacyLimit Limit;

        public PrivacyLimitStatisticsRepository(
            PrivacyLimit privacyLimit,
            IStatisticsRepository mainRepository,
            IStatisticsRepository refRepository) : base()
        {
            this.Limit = privacyLimit;

            this.Repositories = new List<IStatisticsRepository>() {
                refRepository,
                mainRepository,
            };
        }

        public override bool MaySkipPrivacyLimits
        {
            get
            {
                throw new Exception("Not valid here!");
            }
        }

        protected override StatisticsResult Compare(StatisticsResult[] results)
        {
            if (results.Length != 2) {
                throw new ArgumentException(
                    "Should have 2 values, instead got " +
                    results.Length.ToString());
            }

            StatisticsResult refval = results[0]; // value we compare against
            StatisticsResult val = results[1]; // actual value

            StatisticsResult ret_r;

            /* we assume here that refval and val are never both null */

            if (refval == null) { // ref null, nothing to do
                return (StatisticsResult) val.Clone();
            }

            if (val == null) { // val null, create a fake result from ref
                ret_r = (StatisticsResult) refval.Clone();
                ret_r.Value = 0;
            } else {
                ret_r = (StatisticsResult) val.Clone();
            }

            if (refval.Value <= this.Limit.GreaterThan) {
                ret_r.Value = null;
                ret_r.PrivacyLimitTriggered = true;
            }

            return ret_r;
        }
    }
}