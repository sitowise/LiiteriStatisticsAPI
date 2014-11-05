using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using System.Data.SqlClient;
using System.Data.Common;

using System.Diagnostics;
using System.Configuration;

using LiiteriStatisticsCore.Util;
using LiiteriStatisticsCore.Models;
using LiiteriStatisticsCore.Queries;
using LiiteriStatisticsCore.Repositories;

namespace LiiteriDataAPI.Controllers
{
    [RoutePrefix("v1")]
    public class CommuteStatisticsController : ApiController
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static AreaTypeMappings
            AreaTypeMappings = new AreaTypeMappings();

        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        [Route("commuteStatistics/")]
        [HttpGet]
        public HttpResponseMessage GetCommuteStatistics(
            int[] years,
            string type,
            string group = null,
            string work_filter = null,
            string home_filter = null,
            bool debug = false)
        {
            using (DbConnection db = this.GetDbConnection()) {
                var queries = new List<CommuteStatisticsQuery>();

                foreach (int year in years) {
                    var query = new CommuteStatisticsQuery();
                    query.GroupByAreaTypeIdIs = group;
                    query.YearIs = years[0];

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

                var repository = new StatisticsResultRepository(db);

                IEnumerable<StatisticsResult> results;
                if (queries.Count > 0) {
                    results = repository.FindAll(queries);
                } else {
                    throw new Exception("No statistics queries specified!");
                }

                /* Note: we are iterating the generator here, could be
                 * memory-inefficient */
                return Request.CreateResponse(
                    HttpStatusCode.OK,
                    results.ToList());
            }
        }
    }
}