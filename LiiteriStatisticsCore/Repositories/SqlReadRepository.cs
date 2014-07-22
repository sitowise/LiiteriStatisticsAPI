using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;

namespace LiiteriStatisticsCore.Repositories
{
    public abstract class SqlReadRepository<T> :
        IReadRepository<T> where T : class
    {
        protected DbConnection dbConnection;

        public SqlReadRepository(DbConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        public IEnumerable<T> FindAll(
            Queries.ISqlQuery query,
            Factories.IFactory factory)
        {
            var entityList = new List<T>();

            using (DbDataReader rdr = this.GetDbDataReader(query)) {
                while (rdr.Read()) {
                    T p = (T) factory.Create(rdr);
                    entityList.Add(p);
                }
            }

            return entityList;
        }

        public DbDataReader GetDbDataReader(
            Queries.ISqlQuery query)
        {
            using (DbCommand cmd = this.dbConnection.CreateCommand()) {
                cmd.CommandText = query.GetQueryString();

                foreach (KeyValuePair<string, object> param in
                        query.Parameters) {
                    cmd.Parameters.Add(
                        new SqlParameter(param.Key, param.Value));
                }

                return cmd.ExecuteReader();
            }
        }

        public abstract IEnumerable<T> FindAll(Queries.ISqlQuery query);

        public virtual IEnumerable<T> GetAll()
        {
            throw new NotImplementedException();
        }

        public abstract T Single(Queries.ISqlQuery query);

        public abstract T First(Queries.ISqlQuery query);
    }
}