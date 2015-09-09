using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using LiiteriStatisticsCore.Models;

namespace LiiteriStatisticsCore.Repositories
{
    public class UnitConversionStatisticsRepository :
        IReadRepository<StatisticsResult>
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IndicatorDetails Details;
        private IReadRepository<StatisticsResult> Repository;

        public UnitConversionStatisticsRepository(
            IndicatorDetails details,
            IReadRepository<StatisticsResult> repository)
        {
            this.Details = details;
            this.Repository = repository;
        }

        private StatisticsResult MakeUnitConversions(StatisticsResult obj)
        {
            switch (this.Details.InternalUnitId) {
                case 12: // osuus
                    switch (this.Details.DisplayUnitId) {
                        case 1: // %
                            obj.Value = (decimal) ((double) obj.Value * 100);
                            break;
                    }
                    break;
                case 10: // m2
                    switch (this.Details.DisplayUnitId) {
                        case 14: // ha
                            obj.Value = (decimal) ((double) obj.Value / 10000);
                            break;
                    }
                    break;
            }
            return obj;
        }

        private StatisticsResult SetDecimalCount(StatisticsResult obj)
        {
            if (obj.Value == null ||
                    this.Details.DecimalCount == null) {
                return obj;
            }
            obj.Value = decimal.Round((decimal) obj.Value,
                (int) this.Details.DecimalCount);
            return obj;
        }

        public IEnumerable<StatisticsResult> FindAll()
        {
            using (IEnumerator<StatisticsResult> enumerator =
                    this.Repository.FindAll().GetEnumerator()) {
                while (enumerator.MoveNext()) {
                    var ret_r = (StatisticsResult) enumerator.Current.Clone();
                    ret_r = this.MakeUnitConversions(ret_r);
                    ret_r = this.SetDecimalCount(ret_r);
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