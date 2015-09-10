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
        IReadRepository<T>
    {
        private static readonly log4net.ILog logger =
            log4net.LogManager.GetLogger(
                System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        protected Models.SQLQueryTime sqlQueryTime = new Models.SQLQueryTime();

        protected DbConnection dbConnection;
        protected Factories.IFactory factory;
        public IEnumerable<Queries.ISqlQuery> queries;

        public SqlReadRepository(
            DbConnection dbConnection,
            IEnumerable<Queries.ISqlQuery> queries,
            Factories.IFactory factory = null)
        {
            this.dbConnection = dbConnection;
            this.factory = factory;
            this.queries = queries;
        }

        public delegate T ModifierDelegate(T obj);
        public List<ModifierDelegate> Modifiers = new List<ModifierDelegate>();

        public DbDataReader GetDbDataReader(
            Queries.ISqlQuery query)
        {
            using (DbCommand cmd = this.dbConnection.CreateCommand()) {
                cmd.CommandText = query.GetQueryString();
                cmd.CommandTimeout = 600;

                foreach (Infrastructure.Parameter param in query.Parameters) {
                    if (param.Value.GetType().Equals(typeof(int))) {
                        Debug.WriteLine(string.Format(
                            "Using special parameter type for {0}",
                            param.Name));
                        SqlParameter p = new SqlParameter(
                            "@" + param.Name, System.Data.SqlDbType.Int);
                        p.Value = param.Value;
                        cmd.Parameters.Add(p);
                    } else {
                        Debug.WriteLine(string.Format(
                            "Using default parameter type for {0}",
                            param.Name));
                        cmd.Parameters.Add(
                            new SqlParameter("@" + param.Name, param.Value));
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
                TimeSpan elapsed = (endTime - startTime);
                debugString = string.Format(
                    "Time elapsed on query: {0}.{1}s",
                    elapsed.Seconds, elapsed.Milliseconds);
                this.sqlQueryTime.TotalMilliseconds = elapsed.TotalMilliseconds;
                Debug.WriteLine(debugString);
                logger.Debug(debugString);

                return retval;
            }
        }
        private IEnumerable<T> FindAll(Queries.ISqlQuery query)
        {
            if (this.factory == null) {
                throw new ArgumentNullException(
                    "Using standard FindAll(), but factory is null!");
            }
            using (DbDataReader rdr = this.GetDbDataReader(query)) {
                while (rdr.Read()) {
                    T p = (T) this.factory.Create(rdr);

                    if (this.Modifiers != null) {
                        foreach (var d in this.Modifiers) {
                            p = d(p);
                        }
                    }

                    yield return p;
                }
            }
        }

        public virtual IEnumerable<T> FindAll()
        {
            foreach (Queries.ISqlQuery query in this.queries) {
                foreach (T r in this.FindAll(query)) {
                    yield return r;
                }
            }
        }

        /* TODO: should probably have a default implementation for these? */

        public abstract T Single();

        public abstract T First();
    }
}