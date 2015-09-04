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

        public StatisticsRepositoryFactory(
            DbConnection dbConnection,
            Requests.StatisticsRequest request)
        {
            this.db = dbConnection;
            this.Request = request;
        }

        public IReadRepository<StatisticsResult> GetRepository()
        {
            var indicatorQuery = new IndicatorQuery();
            indicatorQuery.IdIs = this.Request.StatisticsId;

            var indicatorDetailsRepository =
                new IndicatorDetailsRepository(
                    this.db,
                    new ISqlQuery[] { indicatorQuery });
            var details = indicatorDetailsRepository.Single();

            IReadRepository<StatisticsResult> repo;

            if (details.CalculationType == 1) {
                repo = this.GetNormalRepository(details);
            } else {
                throw new NotImplementedException();
            }

            Debug.WriteLine(string.Format(
                "Repository for {0} determined to be {1}",
                Request.StatisticsId, repo.GetType().ToString()));

            return repo;
        }

        public NormalStatisticsRepository GetNormalRepository(
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

        public IReadRepository<StatisticsResult> GetDerivedDividedRepository()
        {
            throw new NotImplementedException();
        }
    }
}