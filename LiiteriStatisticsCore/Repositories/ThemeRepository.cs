using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.SqlClient;

namespace LiiteriStatisticsCore.Repositories
{
    public class ThemeRepository : SqlReadWriteRepository<Models.Theme>
    {
        private string tableName = "[LiiteriDataIndex]..[Themes]";

        public ThemeRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries) :
            base(dbConnection, queries, new Factories.ThemeFactory())
        {
        }

        public override Models.Theme Single()
        {
            return this.FindAll().Single();
        }

        public override Models.Theme First()
        {
            return this.FindAll().First();
        }

        public override void Insert(Models.Theme entity)
        {
            if (entity.Id > 0) {
                throw new InvalidOperationException(
                    "This object already has an Id!");
            }
            string queryString = @"
INSERT INTO {0}
    (name, parent_id)
VALUES
    (@Name, @ParentId)
SELECT
    SCOPE_IDENTITY()
";
            queryString = string.Format(queryString, this.tableName);

            using (DbCommand cmd = dbConnection.CreateCommand()) {
                cmd.Parameters.Add(new SqlParameter(
                    "@Name", entity.Name));
                cmd.Parameters.Add(new SqlParameter(
                    "@ParentId", (entity.ParentId != null ?
                        (int) entity.ParentId :
                        (object) DBNull.Value)));
                cmd.CommandText = queryString;
                var id = cmd.ExecuteScalar();
                entity.Id = (int) Convert.ToInt32(id);
            }
        }

        public override void Update(Models.Theme entity)
        {
            string queryString = @"
UPDATE
    {0}
SET
    name = @Name,
    parent_id = @ParentId
WHERE
    Id = @Id
";
            queryString = string.Format(queryString, this.tableName);

            using (DbCommand cmd = dbConnection.CreateCommand()) {
                cmd.Parameters.Add(new SqlParameter(
                    "@Id", entity.Id));
                cmd.Parameters.Add(new SqlParameter(
                    "@Name", entity.Name));
                cmd.Parameters.Add(new SqlParameter(
                    "@ParentId", (entity.ParentId != null ?
                        (int) entity.ParentId :
                        (object) DBNull.Value)));
                cmd.CommandText = queryString;
                cmd.ExecuteNonQuery();
            }
        }

        public override void Delete(Models.Theme entity)
        {
            string queryString = @"
DELETE FROM
    {0}
WHERE
    id = @Id
";
            queryString = string.Format(queryString, this.tableName);

            using (DbCommand cmd = dbConnection.CreateCommand()) {
                cmd.Parameters.Add(new SqlParameter("@Id", entity.Id));
                cmd.CommandText = queryString;
                cmd.ExecuteNonQuery();
            }
        }
    }
}