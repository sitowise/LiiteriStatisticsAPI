using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

using System.Runtime.Serialization; // DataContract / DataMember
using System.ServiceModel; // WCF

namespace LiiteriStatisticsCore.Controllers
{
    [ServiceContract]
    public interface IStatisticsController
    {
        [OperationContract]
        IEnumerable<Models.StatisticsResult> GetStatistics(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null);

        [OperationContract]
        string GetStatisticsDebugString(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null);

        [OperationContract]
        IEnumerable<Models.AreaType> GetAreaTypes();

        [OperationContract]
        IEnumerable<Models.Area> GetAreas(string areaTypeId);
    }

    public class StatisticsController : IStatisticsController
    {
        private static readonly log4net.ILog logger =
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

        /* be prepared to return either a debug string, or the actual
         * results */
        /* TODO: Remove the container encapsulation,
         * or reimplement it along with debug */
        private StatisticsResultContainer GetStatisticsResultContainer(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null,
            bool debug = false)
        {
            using (DbConnection db = this.GetDbConnection()) {

                var request = new Requests.StatisticsRequest() {
                    StatisticsId = statisticsId,
                    Years = years,
                    Group = group,
                    Filter = filter,
                };

                var repofactory = new Factories.StatisticsRepositoryFactory(
                    db, request);
                var repository = repofactory.GetRepository();

                /* debug snipped away from here */

                IEnumerable<Models.StatisticsResult> results;
                results = repository.FindAll();

                /* Note: we are iterating the generator here, could be
                 * memory-inefficient */
                return new StatisticsResultContainer() {
                    DebugString = null,
                    Results = results.ToList()
                };
            }
        }

        public virtual IEnumerable<Models.StatisticsResult> GetStatistics(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null)
        {
            return this.GetStatisticsResultContainer(
                years,
                statisticsId,
                group,
                filter,
                false).Results;
        }

        public string GetStatisticsDebugString(
            int[] years,
            int statisticsId,
            string group = null,
            string filter = null)
        {
            return this.GetStatisticsResultContainer(
                years,
                statisticsId,
                group,
                filter,
                true).DebugString;
        }

        public virtual IEnumerable<Models.AreaType> GetAreaTypes()
        {
            return AreaTypeMappings.GetAreaTypes();
        }

        public virtual IEnumerable<Models.Area> GetAreas(string areaTypeId)
        {
            var query = new Queries.AreaQuery();
            query.AreaTypeIdIs = areaTypeId;
            using (DbConnection db = this.GetDbConnection()) {
                var repository = new Repositories.AreaRepository(
                    db, new Queries.ISqlQuery[] { query });
                foreach (Models.Area r in repository.FindAll()) {
                    yield return r;
                }
            }
        }
    }
}