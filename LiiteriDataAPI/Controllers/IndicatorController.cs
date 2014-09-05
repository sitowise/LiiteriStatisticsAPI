using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.Common;
using System.Data.SqlClient;
using System.Configuration;

using LiiteriStatisticsCore.Repositories;
using LiiteriStatisticsCore.Models;
using LiiteriStatisticsCore.Queries;

namespace LiiteriDataAPI.Controllers
{
    public class IndicatorController : ApiController
    {
        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        [Route("v1/indicators/")]
        [HttpGet]
        public IEnumerable<IndicatorBrief> GetIndicators(
            string name = null)
        {
            var query = new IndicatorQuery();

            query.NameLike = '%' + name + '%';

            using (DbConnection db = this.GetDbConnection()) {
                var repository = new IndicatorBriefRepository(db);
                foreach (IndicatorBrief r in repository.FindAll(query)) {
                    yield return r;
                }
            }
        }

        [Route("v1/indicators/{id}")]
        [HttpGet]
        public IndicatorDetails GetIndicatorDetails(int id)
        {
            var query = new IndicatorQuery();
            query.IdIs = id;

            using (DbConnection db = this.GetDbConnection()) {
                var repository = new IndicatorDetailsRepository(db);
                return (IndicatorDetails) repository.Single(query);
            }
        }
    }
}