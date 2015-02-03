using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Diagnostics;

namespace LiiteriStatisticsCore.Repositories
{
    public class StatisticsResultRepository :
        SqlReadRepository<Models.StatisticsResult>
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /* IndicatorDetails is needed so we know how to make unit conversions
         * Alternatively move MakeUnitConversions away from this class */
        private Models.IndicatorDetails _Indicator;
        public Models.IndicatorDetails Indicator
        {
            get {
                return this._Indicator;
            }
            set
            {
                this._Indicator = value;
                if (!this.Modifiers.Contains(this.MakeUnitConversions)) {
                    this.Modifiers.Add(this.MakeUnitConversions);
                }
                if (!this.Modifiers.Contains(this.SetDecimalCount)) {
                    this.Modifiers.Add(this.SetDecimalCount);
                }
            }
        }

        public StatisticsResultRepository(DbConnection dbConnection) :
            base(dbConnection)
        {
        }

        private Models.StatisticsResult ApplyPrivacyLimits(
            Models.StatisticsResult obj, Models.StatisticsResult refobj)
        {
            if (this.Indicator.PrivacyLimit == null) {
                /* shouldn't even be here */
                Debug.WriteLine(
                    "We were asked to apply privacy limits, but the indicator has the PrivacyLimit member set to NULL!");
                return obj;
            }
            if (this.Indicator.PrivacyLimit.GreaterThan != null) {
                if (refobj.Value <= this.Indicator.PrivacyLimit.GreaterThan) {
                    obj.Value = null;
                    obj.PrivacyLimitTriggered = true;
                }
            }
            return obj;
        }

        private Models.StatisticsResult MakeUnitConversions(
            Models.StatisticsResult obj)
        {
            switch (this.Indicator.InternalUnitId) {
                case 12: // osuus
                    switch (this.Indicator.DisplayUnitId) {
                        case 1: // %
                            obj.Value = (decimal) ((double) obj.Value * 100);
                            break;
                    }
                    break;
                case 10: // m2
                    switch (this.Indicator.DisplayUnitId) {
                        case 14: // ha
                            obj.Value = (decimal) ((double) obj.Value / 100000);
                            break;
                    }
                    break;
            }
            return obj;
        }

        private Models.StatisticsResult SetDecimalCount(
            Models.StatisticsResult obj)
        {
            if (obj.Value == null ||
                    this.Indicator.DecimalCount == null) {
                return obj;
            }
            obj.Value = decimal.Round((decimal) obj.Value,
                (int) this.Indicator.DecimalCount);
            return obj;
        }

        public IEnumerable<Models.StatisticsResult> FindAll(
            IEnumerable<Queries.ISqlQuery> queries)
        {
            foreach (Queries.ISqlQuery query in queries) {
                foreach (Models.StatisticsResult r in this.FindAll(query)) {
                    yield return r;
                }
            }
        }

        /* pairs of queries:
         * left = actual query,
         * right = reference query for privacy limits */
        public IEnumerable<Models.StatisticsResult> FindAll(
            IEnumerable<Tuple<Queries.ISqlQuery, Queries.ISqlQuery>> queries)
        {
            foreach (Tuple<Queries.ISqlQuery, Queries.ISqlQuery> querypair
                    in queries) {
                /* Actual statistics */
                Queries.ISqlQuery query1 = querypair.Item1;
                /* Reference statistics used for checking privacy limits */
                Queries.ISqlQuery query2 = querypair.Item2;

                /* Iterate both actual query and reference query, but
                 * at varying speeds. The list may not be equal, but both
                 * should be sorted by AreaId */

                using (IEnumerator<Models.StatisticsResult> e1 =
                        this.FindAll(query1).GetEnumerator())
                using (IEnumerator<Models.StatisticsResult> e2 =
                        this.FindAll(query2).GetEnumerator()) {
                    Models.StatisticsResult ref_r = null;
                    e2.MoveNext();
                    ref_r = e2.Current;
                    /* logger.Debug(string.Format(
                        "ref not yet fetched, fetched it now and got {0}",
                        ref_r.AreaId)); */

                    while (e1.MoveNext()) {
                        Models.StatisticsResult r = e1.Current;
                        /* logger.Debug(string.Format(
                            "stepped forward actual query and got {0}",
                            r.AreaId)); */

                        /* if actual result is greater than current
                         * reference result, reference result needs to
                         * be iterated until is equal to or greater
                         * than actual result */
                        while (ref_r.AreaId < r.AreaId && e2.MoveNext()) {
                            /* logger.Debug(string.Format(
                                "ref ({0}) was less than actual {1}, so stepping ref forward and got {2}",
                                ref_r.AreaId, r.AreaId, e2.Current.AreaId)); */
                            ref_r = e2.Current;
                        }

                        /* if our current reference matches the current result,
                         * apply privacy limits using it. */
                        if (ref_r.AreaId == r.AreaId) {
                            /* logger.Debug(string.Format(
                                "ref ({0}) matches actual {1}, so applying privacy limits",
                                ref_r.AreaId, r.AreaId)); */
                            r = this.ApplyPrivacyLimits(r, ref_r);
                        /*
                        } else {
                            logger.Debug(string.Format(
                                "ref ({0}) does not match actual {1}, so skipping privacy limits",
                                ref_r.AreaId, r.AreaId));
                        */
                        }

                        yield return r;
                    }
                }
            }
        }

        public override IEnumerable<Models.StatisticsResult>
            FindAll(Queries.ISqlQuery query)
        {
            return this.FindAll(query,
                new Factories.StatisticsResultFactory());
        }

        public override Models.StatisticsResult Single(Queries.ISqlQuery query)
        {
            return this.FindAll(query).Single();
        }

        public override Models.StatisticsResult First(Queries.ISqlQuery query)
        {
            return this.FindAll(query).First();
        }
    }
}