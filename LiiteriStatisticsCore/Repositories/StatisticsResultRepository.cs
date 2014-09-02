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
            if (this.Indicator.DecimalCount == null) {
                return obj;
            }
            obj.Value = decimal.Round(obj.Value,
                (int) this.Indicator.DecimalCount);
            return obj;
        }

        public IEnumerable<Models.StatisticsResult> FindAll(
            IEnumerable<Queries.ISqlQuery> queries)
        {
            var entityList = new List<Models.StatisticsResult>();
            foreach (Queries.ISqlQuery query in queries) {
                entityList.AddRange(this.FindAll(query));
            }
            return entityList;
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