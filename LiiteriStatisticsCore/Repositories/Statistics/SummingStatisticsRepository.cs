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
        IReadRepository<StatisticsResult>
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        IEnumerable<IReadRepository<StatisticsResult>> Repositories;

        public SummingStatisticsRepository(
            IEnumerable<IReadRepository<StatisticsResult>> repositories)
        {
            this.Repositories = repositories;
        }

        /* helper class for processing the results in parallel */
        private class RState
        {
            public IEnumerator<StatisticsResult> Enumerator;
            public StatisticsResult Value;
        }

        /* Call .MoveNext() on all Enumerators, except the ones
         * that are ahead of the currently smallest AreaId */
        private bool MoveAll(IEnumerable<RState> rstates)
        {
            bool found = false;

            /* find out the smallest AreaId, so we can only increment
             * enumerators which are currently at that AreaId */
            int[] areaIds = rstates
                .Where(x => x.Value != null)
                .Select(x => x.Value.AreaId)
                .ToArray();
            int? min = null;
            if (areaIds.Length > 0) {
                min = areaIds.Min();
            }

            foreach (RState rstate in rstates) {
                /* only increment if this is the first iteration,
                   or if this is the lowest AreaId. Other AreaIds have
                   jumped ahead, and will have to wait for their turn. */
                if (min == null || rstate.Value.AreaId == min) {
                    if (rstate.Enumerator.MoveNext()) {
                        rstate.Value = rstate.Enumerator.Current;
                        found = true;
                    } else {
                        rstate.Value = null;
                    }
                }
            }

            return found;
        }


        public IEnumerable<StatisticsResult> FindAll()
        {
            var rstates = new List<RState>();

            /* collect enumerators in a list */
            foreach (IReadRepository<StatisticsResult> repo in
                    this.Repositories) {
                rstates.Add(new RState() {
                    Enumerator = repo.FindAll().GetEnumerator()
                });
            }

            /* this loop should continue for as long as there is at least one
             * successfull .MoveNext() */
            while (this.MoveAll(rstates)) {
                int min = rstates
                    .Where(x => x.Value != null)
                    .Min(x => x.Value.AreaId);

                decimal[] values = rstates
                    .Where(x => x.Value != null && x.Value.AreaId == min)
                    .Select(x => (decimal) x.Value.Value)
                    .ToArray();

                var ret_r = (StatisticsResult) rstates.First().Value.Clone();
                ret_r.Value = values.Sum();
                yield return ret_r;
            }

            foreach (RState rstate in rstates) {
                rstate.Enumerator.Dispose();
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