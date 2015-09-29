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
    public interface IIndicatorController
    {
        [OperationContract]
        IEnumerable<Models.IndicatorBrief> GetIndicators(
            string name = null,
            int? accessRight = null);

        [OperationContract]
        Models.IndicatorDetails GetIndicatorDetails(int id);
    }

    public class IndicatorController : IIndicatorController
    {
        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        public virtual IEnumerable<Models.IndicatorBrief> GetIndicators(
            string name = null,
            int? accessRight = null)
        {
            var query = new Queries.IndicatorQuery();

            query.NameLike = '%' + name + '%';

            if (accessRight != null) {
                query.AccessRightIdIs = accessRight;
            }

            using (DbConnection db = this.GetDbConnection()) {
                var repository = new Repositories.IndicatorBriefRepository(
                    db, new Queries.ISqlQuery[] { query });
                foreach (Models.IndicatorBrief r in repository.FindAll()) {
                    yield return r;
                }
            }
        }

        public virtual Models.IndicatorDetails GetIndicatorDetails(int id)
        {
            var query = new Queries.IndicatorQuery();
            query.IdIs = id;

            using (DbConnection db = this.GetDbConnection()) {
                var repository = new Repositories.IndicatorDetailsRepository(
                    db,
                    new Queries.ISqlQuery[] { query });
                return (Models.IndicatorDetails) repository.Single();
            }
        }
    }
}