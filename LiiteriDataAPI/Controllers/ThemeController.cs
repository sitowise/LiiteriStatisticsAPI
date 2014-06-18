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

namespace LiiteriStatisticsAPI.Controllers
{
    public class ThemeController : ApiController
    {
        private DbConnection GetDbConnection(bool open = true)
        {
            string connStr = ConfigurationManager.ConnectionStrings[
                "LiiteriDB"].ToString();
            DbConnection db = new SqlConnection(connStr);
            if (open) db.Open();
            return db;
        }

        [Route("v1/themes/")]
        [HttpGet]
        public IEnumerable<Theme> GetThemes(
            int? id = null,
            int? parentId = null)
        {
            var query = new ThemeQuery();

            if (id != null) query.IdIs = (int) id;
            if (parentId != null) query.ParentIdIs = (int) parentId;

            using (DbConnection db = this.GetDbConnection()) {
                var repository = new ThemeRepository(db);
                return (List<Theme>) repository.FindAll(query);
            }
        }

        [Route("v1/themes/{id}/")]
        [HttpGet]
        public Theme GetTheme(int id)
        {
            var query = new ThemeQuery();
            query.IdIs = id;

            using (DbConnection db = this.GetDbConnection()) {
                var repository = new ThemeRepository(db);
                return (Theme) repository.Single(query);
            }
        }

        [Route("v1/themes/{id}/subthemes/")]
        [HttpGet]
        public IEnumerable<Theme> GetSubThemes(int id)
        {
            var query = new ThemeQuery();
            query.ParentIdIs = id;

            using (DbConnection db = this.GetDbConnection()) {
                var repository = new ThemeRepository(db);
                return (List<Theme>) repository.FindAll(query);
            }
        }

        [Route("v1/themes/{id}/")]
        [HttpDelete]
        public Theme DeleteTheme(int id)
        {
            using (DbConnection db = this.GetDbConnection()) {
                var repository = new ThemeRepository(db);
                Theme t = repository.Single(
                    new ThemeQuery() { IdIs = id });
                repository.Delete(t);
                return t;
            }
        }

        [Route("v1/themes/")]
        [HttpPost]
        public HttpResponseMessage AddTheme([FromBody] Theme theme)
        {
            using (DbConnection db = this.GetDbConnection()) {
                var repository = new ThemeRepository(db);
                repository.Insert(theme);

                var response = Request.CreateResponse<Theme>
                    (HttpStatusCode.Created, theme);
                response.Headers.Location = new Uri(Request.RequestUri,
                    "/themes/" + theme.Id.ToString());
                return response;
            }
        }

        [Route("v1/themes/{id}/")]
        [HttpPut]
        public HttpResponseMessage UpdateTheme(int id, [FromBody] Theme theme)
        {
            using (DbConnection db = this.GetDbConnection()) {
                var repository = new ThemeRepository(db);
                theme.Id = id;
                repository.Update(theme);

                var response = Request.CreateResponse<Theme>
                    (HttpStatusCode.OK, theme);
                response.Headers.Location = new Uri(Request.RequestUri,
                    "/themes/" + theme.Id.ToString());
                return response;
            }
        }
    }
}