using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

using System.ServiceModel; // WCF

namespace LiiteriStatisticsCore.Controllers
{
    [ServiceContract]
    public interface ICommuteStatisticsController
    {
        [OperationContract]
        IEnumerable<Models.CommuteStatisticsIndicator>
            GetCommuteStatisticsIndicators();

        [OperationContract]
        IEnumerable<Models.StatisticsResult> GetCommuteStatistics(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null);

        [OperationContract]
        string GetCommuteStatisticsDebugString(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null);
    }

    public class CommuteStatisticsController : ICommuteStatisticsController
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Util.AreaTypeMappings
            AreaTypeMappings = new Util.AreaTypeMappings();

        private class StatisticsResultContainer
        {
            public string DebugString;
            public IEnumerable<Models.StatisticsResult> Results;
        }

        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        public IEnumerable<Models.CommuteStatisticsIndicator>
            GetCommuteStatisticsIndicators()
        {
            using (DbConnection db = this.GetDbConnection()) {
                return new Repositories
                    .CommuteStatisticsIndicatorRepository(db)
                    .GetAll();
            }
        }

        private StatisticsResultContainer GetCommuteStatisticsResultContainer(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null,
            bool debug = false)
        {
            using (DbConnection db = this.GetDbConnection()) {

                var indicator = new Repositories
                    .CommuteStatisticsIndicatorRepository(db)
                    .Get(statisticsId);

                var queries = new List<Queries.CommuteStatisticsQuery>();

                foreach (int year in years) {
                    var query = new Queries.CommuteStatisticsQuery();
                    query.GroupByAreaTypeIdIs = group;
                    query.YearIs = year;
                    query.TableName = indicator.TableName;
                    query.GenderIs = gender;

                    if (work_filter != null && work_filter.Length == 0) {
                        work_filter = null;
                    }
                    query.WorkFilterQueryString = work_filter;

                    if (home_filter != null && home_filter.Length == 0) {
                        home_filter = null;
                    }
                    query.HomeFilterQueryString = home_filter;

                    query.Type = type;

                    queries.Add(query);
                }

                if (debug) {
                    Util.DebugOutput debugOutput;
                    if (queries.Count > 0) {
                        debugOutput = new Util.DebugOutput(queries);
                    } else {
                        throw new Exception("No statistics queries specified!");
                    }
                    return new StatisticsResultContainer() {
                        DebugString = debugOutput.ToString(),
                        Results = null
                    };
                    /* We could continue here and fill the actual results,
                     * but if there's an error we might rather just want
                     * to see the query */
                }

                IEnumerable<Models.StatisticsResult> results;
                if (queries.Count > 0) {
                    var repository =
                        new Repositories.NormalStatisticsRepository(
                            db, queries);
                    results = repository.FindAll();
                } else {
                    throw new Exception("No statistics queries specified!");
                }

                /* Note: we are iterating the generator here, could be
                 * memory-inefficient */
                return new StatisticsResultContainer() {
                    DebugString = null,
                    Results = results.ToList()
                };
            }
        }

        public IEnumerable<Models.StatisticsResult> GetCommuteStatistics(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null)
        {
            return this.GetCommuteStatisticsResultContainer(
                statisticsId,
                years,
                type,
                gender,
                group,
                work_filter,
                home_filter,
                false).Results;
        }

        public string GetCommuteStatisticsDebugString(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null)
        {
            return this.GetCommuteStatisticsResultContainer(
                statisticsId,
                years,
                type,
                gender,
                group,
                work_filter,
                home_filter,
                true).DebugString;
        }
    }
}