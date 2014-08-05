using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;

using System.Diagnostics;

namespace LiiteriStatisticsCore.Repositories
{
    public abstract class SqlReadRepository<T> :
        IReadRepository<T> where T : class
    {
        public static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                    if (param.Value.GetType().Equals(typeof(int))) {
                        Debug.WriteLine(string.Format(
                            "Using special parameter type for {0}",
                            param.Key));
                        SqlParameter p = new SqlParameter(
                            param.Key, System.Data.SqlDbType.Int);
                        p.Value = param.Value;
                        cmd.Parameters.Add(p);
                    } else {
                        Debug.WriteLine(string.Format(
                            "Using default parameter type for {0}",
                            param.Key));
                        cmd.Parameters.Add(
                            new SqlParameter(param.Key, param.Value));
                    }
                }

                string debugString = "";
                foreach (DbParameter param in cmd.Parameters) {
                    debugString = string.Format(
                        "DECLARE {0} {1} = {2}",
                        param.ParameterName,
                        "INT",
                        param.Value);
                    Debug.WriteLine(debugString);
                    logger.Debug(debugString);
                }
                logger.Debug(cmd.CommandText.Replace("\n", " ").Replace("\r", ""));
                Debug.WriteLine(cmd.CommandText);

                DateTime startTime = DateTime.Now;
                DbDataReader retval = cmd.ExecuteReader();
                DateTime endTime = DateTime.Now;
                TimeSpan elapsedTime = (endTime - startTime);
                debugString = string.Format(
                    "Time elapsed on query: {0}.{1}s",
                    elapsedTime.Seconds, elapsedTime.Milliseconds);
                Debug.WriteLine(debugString);
                logger.Debug(debugString);

                return retval;
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