using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using LiiteriStatisticsCore.Repositories;
using LiiteriStatisticsCore.Models;
using LiiteriStatisticsCore.Queries;

namespace LiiteriStatisticsCore.Factories
{
    public class StatisticsRepositoryFactory
    {
        private DbConnection db;
        private Requests.StatisticsRequest Request;

        private bool SkipPrivacyLimits = true;

        public StatisticsRepositoryFactory(
            DbConnection dbConnection,
            Requests.StatisticsRequest request)
        {
            this.db = dbConnection;
            this.Request = request;

            if (++this.Request.RecursionDepth > 15) {
                throw new Exception(
                    "Chained repositories reached recursion limit");
            }

            // a foolproof way of making sure we will only do this once
            this.SkipPrivacyLimits = request.SkipPrivacyLimits;
            this.Request.SkipPrivacyLimits = true;
        }

        public IReadRepository<StatisticsResult> GetRepository()
        {
            var indicatorQuery = new IndicatorQuery();
            indicatorQuery.IdIs = this.Request.StatisticsId;
            indicatorQuery.IncludeHelperStatistics = true;

            var indicatorDetailsRepository =
                new IndicatorDetailsRepository(
                    this.db,
                    new ISqlQuery[] { indicatorQuery });
            var details = indicatorDetailsRepository.Single();

            IReadRepository<StatisticsResult> repo;

            if (!this.SkipPrivacyLimits && details.PrivacyLimit != null) {
                repo = this.GetPrivacyLimitRepository(details);
            } else if (!this.Request.SkipUnitConversions) {
                /* privacy limits should be done on values that have gone
                 * through unit conversions, so we do the conversion here
                 * before the normal repositories */
                repo = this.GetUnitConversionRepository(details);
            } else {
                switch (details.CalculationType) {
                    case 1: // normal
                        repo = this.GetNormalRepository(details);
                        break;
                    case 2: // helper
                        repo = this.GetNormalRepository(details);
                        break;
                    case 3: // derived & divided
                        repo = this.GetDividingRepository(details);
                        break;
                    case 4: // special
                        throw new NotImplementedException();
                        //break;
                    case 5: // derived & summed
                        repo = this.GetSummingRepository(details);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            Debug.WriteLine(string.Format(
                "Repository for {0} determined to be {1}",
                Request.StatisticsId, repo.GetType().ToString()));

            return repo;
        }

        private UnitConversionStatisticsRepository GetUnitConversionRepository(
            IndicatorDetails details)
        {
            var request = (Requests.StatisticsRequest) this.Request.Clone();

            // this is important, do not go into infinite loop
            request.SkipUnitConversions = true;

            var mainrepo = new StatisticsRepositoryFactory(
                this.db, request).GetRepository();
            var convrepo = new UnitConversionStatisticsRepository(
                details, mainrepo);

            return convrepo;
        }

        private PrivacyLimitStatisticsRepository GetPrivacyLimitRepository(
            IndicatorDetails details)
        {
            var mainrequest =
                (Requests.StatisticsRequest) this.Request.Clone();

            var mainrepo =
                new StatisticsRepositoryFactory(this.db, mainrequest)
                .GetRepository();

            var refrequest =
                (Requests.StatisticsRequest) this.Request.Clone();
            refrequest.StatisticsId = details.PrivacyLimit.RefId;
            var refrepo =
                new StatisticsRepositoryFactory(this.db, refrequest)
                .GetRepository();

            var repo = new PrivacyLimitStatisticsRepository(
                details.PrivacyLimit, mainrepo, refrepo);
            return repo;
        }

        private NormalStatisticsRepository GetNormalRepository(
            IndicatorDetails details)
        {
            string group = this.Request.Group;
            string filter = this.Request.Filter;

            /* Step 2: Create one or more StatisticsQuery objects */
            var queries = new List<StatisticsQuery>();

            /* although StatisticsQuery could implement .YearIn, which 
             * would accept a list of years, what about if different years
             * end up having different DatabaseAreaTypes?
             * For this reason, let's just loop the years and create
             * multiple queries */
            foreach (int year in this.Request.Years) {
                TimePeriod timePeriod = (
                    from p in details.TimePeriods
                    where p.Id == year
                    select p).Single();
                int[] availableAreaTypes = (
                    from a in timePeriod.DataAreaTypes
                    select a.Id).ToArray();

                var statisticsQuery = new StatisticsQuery(
                    this.Request.StatisticsId);

                statisticsQuery.CalculationTypeIdIs = details.CalculationType;
                statisticsQuery.AvailableAreaTypes = availableAreaTypes;

                if (group == null) group = "finland";
                statisticsQuery.GroupByAreaTypeIdIs = group;
                statisticsQuery.YearIs = year;

                if (filter != null && filter.Length == 0) {
                    filter = null;
                }
                statisticsQuery.AreaFilterQueryString = filter;

                /* at this point the statisticsQuery should be ready,
                 * let's process it here so we can decide if privacy limits
                 * can be applied here */
                statisticsQuery.GenerateQueryString();

                queries.Add(statisticsQuery);
            }

            var repo = new NormalStatisticsRepository(
                this.db, queries.ToArray());
            return repo;
        }

        private DividingStatisticsRepository GetDividingRepository(
            IndicatorDetails details)
        {
            if (details.DerivedStatistics.Length != 2) {
                throw new ArgumentException(
                    "Was excepting 2 derived statistics, instead got " +
                    details.DerivedStatistics.Length);
            }
            var denomstatid = details.DerivedStatistics[0];
            var numerstatid = details.DerivedStatistics[1];

            var denomreq = (Requests.StatisticsRequest) this.Request.Clone();
            denomreq.StatisticsId = denomstatid;
            var denomrepo =
                new StatisticsRepositoryFactory(this.db, denomreq)
                .GetRepository();

            var numerreq = (Requests.StatisticsRequest) this.Request.Clone();
            numerreq.StatisticsId = numerstatid;
            var numerrepo =
                new StatisticsRepositoryFactory(this.db, numerreq)
                .GetRepository();

            var repo = new DividingStatisticsRepository(denomrepo, numerrepo);

            return repo;
        }

        private SummingStatisticsRepository GetSummingRepository(
            IndicatorDetails details)
        {
            var repos = new List<IReadRepository<StatisticsResult>>();

            if (details.DerivedStatistics.Length < 2) {
                throw new ArgumentException(
                    "Was excepting at least 2 derived statistics, instead got " +
                    details.DerivedStatistics.Length);
            }

            foreach (int statisticsId in details.DerivedStatistics) {
                var request = (Requests.StatisticsRequest) this.Request.Clone();
                request.StatisticsId = statisticsId;
                var subrepo = new StatisticsRepositoryFactory(this.db, request)
                    .GetRepository();
                repos.Add(subrepo);
            }

            return new SummingStatisticsRepository(repos);
        }
    }
}