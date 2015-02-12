using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

using System.ServiceModel; // WCF

using Core = LiiteriStatisticsCore;

namespace LiiteriStatisticsCore.Controllers
{
    [ServiceContract]
    public interface ICommuteStatisticsController
    {
        [OperationContract]
        IEnumerable<Core.Models.CommuteStatisticsIndicator>
            GetCommuteStatisticsIndicators();

        [OperationContract]
        IEnumerable<Core.Models.StatisticsResult> GetCommuteStatistics(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null,
            bool debug = false);
    }

    // used for WebAPI POST requests
    public class CommuteStatisticsRequest
    {
        public int[] years { get; set; }
        public string type { get; set; }
        public int gender { get; set; }
        public string group { get; set; }
        public string work_filter { get; set; }
        public string home_filter { get; set; }
    }


    public class CommuteStatisticsController : ICommuteStatisticsController
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static Core.Util.AreaTypeMappings
            AreaTypeMappings = new Core.Util.AreaTypeMappings();

        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        public IEnumerable<Core.Models.CommuteStatisticsIndicator>
            GetCommuteStatisticsIndicators()
        {
            return new Core.Repositories
                .CommuteStatisticsIndicatorRepository().GetAll();
        }

        public IEnumerable<Core.Models.StatisticsResult> GetCommuteStatistics(
            int statisticsId,
            int[] years,
            string type = "yht",
            int gender = 0,
            string group = null,
            string work_filter = null,
            string home_filter = null,
            bool debug = false)
        {
            var indicator = new Core.Repositories
                .CommuteStatisticsIndicatorRepository().Get(statisticsId);

            using (DbConnection db = this.GetDbConnection()) {
                var queries = new List<Core.Queries.CommuteStatisticsQuery>();

                foreach (int year in years) {
                    var query = new Core.Queries.CommuteStatisticsQuery();
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

                /* this debug output is the reason we've declared the
                 * entire controller as HttpResponseMessage */
                /*
                if (debug) {
                    DebugOutput debugOutput;
                    if (queries.Count > 0) {
                        debugOutput = new DebugOutput(queries);
                    } else {
                        throw new Exception("No statistics queries specified!");
                    }
                    return Request.CreateResponse(
                        HttpStatusCode.OK,
                        debugOutput.ToString(),
                        new Formatters.TextPlainFormatter());
                }
                */

                var repository =
                    new Core.Repositories.StatisticsResultRepository(db);

                IEnumerable<Core.Models.StatisticsResult> results;
                if (queries.Count > 0) {
                    results = repository.FindAll(queries);
                } else {
                    throw new Exception("No statistics queries specified!");
                }

                /* Note: we are iterating the generator here, could be
                 * memory-inefficient */
                return results.ToList();
                /*
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    results.ToList());
                */
            }
        }
    }
}