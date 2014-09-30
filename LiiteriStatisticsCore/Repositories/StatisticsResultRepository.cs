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
                            obj.Value = (decimal) ((double) obj.Value / 100);
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

                using (IEnumerator<Models.StatisticsResult> e1 =
                        this.FindAll(query1).GetEnumerator())
                using (IEnumerator<Models.StatisticsResult> e2 =
                        this.FindAll(query2).GetEnumerator()) {
                    while (e1.MoveNext() && e2.MoveNext()) {
                        /* NOTE: e2 (reference statistics) will also 
                         * have modifier methods applied on it by
                         * the repository */
                        Models.StatisticsResult r1 = e1.Current;
                        Models.StatisticsResult r2 = e2.Current;
                        /* We are supposed to be comparing two result tables
                         * side-by-side, check here that we are doing so */
                        if (r1.AreaId != r2.AreaId ||
                                r1.Year != r2.Year) {
                            throw new Exception(
                                "Parallel result checking de-sync!");
                        }
                        r1 = this.ApplyPrivacyLimits(r1, r2);
                        yield return r1;
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