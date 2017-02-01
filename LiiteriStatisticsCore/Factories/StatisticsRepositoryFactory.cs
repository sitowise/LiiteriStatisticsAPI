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
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private DbConnection db;
        private Requests.StatisticsRequest Request;
        public StatisticsRepositoryTracer Tracer;

        private bool SkipPrivacyLimits = true;

        public StatisticsRepositoryFactory(
            DbConnection dbConnection,
            Requests.StatisticsRequest request) :
            this(dbConnection, request, new StatisticsRepositoryTracer())
        {
        }

        private StatisticsRepositoryFactory(
            DbConnection dbConnection,
            Requests.StatisticsRequest request,
            StatisticsRepositoryTracer tracer)
        {
            this.db = dbConnection;
            this.Request = request;

            this.Tracer = tracer.CreateChild();
            this.Tracer.Request = this.Request;

            if (++this.Request.RecursionDepth > 15) {
                throw new Exception(
                    "Chained repositories reached recursion limit");
            }

            // a foolproof way of making sure we will only do this once
            this.SkipPrivacyLimits = request.SkipPrivacyLimits;
            this.Request.SkipPrivacyLimits = true;
        }

        private StatisticsRepositoryFactory GetFactory(
            Requests.StatisticsRequest request)
        {
            var factory = new StatisticsRepositoryFactory(
                this.db, request, this.Tracer);
            return factory;
        }

        public IStatisticsRepository GetRepository()
        {
            var indicatorQuery = new IndicatorQuery();
            indicatorQuery.IdIs = this.Request.StatisticsId;
            indicatorQuery.IncludeHelperStatistics = true;

            var indicatorDetailsRepository =
                new IndicatorDetailsRepository(
                    this.db,
                    new ISqlQuery[] { indicatorQuery });
            var details = indicatorDetailsRepository.Single();

            IStatisticsRepository repo;

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
                        repo = this.GetSpecialRepository(details);
                        break;
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

            this.Tracer.Repository = repo;

            return repo;
        }

        private UnitConversionStatisticsRepository GetUnitConversionRepository(
            IndicatorDetails details)
        {
            var request = (Requests.StatisticsRequest) this.Request.Clone();

            // this is important, do not go into infinite loop
            request.SkipUnitConversions = true;

            var mainrepo = this.GetFactory(request).GetRepository();
            var convrepo = new UnitConversionStatisticsRepository(
                details, mainrepo);

            return convrepo;
        }

        private IStatisticsRepository GetPrivacyLimitRepository(
            IndicatorDetails details)
        {
            var mainrequest =
                (Requests.StatisticsRequest) this.Request.Clone();

            var mainrepo = this.GetFactory(mainrequest).GetRepository();

            /* SUPPORT-14527 / YM-654
             * Privacy limits should be skipped for administrative areas.
             */
            if (mainrepo.MaySkipPrivacyLimits) {
                logger.Debug("Skipping privacy limits");
                return mainrepo;
            }

            var refrequest =
                (Requests.StatisticsRequest) this.Request.Clone();
            refrequest.StatisticsId = details.PrivacyLimit.RefId;
            var refrepo = this.GetFactory(refrequest).GetRepository();

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

                var query = new StatisticsQuery(this.Request.StatisticsId);

                query.BlockedAreaTypes = timePeriod.BlockedAreaTypes;
                query.CalculationTypeIdIs = details.CalculationType;
                query.AvailableAreaTypes = availableAreaTypes;

                if (group == null) group = "finland";
                query.GroupByAreaTypeIdIs = group;
                query.YearIs = year;

                if (this.Request.AreaYear != null) {
                    query.AreaYearIs = (int) this.Request.AreaYear;
                }

                if (filter != null && filter.Length == 0) {
                    filter = null;
                }
                query.AreaFilterQueryString = filter;

                queries.Add(query);
            }

            var repo = new NormalStatisticsRepository(
                this.db, queries.ToArray());

            // the tracer is placed in the repo so we can record DB query time
            repo.Tracer = this.Tracer;
            return repo;
        }

        private SpecialStatisticsRepository GetSpecialRepository(
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

                var query = new SpecialStatisticsQuery(this.Request.StatisticsId);

                query.CalculationTypeIdIs = details.CalculationType;
                query.AvailableAreaTypes = availableAreaTypes;

                if (group == null) group = "finland";
                query.GroupByAreaTypeIdIs = group;
                query.YearIs = year;

                if (filter != null && filter.Length == 0) {
                    filter = null;
                }
                query.AreaFilterQueryString = filter;

                queries.Add(query);
            }

            var repo = new SpecialStatisticsRepository(
                this.db, queries.ToArray());

            // the tracer is placed in the repo so we can record DB query time
            repo.Tracer = this.Tracer;
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
            var denomrepo = this.GetFactory(denomreq).GetRepository();

            var numerreq = (Requests.StatisticsRequest) this.Request.Clone();
            numerreq.StatisticsId = numerstatid;
            var numerrepo = this.GetFactory(numerreq).GetRepository();

            var repo = new DividingStatisticsRepository(denomrepo, numerrepo);

            return repo;
        }

        private SummingStatisticsRepository GetSummingRepository(
            IndicatorDetails details)
        {
            var repos = new List<IStatisticsRepository>();

            if (details.DerivedStatistics.Length < 2) {
                throw new ArgumentException(
                    "Was excepting at least 2 derived statistics, instead got " +
                    details.DerivedStatistics.Length);
            }

            foreach (int statisticsId in details.DerivedStatistics) {
                var request = (Requests.StatisticsRequest) this.Request.Clone();
                request.StatisticsId = statisticsId;
                var subrepo = this.GetFactory(request).GetRepository();
                repos.Add(subrepo);
            }

            return new SummingStatisticsRepository(repos);
        }
    }
}